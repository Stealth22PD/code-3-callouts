using Rage;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;

namespace Stealth.Plugins.Code3Callouts.Models
{

    internal interface IPoliceIncident
	{

		Vector3 GetRandomSpawnPoint(float pMin, float pMax);
		PedBase GetPed(string pName);
		Vehicles.Vehicle GetVehicle(string pName);

		int RadioCode { get; set; }
		List<DISPATCH.CRIMES> CrimeEnums { get; set; }

		Vector3 SpawnPoint { get; set; }
		List<PedBase> Peds { get; set; }

		List<Vehicles.Vehicle> Vehicles { get; set; }
	}

}