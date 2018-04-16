using LSPD_First_Response;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using System.Windows.Forms;
using LSPD_First_Response.Engine.Scripting;
using System;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioPlayerEngine;
using System.Drawing;
using System.Linq;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Unknown Trouble", CalloutProbability.Medium)]
    internal class UnknownTrouble : CalloutBase
	{

		Ped lionPrey = null;
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
		string[] SuspectModels = {
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
		string[] PedModels = {
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
		Blip searchArea;
		bool foundDB = false;
		bool detectiveDispatched = false;
		string detName = "";
		//string vicName = "";
		Vector3 vicPosition;
		float vicHeading;
		DateTime startedinspect = DateTime.MinValue;

		string detUnitNumber = "";
        public UnknownTrouble() : base("Unknown Trouble", CallResponseType.Code_2)
        {
			LionSearchState = LionSearchStateEnum.Null;
			RadioCode = 0;
			CrimeEnums = new List<DISPATCH.CRIMES>() { DISPATCH.CRIMES.CIV_ASSISTANCE };

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "Caller sounded like they needed help, but line was disconnected.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "No answer on callback. No further details available at this time.";

			Objective = "Attend to the scene.~n~Be prepared for anything!";
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			int randomint = Common.gRandom.Next(1, 101);

			if (randomint <= 10) {
				//10% chance
				TroubleType = TroubleTypeEnum.MountainLion;

			} else if (randomint >= 11 && randomint <= 50) {
				//40% chance
				TroubleType = TroubleTypeEnum.FalseCall;

			} else if (randomint >= 51 && randomint <= 70) {
				//20% chance
				TroubleType = TroubleTypeEnum.AssaultinProgress;

			} else if (randomint >= 71 && randomint <= 85) {
				//15% chance
				TroubleType = TroubleTypeEnum.DeadBody;
			} else {
				//15% chance
				TroubleType = TroubleTypeEnum.None;
			}

			return base.OnBeforeCalloutDisplayed();
		}

		public override bool OnCalloutAccepted()
		{
			bool validCall;

			switch (TroubleType) {
				case TroubleTypeEnum.MountainLion:
					validCall = SpawnLion();
					break;
				case TroubleTypeEnum.FalseCall:
					validCall = SpawnFalseCall();
					break;
				case TroubleTypeEnum.AssaultinProgress:
					validCall = SpawnAssault();
					break;
				case TroubleTypeEnum.DeadBody:
					validCall = SpawnDeadBody();
					break;
				default:
					validCall = true;
					break;
			}

			if (validCall == false) {
				return false;
			}

			return base.OnCalloutAccepted();
		}

		internal bool SpawnLion()
		{
			Suspect lion = null;

			Vector3 lionSpawn = PedHelper.GetSafeCoordinatesForPed(World.GetNextPositionOnStreet(SpawnPoint.Around(Common.gRandom.Next(50, 100))));
			if (lionSpawn == Vector3.Zero) {
				lionSpawn = World.GetNextPositionOnStreet(SpawnPoint.Around(Common.gRandom.Next(50, 100)));
			}

			lion = new Suspect("Lion", new Model("a_c_mtlion"), lionSpawn, 0f, true);
			lion.RelationshipGroup = "COUGAR";
			Game.SetRelationshipBetweenRelationshipGroups("COUGAR", "PLAYER", Relationship.Hate);
			Game.SetRelationshipBetweenRelationshipGroups("COUGAR", "CIVMALE", Relationship.Hate);
			Game.SetRelationshipBetweenRelationshipGroups("COUGAR", "CIVFEMALE", Relationship.Hate);

			if (lion != null) {
				if (lion.Exists() == false) {
					return false;
				} else {
					lion.Tasks.Wander();
					Peds.Add(lion);
					return true;
				}
			} else {
				return false;
			}
		}

		internal bool SpawnFalseCall()
		{
			Logger.LogTrivialDebug("spawning false call");

			Vector3 callerSpawn = PedHelper.GetPedSpawnPoint(SpawnPoint.Around(10));

			Victim ped = new Victim("Caller1", PedModels[Common.gRandom.Next(PedModels.Length)], callerSpawn, 0);
			ped.DisplayName = "Caller";

			int victimStory = Common.gRandom.Next(7);

			if (victimStory == 0) {
				ped.SpeechLines.Add("Oh, Officer, thank god you're here!!");
				ped.SpeechLines.Add("He...he was after me!!");
				ped.SpeechLines.Add("The ghost...he was after me!");
				ped.SpeechLines.Add("He's always following me!!");
				ped.SpeechLines.Add("Please help me!!");
			} else if (victimStory == 1) {
				ped.SpeechLines.Add("Officer, I am SO sorry!");
				ped.SpeechLines.Add("I was heading to a friend's place, and my phone was in my pocket.");
				ped.SpeechLines.Add("My lock screen has an \"Emergency Call\" button.");
				ped.SpeechLines.Add("I don't know how, but it somehow got pressed.");
				ped.SpeechLines.Add("I'm sorry to have wasted your time!");
			} else if (victimStory == 2) {
				ped.SpeechLines.Add("It's about time you showed up!!");
				ped.SpeechLines.Add("I was at Up-n-Atom Burger earlier...");
				ped.SpeechLines.Add("I SPECIFICALLY asked for hot French Fries with my order!");
				ped.SpeechLines.Add("But they only gave me lukewarm! and only 2 packets of Ketchup!");
				ped.SpeechLines.Add("Then the Manager asked me to leave the restaurant!");
				ped.SpeechLines.Add("I didn't even get my Sprunk!!!");
				ped.SpeechLines.Add("I want them all arrested!! NOW!!!");
			} else if (victimStory == 3) {
				ped.SpeechLines.Add("I can't get a cab right now.");
				ped.SpeechLines.Add("Can you drive me home?");
			} else if (victimStory == 4) {
				ped.SpeechLines.Add("Oh, Officer, thank god you're here!!");
				ped.SpeechLines.Add("My neighbour really pissed me off today.");
				ped.SpeechLines.Add("Can I borrow your gun?");
				ped.SpeechLines.Add("I'll give it back, I swear!");
			} else if (victimStory == 5) {
				ped.SpeechLines.Add("Oh, Officer, thank god you're here!!");
				ped.SpeechLines.Add("My phone's WiFi has stopped working!");
				ped.SpeechLines.Add("Can you fix it?");
			} else if (victimStory == 6) {
				ped.SpeechLines.Add("Hey, you're pretty cute...");
				ped.SpeechLines.Add("What's your number?");
			}

			if (ped.Exists()) {
				Peds.Add(ped);
				return true;
			} else {
				return false;
			}
		}

		internal bool SpawnAssault()
		{
			Logger.LogTrivialDebug("spawning assault");

			Vector3 callerSpawn = PedHelper.GetPedSpawnPoint(SpawnPoint.Around(10));
			Vector3 suspectSpawn = PedHelper.GetPedSpawnPoint(callerSpawn.Around(10));

			Victim v = new Victim("Victim1", VictimModels[Common.gRandom.Next(VictimModels.Length)], callerSpawn, 0);
			v.BlockPermanentEvents = true;
			Suspect s = new Suspect("Suspect1", SuspectModels[Common.gRandom.Next(SuspectModels.Length)], suspectSpawn, 0, true);

			if (v.Exists() && s.Exists()) {
				Peds.Add(v);
				Peds.Add(s);
				return true;
			} else {
				return false;
			}
		}

		internal bool SpawnDeadBody()
		{
			Vector3 pedSpawn = PedHelper.GetPedSpawnPoint(World.GetNextPositionOnStreet(SpawnPoint.Around(30)));
			vicPosition = pedSpawn;
			Victim ped = new Victim("Victim1", PedModels[Common.gRandom.Next(PedModels.Length)], pedSpawn, 0);
			ped.DisplayName = "Pedestrian";
			ped.MakePersistent();

			vicHeading = ped.Heading;

			ped.Kill();

			//Dim vp As LSPD_First_Response.Engine.Scripting.Entities.Persona
			//vp = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(ped, vp)
			//vicName = "John Doe";

			Peds.Add(ped);

			return true;
		}

		public override void Process()
		{
			base.Process();

			if (Game.LocalPlayer.Character.IsDead) {
				return;
			}

			bool endCall = true;

			if (TroubleType == TroubleTypeEnum.MountainLion) {
				endCall = ProcessLionCall();
			} else {
				endCall = false;
			}

			if (endCall == true) {
				End();
			}

			if (TroubleType == TroubleTypeEnum.DeadBody) {
				Victim v = (Victim)GetPed("Victim1");

				if (CalloutState == CalloutState.AtScene) {
					if (foundDB == false) {
						if (v.Exists() && Game.LocalPlayer.Character.Position.DistanceTo(v.Position) < 15) {
							foundDB = true;

							v.CreateBlip();

							if (searchArea != null) {
								if (searchArea.Exists()) {
									searchArea.Delete();
								}
							}

							Game.DisplayHelp("Press " + Config.SpeakKey.ToString() + " to call for a homicide detective.");
						}
					} else {
						MonitorDetective();
					}
				}
			}

			if (TroubleType == TroubleTypeEnum.AssaultinProgress) {
				Victim v = (Victim)GetPed("Victim1");
				Suspect s = (Suspect)GetPed("Suspect1");

				if (s != null && v != null) {
					if (s.Exists() && v.Exists()) {
						if (s.IsArrested()) {
							v.DeleteBlip();
							v.Tasks.Clear();
							v.Dismiss();
						}
					}
				}
			}

			if (Common.IsKeyDown(Config.SpeakKey)) {
				if (TroubleType == TroubleTypeEnum.FalseCall) {
					Victim v = (Victim)GetPed("Caller1");

					if (v != null & v.Exists()) {
						if (Game.LocalPlayer.Character.Position.DistanceTo(v.Position) < 3) {
							v.Speak();
						}
					}
				} else if (TroubleType == TroubleTypeEnum.DeadBody) {
					if (foundDB == true) {
						if (detectiveDispatched == false) {
							detectiveDispatched = true;
							SpawnHomicideDetective();
							RadioForDetective();
						}
					}
				}
			}
		}

		private void RadioForDetective()
		{
			GameFiber.StartNew(() =>
			{
				Game.DisplayNotification(string.Format("~b~{0}: ~w~{0}, I've got a dead body, possible 187. Requesting a detective, over.", Common.gUnitNumber));
				Radio.PlayRadioAnimation();

				Radio.DispatchMessage("Roger.", true);

				List<AudioFile> pAudio = new List<AudioFile>();

				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.DIVISION.DIV_01));
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.UNIT_TYPE.HENRY));
				Array units = Enum.GetValues(typeof(DISPATCH.BEAT));
				int iDetUnit = Common.gRandom.Next(units.Length);
				DISPATCH.BEAT detUnit = (DISPATCH.BEAT)units.GetValue(iDetUnit);
				pAudio.Add(new AudioFile("DISPATCH", detUnit));

				detUnitNumber = "1-HENRY-" + (iDetUnit + 1);

				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORTING.OFFICERS_REPORT));

				if (Common.gRandom.Next(1) == 0) {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CRIMES.CODE_187));
				} else {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CRIMES.HOMICIDE));
				}

				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CONJUNCTIVES.IN_OR_ON_POSITION));

				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.RESPOND_CODE_2));

				int responseint = Common.gRandom.Next(3);
				if (responseint == 1) {
					pAudio.Add(new AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.ROGER_EN_ROUTE));
				} else if (responseint == 2) {
					pAudio.Add(new AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.ROGER_ON_OUR_WAY));
				} else {
					pAudio.Add(new AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.COPY_IN_VICINITY));
				}

				int rogerint = Common.gRandom.Next(4);
				if (rogerint == 0) {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER));
				} else if (rogerint == 1) {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT));
				} else if (rogerint == 2) {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR));
				} else {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT));
				}

				AudioPlayerEngine.PlayAudio(pAudio, SpawnPoint);

				Game.DisplayHelp("Wait for the Detective before calling the Coroner.");
				string detMsg = string.Format("~b~{0}: ~w~{0} to {1}, don't move the body until I get there, over.", detUnitNumber, Common.gUnitNumber);
				Game.DisplayNotification(detMsg);
			});
		}

		private void SpawnHomicideDetective()
		{
			Cop det = new Cop("Detective", "s_m_m_ciasec_01", World.GetNextPositionOnStreet(SpawnPoint.Around(150)), 0);
			det.MakePersistent();
			det.RelationshipGroup = "COP";
			det.DisplayName = "Detective";
			det.BlockPermanentEvents = true;
			det.CreateBlip();

			DetectiveState = EDetectiveState.Created;

			System.DateTime DoB = Common.GetRandomDateOfBirth();
			string name = LSPD_First_Response.Engine.Scripting.Entities.Persona.GetRandomFullName();
			string[] nameParts = name.Split(' ');
			detName = nameParts[1];
            LSPD_First_Response.Engine.Scripting.Entities.Persona p = new LSPD_First_Response.Engine.Scripting.Entities.Persona(det, Gender.Male, DoB, 0, nameParts[0], nameParts[1], LSPD_First_Response.Engine.Scripting.Entities.ELicenseState.Valid, 0, false, false,
			true);
			det.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 56, false);
			LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(det, p);

			string[] unmarkedPoliceCars = {
				"FBI",
				"POLICE4"
			};
			Vehicles.Vehicle detectiveVeh = new Vehicles.Vehicle(unmarkedPoliceCars[Common.gRandom.Next(unmarkedPoliceCars.Length)], World.GetNextPositionOnStreet(det.Position.Around(10)), 0);
			det.WarpIntoVehicle(detectiveVeh, -1);
			detectiveVeh.IsSirenOn = true;
			detectiveVeh.IsSirenSilent = true;
			detectiveVeh.Name = "DetectiveUnit";
			detectiveVeh.MakePersistent();

			DetectiveState = EDetectiveState.Dispatched;

			Vector3 detTarget = Game.LocalPlayer.Character.Position.Around(15);
			det.Tasks.DriveToPosition(detTarget, 15, (VehicleDrivingFlags.Emergency));

			Peds.Add(det);
			Vehicles.Add(detectiveVeh);
			PedsToIgnore.Add(det.Handle);

			DetectiveState = EDetectiveState.Responding;

			Logger.LogTrivialDebug("Detective responding");
		}

		private void MonitorDetective()
		{
			Cop d = (Cop)GetPed("Detective");
			Victim v = (Victim)GetPed("Victim1");

			if (d != null && d.Exists()) {
				switch (DetectiveState) {
					case EDetectiveState.Responding:
						if (d.Position.DistanceTo(Game.LocalPlayer.Character.Position) <= 25) {
							d.Tasks.Clear();

							if (d.CurrentVehicle != null && d.CurrentVehicle.Exists()) {
								if (d.CurrentVehicle.HasSiren) {
									d.CurrentVehicle.BlipSiren(true);
								}

								d.Tasks.ParkVehicle(d.CurrentVehicle.Position, d.CurrentVehicle.Heading);
							}
							Logger.LogTrivialDebug("Parked vehicle");

							d.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
							DetectiveState = EDetectiveState.AtScene;


							//GameFiber.StartNew(
							//    Sub()
							//        Logger.LogTrivialDebug("Sleeping")
							//        GameFiber.Sleep(8000)
							//        Logger.LogTrivialDebug("Introducing himself")
							//        Game.DisplaySubtitle(text, 6000)
							//    End Sub)
						}
						break;
					case EDetectiveState.AtScene:
						d.Tasks.Clear();
						DetectiveState = EDetectiveState.WalkingToPlayer;

						GameFiber.StartNew(() =>
						{
							d.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 3f, 0f, 1.5f).WaitForCompletion();
							d.TurnToFaceEntity(Game.LocalPlayer.Character);

							while (true) {
								if (d.Position.DistanceTo(Game.LocalPlayer.Character.Position) <= 4f) {
									DetectiveState = EDetectiveState.SpeakingToPlayer;

									string text = string.Format("~b~Detective: ~w~Hey, Detective {0}, Homicide. What do ya got?", detName);
									Game.DisplaySubtitle(text, 6000);
									break; // TODO: might not be correct. Was : Exit While
								}

								GameFiber.Yield();
							}
						});

						break;
					case EDetectiveState.SpeakingToPlayer:
						//Logger.LogTrivialDebug("Walking to victim")

						GameFiber.StartNew(() =>
						{
							GameFiber.Sleep(5000);

							Logger.LogTrivialDebug("Walking to victim");
							d.Tasks.GoToOffsetFromEntity(v, 3f, 0f, 1.5f);
							DetectiveState = EDetectiveState.WalkingToVictim;
						});

						break;
					case EDetectiveState.WalkingToVictim:

						if (d.Position.DistanceTo(vicPosition) <= 4f) {
							Logger.LogTrivialDebug("Close to vic, he should now inspect");
							DetectiveState = EDetectiveState.AboutToinspectVictim;
						}
						break;
					case EDetectiveState.AboutToinspectVictim:
						Logger.LogTrivialDebug("About to inspect vic");
						DetectiveState = EDetectiveState.KneelingToinspect;

						break;
					case EDetectiveState.KneelingToinspect:
						GameFiber.StartNew(() =>
						{
							//Dim v3DetToVic As Vector3 = (vicPosition - d.Position)
							//v3DetToVic.Normalize()
							//Dim hdg As Single = MathHelper.ConvertDirectionToHeading(v3DetToVic)

							d.Tasks.Clear();
							//d.Tasks.AchieveHeading(hdg)
							d.TurnToFaceEntity(v);
							GameFiber.Sleep(1000);

							startedinspect = DateTime.Now;
							d.Tasks.PlayAnimation("amb@medic@standing@kneel@idle_a", "idle_a", 1f, AnimationFlags.Loop);
							Logger.LogTrivialDebug("Should be animated");
							DetectiveState = EDetectiveState.inspectingVictim;
						});

						break;
					case EDetectiveState.inspectingVictim:
						if (startedinspect == DateTime.MinValue) {
							Logger.LogTrivialDebug("Wtf? No time set?");
							return;
						} else {
							if ((DateTime.Now - startedinspect).TotalSeconds < 6) {
								Logger.LogTrivialDebug("Time not expired yet");
								return;
							} else {
								DetectiveState = EDetectiveState.Done;
								Logger.LogTrivialDebug("Time expired");

								GameFiber.StartNew(() =>
								{
									d.Tasks.Clear();

									//Game.DisplaySubtitle(String.Format("~b~Detective: ~w~Alright, victim's ID says {0}. Not sure how long the body has been here.", vicName), 3000)
									//Game.DisplayHelp("Run the victim's name for the case file, then call the Coroner.")
									Game.DisplaySubtitle("~b~Detective: ~w~Got a GSW...no real signs of a struggle. Not sure how long the body has been here.", 3000);

									GameFiber.Sleep(3000);
									Game.DisplaySubtitle("~b~Detective: ~w~I'll get started on the case file. You can call the Coroner.", 3000);
									Game.DisplayHelp("You can end this callout by pressing " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + ".");

									Logger.LogTrivialDebug("Cleared to call coroner");

									Vehicles.Vehicle dv = GetVehicle("DetectiveUnit");
									if (dv != null && dv.Exists()) {
										Logger.LogTrivialDebug("Going back to his car");
										d.Tasks.ClearImmediately();

										Vector3 v3DetToCar = (dv.Position - d.Position);
										v3DetToCar.Normalize();
										float hdg = MathHelper.ConvertDirectionToHeading(v3DetToCar);

										d.Tasks.FollowNavigationMeshToPosition(dv.GetOffsetPosition(Vector3.RelativeLeft * 1.5f), hdg, 2f).WaitForCompletion();
										//d.Tasks.GoToOffsetfromEntity(dv, 3.0F, 90, 1.5F).WaitForCompletion()
										d.Tasks.EnterVehicle(dv, -1);
									} else {
										Logger.LogTrivialDebug("vehicle doesnt exist?!?!?");
									}
								});
							}
						}
						break;
					case EDetectiveState.Done:
						if (v == null || v.Exists() == false) {
							Radio.CallIsCode4(this.ScriptInfo.Name);
							End();
						}
						break;
				}
			}
		}

		public override void OnArrivalAtScene()
		{
			try {
				base.OnArrivalAtScene();

				switch (TroubleType) {
					case TroubleTypeEnum.MountainLion:
						ReportMountainLion();
						break;

					case TroubleTypeEnum.FalseCall:
						Victim v = (Victim)GetPed("Caller1");
						v.CreateBlip();
						v.TurnToFaceEntity(Game.LocalPlayer.Character);

						Game.DisplayHelp("Speak to the caller by pressing " + Config.SpeakKey.ToString() + ". Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout.");
						break;

					case TroubleTypeEnum.AssaultinProgress:
						Game.DisplayHelp("Investigate the area. Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout.");
						Game.DisplaySubtitle("~y~Victim: ~w~OFFICER!! HELP ME!! HE'S GOT A KNIFE!!!", 6000);

						Victim v2 = (Victim)GetPed("Victim1");
						Suspect s = (Suspect)GetPed("Suspect1");
						s.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_KNIFE"), 56, true);

						try {
                            Stealth.Common.Natives.Peds.ReactAndFleePed(v2, s);
						} catch (Exception ex) {
							Logger.LogTrivialDebug("Error fleeing from ped -- " + ex.Message);
						}

						s.AttackPed(v2);

						v2.CreateBlip();
						s.CreateBlip();
						break;

					default:
						Game.DisplayHelp("Investigate the area. Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout.");
						searchArea = new Blip(SpawnPoint, 50);
						if (searchArea != null && searchArea.Exists())
							searchArea.Color = Color.FromArgb(100, Color.Yellow);
						break;
				}
			} catch (Exception ex) {
				Radio.CallIsCode4(this.ScriptInfo.Name);
				End();
				Logger.LogVerbose("Unknown Trouble callout crashed -- " + ex.Message);
				Logger.LogVerbose(ex.ToString());
			}
		}

		bool ProcessLionCall()
		{
			bool endCall = true;
			Suspect lion = (Suspect)GetPed("Lion");

			if (lion != null) {
				if (lion.Exists() == true) {
					if (lion.IsDead == false) {
						endCall = false;

						switch (LionSearchState) {
							case LionSearchStateEnum.Null:
								endCall = false;
								break;
							case LionSearchStateEnum.NotYetLocated:
								endCall = false;
								ProcessLion();
								LionAttack();
								break;
							case LionSearchStateEnum.LionLocated:
								endCall = false;
								LionAttack();
								break;
							case LionSearchStateEnum.LionEscaped:
								Game.DisplayNotification("The mountain lion has escaped.");
								DeleteLion();
								endCall = true;
								break;
						}
					} else {
						if (lion != null) {
							if (lion.Exists()) {
								lion.DeleteBlip();
							}
						}
						LionSearchState = LionSearchStateEnum.LionIsDead;
						endCall = true;
						Radio.CallIsCode4(this.ScriptInfo.Name);
					}
				} else {
				}
			}

			return endCall;
		}

		void ProcessLion()
		{
			Suspect lion = (Suspect)GetPed("Lion");

			if (lion != null) {
				if (lion.Exists() == true && LionSearchState == LionSearchStateEnum.NotYetLocated) {
					if (Game.LocalPlayer.Character.Position.DistanceTo(lion.Position) < 60) {
						LionSearchState = LionSearchStateEnum.LionLocated;
					} else {
						if (lion.Position.DistanceTo(lion.OriginalSpawnPoint) > 150) {
							LionSearchState = LionSearchStateEnum.LionEscaped;
						}
					}
				} else {
					LionSearchState = LionSearchStateEnum.LionEscaped;
				}
			}
		}

		void LionAttack()
		{
			try {
				GameFiber.StartNew(() =>
				{
					Suspect lion = (Suspect)GetPed("Lion");

					if (lionPrey != null) {
						if (lionPrey == Game.LocalPlayer.Character) {
							if (Game.LocalPlayer.Character.IsInAnyVehicle(true) == true) {
								lionPrey = null;
							}
						}
					}

					if (lion != null) {
						if (lionPrey == null) {
							if (lion.Exists()) {
								if (lion.IsDead == false) {
									if (Game.LocalPlayer.Character.IsInAnyVehicle(true) == false && lion.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 25) {
										//Player is on foot, & close to lion
										try {
											lionPrey = Game.LocalPlayer.Character;
                                            Stealth.Common.Natives.Peds.AttackPed(lion, Game.LocalPlayer.Character);
										} catch (Exception ex) {
											Logger.LogVerboseDebug("Error making lion attack player -- " + ex.Message);
										}
									} else {
										List<Ped> peds = lion.GetNearbyPeds(10).ToList();

										if (peds.Count > 0) {
											lionPrey = peds[Common.gRandom.Next(peds.Count)];

											try {
                                                Stealth.Common.Natives.Peds.AttackPed(lion, lionPrey);
											} catch (Exception ex) {
												Logger.LogVerboseDebug("Error attacking lion prey -- " + ex.Message);
											}
										}
									}
								}
							}
						}
					}
				});
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error finding lion prey -- " + ex.Message);
			}
		}

		void ReportMountainLion()
		{
			bool success = false;
			Suspect lion = (Suspect)GetPed("Lion");

			if (lion != null) {
				if (lion.Exists() == true) {
					success = true;
				}
			}

			if (success == true) {
				Game.DisplaySubtitle("Civilian: POLICE!! HELP!! There's a mountain lion on the loose!!", 10000);
				//Game.DisplayNotification("~g~OBJECTIVE: ~w~Find the mountain lion and deal with it appropriately.")
				Radio.DispatchMessage("Civilian called 911 to report mountain lion sighting", true);
				LionSearchState = LionSearchStateEnum.NotYetLocated;
				CreateLionBlip();
			} else {
				TroubleType = TroubleTypeEnum.None;
			}
		}

		void CreateLionBlip()
		{
			Suspect lion = (Suspect)GetPed("Lion");

			if (lion != null) {
				if (lion.Exists() == true) {
					lion.CreateBlip(Color.Purple);
				}
			}
		}

		void DeleteLion()
		{
			Suspect lion = (Suspect)GetPed("Lion");

			if (lion != null) {
				if (lion.Exists() == true) {
					lion.Delete();
					Peds.Remove(lion);
				}
			}
		}

		public override void End()
		{
			lionPrey = null;

			if (searchArea != null) {
				if (searchArea.Exists()) {
					searchArea.Delete();
				}
			}

			if (TroubleType == TroubleTypeEnum.DeadBody) {
				Cop d = (Cop)GetPed("Detective");
				Victim v = (Victim)GetPed("Victim1");
				Vehicles.Vehicle dv = GetVehicle("DetectiveUnit");

				if (dv != null & dv.Exists()) {
					dv.IsSirenOn = false;
					dv.IsPersistent = false;
					dv.Dismiss();
				}

				if (d != null & d.Exists()) {
					d.IsPersistent = false;
					d.Dismiss();
				}

				if (v != null & v.Exists()) {
					v.IsPersistent = false;
					v.Dismiss();
				}
			}

			base.End();
		}

		internal TroubleTypeEnum TroubleType { get; set; }
		internal LionSearchStateEnum LionSearchState { get; set; }

		internal enum TroubleTypeEnum
		{
			None = 0,
			MountainLion = 1,
			FalseCall = 2,
			AssaultinProgress = 3,
			DeadBody = 4
		}

		internal enum LionSearchStateEnum
		{
			Null = 0,
			NotYetLocated = 1,
			LionLocated = 2,
			LionEscaped = 3,
			LionIsDead = 4
		}

		public override bool RequiresSafePedPoint {
			get { return true; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return true; }
		}

		private EDetectiveState DetectiveState { get; set; }
		private enum EDetectiveState
		{
			Null,
			Created,
			Dispatched,
			Responding,
			AtScene,
			WalkingToPlayer,
			SpeakingToPlayer,
			WalkingToVictim,
			AboutToinspectVictim,
			KneelingToinspect,
			inspectingVictim,
			Done
		}

	}

}