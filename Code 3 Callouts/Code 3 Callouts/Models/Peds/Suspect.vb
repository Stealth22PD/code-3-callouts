Imports Rage

Namespace Models.Peds

    Public Class Suspect
        Inherits PedBase

        Public Sub New(ByVal pName As String, ByVal position As Rage.Vector3, ByVal confirmedSuspect As Boolean)
            MyBase.New(pName, PedType.Suspect, position)

            If confirmedSuspect = False Then
                Me.Type = PedType.Unknown
            End If
        End Sub

        Public Sub New(ByVal pName As String, ByVal model As Rage.Model, ByVal position As Rage.Vector3, ByVal heading As Single, ByVal confirmedSuspect As Boolean)
            MyBase.New(pName, PedType.Suspect, model, position, heading)

            If confirmedSuspect = False Then
                Me.Type = PedType.Unknown
            End If
        End Sub

        Protected Friend Sub New(ByVal pName As String, ByVal handle As Rage.PoolHandle, ByVal confirmedSuspect As Boolean)
            MyBase.New(pName, PedType.Suspect, handle)

            If confirmedSuspect = False Then
                Me.Type = PedType.Unknown
            End If
        End Sub

    End Class

End Namespace