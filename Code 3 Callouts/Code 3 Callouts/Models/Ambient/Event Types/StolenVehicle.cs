using Rage;
using Stealth.Common;
using static Stealth.Common.Scripting.Vehicles;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Stealth.Plugins.Code3Callouts.Models.Ambient.EventTypes
{

	internal class StolenVehicle : AmbientBase, IAmbientBase
	{

		private EState mState = EState.LookingForVehicle;

		private Vehicles.Vehicle mVehicle = null;
		internal StolenVehicle()
		{
			RadioCode = 487;
			CrimeEnums = new List<DISPATCH.CRIMES>() { DISPATCH.CRIMES.PERSON_STEALING_A_CAR };
		}

		public override bool IsEventStarted()
		{
			Logger.LogTrivialDebug("starting event");
			bool baseReturn = base.IsEventStarted();

			if (baseReturn) {
				Logger.LogTrivialDebug("base return true");
				PedBase p1 = GetPed("Ped1");
				Logger.LogTrivialDebug("p1.exists=" + p1.Exists().ToString());
				return p1.Exists();
			} else {
				Logger.LogTrivialDebug("base return false");
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
                            if (mVehicle.Exists() && p1.Exists())
                            {
                                p1.Tasks.FollowNavigationMeshToPosition(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f), Common.GetHeadingToPoint(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f), mVehicle.Position), 2.3f);
                                mVehicle.LockStatus = VehicleLockStatus.LockedButCanBeBrokenInto;
                                mState = EState.WalkingToVehicle;
                            }
                            else
                            {
                                End();

                            }
                            break;

                        case EState.WalkingToVehicle:
                            if (mVehicle.Exists() && p1.Exists())
                            {
                                if (p1.Position.DistanceTo(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2f)) < 4)
                                {
                                    mState = EState.BreakingWindow;
                                    p1.Tasks.Clear();
                                    GameFiber.StartNew(() =>
                                    {
                                        if (mVehicle.Exists() && p1.Exists())
                                        {
                                            p1.Tasks.EnterVehicle(mVehicle, -1).WaitForCompletion();

                                            if (mVehicle.Exists() && p1.Exists())
                                            {
                                                mState = EState.VehicleStolen;
                                                mVehicle.IsStolen = true;
                                                Stealth.Common.Natives.Functions.CallByHash(0xb8ff7ab45305c345uL, Common.GetNativeArgument(mVehicle));
                                                p1.Tasks.CruiseWithVehicle(mVehicle, 15, (VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundPeds | VehicleDrivingFlags.DriveAroundObjects));
                                                Dispatch911Call(p1.Position);
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
                                }
                            }
                            else
                            {
                                End();

                            }
                            break;

                        case EState.VehicleStolen:
                            if (mVehicle.Exists())
                            {
                                if (mVehicle.DistanceTo(Game.LocalPlayer.Character.Position) > 250)
                                {
                                    End();
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
			List<Rage.Vehicle> mClosestVehicles = GetVehiclesNearPosition(pPosition, pRadius, GetEntitiesFlags.ConsiderCars | GetEntitiesFlags.ExcludeOccupiedVehicles | GetEntitiesFlags.ExcludePlayerVehicle).ToList();
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
			BreakingWindow,
			VehicleStolen
		}

	}

}