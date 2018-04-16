using Rage;
using Stealth.Common;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioPlayerEngine;

namespace Stealth.Plugins.Code3Callouts.Models.Ambient
{

	internal abstract class AmbientBase : IAmbientBase, IPoliceIncident
	{

		protected string[] PedModels =  {
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

        public bool Active { get; set; }
        public List<DISPATCH.CRIMES> CrimeEnums { get; set; }

		public bool Start()
		{
			Init();

			if (IsEventStarted()) {
				Active = true;

				GameFiber.StartNew(() =>
				{
					while (Active == true) {
						try {
							Process();
						} catch (Exception ex) {
							Logger.LogVerbose("Exception occurred in ambient event; ending...");
							Logger.LogVerbose(ex.ToString());

							try {
								End();
							} catch (Exception ex2) {
								Logger.LogVerbose("Exception occurred while ending ambient event");
								Logger.LogVerbose(ex2.ToString());
							}

							break; // TODO: might not be correct. Was : Exit While
						}

						GameFiber.Yield();
					}
				});
				return true;
			} else {
				Delete();
				return false;
			}
		}

        public virtual bool IsEventStarted()
		{
			if (Game.LocalPlayer.Character.DistanceTo(SpawnPoint) < 100) {
				Logger.LogTrivialDebug("player too close");
				return false;
			} else {
				if (GetRequiredPeds() == true) {
					Logger.LogTrivialDebug("peds found");
					if (CheckPeds() == true) {
						CreateEntityBlips();
						return true;
					} else {
						Logger.LogTrivialDebug("peds check failed");
						Delete();
						return false;
					}
				} else {
					Logger.LogTrivialDebug("peds not found");
					return false;
				}
			}
		}

        public void Init()
		{
			Active = false;
			Peds = new List<PedBase>();
			Vehicles = new List<Vehicles.Vehicle>();
			SpawnPoint = GetRandomSpawnPoint(100, 200);
		}

        public Vector3 GetRandomSpawnPoint(float pMin, float pMax)
		{
			return World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(Common.gRandom.Next((int)pMin, (int)pMax)));
		}

        public bool GetRequiredPeds()
		{
			Logger.LogTrivialDebug("CanUseExistingPeds = " + CanUseExistingPeds.ToString());
			bool mReturn = false;

			if (CanUseExistingPeds) {
				mReturn = GetNearbyPeds();

				if (mReturn == false)
					mReturn = SpawnRequiredPeds();
			} else {
				mReturn = SpawnRequiredPeds();
			}

			return mReturn;
		}

