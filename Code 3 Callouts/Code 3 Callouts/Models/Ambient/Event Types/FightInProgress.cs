using Rage;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;

namespace Stealth.Plugins.Code3Callouts.Models.Ambient.EventTypes
{

	internal class FightinProgress : AmbientBase, IAmbientBase
	{

		bool suspectLeavingArea = false;

		bool m911Called = false;
		internal FightinProgress()
		{
			RadioCode = 242;
			CrimeEnums = new List<DISPATCH.CRIMES>{ DISPATCH.CRIMES.ASSAULT_ON_A_CIVILIAN };
		}

		public override bool IsEventStarted()
		{
			bool baseReturn = base.IsEventStarted();

			if (baseReturn == true) {
				PedBase p1 = GetPed("Ped1");
				PedBase p2 = GetPed("Ped2");

				p1.AttackPed(p2);
				p2.AttackPed(p1);

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

                PedBase p1 = GetPed("Ped1");
                PedBase p2 = GetPed("Ped2");

                if (suspectLeavingArea == false)
                {
                    if ((p1 != null && p1.Exists()) && (p2 != null && p2.Exists()))
                    {
                        if (p1.IsDead && p2.IsDead)
                        {
                            //Both dead, do nothing
                        }
                        else if (p1.IsDead == true && p2.IsDead == false)
                        {
                            //p1 dead, p2 alive
                            suspectLeavingArea = true;
                            p2.Tasks.Wander();
                        }
                        else if (p1.IsDead == false && p2.IsDead == true)
                        {
                            //p2 dead, p1 alive
                            suspectLeavingArea = true;
                            p1.Tasks.Wander();
                        }
                        else
                        {
                            //Both alive, do nothing

                            if (m911Called == false)
                            {
                                if (p1.Exists() && p2.Exists())
                                {
                                    if (p1.DistanceTo(p2.Position) < 5)
                                    {
                                        m911Called = true;

                                        GameFiber.StartNew(() =>
                                        {
                                            GameFiber.Sleep(5000);
                                            if (p1.Exists())
                                                Dispatch911Call(p1.Position);
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogVerbose("Error while processing ambient event: " + Environment.NewLine + ex.ToString());
                End();
            }
        }

		public override int PedsRequired {
			get { return 2; }
		}

		public override bool CanUseExistingPeds {
			get { return true; }
		}

	}

}