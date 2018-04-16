Imports Rage
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util.Audio

Namespace Models

    Public Interface IPoliceIncident

        Function GetRandomSpawnPoint(ByVal pMin As Single, ByVal pMax As Single) As Vector3
        Function GetPed(ByVal pName As String) As PedBase
        Function GetVehicle(ByVal pName As String) As Vehicles.Vehicle

        Property RadioCode As Integer
        Property CrimeEnums As List(Of DISPATCH.CRIMES)
        Property SpawnPoint As Vector3

        Property Peds As List(Of PedBase)
        Property Vehicles As List(Of Vehicles.Vehicle)

    End Interface

End Namespace