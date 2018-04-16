using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System.Windows.Forms;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System;
using static Stealth.Plugins.Code3Callouts.Common;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Impaired Driver", CalloutProbability.Medium)]
    internal class ImpairedDriver : CalloutBase
	{

        string[] PedModels = {
			"A_M_Y_SouCent_01",
			"A_M_Y_StWhi_01",
			"A_M_Y_StBla_01",
			"A_M_Y_Downtown_01",
			"A_M_Y_BevHills_01",
			"G_M_Y_MexGang_01",
			"G_M_Y_MexGoon_01",
			"G_M_Y_StrPunk_01",
			"A_F_Y_GenHot_01",
			"A_F_Y_Hippie_01",
			"A_F_Y_Hipster_01",
			"A_F_Y_BevHills_01",
			"A_F_Y_BevHills_02",
			"A_F_M_Tourist_01",
			"A_F_M_FatWhite_01",
			"A_F_M_Business_02",
			"A_M_M_BevHills_01",
			"A_M_M_GenFat_01",
			"A_M_M_Business_01",
			"A_M_M_Golfer_01",
			"A_M_M_Skater_01",
			"A_M_M_Salton_01",
			"A_M_M_Tourist_01"

		};

        string[] VehModels = {
			"Blista",
			"Felon",
			"Jackal",
			"Oracle",
			"Asea",
			"Emperor",
			"Fugitive",
			"Ingot",
			"Premier",
			"Primo",
			"Stanier",
			"Stratum"
		};

		bool suspectVisual = false;
		LHandle pursuit;
		//bool pursuitinitiated = false;
		//bool officerRespondedCode3 = false;

		public ImpairedDriver() : base("Impaired Driver", CallResponseType.Code_2)
        {
			RadioCode = 502;
			CrimeEnums = new List<DISPATCH.CRIMES>() {
				DISPATCH.CRIMES.A_DUI,
				DISPATCH.CRIMES.DRIVER_UNDER_INFLUENCE,
				DISPATCH.CRIMES.POSSIBLE_502,
				DISPATCH.CRIMES.CODE_502
			};

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "Caller reports a driver who appears to be intoxicated, and driving all over the road.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;

			Objective = "Track down the ~y~driver.~n~~w~Stop them from hurting anyone!";
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			bool baseReturn = base.OnBeforeCalloutDisplayed();

			if (baseReturn == false) {
				return false;
			}

			SkipRespondingState = true;

			string model1 = VehModels[Common.gRandom.Next(VehModels.Length)];

			Vehicles.Vehicle vehicle1 = new Vehicles.Vehicle(model1, World.GetNextPositionOnStreet(SpawnPoint), gRandom.Next(360));
			vehicle1.Name = "Vehicle1";
			vehicle1.MakePersistent();
			vehicle1.SetRandomColor();

			Suspect driver1 = new Suspect("Driver1", PedModels[Common.gRandom.Next(PedModels.Length)], vehicle1.Position.Around(3), 0, false);
			driver1.DisplayName = "Driver";
			driver1.SetIsDrunk(true);
			driver1.MakePersistent();
			driver1.Armor = 69;
			driver1.SpeechLines = PedHelper.RandomizeImpairedDriverStory(Common.gRandom.Next(15));

			try {
				AnimationSet animSet = new AnimationSet("move_m@drunk@verydrunk");
				animSet.LoadAndWait();
				driver1.MovementAnimationSet = animSet;
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error animating ped -- " + ex.Message);
			}

			Peds.Add(driver1);
			Vehicles.Add(vehicle1);

			if (driver1.Exists() && vehicle1.Exists())
				driver1.WarpIntoVehicle(vehicle1, -1);
			if (driver1.Exists() && vehicle1.Exists())
				driver1.Tasks.CruiseWithVehicle(vehicle1, 12, VehicleDrivingFlags.Normal);

			GameFiber.StartNew(() =>
			{
				GameFiber.Sleep(10000);

				while (true) {
					//driver1.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveLeft)
					//GameFiber.Sleep(2000)
					if (driver1.Exists() && vehicle1.Exists() && driver1.IsInAnyVehicle(false)) {
						if (Functions.IsPlayerPerformingPullover()) {
							break; // TODO: might not be correct. Was : Exit While
						}

						if (driver1.Exists() && vehicle1.Exists() && driver1.IsInAnyVehicle(false))
							driver1.Tasks.PerformDrivingManeuver(vehicle1, VehicleManeuver.SwerveRight);
						GameFiber.Sleep(2000);
						if (driver1.Exists() && vehicle1.Exists() && driver1.IsInAnyVehicle(false))
							driver1.Tasks.CruiseWithVehicle(vehicle1, 12, VehicleDrivingFlags.Normal);

						GameFiber.Sleep(10000);
						GameFiber.Yield();
					} else {
						break; // TODO: might not be correct. Was : Exit While
					}
				}
			});

			if (PerformPedChecks()) {
				if (Common.IsComputerPlusRunning()) {
					AddVehicleToCallout(vehicle1);
					AddPedToCallout(driver1);
				}

				return baseReturn;
			} else {
				Logger.LogVerboseDebug("basereturn = false");
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

			Suspect d1 = (Suspect)GetPed("Driver1");
			if (d1 != null && d1.Exists()) {
				d1.CreateBlip();
			} else {
				Game.DisplayNotification("Impaired Driver Callout crashed");
				Logger.LogVerboseDebug("d1 null or !exists");
				End();
				return;
			}

			if (Common.IsComputerPlusRunning()) {
				CADAtScene();
			}

			Vehicles.Vehicle v = GetVehicle("Vehicle1");
			if (v != null && v.Exists()) {
				v.FillColorValues();
				CallDetails += string.Format("Suspect vehicle is a {0} {1}, License # {2}", v.PrimaryColorName, v.Model.Name, v.LicensePlate);
				Radio.DispatchMessage(string.Format("Suspect vehicle is a {0} {1}, License # {2}", v.PrimaryColorName, v.Model.Name, v.LicensePlate), true);
			}

			GameFiber.StartNew(() =>
			{
				GameFiber.Sleep(4000);
				Game.DisplayHelp("Pull over the impaired driver.", 8000);
				GameFiber.Sleep(8000);
				Game.DisplayHelp("Press " + Config.SpeakKey.ToString() + " to talk to the driver.", 8000);
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

			if (Game.LocalPlayer.Character.IsDead) {
				Logger.LogVerboseDebug("player dead");
				return;
			}

			Suspect d1 = (Suspect)GetPed("Driver1");
			Vehicles.Vehicle v = GetVehicle("Vehicle1");

			if (Common.IsKeyDown(Config.SpeakKey)) {
				SpeakToSubject(ref d1);
			}

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
					Radio.CallIsCode4(this.ScriptInfo.Name);
					End();
				//if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
    //            }
            }

			if (suspectVisual == false) {
				if (Game.LocalPlayer.Character.Position.DistanceTo(d1.Position) < 60) {
					suspectVisual = true;
					Radio.SuspectSpotted();

					if (Game.LocalPlayer.Character.IsInAnyVehicle(false) == true) {
						Vehicle copcar = Game.LocalPlayer.Character.CurrentVehicle;

						if (copcar != null) {
							if (copcar.Exists() && copcar.HasSiren) {
								if (copcar.IsSirenOn == true && copcar.IsSirenSilent == false) {
									//officerRespondedCode3 = true;
									//pursuitinitiated = true;
									pursuit = Common.CreatePursuit();
									d1.AddToPursuit(pursuit);

									Game.DisplayNotification("The suspect heard your siren and is fleeing.");
									Radio.SergeantMessage("Exercise proper siren use on Code 2 calls, over");
								}
							}
						}
					}
				}
			}

			bool vehTowed = false;
			if (v != null) {
				if (v.Exists() == false) {
					vehTowed = true;
					Logger.LogVerboseDebug("vehTowed = True");
				}
			} else {
				vehTowed = true;
			}

			if (vehTowed == true && ArrestCheck(d1)) {
				Logger.LogVerboseDebug("vehTowed and ped arrested or dead");

				if (d1.Exists()) {
					Radio.CallIsCode4(this.ScriptInfo.Name, d1.IsArrested());
				} else {
					Radio.CallIsCode4(this.ScriptInfo.Name, false);
				}

				End();
			}
		}

		private bool ArrestCheck(Suspect s)
		{
			if (s.Exists()) {
				return s.IsArrested() || s.IsDead;
			} else {
				return true;
			}
		}

		public override void End()
		{
			Logger.LogVerboseDebug("ending call");

			Suspect d1 = (Suspect)GetPed("Driver1");
			Vehicles.Vehicle v = GetVehicle("Vehicle1");

			if (d1 != null && d1.Exists()) {
				Logger.LogVerboseDebug("deleting ped blip");
				d1.DeleteBlip();
				d1.Dismiss();
			}

			if (v != null && v.Exists()) {
				Logger.LogVerboseDebug("deleting veh blip");
				v.Dismiss();
			}

			base.End();
		}

		private void SpeakToSubject(ref Suspect d1)
		{
			if (d1.Exists() && Game.LocalPlayer.Character.Position.DistanceTo(d1.Position) < 3) {
				d1.Speak();
				return;
			}
		}

		public override bool RequiresSafePedPoint {
			get { return false; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return false; }
		}

	}

}