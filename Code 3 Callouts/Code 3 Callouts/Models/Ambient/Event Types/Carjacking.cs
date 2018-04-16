using Rage;
using LSPD_First_Response.Engine.Scripting.Entities;
using LSPD_First_Response.Mod.API;
using Stealth.Common;
using static Stealth.Common.Scripting.Vehicles;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using System.Collections.Generic;
using System.Linq;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System;
using Stealth.Plugins.Code3Callouts.Util;

namespace Stealth.Plugins.Code3Callouts.Models.Ambient.EventTypes
{

	internal class Carjacking : AmbientBase, IAmbientBase
	{

		private EState mState = EState.LookingForVehicle;
		private Vehicles.Vehicle mVehicle = null;
		private bool mPlayerConfrontingSuspect = false;

		private bool mSuspectAttackingPlayer = false;
		internal Carjacking()
		{
			RadioCode = 211;
			CrimeEnums = new List<DISPATCH.CRIMES>() { DISPATCH.CRIMES.CARJACKING };
		}

		public override bool IsEventStarted()
		{
			bool baseReturn = base.IsEventStarted();

			if (baseReturn) {
				PedBase p1 = GetPed("Ped1");
				return p1.Exists();
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

                if (p1.Exists())
                {
                    switch (mState)
                    {
                        case EState.LookingForVehicle:
                            mVehicle = GetNearbyVehicle(p1.Position, 50);

                            if (mVehicle == null)
                            {
                                mVehicle = GetNearbyVehicle(p1.Position, 100);
                            }

                            if (mVehicle != null && p1.Exists())
                            {
                                mVehicle.MakePersistent();
                                p1.MakePersistent();
                                mState = EState.VehicleFound;
                            }
                            else
                            {
                                End();

                            }
                            break;

                        case EState.VehicleFound:
                            mState = EState.WalkingToVehicle;

                            GameFiber.StartNew(() =>
                            {
                                if (mVehicle.Exists() && mVehicle.HasDriver && mVehicle.Driver.Exists() && p1.Exists())
                                {
                                    mVehicle.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait);
                                    mVehicle.Driver.KeepTasks = true;
                                    p1.Tasks.FollowNavigationMeshToPosition(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f), Common.GetHeadingToPoint(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f), mVehicle.Position), 2.3f, 5f).WaitForCompletion();

                                    if (p1.Exists())
                                    {
                                        p1.Tasks.FollowToOffsetFromEntity(mVehicle, (Vector3.RelativeLeft * 2f));
                                        mVehicle.LockStatus = VehicleLockStatus.Unlocked;
                                        mState = EState.AtVehicle;
                                    }
                                    else
                                    {
                                        End();
                                    }
                                }
                                else
                                {
                                    End();
                                }

                            });
                            break;

                        case EState.AtVehicle:
                            if (mVehicle.Exists() && p1.Exists())
                            {
                                if (p1.Position.DistanceTo(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f)) < 4)
                                {
                                    mState = EState.JackingVehicle;
                                    p1.Tasks.Clear();
                                    p1.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 56, true);
                                    p1.Tasks.AimWeaponAt(mVehicle.GetOffsetPosition(Vector3.RelativeLeft), 5000);
                                    mVehicle.IsPositionFrozen = true;

                                    GameFiber.StartNew(() =>
                                    {
                                        if (mVehicle.Exists() && p1.Exists())
                                        {
                                            Ped mVictimPed = null;

                                            if (mVehicle.HasDriver && mVehicle.Driver.Exists())
                                            {
                                                mVictimPed = mVehicle.Driver;
                                            }

                                            if (mVictimPed != null && mVictimPed.Exists())
                                            {
                                                GameFiber.Sleep(2000);
                                                mVictimPed.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion();

                                                if (mVictimPed.Exists() && p1.Exists())
                                                {
                                                    Stealth.Common.Natives.Peds.ReactAndFleePed(mVictimPed, p1);
                                                    GameFiber.Sleep(1000);
                                                }
                                                else
                                                {
                                                    End();
                                                }
                                            }

                                            mVehicle.IsPositionFrozen = false;

                                            if (p1.Exists() && mVehicle.Exists())
                                            {
                                                GameFiber.Sleep(1000);
                                                p1.Tasks.EnterVehicle(mVehicle, -1);
                                                GameFiber.Sleep(1000);
                                            }

                                            if (mVictimPed.Exists() && mVehicle.Exists())
                                            {
                                                Persona mVicPersona = Functions.GetPersonaForPed(mVictimPed);
                                                Functions.SetVehicleOwnerName(mVehicle, mVicPersona.FullName);
                                            }

                                            mState = EState.VehicleStolen;
                                            mVehicle.IsStolen = true;

                                            if (p1.Exists() && mVehicle.Exists())
                                                p1.Tasks.CruiseWithVehicle(mVehicle, 15, (VehicleDrivingFlags.Emergency));

                                            GameFiber.Sleep(1000);

                                            if (Config.CitizensCall911ForAmbientEvents)
                                            {
                                                Dispatch911Call(p1.Position);
                                                GameFiber.Sleep(1000);
                                            }

                                            mState = EState.VictimCalled911;
                                        }
                                        else
                                        {
                                            End();
                                        }
                                    });
                                }
                            }
                            else
                            {
                                End();

                            }
                            break;

                        case EState.VictimCalled911:
                            if (mVehicle.Exists() && p1.Exists())
                            {
                                if (mVehicle.DistanceTo(Game.LocalPlayer.Character.Position) > 250)
                                {
                                    End();
                                }
                                else
                                {
                                    if (mPlayerConfrontingSuspect == false)
                                    {
                                        if (p1.Exists() && p1.IsOnFoot && p1.DistanceTo(Game.LocalPlayer.Character.Position) < 20)
                                        {
                                            mPlayerConfrontingSuspect = true;

                                            if (mSuspectAttackingPlayer == false)
                                            {
                                                mSuspectAttackingPlayer = true;
                                                p1.AttackPed(Game.LocalPlayer.Character);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                End();

                            }
                            break;
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

		private Vehicles.Vehicle GetNearbyVehicle(Vector3 pPosition, float pRadius)
		{
			List<Rage.Vehicle> mClosestVehicles = GetVehiclesNearPosition(pPosition, pRadius, GetEntitiesFlags.ConsiderCars | GetEntitiesFlags.ExcludeEmptyVehicles | GetEntitiesFlags.ExcludeEmergencyVehicles | GetEntitiesFlags.ExcludePlayerVehicle).ToList();
			Rage.Vehicle mVeh = (from x in mClosestVehicles orderby x.DistanceTo(pPosition) select x).FirstOrDefault();

			if (mVeh != null && mVeh.Exists()) {
				return new Vehicles.Vehicle(mVeh.Handle);
			} else {
				return null;
			}
		}

		public override bool CanUseExistingPeds {
			get { return true; }
		}

		public override int PedsRequired {
			get { return 1; }
		}

		private enum EState
		{
			LookingForVehicle,
			VehicleFound,
			WalkingToVehicle,
			AtVehicle,
			JackingVehicle,
			VehicleStolen,
			VictimCalled911
		}

	}

}