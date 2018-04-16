Imports Rage
Imports Traffic_Policer
Imports Traffic_Policer.API
Imports Traffic_Policer.Impairment_Tests

Module TrafficPolicerFunctions

    Friend Function GetRandomOverTheLimitAlcoholLevel() As AlcoholLevels
        Return Functions.GetRandomOverTheLimitAlcoholLevel()
    End Function

    Friend Function GetRandomUnderTheLimitAlcoholLevel() As AlcoholLevels
        Return Functions.GetRandomUnderTheLimitAlcoholLevel()
    End Function

    Friend Sub MakePedImmuneToAmbientEvents(ByVal pPed As Ped)
        Functions.MakePedImmuneToAmbientEvents(pPed)
    End Sub

    Friend Sub SetPedAlcoholLevel(ByVal pPed As Ped, ByVal pAlcoholLevel As AlcoholLevels)
        Functions.SetPedAlcoholLevel(pPed, pAlcoholLevel)
    End Sub

    Friend Sub SetPedDrugsLevels(ByVal pPed As Ped, ByVal pWeed As Boolean, ByVal pCoke As Boolean)
        Functions.SetPedDrugsLevels(pPed, pWeed, pCoke)
    End Sub

    Friend Sub SetVehicleInsuranceStatus(ByVal pVehicle As Vehicle, ByVal pInsured As Boolean)
        Functions.SetVehicleInsuranceStatus(pVehicle, pInsured)
    End Sub

End Module
