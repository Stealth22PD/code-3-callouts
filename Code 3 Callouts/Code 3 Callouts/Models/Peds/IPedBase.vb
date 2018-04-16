Imports Rage

Namespace Models.Peds

    Public Interface IPedBase

        Property Type As PedType
        Property Name As String
        Property DisplayName As String
        Property Blip As Blip
        'Property SearchArea As Blip
        Property OriginalSpawnPoint As Vector3
        Property SpeechLines As List(Of String)
        Property SpeechIndex As Integer
        Property PhysicalCondition As String

        Sub Speak()
        Sub CreateBlip(Optional ByVal pColor As Drawing.Color = Nothing)
        Sub DeleteBlip()
        'Sub CreateSearchArea()
        'Sub DeleteSearchArea()

        Sub SetIsDrunk(ByVal pValue As Boolean)
        Sub AttackPed(ByVal pTargetPed As Ped)
        Sub TurnToFaceEntity(ByVal pTarget As Entity, Optional ByVal pTimeout As Integer = 5000)

    End Interface

End Namespace