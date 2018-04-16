Imports LSPD_First_Response
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Common
Imports Stealth.Common.Extensions
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports System.Windows.Forms
Imports LSPD_First_Response.Engine.Scripting

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Unknown Trouble", CalloutProbability.High)>
    Public Class UnknownTrouble
        Inherits CalloutBase

        Dim lionPrey As Ped = Nothing
        Dim VictimModels As String() = {"A_F_Y_GenHot_01", "A_F_Y_Hippie_01", "A_F_Y_Hipster_01", "A_F_Y_BevHills_01", "A_F_Y_BevHills_02", "A_F_M_Tourist_01", "A_F_M_FatWhite_01", "A_F_M_Business_02"}
        Dim SuspectModels As String() = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01",
                                         "A_M_M_BevHills_01", "A_M_M_GenFat_01", "A_M_M_Business_01", "A_M_M_Golfer_01", "A_M_M_Skater_01", "A_M_M_Salton_01", "A_M_M_Tourist_01"}

        Dim PedModels As String() = {"A_F_Y_GenHot_01", "A_F_Y_Hippie_01", "A_F_Y_Hipster_01", "A_F_Y_BevHills_01", "A_F_Y_BevHills_02", "A_F_M_Tourist_01", "A_F_M_FatWhite_01", "A_F_M_Business_02",
                                           "A_M_M_BevHills_01", "A_M_M_GenFat_01", "A_M_M_Business_01", "A_M_M_Golfer_01", "A_M_M_Skater_01", "A_M_M_Salton_01", "A_M_M_Tourist_01"}

        Dim searchArea As Blip
        Dim foundDB As Boolean = False
        Dim detectiveDispatched = False
        Dim detName As String = ""
        Dim vicName As String = ""
        Dim vicPosition As Vector3
        Dim vicHeading As Single
        Dim startedInspect As DateTime = DateTime.MinValue
        Dim detUnitNumber As String = ""

        Public Sub New()
            MyBase.New("Unknown Trouble", CallResponseType.Code_2)
            LionSearchState = LionSearchStateEnum.Null
            RadioCode = 0
            CrimeEnums = {DISPATCH.CRIMES.CIV_ASSISTANCE}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "Caller sounded like they needed help, but line was disconnected."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "No answer on callback. No further details available at this time."

            Objective = "Attend to the scene.~n~Be prepared for anything!"
        End Sub

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim randomInt As Integer = gRandom.Next(1, 101)

            If randomInt <= 10 Then
                '10% chance
                TroubleType = TroubleTypeEnum.MountainLion

            ElseIf randomInt >= 11 AndAlso randomInt <= 50 Then
                '40% chance
                TroubleType = TroubleTypeEnum.FalseCall

            ElseIf randomInt >= 51 AndAlso randomInt <= 70 Then
                '20% chance
                TroubleType = TroubleTypeEnum.AssaultInProgress

            ElseIf randomInt >= 71 AndAlso randomInt <= 85 Then
                '15% chance
                TroubleType = TroubleTypeEnum.DeadBody
            Else
                '15% chance
                TroubleType = TroubleTypeEnum.None
            End If

            Return MyBase.OnBeforeCalloutDisplayed()
        End Function

        Public Overrides Function OnCalloutAccepted() As Boolean
            Dim validCall As Boolean

            Select Case TroubleType
                Case TroubleTypeEnum.MountainLion
                    validCall = SpawnLion()
                Case TroubleTypeEnum.FalseCall
                    validCall = SpawnFalseCall()
                Case TroubleTypeEnum.AssaultInProgress
                    validCall = SpawnAssault()
                Case TroubleTypeEnum.DeadBody
                    validCall = SpawnDeadBody()
                Case Else
                    validCall = True
            End Select

            If validCall = False Then
                Return False
            End If

            Return MyBase.OnCalloutAccepted()
        End Function

        Public Function SpawnLion() As Boolean
            Dim lion As Suspect = Nothing

            Dim lionSpawn As Vector3 = PedHelper.GetSafeCoordinatesForPed(World.GetNextPositionOnStreet(SpawnPoint.Around(gRandom.Next(50, 100))))
            If lionSpawn = Vector3.Zero Then
                lionSpawn = World.GetNextPositionOnStreet(SpawnPoint.Around(gRandom.Next(50, 100)))
            End If

            lion = New Suspect("Lion", New Model("a_c_mtlion"), lionSpawn, 0.0F, True)
            lion.RelationshipGroup = "COUGAR"
            Game.SetRelationshipBetweenRelationshipGroups("COUGAR", "PLAYER", Relationship.Hate)
            Game.SetRelationshipBetweenRelationshipGroups("COUGAR", "CIVMALE", Relationship.Hate)
            Game.SetRelationshipBetweenRelationshipGroups("COUGAR", "CIVFEMALE", Relationship.Hate)

            If lion IsNot Nothing Then
                If lion.Exists = False Then
                    Return False
                Else
                    lion.Tasks.Wander()
                    Peds.Add(lion)
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        Public Function SpawnFalseCall() As Boolean
            Logger.LogTrivialDebug("spawning false call")

            Dim callerSpawn As Vector3 = PedHelper.GetPedSpawnPoint(SpawnPoint.Around(10))

            Dim ped As New Victim("Caller1", PedModels(gRandom.Next(PedModels.Count)), callerSpawn, 0)
            ped.DisplayName = "Caller"

            Dim victimStory As Integer = gRandom.Next(7)

            If victimStory = 0 Then
                ped.SpeechLines.Add("Oh, Officer, thank god you're here!!")
                ped.SpeechLines.Add("He...he was after me!!")
                ped.SpeechLines.Add("The ghost...he was after me!")
                ped.SpeechLines.Add("He's always following me!!")
                ped.SpeechLines.Add("Please help me!!")
            ElseIf victimStory = 1 Then
                ped.SpeechLines.Add("Officer, I am SO sorry!")
                ped.SpeechLines.Add("I was heading to a friend's place, and my phone was in my pocket.")
                ped.SpeechLines.Add("My lock screen has an ""Emergency Call"" button.")
                ped.SpeechLines.Add("I don't know how, but it somehow got pressed.")
                ped.SpeechLines.Add("I'm sorry to have wasted your time!")
            ElseIf victimStory = 2 Then
                ped.SpeechLines.Add("It's about time you showed up!!")
                ped.SpeechLines.Add("I was at Up-n-Atom Burger earlier...")
                ped.SpeechLines.Add("I SPECIFICALLY asked for hot French Fries with my order!")
                ped.SpeechLines.Add("But they only gave me lukewarm! AND only 2 packets of Ketchup!")
                ped.SpeechLines.Add("Then the Manager asked me to leave the restaurant!")
                ped.SpeechLines.Add("I didn't even get my Sprunk!!!")
                ped.SpeechLines.Add("I want them all arrested!! NOW!!!")
            ElseIf victimStory = 3 Then
                ped.SpeechLines.Add("I can't get a cab right now.")
                ped.SpeechLines.Add("Can you drive me home?")
            ElseIf victimStory = 4 Then
                ped.SpeechLines.Add("Oh, Officer, thank god you're here!!")
                ped.SpeechLines.Add("My neighbour really pissed me off today.")
                ped.SpeechLines.Add("Can I borrow your gun?")
                ped.SpeechLines.Add("I'll give it back, I swear!")
            ElseIf victimStory = 5 Then
                ped.SpeechLines.Add("Oh, Officer, thank god you're here!!")
                ped.SpeechLines.Add("My phone's WiFi has stopped working!")
                ped.SpeechLines.Add("Can you fix it?")
            ElseIf victimStory = 6 Then
                ped.SpeechLines.Add("Hey, you're pretty cute...")
                ped.SpeechLines.Add("What's your number?")
            End If

            If ped.Exists Then
                Peds.Add(ped)
                Return True
            Else
                Return False
            End If
        End Function

        Public Function SpawnAssault() As Boolean
            Logger.LogTrivialDebug("spawning assault")

            Dim callerSpawn As Vector3 = PedHelper.GetPedSpawnPoint(SpawnPoint.Around(10))
            Dim suspectSpawn As Vector3 = PedHelper.GetPedSpawnPoint(callerSpawn.Around(10))

            Dim v As New Victim("Victim1", VictimModels(gRandom.Next(VictimModels.Count)), callerSpawn, 0)
            v.BlockPermanentEvents = True
            Dim s As New Suspect("Suspect1", SuspectModels(gRandom.Next(SuspectModels.Count)), suspectSpawn, 0, True)

            If v.Exists AndAlso s.Exists Then
                Peds.Add(v)
                Peds.Add(s)
                Return True
            Else
                Return False
            End If
        End Function

        Public Function SpawnDeadBody() As Boolean
            Dim pedSpawn As Vector3 = PedHelper.GetPedSpawnPoint(World.GetNextPositionOnStreet(SpawnPoint.Around(30)))
            vicPosition = pedSpawn
            Dim ped As New Victim("Victim1", PedModels(gRandom.Next(PedModels.Count)), pedSpawn, 0)
            ped.DisplayName = "Pedestrian"
            ped.MakePersistent()

            vicHeading = ped.Heading

            ped.Kill()

            'Dim vp As LSPD_First_Response.Engine.Scripting.Entities.Persona
            'vp = LSPD_First_Response.Mod.API.Functions.GetPersonaForPed(ped, vp)
            vicName = "John Doe"

            Peds.Add(ped)

            Return True
        End Function

        Public Overrides Sub Process()
            MyBase.Process()

            If Game.LocalPlayer.Character.IsDead Then
                Exit Sub
            End If

            Dim endCall As Boolean = True

            If TroubleType = TroubleTypeEnum.MountainLion Then
                endCall = ProcessLionCall()
            Else
                endCall = False
            End If

            If endCall = True Then
                [End]()
            End If

            If TroubleType = TroubleTypeEnum.DeadBody Then
                Dim v As Victim = GetPed("Victim1")

                If State = CalloutState.AtScene Then
                    If foundDB = False Then
                        If v.Exists() AndAlso Game.LocalPlayer.Character.Position.DistanceTo(v.Position) < 15 Then
                            foundDB = True

                            v.CreateBlip()

                            If searchArea IsNot Nothing Then
                                If searchArea.Exists Then
                                    searchArea.Delete()
                                End If
                            End If

                            Game.DisplayHelp("Press " & Config.SpeakKey.ToString() & " to call for a homicide detective.")
                        End If
                    Else
                        MonitorDetective()
                    End If
                End If
            End If

            If TroubleType = TroubleTypeEnum.AssaultInProgress Then
                Dim v As Victim = GetPed("Victim1")
                Dim s As Suspect = GetPed("Suspect1")

                If s IsNot Nothing AndAlso v IsNot Nothing Then
                    If s.Exists AndAlso v.Exists Then
                        If s.IsArrested() Then
                            v.DeleteBlip()
                            v.Tasks.Clear()
                            v.Dismiss()
                        End If
                    End If
                End If
            End If

            If Game.IsKeyDown(Config.SpeakKey) Then
                If TroubleType = TroubleTypeEnum.FalseCall Then
                    Dim v As Victim = GetPed("Caller1")

                    If v IsNot Nothing And v.Exists Then
                        If Game.LocalPlayer.Character.Position.DistanceTo(v.Position) < 3 Then
                            v.Speak()
                        End If
                    End If
                ElseIf TroubleType = TroubleTypeEnum.DeadBody Then
                    If foundDB = True Then
                        If detectiveDispatched = False Then
                            detectiveDispatched = True
                            SpawnHomicideDetective()
                            RadioForDetective()
                        End If
                    End If
                End If
            End If
        End Sub

        Private Sub RadioForDetective()
            GameFiber.StartNew(
                Sub()
                    Game.DisplayNotification(String.Format("~b~{0}: ~w~{0}, I've got a dead body, possible 187. Requesting a detective, over.", Common.gUnitNumber))
                    Radio.PlayRadioAnimation()

                    DispatchMessage("Roger.", True)

                    Dim pAudio As New List(Of AudioFile)

                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.DIVISION.DIV_01))
                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.UNIT_TYPE.HENRY))
                    Dim units As Array = [Enum].GetValues(GetType(DISPATCH.BEAT))
                    Dim iDetUnit As Integer = gRandom.Next(units.Length)
                    Dim detUnit As DISPATCH.BEAT = CType(units.GetValue(iDetUnit), DISPATCH.BEAT)
                    pAudio.Add(New AudioFile("DISPATCH", detUnit))

                    detUnitNumber = "1-HENRY-" & (iDetUnit + 1)

                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORTING.OFFICERS_REPORT))

                    If gRandom.Next(1) = 0 Then
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CRIMES.CODE_187))
                    Else
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CRIMES.HOMICIDE))
                    End If

                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CONJUNCTIVES.IN_OR_ON_POSITION))

                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.RESPOND_CODE_2))

                    Dim responseInt As Integer = gRandom.Next(3)
                    If responseInt = 1 Then
                        pAudio.Add(New AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.ROGER_EN_ROUTE))
                    ElseIf responseInt = 2 Then
                        pAudio.Add(New AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.ROGER_ON_OUR_WAY))
                    Else
                        pAudio.Add(New AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.COPY_IN_VICINITY))
                    End If

                    Dim rogerInt As Integer = gRandom.Next(4)
                    If rogerInt = 0 Then
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER))
                    ElseIf rogerInt = 1 Then
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT))
                    ElseIf rogerInt = 2 Then
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR))
                    Else
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT))
                    End If

                    AudioPlayerEngine.PlayAudio(pAudio, SpawnPoint)

                    Game.DisplayHelp("Wait for the Detective before calling the Coroner.")
                    Dim detMsg As String = String.Format("~b~{0}: ~w~{0} to {1}, don't move the body until I get there, over.", detUnitNumber, Common.gUnitNumber)
                    Game.DisplayNotification(detMsg)
                End Sub)
        End Sub

        Private Sub SpawnHomicideDetective()
            Dim det As New Cop("Detective", "s_m_m_ciasec_01", World.GetNextPositionOnStreet(SpawnPoint.Around(150)), 0)
            det.MakePersistent()
            det.RelationshipGroup = "COP"
            det.DisplayName = "Detective"
            det.BlockPermanentEvents = True
            det.CreateBlip()

            DetectiveState = EDetectiveState.Created

            Dim DoB As Date = Common.GetRandomDateOfBirth()
            Dim name As String = Entities.Persona.GetRandomFullName()
            Dim nameParts As String() = name.Split(" ")
            detName = nameParts(1)
            Dim p As New Entities.Persona(det, Gender.Male, DoB, 0, nameParts(0), nameParts(1), Entities.ELicenseState.Valid, 0, False, False, True)
            det.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 56, False)
            LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(det, p)

            Dim unmarkedPoliceCars As String() = {"FBI", "POLICE4"}
            Dim detectiveVeh As New Vehicles.Vehicle(unmarkedPoliceCars(gRandom.Next(unmarkedPoliceCars.Length)), World.GetNextPositionOnStreet(det.Position.Around(10)), 0)
            det.WarpIntoVehicle(detectiveVeh, -1)
            detectiveVeh.IsSirenOn = True
            detectiveVeh.IsSirenSilent = True
            detectiveVeh.Name = "DetectiveUnit"
            detectiveVeh.MakePersistent()

            DetectiveState = EDetectiveState.Dispatched

            Dim detTarget As Vector3 = Game.LocalPlayer.Character.Position.Around(15)
            det.Tasks.DriveToPosition(detTarget, 15, (VehicleDrivingFlags.Emergency))

            Peds.Add(det)
            Vehicles.Add(detectiveVeh)
            PedsToIgnore.Add(det.Handle)

            DetectiveState = EDetectiveState.Responding

            Logger.LogTrivialDebug("Detective responding")
        End Sub

        Private Sub MonitorDetective()
            Dim d As Cop = GetPed("Detective")
            Dim v As Victim = GetPed("Victim1")

            If d IsNot Nothing AndAlso d.Exists Then
                Select Case DetectiveState
                    Case EDetectiveState.Responding
                        If d.Position.DistanceTo(Game.LocalPlayer.Character.Position) <= 25 Then
                            d.Tasks.Clear()

                            If d.CurrentVehicle IsNot Nothing AndAlso d.CurrentVehicle.Exists Then
                                If d.CurrentVehicle.HasSiren Then
                                    d.CurrentVehicle.BlipSiren(True)
                                End If

                                d.Tasks.ParkVehicle(d.CurrentVehicle.Position, d.CurrentVehicle.Heading)
                            End If
                            Logger.LogTrivialDebug("Parked vehicle")

                            d.Tasks.LeaveVehicle(LeaveVehicleFlags.None)
                            DetectiveState = EDetectiveState.AtScene


                            'GameFiber.StartNew(
                            '    Sub()
                            '        Logger.LogTrivialDebug("Sleeping")
                            '        GameFiber.Sleep(8000)
                            '        Logger.LogTrivialDebug("Introducing himself")
                            '        Game.DisplaySubtitle(text, 6000)
                            '    End Sub)
                        End If
                    Case EDetectiveState.AtScene
                        d.Tasks.Clear()
                        DetectiveState = EDetectiveState.WalkingToPlayer

                        GameFiber.StartNew(
                                Sub()
                                    d.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 3.0F, 0.0F, 1.5F).WaitForCompletion()
                                    d.TurnToFaceEntity(Game.LocalPlayer.Character)

                                    While True
                                        If d.Position.DistanceTo(Game.LocalPlayer.Character.Position) <= 4.0F Then
                                            DetectiveState = EDetectiveState.SpeakingToPlayer

                                            Dim text As String = String.Format("~b~Detective: ~w~Hey, Detective {0}, Homicide. What do ya got?", detName)
                                            Game.DisplaySubtitle(text, 6000)
                                            Exit While
                                        End If

                                        GameFiber.Yield()
                                    End While
                                End Sub)

                    Case EDetectiveState.SpeakingToPlayer
                        'Logger.LogTrivialDebug("Walking to victim")

                        GameFiber.StartNew(
                            Sub()
                                GameFiber.Sleep(5000)

                                Logger.LogTrivialDebug("Walking to victim")
                                d.Tasks.GoToOffsetFromEntity(v, 3.0F, 0.0F, 1.5F)
                                DetectiveState = EDetectiveState.WalkingToVictim
                            End Sub)

                    Case EDetectiveState.WalkingToVictim

                        If d.Position.DistanceTo(vicPosition) <= 4.0F Then
                            Logger.LogTrivialDebug("Close to vic, he should now inspect")
                            DetectiveState = EDetectiveState.AboutToInspectVictim
                        End If
                    Case EDetectiveState.AboutToInspectVictim
                        Logger.LogTrivialDebug("About to inspect vic")
                        DetectiveState = EDetectiveState.KneelingToInspect

                    Case EDetectiveState.KneelingToInspect
                        GameFiber.StartNew(
                            Sub()
                                'Dim v3DetToVic As Vector3 = (vicPosition - d.Position)
                                'v3DetToVic.Normalize()
                                'Dim hdg As Single = MathHelper.ConvertDirectionToHeading(v3DetToVic)

                                d.Tasks.Clear()
                                'd.Tasks.AchieveHeading(hdg)
                                d.TurnToFaceEntity(v)
                                GameFiber.Sleep(1000)

                                startedInspect = DateTime.Now
                                d.Tasks.PlayAnimation("amb@medic@standing@kneel@idle_a", "idle_a", 1.0F, AnimationFlags.Loop)
                                Logger.LogTrivialDebug("Should be animated")
                                DetectiveState = EDetectiveState.InspectingVictim
                            End Sub)

                    Case EDetectiveState.InspectingVictim
                        If startedInspect = DateTime.MinValue Then
                            Logger.LogTrivialDebug("Wtf? No time set?")
                            Exit Sub
                        Else
                            If DateDiff(DateInterval.Second, startedInspect, DateTime.Now) < 6 Then
                                Logger.LogTrivialDebug("Time not expired yet")
                                Exit Sub
                            Else
                                DetectiveState = EDetectiveState.Done
                                Logger.LogTrivialDebug("Time expired")

                                GameFiber.StartNew(
                                    Sub()
                                        d.Tasks.Clear()

                                        'Game.DisplaySubtitle(String.Format("~b~Detective: ~w~Alright, victim's ID says {0}. Not sure how long the body has been here.", vicName), 3000)
                                        'Game.DisplayHelp("Run the victim's name for the case file, then call the Coroner.")
                                        Game.DisplaySubtitle("~b~Detective: ~w~Got a GSW...no real signs of a struggle. Not sure how long the body has been here.", 3000)

                                        GameFiber.Sleep(3000)
                                        Game.DisplaySubtitle("~b~Detective: ~w~I'll get started on the case file. You can call the Coroner.", 3000)
                                        Game.DisplayHelp("You can end this callout by pressing " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & ".")

                                        Logger.LogTrivialDebug("Cleared to call coroner")

                                        Dim dv As Vehicles.Vehicle = GetVehicle("DetectiveUnit")
                                        If dv IsNot Nothing AndAlso dv.Exists Then
                                            Logger.LogTrivialDebug("Going back to his car")
                                            d.Tasks.ClearImmediately()

                                            Dim v3DetToCar As Vector3 = (dv.Position - d.Position)
                                            v3DetToCar.Normalize()
                                            Dim hdg As Single = MathHelper.ConvertDirectionToHeading(v3DetToCar)

                                            d.Tasks.FollowNavigationMeshToPosition(dv.GetOffsetPosition(Vector3.RelativeLeft * 1.5F), hdg, 2.0F).WaitForCompletion()
                                            'd.Tasks.GoToOffsetFromEntity(dv, 3.0F, 90, 1.5F).WaitForCompletion()
                                            d.Tasks.EnterVehicle(dv, -1)
                                        Else
                                            Logger.LogTrivialDebug("vehicle doesnt exist?!?!?")
                                        End If
                                    End Sub)
                            End If
                        End If
                    Case EDetectiveState.Done
                        If v Is Nothing OrElse v.Exists = False Then
                            Radio.CallIsCode4(Me.ScriptInfo.Name)
                            [End]()
                        End If
                End Select
            End If
        End Sub

        Public Overrides Sub OnArrivalAtScene()
            Try
                MyBase.OnArrivalAtScene()

                Select Case TroubleType
                    Case TroubleTypeEnum.MountainLion
                        ReportMountainLion()
                    Case TroubleTypeEnum.FalseCall
                        Dim v As Victim = GetPed("Caller1")
                        v.CreateBlip()
                        v.TurnToFaceEntity(Game.LocalPlayer.Character)

                        Game.DisplayHelp("Speak to the caller by pressing " & Config.SpeakKey.ToString() & ". Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout.")
                    Case TroubleTypeEnum.AssaultInProgress
                        Game.DisplayHelp("Investigate the area. Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout.")
                        Game.DisplaySubtitle("~y~Victim: ~w~OFFICER!! HELP ME!! HE'S GOT A KNIFE!!!", 6000)

                        Dim v As Victim = GetPed("Victim1")
                        Dim s As Suspect = GetPed("Suspect1")
                        s.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_KNIFE"), 56, True)

                        Try
                            Natives.Peds.ReactAndFleePed(v, s)
                        Catch ex As Exception
                            Logger.LogTrivialDebug("Error fleeing from ped -- " & ex.Message)
                        End Try

                        s.AttackPed(v)

                        v.CreateBlip()
                        s.CreateBlip()
                    Case Else
                        Game.DisplayHelp("Investigate the area. Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout.")
                        searchArea = New Blip(SpawnPoint, 50)
                        If searchArea IsNot Nothing AndAlso searchArea.Exists() Then searchArea.Color = Drawing.Color.FromArgb(100, Drawing.Color.Yellow)
                End Select
            Catch ex As Exception
                Radio.CallIsCode4(Me.ScriptInfo.Name)
                [End]()
                Logger.LogVerbose("Unknown Trouble callout crashed -- " & ex.Message)
                Logger.LogVerbose(ex.ToString())
            End Try
        End Sub

        Function ProcessLionCall() As Boolean
            Dim endCall As Boolean = True
            Dim lion As Suspect = GetPed("Lion")

            If lion IsNot Nothing Then
                If lion.Exists = True Then
                    If lion.IsDead = False Then
                        endCall = False

                        Select Case LionSearchState
                            Case LionSearchStateEnum.Null
                                endCall = False
                            Case LionSearchStateEnum.NotYetLocated
                                endCall = False
                                ProcessLion()
                                LionAttack()
                            Case LionSearchStateEnum.LionLocated
                                endCall = False
                                LionAttack()
                            Case LionSearchStateEnum.LionEscaped
                                Game.DisplayNotification("The mountain lion has escaped.")
                                DeleteLion()
                                endCall = True
                        End Select
                    Else
                        If lion IsNot Nothing Then
                            If lion.Exists Then
                                lion.DeleteBlip()
                            End If
                        End If
                        LionSearchState = LionSearchStateEnum.LionIsDead
                        endCall = True
                        Radio.CallIsCode4(Me.ScriptInfo.Name)
                    End If
                Else
                End If
            End If

            Return endCall
        End Function

        Sub ProcessLion()
            Dim lion As Suspect = GetPed("Lion")

            If lion IsNot Nothing Then
                If lion.Exists = True AndAlso LionSearchState = LionSearchStateEnum.NotYetLocated Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(lion.Position) < 60 Then
                        LionSearchState = LionSearchStateEnum.LionLocated
                    Else
                        If lion.Position.DistanceTo(lion.OriginalSpawnPoint) > 150 Then
                            LionSearchState = LionSearchStateEnum.LionEscaped
                        End If
                    End If
                Else
                    LionSearchState = LionSearchStateEnum.LionEscaped
                End If
            End If
        End Sub

        Sub LionAttack()
            Try
                GameFiber.StartNew(
                    Sub()
                        Dim lion As Suspect = GetPed("Lion")

                        If lionPrey IsNot Nothing Then
                            If lionPrey = Game.LocalPlayer.Character Then
                                If Game.LocalPlayer.Character.IsInAnyVehicle(True) = True Then
                                    lionPrey = Nothing
                                End If
                            End If
                        End If

                        If lion IsNot Nothing Then
                            If lionPrey Is Nothing Then
                                If lion.Exists Then
                                    If lion.IsDead = False Then
                                        If Game.LocalPlayer.Character.IsInAnyVehicle(True) = False AndAlso lion.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 25 Then
                                            'Player is on foot, and close to lion
                                            Try
                                                lionPrey = Game.LocalPlayer.Character
                                                Natives.Peds.AttackPed(lion, Game.LocalPlayer.Character)
                                            Catch ex As Exception
                                                Logger.LogVerboseDebug("Error making lion attack player -- " & ex.Message)
                                            End Try
                                        Else
                                            Dim peds As List(Of Ped) = lion.GetNearbyPeds(10).ToList()

                                            If peds.Count > 0 Then
                                                lionPrey = peds(gRandom.Next(peds.Count))

                                                Try
                                                    Natives.Peds.AttackPed(lion, lionPrey)
                                                Catch ex As Exception
                                                    Logger.LogVerboseDebug("Error attacking lion prey -- " & ex.Message)
                                                End Try
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End Sub)
            Catch ex As Exception
                Logger.LogVerboseDebug("Error finding lion prey -- " & ex.Message)
            End Try
        End Sub

        Sub ReportMountainLion()
            Dim success As Boolean = False
            Dim lion As Suspect = GetPed("Lion")

            If lion IsNot Nothing Then
                If lion.Exists = True Then
                    success = True
                End If
            End If

            If success = True Then
                Game.DisplaySubtitle("Civilian: POLICE!! HELP!! There's a mountain lion on the loose!!", 10000)
                'Game.DisplayNotification("~g~OBJECTIVE: ~w~Find the mountain lion and deal with it appropriately.")
                Radio.DispatchMessage("Civilian called 911 to report mountain lion sighting", True)
                LionSearchState = LionSearchStateEnum.NotYetLocated
                CreateLionBlip()
            Else
                TroubleType = TroubleTypeEnum.None
            End If
        End Sub

        Sub CreateLionBlip()
            Dim lion As Suspect = GetPed("Lion")

            If lion IsNot Nothing Then
                If lion.Exists = True Then
                    lion.CreateBlip(Drawing.Color.Purple)
                End If
            End If
        End Sub

        Sub DeleteLion()
            Dim lion As Suspect = GetPed("Lion")

            If lion IsNot Nothing Then
                If lion.Exists = True Then
                    lion.Delete()
                    Peds.Remove(lion)
                End If
            End If
        End Sub

        Overrides Sub [End]()
            lionPrey = Nothing

            If searchArea IsNot Nothing Then
                If searchArea.Exists Then
                    searchArea.Delete()
                End If
            End If

            If TroubleType = TroubleTypeEnum.DeadBody Then
                Dim d As Cop = GetPed("Detective")
                Dim v As Victim = GetPed("Victim1")
                Dim dv As Vehicles.Vehicle = GetVehicle("DetectiveUnit")

                If dv IsNot Nothing And dv.Exists Then
                    dv.IsSirenOn = False
                    dv.IsPersistent = False
                    dv.Dismiss()
                End If

                If d IsNot Nothing And d.Exists Then
                    d.IsPersistent = False
                    d.Dismiss()
                End If

                If v IsNot Nothing And v.Exists Then
                    v.IsPersistent = False
                    v.Dismiss()
                End If
            End If

            MyBase.[End]()
        End Sub

        Public Property TroubleType As TroubleTypeEnum
        Public Property LionSearchState As LionSearchStateEnum

        Public Enum TroubleTypeEnum
            None = 0
            MountainLion = 1
            FalseCall = 2
            AssaultInProgress = 3
            DeadBody = 4
        End Enum

        Public Enum LionSearchStateEnum
            Null = 0
            NotYetLocated = 1
            LionLocated = 2
            LionEscaped = 3
            LionIsDead = 4
        End Enum

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

        Private Property DetectiveState As EDetectiveState = EDetectiveState.Null
        Private Enum EDetectiveState
            Null
            Created
            Dispatched
            Responding
            AtScene
            WalkingToPlayer
            SpeakingToPlayer
            WalkingToVictim
            AboutToInspectVictim
            KneelingToInspect
            InspectingVictim
            Done
        End Enum

    End Class

End Namespace