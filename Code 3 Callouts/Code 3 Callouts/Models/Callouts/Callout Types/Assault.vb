﻿Imports LSPD_First_Response.Engine.Scripting
Imports LSPD_First_Response.Mod.API
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Common.Extensions
Imports Stealth.Common.Models.QuestionWindow
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports System.Windows.Forms

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Assault", CalloutProbability.High)>
    Public Class Assault
        Inherits CalloutBase

        Dim AssaultType As EType = EType.Genuine
        Dim SuspectModels As String() = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01"}
        Dim VictimModels As String() = {"A_F_Y_GenHot_01", "A_F_Y_Hippie_01", "A_F_Y_Hipster_01", "A_F_Y_BevHills_01", "A_F_Y_BevHills_02", "A_F_M_Tourist_01", "A_F_M_FatWhite_01", "A_F_M_Business_02"}
        Dim WitnessModels As String() = {"A_M_M_BevHills_01", "A_M_M_GenFat_01", "A_M_M_Business_01", "A_M_M_Golfer_01", "A_M_M_Skater_01", "A_M_M_Salton_01", "A_M_M_Tourist_01"}

        Dim weaponFactor As Integer = 0
        Dim suspectIdentified As Boolean = False
        Dim anyoneArrested As Boolean = False
        Dim arrestedTipDisplayed As Boolean = False
        Dim suspectReturning As Boolean = False

        Public Sub New()
            MyBase.New("Assault", CallResponseType.Code_2)
            RadioCode = 240
            CrimeEnums = {DISPATCH.CRIMES.CODE_240, DISPATCH.CRIMES.ASSAULT}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "Female RP called to report that she was assualted by a male suspect who left the area."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "A witness is on scene awaiting police arrival; no further information at this time."

            Objective = "Interview the ~o~victim ~w~ and ~o~witness.~w~~n~Determine what ~r~actually ~w~happened!"
        End Sub

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim baseReturn As Boolean = MyBase.OnBeforeCalloutDisplayed()

            If baseReturn = False Then
                Return False
            End If

            Dim suspectSpawn As Vector3 = PedHelper.GetSafeCoordinatesForPed(World.GetNextPositionOnStreet(SpawnPoint.Around(20)))
            If suspectSpawn = Vector3.Zero Then
                suspectSpawn = World.GetNextPositionOnStreet(SpawnPoint.Around(20))
            End If
            Dim s As New Suspect("Suspect1", SuspectModels(gRandom.Next(SuspectModels.Count)), suspectSpawn, 0, False)
            s.DisplayName = "Suspect"
            s.BlockPermanentEvents = True
            s.MakePersistent()

            Dim suspectStory As Integer = gRandom.Next(5)

            Dim genuineFactor As Integer = gRandom.Next(1, 11)
            If genuineFactor < 7 Then
                AssaultType = EType.Genuine

                weaponFactor = gRandom.Next(3)
                If weaponFactor = 1 Then
                    s.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_KNIFE"), 56, False)
                ElseIf weaponFactor = 2 Then
                    s.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 56, False)
                End If

                s.PhysicalCondition = "Subject appears nervous, shifting his weight; he is looking around and avoiding eye contact."
            Else
                AssaultType = EType.Fabricated
                suspectStory = 4
                s.PhysicalCondition = "Subject speaking confidently, and is making firm eye contact."
            End If

            Dim mSuspectQAItems As New List(Of QAItem)
            mSuspectQAItems.Add(New QAItem("Why don't you tell me what happened?"))
            mSuspectQAItems.Add(New QAItem("Why is she calling the police on you?"))
            mSuspectQAItems.Add(New QAItem("Who is she? Do you know her?"))
            mSuspectQAItems.Add(New QAItem("The other guy standing there, do you know him at all?"))
            mSuspectQAItems.Add(New QAItem("What am I going to find on you in my computer? Do you have any warrants?"))

            If suspectStory = 0 Then
                With s
                    .SpeechLines.Add("I ain't got nothin to hide, alright?")
                    .SpeechLines.Add("You can't believe a WORD that she says!!")
                    .SpeechLines.Add("She was all up in my face, throwing herself at me!")
                    .SpeechLines.Add("I told her I got a girl, and she starts screamin!!")
                    .SpeechLines.Add("Saying I did this shit and that, it ain't true!")
                    .SpeechLines.Add("She crazy, man! I ain't done NOTHIN!")
                    .SpeechLines.Add("I could have left, ya know?! But I stayed!!")
                    .SpeechLines.Add("Cause I wanted to talk to you, give my side!!")

                    mSuspectQAItems(0).Answer = "I told you, she threw herself at me!"
                    mSuspectQAItems(1).Answer = "I don't know, man! She wants attention or something!"
                    mSuspectQAItems(2).Answer = "How many times do I have to say it? NO!"
                    mSuspectQAItems(3).Answer = "I think he wants to get with her, if you know what I mean."
                    mSuspectQAItems(4).Answer = "You ain't gonna find shit. It'll tell you to fuck off and leave me alone!"
                End With
            ElseIf suspectStory = 1 Then
                With s.SpeechLines
                    .Add("I ain't got nothing to say, Officer.")
                    .Add("I want my lawyer before I talk to you.")

                    mSuspectQAItems(0).Answer = "I believe I have the right to remain silent."
                    mSuspectQAItems(1).Answer = "I don't know, ask her!"
                    mSuspectQAItems(2).Answer = "I'm refusing to answer that."
                    mSuspectQAItems(3).Answer = "No. In fact, he attacked me. You should arrest him."
                    mSuspectQAItems(4).Answer = "It'll tell you I have the right to an attorney."
                End With
            ElseIf suspectStory = 2 Then
                With s.SpeechLines
                    .Add("Officer, you gotta believe me, I didn't do anything!")
                    .Add("She's making it up, I swear I didn't hit her!")
                    .Add("I mean...uhh...no English!")

                    mSuspectQAItems(0).Answer = "She's a friend of mine, Officer. We were going out for coffee."
                    mSuspectQAItems(1).Answer = "I don't know! I swear!"
                    mSuspectQAItems(2).Answer = "Yeah, she's my girlfriend...I mean, my coworker."
                    mSuspectQAItems(3).Answer = "He's the one who assaulted me!"
                    mSuspectQAItems(4).Answer = "Nothing man, I'm clean!"
                End With
            ElseIf suspectStory = 3 Then
                With s.SpeechLines
                    .Add("Seriously, I don't get why that bitch called the cops.")
                    .Add("All I did was say hi to her, man. She looked kinda hot, ya know?")
                    .Add("You can't blame a guy for trying!")
                    .Add("Come on, is that a crime, for god's sake?!")

                    mSuspectQAItems(0).Answer = "I just wanted to get her number, man, that's it."
                    mSuspectQAItems(1).Answer = "Cause she's a straight up BITCH, that's why."
                    mSuspectQAItems(2).Answer = "No, man. And I'm glad, too. She looks hot, but she's crazy."
                    mSuspectQAItems(3).Answer = "Her boyfriend, maybe? I don't know."
                    mSuspectQAItems(4).Answer = "I had a DUI a few months ago, but that's it."
                End With
            Else
                With s
                    .SpeechLines.Add("I could have left, Officer. But...")
                    .SpeechLines.Add("When I saw you pull up, I had to come back.")
                    .SpeechLines.Add("I haven't got anything to hide, sir.")
                    .SpeechLines.Add("I came back to speak with you personally.")
                    .SpeechLines.Add("I just wanted to make sure you got the TRUTH.")
                    .SpeechLines.Add("Someone has to show her that she can't just make stuff up.")
                    .SpeechLines.Add("She was screaming that I threatened her with a knife.")
                    .SpeechLines.Add("You can check me, I don't have any weapons.")

                    mSuspectQAItems(0).Answer = "I was walking down the street, and she just starts screaming at me."
                    mSuspectQAItems(1).Answer = "How should I know? I was just minding my own business!"
                    mSuspectQAItems(2).Answer = "Hell no, man. I don't know that bitch."
                    mSuspectQAItems(3).Answer = "I've seen him on YouTube...he's Jeff Favignano, isn't he?"
                    mSuspectQAItems(4).Answer = "Not that I know of."
                End With
            End If
            s.QAItems = mSuspectQAItems

            s.Tasks.Wander()
            Peds.Add(s)

            Dim vicQAItems As New List(Of QAItem)
            vicQAItems.Add(New QAItem("Do you know the man who attacked you?"))
            vicQAItems.Add(New QAItem("What happened leading up to the attack?"))
            vicQAItems.Add(New QAItem("Did you see any weapons?"))
            vicQAItems.Add(New QAItem("Did he say anything to you?"))
            vicQAItems.Add(New QAItem("Do you need medical attention?"))

            Dim v As New Victim("Victim1", VictimModels(gRandom.Next(VictimModels.Count)), SpawnPoint, 0)
            v.DisplayName = "Victim"
            v.SpeechLines.Add("Oh, Officer, thank god you're here!!")
            v.BlockPermanentEvents = True
            v.MakePersistent()

            Dim injuryFactor As Integer = 0

            injuryFactor = gRandom.Next(3)

            Select Case injuryFactor
                Case 0
                    v.PhysicalCondition = "Some bruising on the victim's arm"
                Case 1
                    v.PhysicalCondition = "Victim has bruising on her cheek"
                Case Else
                    v.PhysicalCondition = "Dirty clothing; victim's knee is scraped. Wound consistent with a fall to the sidewalk"
            End Select

            Select Case weaponFactor
                Case 1
                    vicQAItems(2).Answer = "Just before the other guy rescued me, the attacker was pulling out a knife."
                Case 2
                    vicQAItems(2).Answer = "I think I saw something in his waistband. Could it have been a gun?"
                Case Else
                    vicQAItems(2).Answer = "No...I didn't see one, at least."
            End Select

            Dim victimStory As Integer = gRandom.Next(5)
            If AssaultType = EType.Fabricated Then victimStory = 4

            If victimStory = 0 Then
                With v
                    .SpeechLines.Add("The man left just before you got here.")
                    .SpeechLines.Add("I was just walking down the street, and he came up to me...")
                    .SpeechLines.Add("He started saying things like ""I love you"", it was really creepy!")
                    .SpeechLines.Add("I told him to screw off and leave me alone, and walked away.")
                    .SpeechLines.Add("All of a sudden, he jumps on me, and starts hitting me!")
                    .SpeechLines.Add("This nice guy here rescued me and chased him off.")
                    .SpeechLines.Add("The guy ran off...but said he would be back.")
                    .SpeechLines.Add("Be careful, Officer. I think he had a weapon.")

                    vicQAItems(0).Answer = "No, Officer, I've never seen him before."
                    vicQAItems(1).Answer = "Nothing, I mean, he just came out of nowhere."
                    vicQAItems(3).Answer = "Just that he loved me, and wanted to be with me forever."
                    vicQAItems(4).Answer = "I...I think I'll be ok. I just need a bandaid, if you have one."
                End With
            ElseIf victimStory = 1 Then

                With v.SpeechLines
                    .Add("I didn't know what to do, I was so scared!")
                    .Add("This guy, he just, grabbed me!")
                    .Add("He came up behind me, put his arms around me...")
                    .Add("I thought it was my boyfriend at first...")
                    .Add("I was on my way to meet him, so I thought it was him...")
                    .Add("But when he spoke, I didn't recognize the voice.")
                    .Add("I don't care what happens, I just want him away from me, please!!")

                    vicQAItems(0).Answer = "No way...like I said, I thought he was my boyfriend at first, but he's not!!"
                    vicQAItems(1).Answer = "I was just walking down the street, he came up suddenly from behind."
                    vicQAItems(3).Answer = "Yeah, he hugged me and said 'Hey baby, you want to have some fun?'. It was so creepy!!"
                    vicQAItems(4).Answer = "No, Officer, I'm fine, thank you."
                End With
            ElseIf victimStory = 2 Then

                With v.SpeechLines
                    .Add("I didn't know what to do, I was so scared!")
                    .Add("I was at Up-n-Atom, getting a bite to eat.")
                    .Add("He came in, and he was just STARING at me while ordering his food.")
                    .Add("I left, and he followed me outside.")
                    .Add("He followed me onto the bus, and he just wouldn't go away.")
                    .Add("I got off the bus, and he followed me!")
                    .Add("I wanted to ask the bus driver for help, but...")
                    .Add("I was just so scared.")
                    .Add("This man here came up to me, called 911, and gave me his phone.")
                    .Add("That's what happened. I just... *starts crying*")

                    vicQAItems(0).Answer = "Never seen him before in my life."
                    vicQAItems(1).Answer = "He didn't get a chance to attack me...but he was stalking me!"
                    vicQAItems(3).Answer = "Yeah, he said 'Hey honey, you want a real man to sleep with tonight?'."
                    vicQAItems(4).Answer = "I think...I'll be fine. I just need a moment."
                End With
            ElseIf victimStory = 3 Then

                With v.SpeechLines
                    .Add("This guy came up to me, saying I dropped a $20 bill.")
                    .Add("I told him that no, it wasn't mine.")
                    .Add("He smiled and asked, 'Can you blame a guy for trying?'")
                    .Add("He seemed cute, so I started talking to him.")
                    .Add("He was really charming at first, I didn't think anything was wrong.")
                    .Add("Then he just GRABS me and tries to drag me into an alley!!")
                    .Add("I didn't know what to do! I was so scared, I just SCREAMED!")
                    .Add("This man here chased him off, and he called 911.")
                    .Add("I don't care what happens, I just want him away from me, please!!")
                    .Add("He needs to be locked up, please!! He's DANGEROUS!!")

                    vicQAItems(0).Answer = "No, Officer! Not at all!"
                    vicQAItems(1).Answer = "Like I said, he was charming at first, then he turned into a fucking monster."
                    vicQAItems(3).Answer = "Yeah, he grabbed me and said 'Come on, I'm going to fuck your brains out, right now!'"
                    vicQAItems(4).Answer = "No...but, um...I'm free tonight, if you're...single."
                End With
            Else
                With v
                    AssaultType = EType.Fabricated

                    injuryFactor = gRandom.Next(3)

                    Select Case injuryFactor
                        Case 0
                            v.PhysicalCondition = "No visible injuries"
                        Case 1
                            v.PhysicalCondition = "Victim's behavior is strange; very much on the defensive."
                        Case Else
                            v.PhysicalCondition = "Scratches on victim's arm; but they seem to match her own fingernails"
                    End Select

                    .SpeechLines.Add("The man left just before you got here.")
                    .SpeechLines.Add("I was just walking down the street, and he came up to me...")
                    .SpeechLines.Add("He started saying things like ""I love you"", it was really creepy!")
                    .SpeechLines.Add("I was just minding my own business!!")
                    .SpeechLines.Add("And he...he just...punched me!!")
                    .SpeechLines.Add("He called me a bitch, I swear, he's crazy!")
                    .SpeechLines.Add("He said he wanted to rape me!!")
                    .SpeechLines.Add("This nice guy here rescued me and chased him off.")
                    .SpeechLines.Add("He was crazy! Seriously! You have to arrest him!!")
                    .SpeechLines.Add("You look like a nice guy...can you help a girl out?")
                    .SpeechLines.Add("Pleeeeeaseeee?")

                    vicQAItems(0).Answer = "He's the asshole that...I mean, no! The asshole that attacked me!"
                    vicQAItems(1).Answer = "He just like, attacked me, for no reason at all!!"
                    vicQAItems(3).Answer = "He said he had a girlfriend, but that I had a better ass."
                    vicQAItems(4).Answer = "I need some attention from you in my bed, if you get my drift."
                End With
            End If
            v.QAItems = vicQAItems

            Peds.Add(v)

            Dim w As New Witness("Witness1", WitnessModels(gRandom.Next(WitnessModels.Count)), SpawnPoint.Around(5.0F), 0)
            w.DisplayName = "Witness"
            w.BlockPermanentEvents = True
            w.MakePersistent()

            Dim mWitnessQA As New List(Of QAItem)
            mWitnessQA.Add(New QAItem("So you were just passing by, is that correct?"))
            mWitnessQA.Add(New QAItem("What happened leading up to the incident?"))
            mWitnessQA.Add(New QAItem("Did the man have a weapon?"))
            mWitnessQA.Add(New QAItem("Did you hear the suspect say anything?"))
            mWitnessQA.Add(New QAItem("Did you use physical force at all?"))

            Select Case weaponFactor
                Case 1
                    mWitnessQA(2).Answer = "Yeah, just before I stepped in, he had a knife."
                Case 2
                    mWitnessQA(2).Answer = "I didn't see it, but I have a feeling he had one."
                Case Else
                    mWitnessQA(2).Answer = "No, sir, I didn't see a weapon."
            End Select

            With w
                Select Case victimStory
                    Case 0
                        .SpeechLines.Add("All I saw was him chasing her, and her telling him to fuck off.")
                        .SpeechLines.Add("The guy tried to grab her, so I pushed him back.")
                        .SpeechLines.Add("I told him, 'Relax dude, walk away.'")
                        .SpeechLines.Add("Kind of stupid, in hindsight, I guess.")
                        .SpeechLines.Add("But you hit a woman, you deserve what you get, ya know?")

                        mWitnessQA(0).Answer = "Yes, sir. I got out of a cab, and caught the end of it."
                        mWitnessQA(1).Answer = "I don't know, as I said, I didn't see the whole thing."
                        mWitnessQA(3).Answer = "He kept saying he loved her, but she didn't know him. Weird."
                        mWitnessQA(4).Answer = "Not a lot...I just put my hand up and told him to walk away."
                    Case 1
                        .SpeechLines.Add("He was hugging her from behind, Officer.")
                        .SpeechLines.Add("He was claiming to be her boyfriend, and she was pushing him away.")
                        .SpeechLines.Add("She told me she didn't know him, and asked me to call you guys.")

                        mWitnessQA(0).Answer = "Yeah, I was across from her when it happened."
                        mWitnessQA(1).Answer = "He just randomly walked up, and hugged her from behind."
                        mWitnessQA(3).Answer = "He said something like 'Hey baby, want to have fun?', I think."
                        mWitnessQA(4).Answer = "No, when she asked me for help, he ran off, that was it."
                    Case 2
                        .SpeechLines.Add("Whatever she says is true, Officer.")
                        .SpeechLines.Add("I don't know about following her onto the bus, but he got on with her.")
                        .SpeechLines.Add("He followed her off the bus, that's when I stepped in.")
                        .SpeechLines.Add("I wanted to stop him before anything bad happened.")

                        mWitnessQA(0).Answer = "Well, I was on the bus before she got on."
                        mWitnessQA(1).Answer = "He was trying to talk to her, then followed her offf the bus."
                        mWitnessQA(3).Answer = "Not much, aside from trying to start a conversation with her."
                        mWitnessQA(4).Answer = "Nope, I just said I was calling the cops, and he ran off."
                    Case 3
                        .SpeechLines.Add("The guy ran off when he saw me, sir.")
                        .SpeechLines.Add("He was trying to drag her into the alley.")
                        .SpeechLines.Add("If I hadn't shown up, I think he mighta raped her.")
                        .SpeechLines.Add("Not sure if the guy is armed or not.")

                        mWitnessQA(0).Answer = "Yes, Officer. I was just walking by."
                        mWitnessQA(1).Answer = "I saw them talking, and suddenly he grabbed her, and she screamed."
                        mWitnessQA(3).Answer = "Yeah, he told her, 'I'm going to fuck your brains out!', or something."
                        mWitnessQA(4).Answer = "No, sir. I just yelled 'Hey!', and he saw me, and took off."
                    Case Else
                        .SpeechLines.Add("Officer, she's making up the whole thing.")
                        .SpeechLines.Add("Guy doesn't give her any attention, so...she cries rape.")
                        .SpeechLines.Add("Maybe she's telling the truth, but, it doesn't seem genuine.")
                        .SpeechLines.Add("I don't know, something just seems off to me.")

                        mWitnessQA(0).Answer = "I was waiting for the bus, and he was passing by."
                        mWitnessQA(1).Answer = "Nothing, really. She ran up to him and started flirting with him."
                        mWitnessQA(3).Answer = "He was just saying he had a girlfriend, and wasn't interested."
                        mWitnessQA(4).Answer = "Nope, I was staying out of it completely."
                End Select

                .SpeechLines.Add("I think that's him, coming back to the scene.")
                .SpeechLines.Add("I'll give you a statement if you want.")
            End With

            w.QAItems = mWitnessQA
            w.PhysicalCondition = "Subject speaking normally, and making eye contact when he speaks."

            Dim pWitness As Entities.Persona = Functions.GetPersonaForPed(w)
            Dim pNewWitness As New Entities.Persona(w, pWitness.Gender, pWitness.BirthDay, 0, pWitness.Forename, pWitness.Surname, Entities.ELicenseState.Valid, 0, False, False, False)
            Functions.SetPersonaForPed(w, pNewWitness)

            Peds.Add(w)

            If Common.IsComputerPlusRunning() Then
                AddPedToCallout(v)
                AddPedToCallout(w)
            End If

            If PerformPedChecks() Then
                Return baseReturn
            Else
                Return False
            End If
        End Function

        Public Overrides Function OnCalloutAccepted() As Boolean
            Dim v As Victim = GetPed("Victim1")
            If v IsNot Nothing Then
                If v.Exists Then
                    v.CreateBlip()
                End If
            End If

            Dim w As Witness = GetPed("Witness1")
            If w IsNot Nothing Then
                If w.Exists Then
                    w.CreateBlip()
                End If
            End If

            Return MyBase.OnCalloutAccepted()
        End Function

        Public Overrides Sub OnArrivalAtScene()
            MyBase.OnArrivalAtScene()

            Dim v As Victim = GetPed("Victim1")
            If v.Exists() Then v.TurnToFaceEntity(Game.LocalPlayer.Character)

            Dim w As Witness = GetPed("Witness1")
            If w.Exists() Then w.TurnToFaceEntity(Game.LocalPlayer.Character)

            GameFiber.StartNew(
                Sub()
                    Game.DisplayHelp("Press " & Config.SpeakKey.ToString() & " to speak with a subject.", 8000)
                    GameFiber.Sleep(8000)
                    Game.DisplayHelp("Use the interaction menu to observe or question a subject.", 8000)
                    GameFiber.Sleep(8000)
                    Game.DisplayHelp("Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout", 8000)
                End Sub)
        End Sub

        Private Sub ReturnSuspectToScene()
            GameFiber.StartNew(
                Sub()
                    Try
                        Dim s As Suspect = GetPed("Suspect1")

                        If s.Exists() Then
                            s.Tasks.ClearImmediately()

                            Dim v3SusToPlayer As Vector3 = (Game.LocalPlayer.Character.Position - s.Position)
                            v3SusToPlayer.Normalize()
                            Dim hdg As Single = MathHelper.ConvertDirectionToHeading(v3SusToPlayer)

                            s.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.GetOffsetPositionFront(5.0F), hdg, 1.0F).WaitForCompletion()
                            s.TurnToFaceEntity(Game.LocalPlayer.Character)
                            'Stealth.Common.Natives.Functions.CallByName("TASK_GO_TO_ENTITY", s.Model, Game.LocalPlayer, -1, 8.0F, 1.0F, 1073741824, 0)
                        End If
                    Catch ex As Exception
                        Logger.LogVerboseDebug("Error returning suspect to scene -- " & ex.Message)
                    End Try
                End Sub)
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            If Game.LocalPlayer.Character.IsDead Then
                Exit Sub
            End If

            Dim v As Victim = GetPed("Victim1")
            Dim w As Witness = GetPed("Witness1")
            Dim s As Suspect = GetPed("Suspect1")

            If State = CalloutState.UnitResponding Then
                For Each p As PedBase In Peds
                    If p.IsDead Then
                        p.Resurrect()
                        p.Health = p.MaxHealth
                    End If
                Next
            End If

            If State = CalloutState.AtScene Then
                If Game.LocalPlayer.Character.IsOnFoot Then
                    If suspectReturning = False Then
                        suspectReturning = True
                        ReturnSuspectToScene()
                    End If
                End If
            End If

            If Game.IsKeyDown(Config.SpeakKey) Then
                SpeakToSubject(v, w, s)
            End If

            ArrestCheck(v, w, s)

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    Radio.CallIsCode4(Me.ScriptInfo.Name, anyoneArrested)
                    [End]()
                End If
            End If
        End Sub

        Private Sub SpeakToSubject(ByRef v As Victim, ByRef w As Witness, ByRef s As Suspect)
            If v IsNot Nothing AndAlso v.Exists Then
                If Game.LocalPlayer.Character.Position.DistanceTo(v.Position) < 3 Then
                    v.Speak()
                    Exit Sub
                End If
            Else
                Game.DisplayNotification("Assault Callout crashed")
                [End]()
            End If

            If w IsNot Nothing AndAlso w.Exists Then
                If Game.LocalPlayer.Character.Position.DistanceTo(w.Position) < 3 Then
                    w.Speak()

                    If suspectIdentified = False Then
                        If s IsNot Nothing AndAlso s.Exists Then
                            suspectIdentified = True
                            s.CreateBlip()

                            If Common.IsComputerPlusRunning() Then
                                AddPedToCallout(s)
                            End If
                        End If
                    End If

                    Exit Sub
                End If
            Else
                Game.DisplayNotification("Assault Callout crashed")
                [End]()
            End If

            If suspectIdentified = True Then
                If s IsNot Nothing AndAlso s.Exists Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(s.Position) < 3 Then
                        s.Speak()
                    End If
                Else
                    Game.DisplayNotification("Assault Callout crashed")
                    [End]()
                End If
            End If
        End Sub

        Private Sub ArrestCheck(ByRef v As Victim, ByRef w As Witness, ByRef s As Suspect)
            ArrestCheck(v)
            ArrestCheck(w)
            ArrestCheck(s)

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

                        If p.Name = "Suspect1" Then
                            If p.Inventory.Weapons.Count > 0 Then
                                p.Inventory.Weapons.Clear()

                                If weaponFactor = 1 Then
                                    Game.DisplayNotification("While searching the suspect, you find/remove a ~r~knife~w~.")
                                ElseIf weaponFactor = 2 Then
                                    Game.DisplayNotification("While searching the suspect, you find/remove a ~r~pistol~w~.")
                                End If
                            End If
                        End If
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

        Private Enum EType
            Genuine
            Fabricated
        End Enum

    End Class

End Namespace