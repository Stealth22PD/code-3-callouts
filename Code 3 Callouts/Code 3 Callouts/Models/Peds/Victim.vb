Imports Rage

Namespace Models.Peds

    Public Class Victim
        Inherits PedBase

        Public Sub New(ByVal pName As String, ByVal position As Rage.Vector3)
            MyBase.New(pName, PedType.Victim, position)
        End Sub

        Public Sub New(ByVal pName As String, ByVal model As Rage.Model, ByVal position As Rage.Vector3, ByVal heading As Single)
            MyBase.New(pName, PedType.Victim, model, position, heading)
        End Sub

        Protected Friend Sub New(ByVal pName As String, ByVal handle As Rage.PoolHandle)
            MyBase.New(pName, PedType.Victim, handle)
        End Sub

    End Class

End Namespace