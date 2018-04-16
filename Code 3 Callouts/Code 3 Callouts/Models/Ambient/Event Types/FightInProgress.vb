Imports Rage
Imports Stealth.Common.Extensions
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util.Audio

Namespace Models.Ambient.EventTypes

    Public Class FightInProgress
        Inherits AmbientBase
        Implements IAmbientBase

        Dim suspectLeavingArea As Boolean = False
        Dim m911Called As Boolean = False

        Public Sub New()
            RadioCode = 242
            CrimeEnums = {DISPATCH.CRIMES.ASSAULT_ON_A_CIVILIAN}.ToList()
        End Sub

        Protected Overrides Function IsEventStarted() As Boolean
            Dim baseReturn As Boolean = MyBase.IsEventStarted()

            If baseReturn = True Then
                Dim p1 As PedBase = GetPed("Ped1")
                Dim p2 As PedBase = GetPed("Ped2")

                p1.AttackPed(p2)
                p2.AttackPed(p1)

                Return True
            Else
                Return False
            End If
        End Function

        Protected Overrides Sub Process()
            MyBase.Process()

            Dim p1 As PedBase = GetPed("Ped1")
            Dim p2 As PedBase = GetPed("Ped2")

            If suspectLeavingArea = False Then
                If (p1 IsNot Nothing AndAlso p1.Exists) AndAlso (p2 IsNot Nothing AndAlso p2.Exists) Then
                    If p1.IsDead AndAlso p2.IsDead Then
                        'Both dead, do nothing
                    ElseIf p1.IsDead = True AndAlso p2.IsDead = False Then
                        'p1 dead, p2 alive
                        suspectLeavingArea = True
                        p2.Tasks.Wander()
                    ElseIf p1.IsDead = False AndAlso p2.IsDead = True Then
                        'p2 dead, p1 alive
                        suspectLeavingArea = True
                        p1.Tasks.Wander()
                    Else
                        'Both alive, do nothing

                        If m911Called = False Then
                            If p1.Exists() AndAlso p2.Exists() Then
                                If p1.DistanceTo(p2.Position) < 5 Then
                                    m911Called = True

                                    GameFiber.StartNew(
                                        Sub()
                                            GameFiber.Sleep(5000)
                                            If p1.Exists() Then Dispatch911Call(p1.Position)
                                        End Sub)
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End Sub

        Protected Overrides ReadOnly Property PedsRequired As Integer
            Get
                Return 2
            End Get
        End Property

        Protected Overrides ReadOnly Property CanUseExistingPeds As Boolean
            Get
                Return True
            End Get
        End Property

    End Class

End Namespace