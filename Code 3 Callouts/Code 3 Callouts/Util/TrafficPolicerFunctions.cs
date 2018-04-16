using Rage;
using Traffic_Policer;
using Traffic_Policer.API;
using Traffic_Policer.Impairment_Tests;

internal static class TrafficPolicerFunctions
{

	internal static AlcoholLevels GetRandomOverTheLimitAlcoholLevel()
	{
		return Functions.GetRandomOverTheLimitAlcoholLevel();
	}

	internal static AlcoholLevels GetRandomUnderTheLimitAlcoholLevel()
	{
		return Functions.GetRandomUnderTheLimitAlcoholLevel();
	}

	internal static void MakePedImmuneToAmbientEvents(Ped pPed)
	{
		Functions.MakePedImmuneToAmbientEvents(pPed);
	}

	internal static void SetPedAlcoholLevel(Ped pPed, AlcoholLevels pAlcoholLevel)
	{
		Functions.SetPedAlcoholLevel(pPed, pAlcoholLevel);
	}

	internal static void SetPedDrugsLevels(Ped pPed, bool pWeed, bool pCoke)
	{
		Functions.SetPedDrugsLevels(pPed, pWeed, pCoke);
	}

	internal static void SetVehicleinsuranceStatus(Vehicle pVehicle, bool pinsured)
	{
		Functions.SetVehicleInsuranceStatus(pVehicle, pinsured);
	}

}