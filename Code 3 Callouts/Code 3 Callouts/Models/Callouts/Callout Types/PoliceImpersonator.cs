using LSPD_First_Response;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common;
using Stealth.Common.Extensions;
using Stealth.Common.Models;
using static Stealth.Common.Natives.Vehicles;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System.Windows.Forms;
using static Stealth.Common.Models.QuestionWindow;
using System;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Common;
using System.Drawing;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Police Impersonator", CalloutProbability.Medium)]
	internal class PoliceImpersonator : CalloutBase
	{

		bool suspectIdentified = false;
		Blip suspectSearchArea;
		float searchAreaRadius = 150f;
		Vector3 suspectLastSeen;
		DateTime lastLocationUpdate = DateTime.Now;

		EImpersonatorVehicle vehType = EImpersonatorVehicle.CivilianVehicle;
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

		string[] PedModels = {
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

		bool suspectVisual = false;
		LHandle pursuit;
		//bool pursuitinitiated = false;
		//bool officerRespondedCode3 = false;
		bool fullPlate = false;

		string licensePlate = "";
		bool suspectWillAttack = false;
		bool suspectAttacking = false;
		int suspectAttackChance = 30;
		//bool pulloverPossible = false;
		//PulloverState mPulloverState = PulloverState.Null;

		bool endTipDisplayed = false;

		bool suspectCounterOn = false;
		private enum PulloverState
		{
			Null,
			Following,
			LightsOrSirenOn,
			Stopping,
			Parked,
			Fleeing,
			LeftVehicle
		}

        public PoliceImpersonator() : base("Police Impersonator", CallResponseType.Code_3)
        {
			RadioCode = 538;
			CrimeEnums = new List<DISPATCH.CRIMES>() { DISPATCH.CRIMES.CIV_ASSISTANCE };

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "Caller was stopped by someone identifying themselves as a police officer.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "No police vehicles were in the area at the time; suspect possibly armed. Proceed with caution.";

			Objective = "Speak to the ~o~victim~w~. Ensure they're not hurt.~n~Apprehend the ~r~suspect!";
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			bool baseReturn = base.OnBeforeCalloutDisplayed();

			if (baseReturn == false) {
				return false;
			}

			Vector3 position = World.GetNextPositionOnStreet(SpawnPoint.Around(5));
			VehicleNode node = Stealth.Common.Natives.Vehicles.GetClosestVehicleNodeWithHeading(position);

			if (node.Position == Vector3.Zero) {
				node.Position = position;
				node.Heading = Common.gRandom.Next(360);
			}

			Vehicles.Vehicle v = new Vehicles.Vehicle(VehModels[Common.gRandom.Next(VehModels.Length)], position, node.Heading);
			Victim p = new Victim("Victim1", PedModels[Common.gRandom.Next(PedModels.Length)], v.GetOffsetPosition(Vector3.RelativeLeft * 2f), 0);
			p.DisplayName = "Victim";

			if (v != null && v.Exists()) {
				v.Name = "VictimCar1";
				v.MakePersistent();
				v.SetRandomColor();
				Vehicles.Add(v);

				p.MakePersistent();
				p.BlockPermanentEvents = true;
				p.WarpIntoVehicle(v, -1);
				Peds.Add(p);

				AddPedToCallout(p);
				AddVehicleToCallout(v);

				return true;
			} else {
				return false;
			}
		}

		public override bool OnCalloutAccepted()
		{
			bool baseReturn = base.OnCalloutAccepted();

			if (baseReturn == false) {
				return false;
			}

			Victim v = (Victim)GetPed("Victim1");

			if (v != null && v.Exists()) {
				string[] vehModels = {
					"STANIER",
					"BUFFALO",
					"GRANGER"
				};
				//Dim vehModels As String() = {"POLICE4", "FBI", "FBI2"}

				string vehModel = vehModels[Common.gRandom.Next(vehModels.Length)];
				string pedModel = "S_M_M_ChemSec_01";

				int witnessStory = Common.gRandom.Next(6);
				{
					v.SpeechLines.Add("Oh jeez, Officer, am I glad to see you!");

					RandomizeVictimStory(ref v, witnessStory);

					int attackFactor = Common.gRandom.Next(1, 101);
					if (attackFactor <= suspectAttackChance) {
						suspectWillAttack = true;
					}

					if (vehType == EImpersonatorVehicle.SecurityVehicle) {
						vehModel = "dilettante2";
						pedModel = "S_M_M_Security_01";
					}

					Vehicles.Vehicle suspectVehicle = new Vehicles.Vehicle(vehModel, World.GetNextPositionOnStreet(SpawnPoint.Around(250)), gRandom.Next(360));
					suspectVehicle.MakePersistent();
					suspectVehicle.Name = "SuspectVehicle";
					suspectVehicle.SetRandomColor();
					suspectVehicle.FillColorValues();
					Vehicles.Add(suspectVehicle);

					Suspect s = new Suspect("Suspect1", pedModel, World.GetNextPositionOnStreet(suspectVehicle.Position.Around(3)), 0, false);
					s.DisplayName = "Suspect";
					s.MakePersistent();
					s.WarpIntoVehicle(suspectVehicle, -1);
					s.Tasks.CruiseWithVehicle(suspectVehicle, 10, VehicleDrivingFlags.Normal);
					s.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 56, false);
					s.RelationshipGroup = new RelationshipGroup("CIVMALE");

                    LSPD_First_Response.Engine.Scripting.Entities.Persona p = Functions.GetPersonaForPed(s);
					Functions.SetVehicleOwnerName(suspectVehicle, p.FullName);

					s.QAItems = new List<QAItem>();

					int susStory = Common.gRandom.Next(4);
					switch (susStory) {
						case 0:
							s.SpeechLines.Add("I ain't got nothin to say to you!");

							s.QAItems.Add(new QAItem("Is this your vehicle?", "Yeah, its my car! So??"));
							s.QAItems.Add(new QAItem("What were you up to today?", "None of your business, pig!"));
							s.QAItems.Add(new QAItem("Did you pull over someone today?", "Yeah, your mom! Or was it your sister?"));
							s.QAItems.Add(new QAItem("where did you get the red and blue lights?", "Your wife bought em for me...a gift. For uh...my services."));
							s.QAItems.Add(new QAItem("Have you had anything to drink?", "Yeah...I had some wine with your wife last night."));
							break;
						case 1:
							s.SpeechLines.Add("Hey, come on now, Officer! It was just a prank. Just kiddin' around, ya know?");

							s.QAItems.Add(new QAItem("Is this your vehicle?", "No! I mean...yes!"));
							s.QAItems.Add(new QAItem("What were you up to today?", "Just playing a joke, man."));
							s.QAItems.Add(new QAItem("Did you pull over someone today?", "Yeah, but come on, I was just kidding!"));
							s.QAItems.Add(new QAItem("where did you get the red and blue lights?", "eBay, dude!"));
							s.QAItems.Add(new QAItem("Have you had anything to drink?", "Just a couple of beers..."));
							break;
						case 2:
							s.SpeechLines.Add("I'm not saying anything without my lawyer.");

							s.QAItems.Add(new QAItem("Is this your vehicle?", "Yes, it is, you have no right to hassle me."));
							s.QAItems.Add(new QAItem("What were you up to today?", "None of your business."));
							s.QAItems.Add(new QAItem("Did you pull over someone today?", "What? You're the one pulling ME over!"));
							s.QAItems.Add(new QAItem("where did you get the red and blue lights?", "What red and blue lights?"));
							s.QAItems.Add(new QAItem("Have you had anything to drink?", "Why don't you breathalyze me?"));
							break;
						default:
							s.SpeechLines.Add("Is there a problem, Officer?");

							s.QAItems.Add(new QAItem("Is this your vehicle?", "Yes, it is."));
							s.QAItems.Add(new QAItem("What were you up to today?", "Just...taking a drive, you know."));
							s.QAItems.Add(new QAItem("Did you pull over someone today?", "Pull someone over? No!"));
							s.QAItems.Add(new QAItem("where did you get the red and blue lights?", "I stole them from...I MEAN...the internet!"));
							s.QAItems.Add(new QAItem("Have you had anything to drink?", "No."));
							break;
					}

					Peds.Add(s);

					if (vehType == EImpersonatorVehicle.CivilianVehicle) {
						v.SpeechLines.Add("But yeah, about his car...it definitely wasn't a police car.");
						v.SpeechLines.Add("But it sure as hell was dressed up to look like one.");
						v.SpeechLines.Add("It's a model that you guys drive, I think.");

						v.SpeechLines.Add(string.Format("The car was a {0} colored {1}.", suspectVehicle.PrimaryColorName, suspectVehicle.Model.Name));
					} else {
						v.SpeechLines.Add("The car was a Dilettante. It was white, and it had Security Patrol written on it.");
					}


					int licensePlateFactor = Common.gRandom.Next(3);
					if (licensePlateFactor == 0) {
						v.SpeechLines.Add(string.Format("The license plate number was {0}.", suspectVehicle.LicensePlate));
						fullPlate = true;
						licensePlate = suspectVehicle.LicensePlate;
					} else if (licensePlateFactor == 1) {
						v.SpeechLines.Add(string.Format("The first three digits of the license plate were {0}.", suspectVehicle.LicensePlate.Substring(0, 3)));
						licensePlate = suspectVehicle.LicensePlate.Substring(0, 3);
					} else {
						int idx = suspectVehicle.LicensePlate.Length - 3;
						v.SpeechLines.Add(string.Format("The last three digits of the license plate were {0}.", suspectVehicle.LicensePlate.Substring(idx)));
						licensePlate = suspectVehicle.LicensePlate.Substring(idx);
					}
				}

				return true;
			} else {
				Radio.CallIsCode4(this.ScriptInfo.Name);
				End();
				return false;
			}
		}

		private void RandomizeVictimStory(ref Victim v, int witnessStory)
		{
			{
				switch (witnessStory) {
					case 0:
						v.SpeechLines.Add("Something just didn't add up!");
						v.SpeechLines.Add("I was driving down the street, and this car comes up from behind.");
						v.SpeechLines.Add("He flashes a blue light in his window, signalling me to pull over.");
						v.SpeechLines.Add("So I did, but he gets out, and this dude looked NOTHinG like a cop!");
						v.SpeechLines.Add("No uniform, no badge, nothing! I was so scared, I just drove off!");
						v.SpeechLines.Add("Was he a cop? I hope I didn't do anything wrong!");
						suspectAttackChance = 30;
						break;
					case 1:
						v.SpeechLines.Add("This guy drove alongside my car a few minutes ago...");
						v.SpeechLines.Add("And he flashed a badge at me!! and it was just a regular car, no lights!");
						v.SpeechLines.Add("I wasn't speeding or anything, the whole thing just seemed odd.");
						v.SpeechLines.Add("Would an off duty cop try to stop someone like that?");
						v.SpeechLines.Add("It was weird...when I didn't listen to him, he drove off!");
						suspectAttackChance = 30;
						break;
					case 2:
						v.SpeechLines.Add("This car came up behind me, and flashed red and blue lights.");
						v.SpeechLines.Add("I thought it was a cop, so naturally, I pulled over.");
						v.SpeechLines.Add("The guy gets out, walks up, and pulls out a gun!!");
						v.SpeechLines.Add("He took my wallet, my credit cards, everything!");
						v.SpeechLines.Add("I was so scared, I thought he was going to kill me!!");
						suspectAttackChance = 60;
						break;
					case 3:
						v.SpeechLines.Add("This guy drove up behind me, and started flashing his high beams.");
						v.SpeechLines.Add("Then he put his hand out the window, showing me a badge.");
						v.SpeechLines.Add("I pulled over, he got out, and started yelling at me.");
						v.SpeechLines.Add("Told me he was a cop, and to get out of the car.");
						v.SpeechLines.Add("He said I was an illegal alien, and I was under arrest.");
						v.SpeechLines.Add("Sir, I was BORN in this country, I'm an American citizen!");
						v.SpeechLines.Add("I told him to fuck off, and he pulled out a gun!");
						v.SpeechLines.Add("So I hit the gas, and got the fuck outta there!");
						suspectAttackChance = 40;
						break;
					case 4:
						v.SpeechLines.Add("This car came up behind me, and flashed red and blue lights from his window.");
						v.SpeechLines.Add("I thought it was a cop, so naturally, I pulled over.");
						v.SpeechLines.Add("He said I was speeding, and asked me for my license.");
						v.SpeechLines.Add("I asked him if he was a cop, and he said yes.");
						v.SpeechLines.Add("Then I said no, you're not wearing a uniform.");
						v.SpeechLines.Add("He told he to shut up, and said he was \"undercover\".");
						v.SpeechLines.Add("I think he was a police reject, to be honest.");
						v.SpeechLines.Add("Then he asked me to step out of the car, and I said no.");
						v.SpeechLines.Add("Then he said, \"Hey! I am a cop, and you will respect my Authoritah!\"");
						v.SpeechLines.Add("I just drove off and left him there.");
						suspectAttackChance = 15;
						break;
					default:
						vehType = EImpersonatorVehicle.SecurityVehicle;
						v.SpeechLines.Add("This car came up behind me, and flashed red and blue lights from his window.");
						v.SpeechLines.Add("I thought it was a cop, so naturally, I pulled over.");
						v.SpeechLines.Add("But he wasn't a cop. He was wearing some kind of security uniform.");
						v.SpeechLines.Add("The car he was driving looked like a fucking Prius!");
						v.SpeechLines.Add("He said I was speeding, and asked me for my license.");
						v.SpeechLines.Add("So I said, \"You're not a cop, you're a fucking security guard.\"");
						v.SpeechLines.Add("He told he to shut up, and said he was \"undercover\".");
						v.SpeechLines.Add("Then he asked me to step out of the car, and I said no.");
						v.SpeechLines.Add("Then he's like, \"Don't make me use force!\"");
						v.SpeechLines.Add("I told him to fuck off, and he pulled out a gun!");
						v.SpeechLines.Add("So I hit the gas, and got the fuck outta there!");
						suspectAttackChance = 25;
						break;
				}
			}
		}

		public override void OnArrivalAtScene()
		{
			base.OnArrivalAtScene();

			Game.DisplaySubtitle("Victim: Officer!! Over here!!", 8000);

			Victim v = (Victim)GetPed("Victim1");
			if (v != null && v.Exists()) {
				v.CreateBlip();
			}

			SuspectSearch = SuspectSearchStateEnum.Null;
			Game.DisplayHelp("Press " + Config.SpeakKey.ToString() + " to talk to the 911 caller.", 8000);
		}

		public override void Process()
		{
			base.Process();

			if (Game.LocalPlayer.Character.IsDead) {
				return;
			}

			Victim v = (Victim)GetPed("Victim1");
			Suspect s = (Suspect)GetPed("Suspect1");
			Vehicles.Vehicle suspectVeh = GetVehicle("SuspectVehicle");

			if (CalloutState == CalloutState.UnitResponding) {
				if (v != null && v.IsDead) {
					v.Resurrect();
				}
			}

			if (Common.IsKeyDown(Config.SpeakKey)) {
				SpeakToSubject(v, s);
			}

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
                Radio.CallIsCode4(this.ScriptInfo.Name);
                End();

    //            if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
					
				//}
			}

			if (CalloutState == CalloutState.AtScene) {
				if (s != null && s.Exists()) {
					if (s.IsArrested() || s.IsDead) {
						if (s.Inventory.Weapons.Count > 0) {
							s.Inventory.Weapons.Clear();
							Game.DisplayNotification("While searching the suspect, you find/remove a ~r~pistol~w~.");
						}

						if (s.IsArrested()) {
							if (endTipDisplayed == false) {
								endTipDisplayed = true;

								GameFiber.StartNew(() =>
								{
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
					} else {
						if (SuspectSearch == SuspectSearchStateEnum.Null) {
							if (suspectIdentified == false && v.HasSpoken) {
								suspectIdentified = true;

								if (suspectVeh != null && suspectVeh.Exists()) {
									if (Common.IsComputerPlusRunning()) {
										AddVehicleToCallout(suspectVeh);
									}

									suspectLastSeen = suspectVeh.Position;
									CreateSearchArea(suspectLastSeen);
									SuspectSearch = SuspectSearchStateEnum.NotYetLocated;
								} else {
									SuspectSearch = SuspectSearchStateEnum.Escaped;
									Game.DisplayNotification("Police Impersonator Callout Crashed");
									End();
								}

								GameFiber.StartNew(() =>
								{
									Vehicles.Vehicle veh = GetVehicle("VictimCar1");

									if (veh != null && veh.Exists()) {
										GameFiber.Sleep(3000);
										v.Tasks.CruiseWithVehicle(veh, 10, VehicleDrivingFlags.Normal);
										v.Dismiss();
										veh.Dismiss();
									}
								});

								CallDetails += Environment.NewLine;
								CallDetails += Environment.NewLine;

								if (fullPlate) {
									CallDetails += string.Format("UPDATE: Vehicle was a {0} colored {1}; License # {2}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate);
								} else {
									CallDetails += string.Format("UPDATE: Vehicle was a {0} colored {1}; Partial license {2}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate);
								}

								if (fullPlate == true) {
									Radio.UnitMessage(string.Format("Suspect vehicle is a {0} {1}, License # {2}", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate));
								} else {
									Radio.UnitMessage(string.Format("Suspect vehicle is a {0} {1}, Partial license # {2}", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate));
								}

								Radio.DispatchMessage("Roger", true);
								Game.DisplayHelp("The victim will now leave the scene. Search the area for the suspect.");

								string pAudio = "SUSPECT_LAST_SEEN IN_OR_ON_POSITION";

								string mHeading = Common.GetDirectionAudiofromHeading(suspectVeh.Heading);
								if (mHeading != "") {
									pAudio = string.Format("SUSPECT_LAST_SEEN DIR_HEADING {0} IN_OR_ON_POSITION", mHeading);
								}

								AudioPlayerEngine.PlayAudio(pAudio, suspectLastSeen);
							}
						}

						if (SuspectSearch == SuspectSearchStateEnum.NotYetLocated) {
							if (suspectVeh != null && suspectVeh.Exists()) {
								if (Game.LocalPlayer.Character.Position.DistanceTo(suspectVeh.GetOffsetPosition(Vector3.RelativeBack * 2)) <= 15) {
									//SuspectSearch = SuspectSearchStateEnum.Located
									//DeleteSearchArea()
									//suspectVeh.CreateBlip(Drawing.Color.Yellow)
									//s.CreateBlip(Drawing.Color.Yellow)
									//Radio.SuspectSpotted()


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

													if (Common.IsComputerPlusRunning()) {
														AddPedToCallout(s);
													}

													break; // TODO: might not be correct. Was : Exit While
												}
											}

											suspectCounterOn = false;
										});
									}

								}

								if ((suspectSearchArea != null && suspectSearchArea.Exists()) && suspectCounterOn == false) {
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
							} else {
								SuspectSearch = SuspectSearchStateEnum.Escaped;
								Game.DisplayNotification("Police Impersonator Callout Crashed");
								End();
							}
						} else if (SuspectSearch == SuspectSearchStateEnum.Located) {
							if (suspectVisual == false) {
								suspectVisual = true;

								ProcessSuspectVisual(ref s);
							} else {
								//If pulloverPossible = True Then
								//    ProcessPullover(s)
								//End If

								if (suspectAttacking == false) {
									if (Game.LocalPlayer.Character.Position.DistanceTo(s.Position) < 10) {
										if (Game.LocalPlayer.Character.IsOnFoot) {
											if (suspectWillAttack == true && s.IsOnFoot) {
												suspectAttacking = true;
                                                Stealth.Common.Natives.Peds.AttackPed(s, Game.LocalPlayer.Character);
											}
										}
									}
								}
							}
						}
					}
				} else {
					//Logger.LogTrivialDebug("Suspect is null?!")
				}
			}

		}

		private void ProcessSuspectVisual(ref Suspect s)
		{
			//Siren = pursuit
			if (Game.LocalPlayer.Character.IsInAnyVehicle(false) == true) {
				Vehicle copcar = Game.LocalPlayer.Character.CurrentVehicle;

				if (copcar != null) {
					if (copcar.Exists() && copcar.HasSiren) {
						if (copcar.IsSirenOn == true && copcar.IsSirenSilent == false) {
							//officerRespondedCode3 = true;
							//pursuitinitiated = true;
							pursuit = Common.CreatePursuit();
							s.AddToPursuit(pursuit);

							Game.DisplayNotification("The suspect heard your siren and is fleeing.");
							return;
						}
					}
				}
			}

			//Game.DisplayHelp("Turn on your lights to pull over the suspect.", 8000)
			//pulloverPossible = True
			//mPulloverState = PulloverState.Following
		}

		//Private Sub ProcessPullover(ByVal s As Suspect)
		//    Dim mCopCar As Vehicle = Game.LocalPlayer.Character.CurrentVehicle

		//    select Case mPulloverState
		//        Case PulloverState.Following
		//            If mCopCar.Exists() Then
		//                If mCopCar.IsSirenOn && mCopCar.Position.DistanceTo(s.Position) < 20 Then
		//                    mPulloverState = PulloverState.LightsOrSirenOn
		//                End If
		//            End If

		//        Case PulloverState.LightsOrSirenOn
		//            Dim reaxFactor As integer = Common.gRandom.Next(4)
		//            If reaxFactor = 3 Then
		//                mPulloverState = PulloverState.Fleeing
		//                pursuitinitiated = True
		//                pursuit = Common.CreatePursuit()
		//                s.AddToPursuit(pursuit)
		//            Else
		//                mPulloverState = PulloverState.Stopping
		//                GameFiber.StartNew(
		//                    Sub()
		//                        s.Tasks.ParkVehicle(s.GetOffsetPositionFront(10), (s.Heading - 25)).WaitForCompletion()
		//                        mPulloverState = PulloverState.Parked
		//                    End Sub)
		//            End If

		//        Case PulloverState.Stopping
		//        Case PulloverState.Parked
		//            Dim fleeFactor As integer = Common.gRandom.Next(5)
		//            If fleeFactor = 4 Then
		//                mPulloverState = PulloverState.Fleeing
		//                GameFiber.StartNew(
		//                    Sub()
		//                        s.Tasks.PerformDrivingManeuver(VehicleManeuver.BurnOut)
		//                        GameFiber.Sleep(1000)
		//                        pursuitinitiated = True
		//                        pursuit = Common.CreatePursuit()
		//                        s.AddToPursuit(pursuit)
		//                    End Sub)
		//            Else
		//                GameFiber.StartNew(
		//                    Sub()
		//                        While True
		//                            GameFiber.Yield()
		//                            If Game.LocalPlayer.Character.IsOnFoot Then
		//                                s.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen)
		//                                Exit While
		//                            End If
		//                        End While
		//                    End Sub)
		//            End If

		//    End select
		//End Sub

		private void SpeakToSubject(Victim v, Suspect s)
		{
			if (s != null && s.Exists()) {
				if (Game.LocalPlayer.Character.Position.DistanceTo(s.Position) < 3) {
					s.Speak();
					return;
				}
			}

			if (v != null && v.Exists()) {
				if (Game.LocalPlayer.Character.Position.DistanceTo(v.Position) < 3) {
					v.Speak();
					return;
				}
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

		public override void End()
		{
			DeleteSearchArea();
			base.End();
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
			get { return false; }
		}

		enum EImpersonatorVehicle
		{
			CivilianVehicle,
			SecurityVehicle
		}

	}

}