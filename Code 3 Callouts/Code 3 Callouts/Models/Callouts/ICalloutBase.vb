Imports Rage
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util.Audio

Namespace Models.Callouts

    Public Interface ICalloutBase

        Sub DisplayObjective()
        Function CalculateSpawnpoint() As Vector3
        Function PerformPedChecks() As Boolean
        Sub OnArrivalAtScene()
        Sub CreateBlip()
        Sub DeleteBlip()
        Sub OfficerDown()
        Sub DeleteEntities()
        Sub AskPedToFollowOfficer()

        Property CalloutID As Guid
        Property Objective As String
        Property CallDispatchTime As DateTime
        Property ResponseType As CallResponseType
        ReadOnly Property IsFixedSpawnPoint As Boolean
        ReadOnly Property RequiresSafePedPoint As Boolean
        Overloads Property State As CalloutState
        Property CallBlip As Blip
        Property CallDetails As String
        Property Markers As List(Of Blip)
        ReadOnly Property ShowAreaBlipBeforeAccepting As Boolean
        Property SkipRespondingState() As Boolean

        Property PedsToIgnore As List(Of Rage.PoolHandle)

        Property FoundPedSafeSpawn As Boolean

    End Interface

End Namespace