        public bool GetNearbyPeds()
		{
			List<Ped> pedList = Stealth.Common.Scripting.Peds.GetPedsNearPosition(SpawnPoint, 75f);

			if (pedList.Count >= PedsRequired) {
				RelationshipGroup cop = new RelationshipGroup("COP");
				List<Ped> myPeds = (from x in pedList where x.Exists() == true && x.IsPlayer == false && x.IsInAnyVehicle(true) == false && x.IsOnFoot == true && x.IsHuman && x.IsInCombat == false && x.RelationshipGroup != cop select x).Take(PedsRequired).ToList();

				if (myPeds.Count >= PedsRequired) {
					int i = 1;

					foreach (Ped p in myPeds) {
						string pedName = "Ped" + i;
						Suspect s = new Suspect(pedName, p.Handle, false);
						s.MakePersistent();
						Peds.Add(s);

						i += 1;
					}

					return true;
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

        public bool SpawnRequiredPeds()
		{
			try {
				for (int i = 1; i <= PedsRequired; i++) {
					string pedName = "Ped" + i;
					Suspect s = new Suspect(pedName, PedModels[Common.gRandom.Next(PedModels.Length)], SpawnPoint.Around(3), 0, false);
					s.MakePersistent();
					Peds.Add(s);
				}

				if (Peds.Count == PedsRequired) {
					return true;
				} else {
					return false;
				}
			} catch (Exception ex) {
				Logger.LogTrivialDebug("Error spawning required ambient peds -- " + ex.Message);
				return false;
			}
		}

        public bool CheckPeds()
		{
			foreach (PedBase p in Peds) {
				if (p == null || p.Exists() == false) {
					return false;
				}
			}

			return true;
		}

        public bool EndBasedOnDistance()
		{
			int farPeds = 0;

			foreach (var p in Peds) {
				if (p != null && p.Exists()) {
					if (p.Position.DistanceTo(Game.LocalPlayer.Character.Position) > 250) {
						farPeds += 1;
					}
				} else {
					farPeds += 1;
				}
			}

			if (farPeds >= Peds.Count) {
				return true;
			} else {
				return false;
			}
		}

        public void CreateEntityBlips()
		{
			if (Config.AmbientPedBlipsEnabled == false) {
				return;
			}

			foreach (PedBase p in Peds) {
				if (p != null && p.Exists()) {
					p.CreateBlip(Common.AmbientBlipColor);
				}
			}

			foreach (Vehicles.Vehicle v in Vehicles) {
				if (v != null && v.Exists()) {
					v.CreateBlip(Common.AmbientBlipColor);
				}
			}
		}

        public virtual void Process()
		{
			int arrestedCount = 0;
			int deadCount = 0;

			foreach (var p in Peds) {
				if (p != null && p.Exists()) {
					if (p.IsDead) {
						deadCount += 1;
						p.DeleteBlip();
					} else {
						if (p.IsArrested()) {
							arrestedCount += 1;
							p.DeleteBlip();
						}
					}
				} else {
					deadCount += 1;
				}
			}

			if ((arrestedCount + deadCount) >= Peds.Count) {
				Active = false;
			}

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
				Active = false;
				//if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
				//}
			}

			if (EndBasedOnDistance()) {
				Active = false;
			}

			if (Active == false) {
				End();
			}
		}

        public void Dispatch911Call(Vector3 pPosition)
		{
			if (Config.CitizensCall911ForAmbientEvents == true) {
				List<AudioFile> pAudio = new List<AudioFile>();
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS));

				int iREPORTING = Common.gRandom.Next(1, 4);
				switch (iREPORTING) {
					case 1:
						pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORTING.CITIZENS_REPORT));
                        break;

					case 2:
						pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORTING.WE_HAVE));
                        break;
					default:
                        pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORTING.WEVE_GOT));
                        break;
                }

                if (CrimeEnums.Count == 0) {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CRIMES.CIV_ASSISTANCE));
				} else if (CrimeEnums.Count == 1) {
					pAudio.Add(new AudioFile("DISPATCH", CrimeEnums[0]));
				} else {
					pAudio.Add(new AudioFile("DISPATCH", CrimeEnums[Common.gRandom.Next(CrimeEnums.Count)]));
				}

				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CONJUNCTIVES.IN_OR_ON_POSITION));

				AudioPlayerEngine.PlayAudio(pAudio, pPosition);
				CreateOverlayBlip(pPosition);
			}
		}

		private void CreateOverlayBlip(Vector3 pPosition)
		{
			GameFiber.StartNew(() =>
			{
				Blip mBlip = new Blip(pPosition, 30);
				mBlip.Color = System.Drawing.Color.FromArgb(70, System.Drawing.Color.Red);
				if (mBlip.Exists())
					mBlip.Flash(1000, 15000);

				GameFiber.Sleep(15000);
				if (mBlip.Exists())
					mBlip.StopFlashing();
				if (mBlip.Exists())
					mBlip.Delete();
			});
		}

        public virtual void End()
		{
			try
            {
                Logger.LogTrivialDebug("Ending event");
                Active = false;

                foreach (PedBase p in Peds)
                {
                    if (p != null && p.Exists())
                    {
                        p.DeleteBlip();
                        p.IsPersistent = false;
                        p.Dismiss();
                    }
                }

                Peds.Clear();

                foreach (Vehicles.Vehicle v in Vehicles)
                {
                    if (v != null && v.Exists())
                    {
                        v.DeleteBlip();
                        v.IsPersistent = false;
                        v.Dismiss();
                    }
                }

                Vehicles.Clear();
            }
            catch (Exception ex)
            {
                Logger.LogVerbose("Error while ending ambient event: " + Environment.NewLine + ex.ToString());
            }
        }

        public virtual void Delete()
		{
			foreach (PedBase p in Peds) {
				if (p != null && p.Exists()) {
					p.DeleteBlip();
					p.Delete();
				}
			}

			Peds.Clear();

			foreach (Vehicles.Vehicle v in Vehicles) {
				if (v != null && v.Exists()) {
					v.DeleteBlip();
					v.Delete();
				}
			}

			Vehicles.Clear();
		}

        public PedBase GetPed(string pName)
		{
			return (from x in Peds where x.Name == pName select x).FirstOrDefault();
		}

        public Vehicles.Vehicle GetVehicle(string pName)
		{
			return (from x in Vehicles where x.Name == pName select x).FirstOrDefault();
		}

		public abstract int PedsRequired { get; }
        public abstract bool CanUseExistingPeds { get; }

        public int RadioCode { get; set; }
        public Vector3 SpawnPoint { get; set; }
        public List<PedBase> Peds { get; set; }
        public List<Vehicles.Vehicle> Vehicles { get; set; }

	}

}