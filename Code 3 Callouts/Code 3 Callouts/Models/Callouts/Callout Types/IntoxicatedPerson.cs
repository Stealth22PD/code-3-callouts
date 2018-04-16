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
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Intoxicated Person", CalloutProbability.Medium)]
    internal class IntoxicatedPerson : CalloutBase
	{

		string[] SuspectModels = {
			"A_M_M_Hillbilly_01",
			"A_M_M_Hillbilly_02",
			"A_M_O_GenStreet_01",
			"A_M_Y_Hippy_01",
			"A_M_Y_MethHead_01",
			"A_M_Y_BusiCas_01",
			"A_M_Y_Downtown_01",
			"A_M_Y_EastSA_01",
			"A_M_Y_GenStreet_02",
			"A_M_Y_SouCent_01",
			"A_M_Y_StWhi_01",
			"A_M_Y_StBla_01",
			"A_M_Y_Downtown_01",
			"A_M_Y_BevHills_01",
			"G_M_Y_MexGang_01",
			"G_M_Y_MexGoon_01",
			"G_M_Y_StrPunk_01"
		};
		LHandle pursuit;
		//bool pursuitinitiated = false;
		bool officerRespondedCode3 = false;

		ECode3Action mCode3Action = ECode3Action.None;
		private enum ECode3Action
		{
			None,
			Flee,
			Deleted
		}

        public IntoxicatedPerson() : base("Intoxicated Person", CallResponseType.Code_2)
        {
			RadioCode = 390;
			CrimeEnums = new List<DISPATCH.CRIMES>{
				DISPATCH.CRIMES.CODE_390,
				DISPATCH.CRIMES.POSSIBLE_390,
				DISPATCH.CRIMES.PUBLIC_INTOX
			};

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "Caller reports a male who appears to be intoxicated, and harassing those around him.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "No further details available at this time.";

			Objective = "Deal with the intoxicated ~y~subject.";
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			bool baseReturn = base.OnBeforeCalloutDisplayed();

			if (baseReturn == false) {
				return false;
			}

			Suspect s = new Suspect("Suspect1", SuspectModels[Common.gRandom.Next(SuspectModels.Length)], SpawnPoint, 0, false);
			s.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_KNIFE"), 56, false);
			s.SetIsDrunk(true);

			try {
				AnimationSet animSet = new AnimationSet("move_m@drunk@verydrunk");
				animSet.LoadAndWait();
				s.MovementAnimationSet = animSet;
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error animating ped -- " + ex.Message);
			}

			s.Tasks.Wander();

			Peds.Add(s);

			if (PerformPedChecks()) {
				return baseReturn;
			} else {
				return false;
			}
		}

		public override void OnArrivalAtScene()
		{
			base.OnArrivalAtScene();

			if (officerRespondedCode3 == true) {
				if (mCode3Action == ECode3Action.Deleted) {
					Game.DisplayNotification("The suspect fled the area.");
					Game.DisplayHelp("The suspect heard your siren and fled.");
					Radio.SergeantMessage("Exercise proper siren use on Code 2 calls, over");
					Radio.CallIsCode4(this.ScriptInfo.Name);

				} else if (mCode3Action == ECode3Action.Flee) {
					Game.DisplayHelp("The suspect heard your siren and is fleeing!");
					Radio.SergeantMessage("Exercise proper siren use on Code 2 calls, over");
				}
				End();
			} else {
				Game.DisplayHelp("Deal with the intoxicated person. Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout.", 5000);

				Suspect s = (Suspect)GetPed("Suspect1");

				if (s != null) {
					if (s.Exists()) {
						if (Common.IsComputerPlusRunning()) {
							AddPedToCallout(s);
						}

						s.Tasks.Clear();
						s.CreateBlip();
						s.TurnToFaceEntity(Game.LocalPlayer.Character);

						int drunkFactor = Common.gRandom.Next(1, 101);
						int reactionFactor = Common.gRandom.Next(3);

						if (drunkFactor > 50) {
							if (reactionFactor == 0) {
								//Stand around & be drunk
								try {
									s.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@drunk@idle_a"), "idle_a", 1f, AnimationFlags.RagdollOnCollision);
								} catch (Exception ex) {
									Logger.LogVerboseDebug("Error playing drunk anim -- " + ex.Message);
								}
							} else {
								//Attack a nearby ped
								GameFiber.StartNew(() =>
								{
									try {
										Ped ped = s.GetNearbyPeds(1).FirstOrDefault();

										if (ped != null && ped.Exists()) {
                                            Stealth.Common.Natives.Peds.AttackPed(s, ped);
										}
									} catch (Exception ex) {
										Logger.LogVerboseDebug("Error attacking ped -- " + ex.Message);
									}
								});
							}

						} else {
							if (reactionFactor == 0) {
								//Attack the player
								GameFiber.StartNew(() =>
								{
									GameFiber.Sleep(3000);
									try {
                                        Stealth.Common.Natives.Peds.AttackPed(s, Game.LocalPlayer.Character);
									} catch (Exception ex) {
										Logger.LogVerboseDebug("Error attacking player -- " + ex.Message);
									}
								});
							} else if (reactionFactor == 1) {
								//Flee

								GameFiber.StartNew(() =>
								{
									GameFiber.Sleep(3000);

									//pursuitinitiated = true;
									pursuit = Common.CreatePursuit();
									s.AddToPursuit(pursuit);
									Functions.SetPursuitIsActiveForPlayer(pursuit, true);
								});

							} else if (reactionFactor == 2) {
								//Steal police vehicle
								DateTime officerArrived = DateTime.Now;
								GameFiber.StartNew(() =>
								{
									GameFiber.Sleep(3000);

									while (true) {
										GameFiber.Yield();

										if (Game.LocalPlayer.Character.IsOnFoot && Game.LocalPlayer.Character.DistanceTo(s.Position) < 10) {
											break; // TODO: might not be correct. Was : Exit While
										}
									}

									Vehicle policeVehicle = Game.LocalPlayer.Character.LastVehicle;

									if (s.Exists() && policeVehicle.Exists()) {
										float tgtHeading = 0;
										if (policeVehicle.Exists())
											tgtHeading = Common.GetHeadingToPoint(policeVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f), policeVehicle.Position);

										if (s.Exists() && policeVehicle.Exists())
											s.Tasks.FollowNavigationMeshToPosition(policeVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f), tgtHeading, 2.2f, 2f).WaitForCompletion();

										if (s.Exists() && policeVehicle.Exists())
											s.Tasks.EnterVehicle(policeVehicle, -1).WaitForCompletion();

										if (s.Exists() && policeVehicle.Exists())
											s.Tasks.DriveToPosition(policeVehicle, World.GetNextPositionOnStreet(s.Position.Around(2000)), 25, VehicleDrivingFlags.Emergency, 50);
										if (policeVehicle.Exists())
											policeVehicle.IsSirenOn = true;
										if (policeVehicle.Exists())
											policeVehicle.IsSirenSilent = false;

										if (s.Exists()) {
											//pursuitinitiated = true;
											pursuit = Common.CreatePursuit(true, true, true);
											s.AddToPursuit(pursuit);

											GameFiber.StartNew(() =>
											{
												GameFiber.Sleep(3000);
												Functions.SetPursuitDisableAI(pursuit, false);
											});
										}
									} else {
										//Cant find vehicle, so just stand there
										try {
											s.Tasks.PlayAnimation(new AnimationDictionary("amb@world_human_bum_standing@drunk@idle_a"), "idle_a", 1f, AnimationFlags.RagdollOnCollision);
										} catch (Exception ex) {
											Logger.LogVerboseDebug("Error playing drunk anim -- " + ex.Message);
										}
									}
								});

							}
						}
					}
				}
			}
		}

		public override void Process()
		{
			base.Process();

			if (mCode3Action == ECode3Action.Deleted) {
				return;
			}

			Suspect s = (Suspect)GetPed("Suspect1");
			if (s != null && s.Exists()) {
				if (Game.LocalPlayer.Character.IsDead) {
					return;
				}

				if (CalloutState == CalloutState.UnitResponding) {
					if (officerRespondedCode3 == false) {
						Code3Check(ref s);
					}
				} else if (CalloutState == CalloutState.AtScene) {
					if (s.IsDead) {
						s.DeleteBlip();
						Radio.CallIsCode4(this.ScriptInfo.Name);
						End();
					} else {
						if (s.IsArrested()) {
							s.DeleteBlip();
							Radio.CallIsCode4(this.ScriptInfo.Name, true);
							End();
						} else {
							if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
								Radio.CallIsCode4(this.ScriptInfo.Name);
								End();
								//if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
        //                        }
                            }
						}
					}
				}
			}
		}

		private void Code3Check(ref Suspect s)
		{
			if (Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 150) {
				if (Game.LocalPlayer.Character.IsInAnyVehicle(false) == true) {
					if (Game.LocalPlayer.Character.CurrentVehicle != null) {
						if (Game.LocalPlayer.Character.CurrentVehicle.Exists()) {
							if (Game.LocalPlayer.Character.CurrentVehicle.HasSiren) {
								if (Game.LocalPlayer.Character.CurrentVehicle.IsSirenOn) {
									if (Game.LocalPlayer.Character.CurrentVehicle.IsSirenSilent == false) {
										officerRespondedCode3 = true;

										if (Game.LocalPlayer.Character.Position.DistanceTo(s.Position) < 75) {
											mCode3Action = ECode3Action.Flee;
											//pursuitinitiated = true;
											pursuit = Common.CreatePursuit();
											s.AddToPursuit(pursuit);
											Functions.SetPursuitIsActiveForPlayer(pursuit, true);
										} else {
											mCode3Action = ECode3Action.Deleted;
											s.DeleteBlip();
											s.Delete();
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public override void End()
		{
			base.End();
		}

		public override bool RequiresSafePedPoint {
			get { return true; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return true; }
		}

	}

}