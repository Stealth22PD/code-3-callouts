Imports Rage

Namespace Models.Peds

    Public Class Witness
        Inherits PedBase

        Public Sub New(ByVal pName As String, ByVal position As Rage.Vector3)
            MyBase.New(pName, PedType.Witness, position)
        End Sub

        Public Sub New(ByVal pName As String, ByVal model As Rage.Model, ByVal position As Rage.Vector3, ByVal heading As Single)
            MyBase.New(pName, PedType.Witness, model, position, heading)
        End Sub

        Protected Friend Sub New(ByVal pName As String, ByVal handle As Rage.PoolHandle)
            MyBase.New(pName, PedType.Witness, handle)
        End Sub

    End Class

End Namespace