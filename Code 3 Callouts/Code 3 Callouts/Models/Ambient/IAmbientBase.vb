Imports Rage

Namespace Models.Ambient

    Public Interface IAmbientBase

        Function Start() As Boolean
        Sub Init()
        Function IsEventStarted() As Boolean
        Function GetRequiredPeds() As Boolean
        Function GetNearbyPeds() As Boolean
        Function SpawnRequiredPeds() As Boolean
        Function CheckPeds() As Boolean
        Function EndBasedOnDistance() As Boolean
        Sub CreateEntityBlips()
        Sub Process()
        Sub Dispatch911Call(ByVal pPosition As Vector3)
        Sub [End]()
        Sub Delete()

        Property Active() As Boolean
        ReadOnly Property PedsRequired() As Integer
        ReadOnly Property CanUseExistingPeds() As Boolean

    End Interface

End Namespace