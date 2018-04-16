Imports LSPD_First_Response.Mod.API
Imports Rage
Imports Stealth.Common
Imports Stealth.Common.Extensions
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio

Namespace Models.Ambient.EventTypes

    Public Class Mugging
        Inherits AmbientBase
        Implements IAmbientBase

        Dim officerInRange As Boolean = False
        Dim pursuit As LHandle
        Dim pursuitInitiated As Boolean = False
        Dim suspectLeavingArea As Boolean = False
        Dim suspectShooting As Boolean = False

        Public Sub New()
            RadioCode = 211
            CrimeEnums = {DISPATCH.CRIMES.MUGGING}.ToList()
        End Sub

        Protected Overrides Function IsEventStarted() As Boolean
            Dim baseReturn As Boolean = MyBase.IsEventStarted()

            If baseReturn = True Then
                Dim s As PedBase = GetPed("Ped1")
                Dim v As PedBase = GetPed("Ped2")

                If (s IsNot Nothing AndAlso s.Exists) AndAlso (v IsNot Nothing AndAlso v.Exists) Then
                    's.BlockPermanentEvents = True
                    v.BlockPermanentEvents = True

                    s.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 56, True)

                    Natives.Peds.TurnToFaceEntity(v, s, 1000)
                    Natives.Peds.AimGunAtEntity(s, v, -1)
                    v.Tasks.Clear()
                    v.Tasks.PutHandsUp(60000, s)

                    GameFiber.StartNew(
                        Sub()
                            GameFiber.Sleep(10000)
                            If v.Exists() Then Dispatch911Call(v.Position)
                        End Sub)

                    GameFiber.StartNew(
                        Sub()
                            GameFiber.Sleep(60000)

                            If suspectShooting = False Then
                                ShootAtVictim(v, s)
                            End If
                        End Sub)
                End If

                Return True
            Else
                Return False
            End If
        End Function

        Protected Overrides Sub Process()
            MyBase.Process()

            Dim s As PedBase = GetPed("Ped1")
            Dim v As PedBase = GetPed("Ped2")

            If (s IsNot Nothing AndAlso s.Exists) AndAlso (v IsNot Nothing AndAlso v.Exists) Then
                If officerInRange = False Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 40 Then
                        officerInRange = True

                        If v.IsDead = False Then
                            Game.DisplaySubtitle("~y~Victim: ~w~OFFICER!! HELP ME!!!", 10000)
                        End If

                        GameFiber.StartNew(
                            Sub()
                                If suspectShooting = False Then
                                    ShootAtVictim(v, s)
                                End If
                            End Sub)
                    End If
                End If

                If suspectShooting = True Then
                    If suspectLeavingArea = False Then
                        If v.IsDead Then
                            If officerInRange = False Then
                                s.Tasks.Wander()
                                suspectLeavingArea = True
                            Else
                                Natives.Peds.AttackPed(s, Game.LocalPlayer.Character)
                                suspectLeavingArea = True
                            End If
                        End If
                    End If
                End If
            Else
            End If
        End Sub

        Private Sub ShootAtVictim(ByVal v As PedBase, ByVal s As PedBase)
            suspectShooting = True

            If (s IsNot Nothing AndAlso s.Exists) AndAlso (v IsNot Nothing AndAlso v.Exists) Then
                v.Tasks.Clear()
                s.Tasks.Clear()

                s.KeepTasks = True
                v.KeepTasks = True

                'Shoot the victim
                Natives.Peds.ReactAndFleePed(v, s)
                Natives.Peds.AttackPed(s, v)
            End If
        End Sub

        Protected Overrides ReadOnly Property CanUseExistingPeds As Boolean
            Get
                Return False
            End Get
        End Property

        Protected Overrides ReadOnly Property PedsRequired As Integer
            Get
                Return 2
            End Get
        End Property

    End Class

End Namespace