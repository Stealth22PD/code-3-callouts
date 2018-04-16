Imports Rage

Namespace Models.Interiors

    Public Class Residence
        Public Sub New()
            Init()
        End Sub

        Public Sub New(ByVal pInterior As Interior, ByVal pEntryPoint As Vector3)
            Interior = pInterior
            EntryPoint = pEntryPoint
        End Sub

        Private Sub Init()
            Interior = Nothing
            EntryPoint = Vector3.Zero
        End Sub

        Public Property Interior As Interior
        Public Property EntryPoint As Vector3
    End Class

End Namespace