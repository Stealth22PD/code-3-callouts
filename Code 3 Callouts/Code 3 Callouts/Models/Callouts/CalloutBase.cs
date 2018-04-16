using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using System.Windows.Forms;
using Stealth.Common.Models;
using static Stealth.Common.Models.QuestionWindow;
using static Stealth.Plugins.Code3Callouts.Common;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System;
using System.Linq;
using System.Drawing;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts
{

	internal abstract class CalloutBase : LSPD_First_Response.Mod.Callouts.Callout, ICalloutBase, IPoliceIncident
	{

		private bool mIsQAModalActive = false;
		private bool mIsObserveWindowActive = false;

		private bool mAssignedToAI = false;
        public CalloutBase(string pCalloutMessage, CallResponseType pResponseType = CallResponseType.Code_2)
		{
			CalloutState = CalloutState.Created;
			CalloutMessage = pCalloutMessage;
			ResponseType = pResponseType;
			Peds = new List<PedBase>();
			Markers = new List<Blip>();
			Vehicles = new List<Vehicles.Vehicle>();
			FollowMePed = null;
			PedsToIgnore = new List<PoolHandle>();
			FoundPedSafeSpawn = false;
			CallDispatchTime = DateTime.Now.AddMinutes(-2);
			CalloutID = Guid.Empty;
		}

		private void RegisterMenuEvents()
		{
			Driver.mInteractionMenu.OnItemSelect += minteractionMenu_OnItemselect;
		}

		private void UnregisterMenuEvents()
		{
            Driver.mInteractionMenu.OnItemSelect -= minteractionMenu_OnItemselect;
		}

		private void minteractionMenu_OnItemselect(UIMenu sender, UIMenuItem selectedItem, int index)
		{
			if (sender.Equals(Driver.mInteractionMenu) == true) {
                if (object.ReferenceEquals(selectedItem, Driver.mObserveSubject))
                {
                    if (Common.ClosestPed != null)
                    {
                        string mPhysicalCondition = Common.ClosestPed.PhysicalCondition;
                        Logger.LogTrivialDebug("selectedItem Is mObserveSubject");
                        //Observe
                        Logger.LogTrivialDebug(".Exists() = " + Common.ClosestPed.Exists());
                        Logger.LogTrivialDebug(".PhysicalCondition = " + Common.ClosestPed.PhysicalCondition);
                        Logger.LogTrivialDebug("mIsObserveWindowActive = " + mIsObserveWindowActive);
                        if (Common.ClosestPed.Exists() && !string.IsNullOrEmpty(Common.ClosestPed.PhysicalCondition) && mIsObserveWindowActive == false)
                        {
                            GameFiber.StartNew(() =>
                            {
                                Logger.LogTrivialDebug("GameFiber started");
                                mIsObserveWindowActive = true;

                                ModalWindow mModal = new ModalWindow("Observation Window", mPhysicalCondition, false);
                                Logger.LogTrivialDebug("ModalWindow created");
                                mModal.Show();
                                Logger.LogTrivialDebug("ModalWindow shown");
                                mModal = null;
                                Logger.LogTrivialDebug("ModalWindow = null");

                                mIsObserveWindowActive = false;
                                Logger.LogTrivialDebug("GameFiber end");
                            });
                            Logger.LogTrivialDebug("If end");
                        }
                    }
                    else
                    {
                        Logger.LogTrivialDebug("Common.ClosestPed Is null");
                    }
                }
                else if (object.ReferenceEquals(selectedItem, Driver.mSpeakToSubject))
                {
                    //Speak
                    if (Common.ClosestPed.Exists() && Common.ClosestPed.IsAlive)
                    {
                        Common.ClosestPed.Speak();
                    }
                }
                else if (object.ReferenceEquals(selectedItem, Driver.mQuestionSubject))
                {
                    //Question
                    if (Common.ClosestPed.Exists() && Common.ClosestPed.IsAlive && Common.ClosestPed.QAItems != null && mIsQAModalActive == false)
                    {
                        OpenQuestionWindow();
                    }
                }
                else if (object.ReferenceEquals(selectedItem, Driver.mAskForID))
                {
                    //Ask for ID
                    if (Common.ClosestPed.Exists())
                    {
                        LSPD_First_Response.Engine.Scripting.Entities.Persona pData = Functions.GetPersonaForPed(Common.ClosestPed);
                        string IDTextFormat = "~b~{0}~n~~y~{1}, ~w~Born: ~y~{2}";
                        string IDText = string.Format(IDTextFormat, pData.FullName, pData.Gender.ToString(), pData.BirthDay.ToString("M/d/yyyy"));

                        Game.DisplayNotification("mpcharselect", "mp_generic_avatar", "STATE ISSUED IDENTIFICATION", pData.FullName.ToUpper(), IDText);
                    }
                }
                else if (object.ReferenceEquals(selectedItem, Driver.mAskToFollow))
                {
                    //Ask to Follow
                    AskPedToFollowOfficer();
                }
            }
		}

		internal bool IsCADModalActive { get; set; }

        public virtual bool IsFixedSpawnPoint {
			get { return false; }
		}

        public Vector3 GetRandomSpawnPoint(float pMin, float pMax)
		{
			return World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(Common.gRandom.Next((int)pMin, (int)pMax)));
		}

        public virtual Vector3 CalculateSpawnpoint()
		{
			return Vector3.Zero;
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			//Base spawn point
			if (IsFixedSpawnPoint == false) {
				SpawnPoint = GetRandomSpawnPoint(150f, 401f);
				int iSpawnTries = 0;
				float pMax = 401f;

				if (RequiresSafePedPoint == true) {
					while (iSpawnTries <= 5) {
						SpawnPoint = PedHelper.GetSafeCoordinatesForPed(SpawnPoint);

						if (SpawnPoint == Vector3.Zero) {
							pMax += 250f;
						} else {
							FoundPedSafeSpawn = true;
							break; // TODO: might not be correct. Was : Exit While
						}

						iSpawnTries += 1;
					}

					if (SpawnPoint == Vector3.Zero) {
						//Fail
						Logger.LogVerboseDebug("Failed to find safe spawn point for callout");
						SpawnPoint = GetRandomSpawnPoint(150f, 501f);
						//Return False
					}
				}
			} else {
				SpawnPoint = CalculateSpawnpoint();
			}

			if (ShowAreaBlipBeforeAccepting) {
				ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
			}

			CalloutPosition = SpawnPoint;

			if (Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 100) {
				Logger.LogVerboseDebug("Player is too close; callout aborted");
				return false;
			}

			return base.OnBeforeCalloutDisplayed();
		}

        public override void OnCalloutDisplayed()
		{
			Radio.DispatchCallout(this.ScriptInfo.Name, SpawnPoint, CrimeEnums, ResponseType);
			CalloutState = CalloutState.Dispatched;
			base.OnCalloutDisplayed();

			bool mComputerPlusRunning = Common.IsComputerPlusRunning();
			if (mComputerPlusRunning == true) {
				CreateCADCallout();
			}
		}

		private void CreateCADCallout()
		{
			Logger.LogTrivialDebug("ComputerPlus is running");

			ComputerPlus.EResponseType mResponse = ComputerPlus.EResponseType.Code_2;
			if (this.ResponseType == CallResponseType.Code_3)
				mResponse = ComputerPlus.EResponseType.Code_3;

			CalloutID = ComputerPlusFunctions.CreateCallout(CalloutMessage, RadioCode.ToString(), SpawnPoint, mResponse, CallDetails, ComputerPlus.ECallStatus.Created);
		}

        public override bool OnCalloutAccepted()
		{
			Radio.AcknowledgeCallout(this.ScriptInfo.Name, ResponseType);
			IsCalloutActive = true;

			if (SkipRespondingState == false) {
				CalloutState = CalloutState.UnitResponding;
				CreateBlip();
			}

			RegisterMenuEvents();
			DisplayObjective();

			if (CallDetails == "") {
				CallDetails = "No Further information Available";
			}

			if (Common.IsComputerPlusRunning()) {
				ComputerCallAccepted();
			} else {
				Game.DisplayHelp("Download ~b~LSPDFR Computer+ ~w~to view complete details on this call!");
			}

			return base.OnCalloutAccepted();
		}

		private void ComputerCallAccepted()
		{
			Logger.LogTrivialDebug("Computer+ running!");
			ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.Dispatched);
			ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.Unit_Responding);

			Game.DisplayHelp("You can view details about the call with ~b~LSPDFR Computer+~w~.");
		}

        public override void OnCalloutNotAccepted()
		{
			Logger.LogVerboseDebug("Callout not accepted");
			UnregisterMenuEvents();
			IsCalloutActive = false;

			CalloutState = CalloutState.Cancelled;

			foreach (var p in Peds) {
				if (p != null) {
					if (p.Exists()) {
						p.Delete();
					}
				}
			}

			foreach (var v in Vehicles) {
				if (v != null) {
					if (v.Exists()) {
						v.Delete();
					}
				}
			}

			if (Common.IsComputerPlusRunning()) {
				CADAssignToAI();
				mAssignedToAI = true;
			}

			base.OnCalloutNotAccepted();

			Radio.AIOfficerResponding();
		}

		private void CADAssignToAI()
		{
			ComputerPlusFunctions.AssignCallToAIUnit(CalloutID);
		}

        public void DisplayObjective()
		{
			if (string.IsNullOrWhiteSpace(Objective) == false) {
				string mTitle = "CODE 3 CALLOUTS";
				string mSubtitle = "~b~" + CalloutMessage;

				Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", mTitle, mSubtitle, Objective);
			}
		}

        public override void Process()
		{
			base.Process();
			Common.ClosestPed = (from x in Peds where x.Exists() && x.DistanceTo(Game.LocalPlayer.Character.Position) < 3f select x).FirstOrDefault();

			if (Game.LocalPlayer.Character.IsDead) {
				OfficerDown();
				End();
			}

			if (CalloutState == CalloutState.UnitResponding) {
				if (Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 30f) {
					Radio.UnitIsOnScene();
					CalloutState = CalloutState.AtScene;
					OnArrivalAtScene();
				}
			} else if (CalloutState == CalloutState.AtScene) {
				if (Common.IsKeyDown(Config.AskToFollowKey, Config.AskToFollowModKey)) {
					AskPedToFollowOfficer();
					//if (Config.AskToFollowModKey == Keys.None || Game.IsKeyDownRightNow(Config.AskToFollowModKey)) {
					//}
				}
			}
		}

        public void AskPedToFollowOfficer()
		{
			if (Game.LocalPlayer.Character.IsInAnyVehicle(true) == false) {
				//If player is on foot...
				if (FollowMePed == null) {
					FollowMePed = Game.LocalPlayer.Character.GetNearbyPeds(1).FirstOrDefault();

					if (FollowMePed != null && FollowMePed.Exists()) {
						if (FollowMePed.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 3) {
							bool isValid = true;
							bool isArrested = FollowMePed.IsArrested() || FollowMePed.IsGettingArrested();

                            if (isArrested || FollowMePed.IsDead || FollowMePed.IsInAnyVehicle(true) || PedsToIgnore.Contains(FollowMePed.Handle) || FollowMePed.IsFleeing || FollowMePed.IsInCombat || FollowMePed.IsShooting)
                            {
                                isValid = false;
                                return;
                            }

                            if (isValid == true) {
								FollowMePed.Tasks.ClearImmediately();
								FollowMePed.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, Vector3.RelativeBack * 3f);
								Game.DisplayHelp("The ped is now following you.", 3000);
							} else {
								FollowMePed = null;
							}
						} else {
							FollowMePed = null;
						}
					} else {
						FollowMePed = null;
					}
				} else {
					if (FollowMePed != null && FollowMePed.Exists()) {
						FollowMePed.Tasks.Clear();
						FollowMePed = null;
						Game.DisplayHelp("The ped is no longer following you.", 3000);
					} else {
						FollowMePed = null;
					}
				}
			}
		}

        public void OfficerDown()
		{
			Radio.OfficerDown();
			DeleteEntities();
			End();
		}

        public virtual void OnArrivalAtScene()
		{
			DeleteBlip();

			if (Common.IsComputerPlusRunning()) {
				CADAtScene();
			}
		}

		private void CADAtScene()
		{
			ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.At_Scene);
		}

		protected void AddPedToCallout(PedBase p)
		{
			if (Common.IsComputerPlusRunning()) {
				if (p.Type != PedType.Cop) {
					ComputerPlusFunctions.AddPedToCallout(CalloutID, p);
				}
			}
		}

		protected void AddVehicleToCallout(Vehicles.Vehicle v)
		{
			if (Common.IsComputerPlusRunning()) {
				ComputerPlusFunctions.AddVehicleToCallout(CalloutID, v);
			}
		}

        public void DeleteEntities()
		{
			foreach (PedBase p in Peds) {
				if (p != null) {
					if (p.Exists() == true) {
						p.DeleteBlip();
						p.Delete();
					}
				}
			}

			Peds.Clear();

			foreach (Blip m in Markers) {
				if (m != null) {
					if (m.Exists() == true) {
						m.Delete();
					}
				}
			}

			Markers.Clear();
		}

		protected void DeleteBlips()
		{
			foreach (PedBase p in Peds) {
				if (p != null) {
					if (p.Exists() == true) {
						p.DeleteBlip();
					}
				}
			}

			foreach (Blip m in Markers) {
				if (m != null) {
					if (m.Exists() == true) {
						m.Delete();
					}
				}
			}

			Markers.Clear();
		}

        public override void End()
		{
			Logger.LogVerboseDebug("CalloutBase.End()");
			base.End();
			UnregisterMenuEvents();
			Common.ClosestPed = null;
			IsCalloutActive = false;

			if (Driver.mInteractionMenu.Visible) {
                Driver.mInteractionMenu.Visible = false;
			}

			DeleteBlip();

			PedsToIgnore.Clear();

			foreach (PedBase p in Peds) {
				if (p != null) {
					if (p.Exists() == true) {
						//p.DeleteSearchArea()
						p.DeleteBlip();

						if (p.IsInAnyPoliceVehicle == false && p.IsArrested() == false) {
							p.Dismiss();
						}
					}
				}
			}

			Peds.Clear();

			foreach (Blip m in Markers) {
				if (m != null) {
					if (m.Exists() == true) {
						m.Delete();
					}
				}
			}

			Markers.Clear();

			foreach (var v in Vehicles) {
				if (v != null) {
					if (v.Exists()) {
						v.Dismiss();
					}
				}
			}

			Vehicles.Clear();

			CalloutState = CalloutState.Completed;

			if (Common.IsComputerPlusRunning()) {
				if (mAssignedToAI == false) {
					if (CalloutState == CalloutState.Created | CalloutState == CalloutState.Dispatched | CalloutState == CalloutState.UnitResponding) {
						CADCancel();
					} else {
						CADConclude();
					}
				}
			}
		}

		private void CADConclude()
		{
			ComputerPlusFunctions.ConcludeCallout(CalloutID);
		}

		private void CADCancel()
		{
			ComputerPlusFunctions.CancelCallout(CalloutID);
		}

        public void CreateBlip()
		{
			CallBlip = new Blip(CalloutPosition);
			CallBlip.Color = Color.Yellow;
			CallBlip.EnableRoute(Color.Yellow);
		}

        public void DeleteBlip()
		{
			if (CallBlip != null) {
				if (CallBlip.Exists()) {
					CallBlip.DisableRoute();
					CallBlip.Delete();
				}
			}
		}

		internal virtual void OpenQuestionWindow()
		{
			GameFiber.StartNew(() =>
			{
				Logger.LogVerboseDebug("QAItems.Count = " + Common.ClosestPed.QAItems.Count);

				List<QAItem> mQAItems = new List<QAItem>();
				mQAItems.AddRange(Common.ClosestPed.QAItems);
                Logger.LogVerboseDebug("mQAItems.Count = " + mQAItems.Count);

				foreach (var x in mQAItems) {
                    Logger.LogVerboseDebug("Q = " + x.Question);
                    Logger.LogVerboseDebug("A = " + x.Answer);
				}

				mIsQAModalActive = true;

				QuestionWindow mModal = new QuestionWindow("Question Subject", mQAItems, true);
				mModal.Show();

				mModal = null;
				mIsQAModalActive = false;
			});
		}

        public virtual bool PerformPedChecks()
		{
			bool isValid = true;

			foreach (PedBase p in Peds) {
				if (p != null) {
					if (p.Exists()) {
						AddMinimumDistanceCheck(5f, p.Position);
					} else {
						isValid = false;
						break; // TODO: might not be correct. Was : Exit For
					}
				} else {
					isValid = false;
					break; // TODO: might not be correct. Was : Exit For
				}
			}

			if (isValid == true) {
				return true;
			} else {
				foreach (PedBase p in Peds) {
					if (p != null) {
						if (p.Exists()) {
							p.Delete();
						}
					}
				}

				Peds.Clear();
				return false;
			}
		}

        public PedBase GetPed(string pName)
		{
			return (from x in Peds where x.Name == pName select x).FirstOrDefault();
		}

        public T GetPed<T>(string pName) where T:PedBase
        {
            PedBase p = (from x in Peds where x.GetType() is T && x.Name == pName select x).FirstOrDefault();

            if (p != null && p is T)
            {
                return (T)p;
            }
            else
            {
                return null;
            }
        }

        public Vehicles.Vehicle GetVehicle(string pName)
		{
			return (from x in Vehicles where x.Name == pName select x).FirstOrDefault();
		}

        public int RadioCode { get; set; }
        public List<DISPATCH.CRIMES> CrimeEnums { get; set; }
        public CallResponseType ResponseType { get; set; }
        public string Objective { get; set; }
        public Vector3 SpawnPoint { get; set; }
        public abstract bool RequiresSafePedPoint { get; }
        public DateTime CallDispatchTime { get; set; }
        public CalloutState CalloutState { get; set; }
        public Blip CallBlip { get; set; }
        public string CallDetails { get; set; }
        public List<PedBase> Peds { get; set; }
        public List<Vehicles.Vehicle> Vehicles { get; set; }
        public List<Blip> Markers { get; set; }
        public abstract bool ShowAreaBlipBeforeAccepting { get; }
        public virtual bool SkipRespondingState { get; set; }

        public List<Rage.PoolHandle> PedsToIgnore { get; set; }
        public bool FoundPedSafeSpawn { get; set; }
        public Guid CalloutID { get; set; }

	}

}