using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Models.Vehicles;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using System.Windows.Forms;
using Stealth.Common.Natives;
using LSPD_First_Response.Engine.Scripting.Entities;
using System;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Backup Required (Domestic)", CalloutProbability.High)]
    internal class BackupDomestic : CalloutBase
	{

		string[] SuspectModels = {
			"A_M_Y_SouCent_01",
			"A_M_Y_StWhi_01",
			"A_M_Y_StBla_01",
			"A_M_Y_Downtown_01",
			"A_M_Y_BevHills_01",
			"G_M_Y_MexGang_01",
			"G_M_Y_MexGoon_01",
			"G_M_Y_StrPunk_01"
		};
		string[] VictimModels = {
			"A_F_Y_GenHot_01",
			"A_F_Y_Hippie_01",
			"A_F_Y_Hipster_01",
			"A_F_Y_BevHills_01",
			"A_F_Y_BevHills_02",
			"A_F_M_Tourist_01",
			"A_F_M_FatWhite_01",
			"A_F_M_Business_02"

		};
		bool anyoneArrested = false;

		bool arrestedTipDisplayed = false;
		ScenarioState mState = ScenarioState.PlayerResponding;
		bool pursuitinitiated = false;
		LHandle pursuit = null;
		DateTime playerAskedToRunNames = DateTime.Now;
		DateTime warrantChecksDoneQuestionDisplayed = DateTime.Now;

		ArrestablePeds mWantedPeds = ArrestablePeds.None;
		Suspect pSuspect = null;

		Persona pDataSuspect = null;
		Suspect pVictim = null;

		Persona pDataVictim = null;
		Peds.Cop pCop1 = null;

		Peds.Cop pCop2 = null;

		Models.Vehicles.Vehicle vPolice1 = null;
		private enum ScenarioState
		{
			PlayerResponding,
			PlayerExitedVehicle,
			OfficerGreetingPlayer,
			PlayerBriefed,
			PlayerJoinedSituation,
			SituationProceeding,
			PlayerGettingIDs,
			PlayerRunningNames,
			WarrantChecksComplete,
			ArrestProceeding,
			ScenarioOver,
			CallIsCode4,
			CalloutEnded
		}

		private enum ArrestablePeds
		{
			None,
			Suspect,
			Victim,
			Both
		}

        public BackupDomestic() : base("Backup Required (Domestic)", CallResponseType.Code_2)
        {
			RadioCode = 240;
			CrimeEnums = new List<DISPATCH.CRIMES> { DISPATCH.CRIMES.OFFICER_IN_NEED_OF_ASSISTANCE };

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "Female RP called to report that her husband had gotten into a verbal altercation with her, during which he struck her physically. RP alleges that the suspect can sometimes become violent if provoked.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "RP seemed hesitant to answer questions; proceed with caution.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;

			int mUnitNumber = Common.gRandom.Next(27, 50);

			//CallDetails += String.Format("[{0}] UPD: 1-ADAM-{1} responding Code 2", CallDispatchTime.AddSeconds(15).ToString("M/d/yyyy HH:mm:ss"), mUnitNumber)
			CallDetails += string.Format("UPDATE: 1-ADAM-{0} responding Code 2", mUnitNumber);
			CallDetails += Environment.NewLine;
			//CallDetails += String.Format("[{0}] UPD: 1-ADAM-{1} on scene; requesting addt'l unit", CallDispatchTime.AddSeconds(80).ToString("M/d/yyyy HH:mm:ss"), mUnitNumber)
			CallDetails += string.Format("UPDATE: 1-ADAM-{0} requesting addt'l unit", mUnitNumber);

			Objective = "Backup your fellow ~b~officers!~n~~w~Stay alert, Officer!";
		}

		public override bool OnCalloutAccepted()
		{
			bool baseReturn = base.OnCalloutAccepted();

			if (baseReturn == false) {
				return false;
			}

			int roadHeading = 0;
			Vector3 roadPos = SpawnPoint;
			Stealth.Common.Models.VehicleNode vehNode = Stealth.Common.Natives.Vehicles.GetClosestVehicleNodeWithHeading(SpawnPoint);

			if (vehNode.Position != Vector3.Zero) {
				roadHeading = (int)vehNode.Heading;
				roadPos = vehNode.Position;
				roadHeading -= 20;

				if (roadHeading > 360) {
					roadHeading = roadHeading - 360;
				}
			}

			if (Common.IsPlayerinLosSantos()) {
				string[] lspdModels = {
					"POLICE",
					"POLICE2",
					"POLICE3",
					"POLICE4"
				};
				vPolice1 = new Vehicles.Vehicle(lspdModels[Common.gRandom.Next(lspdModels.Length)], roadPos, roadHeading);
			} else {
				string[] sheriffModels = {
					"SHERIFF",
					"SHERIFF",
					"SHERIFF2",
					"POLICE4"
				};
				vPolice1 = new Vehicles.Vehicle(sheriffModels[Common.gRandom.Next(sheriffModels.Length)], roadPos, roadHeading);
			}
			vPolice1.IsSirenOn = true;
			vPolice1.IsSirenSilent = true;
			vPolice1.MakePersistent();

			vPolice1.Position = vPolice1.GetOffsetPositionRight(5);
			Vehicles.Add(vPolice1);

			Vector3 pedSpawn = SpawnPoint;

			if (FoundPedSafeSpawn == false) {
				pedSpawn = vPolice1.GetOffsetPositionFront(5);
			}

			pSuspect = new Suspect("Suspect1", SuspectModels[Common.gRandom.Next(SuspectModels.Length)], pedSpawn, 0, false);
			pSuspect.DisplayName = "Suspect";
			pSuspect.BlockPermanentEvents = true;
			pSuspect.MakePersistent();
			//pSuspect.TurnToFaceEntity(vPolice1)
			Peds.Add(pSuspect);

			pVictim = new Suspect("Victim1", VictimModels[Common.gRandom.Next(VictimModels.Length)], pSuspect.GetOffsetPosition(Vector3.RelativeLeft * 5), 0, false);
			pVictim.DisplayName = "Victim";
			pVictim.BlockPermanentEvents = true;
			pVictim.MakePersistent();
			//pVictim.TurnToFaceEntity(vPolice1)
			Peds.Add(pVictim);

			pCop1 = Models.Peds.Cop.Create("Cop1", pSuspect.GetOffsetPositionFront(1.5f), 180, Convert.ToBoolean(Common.gRandom.Next(2)));
			pCop1.CreateBlip();
			pCop2 = Models.Peds.Cop.Create("Cop2", pVictim.GetOffsetPositionFront(1.5f), 180, Convert.ToBoolean(Common.gRandom.Next(2)));
			pCop2.CreateBlip();
			Peds.Add(pCop1);
			Peds.Add(pCop2);
			LSPD_First_Response.Mod.API.Functions.SetPedAsCop(pCop1);
			LSPD_First_Response.Mod.API.Functions.SetPedAsCop(pCop2);

			pCop1.KeepTasks = true;
			pCop1.TurnToFaceEntity(pSuspect);
			pCop1.Tasks.PlayAnimation("move_m@intimidation@cop@unarmed", "idle", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.Loop);

			pCop2.KeepTasks = true;
			pCop2.TurnToFaceEntity(pVictim);
			pCop2.Tasks.PlayAnimation("amb@code_human_police_investigate@idle_a", "idle_a", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.Loop);

			//pVictim.KeepTasks = True
			//pVictim.Tasks.PlayAnimation("amb@code_human_cower_stand@female@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask | AnimationFlags.Loop)

			//pSuspect.KeepTasks = True
			//pSuspect.Tasks.PlayAnimation("amb@code_human_wander_smoking@male@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask | AnimationFlags.Loop)

			if (PerformPedChecks()) {
				pDataSuspect = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(pSuspect);
				int mSuspectWanted = Common.gRandom.Next(3);
				if (mSuspectWanted == 0) {
					pDataSuspect = new Persona(pSuspect, Gender.Male, pDataSuspect.BirthDay, 0, pDataSuspect.Forename, pDataSuspect.Surname, ELicenseState.Valid, 5, true, false,
					false);
				} else {
					pDataSuspect = new Persona(pSuspect, Gender.Male, pDataSuspect.BirthDay, 0, pDataSuspect.Forename, pDataSuspect.Surname, ELicenseState.Valid, 5, false, false,
					false);
				}
				LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(pSuspect, pDataSuspect);

				pDataVictim = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(pVictim);
				int mVictimWanted = Common.gRandom.Next(5);
				if (mVictimWanted == 0) {
					pDataVictim = new Persona(pVictim, Gender.Female, pDataVictim.BirthDay, 0, pDataVictim.Forename, pDataVictim.Surname, ELicenseState.Valid, 0, true, false,
					false);
				} else {
					pDataVictim = new Persona(pVictim, Gender.Female, pDataVictim.BirthDay, 0, pDataVictim.Forename, pDataVictim.Surname, ELicenseState.Valid, 0, false, false,
					false);
				}
				LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(pVictim, pDataVictim);

				if (pDataSuspect.Wanted == true & pDataVictim.Wanted == true) {
					mWantedPeds = ArrestablePeds.Both;
				} else if (pDataSuspect.Wanted == true & pDataVictim.Wanted == false) {
					mWantedPeds = ArrestablePeds.Suspect;
				} else if (pDataSuspect.Wanted == false & pDataVictim.Wanted == true) {
					mWantedPeds = ArrestablePeds.Victim;
				} else {
					mWantedPeds = ArrestablePeds.None;
				}

				if (Common.IsComputerPlusRunning()) {
					AddPedToCallout(pSuspect);
					AddPedToCallout(pVictim);
				}

				return baseReturn;
			} else {
				Radio.DispatchMessage("Disregard, Call is Code 4", true);
				return false;
			}
		}

		public override bool PerformPedChecks()
		{
			bool @base = base.PerformPedChecks();

			if (@base) {
				if (pCop1.Exists() && pCop2.Exists() && pVictim.Exists() && pSuspect.Exists()) {
					if (pCop1.DistanceTo(pCop2.Position) > 25) {
						return false;
					}

					if (pVictim.DistanceTo(pSuspect.Position) > 25) {
						return false;
					}

					if (pVictim.DistanceTo(pCop1.Position) > 25) {
						return false;
					}

					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

		public override void OnArrivalAtScene()
		{
			base.OnArrivalAtScene();
			pCop1.TurnToFaceEntity(pSuspect);

			GameFiber.StartNew(() => { Game.DisplayHelp("Park up and make contact with the officer.", 8000); });
		}

		public override void Process()
		{
			base.Process();

			if (CalloutState == CalloutState.AtScene) {
				switch (mState) {
					case ScenarioState.PlayerResponding:
						if (Game.LocalPlayer.Character.IsInAnyVehicle(true) == false) {
							mState = ScenarioState.PlayerExitedVehicle;
						}

						break;
					case ScenarioState.PlayerExitedVehicle:
						GameFiber.StartNew(() =>
						{
							pCop2.Tasks.Clear();
							pCop2.TurnToFaceEntity(Game.LocalPlayer.Character);
							GameFiber.Sleep(500);
							//pCop2.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, Vector3.RelativeFront * 2)
							mState = ScenarioState.OfficerGreetingPlayer;
							pCop2.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 3f, 0f, 1.6f).WaitForCompletion();
							pCop2.TurnToFaceEntity(Game.LocalPlayer.Character);
						});

						break;
					case ScenarioState.OfficerGreetingPlayer:
						if (Game.LocalPlayer.Character.Position.DistanceTo(pCop2.Position) <= 4) {
							Game.DisplaySubtitle("~b~Officer: ~w~Hey, thanks for coming so quick. Come on, my partner will brief you.", 8000);
							Game.DisplayHelp("Follow the officer, and speak with their partner.", 8000);
							pCop1.Blip.Flash(500, 20000);

							GameFiber.StartNew(() =>
							{
								GameFiber.Sleep(2000);
								pCop2.Tasks.ClearImmediately();
								pCop2.Tasks.GoToOffsetFromEntity(pVictim, 3f, 0f, 1.6f).WaitForCompletion();
								pCop2.TurnToFaceEntity(pVictim);
								GameFiber.Sleep(500);
								pCop2.Tasks.PlayAnimation("amb@code_human_police_investigate@idle_a", "idle_a", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.Loop);
							});
							mState = ScenarioState.PlayerBriefed;
						}

						break;
					case ScenarioState.PlayerBriefed:
						if (Game.LocalPlayer.Character.Position.DistanceTo(pSuspect.Position) < 4 | Game.LocalPlayer.Character.Position.DistanceTo(pCop1.Position) < 4) {
							mState = ScenarioState.PlayerJoinedSituation;
						}

						break;
					case ScenarioState.PlayerJoinedSituation:
						mState = ScenarioState.SituationProceeding;
						AskPlayerToGetIDs();

						break;
					case ScenarioState.PlayerRunningNames:
						TimeSpan ts = DateTime.Now - playerAskedToRunNames;
						if (ts.TotalSeconds > 15 && Game.LocalPlayer.Character.IsOnFoot && Game.LocalPlayer.Character.Position.DistanceTo(pCop1.Position) < 4) {
							TimeSpan ts2 = DateTime.Now - warrantChecksDoneQuestionDisplayed;
							if (ts2.TotalSeconds > 8) {
								Game.DisplayHelp("Press CTRL + E to tell the other officer about the warrant checks.", 5000);
							}
						}

						if (Game.LocalPlayer.Character.Position.DistanceTo(pCop1.Position) < 4) {
							if (Common.IsKeyDown(Keys.E, Keys.ControlKey)) {
								mState = ScenarioState.WarrantChecksComplete;
							}
						}

						break;
					case ScenarioState.WarrantChecksComplete:
						mState = ScenarioState.ArrestProceeding;
						initiateArrest();

						break;
					case ScenarioState.ScenarioOver:
						if (pursuitinitiated == true) {
							if (LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(pursuit) == false) {
								mState = ScenarioState.CallIsCode4;
							}
						} else {
							if (LSPD_First_Response.Mod.API.Functions.IsPedArrested(pSuspect)) {
								if (pSuspect.Inventory.Weapons.Count > 0) {
									pSuspect.Inventory.Weapons.Clear();
									Game.DisplayNotification("While searching the suspect, a ~r~weapon~w~ is found.");
								}

								mState = ScenarioState.CallIsCode4;
							}
						}

						break;
					case ScenarioState.CallIsCode4:
						OnCallCode4();
						mState = ScenarioState.CalloutEnded;
						break;
				}
			}

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
				OnCallCode4();
				//if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
				//}
			}
		}

		private void AskPlayerToGetIDs()
		{
			mState = ScenarioState.PlayerGettingIDs;

			GameFiber.StartNew(() =>
			{
				Game.DisplaySubtitle("~b~Cop: ~w~Nice of you to join us, Officer.", 3000);
				GameFiber.Sleep(3000);
				Game.DisplaySubtitle("~b~Cop: ~w~" + pDataSuspect.Forename + " and " + pDataVictim.Forename + " here seem to have gotten into a fight.", 5000);
				GameFiber.Sleep(5000);
				Game.DisplaySubtitle("~b~Cop: ~w~Their full names are " + pDataSuspect.FullName + " and " + pDataVictim.FullName + ".", 5000);
				GameFiber.Sleep(5000);
				Game.DisplaySubtitle("~b~Cop: ~w~Can you run them for warrants? We'll sort out their stories.", 5000);
				playerAskedToRunNames = DateTime.Now;
				GameFiber.Sleep(3000);

				CallDetails += Environment.NewLine;
				CallDetails += Environment.NewLine;
				//CallDetails += String.Format("[{0}] UPDATE: ", DateTime.Now.ToString("M/d/yyyy HH:mm:ss"))
				CallDetails += "UPDATE: Subject names are " + pDataSuspect.FullName + " and " + pDataVictim.FullName + ".";

				Game.DisplayHelp("Go back to your vehicle and run the two subjects' names.", 5000);
				GameFiber.Sleep(5000);
				Game.DisplayHelp("When done, speak to the lead officer.", 5000);
				mState = ScenarioState.PlayerRunningNames;
			});
		}

		private void initiateArrest()
		{
			try {
				GameFiber.StartNew(() =>
				{
					int reactionFactor = Common.gRandom.Next(5);
					int victimReactionFactor = Common.gRandom.Next(2);

					Game.DisplaySubtitle("~b~Cop: ~w~Thank you, Officer.", 3000);
					GameFiber.Sleep(3000);

					ArrestablePeds mPedsToArrest = ArrestablePeds.None;
					//If mWantedPeds = ArrestablePeds.None Then mPedsToArrest = ArrestablePeds.Suspect

					int variationFactor = Common.gRandom.Next(0, 5);

					switch (variationFactor) {
						case 0:
							Game.DisplaySubtitle("~b~Cop: ~w~A passerby called 911 when they saw these two having a heated argument.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~They are both blaming each other for starting the fight.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~But when we rolled up, " + pDataSuspect.Forename + " shoved her to the ground.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~So he unfortunately, is guilty of Battery.", 5000);
							GameFiber.Sleep(5000);

							mPedsToArrest = ArrestablePeds.Suspect;
							break;
						case 1:
							mPedsToArrest = ArrestablePeds.None;
							Game.DisplaySubtitle("~b~Cop: ~w~What's happened here is, " + pDataSuspect.Forename + " has been having an affair.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~" + pDataVictim.Forename + " found out about it, and is understandably upset.", 5000);
							GameFiber.Sleep(5000);

							Game.DisplaySubtitle("~b~Cop: ~w~" + pDataSuspect.Forename + ", do you have a friend you can stay with today?", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~Maybe it'd be a good idea for both of you to cool off, ya know?", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~y~" + pDataSuspect.Forename + ": ~w~I think that'd be a good idea, Officer.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~y~" + pDataVictim.Forename + ": ~w~Fine with me. But you'll be hearing from my lawyer, " + pDataSuspect.Forename + "!", 5000);
							GameFiber.Sleep(5000);
							break;
						case 2:
							mPedsToArrest = ArrestablePeds.Victim;
							Game.DisplaySubtitle("~b~Cop: ~w~A passerby called 911 when they saw these two having a heated argument.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~" + pDataVictim.Forename + " admitted to attacking " + pDataSuspect.Forename + " with a frying pan.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~She got pretty violent with him...apparently he forgot their anniversary.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~While she's allowed to be mad, she can't be hitting people with things.", 5000);
							GameFiber.Sleep(5000);
							break;
						case 3:
							mPedsToArrest = ArrestablePeds.Both;
							Game.DisplaySubtitle("~b~Cop: ~w~So we can't really determine who is the aggressor here.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~They are both blaming each other for starting the fight.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~Neither of them seems to want to back down at all.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~" + pDataSuspect.Forename + ", do you have a friend you can stay with today?", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~Maybe it'd be a good idea for both of you to cool off, ya know?", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~y~" + pDataSuspect.Forename + ": ~w~Why?! It's MY house!! Why can't I live in my own damn house?!", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~y~" + pDataVictim.Forename + ": ~w~It's HALF MY HOUSE, you asshole!!", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~ENOUGH, both of you!!", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~y~" + pDataSuspect.Forename + ": ~w~Step off, pig. This ain't none of your business.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~When you're out here in public, that MAKES it my business!", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~I think we better settle this down at the station.", 5000);
							GameFiber.Sleep(5000);
							break;
						default:
							mPedsToArrest = ArrestablePeds.Suspect;
							Game.DisplaySubtitle("~b~Cop: ~w~So from their stories, it seems these two got into an argument that escalated.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~It appears that he attacked her during the argument, and assaulted her.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplaySubtitle("~b~Cop: ~w~He also threatened her just as we pulled up.", 5000);
							GameFiber.Sleep(5000);
							break;
					}

					switch (mPedsToArrest) {
						case ArrestablePeds.Suspect:
							if (mWantedPeds == ArrestablePeds.Victim | mWantedPeds == ArrestablePeds.Both) {
								mPedsToArrest = ArrestablePeds.Both;
								Game.DisplaySubtitle("~b~Cop: ~w~" + pDataVictim.Forename + ", you unfortunately have a warrant for your arrest.", 5000);
								GameFiber.Sleep(5000);
								Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000);
								GameFiber.Sleep(5000);
							}

							break;
						case ArrestablePeds.Victim:
							if (mWantedPeds == ArrestablePeds.Suspect | mWantedPeds == ArrestablePeds.Both) {
								mPedsToArrest = ArrestablePeds.Both;
								Game.DisplaySubtitle("~b~Cop: ~w~" + pDataSuspect.Forename + ", you unfortunately have a warrant for your arrest.", 5000);
								GameFiber.Sleep(5000);
								Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000);
								GameFiber.Sleep(5000);
							}

							break;
						case ArrestablePeds.None:
							if (mWantedPeds == ArrestablePeds.Suspect) {
								mPedsToArrest = ArrestablePeds.Suspect;
								Game.DisplaySubtitle("~b~Cop: ~w~" + pDataSuspect.Forename + ", you unfortunately have a warrant for your arrest.", 5000);
								GameFiber.Sleep(5000);
								Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000);
								GameFiber.Sleep(5000);
							} else if (mWantedPeds == ArrestablePeds.Victim) {
								mPedsToArrest = ArrestablePeds.Victim;
								Game.DisplaySubtitle("~b~Cop: ~w~" + pDataVictim.Forename + ", you unfortunately have a warrant for your arrest.", 5000);
								GameFiber.Sleep(5000);
								Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000);
								GameFiber.Sleep(5000);
							} else if (mWantedPeds == ArrestablePeds.Both) {
								mPedsToArrest = ArrestablePeds.Both;
								Game.DisplaySubtitle("~b~Cop: ~w~Unfortunately, you both have warrants for your arrest.", 5000);
								GameFiber.Sleep(5000);
								Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000);
								GameFiber.Sleep(5000);
							}

							break;
					}


					switch (mPedsToArrest) {
						case ArrestablePeds.Suspect:
							reactionFactor = Common.gRandom.Next(6);
							break;
						case ArrestablePeds.Victim:
							reactionFactor = -1;
							break;
						case ArrestablePeds.Both:
							reactionFactor = Common.gRandom.Next(3, 6);
							break;
					}


					if (mPedsToArrest == ArrestablePeds.Victim) {
						Game.DisplaySubtitle("~b~Cop: ~w~" + pDataVictim.Forename + ", I'm placing you under arrest.", 5000);
						GameFiber.Sleep(5000);
						Game.DisplaySubtitle("~b~Cop: ~w~Please put your hands behind your back. Officer, take her into custody.", 5000);
						GameFiber.Sleep(5000);

					} else if (mPedsToArrest == ArrestablePeds.Both) {
						Game.DisplaySubtitle("~b~Cop: ~w~" + pDataSuspect.Forename + " and " + pDataVictim.Forename + ", I'm placing you both under arrest.", 5000);
						GameFiber.Sleep(5000);
						Game.DisplaySubtitle("~b~Cop: ~w~Both of you, please put your hands behind your back. Cuff them please, Officer.", 5000);
						GameFiber.Sleep(5000);

					} else if (mPedsToArrest == ArrestablePeds.Suspect) {
						Game.DisplaySubtitle("~b~Cop: ~w~" + pDataSuspect.Forename + ", I'm placing you under arrest.", 5000);
						GameFiber.Sleep(5000);
						Game.DisplaySubtitle("~b~Cop: ~w~Please put your hands behind your back. Cuff him, Officer.", 5000);
						GameFiber.Sleep(5000);

					} else if (mPedsToArrest == ArrestablePeds.None) {
						mState = ScenarioState.CallIsCode4;
						return;
					}

					switch (reactionFactor) {
						case -1:
							//Victim
							if (victimReactionFactor == 0) {
								//Nothing
								Game.DisplaySubtitle("~y~" + pDataSuspect.Forename + ": ~w~That's what you get, bitch! Lock her up and throw away the key!", 5000);
							} else {
								//She runs
								Game.DisplaySubtitle("~r~" + pDataVictim.Forename + ": ~w~Go to hell, pig!!", 5000);
								pVictim.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_KNIFE"), 100, true);
								Stealth.Common.Natives.Peds.ReactAndFleePed(pVictim, pCop1);

								GameFiber.Sleep(4000);
								TriggerPursuit(false, true);
							}

							break;
						case 0:
							//Nothing
							Game.DisplaySubtitle("~y~Victim: ~w~Thank you, Officer! Lock him up and throw away the key!", 5000);

							break;
						case 1:
							//He runs
							Game.DisplaySubtitle("~r~Suspect: ~w~You'll have to catch me first!!", 5000);
							Stealth.Common.Natives.Peds.ReactAndFleePed(pSuspect, pCop1);

							GameFiber.Sleep(4000);
							TriggerPursuit(true, false);

							pVictim.Tasks.PlayAnimation("amb@code_human_cower_stand@female@idle_a", "idle_a", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.Loop);

							break;
						case 2:
							//He attacks
							Game.DisplaySubtitle("~r~Suspect: ~w~Go to hell, pig!!", 5000);
							pSuspect.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 100, true);
							pSuspect.AttackPed(pCop1);

							GameFiber.Sleep(4000);
							TriggerPursuit(true, false);

							pVictim.Tasks.PlayAnimation("amb@code_human_cower_stand@female@idle_a", "idle_a", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.Loop);

							break;
						case 3:
							//She attacks
							Game.DisplaySubtitle("~r~Victim: ~w~NO!! " + pDataSuspect.Forename.ToUpper() + ", RUN!!", 5000);
							pVictim.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_KNIFE"), 100, true);
							Stealth.Common.Natives.Peds.ReactAndFleePed(pSuspect, pCop1);
							pVictim.AttackPed(Game.LocalPlayer.Character);

							GameFiber.Sleep(4000);
							TriggerPursuit(true, true);

							break;
						case 4:
							//They both run
							Game.DisplaySubtitle("~y~Victim: ~w~" + pDataSuspect.Forename.ToUpper() + ", RUN!!", 5000);
							Stealth.Common.Natives.Peds.ReactAndFleePed(pSuspect, pCop1);
							Stealth.Common.Natives.Peds.ReactAndFleePed(pVictim, pCop2);

							GameFiber.Sleep(4000);
							TriggerPursuit(true, true);

							break;
						default:
							//He attacks her
							Game.DisplaySubtitle("~r~Suspect: ~w~You bitch! I'M GOING TO KILL YOU!!", 5000);
							pSuspect.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 100, true);
							pVictim.Tasks.PlayAnimation("amb@code_human_cower_stand@female@idle_a", "idle_a", 1.0f, AnimationFlags.SecondaryTask | AnimationFlags.Loop);
							pSuspect.AttackPed(pVictim);

							GameFiber.Sleep(4000);
							TriggerPursuit(true, false);

							break;
					}

					mState = ScenarioState.ScenarioOver;
				});
			} catch (Exception ex) {
				End();
				Game.DisplayNotification("Crashed!");
				Logger.LogVerbose(ex.ToString());
			}
		}

		private void TriggerPursuit(bool pAddSuspect, bool pAddVictim)
		{
			GameFiber.StartNew(() =>
			{
				try
				{
					pCop1.Tasks.Clear();
					pCop2.Tasks.Clear();

					pursuitinitiated = true;
					pursuit = Common.CreatePursuit();
					LSPD_First_Response.Mod.API.Functions.SetPursuitIsActiveForPlayer(pursuit, true);

					if (pAddSuspect != null && pAddSuspect.Exists()) {
			    			LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(pursuit, pSuspect);
					}

					if (pAddVictim != null && pAddVictim.Exists()) {
			    			LSPD_First_Response.Mod.API.Functions.AddPedToPursuit(pursuit, pVictim);
					}
				} catch (Exception ex) {
					End();
					Game.DisplayNotification("Crashed!");
					Logger.LogVerbose(ex.ToString());
				}
			}

				GameFiber.Sleep(1000);
				LSPD_First_Response.Mod.API.Functions.AddCopToPursuit(pursuit, pCop1);
				LSPD_First_Response.Mod.API.Functions.AddCopToPursuit(pursuit, pCop2);
			});
		}

		private void OnCallCode4()
		{
			Radio.CallIsCode4(this.ScriptInfo.Name);

			if (vPolice1.Exists() && pCop1.Exists() && pCop2.Exists()) {
				if (pCop2.DistanceTo(vPolice1.Position) > 50) {
					End();
					return;
				}
			} else {
				End();
				return;
			}

			if (pCop1.Exists()) {
				pCop1.Tasks.ClearImmediately();
			}

			if (pCop2.Exists()) {
				pCop2.Tasks.ClearImmediately();
			}

			if (pVictim != null && pVictim.Exists() == true) {
				pVictim.Tasks.Clear();
			}

			DateTime whileLoopStarted;

			GameFiber.StartNew(() =>
			{
				if (vPolice1.Exists()) {
					if (pCop1.Exists()) {
						pCop1.Tasks.FollowNavigationMeshToPosition(vPolice1.GetOffsetPosition(Vector3.RelativeLeft * 2f), 0, 1.2f).WaitForCompletion();
						if (pCop1.Exists() && vPolice1.Exists())
							pCop1.TurnToFaceEntity(vPolice1);
						if (pCop1.Exists() && vPolice1.Exists())
							pCop1.Tasks.EnterVehicle(vPolice1, -1);
					}
				}
			});

			GameFiber.StartNew(() =>
			{
				if (vPolice1 != null && vPolice1.Exists()) {
					if (pCop2 != null && pCop2.Exists()) {
						//pCop2.Tasks.FollowToOffsetFromEntity(vPolice1, Vector3.RelativeRight * 2)
						pCop2.Tasks.FollowNavigationMeshToPosition(vPolice1.GetOffsetPosition(Vector3.RelativeRight * 2f), 0, 1.2f).WaitForCompletion();
						if (pCop2.Exists() && vPolice1.Exists())
							pCop2.TurnToFaceEntity(vPolice1);
						if (pCop2.Exists() && vPolice1.Exists())
							pCop2.Tasks.EnterVehicle(vPolice1, (int)vPolice1.GetFreePassengerSeatIndex());
					}
				}
			});

			GameFiber.StartNew(() =>
			{
				GameFiber.Sleep(3000);

				whileLoopStarted = DateTime.Now;

				while (true) {
					GameFiber.Yield();

					if (pCop1.Exists() & pCop2.Exists()) {
						if (pCop1.IsInAnyVehicle(false) == true && pCop2.IsInAnyVehicle(false) == true) {
							break; // TODO: might not be correct. Was : Exit While
						}
					} else {
						break; // TODO: might not be correct. Was : Exit While
					}

					TimeSpan ts = (DateTime.Now - whileLoopStarted);
					if (ts.TotalSeconds >= 30) {
						break; // TODO: might not be correct. Was : Exit While
					}
				}

				if (vPolice1.Exists()) {
					if (pCop1.Exists()) {
						if (pCop1.IsInAnyVehicle(true) == false) {
							pCop1.Tasks.Clear();
							pCop1.WarpIntoVehicle(vPolice1, -1);
						}
					}

					if (pCop2.Exists()) {
						if (pCop2.IsInAnyVehicle(true) == false) {
							pCop2.Tasks.Clear();
							pCop2.WarpIntoVehicle(vPolice1, (int)vPolice1.GetFreePassengerSeatIndex());
						}
					}

					vPolice1.IsSirenOn = false;
					if (pCop1.Exists()) {
						pCop1.Tasks.CruiseWithVehicle(vPolice1, 12, VehicleDrivingFlags.Normal);
					}
				}

				End();
			});
		}

		private void ArrestCheck()
		{
			ArrestCheck(pSuspect);
			ArrestCheck(pVictim);

			if (arrestedTipDisplayed == false && anyoneArrested == true) {
				arrestedTipDisplayed = true;
				Game.DisplayHelp("You may end this callout with " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + ", or continue investigating.");
			}
		}

		private void ArrestCheck(PedBase p)
		{
			if (p != null) {
				if (p.Exists()) {
					if (p.IsDead | p.IsArrested()) {
						p.DeleteBlip();
						anyoneArrested = true;
					}
				}
			}
		}

		public override bool RequiresSafePedPoint {
			get { return true; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return true; }
		}

	}

}
