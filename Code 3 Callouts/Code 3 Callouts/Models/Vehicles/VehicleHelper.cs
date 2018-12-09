using Rage;
using Stealth.Common.Models;
using Stealth.Common.Natives;

namespace Stealth.Plugins.Code3Callouts.Models.Vehicles
{

	internal static class VehicleHelper
	{

		internal static VehicleColor GetVehicleColors(Rage.Vehicle v)
		{
			return Stealth.Common.Natives.Vehicles.GetVehicleColors(v);
		}

		internal static void SetVehicleColors(Rage.Vehicle v, int pPrimColor, int pSecColor)
		{
            Stealth.Common.Natives.Vehicles.SetVehicleColors(v,
                new VehicleColor((Stealth.Common.Natives.Vehicles.EPaint)pPrimColor, (Stealth.Common.Natives.Vehicles.EPaint)pSecColor)
                );
		}

	}

}