using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common.Extensions;
using Stealth.Common.Models;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using System.Windows.Forms;
using LSPD_First_Response.Mod.API;
using static Stealth.Common.Models.QuestionWindow;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Road Rage", CalloutProbability.Medium)]
	internal class RoadRage : CalloutBase
	{

        string[] VehModels = {
			"Blista",
			"Jackal",
			"Oracle",
			"Asea",
			"Emperor",
			"Fugitive",
			"Ingot",
			"Premier",
			"Primo",
			"Stanier",
			"Stratum",
			"Asterope",
			"Baller",
			"Bison",
			"Cavalcade2",
			"Exemplar",
			"F620",
			"Felon",
			"FQ2",
			"Gresley",
			"Habanero",
			"Intruder",
			"Landstalker",
			"Mesa",
			"Minivan",
			"Patriot",
			"Radi",
			"Regina",
			"schafter2",
			"Seminole",
			"Sentinel",
			"Serrano",
			"Speedo",
			"Surge",
			"Tailgater",
			"Washington",
			"Zion"
		};

        string[] PedModels =  {
			"A_M_Y_SouCent_01",
			"A_M_Y_StWhi_01",
			"A_M_Y_StBla_01",
			"A_M_Y_Downtown_01",
			"A_M_Y_BevHills_01",
			"G_M_Y_MexGang_01",
			"G_M_Y_MexGoon_01",
			"G_M_Y_StrPunk_01",
			"A_M_M_BevHills_01",
			"A_M_M_GenFat_01",
			"A_M_M_Business_01",
			"A_M_M_Golfer_01",
			"A_M_M_Skater_01",
			"A_M_M_Salton_01",
			"A_M_M_Tourist_01"

		};

		bool mIsSuspectHonking = true;
		bool mOfficerFoundVehicles = false;
		bool mSuspectArrested = false;

		bool mOfficerWithVictim = false;
		Vehicles.Vehicle vehVictim;

		Victim pedVictim;
		Vehicles.Vehicle vehSuspect;
		Suspect pedSuspect;

        //bool endTipDisplayed = false;

        public RoadRage() : base("Road Rage in Progress", CallResponseType.Code_3)
        {
			RadioCode = 0;
			CrimeEnums = new List<DISPATCH.CRIMES>() { DISPATCH.CRIMES.CIV_ASSISTANCE };

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "Caller is being harassed by another driver. Suspect is currently chasing RP's vehicle and attempting to ram them.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "Suspect is yelling and giving obscene gestures. Suspect possibly armed. Proceed with caution.";

			Objective = "Track down both vehicles!~n~Pull over the ~r~suspect!";
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			bool baseReturn = base.OnBeforeCalloutDisplayed();

			if (baseReturn == false) {
				return false;
			}

			SkipRespondingState = true;

			Vector3 position = World.GetNextPositionOnStreet(SpawnPoint.Around(5));
			VehicleNode node = Stealth.Common.Natives.Vehicles.GetClosestVehicleNodeWithHeading(position);

			if (node.Position == Vector3.Zero) {
				node.Position = position;
				node.Heading = Common.gRandom.Next(360);
			}

			AddMinimumDistanceCheck(30, position);

			vehVictim = new Vehicles.Vehicle(VehModels[Common.gRandom.Next(VehModels.Length)], position, node.Heading);
			pedVictim = new Victim("Victim1", PedModels[Common.gRandom.Next(PedModels.Length)], vehVictim.GetOffsetPosition(Vector3.RelativeLeft * 3f), 0);
			pedVictim.DisplayName = "Victim";

			vehSuspect = new Vehicles.Vehicle(VehModels[Common.gRandom.Next(VehModels.Length)], vehVictim.GetOffsetPosition(Vector3.RelativeBack * 10f), node.Heading);
			pedSuspect = new Suspect("Suspect1", PedModels[Common.gRandom.Next(PedModels.Length)], vehSuspect.GetOffsetPosition(Vector3.RelativeLeft * 3f), 0, true);
			pedSuspect.DisplayName = "Suspect";

			if (pedVictim.Exists() && vehVictim.Exists() && pedSuspect.Exists() && vehSuspect.Exists()) {
				vehVictim.Name = "VictimCar1";
				vehVictim.MakePersistent();
				vehVictim.SetRandomColor();
				Vehicles.Add(vehVictim);

				pedVictim.MakePersistent();
				pedVictim.BlockPermanentEvents = true;
				pedVictim.WarpIntoVehicle(vehVictim, -1);
				Peds.Add(pedVictim);
				pedVictim.CreateBlip();

				if (Common.IsComputerPlusRunning()) {
					AddPedToCallout(pedVictim);
					AddVehicleToCallout(vehVictim);
					AddVehicleToCallout(vehSuspect);
				}

				{
					pedVictim.SpeechLines.Add("Thank you so much, Officer!!");
					pedVictim.SpeechLines.Add("Your response was really fast! You saved my life!!");
					pedVictim.SpeechLines.Add("What about my car, though?");
					pedVictim.SpeechLines.Add("Mors Mutual will take care of the damage, but...");
					pedVictim.SpeechLines.Add("Can I drive it home, or do you need to tow it away?");
					pedVictim.SpeechLines.Add("Can you check it over for me?");
					pedVictim.SpeechLines.Add("You can tow it away if you need to.");
				}

				LSPD_First_Response.Engine.Scripting.Entities.Persona pDataVictim = Functions.GetPersonaForPed(pedVictim);
				if (pDataVictim.Wanted | pDataVictim.LicenseState != LSPD_First_Response.Engine.Scripting.Entities.ELicenseState.Valid) {
					pDataVictim = new LSPD_First_Response.Engine.Scripting.Entities.Persona(pedVictim, pDataVictim.Gender, pDataVictim.BirthDay, pDataVictim.Citations, pDataVictim.Forename, pDataVictim.Surname, LSPD_First_Response.Engine.Scripting.Entities.ELicenseState.Valid, pDataVictim.TimesStopped, false, false,
					false);
					Functions.SetPersonaForPed(pedVictim, pDataVictim);
				}

				vehSuspect.Name = "SuspectCar1";
				vehSuspect.MakePersistent();
				vehSuspect.SetRandomColor();
				Vehicles.Add(vehSuspect);

				pedSuspect.MakePersistent();
				pedSuspect.BlockPermanentEvents = true;
				pedSuspect.WarpIntoVehicle(vehSuspect, -1);
				pedSuspect.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_KNIFE"), 56, false);
				pedSuspect.SetDrunkRandom();

				pedSuspect.QAItems = new List<QAItem>();
				pedSuspect.QAItems.Add(new QAItem("What the hell did you think you were doing?!", "Teaching that idiot a lesson!"));
				pedSuspect.QAItems.Add(new QAItem("Why were you chasing the other driver?", "That fucker cut me off!! They deserve to die!!"));
				pedSuspect.QAItems.Add(new QAItem("Do you realize you put other drivers in danger?", "Oh spare me the speech, pig!"));
				pedSuspect.QAItems.Add(new QAItem("Don't you think you should calm down?", "Why don't you take off that badge and gun, so I can kick your ass?"));
				pedSuspect.QAItems.Add(new QAItem("Anything else you want to say?", "Yeah. Tell your wife she owes me for last night, bitch."));

				Peds.Add(pedSuspect);
				pedSuspect.CreateBlip();

				//AI::_TASK_VEHICLE_FOLLOW
				//void _TASK_VEHICLE_FOLLOW(Ped driver, Vehicle vehicle, Entity targetEntity,
				//int drivingStyle, float speed, float minDistance)
				//// 0xFC545A9F0626E3B6

				//Dim p As New Ped(Vector3.Zero)

				//Dim pHash As ULong = &HFC545A9F0626E3B6UL
				//Dim args As Native.NativeArgument() = {pedSuspect, vehSuspect, pedVictim, 3, 12, 10}
				//Dim a As New Native.NativeArgument(CType(pedSuspect, IHandleable))
				//Rage.Native.NativeFunction.CallByHash(Of ULong)(pHash, pedSuspect, vehSuspect, pedVictim, 3, 12, 10)

				try {
					GameFiber.StartNew(() =>
					{
						pedVictim.Tasks.CruiseWithVehicle(12, VehicleDrivingFlags.Emergency);
						//Stealth.Common.Natives.Peds.FollowEntityinVehicle(s, susVeh, v, 3, 12, 10)
						//Stealth.Common.Natives.Peds.EscortVehicle(pedSuspect, vehSuspect, vehVictim, 0, 12, 0, 10, 0, 0.1)
						Stealth.Common.Natives.Peds.ChaseEntityInVehicle(pedSuspect, pedVictim);
					});

					GameFiber.StartNew(() =>
					{
						ulong pHash = 0x9c8c6504b5b63d2cuL;

						while (mIsSuspectHonking) {
							Rage.Native.NativeFunction.CallByHash<ulong>(pHash, GetNativeArgument(vehSuspect), 1000, Game.GetHashKey("HELDDOWN"), 0);
							GameFiber.Sleep(1500);
						}
					});
				} catch (Exception ex) {
					Logger.LogVerbose("Exception when calling road rage native");
					Logger.LogVerbose("s -- " + pedSuspect.ToString());
					Logger.LogVerbose("susVeh -- " + vehSuspect.ToString());
					Logger.LogVerbose("p -- " + pedVictim.ToString());
					Logger.LogVerbose(ex.ToString());
				}

				return true;
			} else {
				Logger.LogVerbose("Road rage aborted");
				return false;
			}
		}

		public override bool OnCalloutAccepted()
		{
			CalloutState = CalloutState.AtScene;
			OnArrivalAtScene();

			return base.OnCalloutAccepted();
		}

		public override void OnArrivalAtScene()
		{
			//MyBase.OnArrivalAtScene()

			if (Common.IsComputerPlusRunning()) {
				CADAtScene();
			}

			GameFiber.StartNew(() =>
			{
				GameFiber.Sleep(4000);
				Game.DisplayHelp("Dispatch is on the phone with the victim.", 8000);
				GameFiber.Sleep(8000);
				Game.DisplayHelp("Track down the vehicles, and pull over the suspect.", 8000);
				GameFiber.Sleep(8000);
				Game.DisplayHelp("Deal with the situation as you see fit.", 8000);
			});
		}

		private void CADAtScene()
		{
			if (Common.IsComputerPlusRunning()) {
				ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.At_Scene);
			}
		}

		public override void Process()
		{
			base.Process();

			if ((pedSuspect == null || pedSuspect.Exists() == false) | (pedVictim == null || pedVictim.Exists() == false)) {
				return;
			}

			if (Common.IsKeyDown(Config.SpeakKey)) {
				SpeakToSubject();
			}

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
                Radio.CallIsCode4(this.ScriptInfo.Name);
                End();

    //            if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
					
				//}
			}

			if (pedSuspect.IsArrested() || pedSuspect.IsDead) {
				//Radio.CallIsCode4(Me.Scriptinfo.Name, pedSuspect.IsArrested)
				//[End]()

				if (mSuspectArrested == false) {
					mSuspectArrested = true;

					if (Common.IsComputerPlusRunning()) {
						AddPedToCallout(pedSuspect);
					}

					if (pedSuspect.Inventory.Weapons.Count > 0) {
						pedSuspect.Inventory.Weapons.Clear();
						Game.DisplayNotification("While searching the suspect, you find/remove a ~r~knife~w~.");
					}

					GameFiber.StartNew(() =>
					{
						if (pedVictim.Exists()) {
							Radio.SergeantMessage("Please return to the victim and ensure they're okay.");

							if (pedVictim.IsDead == false) {
								Radio.DispatchMessage("We have the RP on the phone; they are awaiting police arrival");

								Game.DisplayHelp("Return to the victim's location.", 8000);
								GameFiber.Sleep(8000);
								Game.DisplayHelp("Press " + Config.SpeakKey.ToString() + " to speak to them, and ensure they are okay.", 8000);
								GameFiber.Sleep(8000);
								Game.DisplayHelp("Call EMS for the victim if necessary.", 8000);
								GameFiber.Sleep(8000);

								if (pedSuspect.IsAlive) {
									Game.DisplayHelp("Also, ensure that you question the suspect using the interaction menu.", 8000);
									GameFiber.Sleep(8000);
								}

								Game.DisplayHelp("Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout when the situation is over.", 8000);
							} else {
								Game.DisplayHelp("Return to the victim's location, and ensure they are okay.", 8000);
								GameFiber.Sleep(8000);
								Game.DisplayHelp("Call EMS for the victim if necessary.", 8000);
								GameFiber.Sleep(8000);

								if (pedSuspect.IsAlive) {
									Game.DisplayHelp("Also, ensure that you question the suspect using the interaction menu.", 8000);
									GameFiber.Sleep(8000);
								}
								Game.DisplayHelp("Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout when the situation is over.", 8000);
							}
						}
					});
				}
			}

			if (mOfficerFoundVehicles == false) {
				if ((vehVictim != null && vehVictim.Exists()) && (vehSuspect != null && vehSuspect.Exists())) {
					if (Game.LocalPlayer.Character.Position.DistanceTo(vehVictim.Position) < 20 | Game.LocalPlayer.Character.Position.DistanceTo(vehSuspect.Position) < 20) {
						mOfficerFoundVehicles = true;
						Radio.SuspectSpotted();

						GameFiber.StartNew(() =>
						{
							GameFiber.Sleep(6000);
							mIsSuspectHonking = false;
						});

						GameFiber.StartNew(() =>
						{
							pedVictim.Tasks.Clear();
							pedVictim.Tasks.ParkVehicle(vehVictim.Position, vehVictim.Heading);
							//GameFiber.StartNew(
							//    Sub()
							//        pedVictim.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait)
							//        GameFiber.Sleep(2000)
							//        pedVictim.Tasks.ParkVehicle(vehVictim.Position, vehVictim.Heading)
							//    End Sub)

							pedSuspect.Tasks.Clear();

							if (pedSuspect.IsInAnyVehicle(false) && pedSuspect.CurrentVehicle != null && pedSuspect.CurrentVehicle.Exists()) {
								pedSuspect.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
							}

							int susReaction = Common.gRandom.Next(10);
							if (susReaction >= 6) {
								//Drive away casually
								if (pedSuspect.IsInAnyVehicle(false) && pedSuspect.CurrentVehicle != null && pedSuspect.CurrentVehicle.Exists()) {
									pedSuspect.Tasks.CruiseWithVehicle(12, VehicleDrivingFlags.Normal);
								}

							} else if (susReaction >= 2 & susReaction <= 5) {
								//Flee
								LHandle pursuit = Common.CreatePursuit();
								Functions.AddPedToPursuit(pursuit, pedSuspect);

							} else {
								//Attack victim
								if (pedSuspect.IsOnFoot == false)
									pedSuspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
								if (pedVictim.IsOnFoot == false)
									pedVictim.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
								pedSuspect.AttackPed(pedVictim);
							}
						});
					}
				}
			}

			if (pedVictim.Exists() == true && pedVictim.IsDead == false) {
				if (mSuspectArrested == true && mOfficerWithVictim == false) {
					if (Game.LocalPlayer.Character.Position.DistanceTo(pedVictim.Position) < 20) {
						mOfficerWithVictim = true;

						if (pedVictim.IsInAnyVehicle(true) == true) {
							GameFiber.StartNew(() =>
							{
								pedVictim.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
								GameFiber.Sleep(2000);
								pedVictim.TurnToFaceEntity(Game.LocalPlayer.Character);
							});
						} else {
							pedVictim.Tasks.Clear();
							pedVictim.TurnToFaceEntity(Game.LocalPlayer.Character);
						}
					}
				}
			}
		}

		private void SpeakToSubject()
		{
			if (pedVictim.Exists()) {
				if (Game.LocalPlayer.Character.Position.DistanceTo(pedVictim.Position) < 3) {
					pedVictim.Speak();
					return;
				}
			}
		}

		public override bool RequiresSafePedPoint {
			get { return false; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return true; }
		}

	}

}