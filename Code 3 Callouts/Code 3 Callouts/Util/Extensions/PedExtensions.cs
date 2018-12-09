using LSPD_First_Response.Mod.API;
using Rage;
using Rage.Native;

namespace Stealth.Plugins.Code3Callouts.Util.Extensions
{

    internal static class PedExtensions
	{

		static internal bool IsArrested(this Ped p)
		{
			if (p != null && p.Exists()) {
				return Functions.IsPedArrested(p);
			} else {
				return false;
			}
		}

		static internal bool IsGettingArrested(this Ped p)
		{
			if (p != null && p.Exists()) {
				return Functions.IsPedGettingArrested(p);
			} else {
				return false;
			}
		}

		static internal void AddToPursuit(this Ped p, LHandle pursuit)
		{
			if (p != null && p.Exists()) {
				Functions.AddPedToPursuit(pursuit, p);
			}
		}

		static internal void AimGunAtCoords(this Ped p, Vector3 v)
		{
			AimGunAtCoords(p, v.X, v.Y, v.Z, 5000);
		}

		static internal void AimGunAtCoords(this Ped p, float x, float y, float z, int time)
		{
			if (p.Exists())
			    NativeFunction.Natives.TaskAimGunAtCoord(p, x, y, z, time, true, true);

        }

	}

}