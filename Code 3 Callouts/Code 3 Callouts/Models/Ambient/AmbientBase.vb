Imports Rage
Imports Stealth.Common
Imports Stealth.Common.Extensions
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports System.Windows.Forms

Namespace Models.Ambient

    Public MustInherit Class AmbientBase
        Implements IAmbientBase, IPoliceIncident

        Protected PedModels As List(Of String) = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01",
                                         "A_F_Y_GenHot_01", "A_F_Y_Hippie_01", "A_F_Y_Hipster_01", "A_F_Y_BevHills_01", "A_F_Y_BevHills_02", "A_F_M_Tourist_01", "A_F_M_FatWhite_01", "A_F_M_Business_02",
                                        "A_M_M_BevHills_01", "A_M_M_GenFat_01", "A_M_M_Business_01", "A_M_M_Golfer_01", "A_M_M_Skater_01", "A_M_M_Salton_01", "A_M_M_Tourist_01"}.ToList()

        Public Property Active() As Boolean Implements IAmbientBase.Active
        Public Property CrimeEnums As List(Of Util.Audio.DISPATCH.CRIMES) Implements IPoliceIncident.CrimeEnums

        Public Function Start() As Boolean Implements IAmbientBase.Start
            Init()

            If IsEventStarted() Then
                Active = True

                GameFiber.StartNew(
                    Sub()
                        While Active = True
                            Try
                                Process()
                            Catch ex As Exception
                                Logger.LogVerbose("Exception occurred in ambient event; ending...")
                                Logger.LogVerbose(ex.ToString())

                                Try
                                    [End]()
                                Catch ex2 As Exception
                                    Logger.LogVerbose("Exception occurred while ending ambient event")
                                    Logger.LogVerbose(ex.ToString())
                                End Try

                                Exit While
                            End Try

                            GameFiber.Yield()
                        End While
                    End Sub)
                Return True
            Else
                Delete()
                Return False
            End If
        End Function

        Protected Overridable Function IsEventStarted() As Boolean Implements IAmbientBase.IsEventStarted
            If Game.LocalPlayer.Character.DistanceTo(SpawnPoint) < 100 Then
                Logger.LogTrivialDebug("player too close")
                Return False
            Else
                If GetRequiredPeds() = True Then
                    Logger.LogTrivialDebug("peds found")
                    If CheckPeds() = True Then
                        CreateEntityBlips()
                        Return True
                    Else
                        Logger.LogTrivialDebug("peds check failed")
                        Delete()
                        Return False
                    End If
                Else
                    Logger.LogTrivialDebug("peds not found")
                    Return False
                End If
            End If
        End Function

        Private Sub Init() Implements IAmbientBase.Init
            Active = False
            Peds = New List(Of PedBase)
            Vehicles = New List(Of Vehicles.Vehicle)
            SpawnPoint = GetRandomSpawnPoint(100, 200)
        End Sub

        Protected Function GetRandomSpawnPoint(ByVal pMin As Single, ByVal pMax As Single) As Vector3 Implements IPoliceIncident.GetRandomSpawnPoint
            Return World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(gRandom.Next(pMin, pMax)))
        End Function

        Protected Function GetRequiredPeds() As Boolean Implements IAmbientBase.GetRequiredPeds
            Logger.LogTrivialDebug("CanUseExistingPeds = " & CanUseExistingPeds.ToString)
            Dim mReturn As Boolean = False

            If CanUseExistingPeds Then
                mReturn = GetNearbyPeds()

                If mReturn = False Then mReturn = SpawnRequiredPeds()
            Else
                mReturn = SpawnRequiredPeds()
            End If

            Return mReturn
        End Function

        Protected Function GetNearbyPeds() As Boolean Implements IAmbientBase.GetNearbyPeds
            Dim pedList As List(Of Ped) = Scripting.Peds.GetPedsNearPosition(SpawnPoint, 75.0F)

            If pedList.Count >= PedsRequired Then
                Dim cop As New RelationshipGroup("COP")
                Dim myPeds As List(Of Ped) = (From x In pedList Where x.Exists = True AndAlso x.IsPlayer = False AndAlso x.IsInAnyVehicle(True) = False AndAlso x.IsOnFoot = True AndAlso x.IsHuman AndAlso x.IsInCombat = False AndAlso x.RelationshipGroup <> cop).Take(PedsRequired).ToList()

                If myPeds.Count >= PedsRequired Then
                    Dim i As Integer = 1

                    For Each p As Ped In myPeds
                        Dim pedName As String = "Ped" & i
                        Dim s As New Suspect(pedName, p.Handle, False)
                        s.MakePersistent()
                        Peds.Add(s)

                        i += 1
                    Next

                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Protected Function SpawnRequiredPeds() As Boolean Implements IAmbientBase.SpawnRequiredPeds
            Try
                For i As Integer = 1 To PedsRequired
                    Dim pedName As String = "Ped" & i
                    Dim s As New Suspect(pedName, PedModels(gRandom.Next(PedModels.Count)), SpawnPoint.Around(3), 0, False)
                    s.MakePersistent()
                    Peds.Add(s)
                Next

                If Peds.Count = PedsRequired Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Logger.LogTrivialDebug("Error spawning required ambient peds -- " & ex.Message)
                Return False
            End Try
        End Function

        Protected Function CheckPeds() As Boolean Implements IAmbientBase.CheckPeds
            For Each p As PedBase In Peds
                If p Is Nothing OrElse p.Exists = False Then
                    Return False
                End If
            Next

            Return True
        End Function

        Protected Function EndBasedOnDistance() As Boolean Implements IAmbientBase.EndBasedOnDistance
            Dim farPeds As Integer = 0

            For Each p In Peds
                If p IsNot Nothing AndAlso p.Exists Then
                    If p.Position.DistanceTo(Game.LocalPlayer.Character.Position) > 250 Then
                        farPeds += 1
                    End If
                Else
                    farPeds += 1
                End If
            Next

            If farPeds >= Peds.Count Then
                Return True
            Else
                Return False
            End If
        End Function

        Protected Sub CreateEntityBlips() Implements IAmbientBase.CreateEntityBlips
            If Config.AmbientPedBlipsEnabled = False Then
                Exit Sub
            End If

            For Each p As PedBase In Peds
                If p IsNot Nothing AndAlso p.Exists Then
                    p.CreateBlip(Common.AmbientBlipColor)
                End If
            Next

            For Each v As Vehicles.Vehicle In Vehicles
                If v IsNot Nothing AndAlso v.Exists Then
                    v.CreateBlip(Common.AmbientBlipColor)
                End If
            Next
        End Sub

        Protected Overridable Sub Process() Implements IAmbientBase.Process
            Dim arrestedCount As Integer = 0
            Dim deadCount As Integer = 0

            For Each p In Peds
                If p IsNot Nothing AndAlso p.Exists Then
                    If p.IsDead Then
                        deadCount += 1
                        p.DeleteBlip()
                    Else
                        If p.IsArrested() Then
                            arrestedCount += 1
                            p.DeleteBlip()
                        End If
                    End If
                Else
                    deadCount += 1
                End If
            Next

            If (arrestedCount + deadCount) >= Peds.Count Then
                Active = False
            End If

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    Active = False
                End If
            End If

            If EndBasedOnDistance() Then
                Active = False
            End If

            If Active = False Then
                [End]()
            End If
        End Sub

        Protected Sub Dispatch911Call(ByVal pPosition As Vector3) Implements IAmbientBase.Dispatch911Call
            If Config.CitizensCall911ForAmbientEvents = True Then
                Dim pAudio As New List(Of AudioFile)
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS))

                Dim iReporting As Integer = gRandom.Next(1, 4)
                Select Case iReporting
                    Case 1
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORTING.CITIZENS_REPORT))
                    Case 2
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORTING.WE_HAVE))
                    Case Else
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORTING.WEVE_GOT))
                End Select

                If CrimeEnums.Count = 0 Then
                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CRIMES.CIV_ASSISTANCE))
                ElseIf CrimeEnums.Count = 1 Then
                    pAudio.Add(New AudioFile("DISPATCH", CrimeEnums(0)))
                Else
                    pAudio.Add(New AudioFile("DISPATCH", CrimeEnums(gRandom.Next(CrimeEnums.Count))))
                End If

                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CONJUNCTIVES.IN_OR_ON_POSITION))

                AudioPlayerEngine.PlayAudio(pAudio, pPosition)
                CreateOverlayBlip(pPosition)
            End If
        End Sub

        Private Sub CreateOverlayBlip(ByVal pPosition As Vector3)
            GameFiber.StartNew(
                Sub()
                    Dim mBlip As New Blip(pPosition, 30)
                    mBlip.Color = Drawing.Color.FromArgb(70, Drawing.Color.Red)
                    If mBlip.Exists() Then mBlip.Flash(1000, 15000)

                    GameFiber.Sleep(15000)
                    If mBlip.Exists() Then mBlip.StopFlashing()
                    If mBlip.Exists() Then mBlip.Delete()
                End Sub)
        End Sub

        Protected Overridable Sub [End]() Implements IAmbientBase.End
            Logger.LogTrivialDebug("Ending event")
            Active = False

            For Each p As PedBase In Peds
                If p IsNot Nothing AndAlso p.Exists Then
                    p.DeleteBlip()
                    p.IsPersistent = False
                    p.Dismiss()
                End If
            Next

            Peds.Clear()

            For Each v As Vehicles.Vehicle In Vehicles
                If v IsNot Nothing AndAlso v.Exists Then
                    v.DeleteBlip()
                    v.IsPersistent = False
                    v.Dismiss()
                End If
            Next

            Vehicles.Clear()
        End Sub

        Protected Overridable Sub Delete() Implements IAmbientBase.Delete
            For Each p As PedBase In Peds
                If p IsNot Nothing AndAlso p.Exists Then
                    p.DeleteBlip()
                    p.Delete()
                End If
            Next

            Peds.Clear()

            For Each v As Vehicles.Vehicle In Vehicles
                If v IsNot Nothing AndAlso v.Exists Then
                    v.DeleteBlip()
                    v.Delete()
                End If
            Next

            Vehicles.Clear()
        End Sub

        Public Function GetPed(pName As String) As Peds.PedBase Implements IPoliceIncident.GetPed
            Return (From x In Peds Where x.Name = pName Select x).FirstOrDefault()
        End Function

        Public Function GetVehicle(pName As String) As Vehicles.Vehicle Implements IPoliceIncident.GetVehicle
            Return (From x In Vehicles Where x.Name = pName Select x).FirstOrDefault()
        End Function

        Protected MustOverride ReadOnly Property PedsRequired As Integer Implements IAmbientBase.PedsRequired
        Protected MustOverride ReadOnly Property CanUseExistingPeds As Boolean Implements IAmbientBase.CanUseExistingPeds

        Public Property RadioCode As Integer Implements IPoliceIncident.RadioCode
        Public Property SpawnPoint As Rage.Vector3 Implements IPoliceIncident.SpawnPoint
        Public Property Peds As List(Of Peds.PedBase) Implements IPoliceIncident.Peds
        Public Property Vehicles As List(Of Vehicles.Vehicle) Implements IPoliceIncident.Vehicles

    End Class

End Namespace