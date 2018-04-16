Imports Rage
Imports LSPD_First_Response.Mod.API

Namespace Models.Peds

    Public Class Cop
        Inherits PedBase

        Public Shared Function Create(ByVal pName As String, ByVal position As Rage.Vector3, ByVal heading As Single, ByVal pIsMale As Boolean) As Cop
            Dim pType As CopType = CopType.PoliceOfficer

            If Common.IsPlayerInLosSantos = False Then
                pType = CopType.Sheriff
            End If

            If pType = CopType.PoliceOfficer Then
                Return CreateCityCop(pName, position, heading, pIsMale)
            Else
                Return CreateSheriff(pName, position, heading, pIsMale)
            End If
        End Function

        Public Shared Function Create(ByVal pName As String, ByVal position As Rage.Vector3, ByVal heading As Single, ByVal pIsMale As Boolean, ByVal pType As CopType) As Cop
            If pType = CopType.PoliceOfficer Then
                Return CreateCityCop(pName, position, heading, pIsMale)
            Else
                Return CreateSheriff(pName, position, heading, pIsMale)
            End If
        End Function

        Private Shared Function CreateCityCop(ByVal pName As String, ByVal position As Rage.Vector3, ByVal heading As Single, ByVal pIsMale As Boolean) As Cop
            If pIsMale = True Then
                Return New Cop(pName, "S_M_Y_COP_01", position, heading)
            Else
                Return New Cop(pName, "S_F_Y_COP_01", position, heading)
            End If
        End Function

        Private Shared Function CreateSheriff(ByVal pName As String, ByVal position As Rage.Vector3, ByVal heading As Single, ByVal pIsMale As Boolean) As Cop
            If pIsMale = True Then
                Return New Cop(pName, "S_M_Y_SHERIFF_01", position, heading)
            Else
                Return New Cop(pName, "S_F_Y_SHERIFF_01", position, heading)
            End If
        End Function

        'Public Sub New(ByVal pName As String, ByVal position As Rage.Vector3)
        '    MyBase.New(pName, PedType.Cop, position)
        'End Sub

        Public Sub New(ByVal pName As String, ByVal model As Rage.Model, ByVal position As Rage.Vector3, ByVal heading As Single)
            MyBase.New(pName, PedType.Cop, model, position, heading)
            InitCop()
        End Sub

        Protected Friend Sub New(ByVal pName As String, ByVal handle As Rage.PoolHandle)
            MyBase.New(pName, PedType.Cop, handle)
            InitCop()
        End Sub

        Private Sub InitCop()
            Me.BlockPermanentEvents = True
            Me.RelationshipGroup = New RelationshipGroup("COP")
            Me.DisplayName = "Officer"
            Me.MakePersistent()
            Functions.SetCopAsBusy(Me, True)
        End Sub

        Public Overrides Sub Dismiss()
            Functions.SetCopAsBusy(Me, False)
            MyBase.Dismiss()
        End Sub

        Enum CopType
            PoliceOfficer
            Sheriff
        End Enum

    End Class

End Namespace