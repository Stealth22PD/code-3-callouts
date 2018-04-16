Imports LSPD_First_Response
Imports LSPD_First_Response.Mod.API
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Common
Imports Stealth.Common.Extensions
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports System.Windows.Forms

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Intoxicated Person", CalloutProbability.Medium)>
    Public Class IntoxicatedPerson
        Inherits CalloutBase

        Dim SuspectModels As String() = {"A_M_M_Hillbilly_01", "A_M_M_Hillbilly_02", "A_M_O_GenStreet_01", "A_M_Y_Hippy_01", "A_M_Y_MethHead_01", "A_M_Y_BusiCas_01", "A_M_Y_Downtown_01", "A_M_Y_EastSA_01", "A_M_Y_GenStreet_02",
                                         "A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01"}
        Dim pursuit As LHandle
        Dim pursuitInitiated As Boolean = False
        Dim officerRespondedCode3 As Boolean = False
        Dim mCode3Action As ECode3Action = ECode3Action.None

        Private Enum ECode3Action
            None
            Flee
            Deleted
        End Enum

        Public Sub New()
            MyBase.New("Intoxicated Person", CallResponseType.Code_2)
            RadioCode = 390
            CrimeEnums = {DISPATCH.CRIMES.CODE_390, DISPATCH.CRIMES.POSSIBLE_390, DISPATCH.CRIMES.PUBLIC_INTOX}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "Caller reports a male who appears to be intoxicated, and harassing those around him."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "No further details available at this time."

            Objective = "Deal with the intoxicated ~y~subject."
        End Sub

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim baseReturn As Boolean = MyBase.OnBeforeCalloutDisplayed()

            If baseReturn = False Then
                Return False
            End If

            Dim s As New Suspect("Suspect1", SuspectModels(gRandom.Next(SuspectModels.Count)), SpawnPoint, 0, False)
            s.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_KNIFE"), 56, False)
            s.SetIsDrunk(True)

            Try
                Dim animSet As New AnimationSet("move_m@drunk@verydrunk")
                animSet.LoadAndWait()
                s.MovementAnimationSet = animSet
            Catch ex As Exception
                Logger.LogVerboseDebug("Error animating ped -- " & ex.Message)
            End Try

            s.Tasks.Wander()

            Peds.Add(s)

            If PerformPedChecks() Then
                Return baseReturn
            Else
                Return False
            End If
        End Function

        Public Overrides Sub OnArrivalAtScene()
            MyBase.OnArrivalAtScene()

            If officerRespondedCode3 = True Then
                If mCode3Action = ECode3Action.Deleted Then
                    Game.DisplayNotification("The suspect fled the area.")
                    Game.DisplayHelp("The suspect heard your siren and fled.")
                    Radio.SergeantMessage("Exercise proper siren use on Code 2 calls, over")
                    Radio.CallIsCode4(Me.ScriptInfo.Name)

                ElseIf mCode3Action = ECode3Action.Flee Then
                    Game.DisplayHelp("The suspect heard your siren and is fleeing!")
                    Radio.SergeantMessage("Exercise proper siren use on Code 2 calls, over")
                End If
                [End]()
            Else
                Game.DisplayHelp("Deal with the intoxicated person. Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout.", 5000)

                Dim s As Suspect = GetPed("Suspect1")

                If s IsNot Nothing Then
                    If s.Exists Then
                        If Common.IsComputerPlusRunning() Then
                            AddPedToCallout(s)
                        End If

                        s.Tasks.Clear()
                        s.CreateBlip()
                        s.TurnToFaceEntity(Game.LocalPlayer.Character)

                        Dim drunkFactor As Integer = gRandom.Next(1, 101)
                        Dim reactionFactor As Integer = gRandom.Next(3)

                        If drunkFactor > 50 Then
                            If reactionFactor = 0 Then
                                'Stand around and be drunk
                                Try
                                    s.Tasks.PlayAnimation(New AnimationDictionary("amb@world_human_bum_standing@drunk@idle_a"), "idle_a", 1.0F, AnimationFlags.RagdollOnCollision)
                                Catch ex As Exception
                                    Logger.LogVerboseDebug("Error playing drunk anim -- " & ex.Message)
                                End Try
                            Else
                                'Attack a nearby ped
                                GameFiber.StartNew(
                                    Sub()
                                        Try
                                            Dim ped As Ped = s.GetNearbyPeds(1).FirstOrDefault()

                                            If ped IsNot Nothing AndAlso ped.Exists Then
                                                Natives.Peds.AttackPed(s, ped)
                                            End If
                                        Catch ex As Exception
                                            Logger.LogVerboseDebug("Error attacking ped -- " & ex.Message)
                                        End Try
                                    End Sub)
                            End If
                        Else

                            If reactionFactor = 0 Then
                                'Attack the player
                                GameFiber.StartNew(
                                    Sub()
                                        GameFiber.Sleep(3000)
                                        Try
                                            Natives.Peds.AttackPed(s, Game.LocalPlayer.Character)
                                        Catch ex As Exception
                                            Logger.LogVerboseDebug("Error attacking player -- " & ex.Message)
                                        End Try
                                    End Sub)
                            ElseIf reactionFactor = 1 Then
                                'Flee

                                GameFiber.StartNew(
                                    Sub()
                                        GameFiber.Sleep(3000)

                                        pursuitInitiated = True
                                        pursuit = Common.CreatePursuit()
                                        s.AddToPursuit(pursuit)
                                        Functions.SetPursuitIsActiveForPlayer(pursuit, True)
                                    End Sub)

                            ElseIf reactionFactor = 2 Then
                                'Steal police vehicle
                                Dim officerArrived As DateTime = DateTime.Now
                                GameFiber.StartNew(
                                    Sub()
                                        GameFiber.Sleep(3000)

                                        While True
                                            GameFiber.Yield()

                                            If Game.LocalPlayer.Character.IsOnFoot AndAlso Game.LocalPlayer.Character.DistanceTo(s.Position) < 10 Then
                                                Exit While
                                            End If
                                        End While

                                        Dim policeVehicle As Vehicle = Game.LocalPlayer.Character.LastVehicle

                                        If s.Exists AndAlso policeVehicle.Exists() Then
                                            Dim tgtHeading As Single = 0
                                            If policeVehicle.Exists Then tgtHeading = Common.GetHeadingToPoint(policeVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2.0F), policeVehicle.Position)

                                            If s.Exists AndAlso policeVehicle.Exists Then s.Tasks.FollowNavigationMeshToPosition(policeVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2.0F), tgtHeading, 2.2F, 2.0F).WaitForCompletion()

                                            If s.Exists AndAlso policeVehicle.Exists Then s.Tasks.EnterVehicle(policeVehicle, -1).WaitForCompletion()

                                            If s.Exists AndAlso policeVehicle.Exists Then s.Tasks.DriveToPosition(policeVehicle, World.GetNextPositionOnStreet(s.Position.Around(2000)), 25, VehicleDrivingFlags.Emergency, 50)
                                            If policeVehicle.Exists Then policeVehicle.IsSirenOn = True
                                            If policeVehicle.Exists Then policeVehicle.IsSirenSilent = False

                                            If s.Exists() Then
                                                pursuitInitiated = True
                                                pursuit = Common.CreatePursuit(True, True, True)
                                                s.AddToPursuit(pursuit)

                                                GameFiber.StartNew(
                                                Sub()
                                                    GameFiber.Sleep(3000)
                                                    Functions.SetPursuitDisableAI(pursuit, False)
                                                End Sub)
                                            End If
                                        Else
                                            'Cant find vehicle, so just stand there
                                            Try
                                                s.Tasks.PlayAnimation(New AnimationDictionary("amb@world_human_bum_standing@drunk@idle_a"), "idle_a", 1.0F, AnimationFlags.RagdollOnCollision)
                                            Catch ex As Exception
                                                Logger.LogVerboseDebug("Error playing drunk anim -- " & ex.Message)
                                            End Try
                                        End If
                                    End Sub)

                            End If
                        End If
                    End If
                End If
            End If
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            If mCode3Action = ECode3Action.Deleted Then
                Exit Sub
            End If

            Dim s As Suspect = GetPed("Suspect1")
            If s IsNot Nothing AndAlso s.Exists Then
                If Game.LocalPlayer.Character.IsDead Then
                    Exit Sub
                End If

                If State = CalloutState.UnitResponding Then
                    If officerRespondedCode3 = False Then
                        Code3Check(s)
                    End If
                ElseIf State = CalloutState.AtScene Then
                    If s.IsDead Then
                        s.DeleteBlip()
                        Radio.CallIsCode4(Me.ScriptInfo.Name)
                        [End]()
                    Else
                        If s.IsArrested Then
                            s.DeleteBlip()
                            Radio.CallIsCode4(Me.ScriptInfo.Name, True)
                            [End]()
                        Else
                            If Game.IsKeyDown(Config.EndCallKey) Then
                                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                                    Radio.CallIsCode4(Me.ScriptInfo.Name)
                                    [End]()
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End Sub

        Private Sub Code3Check(ByRef s As Suspect)
            If Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 150 Then
                If Game.LocalPlayer.Character.IsInAnyVehicle(False) = True Then
                    If Game.LocalPlayer.Character.CurrentVehicle IsNot Nothing Then
                        If Game.LocalPlayer.Character.CurrentVehicle.Exists Then
                            If Game.LocalPlayer.Character.CurrentVehicle.HasSiren Then
                                If Game.LocalPlayer.Character.CurrentVehicle.IsSirenOn Then
                                    If Game.LocalPlayer.Character.CurrentVehicle.IsSirenSilent = False Then
                                        officerRespondedCode3 = True

                                        If Game.LocalPlayer.Character.Position.DistanceTo(s.Position) < 75 Then
                                            mCode3Action = ECode3Action.Flee
                                            pursuitInitiated = True
                                            pursuit = Common.CreatePursuit()
                                            s.AddToPursuit(pursuit)
                                            Functions.SetPursuitIsActiveForPlayer(pursuit, True)
                                        Else
                                            mCode3Action = ECode3Action.Deleted
                                            s.DeleteBlip()
                                            s.Delete()
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End Sub

        Public Overrides Sub [End]()
            MyBase.[End]()
        End Sub

        Public Overrides ReadOnly Property RequiresSafePedPoint As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property ShowAreaBlipBeforeAccepting As Boolean
            Get
                Return True
            End Get
        End Property

    End Class

End Namespace