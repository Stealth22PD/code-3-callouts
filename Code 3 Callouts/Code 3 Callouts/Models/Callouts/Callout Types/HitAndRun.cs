using LSPD_First_Response;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common.Extensions;
using Stealth.Common.Models;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static Stealth.Common.Models.QuestionWindow;
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Hit and Run", CalloutProbability.Medium)]
    internal class HitAndRun : CalloutBase
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
            "Stratum"
        };

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

		bool suspectIdentified = false;
		//bool spokenToMedics = false;
		//bool medicsCode4 = false;
		Blip suspectSearchArea;
		float searchAreaRadius = 150f;
		Vector3 suspectLastSeen;
		bool fullPlate = false;
		int licensePlateFactor;
		string licensePlate = "";
		DateTime lastLocationUpdate = DateTime.Now;

		bool endTipDisplayed = false;

		bool suspectCounterOn = false;

        public HitAndRun() : base("Hit and Run", CallResponseType.Code_3)
        {
			
			RadioCode = 480;
			CrimeEnums = new List<DISPATCH.CRIMES>() {
				DISPATCH.CRIMES.CODE_480,
				DISPATCH.CRIMES.HIT_AND_RUN
			};

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "A pedestrian has been struck by a vehicle; the driver left the scene immediately, and did not stop.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "EMS has been dispatched to the scene; should arrive shortly. Do not leave until EMS arrives.";

			Objective = "Speak to the ~o~witness~w~, and wait for EMS!~n~Apprehend the ~r~suspect!";
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			bool baseReturn = base.OnBeforeCalloutDisplayed();

			try {
				if (baseReturn == false) {
					return false;
				}

				Vector3 pedSpawn = PedHelper.GetPedSpawnPoint(World.GetNextPositionOnStreet(SpawnPoint.Around(3)));
				Victim ped = new Victim("Victim1", PedModels[Common.gRandom.Next(PedModels.Length)], pedSpawn, 0);
				ped.DisplayName = "Pedestrian";
				//ped.CreateBlip()
				//ped.Tasks.PlayAnimation(New AnimationDictionary("amb@world_human_bum_slumped@male@laying_on_left_side@idle_a"), "idle_a", 1.0F, AnimationFlags.StayInEndFrame)
				ped.MakePersistent();
				ped.Kill();
				Peds.Add(ped);

				Vector3 wSpawn = PedHelper.GetPedSpawnPoint(World.GetNextPositionOnStreet(SpawnPoint.Around(10)));
				Witness w = new Witness("Witness1", PedModels[Common.gRandom.Next(PedModels.Length)], wSpawn, 0);
				w.DisplayName = "Witness";
				w.MakePersistent();
				w.BlockPermanentEvents = true;
				Peds.Add(w);

				if (Common.IsComputerPlusRunning()) {
					AddPedToCallout(ped);
					AddPedToCallout(w);
				}
			} catch (Exception ex) {
				Logger.LogTrivialDebug(ex.Message);
				Logger.LogTrivialDebug(ex.StackTrace);
			}

			try {
				//Dim directions As Vector3() = {Vector3.WorldNorth, Vector3.WorldSouth, Vector3.WorldEast, Vector3.WorldWest}
				//Dim mDirection As Vector3 = directions[Common.gRandom.Next(directions.Length))
				//Dim suspectSpawn As Vector3 = World.GetNextPositionOnStreet(SpawnPoint)

				Vehicles.Vehicle suspectVehicle = new Vehicles.Vehicle(VehModels[Common.gRandom.Next(VehModels.Length)], World.GetNextPositionOnStreet(SpawnPoint.Around(250)), gRandom.Next(360));
				suspectVehicle.MakePersistent();
				suspectVehicle.Name = "SuspectVehicle";
				suspectVehicle.SetRandomColor();
				DamageVehicle(suspectVehicle);

				if (Common.IsTrafficPolicerRunning()) {
					TrafficPolicerFunctions.SetVehicleinsuranceStatus(suspectVehicle, false);
				}

				Vehicles.Add(suspectVehicle);

				Suspect driver = new Suspect("Suspect1", PedModels[Common.gRandom.Next(PedModels.Length)], World.GetNextPositionOnStreet(suspectVehicle.Position.Around(3)), 0, false);
				driver.Name = "Suspect1";
				driver.MakePersistent();
				driver.Tasks.ClearImmediately();
				driver.WarpIntoVehicle(suspectVehicle, -1);
				driver.Tasks.CruiseWithVehicle(suspectVehicle, 12, VehicleDrivingFlags.Normal);
				driver.SetDrunkRandom();

				driver.QAItems = new List<QAItem>();

				int mStory = Common.gRandom.Next(4);

				switch (mStory) {
					case 0:
						driver.QAItems.Add(new QAItem("Do you know what happened?", "Yes, officer. I...I'm so sorry."));
						driver.QAItems.Add(new QAItem("Why did you leave the scene?", "I was scared...I don't have insurance."));
						driver.QAItems.Add(new QAItem("Have you been drinking today?", "Just a...um...no?"));
						driver.QAItems.Add(new QAItem("Why didn't you come back to the scene?", "I don't know. I didn't want to get in trouble."));
						driver.QAItems.Add(new QAItem("Do you have anything else to say?", "Am I going to go to jail?"));
						break;
					case 1:
						driver.QAItems.Add(new QAItem("Do you know what happened?", "That pedestrian jumped in front of me!"));
						driver.QAItems.Add(new QAItem("Why did you leave the scene?", "Leave the scene? Why do I need to stop?"));
						driver.QAItems.Add(new QAItem("Have you been drinking today?", "Don't you have anything better to do?"));
						driver.QAItems.Add(new QAItem("Why didn't you come back to the scene?", "Come back? I was going home!"));
						driver.QAItems.Add(new QAItem("Do you have anything else to say?", "Why are you hassling me for no reason?!"));
						break;
					case 2:
						driver.QAItems.Add(new QAItem("Do you know what happened?", "Yeah! Polecat released another video!!"));
						driver.QAItems.Add(new QAItem("Why did you leave the scene?", "Because I don't slow down for nobody!"));
						driver.QAItems.Add(new QAItem("Have you been drinking today?", "No, but I think I need one after seeing your ugly mug."));
						driver.QAItems.Add(new QAItem("Why didn't you come back to the scene?", "Ain't nobody got TIME for that!!"));
						driver.QAItems.Add(new QAItem("Do you have anything else to say?", "Are you always such an asshole?"));
						break;
					default:
						driver.QAItems.Add(new QAItem("Do you know what happened?", "Look what that idiot did to my car!!"));
						driver.QAItems.Add(new QAItem("Why did you leave the scene?", "I don't have time for that shit!!"));
						driver.QAItems.Add(new QAItem("Have you been drinking today?", "Why don't you breathalyze me, pig?"));
						driver.QAItems.Add(new QAItem("Why didn't you come back to the scene?", "Are you kidding? I'm late for an appointment!!"));
						driver.QAItems.Add(new QAItem("Do you have anything else to say?", "Can I leave now? I'm late!!"));
						break;
				}

				Peds.Add(driver);
				suspectLastSeen = driver.Position;
			} catch (Exception ex) {
				Logger.LogTrivialDebug(ex.Message);
				Logger.LogTrivialDebug(ex.StackTrace);
			}

			if (PerformPedChecks()) {
				return baseReturn;
			} else {
				return false;
			}
		}

		public override bool OnCalloutAccepted()
		{
			Radio.DispatchMessage("~g~EMS ~w~has been dispatched to the scene", true);

			Vehicles.Vehicle suspectVeh = GetVehicle("SuspectVehicle");
			if (suspectVeh != null && suspectVeh.Exists()) {
				suspectVeh.FillColorValues();
			}

			return base.OnCalloutAccepted();
		}

		public override void OnArrivalAtScene()
		{
			base.OnArrivalAtScene();

			LSPD_First_Response.Mod.API.Functions.RequestBackup(Game.LocalPlayer.Character.Position, EBackupResponseType.Code3, EBackupUnitType.Ambulance);

			Victim vic = (Victim)GetPed("Victim1");

			Vehicles.Vehicle suspectVeh = GetVehicle("SuspectVehicle");

			Witness w = (Witness)GetPed("Witness1");
			if (w != null && w.Exists()) {
				w.SpeechLines = new List<string>();
				w.SpeechLines.Add("I saw what happened, Officer!");
				w.SpeechLines.Add("That car just hit the poor pedestrian and took off!");
				w.SpeechLines.Add("The windshield shattered, and the car has a lot of front-end damage.");
				w.SpeechLines.Add(string.Format("The car was a {0} colored {1}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name));

				licensePlateFactor = Common.gRandom.Next(3);
				if (licensePlateFactor == 0) {
					w.SpeechLines.Add(string.Format("The license plate number was {0}.", suspectVeh.LicensePlate));
					licensePlate = suspectVeh.LicensePlate;
					fullPlate = true;
				} else if (licensePlateFactor == 1) {
					w.SpeechLines.Add(string.Format("The first three digits of the license plate were {0}.", suspectVeh.LicensePlate.Substring(0, 3)));
					licensePlate = suspectVeh.LicensePlate.Substring(0, 3);
				} else {
					int idx = suspectVeh.LicensePlate.Length - 3;
					w.SpeechLines.Add(string.Format("The last three digits of the license plate were {0}.", suspectVeh.LicensePlate.Substring(idx)));
					licensePlate = suspectVeh.LicensePlate.Substring(idx);
				}

				w.CreateBlip();
				w.TurnToFaceEntity(Game.LocalPlayer.Character);
				Game.DisplaySubtitle("Witness: Officer!! Over here!!", 8000);
			}

			SuspectSearch = SuspectSearchStateEnum.Null;
			Game.DisplayHelp("Press " + Config.SpeakKey.ToString() + " to talk to the witness.", 8000);
		}

		public override void Process()
		{
			base.Process();

			if (Game.LocalPlayer.Character.IsDead) {
				return;
			}

			Vehicles.Vehicle suspectVeh = GetVehicle("SuspectVehicle");
			//Dim ambu As Vehicles.Vehicle = GetVehicle("Ambulance")
			//Dim m1 As Witness = GetPed("Medic1")
			//Dim m2 As Witness = GetPed("Medic2")
			Victim vic = (Victim)GetPed("Victim1");
			Witness w = (Witness)GetPed("Witness1");
			Suspect s = (Suspect)GetPed("Suspect1");

			if (CalloutState == CalloutState.UnitResponding) {
				if (w.IsDead) {
					Vector3 wSpawn = PedHelper.GetPedSpawnPoint(World.GetNextPositionOnStreet(SpawnPoint.Around(10)));
					w.Position = wSpawn;
					w.Resurrect();
				}
			}

			if (Common.IsKeyDown(Config.SpeakKey)) {
				SpeakToSubject(ref w);
			}

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
				Radio.CallIsCode4(this.ScriptInfo.Name);
				End();

				//if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
    //            }
            }

			if (CalloutState == CalloutState.AtScene) {
				if (s == null || s.Exists() == false) {
					Game.DisplayNotification("Hit and Run callout crashed.");
					Logger.LogTrivial("Error occurred - Suspect no longer exists; possibly despawned by GTA V?");
					Radio.CallIsCode4(this.ScriptInfo.Name);
					End();
					return;
				}

				if (s.IsArrested()) {
					if (endTipDisplayed == false) {
						endTipDisplayed = true;

						GameFiber.StartNew(() =>
						{
							AddPedToCallout(s);
							Game.DisplayHelp("Ensure that you question the suspect using the interaction menu.", 5000);
							GameFiber.Sleep(5000);
							Game.DisplayHelp("Press ~b~" + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " ~w~to end this callout when the situation is over.", 8000);
						});
					}
				} else {
					if (s.IsDead) {
						Radio.CallIsCode4(this.ScriptInfo.Name, s.IsArrested());
						End();
					}
				}

				if (SuspectSearch == SuspectSearchStateEnum.Null) {
					if (suspectIdentified == false && w.HasSpoken) {
						suspectIdentified = true;
						suspectLastSeen = suspectVeh.Position;
						CreateSearchArea(suspectLastSeen);
						SuspectSearch = SuspectSearchStateEnum.NotYetLocated;

						string mUpdate = "";

						if (fullPlate) {
							mUpdate += string.Format("UPDATE: Vehicle was a {0} colored {1}; License # {2}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate);
						} else {
							mUpdate += string.Format("UPDATE: Vehicle was a {0} colored {1}; Partial license {2}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate);
						}

						if (Common.IsComputerPlusRunning()) {
							AddVehicleToCallout(suspectVeh);
							ComputerPlusFunctions.AddUpdateToCallout(CalloutID, mUpdate);
						}

						if (fullPlate == true) {
							Radio.UnitMessage(string.Format("Suspect vehicle is a {0} {1}, License # {2}", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate));
						} else {
							Radio.UnitMessage(string.Format("Suspect vehicle is a {0} {1}, Partial license # {2}", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate));
						}

						Radio.DispatchMessage("Roger", true);
						Game.DisplayHelp("Search the area for the suspect.");

						string pAudio = "SUSPECT_LAST_SEEN IN_OR_ON_POSITION";

						string mHeading = Common.GetDirectionAudiofromHeading(suspectVeh.Heading);
						if (mHeading != "") {
							pAudio = string.Format("SUSPECT_LAST_SEEN DIR_HEADING {0} IN_OR_ON_POSITION", mHeading);
						}

						AudioPlayerEngine.PlayAudio(pAudio, suspectLastSeen);
					}
				}

				if (SuspectSearch == SuspectSearchStateEnum.NotYetLocated) {

					if (Game.LocalPlayer.Character.Position.DistanceTo(suspectVeh.GetOffsetPosition(Vector3.RelativeBack * 2)) <= 15) {
						if (suspectCounterOn == false) {
							suspectCounterOn = true;
							DateTime startTime = DateTime.Now;

							GameFiber.StartNew(() =>
							{
								while (Game.LocalPlayer.Character.Position.DistanceTo(suspectVeh.GetOffsetPosition(Vector3.RelativeBack * 2)) <= 15) {
									GameFiber.Yield();

									TimeSpan ts = DateTime.Now - startTime;

									if (ts.TotalSeconds >= 3) {
										SuspectSearch = SuspectSearchStateEnum.Located;
										DeleteSearchArea();
										//suspectVeh.CreateBlip(Drawing.Color.Yellow)
										s.CreateBlip();
										Radio.SuspectSpotted();

										break; // TODO: might not be correct. Was : Exit While
									}
								}

								suspectCounterOn = false;
							});
						}

					}

					if (suspectSearchArea != null && suspectSearchArea.Exists() && suspectCounterOn == false) {
						TimeSpan ts = DateTime.Now - lastLocationUpdate;

						if (ts.TotalSeconds > 30) {
							GameFiber.StartNew(() =>
							{
								suspectLastSeen = suspectVeh.Position;
								suspectSearchArea.Position = suspectVeh.Position;
								lastLocationUpdate = DateTime.Now;

								if (fullPlate == true) {
									Radio.DispatchMessage(string.Format("License # ~b~{0} ~w~captured by ALPR camera, over", licensePlate), true);
								} else {
									Radio.DispatchMessage(string.Format("Partial license ~b~{0} ~w~captured by ALPR camera, over", licensePlate), true);
								}

								string pAudio = "SUSPECT_LAST_SEEN IN_OR_ON_POSITION";

								string mHeading = Common.GetDirectionAudiofromHeading(suspectVeh.Heading);
								if (mHeading != "") {
									pAudio = string.Format("SUSPECT_LAST_SEEN DIR_HEADING {0} IN_OR_ON_POSITION", mHeading);
								}

								AudioPlayerEngine.PlayAudio(pAudio, suspectVeh.Position);

								Game.DisplayHelp("The search area has been updated.");

								Blip tempBlip = new Blip(s.Position);
								tempBlip.Color = Color.Red;

								GameFiber.Sleep(3000);

								if (tempBlip != null && tempBlip.IsValid()) {
									tempBlip.Delete();
								}
							});
						}
					}

				}
			}
		}

		public override void End()
		{
			Victim vic = (Victim)GetPed("Victim1");
			Vehicles.Vehicle ambu = GetVehicle("Ambulance");
			Witness m1 = (Witness)GetPed("Medic1");
			Witness m2 = (Witness)GetPed("Medic2");

			if (vic != null && vic.Exists()) {
				vic.Delete();
			}

			if (m1 != null && m1.Exists()) {
				m1.Delete();
			}

			if (m2 != null && m2.Exists()) {
				m2.Delete();
			}

			if (ambu != null && ambu.Exists()) {
				ambu.Delete();
			}

			DeleteSearchArea();
			base.End();
		}

		private void SpeakToSubject(ref Witness w)
		{
			//If m1 IsNot Nothing && m1.Exists Then
			//    If Game.LocalPlayer.Character.Position.DistanceTo(m1.Position) < 3 Then
			//        m1.Speak()
			//        spokenToMedics = True
			//        Exit Sub
			//    End If
			//Else
			//    Game.DisplayNotification("Hit and Run Callout crashed")
			//    [End]()
			//End If

			//If m2 IsNot Nothing && m2.Exists Then
			//    If Game.LocalPlayer.Character.Position.DistanceTo(m2.Position) < 3 Then
			//        m2.Speak()
			//        spokenToMedics = True
			//        Exit Sub
			//    End If
			//Else
			//    Game.DisplayNotification("Hit and Run Callout crashed")
			//    [End]()
			//End If

			if (w != null && w.Exists()) {
				if (Game.LocalPlayer.Character.Position.DistanceTo(w.Position) < 3) {
					w.Speak();
					return;
				}
			} else {
				Game.DisplayNotification("Hit and Run Callout crashed");
				End();
			}
		}

		private void CreateSearchArea(Vector3 pSpawnPoint)
		{
			suspectSearchArea = new Blip(pSpawnPoint, searchAreaRadius);
			suspectSearchArea.Color = Color.FromArgb(100, Color.Yellow);
			suspectSearchArea.StopFlashing();
		}

		private void DeleteSearchArea()
		{
			try {
				if (suspectSearchArea != null) {
					suspectSearchArea.Delete();
				}
			} catch (Exception ex) {
				Logger.LogTrivialDebug("Error deleting search area -- " + ex.Message);
			}
		}

		private void DamageVehicle(Vehicle v)
		{
			{
				float radius = Common.gRandom.Next(300, 500);
				float damageFactor = Common.gRandom.Next(200, 300);

				//Try
				//    .Windows.Item(0).Smash()
				//Catch ex As Exception
				//End Try

				v.Deform(Vector3.RelativeFront, radius, damageFactor);

				int health = Common.gRandom.Next(500, 700);
				v.Health = health;
				v.EngineHealth = health;
				v.FuelTankHealth = health;
			}
		}

		private SuspectSearchStateEnum SuspectSearch = SuspectSearchStateEnum.Null;
		enum SuspectSearchStateEnum
		{
			Null = 0,
			NotYetLocated = 1,
			Located = 2,
			Escaped = 3
		}

		public override bool RequiresSafePedPoint {
			get { return false; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return true; }
		}

	}

}