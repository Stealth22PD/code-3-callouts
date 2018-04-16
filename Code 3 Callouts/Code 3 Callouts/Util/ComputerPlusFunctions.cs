using Rage;
using System;
using ComputerPlus;
using ComputerPlus.API;
using System.Collections.Generic;

class ComputerPlusFunctions
{

	internal static CalloutData CreateCalloutData(string pFullName, string pShortName, Vector3 pLocation, EResponseType pResponseType, string pDescription, ECallStatus pState = ECallStatus.Created, List<Ped> pPeds = null, List<Vehicle> pVehicles = null)
	{

		return new CalloutData(pFullName, pShortName, pLocation, pResponseType, pDescription, pState, pPeds, pVehicles);
	}

	internal static void CreateCallout(CalloutData pData)
	{
		Functions.CreateCallout(pData);
	}

	internal static Guid CreateCallout(string pFullName, string pShortName, Vector3 pLocation, EResponseType pResponseType, string pDescription, ECallStatus pState = ECallStatus.Created, List<Ped> pPeds = null, List<Vehicle> pVehicles = null)
	{

		CalloutData pData = new CalloutData(pFullName, pShortName, pLocation, pResponseType, pDescription, pState, pPeds, pVehicles);
		CreateCallout(pData);

		return pData.ID;
	}

	internal static void UpdateCallStatus(Guid pCallID, ECallStatus pStatus)
	{
		switch (pStatus) {
			case ECallStatus.Unit_Responding:
				Functions.SetCalloutStatusToUnitResponding(pCallID);
                break;
			case ECallStatus.At_Scene:
				Functions.SetCalloutStatusToAtScene(pCallID);
                break;
			default:
                Functions.UpdateCalloutStatus(pCallID, pStatus);
                break;
        }
    }

	internal static void AssignCallToAIUnit(Guid pCallID)
	{
		Functions.AssignCallToAIUnit(pCallID);
	}

	internal static void ConcludeCallout(Guid pCallID)
	{
		Functions.ConcludeCallout(pCallID);
	}

	internal static void CancelCallout(Guid pCallID)
	{
		Functions.CancelCallout(pCallID);
	}

	internal static void AddUpdateToCallout(Guid pCallID, string pText)
	{
		Functions.AddUpdateToCallout(pCallID, pText);
	}

	internal static void AddPedToCallout(Guid pCallID, Ped pPed)
	{
		Functions.AddPedToCallout(pCallID, pPed);
	}

	internal static void AddVehicleToCallout(Guid pCallID, Vehicle pVeh)
	{
		Functions.AddVehicleToCallout(pCallID, pVeh);
	}

}