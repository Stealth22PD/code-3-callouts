Imports LSPD_First_Response.Mod.API
Imports Rage
Imports Rage.Native
Imports System.Runtime.CompilerServices

Namespace Util.Extensions

    Module PedExtensions

        <Extension()>
        Friend Function IsArrested(ByVal p As Ped) As Boolean
            If p IsNot Nothing AndAlso p.Exists Then
                Return Functions.IsPedArrested(p)
            Else
                Return False
            End If
        End Function

        <Extension()>
        Friend Function IsGettingArrested(ByVal p As Ped) As Boolean
            If p IsNot Nothing AndAlso p.Exists Then
                Return Functions.IsPedGettingArrested(p)
            Else
                Return False
            End If
        End Function

        <Extension()>
        Friend Sub AddToPursuit(ByVal p As Ped, ByVal pursuit As LHandle)
            If p IsNot Nothing AndAlso p.Exists Then
                Functions.AddPedToPursuit(pursuit, p)
            End If
        End Sub

        <Extension()>
        Friend Sub AimGunAtCoords(ByVal p As Ped, ByVal v As Vector3)
            AimGunAtCoords(p, v.X, v.Y, v.Z, 5000)
        End Sub

        <Extension()>
        Friend Sub AimGunAtCoords(ByVal p As Ped, ByVal x As Single, ByVal y As Single, ByVal z As Single, ByVal time As Integer)
            If p.Exists() Then NativeFunction.CallByHash(Of UInteger)(&H6671F3EEC681BDA1UL, Common.GetNativeArgument(p), x, y, z, time, True, True)
        End Sub

    End Module

End Namespace