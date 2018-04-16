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
Imports LSPD_First_Response.Engine.Scripting

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Person With a Firearm", CalloutProbability.Medium)>
    Public Class PersonWithWeapon
        Inherits CalloutBase

        Dim mSuspectReacted As Boolean = False
        Dim suspectSearchArea As Blip
        Dim pursuit As LHandle
        Dim pursuitInitiated As Boolean = False
        Private SuspectGender As LSPD_First_Response.Gender

        Dim MaleModels As WeaponSuspect() = {New WeaponSuspect("A_M_Y_SouCent_01", "Black male wearing a blue shirt and a sweater vest"),
                                            New WeaponSuspect("A_M_Y_StWhi_01", "White male with long brown hair wearing a white t-shirt and denim shorts"),
                                            New WeaponSuspect("A_M_Y_StBla_01", "Black male wearing a striped shirt, khaki shorts; possibly red shoes"),
                                            New WeaponSuspect("A_M_Y_Downtown_01", "Black heavyset male, bald headed, wearing blue jeans"),
                                            New WeaponSuspect("A_M_M_Soucent_01", "Black male wearing a Liberty City jersey, blue jeans, and black shoes")}

        Dim FemaleModels As WeaponSuspect() = {New WeaponSuspect("A_F_Y_GenHot_01", "White female with short dark hair, possibly wearing a yellow shirt"),
                                            New WeaponSuspect("A_F_Y_Yoga_01", "White female with short hair, wearing a tanktop and capri shorts")}

        Dim pSuspect As Suspect = Nothing
        Dim mSuspectModel As WeaponSuspect
        Dim suspectCounterOn As Boolean = False
        Dim suspectLastSeen As Vector3
        Dim lastLocationUpdate As DateTime = DateTime.Now

        Public Sub New()
            MyBase.New("Person With a Firearm", CallResponseType.Code_3)
            RadioCode = 417
            CrimeEnums = {DISPATCH.CRIMES.CODE_417, DISPATCH.CRIMES.PERSON_WITH_FIREARM}.ToList()

            Dim genderFactor As Integer = gRandom.Next(2)
            If genderFactor = 0 Then
                SuspectGender = Gender.Male
                mSuspectModel = MaleModels(gRandom.Next(MaleModels.Count))
            Else
                SuspectGender = Gender.Female
                mSuspectModel = FemaleModels(gRandom.Next(FemaleModels.Count))
            End If

            'CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails = ""
            CallDetails += "Caller reported a person with a firearm, and hung up."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "Suspect is described as a " & mSuspectModel.Description & "."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "Suspect is reportedly still in the area on foot. No further details available at this time."

            Objective = "Find and apprehend the ~r~suspect!"
        End Sub

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim baseReturn As Boolean = MyBase.OnBeforeCalloutDisplayed()

            If baseReturn = False Then
                Return False
            End If

            Dim pedSpawn As Vector3 = PedHelper.GetSafeCoordinatesForPed(World.GetNextPositionOnStreet(SpawnPoint.Around(60)))
            If pedSpawn = Vector3.Zero Then
                pedSpawn = World.GetNextPositionOnStreet(SpawnPoint.Around(60))
            End If

            pSuspect = New Suspect("Suspect1", mSuspectModel.Model, pedSpawn, gRandom.Next(360), False)
            pSuspect.DisplayName = "Suspect"

            Dim typeFactor As Integer = gRandom.Next(10)

            Dim p As Entities.Persona = Functions.GetPersonaForPed(pSuspect)

            If typeFactor >= 3 Then
                SuspectType = SuspectTypeEnum.Suspect
                p = New Entities.Persona(pSuspect, p.Gender, p.BirthDay, p.Citations, p.Forename, p.Surname, Entities.ELicenseState.Suspended, p.TimesStopped, True, False, False)
                Functions.SetPersonaForPed(pSuspect, p)
                Logger.LogVerboseDebug("SuspectType = Suspect")

                With pSuspect
                    .SpeechLines.Add("Hey Officer, how's it going?")
                    .SpeechLines.Add("Is there a problem?")
                    .SpeechLines.Add("Gun?")
                    .SpeechLines.Add("Someone called saying I have a gun?")
                    .SpeechLines.Add("No...I don't have a gun...")
                    .SpeechLines.Add("Who gave you a crazy idea like that?")
                    .SpeechLines.Add("I mean, that's the funniest thing I've ever heard!")
                End With
            Else
                SuspectType = SuspectTypeEnum.PoliceOfficer
                pSuspect.RelationshipGroup = "OFFDUTY_COP"
                Game.SetRelationshipBetweenRelationshipGroups("OFFDUTY_COP", "PLAYER", Relationship.Respect)
                Game.SetRelationshipBetweenRelationshipGroups("PLAYER", "OFFDUTY_COP", Relationship.Respect)

                Dim mCopFactor As Integer = gRandom.Next(3)
                Dim mIsCop As Boolean = False
                Dim mIsAgent As Boolean = False

                Select Case mCopFactor
                    Case 0
                        CopType = CopTypeEnum.PoliceOfficer
                        mIsCop = True
                    Case 1
                        CopType = CopTypeEnum.DeputySheriff
                        mIsCop = True
                    Case 2
                        CopType = CopTypeEnum.FederalAgent
                        mIsAgent = True
                End Select

                p = New Entities.Persona(pSuspect, p.Gender, p.BirthDay, 0, p.Forename, p.Surname, Entities.ELicenseState.Valid, 0, False, mIsAgent, mIsCop)
                LSPD_First_Response.Mod.API.Functions.SetPersonaForPed(pSuspect, p)
                pSuspect.BlockPermanentEvents = False
                Logger.LogVerboseDebug("SuspectType = PoliceOfficer")

                With pSuspect
                    .SpeechLines.Add("Hey Officer, how's it going?")
                    .SpeechLines.Add("Is there a problem?")
                    .SpeechLines.Add("Gun?")
                    .SpeechLines.Add("Someone called saying I have a gun?")
                End With

                Select Case CopType
                    Case CopTypeEnum.FederalAgent
                        pSuspect.SpeechLines.Add("Well, maybe if I tell you my name, things will get clearer.")
                        pSuspect.SpeechLines.Add(String.Format("Special Agent {0}, FIB.", p.FullName))
                        pSuspect.SpeechLines.Add("Check my ID, if you want. Just make it quick, I have things to do.")

                    Case CopTypeEnum.DeputySheriff
                        pSuspect.SpeechLines.Add("Well, I do have one, but I don't think you're going to be worried!")
                        pSuspect.SpeechLines.Add(String.Format("I'm Deputy {0}, with the Sheriff's Department.", p.FullName))
                        pSuspect.SpeechLines.Add("I have my ID with me, if you want to see it.")

                    Case CopTypeEnum.PoliceOfficer
                        pSuspect.SpeechLines.Add("Seriously? I gotta put up with this shit?")
                        pSuspect.SpeechLines.Add("Oh shit, I'm sorry! Where are my manners?")

                        Dim mDivisions As String() = {"Mission Row", "La Mesa", "Davis", "Vinewood", "Rockford Hills", "Del Perro", "Vespucci"}

                        pSuspect.SpeechLines.Add(String.Format("I'm a cop. My name is {0}. ~n~LSPD {1} Division.", p.FullName, mDivisions(gRandom.Next(mDivisions.Length))))
                        pSuspect.SpeechLines.Add("I have my ID with me, if you want to see it.")
                End Select
            End If

            Dim pAnimSet As String = ""
            If SuspectGender = Gender.Female Then
                pAnimSet = "move_f@arrogant"
            Else
                pAnimSet = "move_m@confident"
            End If

            Try
                Dim animSet As New AnimationSet(pAnimSet)
                animSet.LoadAndWait()
                pSuspect.MovementAnimationSet = animSet
            Catch ex As Exception
                Logger.LogVerboseDebug("Error animating ped -- " & ex.Message)
            End Try

            pSuspect.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 56, False)

            pSuspect.Tasks.Wander()

            If pSuspect IsNot Nothing Then
                If pSuspect.Exists Then
                    Peds.Add(pSuspect)
                Else
                    Return False
                End If
            End If

            If PerformPedChecks() Then
                Return baseReturn
            Else
                Return False
            End If
        End Function

        Public Overrides Function OnCalloutAccepted() As Boolean
            SuspectSearch = SuspectSearchStateEnum.Null
            Return MyBase.OnCalloutAccepted()
        End Function

        Public Overrides Sub OnArrivalAtScene()
            MyBase.OnArrivalAtScene()

            Radio.DispatchMessage("Suspect description has been transmitted to your computer.", True)

            Dim mDescription As String = "Suspect is described as a " & mSuspectModel.Description & "."
            Game.DisplayNotification(mDescription)

            Game.DisplayHelp("Search the area for the suspect. Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout.")

            If pSuspect.Exists Then
                lastLocationUpdate = DateTime.Now
                CreateSearchArea(pSuspect.OriginalSpawnPoint)
                SuspectSearch = SuspectSearchStateEnum.NotYetLocated
            Else
                Game.DisplayNotification("Person With a Gun callout crashed")
                Radio.CallIsCode4(Me.ScriptInfo.Name)
                [End]()
            End If
        End Sub

        Private Sub CreateSearchArea(ByVal pSpawnPoint As Vector3)
            suspectSearchArea = New Blip(pSpawnPoint, 150)
            suspectSearchArea.Color = Drawing.Color.FromArgb(70, Drawing.Color.Red)
        End Sub

        Private Sub DeleteSearchArea()
            Try
                If suspectSearchArea IsNot Nothing Then
                    suspectSearchArea.Delete()
                End If
            Catch ex As Exception

            End Try
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            If Game.LocalPlayer.Character.IsDead Then
                Exit Sub
            End If

            If Game.IsKeyDown(Config.SpeakKey) Then
                If pSuspect.Exists AndAlso pSuspect.IsAlive Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(pSuspect.Position) < 3 Then
                        pSuspect.Speak()

                        If pSuspect.SpeechIndex = pSuspect.SpeechLines.Count Then
                            If SuspectType = SuspectTypeEnum.Suspect AndAlso mSuspectReacted = False Then
                                Dim reaxFactor As Integer = gRandom.Next(10)

                                If reaxFactor <= 3 Then
                                    mSuspectReacted = True
                                    AttackPlayer()
                                Else
                                    mSuspectReacted = True
                                    pursuitInitiated = True
                                    pursuit = Common.CreatePursuit()
                                    pSuspect.AddToPursuit(pursuit)
                                End If
                            End If
                        End If
                    End If
                End If
            End If

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    Radio.CallIsCode4(Me.ScriptInfo.Name)
                    [End]()
                End If
            End If

            If pSuspect IsNot Nothing AndAlso pSuspect.Exists Then

                If pSuspect.IsDead Or pSuspect.IsArrested Then
                    If pSuspect.Inventory.Weapons.Count > 0 Then
                        pSuspect.Inventory.Weapons.Clear()
                        Game.DisplayNotification("While searching the suspect, you find/remove a ~r~pistol~w~.")
                    End If

                    pSuspect.DeleteBlip()
                    Radio.CallIsCode4(Me.ScriptInfo.Name, pSuspect.IsArrested)
                    [End]()
                Else
                    If SuspectSearch = SuspectSearchStateEnum.NotYetLocated Then
                        If Game.LocalPlayer.Character.Position.DistanceTo(pSuspect.Position) <= 30 Then

                            If suspectCounterOn = False Then
                                suspectCounterOn = True
                                Dim startTime As DateTime = DateTime.Now

                                GameFiber.StartNew(
                                    Sub()
                                        While Game.LocalPlayer.Character.Position.DistanceTo(pSuspect.Position) <= 30
                                            GameFiber.Yield()

                                            Dim ts As TimeSpan = DateTime.Now - startTime

                                            If ts.TotalSeconds >= 3 Then
                                                SuspectSearch = SuspectSearchStateEnum.Located
                                                DeleteSearchArea()
                                                pSuspect.CreateBlip()
                                                Radio.SuspectSpotted()
                                                SuspectFound(pSuspect)

                                                Exit While
                                            End If
                                        End While

                                        suspectCounterOn = False
                                    End Sub)
                            End If

                        End If

                        If suspectSearchArea IsNot Nothing AndAlso suspectSearchArea.Exists AndAlso suspectCounterOn = False Then
                            Dim ts As TimeSpan = DateTime.Now - lastLocationUpdate

                            If ts.TotalSeconds > 60 Then
                                GameFiber.StartNew(
                                    Sub()
                                        suspectLastSeen = pSuspect.Position
                                        suspectSearchArea.Position = pSuspect.Position
                                        lastLocationUpdate = DateTime.Now

                                        Dim pAudio As String = "SUSPECT_LAST_SEEN IN_OR_ON_POSITION"

                                        Dim mHeading As String = Common.GetDirectionAudioFromHeading(pSuspect.Heading)
                                        If mHeading <> "" Then
                                            pAudio = String.Format("SUSPECT_LAST_SEEN DIR_HEADING {0} IN_OR_ON_POSITION", mHeading)
                                        End If

                                        AudioPlayerEngine.PlayAudio(pAudio, pSuspect.Position)

                                        Game.DisplayHelp("The search area has been updated.")
                                    End Sub)
                            End If
                        End If

                    ElseIf SuspectSearch = SuspectSearchStateEnum.Located Then
                        DeleteSearchArea()

                        If pursuitInitiated = True Then
                            If LSPD_First_Response.Mod.API.Functions.IsPursuitStillRunning(pursuit) = False Then
                                pSuspect.DeleteBlip()
                                [End]()
                            End If
                        End If
                    End If

                End If
            Else
                Game.DisplayNotification("Person With Weapon Callout crashed")
                [End]()
            End If
        End Sub

        Private Sub SuspectFound(ByVal pSuspect As Suspect)
            pSuspect.Tasks.Clear()
            pSuspect.TurnToFaceEntity(Game.LocalPlayer.Character)

            Game.DisplayHelp("Speak to the subject by pressing " & Config.SpeakKey.ToString() & ". Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout.")

            If Common.IsComputerPlusRunning() Then
                AddPedToCallout(pSuspect)
            End If

            If SuspectType = SuspectTypeEnum.Suspect Then
                Dim reaxFactor As Integer = gRandom.Next(10)
                reaxFactor = 9

                If reaxFactor <= 1 Then
                    mSuspectReacted = True
                    AttackPlayer()
                ElseIf reaxFactor >= 2 And reaxFactor <= 4 Then
                    mSuspectReacted = True
                    pursuitInitiated = True
                    pursuit = Common.CreatePursuit()
                    pSuspect.AddToPursuit(pursuit)
                Else
                    'Nothing
                End If
            End If
        End Sub

        Private Sub AttackPlayer()
            'Dim attackDelay As Integer = gRandom.Next(30000, 60000)
            Dim attackDelay As Integer = 2500

            GameFiber.StartNew(
                Sub()
                    GameFiber.Sleep(attackDelay)

                    Try
                        pSuspect.RelationshipGroup = "HATES_PLAYER"
                        Natives.Peds.AttackPed(pSuspect, Game.LocalPlayer.Character)
                    Catch ex As Exception
                        Logger.LogVerboseDebug("Error attacking player -- " & ex.Message)
                    End Try
                End Sub)
        End Sub

        Public Overrides Sub [End]()
            MyBase.[End]()
        End Sub

        Private SuspectSearch As SuspectSearchStateEnum = SuspectSearchStateEnum.Null
        Enum SuspectSearchStateEnum
            Null = 0
            NotYetLocated = 1
            Located = 2
        End Enum

        Private SuspectType As SuspectTypeEnum
        Enum SuspectTypeEnum
            Suspect
            PoliceOfficer
        End Enum

        Private CopType As CopTypeEnum
        Enum CopTypeEnum
            PoliceOfficer
            DeputySheriff
            FederalAgent
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

        Private Structure WeaponSuspect
            Friend Sub New(ByVal pModel As String, ByVal pDesc As String)
                Model = pModel
                Description = pDesc
            End Sub

            Friend Property Model As String
            Friend Property Description As String
        End Structure

    End Class

End Namespace