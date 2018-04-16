using LSPD_First_Response.Mod.API;
using Rage;
using Stealth.Common;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;

namespace Stealth.Plugins.Code3Callouts.Models.Ambient.EventTypes
{

	internal class Mugging : AmbientBase, IAmbientBase
	{

		bool officerinRange = false;
		//LHandle pursuit;
		//bool pursuitinitiated = false;
		bool suspectLeavingArea = false;

		bool suspectShooting = false;
		internal Mugging()
		{
			RadioCode = 211;
            CrimeEnums = new List<DISPATCH.CRIMES>() { DISPATCH.CRIMES.MUGGING };
		}

		public override bool IsEventStarted()
		{
			bool baseReturn = base.IsEventStarted();

			if (baseReturn == true) {
				PedBase s = GetPed("Ped1");
				PedBase v = GetPed("Ped2");

				if ((s != null && s.Exists()) && (v != null && v.Exists())) {
					//s.BlockPermanentEvents = True
					v.BlockPermanentEvents = true;

					s.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 56, true);

                    Stealth.Common.Natives.Peds.TurnToFaceEntity(v, s, 1000);
                    Stealth.Common.Natives.Peds.AimGunAtEntity(s, v, -1);
					v.Tasks.Clear();
					v.Tasks.PutHandsUp(60000, s);

					GameFiber.StartNew(() =>
					{
						GameFiber.Sleep(10000);
						if (v.Exists())
							Dispatch911Call(v.Position);
					});

					GameFiber.StartNew(() =>
					{
						GameFiber.Sleep(60000);

						if (suspectShooting == false) {
							ShootAtVictim(v, s);
						}
					});
				}

				return true;
			} else {
				return false;
			}
		}

		public override void Process()
		{
			try
            {
                base.Process();

                PedBase s = GetPed("Ped1");
                PedBase v = GetPed("Ped2");

                if ((s != null && s.Exists()) && (v != null && v.Exists()))
                {
                    if (officerinRange == false)
                    {
                        if (Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 40)
                        {
                            officerinRange = true;

                            if (v.IsDead == false)
                            {
                                Game.DisplaySubtitle("~y~Victim: ~w~OFFICER!! HELP ME!!!", 10000);
                            }

                            GameFiber.StartNew(() =>
                            {
                                if (suspectShooting == false)
                                {
                                    ShootAtVictim(v, s);
                                }
                            });
                        }
                    }

                    if (suspectShooting == true)
                    {
                        if (suspectLeavingArea == false)
                        {
                            if (v.IsDead)
                            {
                                if (officerinRange == false)
                                {
                                    s.Tasks.Wander();
                                    suspectLeavingArea = true;
                                }
                                else
                                {
                                    Stealth.Common.Natives.Peds.AttackPed(s, Game.LocalPlayer.Character);
                                    suspectLeavingArea = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    End();
                }
            }
            catch (Exception ex)
            {
                Logger.LogVerbose("Error while processing ambient event: " + Environment.NewLine + ex.ToString());
                End();
            }
        }

		private void ShootAtVictim(PedBase v, PedBase s)
		{
			suspectShooting = true;

			if ((s != null && s.Exists()) && (v != null && v.Exists())) {
				v.Tasks.Clear();
				s.Tasks.Clear();

				s.KeepTasks = true;
				v.KeepTasks = true;

                //Shoot the victim
                Stealth.Common.Natives.Peds.ReactAndFleePed(v, s);
                Stealth.Common.Natives.Peds.AttackPed(s, v);
			}
		}

		public override bool CanUseExistingPeds {
			get { return false; }
		}

		public override int PedsRequired {
			get { return 2; }
		}

	}

}