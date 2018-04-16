using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System.Windows.Forms;
using LSPD_First_Response.Engine.Scripting;
using System;
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System.Collections.Generic;
using System.Drawing;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Person with a Firearm", CalloutProbability.Low)]
    internal class PersonWithWeapon : CalloutBase
	{

		bool mSuspectReacted = false;
		Blip suspectSearchArea;
		LHandle pursuit;
		bool pursuitinitiated = false;

		private LSPD_First_Response.Gender SuspectGender;
		WeaponSuspect[] MaleModels = {
			new WeaponSuspect("A_M_Y_SouCent_01", "Black male wearing a blue shirt and a sweater vest"),
			new WeaponSuspect("A_M_Y_StWhi_01", "White male with long brown hair wearing a white t-shirt and denim shorts"),
			new WeaponSuspect("A_M_Y_StBla_01", "Black male wearing a striped shirt, khaki shorts; possibly red shoes"),
			new WeaponSuspect("A_M_Y_Downtown_01", "Black heavyset male, bald headed, wearing blue jeans"),
			new WeaponSuspect("A_M_M_Soucent_01", "Black male wearing a Liberty City jersey, blue jeans, and black shoes")

		};
		WeaponSuspect[] FemaleModels = {
			new WeaponSuspect("A_F_Y_GenHot_01", "White female with short dark hair, possibly wearing a yellow shirt"),
			new WeaponSuspect("A_F_Y_Yoga_01", "White female with short hair, wearing a tanktop and capri shorts")

		};
		Suspect pSuspect = null;
		WeaponSuspect mSuspectModel;
		bool suspectCounterOn = false;
		Vector3 suspectLastSeen;

		DateTime lastLocationUpdate = DateTime.Now;

        public PersonWithWeapon() : base("Person With a Firearm", CallResponseType.Code_3)
        {
			RadioCode = 417;
			CrimeEnums = new List<DISPATCH.CRIMES>() {
				DISPATCH.CRIMES.CODE_417,
				DISPATCH.CRIMES.PERSON_WITH_FIREARM
			};

			int genderFactor = Common.gRandom.Next(2);
			if (genderFactor == 0) {
				SuspectGender = Gender.Male;
				mSuspectModel = MaleModels[Common.gRandom.Next(MaleModels.Length)];
			} else {
				SuspectGender = Gender.Female;
				mSuspectModel = FemaleModels[Common.gRandom.Next(FemaleModels.Length)];
			}

			//CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
			CallDetails = "";
			CallDetails += "Caller reported a person with a firearm, and hung up.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "Suspect is described as a " + mSuspectModel.Description + ".";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "Suspect is reportedly still in the area on foot. No further details available at this time.";

			Objective = "Find and apprehend the ~r~suspect!";
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			bool baseReturn = base.OnBeforeCalloutDisplayed();

			if (baseReturn == false) {
				return false;
			}

			Vector3 pedSpawn = PedHelper.GetSafeCoordinatesForPed(World.GetNextPositionOnStreet(SpawnPoint.Around(60)));
			if (pedSpawn == Vector3.Zero) {
				pedSpawn = World.GetNextPositionOnStreet(SpawnPoint.Around(60));
			}

			pSuspect = new Suspect("Suspect1", mSuspectModel.Model, pedSpawn, gRandom.Next(360), false);
			pSuspect.DisplayName = "Suspect";

			int typeFactor = Common.gRandom.Next(10);

            LSPD_First_Response.Engine.Scripting.Entities.Persona p = Functions.GetPersonaForPed(pSuspect);

			if (typeFactor >= 3) {
				SuspectType = SuspectTypeEnum.Suspect;
				p = new LSPD_First_Response.Engine.Scripting.Entities.Persona(pSuspect, p.Gender, p.BirthDay, p.Citations, p.Forename, p.Surname, LSPD_First_Response.Engine.Scripting.Entities.ELicenseState.Suspended, p.TimesStopped, true, false,
				false);
				Functions.SetPersonaForPed(pSuspect, p);
				Logger.LogVerboseDebug("SuspectType = Suspect");

				{
					pSuspect.SpeechLines.Add("Hey Officer, how's it going?");
					pSuspect.SpeechLines.Add("Is there a problem?");
					pSuspect.SpeechLines.Add("Gun?");
					pSuspect.SpeechLines.Add("Someone called saying I have a gun?");
					pSuspect.SpeechLines.Add("No...I don't have a gun...");
					pSuspect.SpeechLines.Add("Who gave you a crazy idea like that?");
					pSuspect.SpeechLines.Add("I mean, that's the funniest thing I've ever heard!");
				}
			} else {
				SuspectType = SuspectTypeEnum.PoliceOfficer;
				pSuspect.RelationshipGroup = "OFFDUTY_COP";
				Game.SetRelationshipBetweenRelationshipGroups("OFFDUTY_COP", "PLAYER", Relationship.Respect);
				Game.SetRelationshipBetweenRelationshipGroups("PLAYER", "OFFDUTY_COP", Relationship.Respect);

				int mCopFactor = Common.gRandom.Next(3);
				bool mIsCop = false;
				bool mIsAgent = false;

				switch (mCopFactor) {
					case 0:
						CopType = CopTypeEnum.PoliceOfficer;
						mIsCop = true;
						break;
					case 1:
						CopType = CopTypeEnum.DeputySheriff;
						mIsCop = true;
						break;
					case 2:
						CopType = CopTypeEnum.FederalAgent;
						mIsAgent = true;
						break;
				}

				p = new LSPD_First_Response.Engine.Scripting.Entities.Persona(pSuspect, p.Gender, p.BirthDay, 0, p.Forename, p.Surname, LSPD_First_Response.Engine.Scripting.Entities.ELicenseState.Valid, 0, false, mIsAgent,
				mIsCop);
				LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(pSuspect, p);
				pSuspect.BlockPermanentEvents = false;
				Logger.LogVerboseDebug("SuspectType = PoliceOfficer");

				{
					pSuspect.SpeechLines.Add("Hey Officer, how's it going?");
					pSuspect.SpeechLines.Add("Is there a problem?");
					pSuspect.SpeechLines.Add("Gun?");
					pSuspect.SpeechLines.Add("Someone called saying I have a gun?");
				}

				switch (CopType) {
					case CopTypeEnum.FederalAgent:
						pSuspect.SpeechLines.Add("Well, maybe if I tell you my name, things will get clearer.");
						pSuspect.SpeechLines.Add(string.Format("Special Agent {0}, FIB.", p.FullName));
						pSuspect.SpeechLines.Add("Check my ID, if you want. Just make it quick, I have things to do.");

						break;
					case CopTypeEnum.DeputySheriff:
						pSuspect.SpeechLines.Add("Well, I do have one, but I don't think you're going to be worried!");
						pSuspect.SpeechLines.Add(string.Format("I'm Deputy {0}, with the Sheriff's Department.", p.FullName));
						pSuspect.SpeechLines.Add("I have my ID with me, if you want to see it.");

						break;
					case CopTypeEnum.PoliceOfficer:
						pSuspect.SpeechLines.Add("Seriously? I gotta put up with this shit?");
						pSuspect.SpeechLines.Add("Oh shit, I'm sorry! where are my manners?");

						string[] mDivisions = {
							"Mission Row",
							"La Mesa",
							"Davis",
							"Vinewood",
							"Rockford Hills",
							"Del Perro",
							"Vespucci"
						};

						pSuspect.SpeechLines.Add(string.Format("I'm a cop. My name is {0}. ~n~LSPD {1} Division.", p.FullName, mDivisions[Common.gRandom.Next(mDivisions.Length)]));
						pSuspect.SpeechLines.Add("I have my ID with me, if you want to see it.");
						break;
				}
			}

			string pAnimSet = "";
			if (SuspectGender == Gender.Female) {
				pAnimSet = "move_f@arrogant";
			} else {
				pAnimSet = "move_m@confident";
			}

			try {
				AnimationSet animSet = new AnimationSet(pAnimSet);
				animSet.LoadAndWait();
				pSuspect.MovementAnimationSet = animSet;
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error animating ped -- " + ex.Message);
			}

			pSuspect.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 56, false);

			pSuspect.Tasks.Wander();

			if (pSuspect != null) {
				if (pSuspect.Exists()) {
					Peds.Add(pSuspect);
				} else {
					return false;
				}
			}

			if (PerformPedChecks()) {
				return baseReturn;
			} else {
				return false;
			}
		}

		public override bool OnCalloutAccepted()
		{
			SuspectSearch = SuspectSearchStateEnum.Null;
			return base.OnCalloutAccepted();
		}

		public override void OnArrivalAtScene()
		{
			base.OnArrivalAtScene();

			Radio.DispatchMessage("Suspect description has been transmitted to your computer.", true);

			string mDescription = "Suspect is described as a " + mSuspectModel.Description + ".";
			Game.DisplayNotification(mDescription);

			Game.DisplayHelp("Search the area for the suspect. Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout.");

			if (pSuspect.Exists()) {
				lastLocationUpdate = DateTime.Now;
				CreateSearchArea(pSuspect.OriginalSpawnPoint);
				SuspectSearch = SuspectSearchStateEnum.NotYetLocated;
			} else {
				Game.DisplayNotification("Person With a Gun callout crashed");
				Radio.CallIsCode4(this.ScriptInfo.Name);
				End();
			}
		}

		private void CreateSearchArea(Vector3 pSpawnPoint)
		{
			suspectSearchArea = new Blip(pSpawnPoint, 150);
			suspectSearchArea.Color = Color.FromArgb(70, Color.Red);
		}

		private void DeleteSearchArea()
		{
			try {
				if (suspectSearchArea != null) {
					suspectSearchArea.Delete();
				}

			} catch {
			}
		}

		public override void Process()
		{
			base.Process();

			if (Game.LocalPlayer.Character.IsDead) {
				return;
			}

			if (Common.IsKeyDown(Config.SpeakKey)) {
				if (pSuspect.Exists() && pSuspect.IsAlive) {
					if (Game.LocalPlayer.Character.Position.DistanceTo(pSuspect.Position) < 3) {
						pSuspect.Speak();

						if (pSuspect.SpeechIndex == pSuspect.SpeechLines.Count) {
							if (SuspectType == SuspectTypeEnum.Suspect && mSuspectReacted == false) {
								int reaxFactor = Common.gRandom.Next(10);

								if (reaxFactor <= 3) {
									mSuspectReacted = true;
									AttackPlayer();
								} else {
									mSuspectReacted = true;
									pursuitinitiated = true;
									pursuit = Common.CreatePursuit();
									pSuspect.AddToPursuit(pursuit);
								}
							}
						}
					}
				}
			}

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
                Radio.CallIsCode4(this.ScriptInfo.Name);
                End();

    //            if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
					
				//}
			}


			if (pSuspect != null && pSuspect.Exists()) {
				if (pSuspect.IsDead | pSuspect.IsArrested()) {
					if (pSuspect.Inventory.Weapons.Count > 0) {
						pSuspect.Inventory.Weapons.Clear();
						Game.DisplayNotification("While searching the suspect, you find/remove a ~r~pistol~w~.");
					}

					pSuspect.DeleteBlip();
					Radio.CallIsCode4(this.ScriptInfo.Name, pSuspect.IsArrested());
					End();
				} else {
					if (SuspectSearch == SuspectSearchStateEnum.NotYetLocated) {

						if (Game.LocalPlayer.Character.Position.DistanceTo(pSuspect.Position) <= 30) {
							if (suspectCounterOn == false) {
								suspectCounterOn = true;
								DateTime startTime = DateTime.Now;

								GameFiber.StartNew(() =>
								{
									while (Game.LocalPlayer.Character.Position.DistanceTo(pSuspect.Position) <= 30) {
										GameFiber.Yield();

										TimeSpan ts = DateTime.Now - startTime;

										if (ts.TotalSeconds >= 3) {
											SuspectSearch = SuspectSearchStateEnum.Located;
											DeleteSearchArea();
											pSuspect.CreateBlip();
											Radio.SuspectSpotted();
											SuspectFound(pSuspect);

											break; // TODO: might not be correct. Was : Exit While
										}
									}

									suspectCounterOn = false;
								});
							}

						}

						if (suspectSearchArea != null && suspectSearchArea.Exists() && suspectCounterOn == false) {
							TimeSpan ts = DateTime.Now - lastLocationUpdate;

							if (ts.TotalSeconds > 60) {
								GameFiber.StartNew(() =>
								{
									suspectLastSeen = pSuspect.Position;
									suspectSearchArea.Position = pSuspect.Position;
									lastLocationUpdate = DateTime.Now;

									string pAudio = "SUSPECT_LAST_SEEN IN_OR_ON_POSITION";

									string mHeading = Common.GetDirectionAudiofromHeading(pSuspect.Heading);
									if (mHeading != "") {
										pAudio = string.Format("SUSPECT_LAST_SEEN DIR_HEADING {0} IN_OR_ON_POSITION", mHeading);
									}

									AudioPlayerEngine.PlayAudio(pAudio, pSuspect.Position);

									Game.DisplayHelp("The search area has been updated.");
								});
							}
						}

					} else if (SuspectSearch == SuspectSearchStateEnum.Located) {
						DeleteSearchArea();

						if (pursuitinitiated == true) {
							if (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(pursuit) == false) {
								pSuspect.DeleteBlip();
								End();
							}
						}
					}

				}
			} else {
				Game.DisplayNotification("Person With Weapon Callout crashed");
				End();
			}
		}

		private void SuspectFound(Suspect pSuspect)
		{
			pSuspect.Tasks.Clear();
			pSuspect.TurnToFaceEntity(Game.LocalPlayer.Character);

			Game.DisplayHelp("Speak to the subject by pressing " + Config.SpeakKey.ToString() + ". Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout.");

			if (Common.IsComputerPlusRunning()) {
				AddPedToCallout(pSuspect);
			}

			if (SuspectType == SuspectTypeEnum.Suspect) {
				int reaxFactor = Common.gRandom.Next(10);
				reaxFactor = 9;

				if (reaxFactor <= 1) {
					mSuspectReacted = true;
					AttackPlayer();
				} else if (reaxFactor >= 2 & reaxFactor <= 4) {
					mSuspectReacted = true;
					pursuitinitiated = true;
					pursuit = Common.CreatePursuit();
					pSuspect.AddToPursuit(pursuit);
				} else {
					//Nothing
				}
			}
		}

		private void AttackPlayer()
		{
			//Dim attackDelay As integer = Common.gRandom.Next(30000, 60000)
			int attackDelay = 2500;

			GameFiber.StartNew(() =>
			{
				GameFiber.Sleep(attackDelay);

				try {
					pSuspect.RelationshipGroup = "HATES_PLAYER";
                    Stealth.Common.Natives.Peds.AttackPed(pSuspect, Game.LocalPlayer.Character);
				} catch (Exception ex) {
					Logger.LogVerboseDebug("Error attacking player -- " + ex.Message);
				}
			});
		}

		public override void End()
		{
			base.End();
		}

		private SuspectSearchStateEnum SuspectSearch = SuspectSearchStateEnum.Null;
		enum SuspectSearchStateEnum
		{
			Null = 0,
			NotYetLocated = 1,
			Located = 2
		}

		private SuspectTypeEnum SuspectType;
		enum SuspectTypeEnum
		{
			Suspect,
			PoliceOfficer
		}

		private CopTypeEnum CopType;
		enum CopTypeEnum
		{
			PoliceOfficer,
			DeputySheriff,
			FederalAgent
		}

		public override bool RequiresSafePedPoint {
			get { return true; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return true; }
		}

		private struct WeaponSuspect
		{
			internal WeaponSuspect(string pModel, string pDesc)
			{
				Model = pModel;
				Description = pDesc;
			}

			internal string Model { get; set; }
			internal string Description { get; set; }
		}

	}

}