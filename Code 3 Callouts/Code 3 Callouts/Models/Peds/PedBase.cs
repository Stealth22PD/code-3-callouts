using Rage;
using Stealth.Common;
using Stealth.Common.Extensions;
using Stealth.Common.Models;
using Stealth.Plugins.Code3Callouts.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Stealth.Common.Models.QuestionWindow;
using static Stealth.Plugins.Code3Callouts.Common;

namespace Stealth.Plugins.Code3Callouts.Models.Peds
{

	internal class PedBase : Ped, IPedBase, IHandleable
	{

		public PedType Type { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Blip Blip { get; set; }
        //internal Property SearchArea As Blip Implements IPedBase.SearchArea
        public Vector3 OriginalSpawnPoint { get; set; }
        public List<string> SpeechLines { get; set; }
        public int SpeechIndex { get; set; }
        public string PhysicalCondition { get; set; }
        public List<QAItem> QAItems { get; set; }

		internal bool HasSpoken { get; set; }

		private bool mIsModalActive = false;
		internal PedBase(string pName, Rage.Vector3 position) : this(pName, PedType.Unknown, position) {
		}

		internal PedBase(string pName, Rage.Model model, Rage.Vector3 position, float heading) : this(pName, PedType.Unknown, model, position, heading) {
		}

		protected internal PedBase(string pName, Rage.PoolHandle handle) : this(pName, PedType.Unknown, handle) {
		}

		internal PedBase(string pName, PedType pType, Rage.Vector3 position) : base(position)
        {
			Name = pName;
			Type = pType;
			init();
		}

		internal PedBase(string pName, PedType pType, Rage.Model model, Rage.Vector3 position, float heading) : base(model, position, heading)
        {
			Name = pName;
			Type = pType;
			init();
		}

		protected internal PedBase(string pName, PedType pType, Rage.PoolHandle handle) : base(handle)
        {
			Name = pName;
			Type = pType;
			init();
		}

		protected internal void init()
		{
			OriginalSpawnPoint = this.Position;
			//SpeechIndex = -1
			SpeechLines = new List<string>();

			if (Common.IsTrafficPolicerRunning()) {
				Driver.MakePedImmuneToTrafficEvents(this);
			}
		}

		public override void Dismiss()
		{
			DeleteBlip();
			base.Dismiss();
		}

        public override void Delete()
		{
			DeleteBlip();
			base.Delete();
		}

        public virtual void Speak()
		{
			SpeakSubtitle();
		}

		private void SpeakModal()
		{
			if (this.IsDead) {
				SpeechLines.Clear();
				return;
			}

			if (SpeechLines.Count < 1) {
				return;
			}

			if (this.HasAttachedBlip() == false) {
				return;
			}

			if (mIsModalActive == false) {
				GameFiber.StartNew(() =>
				{
					ModalBase mModal = null;

					mIsModalActive = true;

					mModal = new SpeechModal(DisplayName, SpeechLines, true);
					mModal.Show();

					mModal = null;
					mIsModalActive = false;
					HasSpoken = true;
				});
			}
		}

		private void SpeakSubtitle()
		{
			if (SpeechLines.Count < 1) {
				return;
			}

			if (this.IsDead) {
				SpeechLines.Clear();
				return;
			}

			//If Me.HasAttachedBlip() = False Then
			//    Exit Sub
			//End If

			if (SpeechIndex == -1) {
				SpeechIndex = 0;
			}

			if (SpeechIndex < SpeechLines.Count) {
				string pedName;
				if (DisplayName == "") {
					pedName = Name;
				} else {
					pedName = DisplayName;
				}

				string colorCode = "~w~";
				if (Type == PedType.Victim | Type == PedType.Witness) {
					colorCode = "~o~";
				} else if (Type == PedType.Cop) {
					colorCode = "~b~";
				} else {
					colorCode = "~y~";
				}

				string speech = string.Format("{2}{0}: ~w~{1}", pedName, SpeechLines[SpeechIndex], colorCode);

				if (SpeechLines.Count > 1) {
					speech += string.Format(" ({0}/{1})", (SpeechIndex + 1), SpeechLines.Count);
				}

				Game.DisplaySubtitle(speech, 8000);

				SpeechIndex += 1;
			} else {
				HasSpoken = true;
				SpeechIndex = -1;
			}
		}

        public void CreateBlip(Color? pColor = null)
		{
			if (this.Exists()) {
				Color color;

				if (pColor == null) {
					switch (Type) {
						case PedType.Suspect:
							color = Color.Red;
                            break;
						case PedType.Unknown:
							color = Color.Yellow;
                            break;
						case PedType.Cop:
							color = Color.LightBlue;
                            break;
						default:
                            color = Color.Orange;
                            break;
                    }
                } else {
					color = pColor.Value;
				}

				this.Blip = new Blip(this);
				this.Blip.Color = color;
				this.Blip.Scale = 0.75f;
			}
		}

        public void DeleteBlip()
		{
			try {
				if (this.Blip != null) {
					if (this.Blip.Exists() == true) {
						this.Blip.Delete();
						//Me.Blip = Nothing
					}
				} else {
					Logger.LogVerboseDebug("Tried to delete Ped blip, but it was null");
				}
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error deleting Ped blip -- " + ex.Message);
			}
		}

        internal void SetDrunkRandom()
		{
			if (Common.IsTrafficPolicerRunning()) {
				if (Common.gRandom.Next(2) == 0) {
					SetIsDrunk(false);
				} else {
					SetIsDrunk(true);
				}
			}
		}

        public void SetIsDrunk(bool pValue)
		{
			try {
				if (Common.IsTrafficPolicerRunning()) {
					Logger.LogTrivialDebug("Traffic Policer running; setting ped as drunk");
					SetBACDrunk(pValue);
				} else {
					Logger.LogTrivialDebug("Traffic Policer not running; not setting ped as drunk");
				}

				if (pValue == true) {
                    //Rage.Native.NativeFunction.CallByName("SET_PED_IS_DRUNK", typeof(Object), Me, pValue)
                    //Rage.Native.NativeFunction.CallByName(Of Uinteger)("SET_PED_IS_DRUNK", Me, pValue)
                    Stealth.Common.Natives.Peds.SetPedIsDrunk(this, pValue);
				}
			} catch (Exception ex) {
				Logger.LogVerboseDebug(string.Format("Error setting Ped.Drunk({0}) -- {1}", pValue, ex.Message));
			}
		}

		private void SetBACDrunk(bool pValue)
		{
			if (pValue == true) {
				TrafficPolicerFunctions.SetPedAlcoholLevel(this, TrafficPolicerFunctions.GetRandomOverTheLimitAlcoholLevel());
			} else {
				TrafficPolicerFunctions.SetPedAlcoholLevel(this, TrafficPolicerFunctions.GetRandomUnderTheLimitAlcoholLevel());
			}
		}

        public void AttackPed(Ped pTargetPed)
		{
			try {
                Stealth.Common.Natives.Peds.AttackPed(this, pTargetPed);
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error triggering ped combat -- " + ex.Message);
			}
		}

        public void TurnToFaceEntity(Entity pTarget, int pTimeout = 5000)
		{
			try {
				ulong pHash = 0x5ad23d40115353acuL;
				Rage.Native.NativeFunction.CallByHash<uint>(pHash, Common.GetNativeArgument(this), Common.GetNativeArgument(pTarget), pTimeout);
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error calling native -- " + ex.Message);
			}
		}

	}

}