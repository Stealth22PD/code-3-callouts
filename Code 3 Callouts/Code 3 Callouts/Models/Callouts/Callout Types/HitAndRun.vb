Imports LSPD_First_Response
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Common.Extensions
Imports Stealth.Common.Models
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports System.Windows.Forms
Imports Stealth.Common.Models.QuestionWindow

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Hit and Run", CalloutProbability.Medium)>
    Public Class HitAndRun
        Inherits CalloutBase

        Dim VehModels As List(Of String) = {"Blista", "Jackal", "Oracle", "Asea", "Emperor", "Fugitive", "Ingot", "Premier", "Primo", "Stanier", "Stratum"}.ToList()
        Dim PedModels As List(Of String) = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01",
                                         "A_F_Y_GenHot_01", "A_F_Y_Hippie_01", "A_F_Y_Hipster_01", "A_F_Y_BevHills_01", "A_F_Y_BevHills_02", "A_F_M_Tourist_01", "A_F_M_FatWhite_01", "A_F_M_Business_02",
                                        "A_M_M_BevHills_01", "A_M_M_GenFat_01", "A_M_M_Business_01", "A_M_M_Golfer_01", "A_M_M_Skater_01", "A_M_M_Salton_01", "A_M_M_Tourist_01"}.ToList()

        Dim suspectIdentified As Boolean = False
        Dim spokenToMedics As Boolean = False
        Dim medicsCode4 As Boolean = False
        Dim suspectSearchArea As Blip
        Dim searchAreaRadius As Single = 150.0F
        Dim suspectLastSeen As Vector3
        Dim fullPlate As Boolean = False
        Dim licensePlateFactor As Integer
        Dim licensePlate As String = ""
        Dim lastLocationUpdate As DateTime = DateTime.Now
        Dim endTipDisplayed As Boolean = False

        Dim suspectCounterOn As Boolean = False

        Public Sub New()
            MyBase.New("Hit and Run", CallResponseType.Code_3)
            RadioCode = 480
            CrimeEnums = {DISPATCH.CRIMES.CODE_480, DISPATCH.CRIMES.HIT_AND_RUN}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "A pedestrian has been struck by a vehicle; the driver left the scene immediately, and did not stop."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "EMS has been dispatched to the scene; should arrive shortly. Do not leave until EMS arrives."

            Objective = "Speak to the ~o~witness~w~, and wait for EMS!~n~Apprehend the ~r~suspect!"
        End Sub

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim baseReturn As Boolean

            Try
                baseReturn = MyBase.OnBeforeCalloutDisplayed()

                If baseReturn = False Then
                    Return False
                End If

                Dim pedSpawn As Vector3 = PedHelper.GetPedSpawnPoint(World.GetNextPositionOnStreet(SpawnPoint.Around(3)))
                Dim ped As New Victim("Victim1", PedModels(gRandom.Next(PedModels.Count)), pedSpawn, 0)
                ped.DisplayName = "Pedestrian"
                'ped.CreateBlip()
                'ped.Tasks.PlayAnimation(New AnimationDictionary("amb@world_human_bum_slumped@male@laying_on_left_side@idle_a"), "idle_a", 1.0F, AnimationFlags.StayInEndFrame)
                ped.MakePersistent()
                ped.Kill()
                Peds.Add(ped)

                Dim wSpawn As Vector3 = PedHelper.GetPedSpawnPoint(World.GetNextPositionOnStreet(SpawnPoint.Around(10)))
                Dim w As New Witness("Witness1", PedModels(gRandom.Next(PedModels.Count)), wSpawn, 0)
                w.DisplayName = "Witness"
                w.MakePersistent()
                w.BlockPermanentEvents = True
                Peds.Add(w)

                If Common.IsComputerPlusRunning() Then
                    AddPedToCallout(ped)
                    AddPedToCallout(w)
                End If
            Catch ex As Exception
                Logger.LogTrivialDebug(ex.Message)
                Logger.LogTrivialDebug(ex.StackTrace)
            End Try

            Try
                'Dim directions As Vector3() = {Vector3.WorldNorth, Vector3.WorldSouth, Vector3.WorldEast, Vector3.WorldWest}
                'Dim mDirection As Vector3 = directions(gRandom.Next(directions.Length))
                'Dim suspectSpawn As Vector3 = World.GetNextPositionOnStreet(SpawnPoint)

                Dim suspectVehicle As New Vehicles.Vehicle(VehModels(gRandom.Next(VehModels.Count)), World.GetNextPositionOnStreet(SpawnPoint.Around(250)), gRandom.Next(360))
                suspectVehicle.MakePersistent()
                suspectVehicle.Name = "SuspectVehicle"
                suspectVehicle.SetRandomColor()
                DamageVehicle(suspectVehicle)

                If Common.IsTrafficPolicerRunning() Then
                    TrafficPolicerFunctions.SetVehicleInsuranceStatus(suspectVehicle, False)
                End If

                Vehicles.Add(suspectVehicle)

                Dim driver As New Suspect("Suspect1", PedModels(gRandom.Next(PedModels.Count)), World.GetNextPositionOnStreet(suspectVehicle.Position.Around(3)), 0, False)
                driver.Name = "Suspect1"
                driver.MakePersistent()
                driver.Tasks.ClearImmediately()
                driver.WarpIntoVehicle(suspectVehicle, -1)
                driver.Tasks.CruiseWithVehicle(suspectVehicle, 12, VehicleDrivingFlags.Normal)
                driver.SetDrunkRandom()

                driver.QAItems = New List(Of QAItem)

                Dim mStory As Integer = gRandom.Next(4)

                Select Case mStory
                    Case 0
                        driver.QAItems.Add(New QAItem("Do you know what happened?", "Yes, officer. I...I'm so sorry."))
                        driver.QAItems.Add(New QAItem("Why did you leave the scene?", "I was scared...I don't have insurance."))
                        driver.QAItems.Add(New QAItem("Have you been drinking today?", "Just a...um...no?"))
                        driver.QAItems.Add(New QAItem("Why didn't you come back to the scene?", "I don't know. I didn't want to get in trouble."))
                        driver.QAItems.Add(New QAItem("Do you have anything else to say?", "Am I going to go to jail?"))
                    Case 1
                        driver.QAItems.Add(New QAItem("Do you know what happened?", "That pedestrian jumped in front of me!"))
                        driver.QAItems.Add(New QAItem("Why did you leave the scene?", "Leave the scene? Why do I need to stop?"))
                        driver.QAItems.Add(New QAItem("Have you been drinking today?", "Don't you have anything better to do?"))
                        driver.QAItems.Add(New QAItem("Why didn't you come back to the scene?", "Come back? I was going home!"))
                        driver.QAItems.Add(New QAItem("Do you have anything else to say?", "Why are you hassling me for no reason?!"))
                    Case 2
                        driver.QAItems.Add(New QAItem("Do you know what happened?", "Yeah! Polecat released another video!!"))
                        driver.QAItems.Add(New QAItem("Why did you leave the scene?", "Because I don't slow down for nobody!"))
                        driver.QAItems.Add(New QAItem("Have you been drinking today?", "No, but I think I need one after seeing your ugly mug."))
                        driver.QAItems.Add(New QAItem("Why didn't you come back to the scene?", "Ain't nobody got TIME for that!!"))
                        driver.QAItems.Add(New QAItem("Do you have anything else to say?", "Are you always such an asshole?"))
                    Case Else
                        driver.QAItems.Add(New QAItem("Do you know what happened?", "Look what that idiot did to my car!!"))
                        driver.QAItems.Add(New QAItem("Why did you leave the scene?", "I don't have time for that shit!!"))
                        driver.QAItems.Add(New QAItem("Have you been drinking today?", "Why don't you breathalyze me, pig?"))
                        driver.QAItems.Add(New QAItem("Why didn't you come back to the scene?", "Are you kidding? I'm late for an appointment!!"))
                        driver.QAItems.Add(New QAItem("Do you have anything else to say?", "Can I leave now? I'm late!!"))
                End Select

                Peds.Add(driver)
                suspectLastSeen = driver.Position
            Catch ex As Exception
                Logger.LogTrivialDebug(ex.Message)
                Logger.LogTrivialDebug(ex.StackTrace)
            End Try

            If PerformPedChecks() Then
                Return baseReturn
            Else
                Return False
            End If
        End Function

        Public Overrides Function OnCalloutAccepted() As Boolean
            Radio.DispatchMessage("~g~EMS ~w~has been dispatched to the scene", True)

            Dim suspectVeh As Vehicles.Vehicle = GetVehicle("SuspectVehicle")
            If suspectVeh IsNot Nothing AndAlso suspectVeh.Exists Then
                suspectVeh.FillColorValues()
            End If

            Return MyBase.OnCalloutAccepted()
        End Function

        Public Overrides Sub OnArrivalAtScene()
            MyBase.OnArrivalAtScene()

            LSPD_First_Response.Mod.API.Functions.RequestBackup(Game.LocalPlayer.Character.Position, EBackupResponseType.Code3, EBackupUnitType.Ambulance)

            Dim vic As Victim = GetPed("Victim1")

            Dim suspectVeh As Vehicles.Vehicle = GetVehicle("SuspectVehicle")

            Dim w As Witness = GetPed("Witness1")
            If w IsNot Nothing AndAlso w.Exists Then
                w.SpeechLines = New List(Of String)
                w.SpeechLines.Add("I saw what happened, Officer!")
                w.SpeechLines.Add("That car just hit the poor pedestrian and took off!")
                w.SpeechLines.Add("The windshield shattered, and the car has a lot of front-end damage.")
                w.SpeechLines.Add(String.Format("The car was a {0} colored {1}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name))

                licensePlateFactor = gRandom.Next(3)
                If licensePlateFactor = 0 Then
                    w.SpeechLines.Add(String.Format("The license plate number was {0}.", suspectVeh.LicensePlate))
                    licensePlate = suspectVeh.LicensePlate
                    fullPlate = True
                ElseIf licensePlateFactor = 1 Then
                    w.SpeechLines.Add(String.Format("The first three digits of the license plate were {0}.", suspectVeh.LicensePlate.Substring(0, 3)))
                    licensePlate = suspectVeh.LicensePlate.Substring(0, 3)
                Else
                    Dim idx As Integer = suspectVeh.LicensePlate.Length - 3
                    w.SpeechLines.Add(String.Format("The last three digits of the license plate were {0}.", suspectVeh.LicensePlate.Substring(idx)))
                    licensePlate = suspectVeh.LicensePlate.Substring(idx)
                End If

                w.CreateBlip()
                w.TurnToFaceEntity(Game.LocalPlayer.Character)
                Game.DisplaySubtitle("Witness: Officer!! Over here!!", 8000)
            End If

            SuspectSearch = SuspectSearchStateEnum.Null
            Game.DisplayHelp("Press " & Config.SpeakKey.ToString() & " to talk to the witness.", 8000)
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            If Game.LocalPlayer.Character.IsDead Then
                Exit Sub
            End If

            Dim suspectVeh As Vehicles.Vehicle = GetVehicle("SuspectVehicle")
            'Dim ambu As Vehicles.Vehicle = GetVehicle("Ambulance")
            'Dim m1 As Witness = GetPed("Medic1")
            'Dim m2 As Witness = GetPed("Medic2")
            Dim vic As Victim = GetPed("Victim1")
            Dim w As Witness = GetPed("Witness1")
            Dim s As Suspect = GetPed("Suspect1")

            If State = CalloutState.UnitResponding Then
                If w.IsDead Then
                    Dim wSpawn As Vector3 = PedHelper.GetPedSpawnPoint(World.GetNextPositionOnStreet(SpawnPoint.Around(10)))
                    w.Position = wSpawn
                    w.Resurrect()
                End If
            End If

            If Game.IsKeyDown(Config.SpeakKey) Then
                SpeakToSubject(w)
            End If

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    Radio.CallIsCode4(Me.ScriptInfo.Name)
                    [End]()
                End If
            End If

            If State = CalloutState.AtScene Then
                If s Is Nothing OrElse s.Exists = False Then
                    Game.DisplayNotification("Hit and Run callout crashed.")
                    Logger.LogTrivial("Error occurred - Suspect no longer exists; possibly despawned by GTA V?")
                    Radio.CallIsCode4(Me.ScriptInfo.Name)
                    [End]()
                    Exit Sub
                End If

                If s.IsArrested Then
                    If endTipDisplayed = False Then
                        endTipDisplayed = True

                        GameFiber.StartNew(
                           Sub()
                               AddPedToCallout(s)
                               Game.DisplayHelp("Ensure that you question the suspect using the interaction menu.", 5000)
                               GameFiber.Sleep(5000)
                               Game.DisplayHelp("Press ~b~" & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " ~w~to end this callout when the situation is over.", 8000)
                           End Sub)
                    End If
                Else
                    If s.IsDead Then
                        Radio.CallIsCode4(Me.ScriptInfo.Name, s.IsArrested)
                        [End]()
                    End If
                End If

                If SuspectSearch = SuspectSearchStateEnum.Null Then
                    If suspectIdentified = False AndAlso w.HasSpoken Then
                        suspectIdentified = True
                        suspectLastSeen = suspectVeh.Position
                        CreateSearchArea(suspectLastSeen)
                        SuspectSearch = SuspectSearchStateEnum.NotYetLocated

                        Dim mUpdate As String = ""

                        If fullPlate Then
                            mUpdate += String.Format("UPDATE: Vehicle was a {0} colored {1}; License # {2}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate)
                        Else
                            mUpdate += String.Format("UPDATE: Vehicle was a {0} colored {1}; Partial license {2}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate)
                        End If

                        If Common.IsComputerPlusRunning() Then
                            AddVehicleToCallout(suspectVeh)
                            ComputerPlusFunctions.AddUpdateToCallout(CalloutID, mUpdate)
                        End If

                        If fullPlate = True Then
                            Radio.UnitMessage(String.Format("Suspect vehicle is a {0} {1}, License # {2}", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate))
                        Else
                            Radio.UnitMessage(String.Format("Suspect vehicle is a {0} {1}, Partial license # {2}", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate))
                        End If

                        Radio.DispatchMessage("Roger", True)
                        Game.DisplayHelp("Search the area for the suspect.")

                        Dim pAudio As String = "SUSPECT_LAST_SEEN IN_OR_ON_POSITION"

                        Dim mHeading As String = Common.GetDirectionAudioFromHeading(suspectVeh.Heading)
                        If mHeading <> "" Then
                            pAudio = String.Format("SUSPECT_LAST_SEEN DIR_HEADING {0} IN_OR_ON_POSITION", mHeading)
                        End If

                        AudioPlayerEngine.PlayAudio(pAudio, suspectLastSeen)
                    End If
                End If

                If SuspectSearch = SuspectSearchStateEnum.NotYetLocated Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(suspectVeh.GetOffsetPosition(Vector3.RelativeBack * 2)) <= 15 Then

                        If suspectCounterOn = False Then
                            suspectCounterOn = True
                            Dim startTime As DateTime = DateTime.Now

                            GameFiber.StartNew(
                                Sub()
                                    While Game.LocalPlayer.Character.Position.DistanceTo(suspectVeh.GetOffsetPosition(Vector3.RelativeBack * 2)) <= 15
                                        GameFiber.Yield()

                                        Dim ts As TimeSpan = DateTime.Now - startTime

                                        If ts.TotalSeconds >= 3 Then
                                            SuspectSearch = SuspectSearchStateEnum.Located
                                            DeleteSearchArea()
                                            'suspectVeh.CreateBlip(Drawing.Color.Yellow)
                                            s.CreateBlip()
                                            Radio.SuspectSpotted()

                                            Exit While
                                        End If
                                    End While

                                    suspectCounterOn = False
                                End Sub)
                        End If

                    End If

                    If suspectSearchArea IsNot Nothing AndAlso suspectSearchArea.Exists AndAlso suspectCounterOn = False Then
                        Dim ts As TimeSpan = DateTime.Now - lastLocationUpdate

                        If ts.TotalSeconds > 30 Then
                            GameFiber.StartNew(
                                Sub()
                                    suspectLastSeen = suspectVeh.Position
                                    suspectSearchArea.Position = suspectVeh.Position
                                    lastLocationUpdate = DateTime.Now

                                    If fullPlate = True Then
                                        Radio.DispatchMessage(String.Format("License # ~b~{0} ~w~captured by ALPR camera, over", licensePlate), True)
                                    Else
                                        Radio.DispatchMessage(String.Format("Partial license ~b~{0} ~w~captured by ALPR camera, over", licensePlate), True)
                                    End If

                                    Dim pAudio As String = "SUSPECT_LAST_SEEN IN_OR_ON_POSITION"

                                    Dim mHeading As String = Common.GetDirectionAudioFromHeading(suspectVeh.Heading)
                                    If mHeading <> "" Then
                                        pAudio = String.Format("SUSPECT_LAST_SEEN DIR_HEADING {0} IN_OR_ON_POSITION", mHeading)
                                    End If

                                    AudioPlayerEngine.PlayAudio(pAudio, suspectVeh.Position)

                                    Game.DisplayHelp("The search area has been updated.")

                                    Dim tempBlip As New Blip(s.Position)
                                    tempBlip.Color = Drawing.Color.Red

                                    GameFiber.Sleep(3000)

                                    If tempBlip IsNot Nothing AndAlso tempBlip.IsValid() Then
                                        tempBlip.Delete()
                                    End If
                                End Sub)
                        End If
                    End If

                End If
            End If
        End Sub

        Public Overrides Sub [End]()
            Dim vic As Victim = GetPed("Victim1")
            Dim ambu As Vehicles.Vehicle = GetVehicle("Ambulance")
            Dim m1 As Witness = GetPed("Medic1")
            Dim m2 As Witness = GetPed("Medic2")

            If vic IsNot Nothing AndAlso vic.Exists Then
                vic.Delete()
            End If

            If m1 IsNot Nothing AndAlso m1.Exists Then
                m1.Delete()
            End If

            If m2 IsNot Nothing AndAlso m2.Exists Then
                m2.Delete()
            End If

            If ambu IsNot Nothing AndAlso ambu.Exists Then
                ambu.Delete()
            End If

            DeleteSearchArea()
            MyBase.[End]()
        End Sub

        Private Sub SpeakToSubject(ByRef w As Witness)
            'If m1 IsNot Nothing AndAlso m1.Exists Then
            '    If Game.LocalPlayer.Character.Position.DistanceTo(m1.Position) < 3 Then
            '        m1.Speak()
            '        spokenToMedics = True
            '        Exit Sub
            '    End If
            'Else
            '    Game.DisplayNotification("Hit and Run Callout crashed")
            '    [End]()
            'End If

            'If m2 IsNot Nothing AndAlso m2.Exists Then
            '    If Game.LocalPlayer.Character.Position.DistanceTo(m2.Position) < 3 Then
            '        m2.Speak()
            '        spokenToMedics = True
            '        Exit Sub
            '    End If
            'Else
            '    Game.DisplayNotification("Hit and Run Callout crashed")
            '    [End]()
            'End If

            If w IsNot Nothing AndAlso w.Exists Then
                If Game.LocalPlayer.Character.Position.DistanceTo(w.Position) < 3 Then
                    w.Speak()
                    Exit Sub
                End If
            Else
                Game.DisplayNotification("Hit and Run Callout crashed")
                [End]()
            End If
        End Sub

        Private Sub CreateSearchArea(ByVal pSpawnPoint As Vector3)
            suspectSearchArea = New Blip(pSpawnPoint, searchAreaRadius)
            suspectSearchArea.Color = Drawing.Color.FromArgb(100, Drawing.Color.Yellow)
            suspectSearchArea.StopFlashing()
        End Sub

        Private Sub DeleteSearchArea()
            Try
                If suspectSearchArea IsNot Nothing Then
                    suspectSearchArea.Delete()
                End If
            Catch ex As Exception
                Logger.LogTrivialDebug("Error deleting search area -- " & ex.Message)
            End Try
        End Sub

        Private Sub DamageVehicle(ByRef v As Vehicle)
            With v
                Dim radius As Single = gRandom.Next(300, 500)
                Dim damageFactor As Single = gRandom.Next(200, 300)

                'Try
                '    .Windows.Item(0).Smash()
                'Catch ex As Exception
                'End Try

                .Deform(Vector3.RelativeFront, radius, damageFactor)

                Dim health As Integer = gRandom.Next(500, 700)
                .Health = health
                .EngineHealth = health
                .FuelTankHealth = health
            End With
        End Sub

        Private SuspectSearch As SuspectSearchStateEnum = SuspectSearchStateEnum.Null
        Enum SuspectSearchStateEnum
            Null = 0
            NotYetLocated = 1
            Located = 2
            Escaped = 3
        End Enum

        Public Overrides ReadOnly Property RequiresSafePedPoint As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property ShowAreaBlipBeforeAccepting As Boolean
            Get
                Return True
            End Get
        End Property

    End Class

End Namespace