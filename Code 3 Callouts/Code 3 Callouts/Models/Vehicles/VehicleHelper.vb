Imports Rage
Imports Stealth.Common.Models
Imports Stealth.Common.Natives

Namespace Models.Vehicles

    Public Module VehicleHelper

        Friend Function GetVehicleColors(ByVal v As Rage.Vehicle) As VehicleColor
            Return Stealth.Common.Natives.Vehicles.GetVehicleColors(v)
        End Function

        Friend Sub SetVehicleColors(ByVal v As Rage.Vehicle, ByVal pPrimColor As Integer, ByVal pSecColor As Integer)
            Functions.CallByName("SET_VEHICLE_COLOURS", pPrimColor, pSecColor)
        End Sub

    End Module

End Namespace