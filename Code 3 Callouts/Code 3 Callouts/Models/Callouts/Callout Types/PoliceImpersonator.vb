Imports LSPD_First_Response
Imports LSPD_First_Response.Mod.API
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Common
Imports Stealth.Common.Extensions
Imports Stealth.Common.Models
Imports Stealth.Common.Natives.Vehicles
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports System.Windows.Forms
Imports Stealth.Common.Models.QuestionWindow

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Police Impersonator", CalloutProbability.Medium)>
    Public Class PoliceImpersonator
        Inherits CalloutBase

        Dim suspectIdentified As Boolean = False
        Dim suspectSearchArea As Blip
        Dim searchAreaRadius As Single = 150.0F
        Dim suspectLastSeen As Vector3
        Dim lastLocationUpdate As DateTime = DateTime.Now
        Dim vehType As EImpersonatorVehicle = EImpersonatorVehicle.CivilianVehicle

        Dim VehModels As List(Of String) = {"Blista", "Jackal", "Oracle", "Asea", "Emperor", "Fugitive", "Ingot", "Premier", "Primo", "Stanier", "Stratum", "Asterope", "Baller", "Bison", "Cavalcade2", "Exemplar", "F620", "Felon", "FQ2", "Gresley", "Habanero", "Intruder", "Landstalker", "Mesa", "Minivan", "Patriot", "Radi", "Regina", "schafter2", "Seminole", "Sentinel", "Serrano", "Speedo", "Surge", "Tailgater", "Washington", "Zion"}.ToList()
        Dim PedModels As List(Of String) = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01",
                                        "A_M_M_BevHills_01", "A_M_M_GenFat_01", "A_M_M_Business_01", "A_M_M_Golfer_01", "A_M_M_Skater_01", "A_M_M_Salton_01", "A_M_M_Tourist_01"}.ToList()

        Dim suspectVisual As Boolean = False
        Dim pursuit As LHandle
        Dim pursuitInitiated As Boolean = False
        Dim officerRespondedCode3 As Boolean = False
        Dim fullPlate As Boolean = False
        Dim licensePlate As String = ""

        Dim suspectWillAttack As Boolean = False
        Dim suspectAttacking As Boolean = False
        Dim suspectAttackChance As Integer = 30
        Dim pulloverPossible As Boolean = False
        Dim mPulloverState As PulloverState = PulloverState.Null
        Dim endTipDisplayed As Boolean = False

        Dim suspectCounterOn As Boolean = False

        Private Enum PulloverState
            Null
            Following
            LightsOrSirenOn
            Stopping
            Parked
            Fleeing
            LeftVehicle
        End Enum

        Public Sub New()
            MyBase.New("Police Impersonator", CallResponseType.Code_3)
            RadioCode = 538
            CrimeEnums = {DISPATCH.CRIMES.CIV_ASSISTANCE}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "Caller was stopped by someone identifying themselves as a police officer."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "No police vehicles were in the area at the time; suspect possibly armed. Proceed with caution."

            Objective = "Speak to the ~o~victim~w~. Ensure they're not hurt.~n~Apprehend the ~r~suspect!"
        End Sub

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim baseReturn As Boolean = MyBase.OnBeforeCalloutDisplayed()

            If baseReturn = False Then
                Return False
            End If

            Dim position As Vector3 = World.GetNextPositionOnStreet(SpawnPoint.Around(5))
            Dim node As VehicleNode = Stealth.Common.Natives.Vehicles.GetClosestVehicleNodeWithHeading(position)

            If node.Position = Vector3.Zero Then
                node.Position = position
                node.Heading = gRandom.Next(360)
            End If

            Dim v As New Vehicles.Vehicle(VehModels(gRandom.Next(VehModels.Count)), position, node.Heading)
            Dim p As New Victim("Victim1", PedModels(gRandom.Next(PedModels.Count)), v.GetOffsetPosition(Vector3.RelativeLeft * 2.0F), 0)
            p.DisplayName = "Victim"

            If v IsNot Nothing AndAlso v.Exists() Then
                v.Name = "VictimCar1"
                v.MakePersistent()
                v.SetRandomColor()
                Vehicles.Add(v)

                p.MakePersistent()
                p.BlockPermanentEvents = True
                p.WarpIntoVehicle(v, -1)
                Peds.Add(p)

                AddPedToCallout(p)
                AddVehicleToCallout(v)

                Return True
            Else
                Return False
            End If
        End Function

        Public Overrides Function OnCalloutAccepted() As Boolean
            Dim baseReturn As Boolean = MyBase.OnCalloutAccepted()

            If baseReturn = False Then
                Return False
            End If

            Dim v As Victim = GetPed("Victim1")

            If v IsNot Nothing AndAlso v.Exists Then
                Dim vehModels As String() = {"STANIER", "BUFFALO", "GRANGER"}
                'Dim vehModels As String() = {"POLICE4", "FBI", "FBI2"}

                Dim vehModel As String = vehModels(gRandom.Next(vehModels.Count))
                Dim pedModel As String = "S_M_M_ChemSec_01"

                Dim witnessStory As Integer = gRandom.Next(6)
                With v
                    .SpeechLines.Add("Oh jeez, Officer, am I glad to see you!")

                    RandomizeVictimStory(v, witnessStory)

                    Dim attackFactor As Integer = gRandom.Next(1, 101)
                    If attackFactor <= suspectAttackChance Then
                        suspectWillAttack = True
                    End If

                    If vehType = EImpersonatorVehicle.SecurityVehicle Then
                        vehModel = "dilettante2"
                        pedModel = "S_M_M_Security_01"
                    End If

                    Dim suspectVehicle As New Vehicles.Vehicle(vehModel, World.GetNextPositionOnStreet(SpawnPoint.Around(250)), gRandom.Next(360))
                    suspectVehicle.MakePersistent()
                    suspectVehicle.Name = "SuspectVehicle"
                    suspectVehicle.SetRandomColor()
                    suspectVehicle.FillColorValues()
                    Vehicles.Add(suspectVehicle)

                    Dim s As New Suspect("Suspect1", pedModel, World.GetNextPositionOnStreet(suspectVehicle.Position.Around(3)), 0, False)
                    s.DisplayName = "Suspect"
                    s.MakePersistent()
                    s.WarpIntoVehicle(suspectVehicle, -1)
                    s.Tasks.CruiseWithVehicle(suspectVehicle, 10, VehicleDrivingFlags.Normal)
                    s.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 56, False)
                    s.RelationshipGroup = New RelationshipGroup("CIVMALE")

                    Dim p As Engine.Scripting.Entities.Persona = Functions.GetPersonaForPed(s)
                    Functions.SetVehicleOwnerName(suspectVehicle, p.FullName)

                    s.QAItems = New List(Of QAItem)

                    Dim susStory As Integer = gRandom.Next(4)
                    Select Case susStory
                        Case 0
                            s.SpeechLines.Add("I ain't got nothin to say to you!")

                            s.QAItems.Add(New QAItem("Is this your vehicle?", "Yeah, its my car! So??"))
                            s.QAItems.Add(New QAItem("What were you up to today?", "None of your business, pig!"))
                            s.QAItems.Add(New QAItem("Did you pull over someone today?", "Yeah, your mom! Or was it your sister?"))
                            s.QAItems.Add(New QAItem("Where did you get the red and blue lights?", "Your wife bought em for me...a gift. For uh...my services."))
                            s.QAItems.Add(New QAItem("Have you had anything to drink?", "Yeah...I had some wine with your wife last night."))
                        Case 1
                            s.SpeechLines.Add("Hey, come on now, Officer! It was just a prank. Just kiddin' around, ya know?")

                            s.QAItems.Add(New QAItem("Is this your vehicle?", "No! I mean...yes!"))
                            s.QAItems.Add(New QAItem("What were you up to today?", "Just playing a joke, man."))
                            s.QAItems.Add(New QAItem("Did you pull over someone today?", "Yeah, but come on, I was just kidding!"))
                            s.QAItems.Add(New QAItem("Where did you get the red and blue lights?", "eBay, dude!"))
                            s.QAItems.Add(New QAItem("Have you had anything to drink?", "Just a couple of beers..."))
                        Case 2
                            s.SpeechLines.Add("I'm not saying anything without my lawyer.")

                            s.QAItems.Add(New QAItem("Is this your vehicle?", "Yes, it is, you have no right to hassle me."))
                            s.QAItems.Add(New QAItem("What were you up to today?", "None of your business."))
                            s.QAItems.Add(New QAItem("Did you pull over someone today?", "What? You're the one pulling ME over!"))
                            s.QAItems.Add(New QAItem("Where did you get the red and blue lights?", "What red and blue lights?"))
                            s.QAItems.Add(New QAItem("Have you had anything to drink?", "Why don't you breathalyze me?"))
                        Case Else
                            s.SpeechLines.Add("Is there a problem, Officer?")

                            s.QAItems.Add(New QAItem("Is this your vehicle?", "Yes, it is."))
                            s.QAItems.Add(New QAItem("What were you up to today?", "Just...taking a drive, you know."))
                            s.QAItems.Add(New QAItem("Did you pull over someone today?", "Pull someone over? No!"))
                            s.QAItems.Add(New QAItem("Where did you get the red and blue lights?", "I stole them from...I MEAN...the internet!"))
                            s.QAItems.Add(New QAItem("Have you had anything to drink?", "No."))
                    End Select

                    Peds.Add(s)

                    If vehType = EImpersonatorVehicle.CivilianVehicle Then
                        .SpeechLines.Add("But yeah, about his car...it definitely wasn't a police car.")
                        .SpeechLines.Add("But it sure as hell was dressed up to look like one.")
                        .SpeechLines.Add("It's a model that you guys drive, I think.")

                        .SpeechLines.Add(String.Format("The car was a {0} colored {1}.", suspectVehicle.PrimaryColorName, suspectVehicle.Model.Name))
                    Else
                        .SpeechLines.Add("The car was a Dilettante. It was white, and it had Security Patrol written on it.")
                    End If


                    Dim licensePlateFactor As Integer = gRandom.Next(3)
                    If licensePlateFactor = 0 Then
                        .SpeechLines.Add(String.Format("The license plate number was {0}.", suspectVehicle.LicensePlate))
                        fullPlate = True
                        licensePlate = suspectVehicle.LicensePlate
                    ElseIf licensePlateFactor = 1 Then
                        .SpeechLines.Add(String.Format("The first three digits of the license plate were {0}.", suspectVehicle.LicensePlate.Substring(0, 3)))
                        licensePlate = suspectVehicle.LicensePlate.Substring(0, 3)
                    Else
                        Dim idx As Integer = suspectVehicle.LicensePlate.Length - 3
                        .SpeechLines.Add(String.Format("The last three digits of the license plate were {0}.", suspectVehicle.LicensePlate.Substring(idx)))
                        licensePlate = suspectVehicle.LicensePlate.Substring(idx)
                    End If
                End With

                Return True
            Else
                Radio.CallIsCode4(Me.ScriptInfo.Name)
                [End]()
                Return False
            End If
        End Function

        Private Sub RandomizeVictimStory(ByRef v As Victim, ByVal witnessStory As Integer)
            With v
                Select Case witnessStory
                    Case 0
                        .SpeechLines.Add("Something just didn't add up!")
                        .SpeechLines.Add("I was driving down the street, and this car comes up from behind.")
                        .SpeechLines.Add("He flashes a blue light in his window, signalling me to pull over.")
                        .SpeechLines.Add("So I did, but he gets out, and this dude looked NOTHING like a cop!")
                        .SpeechLines.Add("No uniform, no badge, nothing! I was so scared, I just drove off!")
                        .SpeechLines.Add("Was he a cop? I hope I didn't do anything wrong!")
                        suspectAttackChance = 30
                    Case 1
                        .SpeechLines.Add("This guy drove alongside my car a few minutes ago...")
                        .SpeechLines.Add("And he flashed a badge at me!! And it was just a regular car, no lights!")
                        .SpeechLines.Add("I wasn't speeding or anything, the whole thing just seemed odd.")
                        .SpeechLines.Add("Would an off duty cop try to stop someone like that?")
                        .SpeechLines.Add("It was weird...when I didn't listen to him, he drove off!")
                        suspectAttackChance = 30
                    Case 2
                        .SpeechLines.Add("This car came up behind me, and flashed red and blue lights.")
                        .SpeechLines.Add("I thought it was a cop, so naturally, I pulled over.")
                        .SpeechLines.Add("The guy gets out, walks up, and pulls out a gun!!")
                        .SpeechLines.Add("He took my wallet, my credit cards, everything!")
                        .SpeechLines.Add("I was so scared, I thought he was going to kill me!!")
                        suspectAttackChance = 60
                    Case 3
                        .SpeechLines.Add("This guy drove up behind me, and started flashing his high beams.")
                        .SpeechLines.Add("Then he put his hand out the window, showing me a badge.")
                        .SpeechLines.Add("I pulled over, he got out, and started yelling at me.")
                        .SpeechLines.Add("Told me he was a cop, and to get out of the car.")
                        .SpeechLines.Add("He said I was an illegal alien, and I was under arrest.")
                        .SpeechLines.Add("Sir, I was BORN in this country, I'm an American citizen!")
                        .SpeechLines.Add("I told him to fuck off, and he pulled out a gun!")
                        .SpeechLines.Add("So I hit the gas, and got the fuck outta there!")
                        suspectAttackChance = 40
                    Case 4
                        .SpeechLines.Add("This car came up behind me, and flashed red and blue lights from his window.")
                        .SpeechLines.Add("I thought it was a cop, so naturally, I pulled over.")
                        .SpeechLines.Add("He said I was speeding, and asked me for my license.")
                        .SpeechLines.Add("I asked him if he was a cop, and he said yes.")
                        .SpeechLines.Add("Then I said no, you're not wearing a uniform.")
                        .SpeechLines.Add("He told he to shut up, and said he was ""undercover"".")
                        .SpeechLines.Add("I think he was a police reject, to be honest.")
                        .SpeechLines.Add("Then he asked me to step out of the car, and I said no.")
                        .SpeechLines.Add("Then he said, ""Hey! I am a cop, and you will respect my Authoritah!""")
                        .SpeechLines.Add("I just drove off and left him there.")
                        suspectAttackChance = 15
                    Case Else
                        vehType = EImpersonatorVehicle.SecurityVehicle
                        .SpeechLines.Add("This car came up behind me, and flashed red and blue lights from his window.")
                        .SpeechLines.Add("I thought it was a cop, so naturally, I pulled over.")
                        .SpeechLines.Add("But he wasn't a cop. He was wearing some kind of security uniform.")
                        .SpeechLines.Add("The car he was driving looked like a fucking Prius!")
                        .SpeechLines.Add("He said I was speeding, and asked me for my license.")
                        .SpeechLines.Add("So I said, ""You're not a cop, you're a fucking security guard.""")
                        .SpeechLines.Add("He told he to shut up, and said he was ""undercover"".")
                        .SpeechLines.Add("Then he asked me to step out of the car, and I said no.")
                        .SpeechLines.Add("Then he's like, ""Don't make me use force!""")
                        .SpeechLines.Add("I told him to fuck off, and he pulled out a gun!")
                        .SpeechLines.Add("So I hit the gas, and got the fuck outta there!")
                        suspectAttackChance = 25
                End Select
            End With
        End Sub

        Public Overrides Sub OnArrivalAtScene()
            MyBase.OnArrivalAtScene()

            Game.DisplaySubtitle("Victim: Officer!! Over here!!", 8000)

            Dim v As Victim = GetPed("Victim1")
            If v IsNot Nothing AndAlso v.Exists Then
                v.CreateBlip()
            End If

            SuspectSearch = SuspectSearchStateEnum.Null
            Game.DisplayHelp("Press " & Config.SpeakKey.ToString() & " to talk to the 911 caller.", 8000)
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            If Game.LocalPlayer.Character.IsDead Then
                Exit Sub
            End If

            Dim v As Victim = GetPed("Victim1")
            Dim s As Suspect = GetPed("Suspect1")
            Dim suspectVeh As Vehicles.Vehicle = GetVehicle("SuspectVehicle")

            If State = CalloutState.UnitResponding Then
                If v IsNot Nothing AndAlso v.IsDead Then
                    v.Resurrect()
                End If
            End If

            If Game.IsKeyDown(Config.SpeakKey) Then
                SpeakToSubject(v, s)
            End If

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    Radio.CallIsCode4(Me.ScriptInfo.Name)
                    [End]()
                End If
            End If

            If State = CalloutState.AtScene Then
                If s IsNot Nothing AndAlso s.Exists Then
                    If s.IsArrested() OrElse s.IsDead Then
                        If s.Inventory.Weapons.Count > 0 Then
                            s.Inventory.Weapons.Clear()
                            Game.DisplayNotification("While searching the suspect, you find/remove a ~r~pistol~w~.")
                        End If

                        If s.IsArrested() Then
                            If endTipDisplayed = False Then
                                endTipDisplayed = True

                                GameFiber.StartNew(
                                   Sub()
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
                    Else
                        If SuspectSearch = SuspectSearchStateEnum.Null Then
                            If suspectIdentified = False AndAlso v.HasSpoken Then
                                suspectIdentified = True

                                If suspectVeh IsNot Nothing AndAlso suspectVeh.Exists Then
                                    If Common.IsComputerPlusRunning() Then
                                        AddVehicleToCallout(suspectVeh)
                                    End If

                                    suspectLastSeen = suspectVeh.Position
                                    CreateSearchArea(suspectLastSeen)
                                    SuspectSearch = SuspectSearchStateEnum.NotYetLocated
                                Else
                                    SuspectSearch = SuspectSearchStateEnum.Escaped
                                    Game.DisplayNotification("Police Impersonator Callout Crashed")
                                    [End]()
                                End If

                                GameFiber.StartNew(
                                    Sub()
                                        Dim veh As Vehicles.Vehicle = GetVehicle("VictimCar1")

                                        If veh IsNot Nothing AndAlso veh.Exists Then
                                            GameFiber.Sleep(3000)
                                            v.Tasks.CruiseWithVehicle(veh, 10, VehicleDrivingFlags.Normal)
                                            v.Dismiss()
                                            veh.Dismiss()
                                        End If
                                    End Sub)

                                CallDetails += Environment.NewLine
                                CallDetails += Environment.NewLine

                                If fullPlate Then
                                    CallDetails += String.Format("UPDATE: Vehicle was a {0} colored {1}; License # {2}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate)
                                Else
                                    CallDetails += String.Format("UPDATE: Vehicle was a {0} colored {1}; Partial license {2}.", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate)
                                End If

                                If fullPlate = True Then
                                    Radio.UnitMessage(String.Format("Suspect vehicle is a {0} {1}, License # {2}", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate))
                                Else
                                    Radio.UnitMessage(String.Format("Suspect vehicle is a {0} {1}, Partial license # {2}", suspectVeh.PrimaryColorName, suspectVeh.Model.Name, licensePlate))
                                End If

                                Radio.DispatchMessage("Roger", True)
                                Game.DisplayHelp("The victim will now leave the scene. Search the area for the suspect.")

                                Dim pAudio As String = "SUSPECT_LAST_SEEN IN_OR_ON_POSITION"

                                Dim mHeading As String = Common.GetDirectionAudioFromHeading(suspectVeh.Heading)
                                If mHeading <> "" Then
                                    pAudio = String.Format("SUSPECT_LAST_SEEN DIR_HEADING {0} IN_OR_ON_POSITION", mHeading)
                                End If

                                AudioPlayerEngine.PlayAudio(pAudio, suspectLastSeen)
                            End If
                        End If

                        If SuspectSearch = SuspectSearchStateEnum.NotYetLocated Then
                            If suspectVeh IsNot Nothing AndAlso suspectVeh.Exists Then
                                If Game.LocalPlayer.Character.Position.DistanceTo(suspectVeh.GetOffsetPosition(Vector3.RelativeBack * 2)) <= 15 Then
                                    'SuspectSearch = SuspectSearchStateEnum.Located
                                    'DeleteSearchArea()
                                    'suspectVeh.CreateBlip(Drawing.Color.Yellow)
                                    's.CreateBlip(Drawing.Color.Yellow)
                                    'Radio.SuspectSpotted()


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

                                                        If Common.IsComputerPlusRunning() Then
                                                            AddPedToCallout(s)
                                                        End If

                                                        Exit While
                                                    End If
                                                End While

                                                suspectCounterOn = False
                                            End Sub)
                                    End If

                                End If

                                If (suspectSearchArea IsNot Nothing AndAlso suspectSearchArea.Exists) AndAlso suspectCounterOn = False Then
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
                            Else
                                SuspectSearch = SuspectSearchStateEnum.Escaped
                                Game.DisplayNotification("Police Impersonator Callout Crashed")
                                [End]()
                            End If
                        ElseIf SuspectSearch = SuspectSearchStateEnum.Located Then
                            If suspectVisual = False Then
                                suspectVisual = True

                                ProcessSuspectVisual(s)
                            Else
                                'If pulloverPossible = True Then
                                '    ProcessPullover(s)
                                'End If

                                If suspectAttacking = False Then
                                    If Game.LocalPlayer.Character.Position.DistanceTo(s.Position) < 10 Then
                                        If Game.LocalPlayer.Character.IsOnFoot Then
                                            If suspectWillAttack = True AndAlso s.IsOnFoot Then
                                                suspectAttacking = True
                                                Natives.Peds.AttackPed(s, Game.LocalPlayer.Character)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Else
                    'Logger.LogTrivialDebug("Suspect is null?!")
                End If
            End If

        End Sub

        Private Sub ProcessSuspectVisual(ByRef s As Suspect)
            'Siren = pursuit
            If Game.LocalPlayer.Character.IsInAnyVehicle(False) = True Then
                Dim copcar As Vehicle = Game.LocalPlayer.Character.CurrentVehicle

                If copcar IsNot Nothing Then
                    If copcar.Exists AndAlso copcar.HasSiren Then
                        If copcar.IsSirenOn = True AndAlso copcar.IsSirenSilent = False Then
                            officerRespondedCode3 = True
                            pursuitInitiated = True
                            pursuit = Common.CreatePursuit()
                            s.AddToPursuit(pursuit)

                            Game.DisplayNotification("The suspect heard your siren and is fleeing.")
                            Exit Sub
                        End If
                    End If
                End If
            End If

            'Game.DisplayHelp("Turn on your lights to pull over the suspect.", 8000)
            'pulloverPossible = True
            'mPulloverState = PulloverState.Following
        End Sub

        'Private Sub ProcessPullover(ByVal s As Suspect)
        '    Dim mCopCar As Vehicle = Game.LocalPlayer.Character.CurrentVehicle

        '    Select Case mPulloverState
        '        Case PulloverState.Following
        '            If mCopCar.Exists() Then
        '                If mCopCar.IsSirenOn AndAlso mCopCar.Position.DistanceTo(s.Position) < 20 Then
        '                    mPulloverState = PulloverState.LightsOrSirenOn
        '                End If
        '            End If

        '        Case PulloverState.LightsOrSirenOn
        '            Dim reaxFactor As Integer = gRandom.Next(4)
        '            If reaxFactor = 3 Then
        '                mPulloverState = PulloverState.Fleeing
        '                pursuitInitiated = True
        '                pursuit = Common.CreatePursuit()
        '                s.AddToPursuit(pursuit)
        '            Else
        '                mPulloverState = PulloverState.Stopping
        '                GameFiber.StartNew(
        '                    Sub()
        '                        s.Tasks.ParkVehicle(s.GetOffsetPositionFront(10), (s.Heading - 25)).WaitForCompletion()
        '                        mPulloverState = PulloverState.Parked
        '                    End Sub)
        '            End If

        '        Case PulloverState.Stopping
        '        Case PulloverState.Parked
        '            Dim fleeFactor As Integer = gRandom.Next(5)
        '            If fleeFactor = 4 Then
        '                mPulloverState = PulloverState.Fleeing
        '                GameFiber.StartNew(
        '                    Sub()
        '                        s.Tasks.PerformDrivingManeuver(VehicleManeuver.BurnOut)
        '                        GameFiber.Sleep(1000)
        '                        pursuitInitiated = True
        '                        pursuit = Common.CreatePursuit()
        '                        s.AddToPursuit(pursuit)
        '                    End Sub)
        '            Else
        '                GameFiber.StartNew(
        '                    Sub()
        '                        While True
        '                            GameFiber.Yield()
        '                            If Game.LocalPlayer.Character.IsOnFoot Then
        '                                s.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen)
        '                                Exit While
        '                            End If
        '                        End While
        '                    End Sub)
        '            End If

        '    End Select
        'End Sub

        Private Sub SpeakToSubject(ByVal v As Victim, ByVal s As Suspect)
            If s IsNot Nothing AndAlso s.Exists Then
                If Game.LocalPlayer.Character.Position.DistanceTo(s.Position) < 3 Then
                    s.Speak()
                    Exit Sub
                End If
            End If

            If v IsNot Nothing AndAlso v.Exists Then
                If Game.LocalPlayer.Character.Position.DistanceTo(v.Position) < 3 Then
                    v.Speak()
                    Exit Sub
                End If
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

        Public Overrides Sub [End]()
            DeleteSearchArea()
            MyBase.[End]()
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
                Return False
            End Get
        End Property

        Enum EImpersonatorVehicle
            CivilianVehicle
            SecurityVehicle
        End Enum

    End Class

End Namespace