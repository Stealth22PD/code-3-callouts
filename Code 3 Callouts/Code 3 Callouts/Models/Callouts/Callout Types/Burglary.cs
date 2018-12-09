using LSPD_First_Response.Engine;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using Stealth.Common.Extensions;
using static Stealth.Common.Models.QuestionWindow;
using static Stealth.Common.Scripting.Peds;
using Stealth.Plugins.Code3Callouts.Models.Interiors;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using System.Collections.Generic;
using System.Windows.Forms;
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System;
using System.Drawing;
using System.Linq;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Burglary In Progress", CalloutProbability.Medium)]
    internal class Burglary : CalloutBase
	{

		Suspect pSuspect = null;
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
		Victim pVictim = null;
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
		private bool mDoorHelpDisplayed = false;
		private bool mBackupDoorHelpDisplayed = false;
		private bool mIsPlayerindoors = false;
		private bool mSuspectReacted = false;
		private bool mDoorsEnabled = false;
		private bool mIsBackupOnScene = false;
		private bool mIsBackupOnFoot = false;
		private bool mIsScenarioOver = false;

		private int mAIUnitNumber = Common.gRandom.Next(27, 50);
		private LHandle mPursuit = null;
		private bool mPursuitinitiated = false;

		//private bool mAimWeapons = true;
		Cop pCop1 = null;
		Cop pCop2 = null;
		Vehicles.Vehicle vPolice1 = null;

		Vector3 mBackupDriveTo;
		bool mPlayerVehDataSaved = false;
		string mPlayerVehModel;
		Vector3 mPlayerVehPos;

		float mPlayerVehHdg;
        public Burglary() : base("Burglary in Progress", CallResponseType.Code_2)
        {
			
			RadioCode = 459;
			CrimeEnums = new List<DISPATCH.CRIMES> {
				DISPATCH.CRIMES.CODE_459,
				DISPATCH.CRIMES.POSSIBLE_459
			};

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "Female RP reports that she came home to find her front door open, and heard someone moving around inside.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "The front door is damaged, and appears to have been kicked in. RP is on scene awaiting police arrival; approach with caution.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += string.Format("UPDATE: {0} is responding, and will meet RP outside.", Common.gUnitNumber);
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += string.Format("UPDATE: 1-ADAM-{0} will back up {1}; entry not to be made until 1-ADAM-{0}'s arrival.", mAIUnitNumber, Common.gUnitNumber);

			Objective = "Apprehend the ~r~suspect!~n~Respond quickly, but quietly!";
		}


		private Residence mHouse = null;
		public override bool OnBeforeCalloutDisplayed()
		{
			bool baseReturn = base.OnBeforeCalloutDisplayed();

			if (baseReturn == false) {
				return false;
			}

			if (mHouse == null || SpawnPoint == Vector3.Zero) {
				Logger.LogVerboseDebug("Failed to find house for burglary; callout aborted");
				return false;
			}

			Logger.LogVerboseDebug("Spawn point found");

			List<Ped> nearbyPeds = GetPedsNearPosition(mHouse.EntryPoint, 30f, GetEntitiesFlags.ConsiderHumanPeds);
			foreach (Ped p in nearbyPeds) {
				if (p.Exists()) {
					p.Delete();
				}
			}

			SpawnPoint suspectSpawn = mHouse.Interior.HidingPlaces[Common.gRandom.Next(0, mHouse.Interior.HidingPlaces.Count)];
			pSuspect = new Suspect("Suspect1", SuspectModels[Common.gRandom.Next(SuspectModels.Length)], suspectSpawn.Position, suspectSpawn.Heading, false);
			pSuspect.DisplayName = "Suspect";
			pSuspect.BlockPermanentEvents = true;
			pSuspect.MakePersistent();
			Peds.Add(pSuspect);
			Logger.LogVerboseDebug("Suspect created");

			RandomizeSuspectStory();
			Logger.LogVerboseDebug("Suspect story randomized");

			int weaponFactor = Common.gRandom.Next(6);

			if (weaponFactor == 5) {
				pSuspect.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 32, true);
			} else if (weaponFactor == 4) {
				pSuspect.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_KNIFE"), 32, true);
			}

			pVictim = new Victim("Victim1", VictimModels[Common.gRandom.Next(VictimModels.Length)], mHouse.EntryPoint, 0);
			pVictim.DisplayName = "Victim";
			pVictim.BlockPermanentEvents = true;
			pVictim.MakePersistent();

			{
				pVictim.SpeechLines.Add("Oh Officer, thank god you're here!!");
				pVictim.SpeechLines.Add("I came home a few minutes ago, and I went to unlock my door...");
				pVictim.SpeechLines.Add("And I found it was already open!! and the lock was damaged!");
				pVictim.SpeechLines.Add("I didn't know what had happened at first, then I peeked through the door...");
				pVictim.SpeechLines.Add("And I heard someone moving around inside!");
				pVictim.SpeechLines.Add("I was SO scared, I just ran out here and called 911!");
				pVictim.SpeechLines.Add("I live alone, Officer. I think someone has broken in!");
				pVictim.SpeechLines.Add("Please, I need you to go inside for me! I'm so scared right now!");
			}

			Peds.Add(pVictim);

			if (Common.IsComputerPlusRunning()) {
				AddPedToCallout(pVictim);
			}

			return true;
		}

		public override bool OnCalloutAccepted()
		{
			GameFiber.StartNew(() =>
			{
				GameFiber.Sleep(10000);

				Game.DisplayNotification(string.Format("~b~1-ADAM-{0}~w~: ~w~Dispatch, 1-ADAM-{0} will be backing up ~b~{1}.", mAIUnitNumber, Common.gUnitNumber));
				Radio.AIOfficerResponding();
			});

			return base.OnCalloutAccepted();
		}

		public override void OnArrivalAtScene()
		{
			Logger.LogVerboseDebug("Onarrival at burglary");

			base.OnArrivalAtScene();

			if (Game.LocalPlayer.Character.CurrentVehicle.Exists()) {
				Game.LocalPlayer.Character.CurrentVehicle.MakePersistent();
			} else {
				if (Game.LocalPlayer.Character.LastVehicle.Exists()) {
					Game.LocalPlayer.Character.LastVehicle.MakePersistent();
				}
			}

			if (pVictim.Exists()) {
				pVictim.CreateBlip();
				pVictim.TurnToFaceEntity(Game.LocalPlayer.Character);

				GameFiber.StartNew(() =>
				{
					while (Game.LocalPlayer.Character.IsInAnyVehicle(false)) {
						GameFiber.Yield();
					}

					if (pVictim.Exists()) {
						float heading = Common.GetHeadingToPoint(pVictim.Position, Game.LocalPlayer.Character.Position);
						pVictim.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.GetOffsetPositionFront(5), heading, 2.5f).WaitForCompletion();
						pVictim.TurnToFaceEntity(Game.LocalPlayer.Character);
					}
				});
			}

			Blip mBlip = new Blip(mHouse.EntryPoint);
			mBlip.Scale = 0.75f;
			mBlip.Color = Color.Yellow;
			Markers.Add(mBlip);

			Radio.SergeantMessage("~r~DO NOT ~w~make entry until backup arrives, over");
			SpawnBackup();
		}

		private void SpawnBackup()
		{
			if (Common.IsPlayerinLosSantos()) {
				string[] lspdModels = {
					"POLICE",
					"POLICE2",
					"POLICE3",
					"POLICE4"
				};
				vPolice1 = new Vehicles.Vehicle(lspdModels[Common.gRandom.Next(lspdModels.Length)], World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250)));
			} else {
				string[] sheriffModels = {
					"SHERIFF",
					"SHERIFF",
					"SHERIFF2",
					"POLICE4"
				};
				vPolice1 = new Vehicles.Vehicle(sheriffModels[Common.gRandom.Next(sheriffModels.Length)], World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250)));
			}
			vPolice1.IsSirenOn = true;
			vPolice1.IsSirenSilent = true;
			vPolice1.MakePersistent();

			pCop1 = Models.Peds.Cop.Create("Cop1", vPolice1.GetOffsetPosition(Vector3.RelativeLeft * 1.5f), 180, Convert.ToBoolean(Common.gRandom.Next(2)));
			pCop1.CreateBlip();
			pCop2 = Models.Peds.Cop.Create("Cop2", vPolice1.GetOffsetPositionRight(1.5f), 180, Convert.ToBoolean(Common.gRandom.Next(2)));
			pCop2.CreateBlip();
			Peds.Add(pCop1);
			Peds.Add(pCop2);
			Functions.SetPedAsCop(pCop1);
			Functions.SetPedAsCop(pCop2);

			pCop1.WarpIntoVehicle(vPolice1, -1);
			pCop2.WarpIntoVehicle(vPolice1, (int)vPolice1.GetFreePassengerSeatIndex());

			GameFiber.StartNew(() =>
			{
				GameFiber.Sleep(3000);

				mBackupDriveTo = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(5));
				{
					if (Game.LocalPlayer.Character.CurrentVehicle.Exists()) {
						mBackupDriveTo = Game.LocalPlayer.Character.CurrentVehicle.GetOffsetPosition(Vector3.RelativeBack * 5);
					} else {
						if (Game.LocalPlayer.Character.LastVehicle.Exists()) {
							mBackupDriveTo = Game.LocalPlayer.Character.LastVehicle.GetOffsetPosition(Vector3.RelativeBack * 5);
						}
					}
				}

				pCop1.Tasks.DriveToPosition(mBackupDriveTo, 15, VehicleDrivingFlags.Emergency, 5).WaitForCompletion();
				mIsBackupOnScene = true;
			});

			GameFiber.StartNew(() =>
			{
				GameFiber.Sleep(30000);
				if (mIsBackupOnScene == false)
					Game.DisplayHelp("Backup unit stuck or taking too long? ~n~Press ~b~0 ~w~to speed it up.", 10000);
			});
		}

		private void TeleportBackup()
		{
			if (mIsBackupOnScene == false) {
				if (vPolice1.Exists()) {
					vPolice1.Position = mBackupDriveTo;
				}
			}
		}

		private void RandomizeSuspectStory()
		{
			if (pSuspect.Exists()) {
				List<QAItem> mSuspectQAItems = new List<QAItem>();
				mSuspectQAItems.Add(new QAItem("What were you doing in the house?"));
				mSuspectQAItems.Add(new QAItem("Why did you break in?"));
				mSuspectQAItems.Add(new QAItem("What were you trying to steal?"));
				mSuspectQAItems.Add(new QAItem("Have you ever done this before?"));

				int mStoryFactor = Common.gRandom.Next(5);


				{
					switch (mStoryFactor) {
						case 0:
							pSuspect.SpeechLines.Add("Officer, you're making a mistake, this is MY house!");

							
							{
								mSuspectQAItems[0].Answer = "I live here! This is my house!";
								mSuspectQAItems[1].Answer = "My girlfriend left after I did, and I..uhh..forgot my keys!";
								mSuspectQAItems[2].Answer = "Er...nothing!";
								mSuspectQAItems[3].Answer = "No, sir!";

								mSuspectQAItems.Add(new QAItem("What's your girlfriend's name?", "Uhhh...Kristen. Wait, no...Kirsten. No, Melissa! That's it!"));
							}


							break;
						case 1:
							pSuspect.SpeechLines.Add("Heyyyy man, you got it all wrong! This is my buddy's place.");

							
							{
								mSuspectQAItems[0].Answer = "I was checking to see if my buddy was home!";
								mSuspectQAItems[1].Answer = "I didn't! The door was unlocked.";
								mSuspectQAItems[2].Answer = "Nothing, man!";
								mSuspectQAItems[3].Answer = "Yeah! He lets me in through the window all the time!";

								string mFakeName = "";
								switch (Common.gRandom.Next(0, 5)) {
									case 0:
										mFakeName = "Uhhh...Jeff Favignano!!";
										break;
									case 1:
										mFakeName = "His name is...Zach something. Zach...House? Houseknecht?";
										break;
									case 2:
										mFakeName = "I want my lawyer.";
										break;
									case 3:
										mFakeName = "Sarah...yeah, that's my sister. Its her house.";
										break;
									default:
										mFakeName = "Its uh...I, uh...forget.";
										break;
								}

								mSuspectQAItems.Add(new QAItem("What's your buddy's name?", mFakeName));
							}


							break;
						case 2:
							pSuspect.SpeechLines.Add("Come on...you know their insurance will cover it! What's the harm?");

							
							{
								mSuspectQAItems[0].Answer = "What the hell do you think I was doing?";
								mSuspectQAItems[1].Answer = "Have you seen that fine piece of ass that lives here? Damn!";
								mSuspectQAItems[2].Answer = "Dentures. I fence false teeth for a living.";
								mSuspectQAItems[3].Answer = "Have I done this before? Do I look like a rookie to you?";
							}


							break;
						case 3:
							pSuspect.SpeechLines.Add("I'm sorry, man! Its just...I gotta have it!! I need the money!!");

							
							{
								mSuspectQAItems[0].Answer = "I just...I need it!!";
								mSuspectQAItems[1].Answer = "I need to get some Coke, man. That don't come cheap!";
								mSuspectQAItems[2].Answer = "Anything I could find...cash, jewelry, anything.";
								mSuspectQAItems[3].Answer = "Do I really gotta answer that?";
							}


							break;
						// Add one more
						default:
							pSuspect.SpeechLines.Add("Go to hell, pig! I ain't saying nothin to you!");

							
							{
								mSuspectQAItems[0].Answer = "I want my lawyer!";
								mSuspectQAItems[1].Answer = "LAWYER!";
								mSuspectQAItems[2].Answer = "No hablo ingles!";
								mSuspectQAItems[3].Answer = "Hey! Take a hint! LAWYER!";
							}

							break;
					}
				}

				pSuspect.QAItems = mSuspectQAItems;
			} else {
			}
		}

		public override void Process()
		{
			base.Process();

			ProcessTeleportKey();
			ProcessBackupArrival();
			ProcessHouseEntryExit();
			ProcessSuspectReaction();
			ProcessEndOfSituation();

			if (Common.IsKeyDown(Config.SpeakKey)) {
				if (pVictim.Exists() && pVictim.IsAlive && Game.LocalPlayer.Character.DistanceTo(pVictim.Position) <= 3) {
					pVictim.Speak();
				} else {
					if (pSuspect.Exists() && pSuspect.IsAlive && Game.LocalPlayer.Character.DistanceTo(pSuspect.Position) <= 3) {
						pSuspect.Speak();
					}
				}
			}

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
				End();
				//if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
				//}
			}
		}

		private void ProcessTeleportKey()
		{
			if (mIsBackupOnScene == true | mIsScenarioOver == true) {
				return;
			}

			if (mIsBackupOnScene == false) {
				if (Common.IsKeyDown(Keys.D0)) {
					TeleportBackup();
				}
			}
		}

		private void ProcessBackupArrival()
		{
			if (mIsBackupOnScene == false | mIsBackupOnFoot == true) {
				return;
			}

			if (mIsBackupOnScene == true && mIsBackupOnFoot == false) {
				mIsBackupOnFoot = true;
				if (vPolice1.Exists())
					vPolice1.IsSirenOn = false;

				GameFiber.StartNew(() =>
				{
					if (pCop1.Exists() && pCop1.CurrentVehicle.Exists())
						pCop1.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
					pCop1.Tasks.Clear();
					pCop1.KeepTasks = true;
					pCop1.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 64, true);
					pCop1.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeBack * 3), Common.GetHeadingToPoint(pCop1.Position, Game.LocalPlayer.Character.Position), 2.5f);
					mDoorsEnabled = true;
				});

				GameFiber.StartNew(() =>
				{
					if (pCop2.Exists() && pCop2.CurrentVehicle.Exists())
						pCop2.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
					pCop2.Tasks.Clear();
					pCop2.KeepTasks = true;
					pCop2.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 64, true);
					pCop2.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeBack * 3), Common.GetHeadingToPoint(pCop1.Position, Game.LocalPlayer.Character.Position), 2.5f);
					mDoorsEnabled = true;
				});

				GameFiber.StartNew(() =>
				{
					{
						while (true) {
							GameFiber.Yield();

							if (pCop1.Exists() == false || pCop1.DistanceTo(Game.LocalPlayer.Character.Position) < 5) {
								break; // TODO: might not be correct. Was : Exit While
							}
						}

						if (pCop1.Exists()) {
							pCop1.Tasks.Clear();
							pCop1.KeepTasks = true;
							pCop1.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, Vector3.RelativeBack * 6f);
						}
					}
				});

				GameFiber.StartNew(() =>
				{
					{
						while (true) {
							GameFiber.Yield();

							if (pCop2.Exists() == false || pCop2.DistanceTo(Game.LocalPlayer.Character.Position) < 5) {
								break; // TODO: might not be correct. Was : Exit While
							}
						}

						if (pCop2.Exists()) {
							pCop2.Tasks.Clear();
							pCop2.KeepTasks = true;
							pCop2.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, Vector3.RelativeBack * 6f);
						}
					}
				});
			}
		}

		private void ProcessHouseEntryExit()
		{
			if (mDoorsEnabled == true) {
				if (mIsPlayerindoors == false) {
					if (Game.LocalPlayer.Character.Position.DistanceTo(mHouse.EntryPoint) < 2f) {
						if (mDoorHelpDisplayed == false) {
							mDoorHelpDisplayed = true;
							Game.DisplayHelp("Press ~b~CTRL + E~w~ to enter.", 5000);

							GameFiber.StartNew(() =>
							{
								GameFiber.Sleep(10000);
								mDoorHelpDisplayed = false;
							});
						}

						if (Common.IsKeyDown(Keys.E, Keys.ControlKey))
                        {
							EnterHouse();
						}
					}
				} else {
					if (Game.LocalPlayer.Character.Position.DistanceTo(mHouse.Interior.InteriorSpawnPoint.Position) < 2f) {
						if (mDoorHelpDisplayed == false) {
							mDoorHelpDisplayed = true;
							Game.DisplayHelp("Press ~b~CTRL + E~w~ to exit.", 5000);

							GameFiber.StartNew(() =>
							{
								GameFiber.Sleep(10000);
								mDoorHelpDisplayed = false;
							});
						}

						if (Common.IsKeyDown(Keys.E, Keys.ControlKey))
                        {
							ExitHouse();
						}
					}
				}
			} else {
				if (mIsPlayerindoors == false && mIsBackupOnScene == false) {
					if (Game.LocalPlayer.Character.Position.DistanceTo(mHouse.EntryPoint) < 2f) {
						if (mBackupDoorHelpDisplayed == false) {
							mBackupDoorHelpDisplayed = true;
							Game.DisplayHelp("Wait for your ~b~backup unit ~w~before entering!", 5000);

							GameFiber.StartNew(() =>
							{
								GameFiber.Sleep(10000);
								mBackupDoorHelpDisplayed = false;
							});
						}
					}
				}
			}
		}

		private void MakeCopsAimWeapons()
		{
			if (pCop1.Exists()) {
                pCop1.Inventory.EquippedWeapon = pCop1.Inventory.Weapons.FirstOrDefault();
				pCop1.Tasks.PlayAnimation("combat@chg_positionpose_b", "aimb_calm_fwd", 1f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly | AnimationFlags.StayInEndFrame);
			}

			if (pCop2.Exists()) {
                pCop2.Inventory.EquippedWeapon = pCop2.Inventory.Weapons.FirstOrDefault();
				pCop2.Tasks.PlayAnimation("combat@chg_positionpose_b", "aimb_calm_fwd", 1f, AnimationFlags.SecondaryTask | AnimationFlags.UpperBodyOnly | AnimationFlags.StayInEndFrame);
			}
		}

		private void EnterHouse(bool pForcePedsinside = false)
		{
			if (Game.LocalPlayer.Character.LastVehicle.Exists()) {
				Game.LocalPlayer.Character.LastVehicle.MakePersistent();

				mPlayerVehModel = Game.LocalPlayer.Character.LastVehicle.Model.Name;
				mPlayerVehPos = Game.LocalPlayer.Character.LastVehicle.Position;
				mPlayerVehHdg = Game.LocalPlayer.Character.LastVehicle.Heading;
				mPlayerVehDataSaved = true;
			}

			Game.FadeScreenOut(1800, true);

			mHouse.Interior.LoadInterior();

			Game.LocalPlayer.Character.Position = mHouse.Interior.InteriorSpawnPoint.Position;
			Game.LocalPlayer.Character.Heading = mHouse.Interior.InteriorSpawnPoint.Heading;

			if (mIsScenarioOver == false || pForcePedsinside == true) {
				if (pCop1.Exists()) {
					pCop1.Position = mHouse.Interior.InteriorSpawnPoint.Position;
					pCop1.Heading = mHouse.Interior.InteriorSpawnPoint.Heading;
				}

				if (pCop2.Exists()) {
					pCop2.Position = mHouse.Interior.InteriorSpawnPoint.Position;
					pCop2.Heading = mHouse.Interior.InteriorSpawnPoint.Heading;
				}
			}

			mIsPlayerindoors = true;
			Game.FadeScreenIn(1800, true);

			if (mIsScenarioOver == false) {
				MakeCopsAimWeapons();
			}
		}

		private void ExitHouse(bool pForcePedsOutside = false)
		{
			Game.FadeScreenOut(1800, true);

			Vector3 mOutsideSpawnpoint = mHouse.EntryPoint;

			mIsPlayerindoors = false;

			if (mIsScenarioOver == false | pForcePedsOutside == true) {
				if (Game.LocalPlayer.Character.LastVehicle.Exists()) {
					mOutsideSpawnpoint = Game.LocalPlayer.Character.LastVehicle.GetOffsetPositionFront(4f);
				} else {
					bool mUseAIVeh = true;

					if (mPlayerVehDataSaved == true) {
						string[] mValidModels = {
							"POLICE",
							"POLICE2",
							"POLICE3",
							"POLICE4",
							"SHERIFF",
							"SHERIFF2",
							"FBI",
							"FBI2"
						};

						if (mValidModels.Contains(mPlayerVehModel.ToUpper())) {
							try {
								Vehicle vNewPlayerVeh = new Vehicle(mPlayerVehModel, mPlayerVehPos, mPlayerVehHdg);

								if (vNewPlayerVeh.Exists()) {
									vNewPlayerVeh.MakePersistent();
									mOutsideSpawnpoint = vNewPlayerVeh.GetOffsetPositionFront(4f);
									mUseAIVeh = false;
								}
							} catch (Exception ex) {
								Logger.LogTrivial("Error respawning player vehicle -- " + ex.Message);
							}
						}
					}

					if (mUseAIVeh == true || mPlayerVehDataSaved == false) {
						if (vPolice1.Exists()) {
							mOutsideSpawnpoint = vPolice1.GetOffsetPositionFront(4f);
						} else {
							mOutsideSpawnpoint = PedHelper.GetSafeCoordinatesForPed(World.GetNextPositionOnStreet(mHouse.EntryPoint));
							if (mOutsideSpawnpoint == Vector3.Zero)
								mOutsideSpawnpoint = World.GetNextPositionOnStreet(mHouse.EntryPoint);
						}
					}
				}

				if (mOutsideSpawnpoint.DistanceTo(mHouse.EntryPoint) > 200)
					mOutsideSpawnpoint = World.GetNextPositionOnStreet(mHouse.EntryPoint);

				if (pSuspect.Exists())
					pSuspect.Position = mOutsideSpawnpoint;
				if (pCop1.Exists())
					pCop1.Position = mOutsideSpawnpoint;
				if (pCop2.Exists())
					pCop2.Position = mOutsideSpawnpoint;
			}

			Game.LocalPlayer.Character.Position = mOutsideSpawnpoint;

			Game.FadeScreenIn(1800, true);
		}

		private void ProcessSuspectReaction()
		{
			if (mIsPlayerindoors == true && mSuspectReacted == false) {
				try {
					uint mRoomSuspect = NativeFunction.CallByHash<uint>(0x47c2a06d4f5f424buL, Common.GetNativeArgument(pSuspect));
					uint mRoomPlayer = NativeFunction.CallByHash<uint>(0x47c2a06d4f5f424buL, Common.GetNativeArgument(Game.LocalPlayer.Character));
					uint mRoomCop1 = 0;
					uint mRoomCop2 = 0;

                    if (pCop1.Exists())
                        //mRoomCop1 = NativeFunction.CallByHash<uint>(0x47c2a06d4f5f424buL, Common.GetNativeArgument(pCop1));
                        mRoomCop1 = NativeFunction.Natives.GetRoomKeyFromEntity(pCop1);
                    if (pCop2.Exists())
                        //mRoomCop2 = NativeFunction.CallByHash<uint>(0x47c2a06d4f5f424buL, Common.GetNativeArgument(pCop2));
                        mRoomCop2 = NativeFunction.Natives.GetRoomKeyFromEntity(pCop2);

					if (mRoomPlayer == mRoomSuspect) {
						MakeSuspectReact();
					} else {
						if (mRoomSuspect == mRoomCop1 | mRoomSuspect == mRoomCop2) {
							MakeSuspectReact();
						}
					}
				} catch {
					if (pSuspect.DistanceTo(Game.LocalPlayer.Character.Position) < 10) {
						MakeSuspectReact();
					}
				}
			}
		}

		private void MakeSuspectReact()
		{
			if (mSuspectReacted == true)
				return;

			mSuspectReacted = true;

			//If pSuspect.Exists() = True && pSuspect.HasAttachedBlip() = False Then pSuspect.CreateBlip()

			if (pCop1.Exists())
				pCop1.Tasks.Clear();
			if (pCop2.Exists())
				pCop2.Tasks.Clear();

			int reaxFactor = Common.gRandom.Next(3);

			if (reaxFactor <= 1) {
				//Flee
				if (mPursuitinitiated == false) {
					TriggerPursuit();
				}
			} else if (reaxFactor == 2) {
				//Attack
				if (mPursuitinitiated == false) {
					TriggerPursuit();
					if (pSuspect.Exists())
						pSuspect.AttackPed(Game.LocalPlayer.Character);
				}
			}
		}

		private void TriggerPursuit()
		{
			mPursuitinitiated = true;
			mPursuit = Common.CreatePursuit();
			if (pSuspect.Exists())
				Functions.AddPedToPursuit(mPursuit, pSuspect);
			if (pCop1.Exists())
				Functions.AddCopToPursuit(mPursuit, pCop1);
			if (pCop2.Exists())
				Functions.AddCopToPursuit(mPursuit, pCop2);
		}

		private void ProcessEndOfSituation()
		{

			if (mIsPlayerindoors == true && mIsScenarioOver == false) {
				if (pSuspect.Exists() && (pSuspect.IsArrested() || pSuspect.IsDead)) {
					mIsScenarioOver = true;
					//mAimWeapons = false;

					if (Common.IsComputerPlusRunning()) {
						AddPedToCallout(pSuspect);
					}

					if (pCop1.Exists()) {
						pCop1.Tasks.ClearImmediately();
						pCop1.Inventory.Weapons.Clear();
						pCop1.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 64, false);
					}

					if (pCop2.Exists()) {
						pCop2.Tasks.ClearImmediately();
						pCop2.Inventory.Weapons.Clear();
						pCop2.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 64, false);
					}

					GameFiber.StartNew(() =>
					{
						GameFiber.Sleep(5000);
						ExitHouse(true);
						GameFiber.Sleep(3000);

						if (pSuspect.Exists() & pSuspect.IsAlive) {
							Game.DisplayHelp("Ensure that you question the suspect using the interaction menu.", 5000);
						} else {
							Game.DisplayHelp("Check the suspect for ID using the interaction menu, and call the coroner.", 5000);
						}

						GameFiber.Sleep(5000);
						Game.DisplayHelp("Press ~b~" + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " ~w~to end this callout when the situation is over.", 8000);
					});

					GameFiber.StartNew(() =>
					{
						if (pVictim.Exists()) {
							pVictim.SpeechLines = new List<string>();

							{
								pVictim.SpeechLines.Add("Thank you SO much, Officer!");
								pVictim.SpeechLines.Add("I don't know what I would have done without you!");
								pVictim.SpeechLines.Add("I swear, I don't know where that man came from. I don't know him!!");
								pVictim.SpeechLines.Add("He's not going to come back, is he??");
							}
						}
					});
				}
			}
		}

		public override void End()
		{
			if (IsCalloutActive == false) {
				base.End();
				return;
			}

			DeleteBlips();

			Radio.CallIsCode4(this.ScriptInfo.Name);

			if (vPolice1.Exists() && pCop1.Exists() && pCop2.Exists()) {
				if (pCop2.DistanceTo(vPolice1.Position) > 100) {
					base.End();
					return;
				}
			} else {
				base.End();
				return;
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
			});

			base.End();
		}

		public override Vector3 CalculateSpawnpoint()
		{
			mHouse = InteriorDatabase.GetRandomHouse();

			if (mHouse != null) {
				return mHouse.EntryPoint;
			} else {
				return Vector3.Zero;
			}
		}

		public override bool IsFixedSpawnPoint {
			get { return true; }
		}

		public override bool RequiresSafePedPoint {
			get { return false; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return true; }
		}
	}

}