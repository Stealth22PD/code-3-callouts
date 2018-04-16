Imports LSPD_First_Response
Imports LSPD_First_Response.Mod.API
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Common.Extensions
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Models.Vehicles
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports System.Windows.Forms
Imports Stealth.Common.Natives
Imports LSPD_First_Response.Engine.Scripting.Entities

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Domestic", CalloutProbability.High)>
    Public Class BackupDomestic
        Inherits CalloutBase

        Dim SuspectModels As String() = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01"}
        Dim VictimModels As String() = {"A_F_Y_GenHot_01", "A_F_Y_Hippie_01", "A_F_Y_Hipster_01", "A_F_Y_BevHills_01", "A_F_Y_BevHills_02", "A_F_M_Tourist_01", "A_F_M_FatWhite_01", "A_F_M_Business_02"}

        Dim anyoneArrested As Boolean = False
        Dim arrestedTipDisplayed As Boolean = False

        Dim mState As ScenarioState = ScenarioState.PlayerResponding
        Dim pursuitInitiated As Boolean = False
        Dim pursuit As LHandle = Nothing
        Dim playerAskedToRunNames As DateTime = DateTime.Now
        Dim warrantChecksDoneQuestionDisplayed As DateTime = DateTime.Now
        Dim mWantedPeds As ArrestablePeds = ArrestablePeds.None

        Dim pSuspect As Suspect = Nothing
        Dim pDataSuspect As Persona = Nothing

        Dim pVictim As Suspect = Nothing
        Dim pDataVictim As Persona = Nothing

        Dim pCop1 As Peds.Cop = Nothing
        Dim pCop2 As Peds.Cop = Nothing

        Dim vPolice1 As Models.Vehicles.Vehicle = Nothing

        Private Enum ScenarioState
            PlayerResponding
            PlayerExitedVehicle
            OfficerGreetingPlayer
            PlayerBriefed
            PlayerJoinedSituation
            SituationProceeding
            PlayerGettingIDs
            PlayerRunningNames
            WarrantChecksComplete
            ArrestProceeding
            ScenarioOver
            CallIsCode4
            CalloutEnded
        End Enum

        Private Enum ArrestablePeds
            None
            Suspect
            Victim
            Both
        End Enum

        Public Sub New()
            MyBase.New("Backup Required (Domestic)", CallResponseType.Code_2)
            RadioCode = 240
            CrimeEnums = {DISPATCH.CRIMES.OFFICER_IN_NEED_OF_ASSISTANCE}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "Female RP called to report that her husband had gotten into a verbal altercation with her, during which he struck her physically. RP alleges that the suspect can sometimes become violent if provoked."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "RP seemed hesitant to answer questions; proceed with caution."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine

            Dim mUnitNumber As Integer = gRandom.Next(27, 50)

            'CallDetails += String.Format("[{0}] UPD: 1-ADAM-{1} responding Code 2", CallDispatchTime.AddSeconds(15).ToString("M/d/yyyy HH:mm:ss"), mUnitNumber)
            CallDetails += String.Format("UPDATE: 1-ADAM-{0} responding Code 2", mUnitNumber)
            CallDetails += Environment.NewLine
            'CallDetails += String.Format("[{0}] UPD: 1-ADAM-{1} on scene; requesting addt'l unit", CallDispatchTime.AddSeconds(80).ToString("M/d/yyyy HH:mm:ss"), mUnitNumber)
            CallDetails += String.Format("UPDATE: 1-ADAM-{0} requesting addt'l unit", mUnitNumber)

            Objective = "Backup your fellow ~b~officers!~n~~w~Stay alert, Officer!"
        End Sub

        Public Overrides Function OnCalloutAccepted() As Boolean
            Dim baseReturn As Boolean = MyBase.OnCalloutAccepted()

            If baseReturn = False Then
                Return False
            End If

            Dim roadHeading As Integer = 0
            Dim roadPos As Vector3 = SpawnPoint
            Dim vehNode As Stealth.Common.Models.VehicleNode = Stealth.Common.Natives.Vehicles.GetClosestVehicleNodeWithHeading(SpawnPoint)

            If vehNode.Position <> Vector3.Zero Then
                roadHeading = vehNode.Heading
                roadPos = vehNode.Position
                roadHeading -= 20

                If roadHeading > 360 Then
                    roadHeading = roadHeading - 360
                End If
            End If

            If Common.IsPlayerInLosSantos Then
                Dim lspdModels As String() = {"POLICE", "POLICE2", "POLICE3", "POLICE4"}
                vPolice1 = New Vehicles.Vehicle(lspdModels(gRandom.Next(lspdModels.Count)), roadPos, roadHeading)
            Else
                Dim sheriffModels As String() = {"SHERIFF", "SHERIFF", "SHERIFF2", "POLICE4"}
                vPolice1 = New Vehicles.Vehicle(sheriffModels(gRandom.Next(sheriffModels.Count)), roadPos, roadHeading)
            End If
            vPolice1.IsSirenOn = True
            vPolice1.IsSirenSilent = True
            vPolice1.MakePersistent()

            vPolice1.Position = vPolice1.GetOffsetPositionRight(5)
            Vehicles.Add(vPolice1)

            Dim pedSpawn As Vector3 = SpawnPoint

            If FoundPedSafeSpawn = False Then
                pedSpawn = vPolice1.GetOffsetPositionFront(5)
            End If

            pSuspect = New Suspect("Suspect1", SuspectModels(gRandom.Next(SuspectModels.Count)), pedSpawn, 0, False)
            pSuspect.DisplayName = "Suspect"
            pSuspect.BlockPermanentEvents = True
            pSuspect.MakePersistent()
            'pSuspect.TurnToFaceEntity(vPolice1)
            Peds.Add(pSuspect)

            pVictim = New Suspect("Victim1", VictimModels(gRandom.Next(VictimModels.Count)), pSuspect.GetOffsetPosition(Vector3.RelativeLeft * 5), 0, False)
            pVictim.DisplayName = "Victim"
            pVictim.BlockPermanentEvents = True
            pVictim.MakePersistent()
            'pVictim.TurnToFaceEntity(vPolice1)
            Peds.Add(pVictim)

            pCop1 = Models.Peds.Cop.Create("Cop1", pSuspect.GetOffsetPositionFront(1.5), 180, Convert.ToBoolean(gRandom.Next(2)))
            pCop1.CreateBlip()
            pCop2 = Models.Peds.Cop.Create("Cop2", pVictim.GetOffsetPositionFront(1.5), 180, Convert.ToBoolean(gRandom.Next(2)))
            pCop2.CreateBlip()
            Peds.Add(pCop1)
            Peds.Add(pCop2)
            [Mod].API.Functions.SetPedAsCop(pCop1)
            [Mod].API.Functions.SetPedAsCop(pCop2)

            pCop1.KeepTasks = True
            pCop1.TurnToFaceEntity(pSuspect)
            pCop1.Tasks.PlayAnimation("move_m@intimidation@cop@unarmed", "idle", 1.0, AnimationFlags.SecondaryTask Or AnimationFlags.Loop)

            pCop2.KeepTasks = True
            pCop2.TurnToFaceEntity(pVictim)
            pCop2.Tasks.PlayAnimation("amb@code_human_police_investigate@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask Or AnimationFlags.Loop)

            'pVictim.KeepTasks = True
            'pVictim.Tasks.PlayAnimation("amb@code_human_cower_stand@female@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask Or AnimationFlags.Loop)

            'pSuspect.KeepTasks = True
            'pSuspect.Tasks.PlayAnimation("amb@code_human_wander_smoking@male@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask Or AnimationFlags.Loop)

            If PerformPedChecks() Then
                pDataSuspect = [Mod].API.Functions.GetPersonaForPed(pSuspect)
                Dim mSuspectWanted As Integer = gRandom.Next(3)
                If mSuspectWanted = 0 Then
                    pDataSuspect = New Persona(pSuspect, Gender.Male, pDataSuspect.BirthDay, 0, pDataSuspect.Forename, pDataSuspect.Surname, ELicenseState.Valid, 5, True, False, False)
                Else
                    pDataSuspect = New Persona(pSuspect, Gender.Male, pDataSuspect.BirthDay, 0, pDataSuspect.Forename, pDataSuspect.Surname, ELicenseState.Valid, 5, False, False, False)
                End If
                [Mod].API.Functions.SetPersonaForPed(pSuspect, pDataSuspect)

                pDataVictim = [Mod].API.Functions.GetPersonaForPed(pVictim)
                Dim mVictimWanted As Integer = gRandom.Next(5)
                If mVictimWanted = 0 Then
                    pDataVictim = New Persona(pVictim, Gender.Female, pDataVictim.BirthDay, 0, pDataVictim.Forename, pDataVictim.Surname, ELicenseState.Valid, 0, True, False, False)
                Else
                    pDataVictim = New Persona(pVictim, Gender.Female, pDataVictim.BirthDay, 0, pDataVictim.Forename, pDataVictim.Surname, ELicenseState.Valid, 0, False, False, False)
                End If
                [Mod].API.Functions.SetPersonaForPed(pVictim, pDataVictim)

                If pDataSuspect.Wanted = True And pDataVictim.Wanted = True Then
                    mWantedPeds = ArrestablePeds.Both
                ElseIf pDataSuspect.Wanted = True And pDataVictim.Wanted = False Then
                    mWantedPeds = ArrestablePeds.Suspect
                ElseIf pDataSuspect.Wanted = False And pDataVictim.Wanted = True Then
                    mWantedPeds = ArrestablePeds.Victim
                Else
                    mWantedPeds = ArrestablePeds.None
                End If

                If Common.IsComputerPlusRunning() Then
                    AddPedToCallout(pSuspect)
                    AddPedToCallout(pVictim)
                End If

                Return baseReturn
            Else
                Radio.DispatchMessage("Disregard, Call is Code 4", True)
                Return False
            End If
        End Function

        Public Overrides Function PerformPedChecks() As Boolean
            Dim base As Boolean = MyBase.PerformPedChecks()

            If base Then
                If pCop1.Exists() AndAlso pCop2.Exists() AndAlso pVictim.Exists() AndAlso pSuspect.Exists() Then
                    If pCop1.DistanceTo(pCop2.Position) > 25 Then
                        Return False
                    End If

                    If pVictim.DistanceTo(pSuspect.Position) > 25 Then
                        Return False
                    End If

                    If pVictim.DistanceTo(pCop1.Position) > 25 Then
                        Return False
                    End If

                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Function

        Public Overrides Sub OnArrivalAtScene()
            MyBase.OnArrivalAtScene()
            pCop1.TurnToFaceEntity(pSuspect)

            GameFiber.StartNew(
                Sub()
                    Game.DisplayHelp("Park up and make contact with the officer.", 8000)
                End Sub)
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            If State = CalloutState.AtScene Then
                Select Case mState
                    Case ScenarioState.PlayerResponding
                        If Game.LocalPlayer.Character.IsInAnyVehicle(True) = False Then
                            mState = ScenarioState.PlayerExitedVehicle
                        End If

                    Case ScenarioState.PlayerExitedVehicle
                        GameFiber.StartNew(
                            Sub()
                                pCop2.Tasks.Clear()
                                pCop2.TurnToFaceEntity(Game.LocalPlayer.Character)
                                GameFiber.Sleep(500)
                                'pCop2.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, Vector3.RelativeFront * 2)
                                mState = ScenarioState.OfficerGreetingPlayer
                                pCop2.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 3.0F, 0.0F, 1.6F).WaitForCompletion()
                                pCop2.TurnToFaceEntity(Game.LocalPlayer.Character)
                            End Sub)

                    Case ScenarioState.OfficerGreetingPlayer
                        If Game.LocalPlayer.Character.Position.DistanceTo(pCop2.Position) <= 4 Then
                            Game.DisplaySubtitle("~b~Officer: ~w~Hey, thanks for coming so quick. Come on, my partner will brief you.", 8000)
                            Game.DisplayHelp("Follow the officer, and speak with their partner.", 8000)
                            pCop1.Blip.Flash(500, 20000)

                            GameFiber.StartNew(
                                Sub()
                                    GameFiber.Sleep(2000)
                                    pCop2.Tasks.ClearImmediately()
                                    pCop2.Tasks.GoToOffsetFromEntity(pVictim, 3.0F, 0.0F, 1.6F).WaitForCompletion()
                                    pCop2.TurnToFaceEntity(pVictim)
                                    GameFiber.Sleep(500)
                                    pCop2.Tasks.PlayAnimation("amb@code_human_police_investigate@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask Or AnimationFlags.Loop)
                                End Sub)
                            mState = ScenarioState.PlayerBriefed
                        End If

                    Case ScenarioState.PlayerBriefed
                        If Game.LocalPlayer.Character.Position.DistanceTo(pSuspect.Position) < 4 Or Game.LocalPlayer.Character.Position.DistanceTo(pCop1.Position) < 4 Then
                            mState = ScenarioState.PlayerJoinedSituation
                        End If

                    Case ScenarioState.PlayerJoinedSituation
                        mState = ScenarioState.SituationProceeding
                        AskPlayerToGetIDs()

                    Case ScenarioState.PlayerRunningNames
                        Dim ts As TimeSpan = DateTime.Now - playerAskedToRunNames
                        If ts.TotalSeconds > 15 AndAlso Game.LocalPlayer.Character.IsOnFoot AndAlso Game.LocalPlayer.Character.Position.DistanceTo(pCop1.Position) < 4 Then
                            Dim ts2 As TimeSpan = DateTime.Now - warrantChecksDoneQuestionDisplayed
                            If ts2.TotalSeconds > 8 Then
                                Game.DisplayHelp("Press CTRL + E to tell the other officer about the warrant checks.", 5000)
                            End If
                        End If

                        If Game.LocalPlayer.Character.Position.DistanceTo(pCop1.Position) < 4 Then
                            If Game.IsKeyDown(Keys.E) Then
                                If Game.IsKeyDownRightNow(Keys.ControlKey) Then
                                    mState = ScenarioState.WarrantChecksComplete
                                End If
                            End If
                        End If

                    Case ScenarioState.WarrantChecksComplete
                        mState = ScenarioState.ArrestProceeding
                        InitiateArrest()

                    Case ScenarioState.ScenarioOver
                        If pursuitInitiated = True Then
                            If [Mod].API.Functions.IsPursuitStillRunning(pursuit) = False Then
                                mState = ScenarioState.CallIsCode4
                            End If
                        Else
                            If [Mod].API.Functions.IsPedArrested(pSuspect) Then
                                If pSuspect.Inventory.Weapons.Count > 0 Then
                                    pSuspect.Inventory.Weapons.Clear()
                                    Game.DisplayNotification("While searching the suspect, a ~r~weapon~w~ is found.")
                                End If

                                mState = ScenarioState.CallIsCode4
                            End If
                        End If

                    Case ScenarioState.CallIsCode4
                        OnCallCode4()
                        mState = ScenarioState.CalloutEnded
                End Select
            End If

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    OnCallCode4()
                End If
            End If
        End Sub

        Private Sub AskPlayerToGetIDs()
            mState = ScenarioState.PlayerGettingIDs

            GameFiber.StartNew(
                Sub()
                    Game.DisplaySubtitle("~b~Cop: ~w~Nice of you to join us, Officer.", 3000)
                    GameFiber.Sleep(3000)
                    Game.DisplaySubtitle("~b~Cop: ~w~" & pDataSuspect.Forename & " and " & pDataVictim.Forename & " here seem to have gotten into a fight.", 5000)
                    GameFiber.Sleep(5000)
                    Game.DisplaySubtitle("~b~Cop: ~w~Their full names are " & pDataSuspect.FullName & " and " & pDataVictim.FullName & ".", 5000)
                    GameFiber.Sleep(5000)
                    Game.DisplaySubtitle("~b~Cop: ~w~Can you run them for warrants? We'll sort out their stories.", 5000)
                    playerAskedToRunNames = DateTime.Now
                    GameFiber.Sleep(3000)

                    CallDetails += Environment.NewLine
                    CallDetails += Environment.NewLine
                    'CallDetails += String.Format("[{0}] UPDATE: ", DateTime.Now.ToString("M/d/yyyy HH:mm:ss"))
                    CallDetails += "UPDATE: Subject names are " & pDataSuspect.FullName & " and " & pDataVictim.FullName & "."

                    Game.DisplayHelp("Go back to your vehicle and run the two subjects' names.", 5000)
                    GameFiber.Sleep(5000)
                    Game.DisplayHelp("When done, speak to the lead officer.", 5000)
                    mState = ScenarioState.PlayerRunningNames
                End Sub)
        End Sub

        Private Sub InitiateArrest()
            Try
                GameFiber.StartNew(
                Sub()
                    Dim reactionFactor As Integer = gRandom.Next(5)
                    Dim victimReactionFactor As Integer = gRandom.Next(2)

                    Game.DisplaySubtitle("~b~Cop: ~w~Thank you, Officer.", 3000)
                    GameFiber.Sleep(3000)

                    Dim mPedsToArrest As ArrestablePeds = ArrestablePeds.None
                    'If mWantedPeds = ArrestablePeds.None Then mPedsToArrest = ArrestablePeds.Suspect

                    Dim variationFactor As Integer = gRandom.Next(0, 5)

                    Select Case variationFactor
                        Case 0
                            ' " & pDataSuspect.Forename & "
                            Game.DisplaySubtitle("~b~Cop: ~w~A passerby called 911 when they saw these two having a heated argument.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~They are both blaming each other for starting the fight.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~But when we rolled up, " & pDataSuspect.Forename & " shoved her to the ground.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~So he unfortunately, is guilty of Battery.", 5000)
                            GameFiber.Sleep(5000)

                            mPedsToArrest = ArrestablePeds.Suspect
                        Case 1
                            mPedsToArrest = ArrestablePeds.None
                            Game.DisplaySubtitle("~b~Cop: ~w~What's happened here is, " & pDataSuspect.Forename & " has been having an affair.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~" & pDataVictim.Forename & " found out about it, and is understandably upset.", 5000)
                            GameFiber.Sleep(5000)

                            Game.DisplaySubtitle("~b~Cop: ~w~" & pDataSuspect.Forename & ", do you have a friend you can stay with today?", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~Maybe it'd be a good idea for both of you to cool off, ya know?", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~y~" & pDataSuspect.Forename & ": ~w~I think that'd be a good idea, Officer.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~y~" & pDataVictim.Forename & ": ~w~Fine with me. But you'll be hearing from my lawyer, " & pDataSuspect.Forename & "!", 5000)
                            GameFiber.Sleep(5000)
                        Case 2
                            mPedsToArrest = ArrestablePeds.Victim
                            Game.DisplaySubtitle("~b~Cop: ~w~A passerby called 911 when they saw these two having a heated argument.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~" & pDataVictim.Forename & " admitted to attacking " & pDataSuspect.Forename & " with a frying pan.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~She got pretty violent with him...apparently he forgot their anniversary.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~While she's allowed to be mad, she can't be hitting people with things.", 5000)
                            GameFiber.Sleep(5000)
                        Case 3
                            mPedsToArrest = ArrestablePeds.Both
                            Game.DisplaySubtitle("~b~Cop: ~w~So we can't really determine who is the aggressor here.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~They are both blaming each other for starting the fight.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~Neither of them seems to want to back down at all.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~" & pDataSuspect.Forename & ", do you have a friend you can stay with today?", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~Maybe it'd be a good idea for both of you to cool off, ya know?", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~y~" & pDataSuspect.Forename & ": ~w~Why?! It's MY house!! Why can't I live in my own damn house?!", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~y~" & pDataVictim.Forename & ": ~w~It's HALF MY HOUSE, you asshole!!", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~ENOUGH, both of you!!", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~y~" & pDataSuspect.Forename & ": ~w~Step off, pig. This ain't none of your business.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~When you're out here in public, that MAKES it my business!", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~I think we better settle this down at the station.", 5000)
                            GameFiber.Sleep(5000)
                        Case Else
                            mPedsToArrest = ArrestablePeds.Suspect
                            Game.DisplaySubtitle("~b~Cop: ~w~So from their stories, it seems these two got into an argument that escalated.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~It appears that he attacked her during the argument, and assaulted her.", 5000)
                            GameFiber.Sleep(5000)
                            Game.DisplaySubtitle("~b~Cop: ~w~He also threatened her just as we pulled up.", 5000)
                            GameFiber.Sleep(5000)
                    End Select

                    Select Case mPedsToArrest
                        Case ArrestablePeds.Suspect
                            If mWantedPeds = ArrestablePeds.Victim Or mWantedPeds = ArrestablePeds.Both Then
                                mPedsToArrest = ArrestablePeds.Both
                                Game.DisplaySubtitle("~b~Cop: ~w~" & pDataVictim.Forename & ", you unfortunately have a warrant for your arrest.", 5000)
                                GameFiber.Sleep(5000)
                                Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000)
                                GameFiber.Sleep(5000)
                            End If

                        Case ArrestablePeds.Victim
                            If mWantedPeds = ArrestablePeds.Suspect Or mWantedPeds = ArrestablePeds.Both Then
                                mPedsToArrest = ArrestablePeds.Both
                                Game.DisplaySubtitle("~b~Cop: ~w~" & pDataSuspect.Forename & ", you unfortunately have a warrant for your arrest.", 5000)
                                GameFiber.Sleep(5000)
                                Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000)
                                GameFiber.Sleep(5000)
                            End If

                        Case ArrestablePeds.None
                            If mWantedPeds = ArrestablePeds.Suspect Then
                                mPedsToArrest = ArrestablePeds.Suspect
                                Game.DisplaySubtitle("~b~Cop: ~w~" & pDataSuspect.Forename & ", you unfortunately have a warrant for your arrest.", 5000)
                                GameFiber.Sleep(5000)
                                Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000)
                                GameFiber.Sleep(5000)
                            ElseIf mWantedPeds = ArrestablePeds.Victim
                                mPedsToArrest = ArrestablePeds.Victim
                                Game.DisplaySubtitle("~b~Cop: ~w~" & pDataVictim.Forename & ", you unfortunately have a warrant for your arrest.", 5000)
                                GameFiber.Sleep(5000)
                                Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000)
                                GameFiber.Sleep(5000)
                            ElseIf mWantedPeds = ArrestablePeds.Both
                                mPedsToArrest = ArrestablePeds.Both
                                Game.DisplaySubtitle("~b~Cop: ~w~Unfortunately, you both have warrants for your arrest.", 5000)
                                GameFiber.Sleep(5000)
                                Game.DisplaySubtitle("~b~Cop: ~w~You're going to have to come with us to have that sorted out.", 5000)
                                GameFiber.Sleep(5000)
                            End If

                    End Select


                    Select Case mPedsToArrest
                        Case ArrestablePeds.Suspect
                            reactionFactor = gRandom.Next(6)
                        Case ArrestablePeds.Victim
                            reactionFactor = -1
                        Case ArrestablePeds.Both
                            reactionFactor = gRandom.Next(3, 6)
                    End Select


                    If mPedsToArrest = ArrestablePeds.Victim Then
                        Game.DisplaySubtitle("~b~Cop: ~w~" & pDataVictim.Forename & ", I'm placing you under arrest.", 5000)
                        GameFiber.Sleep(5000)
                        Game.DisplaySubtitle("~b~Cop: ~w~Please put your hands behind your back. Officer, take her into custody.", 5000)
                        GameFiber.Sleep(5000)

                    ElseIf mPedsToArrest = ArrestablePeds.Both Then
                        Game.DisplaySubtitle("~b~Cop: ~w~" & pDataSuspect.Forename & " and " & pDataVictim.Forename & ", I'm placing you both under arrest.", 5000)
                        GameFiber.Sleep(5000)
                        Game.DisplaySubtitle("~b~Cop: ~w~Both of you, please put your hands behind your back. Cuff them please, Officer.", 5000)
                        GameFiber.Sleep(5000)

                    ElseIf mPedsToArrest = ArrestablePeds.Suspect Then
                        Game.DisplaySubtitle("~b~Cop: ~w~" & pDataSuspect.Forename & ", I'm placing you under arrest.", 5000)
                        GameFiber.Sleep(5000)
                        Game.DisplaySubtitle("~b~Cop: ~w~Please put your hands behind your back. Cuff him, Officer.", 5000)
                        GameFiber.Sleep(5000)

                    ElseIf mPedsToArrest = ArrestablePeds.None
                        mState = ScenarioState.CallIsCode4
                        Exit Sub
                    End If

                    Select Case reactionFactor
                        Case -1
                            'Victim
                            If victimReactionFactor = 0 Then
                                'Nothing
                                Game.DisplaySubtitle("~y~" & pDataSuspect.Forename & ": ~w~That's what you get, bitch! Lock her up and throw away the key!", 5000)
                            Else
                                'She runs
                                Game.DisplaySubtitle("~r~" & pDataVictim.Forename & ": ~w~Go to hell, pig!!", 5000)
                                pVictim.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_KNIFE"), 100, True)
                                Stealth.Common.Natives.Peds.ReactAndFleePed(pVictim, pCop1)

                                GameFiber.Sleep(4000)
                                TriggerPursuit(False, True)
                            End If

                        Case 0
                            'Nothing
                            Game.DisplaySubtitle("~y~Victim: ~w~Thank you, Officer! Lock him up and throw away the key!", 5000)

                        Case 1
                            'He runs
                            Game.DisplaySubtitle("~r~Suspect: ~w~You'll have to catch me first!!", 5000)
                            Stealth.Common.Natives.Peds.ReactAndFleePed(pSuspect, pCop1)

                            GameFiber.Sleep(4000)
                            TriggerPursuit(True, False)

                            pVictim.Tasks.PlayAnimation("amb@code_human_cower_stand@female@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask Or AnimationFlags.Loop)

                        Case 2
                            'He attacks
                            Game.DisplaySubtitle("~r~Suspect: ~w~Go to hell, pig!!", 5000)
                            pSuspect.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 100, True)
                            pSuspect.AttackPed(pCop1)

                            GameFiber.Sleep(4000)
                            TriggerPursuit(True, False)

                            pVictim.Tasks.PlayAnimation("amb@code_human_cower_stand@female@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask Or AnimationFlags.Loop)

                        Case 3
                            'She attacks
                            Game.DisplaySubtitle("~r~Victim: ~w~NO!! " & pDataSuspect.Forename.ToUpper() & ", RUN!!", 5000)
                            pVictim.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_KNIFE"), 100, True)
                            Stealth.Common.Natives.Peds.ReactAndFleePed(pSuspect, pCop1)
                            pVictim.AttackPed(Game.LocalPlayer.Character)

                            GameFiber.Sleep(4000)
                            TriggerPursuit(True, True)

                        Case 4
                            'They both run
                            Game.DisplaySubtitle("~y~Victim: ~w~" & pDataSuspect.Forename.ToUpper() & ", RUN!!", 5000)
                            Stealth.Common.Natives.Peds.ReactAndFleePed(pSuspect, pCop1)
                            Stealth.Common.Natives.Peds.ReactAndFleePed(pVictim, pCop2)

                            GameFiber.Sleep(4000)
                            TriggerPursuit(True, True)

                        Case Else
                            'He attacks her
                            Game.DisplaySubtitle("~r~Suspect: ~w~You bitch! I'M GOING TO KILL YOU!!", 5000)
                            pSuspect.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 100, True)
                            pVictim.Tasks.PlayAnimation("amb@code_human_cower_stand@female@idle_a", "idle_a", 1.0, AnimationFlags.SecondaryTask Or AnimationFlags.Loop)
                            pSuspect.AttackPed(pVictim)

                            GameFiber.Sleep(4000)
                            TriggerPursuit(True, False)

                    End Select

                    mState = ScenarioState.ScenarioOver
                End Sub)
            Catch ex As Exception
                [End]()
                Game.DisplayNotification("Crashed!")
                Logger.LogVerbose(ex.ToString())
            End Try
        End Sub

        Private Sub TriggerPursuit(ByVal pAddSuspect As Boolean, ByVal pAddVictim As Boolean)
            GameFiber.StartNew(
                Sub()
                    pCop1.Tasks.Clear()
                    pCop2.Tasks.Clear()

                    pursuitInitiated = True
                    pursuit = Common.CreatePursuit()
                    [Mod].API.Functions.SetPursuitIsActiveForPlayer(pursuit, True)

                    If pAddSuspect Then
                        [Mod].API.Functions.AddPedToPursuit(pursuit, pSuspect)
                    End If

                    If pAddVictim Then
                        [Mod].API.Functions.AddPedToPursuit(pursuit, pVictim)
                    End If

                    GameFiber.Sleep(1000)
                    [Mod].API.Functions.AddCopToPursuit(pursuit, pCop1)
                    [Mod].API.Functions.AddCopToPursuit(pursuit, pCop2)
                End Sub)
        End Sub

        Private Sub OnCallCode4()
            Radio.CallIsCode4(Me.ScriptInfo.Name)

            If vPolice1.Exists() AndAlso pCop1.Exists() AndAlso pCop2.Exists() Then
                If pCop2.DistanceTo(vPolice1.Position) > 50 Then
                    [End]()
                    Exit Sub
                End If
            Else
                [End]()
                Exit Sub
            End If

            If pCop1.Exists() Then
                pCop1.Tasks.ClearImmediately()
            End If

            If pCop2.Exists() Then
                pCop2.Tasks.ClearImmediately()
            End If

            If pVictim IsNot Nothing AndAlso pVictim.Exists() = True Then
                pVictim.Tasks.Clear()
            End If

            Dim whileLoopStarted As DateTime

            GameFiber.StartNew(
                Sub()
                    If vPolice1.Exists() Then
                        If pCop1.Exists() Then
                            pCop1.Tasks.FollowNavigationMeshToPosition(vPolice1.GetOffsetPosition(Vector3.RelativeLeft * 2.0F), 0, 1.2F).WaitForCompletion()
                            If pCop1.Exists() AndAlso vPolice1.Exists() Then pCop1.TurnToFaceEntity(vPolice1)
                            If pCop1.Exists() AndAlso vPolice1.Exists() Then pCop1.Tasks.EnterVehicle(vPolice1, -1)
                        End If
                    End If
                End Sub)

            GameFiber.StartNew(
                Sub()
                    If vPolice1 IsNot Nothing AndAlso vPolice1.Exists() Then
                        If pCop2 IsNot Nothing AndAlso pCop2.Exists() Then
                            'pCop2.Tasks.FollowToOffsetFromEntity(vPolice1, Vector3.RelativeRight * 2)
                            pCop2.Tasks.FollowNavigationMeshToPosition(vPolice1.GetOffsetPosition(Vector3.RelativeRight * 2.0F), 0, 1.2F).WaitForCompletion()
                            If pCop2.Exists() AndAlso vPolice1.Exists() Then pCop2.TurnToFaceEntity(vPolice1)
                            If pCop2.Exists() AndAlso vPolice1.Exists() Then pCop2.Tasks.EnterVehicle(vPolice1, vPolice1.GetFreePassengerSeatIndex())
                        End If
                    End If
                End Sub)

            GameFiber.StartNew(
                Sub()
                    GameFiber.Sleep(3000)

                    whileLoopStarted = DateTime.Now

                    While True
                        GameFiber.Yield()

                        If pCop1.Exists() And pCop2.Exists() Then
                            If pCop1.IsInAnyVehicle(False) = True AndAlso pCop2.IsInAnyVehicle(False) = True Then
                                Exit While
                            End If
                        Else
                            Exit While
                        End If

                        Dim ts As TimeSpan = (DateTime.Now - whileLoopStarted)
                        If ts.TotalSeconds >= 30 Then
                            Exit While
                        End If
                    End While

                    If vPolice1.Exists() Then
                        If pCop1.Exists() Then
                            If pCop1.IsInAnyVehicle(True) = False Then
                                pCop1.Tasks.Clear()
                                pCop1.WarpIntoVehicle(vPolice1, -1)
                            End If
                        End If

                        If pCop2.Exists() Then
                            If pCop2.IsInAnyVehicle(True) = False Then
                                pCop2.Tasks.Clear()
                                pCop2.WarpIntoVehicle(vPolice1, vPolice1.GetFreePassengerSeatIndex())
                            End If
                        End If

                        vPolice1.IsSirenOn = False
                        If pCop1.Exists() Then
                            pCop1.Tasks.CruiseWithVehicle(vPolice1, 12, VehicleDrivingFlags.Normal)
                        End If
                    End If

                    [End]()
                End Sub)
        End Sub

        Private Sub ArrestCheck()
            ArrestCheck(pSuspect)
            ArrestCheck(pVictim)

            If arrestedTipDisplayed = False AndAlso anyoneArrested = True Then
                arrestedTipDisplayed = True
                Game.DisplayHelp("You may end this callout with " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & ", or continue investigating.")
            End If
        End Sub

        Private Sub ArrestCheck(ByRef p As PedBase)
            If p IsNot Nothing Then
                If p.Exists Then
                    If p.IsDead Or p.IsArrested() Then
                        p.DeleteBlip()
                        anyoneArrested = True
                    End If
                End If
            End If
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