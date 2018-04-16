Imports LSPD_First_Response.Engine
Imports LSPD_First_Response.Mod.API
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Rage.Native
Imports Stealth.Common.Extensions
Imports Stealth.Common.Models.QuestionWindow
Imports Stealth.Common.Scripting.Peds
Imports Stealth.Plugins.Code3Callouts.Models.Interiors
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports System.Windows.Forms

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Burglary In Progress", CalloutProbability.High)>
    Public Class Burglary
        Inherits CalloutBase

        Dim pSuspect As Suspect = Nothing
        Dim SuspectModels As String() = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01"}

        Dim pVictim As Victim = Nothing
        Dim VictimModels As String() = {"A_F_Y_GenHot_01", "A_F_Y_Hippie_01", "A_F_Y_Hipster_01", "A_F_Y_BevHills_01", "A_F_Y_BevHills_02", "A_F_M_Tourist_01", "A_F_M_FatWhite_01", "A_F_M_Business_02"}

        Private mDoorHelpDisplayed As Boolean = False
        Private mBackupDoorHelpDisplayed As Boolean = False
        Private mIsPlayerIndoors As Boolean = False
        Private mSuspectReacted As Boolean = False
        Private mDoorsEnabled As Boolean = False
        Private mIsBackupOnScene As Boolean = False
        Private mIsBackupOnFoot As Boolean = False
        Private mIsScenarioOver As Boolean = False
        Private mAIUnitNumber As Integer = gRandom.Next(27, 50)

        Private mPursuit As LHandle = Nothing
        Private mPursuitInitiated As Boolean = False
        Private mAimWeapons As Boolean = True

        Dim pCop1 As Cop = Nothing, pCop2 As Cop = Nothing, vPolice1 As Vehicles.Vehicle = Nothing
        Dim mBackupDriveTo As Vector3

        Dim mPlayerVehDataSaved As Boolean = False
        Dim mPlayerVehModel As String
        Dim mPlayerVehPos As Vector3
        Dim mPlayerVehHdg As Single

        Public Sub New()
            MyBase.New("Burglary In Progress", CallResponseType.Code_2)
            RadioCode = 459
            CrimeEnums = {DISPATCH.CRIMES.CODE_459, DISPATCH.CRIMES.POSSIBLE_459}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "Female RP reports that she came home to find her front door open, and heard someone moving around inside."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "The front door is damaged, and appears to have been kicked in. RP is on scene awaiting police arrival; approach with caution."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += String.Format("UPDATE: {0} is responding, and will meet RP outside.", Common.gUnitNumber)
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += String.Format("UPDATE: 1-ADAM-{0} will back up {1}; entry not to be made until 1-ADAM-{0}'s arrival.", mAIUnitNumber, Common.gUnitNumber)

            Objective = "Apprehend the ~r~suspect!~n~Respond quickly, but quietly!"
        End Sub

        Private mHouse As Residence = Nothing

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim baseReturn As Boolean = MyBase.OnBeforeCalloutDisplayed()

            If baseReturn = False Then
                Return False
            End If

            If mHouse Is Nothing OrElse SpawnPoint = Vector3.Zero Then
                Logger.LogVerboseDebug("Failed to find house for burglary; callout aborted")
                Return False
            End If

            Logger.LogVerboseDebug("Spawn point found")

            Dim nearbyPeds As List(Of Ped) = GetPedsNearPosition(mHouse.EntryPoint, 30.0F, GetEntitiesFlags.ConsiderHumanPeds)
            For Each p As Ped In nearbyPeds
                If p.Exists() Then
                    p.Delete()
                End If
            Next

            Dim suspectSpawn As SpawnPoint = mHouse.Interior.HidingPlaces(gRandom.Next(0, mHouse.Interior.HidingPlaces.Count))
            pSuspect = New Suspect("Suspect1", SuspectModels(gRandom.Next(SuspectModels.Count)), suspectSpawn.Position, suspectSpawn.Heading, False)
            pSuspect.DisplayName = "Suspect"
            pSuspect.BlockPermanentEvents = True
            pSuspect.MakePersistent()
            Peds.Add(pSuspect)
            Logger.LogVerboseDebug("Suspect created")

            RandomizeSuspectStory()
            Logger.LogVerboseDebug("Suspect story randomized")

            Dim weaponFactor As Integer = gRandom.Next(6)

            If weaponFactor = 5 Then
                pSuspect.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 32, True)
            ElseIf weaponFactor = 4
                pSuspect.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_KNIFE"), 32, True)
            End If

            pVictim = New Victim("Victim1", VictimModels(gRandom.Next(VictimModels.Count)), mHouse.EntryPoint, 0)
            pVictim.DisplayName = "Victim"
            pVictim.BlockPermanentEvents = True
            pVictim.MakePersistent()

            With pVictim.SpeechLines
                .Add("Oh Officer, thank god you're here!!")
                .Add("I came home a few minutes ago, and I went to unlock my door...")
                .Add("And I found it was already open!! And the lock was damaged!")
                .Add("I didn't know what had happened at first, then I peeked through the door...")
                .Add("And I heard someone moving around inside!")
                .Add("I was SO scared, I just ran out here and called 911!")
                .Add("I live alone, Officer. I think someone has broken in!")
                .Add("Please, I need you to go inside for me! I'm so scared right now!")
            End With

            Peds.Add(pVictim)

            If Common.IsComputerPlusRunning() Then
                AddPedToCallout(pVictim)
            End If

            Return True
        End Function

        Public Overrides Function OnCalloutAccepted() As Boolean
            GameFiber.StartNew(
                Sub()
                    GameFiber.Sleep(10000)

                    Game.DisplayNotification(String.Format("~b~1-ADAM-{0}~w~: ~w~Dispatch, 1-ADAM-{0} will be backing up ~b~{1}.", mAIUnitNumber, Common.gUnitNumber))
                    Radio.AIOfficerResponding()
                End Sub)

            Return MyBase.OnCalloutAccepted()
        End Function

        Public Overrides Sub OnArrivalAtScene()
            Logger.LogVerboseDebug("Onarrival at burglary")

            MyBase.OnArrivalAtScene()

            If Game.LocalPlayer.Character.CurrentVehicle.Exists() Then
                Game.LocalPlayer.Character.CurrentVehicle.MakePersistent()
            Else
                If Game.LocalPlayer.Character.LastVehicle.Exists() Then
                    Game.LocalPlayer.Character.LastVehicle.MakePersistent()
                End If
            End If

            If pVictim.Exists() Then
                pVictim.CreateBlip()
                pVictim.TurnToFaceEntity(Game.LocalPlayer.Character)

                GameFiber.StartNew(
                    Sub()
                        While Game.LocalPlayer.Character.IsInAnyVehicle(False)
                            GameFiber.Yield()
                        End While

                        If pVictim.Exists() Then
                            Dim heading As Single = Common.GetHeadingToPoint(pVictim.Position, Game.LocalPlayer.Character.Position)
                            pVictim.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.GetOffsetPositionFront(5), heading, 2.5F).WaitForCompletion()
                            pVictim.TurnToFaceEntity(Game.LocalPlayer.Character)
                        End If
                    End Sub)
            End If

            Dim mBlip As New Blip(mHouse.EntryPoint)
            mBlip.Scale = 0.75
            mBlip.Color = Drawing.Color.Yellow
            Markers.Add(mBlip)

            Radio.SergeantMessage("~r~DO NOT ~w~make entry until backup arrives, over")
            SpawnBackup()
        End Sub

        Private Sub SpawnBackup()
            If Common.IsPlayerInLosSantos Then
                Dim lspdModels As String() = {"POLICE", "POLICE2", "POLICE3", "POLICE4"}
                vPolice1 = New Vehicles.Vehicle(lspdModels(gRandom.Next(lspdModels.Count)), World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250)))
            Else
                Dim sheriffModels As String() = {"SHERIFF", "SHERIFF", "SHERIFF2", "POLICE4"}
                vPolice1 = New Vehicles.Vehicle(sheriffModels(gRandom.Next(sheriffModels.Count)), World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(250)))
            End If
            vPolice1.IsSirenOn = True
            vPolice1.IsSirenSilent = True
            vPolice1.MakePersistent()

            pCop1 = Models.Peds.Cop.Create("Cop1", vPolice1.GetOffsetPosition(Vector3.RelativeLeft * 1.5), 180, Convert.ToBoolean(gRandom.Next(2)))
            pCop1.CreateBlip()
            pCop2 = Models.Peds.Cop.Create("Cop2", vPolice1.GetOffsetPositionRight(1.5), 180, Convert.ToBoolean(gRandom.Next(2)))
            pCop2.CreateBlip()
            Peds.Add(pCop1)
            Peds.Add(pCop2)
            Functions.SetPedAsCop(pCop1)
            Functions.SetPedAsCop(pCop2)

            pCop1.WarpIntoVehicle(vPolice1, -1)
            pCop2.WarpIntoVehicle(vPolice1, vPolice1.GetFreePassengerSeatIndex())

            GameFiber.StartNew(
                Sub()
                    GameFiber.Sleep(3000)

                    mBackupDriveTo = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(5))
                    With Game.LocalPlayer.Character
                        If .CurrentVehicle.Exists() Then
                            mBackupDriveTo = .CurrentVehicle.GetOffsetPosition(Vector3.RelativeBack * 5)
                        Else
                            If .LastVehicle.Exists() Then
                                mBackupDriveTo = .LastVehicle.GetOffsetPosition(Vector3.RelativeBack * 5)
                            End If
                        End If
                    End With

                    pCop1.Tasks.DriveToPosition(mBackupDriveTo, 15, VehicleDrivingFlags.Emergency, 5).WaitForCompletion()
                    mIsBackupOnScene = True
                End Sub)

            GameFiber.StartNew(
                Sub()
                    GameFiber.Sleep(30000)
                    If mIsBackupOnScene = False Then Game.DisplayHelp("Backup unit stuck or taking too long? ~n~Press ~b~0 ~w~to speed it up.", 10000)
                End Sub)
        End Sub

        Private Sub TeleportBackup()
            If mIsBackupOnScene = False Then
                If vPolice1.Exists() Then
                    vPolice1.Position = mBackupDriveTo
                End If
            End If
        End Sub

        Private Sub RandomizeSuspectStory()
            If pSuspect.Exists Then
                Dim mSuspectQAItems As New List(Of QAItem)
                mSuspectQAItems.Add(New QAItem("What were you doing in the house?"))
                mSuspectQAItems.Add(New QAItem("Why did you break in?"))
                mSuspectQAItems.Add(New QAItem("What were you trying to steal?"))
                mSuspectQAItems.Add(New QAItem("Have you ever done this before?"))

                Dim mStoryFactor As Integer = gRandom.Next(5)

                With pSuspect.SpeechLines

                    Select Case mStoryFactor
                        Case 0
                            .Add("Officer, you're making a mistake, this is MY house!")

                            With mSuspectQAItems
                                .Item(0).Answer = "I live here! This is my house!"
                                .Item(1).Answer = "My girlfriend left after I did, and I..uhh..forgot my keys!"
                                .Item(2).Answer = "Er...nothing!"
                                .Item(3).Answer = "No, sir!"

                                .Add(New QAItem("What's your girlfriend's name?", "Uhhh...Kristen. Wait, no...Kirsten. No, Melissa! That's it!"))
                            End With

                        Case 1
                            .Add("Heyyyy man, you got it all wrong! This is my buddy's place.")

                            With mSuspectQAItems
                                .Item(0).Answer = "I was checking to see if my buddy was home!"
                                .Item(1).Answer = "I didn't! The door was unlocked."
                                .Item(2).Answer = "Nothing, man!"
                                .Item(3).Answer = "Yeah! He lets me in through the window all the time!"

                                Dim mFakeName As String = ""
                                Select Case gRandom.Next(0, 5)
                                    Case 0
                                        mFakeName = "Uhhh...Jeff Favignano!!"
                                    Case 1
                                        mFakeName = "His name is...Zach something. Zach...House? Houseknecht?"
                                    Case 2
                                        mFakeName = "I want my lawyer."
                                    Case 3
                                        mFakeName = "Sarah...yeah, that's my sister. Its her house."
                                    Case Else
                                        mFakeName = "Its uh...I, uh...forget."
                                End Select

                                .Add(New QAItem("What's your buddy's name?", mFakeName))
                            End With

                        Case 2
                            .Add("Come on...you know their insurance will cover it! What's the harm?")

                            With mSuspectQAItems
                                .Item(0).Answer = "What the hell do you think I was doing?"
                                .Item(1).Answer = "Have you seen that fine piece of ass that lives here? Damn!"
                                .Item(2).Answer = "Dentures. I fence false teeth for a living."
                                .Item(3).Answer = "Have I done this before? Do I look like a rookie to you?"
                            End With

                        Case 3
                            .Add("I'm sorry, man! Its just...I gotta have it!! I need the money!!")

                            With mSuspectQAItems
                                .Item(0).Answer = "I just...I need it!!"
                                .Item(1).Answer = "I need to get some Coke, man. That don't come cheap!"
                                .Item(2).Answer = "Anything I could find...cash, jewelry, anything."
                                .Item(3).Answer = "Do I really gotta answer that?"
                            End With

                        ' Add one more
                        Case Else
                            .Add("Go to hell, pig! I ain't saying nothin to you!")

                            With mSuspectQAItems
                                .Item(0).Answer = "I want my lawyer!"
                                .Item(1).Answer = "LAWYER!"
                                .Item(2).Answer = "No hablo Ingles!"
                                .Item(3).Answer = "Hey! Take a hint! LAWYER!"
                            End With
                    End Select
                End With

                pSuspect.QAItems = mSuspectQAItems
            Else
            End If
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            ProcessTeleportKey()
            ProcessBackupArrival()
            ProcessHouseEntryExit()
            ProcessSuspectReaction()
            ProcessEndOfSituation()

            If Game.IsKeyDown(Config.SpeakKey) Then
                If pVictim.Exists() AndAlso pVictim.IsAlive AndAlso Game.LocalPlayer.Character.DistanceTo(pVictim.Position) <= 3 Then
                    pVictim.Speak()
                Else
                    If pSuspect.Exists() AndAlso pSuspect.IsAlive AndAlso Game.LocalPlayer.Character.DistanceTo(pSuspect.Position) <= 3 Then
                        pSuspect.Speak()
                    End If
                End If
            End If

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    [End]()
                End If
            End If
        End Sub

        Private Sub ProcessTeleportKey()
            If mIsBackupOnScene = True Or mIsScenarioOver = True Then
                Exit Sub
            End If

            If mIsBackupOnScene = False Then
                If Game.IsKeyDown(Keys.D0) Then
                    TeleportBackup()
                End If
            End If
        End Sub

        Private Sub ProcessBackupArrival()
            If mIsBackupOnScene = False Or mIsBackupOnFoot = True Then
                Exit Sub
            End If

            If mIsBackupOnScene = True AndAlso mIsBackupOnFoot = False Then
                mIsBackupOnFoot = True
                If vPolice1.Exists() Then vPolice1.IsSirenOn = False

                GameFiber.StartNew(
                    Sub()
                        If pCop1.Exists() AndAlso pCop1.CurrentVehicle.Exists() Then pCop1.Tasks.LeaveVehicle(LeaveVehicleFlags.None)
                        pCop1.Tasks.Clear()
                        pCop1.KeepTasks = True
                        pCop1.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 64, True)
                        pCop1.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeBack * 3), Common.GetHeadingToPoint(pCop1.Position, Game.LocalPlayer.Character.Position), 2.5F)
                        mDoorsEnabled = True
                    End Sub)

                GameFiber.StartNew(
                    Sub()
                        If pCop2.Exists() AndAlso pCop2.CurrentVehicle.Exists() Then pCop2.Tasks.LeaveVehicle(LeaveVehicleFlags.None)
                        pCop2.Tasks.Clear()
                        pCop2.KeepTasks = True
                        pCop2.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 64, True)
                        pCop2.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeBack * 3), Common.GetHeadingToPoint(pCop1.Position, Game.LocalPlayer.Character.Position), 2.5F)
                        mDoorsEnabled = True
                    End Sub)

                GameFiber.StartNew(
                    Sub()
                        With pCop1
                            While True
                                GameFiber.Yield()

                                If .Exists = False OrElse .DistanceTo(Game.LocalPlayer.Character.Position) < 5 Then
                                    Exit While
                                End If
                            End While

                            If .Exists() Then
                                .Tasks.Clear()
                                .KeepTasks = True
                                .Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, Vector3.RelativeBack * 6.0F)
                            End If
                        End With
                    End Sub)

                GameFiber.StartNew(
                    Sub()
                        With pCop2
                            While True
                                GameFiber.Yield()

                                If .Exists = False OrElse .DistanceTo(Game.LocalPlayer.Character.Position) < 5 Then
                                    Exit While
                                End If
                            End While

                            If .Exists() Then
                                .Tasks.Clear()
                                .KeepTasks = True
                                .Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, Vector3.RelativeBack * 6.0F)
                            End If
                        End With
                    End Sub)
            End If
        End Sub

        Private Sub ProcessHouseEntryExit()
            If mDoorsEnabled = True Then
                If mIsPlayerIndoors = False Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(mHouse.EntryPoint) < 2.0F Then
                        If mDoorHelpDisplayed = False Then
                            mDoorHelpDisplayed = True
                            Game.DisplayHelp("Press ~b~CTRL + E~w~ to enter.", 5000)

                            GameFiber.StartNew(
                                Sub()
                                    GameFiber.Sleep(10000)
                                    mDoorHelpDisplayed = False
                                End Sub)
                        End If

                        If Game.IsKeyDown(Keys.E) AndAlso Game.IsKeyDownRightNow(Keys.ControlKey) Then
                            EnterHouse()
                        End If
                    End If
                Else
                    If Game.LocalPlayer.Character.Position.DistanceTo(mHouse.Interior.InteriorSpawnPoint.Position) < 2.0F Then
                        If mDoorHelpDisplayed = False Then
                            mDoorHelpDisplayed = True
                            Game.DisplayHelp("Press ~b~CTRL + E~w~ to exit.", 5000)

                            GameFiber.StartNew(
                                Sub()
                                    GameFiber.Sleep(10000)
                                    mDoorHelpDisplayed = False
                                End Sub)
                        End If

                        If Game.IsKeyDown(Keys.E) AndAlso Game.IsKeyDownRightNow(Keys.ControlKey) Then
                            ExitHouse()
                        End If
                    End If
                End If
            Else
                If mIsPlayerIndoors = False AndAlso mIsBackupOnScene = False Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(mHouse.EntryPoint) < 2.0F Then
                        If mBackupDoorHelpDisplayed = False Then
                            mBackupDoorHelpDisplayed = True
                            Game.DisplayHelp("Wait for your ~b~backup unit ~w~before entering!", 5000)

                            GameFiber.StartNew(
                                Sub()
                                    GameFiber.Sleep(10000)
                                    mBackupDoorHelpDisplayed = False
                                End Sub)
                        End If
                    End If
                End If
            End If
        End Sub

        Private Sub MakeCopsAimWeapons()
            If pCop1.Exists() Then
                pCop1.Tasks.PlayAnimation("combat@chg_positionpose_b", "aimb_calm_fwd", 1.0F, AnimationFlags.SecondaryTask Or AnimationFlags.UpperBodyOnly Or AnimationFlags.StayInEndFrame)
            End If

            If pCop2.Exists() Then
                pCop2.Tasks.PlayAnimation("combat@chg_positionpose_b", "aimb_calm_fwd", 1.0F, AnimationFlags.SecondaryTask Or AnimationFlags.UpperBodyOnly Or AnimationFlags.StayInEndFrame)
            End If
        End Sub

        Private Sub EnterHouse(Optional ByVal pForcePedsInside As Boolean = False)
            If Game.LocalPlayer.Character.LastVehicle.Exists() Then
                Game.LocalPlayer.Character.LastVehicle.MakePersistent()

                mPlayerVehModel = Game.LocalPlayer.Character.LastVehicle.Model.Name
                mPlayerVehPos = Game.LocalPlayer.Character.LastVehicle.Position
                mPlayerVehHdg = Game.LocalPlayer.Character.LastVehicle.Heading
                mPlayerVehDataSaved = True
            End If

            Game.FadeScreenOut(1800, True)

            mHouse.Interior.LoadInterior()

            Game.LocalPlayer.Character.Position = mHouse.Interior.InteriorSpawnPoint.Position
            Game.LocalPlayer.Character.Heading = mHouse.Interior.InteriorSpawnPoint.Heading

            If mIsScenarioOver = False OrElse pForcePedsInside = True Then
                If pCop1.Exists() Then
                    pCop1.Position = mHouse.Interior.InteriorSpawnPoint.Position
                    pCop1.Heading = mHouse.Interior.InteriorSpawnPoint.Heading
                End If

                If pCop2.Exists() Then
                    pCop2.Position = mHouse.Interior.InteriorSpawnPoint.Position
                    pCop2.Heading = mHouse.Interior.InteriorSpawnPoint.Heading
                End If
            End If

            mIsPlayerIndoors = True
            Game.FadeScreenIn(1800, True)

            If mIsScenarioOver = False Then
                MakeCopsAimWeapons()
            End If
        End Sub

        Private Sub ExitHouse(Optional ByVal pForcePedsOutside As Boolean = False)
            Game.FadeScreenOut(1800, True)

            Dim mOutsideSpawnpoint As Vector3 = mHouse.EntryPoint

            mIsPlayerIndoors = False

            If mIsScenarioOver = False Or pForcePedsOutside = True Then
                If Game.LocalPlayer.Character.LastVehicle.Exists() Then
                    mOutsideSpawnpoint = Game.LocalPlayer.Character.LastVehicle.GetOffsetPositionFront(4.0F)
                Else
                    Dim mUseAIVeh As Boolean = True

                    If mPlayerVehDataSaved = True Then
                        Dim mValidModels As String() = {"POLICE", "POLICE2", "POLICE3", "POLICE4", "SHERIFF", "SHERIFF2", "FBI", "FBI2"}

                        If mValidModels.Contains(mPlayerVehModel.ToUpper()) Then
                            Try
                                Dim vNewPlayerVeh As New Vehicle(mPlayerVehModel, mPlayerVehPos, mPlayerVehHdg)

                                If vNewPlayerVeh.Exists() Then
                                    vNewPlayerVeh.MakePersistent()
                                    mOutsideSpawnpoint = vNewPlayerVeh.GetOffsetPositionFront(4.0F)
                                    mUseAIVeh = False
                                End If
                            Catch ex As Exception
                                Logger.LogTrivial("Error respawning player vehicle -- " & ex.Message)
                            End Try
                        End If
                    End If

                    If mUseAIVeh = True OrElse mPlayerVehDataSaved = False Then
                        If vPolice1.Exists() Then
                            mOutsideSpawnpoint = vPolice1.GetOffsetPositionFront(4.0F)
                        Else
                            mOutsideSpawnpoint = PedHelper.GetSafeCoordinatesForPed(World.GetNextPositionOnStreet(mHouse.EntryPoint))
                            If mOutsideSpawnpoint = Vector3.Zero Then mOutsideSpawnpoint = World.GetNextPositionOnStreet(mHouse.EntryPoint)
                        End If
                    End If
                End If

                If mOutsideSpawnpoint.DistanceTo(mHouse.EntryPoint) > 200 Then mOutsideSpawnpoint = World.GetNextPositionOnStreet(mHouse.EntryPoint)

                If pSuspect.Exists() Then pSuspect.Position = mOutsideSpawnpoint
                    If pCop1.Exists() Then pCop1.Position = mOutsideSpawnpoint
                    If pCop2.Exists() Then pCop2.Position = mOutsideSpawnpoint
                End If

                Game.LocalPlayer.Character.Position = mOutsideSpawnpoint

            Game.FadeScreenIn(1800, True)
        End Sub

        Private Sub ProcessSuspectReaction()
            If mIsPlayerIndoors = True AndAlso mSuspectReacted = False Then
                Try
                    Dim mRoomSuspect As UInteger = NativeFunction.CallByHash(Of UInteger)(&H47C2A06D4F5F424BUL, Common.GetNativeArgument(pSuspect))
                    Dim mRoomPlayer As UInteger = NativeFunction.CallByHash(Of UInteger)(&H47C2A06D4F5F424BUL, Common.GetNativeArgument(Game.LocalPlayer.Character))
                    Dim mRoomCop1 As UInteger = 0
                    Dim mRoomCop2 As UInteger = 0

                    If pCop1.Exists() Then mRoomCop1 = NativeFunction.CallByHash(Of UInteger)(&H47C2A06D4F5F424BUL, Common.GetNativeArgument(pCop1))
                    If pCop2.Exists() Then mRoomCop2 = NativeFunction.CallByHash(Of UInteger)(&H47C2A06D4F5F424BUL, Common.GetNativeArgument(pCop2))

                    If mRoomPlayer = mRoomSuspect Then
                        MakeSuspectReact()
                    Else
                        If mRoomSuspect = mRoomCop1 Or mRoomSuspect = mRoomCop2 Then
                            MakeSuspectReact()
                        End If
                    End If
                Catch ex As Exception
                    If pSuspect.DistanceTo(Game.LocalPlayer.Character.Position) < 10 Then
                        MakeSuspectReact()
                    End If
                End Try
            End If
        End Sub

        Private Sub MakeSuspectReact()
            If mSuspectReacted = True Then Exit Sub

            mSuspectReacted = True

            'If pSuspect.Exists() = True AndAlso pSuspect.HasAttachedBlip() = False Then pSuspect.CreateBlip()

            If pCop1.Exists() Then pCop1.Tasks.Clear()
            If pCop2.Exists() Then pCop2.Tasks.Clear()

            Dim reaxFactor As Integer = gRandom.Next(3)

            If reaxFactor <= 1 Then
                'Flee
                If mPursuitInitiated = False Then
                    TriggerPursuit()
                End If
            ElseIf reaxFactor = 2
                'Attack
                If mPursuitInitiated = False Then
                    TriggerPursuit()
                    If pSuspect.Exists() Then pSuspect.AttackPed(Game.LocalPlayer.Character)
                End If
            End If
        End Sub

        Private Sub TriggerPursuit()
            mPursuitInitiated = True
            mPursuit = Common.CreatePursuit()
            If pSuspect.Exists() Then Functions.AddPedToPursuit(mPursuit, pSuspect)
            If pCop1.Exists() Then Functions.AddCopToPursuit(mPursuit, pCop1)
            If pCop2.Exists() Then Functions.AddCopToPursuit(mPursuit, pCop2)
        End Sub

        Private Sub ProcessEndOfSituation()
            If mIsPlayerIndoors = True AndAlso mIsScenarioOver = False Then

                If pSuspect.Exists() AndAlso (pSuspect.IsArrested OrElse pSuspect.IsDead) Then
                    mIsScenarioOver = True
                    mAimWeapons = False

                    If Common.IsComputerPlusRunning() Then
                        AddPedToCallout(pSuspect)
                    End If

                    If pCop1.Exists() Then
                        pCop1.Tasks.ClearImmediately()
                        pCop1.Inventory.Weapons.Clear()
                        pCop1.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 64, False)
                    End If

                    If pCop2.Exists() Then
                        pCop2.Tasks.ClearImmediately()
                        pCop2.Inventory.Weapons.Clear()
                        pCop2.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 64, False)
                    End If

                    GameFiber.StartNew(
                        Sub()
                            GameFiber.Sleep(5000)
                            ExitHouse(True)
                            GameFiber.Sleep(3000)

                            If pSuspect.Exists() And pSuspect.IsAlive Then
                                Game.DisplayHelp("Ensure that you question the suspect using the interaction menu.", 5000)
                            Else
                                Game.DisplayHelp("Check the suspect for ID using the interaction menu, and call the coroner.", 5000)
                            End If

                            GameFiber.Sleep(5000)
                            Game.DisplayHelp("Press ~b~" & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " ~w~to end this callout when the situation is over.", 8000)
                        End Sub)

                    GameFiber.StartNew(
                        Sub()
                            If pVictim.Exists() Then
                                pVictim.SpeechLines = New List(Of String)

                                With pVictim.SpeechLines
                                    .Add("Thank you SO much, Officer!")
                                    .Add("I don't know what I would have done without you!")
                                    .Add("I swear, I don't know where that man came from. I don't know him!!")
                                    .Add("He's not going to come back, is he??")
                                End With
                            End If
                        End Sub)
                End If
            End If
        End Sub

        Public Overrides Sub [End]()
            If IsCalloutActive = False Then
                MyBase.End()
                Exit Sub
            End If

            DeleteBlips()

            Radio.CallIsCode4(Me.ScriptInfo.Name)

            If vPolice1.Exists() AndAlso pCop1.Exists() AndAlso pCop2.Exists() Then
                If pCop2.DistanceTo(vPolice1.Position) > 100 Then
                    MyBase.End()
                    Exit Sub
                End If
            Else
                MyBase.End()
                Exit Sub
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
                End Sub)

            MyBase.End()
        End Sub

        Public Overrides Function CalculateSpawnpoint() As Vector3
            mHouse = InteriorDatabase.GetRandomHouse()

            If mHouse IsNot Nothing Then
                Return mHouse.EntryPoint
            Else
                Return Vector3.Zero
            End If
        End Function

        Public Overrides ReadOnly Property IsFixedSpawnPoint As Boolean
            Get
                Return True
            End Get
        End Property

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