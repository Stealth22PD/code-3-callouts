Imports LSPD_First_Response.Mod.API
Imports Rage
Imports RAGENativeUI
Imports RAGENativeUI.Elements
Imports Stealth.Plugins.Code3Callouts.Models.Ambient
Imports Stealth.Plugins.Code3Callouts.Models.Ambient.EventTypes
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Stealth.Plugins.Code3Callouts.Models.Callouts
Imports Stealth.Plugins.Code3Callouts.Util

Friend Module Driver

    Friend gMenuPool As New MenuPool()
    Friend WithEvents mInteractionMenu As UIMenu = Nothing

    Private mRadioOptions As IEnumerable(Of Object) = {"Status OK", "Help Me!"}
    Friend mRadioDispatch As New UIMenuListItem("Radio Dispatch", mRadioOptions.ToList(), 0)

    Friend mObserveSubject As New UIMenuItem("Observe Subject")
    Friend mSpeakToSubject As New UIMenuItem("Speak to Subject")
    Friend mQuestionSubject As New UIMenuItem("Question Subject")
    Friend mAskForID As New UIMenuItem("Ask Subject for ID")
    Friend mAskToFollow As New UIMenuItem("Ask Subject to Follow")

    Dim callouts As IEnumerable(Of Object) = Common.GetCalloutFriendlyNames()
    Friend mStartCallout As New UIMenuListItem("Start Callout", callouts.ToList(), 0)

    Friend mEndCallout As New UIMenuItem("End Callout")
    Friend mCloseMenu As New UIMenuItem("Close Menu")

    Private mLastStatusCheck As DateTime
    Private mTimeUntilNextStatusCheck As Integer
    Private mDispatchCallingPlayer As Boolean = False
    Private mDispatchCalled As DateTime

    Friend Sub RunAmbientEvents()
        If Config.AmbientEventsEnabled Then
            GameFiber.StartNew(
            Sub()
                While gIsPlayerOnDuty
                    If gActiveAmbientEvent Is Nothing Then
                        Dim ts As TimeSpan = (DateTime.Now - gLastAmbientEvent)

                        If ts.TotalSeconds > gTimeUntilNextAmbientEvent Then
                            Dim a As AmbientBase = GetRandomAmbientEvent()
                            If a IsNot Nothing Then
                                If a.Start() Then
                                    gActiveAmbientEvent = a
                                Else
                                    a = Nothing
                                    gLastAmbientEvent = DateTime.Now
                                    Common.RandomizeAmbientInterval()
                                End If
                            Else
                                gLastAmbientEvent = DateTime.Now
                                Common.RandomizeAmbientInterval()
                            End If
                        End If
                    Else
                        If gActiveAmbientEvent.Active = False Then
                            gActiveAmbientEvent = Nothing
                            gLastAmbientEvent = DateTime.Now
                            Common.RandomizeAmbientInterval()
                        End If
                    End If

                    GameFiber.Yield()
                End While
            End Sub)
        End If
    End Sub

    Private Function GetRandomAmbientEvent() As AmbientBase
        If Functions.IsCalloutRunning = False AndAlso Functions.IsPlayerPerformingPullover = False Then
            Dim eventFactor As Integer = gRandom.Next(1, 100)

            Select Case eventFactor
                Case 1 To 10
                    Return New Mugging()
                Case 11 To 40
                    Return New FightInProgress()
                Case 41 To 70
                    Return New StolenVehicle()
                Case Else
                    Return New Carjacking()
            End Select

        Else
            Return Nothing
        End If
    End Function

    Friend Sub RunDispatchStatusCheck()
        If Config.EnableDispatchStatusCheck Then
            ListenForResponseKeys()
            mLastStatusCheck = DateTime.Now
            mTimeUntilNextStatusCheck = gRandom.Next(15, 30)

            GameFiber.StartNew(
                Sub()
                    While gIsPlayerOnDuty
                        Dim ts As TimeSpan = (DateTime.Now - mLastStatusCheck)

                        If ts.TotalMinutes > mTimeUntilNextStatusCheck Then
                            AskPlayerForStatus()
                        End If

                        GameFiber.Yield()
                    End While
                End Sub)
        End If
    End Sub

    Private Sub ListenForResponseKeys()
        If Config.StatusRadioResponseOK = Keys.None OrElse Config.StatusRadioResponseHelpMe = Keys.None Then Exit Sub

        GameFiber.StartNew(
            Sub()
                While gIsPlayerOnDuty
                    If mDispatchCallingPlayer Then
                        If Config.StatusRadioResponseOK <> Keys.None Then
                            If Game.IsKeyDown(Config.StatusRadioResponseOK) Then
                                If Config.StatusRadioResponseOKModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.StatusRadioResponseOKModKey) Then
                                    mDispatchCallingPlayer = False
                                    OfficerRespondOK()
                                End If
                            End If
                        End If

                        If Config.StatusRadioResponseHelpMe <> Keys.None Then
                            If Game.IsKeyDown(Config.StatusRadioResponseHelpMe) Then
                                If Config.StatusRadioResponseHelpMeModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.StatusRadioResponseHelpMeModKey) Then
                                    mDispatchCallingPlayer = False
                                    OfficerRespondHelpMe()
                                End If
                            End If
                        End If
                    End If

                    GameFiber.Yield()
                End While
            End Sub)
    End Sub

    Private Sub AskPlayerForStatus()
        mLastStatusCheck = DateTime.Now
        CallPlayerForStatus()
        mTimeUntilNextStatusCheck = gRandom.Next(Config.MinTimeBetweenStatusChecks, Config.MaxTimeBetweenAmbientEvents)

        GameFiber.StartNew(
            Sub()
                Game.DisplayHelp("Use the interaction menu to respond to Dispatch.", 10000)
                GameFiber.Sleep(10000)
                Game.DisplayHelp(String.Format("You can open the menu by pressing ~b~{0}.", Config.GetInteractionMenuKey()), 10000)
                GameFiber.Sleep(10000)

                If mDispatchCallingPlayer Then
                    Game.DisplayHelp("If you don't respond, Dispatch will assume you need backup!", 10000)
                    GameFiber.Sleep(10000)
                End If
            End Sub)

        GameFiber.StartNew(
            Sub()
                GameFiber.Sleep(30000)

                If mDispatchCallingPlayer Then
                    CallPlayerForStatus()
                    Game.DisplayHelp("Dispatch is waiting for your response!!", 10000)
                    GameFiber.Sleep(10000)

                    If mDispatchCallingPlayer Then
                        Game.DisplayHelp(String.Format("You can open the menu by pressing ~b~{0}.", Config.GetInteractionMenuKey()), 10000)
                        GameFiber.Sleep(10000)
                    End If
                End If
            End Sub)

        GameFiber.StartNew(
            Sub()
                mDispatchCallingPlayer = True
                mDispatchCalled = DateTime.Now

                GameFiber.Sleep(60000)

                If mDispatchCallingPlayer Then
                    mDispatchCallingPlayer = False
                    CallBackupForPlayer(True)
                End If
            End Sub)
    End Sub

    Private Sub CallPlayerForStatus()
        Radio.DispatchCallingUnit()
        Game.DisplayNotification(String.Format("~b~Dispatch~w~: ~w~{0}, what's your status?", Common.gUnitNumber))
    End Sub

    Friend Sub CallBackupForPlayer(ByVal pNoResponse As Boolean)
        GameFiber.StartNew(
            Sub()
                If Game.LocalPlayer.Character.Exists() Then
                    Dim destPoint As Vector3 = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position)

                    Radio.OfficerCode99(pNoResponse)
                    GameFiber.Sleep(5000)

                    Functions.RequestBackup(destPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit)
                    Functions.RequestBackup(destPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit)
                    Functions.RequestBackup(destPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit)
                End If
            End Sub)
    End Sub

    Private Sub OfficerRespondOK()
        GameFiber.StartNew(
            Sub()
                If Functions.IsPlayerPerformingPullover() Then
                    Radio.UnitMessage(String.Format("{0} is Code 6 on a traffic stop", gUnitNumber))
                ElseIf Functions.IsCalloutRunning() Then
                    If Game.LocalPlayer.Character.IsOnFoot Then
                        Radio.UnitMessage(String.Format("{0}, show me Code 6", gUnitNumber))
                    Else
                        Radio.UnitMessage(String.Format("{0} currently 10-8", gUnitNumber))
                    End If
                Else
                    If Game.LocalPlayer.Character.IsOnFoot Then
                        Radio.UnitMessage(String.Format("{0}, show me Code 6", gUnitNumber))
                    Else
                        Radio.UnitMessage(String.Format("{0} currently 10-8", gUnitNumber))
                    End If
                End If

                GameFiber.Sleep(1000)
                Radio.DispatchMessage("10-4", True)
                Radio.DispatchAcknowledge()
            End Sub)
    End Sub

    Private Sub OfficerRespondHelpMe()
        GameFiber.StartNew(
            Sub()
                Radio.UnitMessage(String.Format("{0}, I'm Code 99 at this time", gUnitNumber))
                GameFiber.Sleep(1000)
                Radio.DispatchMessage("10-4", True)
                Radio.DispatchAcknowledge()
                GameFiber.Sleep(2000)
                CallBackupForPlayer(False)
            End Sub)
    End Sub

    Private Sub StartCallout(ByVal name As String)
        GameFiber.StartNew(
            Sub()
                Try
                    If String.IsNullOrWhiteSpace(name) = False Then
                        If Config.RegisteredCallouts.Contains(name) Then
                            If Functions.IsCalloutRunning() Then
                                Functions.StopCurrentCallout()
                            End If

                            Functions.StartCallout(name)
                        Else
                            Game.DisplayNotification(String.Format("~r~ERROR: ~b~{0} ~w~ is ~r~disabled ~w~in the config file.", name))
                        End If
                    End If
                Catch ex As Exception
                    Game.DisplayNotification("~r~ERROR: ~w~" & ex.Message)
                    Util.Logger.LogTrivial("Error starting callout -- " & ex.ToString())
                End Try
            End Sub)
    End Sub

    Friend Sub InitializeMenu()
        gMenuPool = New MenuPool()
        mInteractionMenu = New UIMenu("Code 3 Callouts", "~b~INTERACTION MENU")
        mInteractionMenu.MouseControlsEnabled = False
        mInteractionMenu.AllowCameraMovement = True

        mInteractionMenu.AddItem(mRadioDispatch)
        mInteractionMenu.AddItem(mObserveSubject)
        mInteractionMenu.AddItem(mSpeakToSubject)
        mInteractionMenu.AddItem(mQuestionSubject)
        mInteractionMenu.AddItem(mAskForID)
        mInteractionMenu.AddItem(mAskToFollow)
        mInteractionMenu.AddItem(mStartCallout)
        mInteractionMenu.AddItem(mEndCallout)
        mInteractionMenu.AddItem(mCloseMenu)

        mInteractionMenu.RefreshIndex()

        gMenuPool.Add(mInteractionMenu)

        AddHandler Game.FrameRender, AddressOf ProcessMenu
        RegisterMenuEvents()
        ProcessMenuItems()
    End Sub

    Private Sub RegisterMenuEvents()
        AddHandler mInteractionMenu.OnItemSelect, AddressOf mInteractionMenu_OnItemSelect
    End Sub

    Private Sub UnregisterMenuEvents()
        RemoveHandler mInteractionMenu.OnItemSelect, AddressOf mInteractionMenu_OnItemSelect
    End Sub

    Private Sub ProcessMenuItems()
        GameFiber.StartNew(
            Sub()
                While gIsPlayerOnDuty
                    GameFiber.Yield()

                    mRadioDispatch.Enabled = False
                    mObserveSubject.Enabled = False
                    mSpeakToSubject.Enabled = False
                    mQuestionSubject.Enabled = False
                    mAskForID.Enabled = False
                    mAskToFollow.Enabled = False
                    mEndCallout.Enabled = False

                    If mDispatchCallingPlayer Then mRadioDispatch.Enabled = True

                    If Common.ClosestPed IsNot Nothing AndAlso Common.ClosestPed.Exists() Then
                        With Common.ClosestPed
                            mAskForID.Enabled = True

                            If .PhysicalCondition <> "" Then
                                mObserveSubject.Enabled = True
                            End If

                            If .IsAlive() Then
                                If .SpeechLines.Count > 0 Then
                                    mSpeakToSubject.Enabled = True
                                End If

                                If .QAItems IsNot Nothing Then
                                    mQuestionSubject.Enabled = True
                                End If

                                mAskForID.Text = "Ask Subject for ID"

                                If .IsOnFoot = True And .IsInCombat = False Then
                                    If Game.LocalPlayer.Character.IsOnFoot Then
                                        mAskToFollow.Enabled = True

                                        If Common.FollowMePed IsNot Nothing And Common.FollowMePed.Exists() Then
                                            mAskToFollow.Text = "Stop Following"
                                        Else
                                            mAskToFollow.Text = "Ask Subject to Follow"
                                        End If
                                    Else
                                        mAskToFollow.Enabled = False
                                    End If
                                End If
                            Else
                                mAskForID.Text = "Check Subject for ID"
                            End If
                        End With
                    End If

                    If Functions.IsCalloutRunning() Then
                        mEndCallout.Enabled = True
                    End If
                End While
            End Sub)
    End Sub

    Private Sub ProcessMenu(sender As Object, e As GraphicsEventArgs)
        If gMenuPool Is Nothing Then
            Exit Sub
        End If

        If Game.IsKeyDown(Config.InteractionMenuKey) Then
            If Config.InteractionMenuModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.InteractionMenuModKey) Then
                If mInteractionMenu.Visible = False Then
                    mInteractionMenu.Visible = True
                Else
                    mInteractionMenu.Visible = False
                End If
            End If
        End If

        gMenuPool.ProcessMenus()
    End Sub

    Private Sub mInteractionMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer)
        If sender.Equals(mInteractionMenu) = True Then
            If selectedItem Is mRadioDispatch Then
                If mDispatchCallingPlayer Then
                    mDispatchCallingPlayer = False

                    Select Case mRadioDispatch.Index
                        Case 0
                            'OK
                            OfficerRespondOK()

                        Case 1
                            'HELP!
                            OfficerRespondHelpMe()
                    End Select

                End If

            ElseIf selectedItem Is mStartCallout Then
                'Start callout
                Dim mCalloutToStart As String = mStartCallout.IndexToItem(mStartCallout.Index)
                StartCallout(mCalloutToStart)

            ElseIf selectedItem Is mEndCallout
                If Functions.IsCalloutRunning() Then
                    Functions.StopCurrentCallout()
                End If

            ElseIf selectedItem Is mCloseMenu
                mInteractionMenu.Visible = False

            End If
        End If
    End Sub

    Friend Sub MakePedImmuneToTrafficEvents(ByVal p As Ped)
        Try
            If Common.IsTrafficPolicerRunning() Then
                If p IsNot Nothing AndAlso p.Exists() Then
                    TrafficPolicerFunctions.MakePedImmuneToAmbientEvents(p)
                End If
            End If
        Catch ex As Exception
            Logger.LogVerboseDebug("Error making ped immune to Traffic Policer events -- " & ex.ToString())
        End Try
    End Sub

End Module