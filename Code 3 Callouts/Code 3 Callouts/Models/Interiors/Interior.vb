Imports LSPD_First_Response.Engine
Imports Rage
Imports Rage.Native

Namespace Models.Interiors

    Public Class Interior
        Public Sub New()
            Init()
        End Sub

        Public Sub New(ByVal pType As InteriorType, ByVal pInteriorSpawnPoint As SpawnPoint, ByVal pHidingPlaces As List(Of SpawnPoint))
            Type = pType
            InteriorSpawnPoint = pInteriorSpawnPoint
            HidingPlaces = pHidingPlaces
        End Sub

        Private Sub Init()
            Type = InteriorType.Null
            InteriorSpawnPoint = SpawnPoint.Zero
            HidingPlaces = New List(Of SpawnPoint)
        End Sub

        Public Sub LoadInterior()
            If InteriorSpawnPoint <> SpawnPoint.Zero Then
                Dim mInterior As Integer = NativeFunction.CallByName(Of Integer)("GET_INTERIOR_AT_COORDS", InteriorSpawnPoint.Position.X, InteriorSpawnPoint.Position.Y, InteriorSpawnPoint.Position.Z)
                NativeFunction.CallByHash(Of UInteger)(&H2CA429C029CCF247UL, mInterior)
                NativeFunction.CallByName(Of UInteger)("SET_INTERIOR_ACTIVE", mInterior, True)
                NativeFunction.CallByName(Of UInteger)("DISABLE_INTERIOR", mInterior, False)
            End If
        End Sub

        Public Property Type As InteriorType
        Public Property InteriorSpawnPoint As SpawnPoint
        Public Property HidingPlaces As List(Of SpawnPoint)
    End Class

End Namespace