using Rage;
using Stealth.Common.Models;
using System.Drawing;
using static Stealth.Common.Natives.Vehicles;

namespace Stealth.Plugins.Code3Callouts.Models.Vehicles
{

	internal interface IVehicle
	{

		void Init();
		void FillColorValues();
		void CreateBlip(Color? color = null);

		void DeleteBlip();
		VehicleColor Colors { get; }
		EPaint PrimaryColorEnum { get; }
		string PrimaryColorName { get; }
		EPaint SecondaryColorEnum { get; }
		string SecondaryColorName { get; }
		Blip Blip { get; set; }

		string Name { get; set; }
	}

}