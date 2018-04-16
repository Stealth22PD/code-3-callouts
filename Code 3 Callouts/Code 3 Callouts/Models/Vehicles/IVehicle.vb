Imports Rage
Imports Stealth.Common.Models
Imports Stealth.Common.Natives.Vehicles

Namespace Models.Vehicles

    Public Interface IVehicle

        Sub Init()
        Sub FillColorValues()
        Sub CreateBlip(Optional ByVal color As Drawing.Color = Nothing)
        Sub DeleteBlip()

        ReadOnly Property Colors As VehicleColor
        ReadOnly Property PrimaryColorEnum As EPaint
        ReadOnly Property PrimaryColorName As String
        ReadOnly Property SecondaryColorEnum As EPaint
        ReadOnly Property SecondaryColorName As String
        Property Blip As Blip
        Property Name As String

    End Interface

End Namespace