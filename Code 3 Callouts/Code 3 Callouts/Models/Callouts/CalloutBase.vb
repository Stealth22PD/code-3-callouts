Imports LSPD_First_Response
Imports LSPD_First_Response.Mod.API
Imports Rage
Imports RAGENativeUI
Imports RAGENativeUI.Elements
Imports Stealth.Common.Extensions
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports System.Windows.Forms
Imports Stealth.Common.Models
Imports Stealth.Common.Models.QuestionWindow

Namespace Models.Callouts

    Public MustInherit Class CalloutBase
        Inherits LSPD_First_Response.Mod.Callouts.Callout
        Implements ICalloutBase, IPoliceIncident

        Private mIsQAModalActive As Boolean = False
        Private mIsObserveWindowActive As Boolean = False
        Private mAssignedToAI As Boolean = False

        Public Sub New(ByVal pCalloutMessage As String, Optional ByVal pResponseType As CallResponseType = CallResponseType.Code_2)
            State = CalloutState.Created
            CalloutMessage = pCalloutMessage
            ResponseType = pResponseType
            Peds = New List(Of PedBase)
            Markers = New List(Of Blip)
            Vehicles = New List(Of Vehicles.Vehicle)
            FollowMePed = Nothing
            PedsToIgnore = New List(Of PoolHandle)
            FoundPedSafeSpawn = False
            CallDispatchTime = DateTime.Now.AddMinutes(-2)
            CalloutID = Guid.Empty
        End Sub

        Private Sub RegisterMenuEvents()
            AddHandler mInteractionMenu.OnItemSelect, AddressOf mInteractionMenu_OnItemSelect
        End Sub

        Private Sub UnregisterMenuEvents()
            RemoveHandler mInteractionMenu.OnItemSelect, AddressOf mInteractionMenu_OnItemSelect
        End Sub

        Private Sub mInteractionMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer)
            If sender.Equals(mInteractionMenu) = True Then
                With Common.ClosestPed
                    If selectedItem Is mObserveSubject Then
                        If Common.ClosestPed IsNot Nothing Then
                            Dim mPhysicalCondition As String = .PhysicalCondition
                            Logger.LogTrivialDebug("selectedItem Is mObserveSubject")
                            'Observe
                            Logger.LogTrivialDebug(".Exists() = " & .Exists())
                            Logger.LogTrivialDebug(".PhysicalCondition = " & .PhysicalCondition)
                            Logger.LogTrivialDebug("mIsObserveWindowActive = " & mIsObserveWindowActive)
                            If .Exists() AndAlso .PhysicalCondition <> "" AndAlso mIsObserveWindowActive = False Then
                                GameFiber.StartNew(
                                    Sub()
                                        Logger.LogTrivialDebug("GameFiber started")
                                        mIsObserveWindowActive = True

                                        Dim mModal As New ModalWindow("Observation Window", mPhysicalCondition, False)
                                        Logger.LogTrivialDebug("ModalWindow created")
                                        mModal.Show()
                                        Logger.LogTrivialDebug("ModalWindow shown")
                                        mModal = Nothing
                                        Logger.LogTrivialDebug("ModalWindow = null")

                                        mIsObserveWindowActive = False
                                        Logger.LogTrivialDebug("GameFiber end")
                                    End Sub)
                                Logger.LogTrivialDebug("If end")
                            End If
                        Else
                            Logger.LogTrivialDebug("Common.ClosestPed Is null")
                        End If
                    ElseIf selectedItem Is mSpeakToSubject
                        'Speak
                        If .Exists() AndAlso .IsAlive() Then
                            .Speak()
                        End If
                    ElseIf selectedItem Is mQuestionSubject
                        'Question
                        If .Exists() AndAlso .IsAlive() AndAlso .QAItems IsNot Nothing AndAlso mIsQAModalActive = False Then
                            OpenQuestionWindow()
                        End If
                    ElseIf selectedItem Is mAskForID
                        'Ask for ID
                        If .Exists() Then
                            Dim pData As Engine.Scripting.Entities.Persona = Functions.GetPersonaForPed(Common.ClosestPed)
                            Dim IDTextFormat As String = "~b~{0}~n~~y~{1}, ~w~Born: ~y~{2}"
                            Dim IDText As String = String.Format(IDTextFormat, pData.FullName, pData.Gender.ToString(), pData.BirthDay.ToString("M/d/yyyy"))

                            Game.DisplayNotification("mpcharselect", "mp_generic_avatar", "STATE ISSUED IDENTIFICATION", pData.FullName.ToUpper(), IDText)
                        End If
                    ElseIf selectedItem Is mAskToFollow
                        'Ask to Follow
                        AskPedToFollowOfficer()
                    End If
                End With
            End If
        End Sub

        Public Property IsCADModalActive As Boolean = False

        Public Overridable ReadOnly Property IsFixedSpawnPoint As Boolean Implements ICalloutBase.IsFixedSpawnPoint
            Get
                Return False
            End Get
        End Property

        Protected Function GetRandomSpawnPoint(ByVal pMin As Single, ByVal pMax As Single) As Vector3 Implements IPoliceIncident.GetRandomSpawnPoint
            Return World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(gRandom.Next(pMin, pMax)))
        End Function

        Public Overridable Function CalculateSpawnpoint() As Vector3 Implements ICalloutBase.CalculateSpawnpoint
            Return Vector3.Zero
        End Function

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            'Base spawn point
            If IsFixedSpawnPoint = False Then
                SpawnPoint = GetRandomSpawnPoint(150.0F, 401.0F)
                Dim iSpawnTries As Integer = 0
                Dim pMax As Single = 401.0F

                If RequiresSafePedPoint = True Then
                    While iSpawnTries <= 5
                        SpawnPoint = PedHelper.GetSafeCoordinatesForPed(SpawnPoint)

                        If SpawnPoint = Vector3.Zero Then
                            pMax += 250.0F
                        Else
                            FoundPedSafeSpawn = True
                            Exit While
                        End If

                        iSpawnTries += 1
                    End While

                    If SpawnPoint = Vector3.Zero Then
                        'Fail
                        Logger.LogVerboseDebug("Failed to find safe spawn point for callout")
                        SpawnPoint = GetRandomSpawnPoint(150.0F, 501.0F)
                        'Return False
                    End If
                End If
            Else
                SpawnPoint = CalculateSpawnpoint()
            End If

            If ShowAreaBlipBeforeAccepting Then
                ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30.0F)
            End If

            CalloutPosition = SpawnPoint

            If Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 100 Then
                Logger.LogVerboseDebug("Player is too close; callout aborted")
                Return False
            End If

            Return MyBase.OnBeforeCalloutDisplayed()
        End Function

        Public Overrides Sub OnCalloutDisplayed()
            Radio.DispatchCallout(Me.ScriptInfo.Name, SpawnPoint, CrimeEnums, ResponseType)
            State = CalloutState.Dispatched
            MyBase.OnCalloutDisplayed()

            Dim mComputerPlusRunning As Boolean = Common.IsComputerPlusRunning()
            If mComputerPlusRunning = True Then
                CreateCADCallout()
            End If
        End Sub

        Private Sub CreateCADCallout()
            Logger.LogTrivialDebug("ComputerPlus is running")

            Dim mResponse As ComputerPlus.EResponseType = ComputerPlus.EResponseType.Code_2
            If Me.ResponseType = CallResponseType.Code_3 Then mResponse = ComputerPlus.EResponseType.Code_3

            CalloutID = ComputerPlusFunctions.CreateCallout(CalloutMessage, RadioCode, SpawnPoint, mResponse, CallDetails,
                                                          ComputerPlus.ECallStatus.Created)
        End Sub

        Public Overrides Function OnCalloutAccepted() As Boolean
            Radio.AcknowledgeCallout(Me.ScriptInfo.Name, ResponseType)
            IsCalloutActive = True

            If _SkipRespondingState = False Then
                State = CalloutState.UnitResponding
                CreateBlip()
            End If

            RegisterMenuEvents()
            DisplayObjective()

            If CallDetails = "" Then
                CallDetails = "No Further Information Available"
            End If

            If Common.IsComputerPlusRunning() Then
                ComputerCallAccepted()
            Else
                Game.DisplayHelp("Download ~b~LSPDFR Computer+ ~w~to view complete details on this call!")
            End If

            Return MyBase.OnCalloutAccepted()
        End Function

        Private Sub ComputerCallAccepted()
            Logger.LogTrivialDebug("Computer+ running!")
            ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.Dispatched)
            ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.Unit_Responding)

            Game.DisplayHelp("You can view details about the call with ~b~LSPDFR Computer+~w~.")
        End Sub

        Public Overrides Sub OnCalloutNotAccepted()
            Logger.LogVerboseDebug("Callout not accepted")
            UnregisterMenuEvents()
            IsCalloutActive = False

            State = CalloutState.Cancelled

            For Each p In Peds
                If p IsNot Nothing Then
                    If p.Exists Then
                        p.Delete()
                    End If
                End If
            Next

            For Each v In Vehicles
                If v IsNot Nothing Then
                    If v.Exists Then
                        v.Delete()
                    End If
                End If
            Next

            If Common.IsComputerPlusRunning Then
                CADAssignToAI()
                mAssignedToAI = True
            End If

            MyBase.OnCalloutNotAccepted()

            Radio.AIOfficerResponding()
        End Sub

        Private Sub CADAssignToAI()
            ComputerPlusFunctions.AssignCallToAIUnit(CalloutID)
        End Sub

        Public Sub DisplayObjective() Implements ICalloutBase.DisplayObjective
            If String.IsNullOrWhiteSpace(Objective) = False Then
                Dim mTitle As String = "CODE 3 CALLOUTS"
                Dim mSubtitle As String = "~b~" & CalloutMessage

                Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", mTitle, mSubtitle, Objective)
            End If
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()
            Common.ClosestPed = (From x In Peds Where x.Exists() AndAlso x.DistanceTo(Game.LocalPlayer.Character.Position) < 3.0F).FirstOrDefault()

            If Game.LocalPlayer.Character.IsDead Then
                OfficerDown()
                [End]()
            End If

            If State = CalloutState.UnitResponding Then
                If Game.LocalPlayer.Character.Position.DistanceTo(SpawnPoint) < 30.0F Then
                    Radio.UnitIsOnScene()
                    State = CalloutState.AtScene
                    OnArrivalAtScene()
                End If
            ElseIf State = CalloutState.AtScene Then
                If Game.IsKeyDown(Config.AskToFollowKey) Then
                    If Config.AskToFollowModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.AskToFollowModKey) Then
                        AskPedToFollowOfficer()
                    End If
                End If
            End If
        End Sub

        Protected Sub AskPedToFollowOfficer() Implements ICalloutBase.AskPedToFollowOfficer
            If Game.LocalPlayer.Character.IsInAnyVehicle(True) = False Then
                'If player is on foot...
                If FollowMePed Is Nothing Then
                    FollowMePed = Game.LocalPlayer.Character.GetNearbyPeds(1).FirstOrDefault()

                    If FollowMePed IsNot Nothing AndAlso FollowMePed.Exists Then
                        If FollowMePed.Position.DistanceTo(Game.LocalPlayer.Character.Position) < 3 Then
                            Dim isValid As Boolean = True
                            Dim isArrested As Boolean = FollowMePed.IsArrested OrElse FollowMePed.IsGettingArrested

                            With FollowMePed
                                If isArrested OrElse .IsDead OrElse .IsInAnyVehicle(True) OrElse PedsToIgnore.Contains(.Handle) OrElse .IsFleeing OrElse .IsInCombat OrElse .IsShooting Then
                                    isValid = False
                                    Exit Sub
                                End If
                            End With

                            If isValid = True Then
                                FollowMePed.Tasks.ClearImmediately()
                                FollowMePed.Tasks.FollowToOffsetFromEntity(Game.LocalPlayer.Character, Vector3.RelativeBack * 3.0F)
                                Game.DisplayHelp("The ped is now following you.", 3000)
                            Else
                                FollowMePed = Nothing
                            End If
                        Else
                            FollowMePed = Nothing
                        End If
                    Else
                        FollowMePed = Nothing
                    End If
                Else
                    If FollowMePed IsNot Nothing AndAlso FollowMePed.Exists Then
                        FollowMePed.Tasks.Clear()
                        FollowMePed = Nothing
                        Game.DisplayHelp("The ped is no longer following you.", 3000)
                    Else
                        FollowMePed = Nothing
                    End If
                End If
            End If
        End Sub

        Private Sub OfficerDown() Implements ICalloutBase.OfficerDown
            Radio.OfficerDown()
            DeleteEntities()
            [End]()
        End Sub

        Public Overridable Sub OnArrivalAtScene() Implements ICalloutBase.OnArrivalAtScene
            DeleteBlip()

            If Common.IsComputerPlusRunning() Then
                CADAtScene()
            End If
        End Sub

        Private Sub CADAtScene()
            ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.At_Scene)
        End Sub

        Protected Sub AddPedToCallout(ByVal p As PedBase)
            If Common.IsComputerPlusRunning() Then
                If p.Type <> PedType.Cop Then
                    ComputerPlusFunctions.AddPedToCallout(CalloutID, p)
                End If
            End If
        End Sub

        Protected Sub AddVehicleToCallout(ByVal v As Vehicles.Vehicle)
            If Common.IsComputerPlusRunning() Then
                ComputerPlusFunctions.AddVehicleToCallout(CalloutID, v)
            End If
        End Sub

        Protected Sub DeleteEntities() Implements ICalloutBase.DeleteEntities
            For Each p As PedBase In Peds
                If p IsNot Nothing Then
                    If p.Exists = True Then
                        p.DeleteBlip()
                        p.Delete()
                    End If
                End If
            Next

            Peds.Clear()

            For Each m As Blip In Markers
                If m IsNot Nothing Then
                    If m.IsValid = True Then
                        m.Delete()
                        m = Nothing
                    End If
                End If
            Next

            Markers.Clear()
        End Sub

        Protected Sub DeleteBlips()
            For Each p As PedBase In Peds
                If p IsNot Nothing Then
                    If p.Exists = True Then
                        p.DeleteBlip()
                    End If
                End If
            Next

            For Each m As Blip In Markers
                If m IsNot Nothing Then
                    If m.IsValid = True Then
                        m.Delete()
                        m = Nothing
                    End If
                End If
            Next

            Markers.Clear()
        End Sub

        Public Overrides Sub [End]()
            Logger.LogVerboseDebug("CalloutBase.End()")
            MyBase.[End]()
            UnregisterMenuEvents()
            Common.ClosestPed = Nothing
            IsCalloutActive = False

            If mInteractionMenu.Visible Then
                mInteractionMenu.Visible = False
            End If

            DeleteBlip()

            PedsToIgnore.Clear()

            For Each p As PedBase In Peds
                If p IsNot Nothing Then
                    If p.Exists = True Then
                        'p.DeleteSearchArea()
                        p.DeleteBlip()

                        If p.IsInAnyPoliceVehicle() = False AndAlso p.IsArrested() = False Then
                            p.Dismiss()
                        End If
                    End If
                End If
            Next

            Peds.Clear()

            For Each m As Blip In Markers
                If m IsNot Nothing Then
                    If m.IsValid = True Then
                        m.Delete()
                        m = Nothing
                    End If
                End If
            Next

            Markers.Clear()

            For Each v In Vehicles
                If v IsNot Nothing Then
                    If v.Exists Then
                        v.Dismiss()
                    End If
                End If
            Next

            Vehicles.Clear()

            State = CalloutState.Completed

            If Common.IsComputerPlusRunning() Then
                If mAssignedToAI = False Then
                    If State = CalloutState.Created Or State = CalloutState.Dispatched Or State = CalloutState.UnitResponding Then
                        CADCancel()
                    Else
                        CADConclude()
                    End If
                End If
            End If
        End Sub

        Private Sub CADConclude()
            ComputerPlusFunctions.ConcludeCallout(CalloutID)
        End Sub

        Private Sub CADCancel()
            ComputerPlusFunctions.CancelCallout(CalloutID)
        End Sub

        Sub CreateBlip() Implements ICalloutBase.CreateBlip
            CallBlip = New Blip(CalloutPosition)
            CallBlip.Color = Drawing.Color.Yellow
            CallBlip.EnableRoute(Drawing.Color.Yellow)
        End Sub

        Sub DeleteBlip() Implements ICalloutBase.DeleteBlip
            If CallBlip IsNot Nothing Then
                If CallBlip.IsValid Then
                    CallBlip.DisableRoute()
                    CallBlip.Delete()
                End If
            End If
        End Sub

        Public Overridable Sub OpenQuestionWindow()
            GameFiber.StartNew(
                Sub()
                    LogVerboseDebug("QAItems.Count = " & Common.ClosestPed.QAItems.Count)

                    Dim mQAItems As New List(Of QAItem)
                    mQAItems.AddRange(Common.ClosestPed.QAItems)
                    LogVerboseDebug("mQAItems.Count = " & mQAItems.Count)

                    For Each x In mQAItems
                        LogVerboseDebug("Q = " & x.Question)
                        LogVerboseDebug("A = " & x.Answer)
                    Next

                    mIsQAModalActive = True

                    Dim mModal As New QuestionWindow("Question Subject", mQAItems, True)
                    mModal.Show()

                    mModal = Nothing
                    mIsQAModalActive = False
                End Sub)
        End Sub

        Public Overridable Function PerformPedChecks() As Boolean Implements ICalloutBase.PerformPedChecks
            Dim isValid As Boolean = True

            For Each p As PedBase In Peds
                If p IsNot Nothing Then
                    If p.Exists Then
                        AddMinimumDistanceCheck(5.0F, p.Position)
                    Else
                        isValid = False
                        Exit For
                    End If
                Else
                    isValid = False
                    Exit For
                End If
            Next

            If isValid = True Then
                Return True
            Else
                For Each p As PedBase In Peds
                    If p IsNot Nothing Then
                        If p.Exists Then
                            p.Delete()
                        End If
                    End If
                Next

                Peds.Clear()
                Return False
            End If
        End Function

        Function GetPed(ByVal pName As String) As PedBase Implements IPoliceIncident.GetPed
            Return (From x In Peds Where x.Name = pName Select x).FirstOrDefault()
        End Function

        Function GetVehicle(ByVal pName As String) As Vehicles.Vehicle Implements IPoliceIncident.GetVehicle
            Return (From x In Vehicles Where x.Name = pName Select x).FirstOrDefault()
        End Function

        Public Property RadioCode As Integer Implements IPoliceIncident.RadioCode
        Public Property CrimeEnums As List(Of DISPATCH.CRIMES) Implements IPoliceIncident.CrimeEnums
        Public Property ResponseType As CallResponseType Implements ICalloutBase.ResponseType
        Public Property Objective As String Implements ICalloutBase.Objective
        Public Property SpawnPoint As Vector3 Implements IPoliceIncident.SpawnPoint
        Public MustOverride ReadOnly Property RequiresSafePedPoint() As Boolean Implements ICalloutBase.RequiresSafePedPoint
        Public Property CallDispatchTime As DateTime Implements ICalloutBase.CallDispatchTime
        Public Overloads Property State As CalloutState Implements ICalloutBase.State
        Public Property CallBlip As Blip Implements ICalloutBase.CallBlip
        Public Property CallDetails As String Implements ICalloutBase.CallDetails
        Public Property Peds As List(Of PedBase) Implements IPoliceIncident.Peds
        Public Property Vehicles As List(Of Vehicles.Vehicle) Implements IPoliceIncident.Vehicles
        Public Property Markers As List(Of Blip) Implements ICalloutBase.Markers
        Public MustOverride ReadOnly Property ShowAreaBlipBeforeAccepting() As Boolean Implements ICalloutBase.ShowAreaBlipBeforeAccepting
        Public Overridable Property SkipRespondingState() As Boolean Implements ICalloutBase.SkipRespondingState

        Public Property PedsToIgnore As List(Of Rage.PoolHandle) Implements ICalloutBase.PedsToIgnore
        Public Property FoundPedSafeSpawn As Boolean Implements ICalloutBase.FoundPedSafeSpawn
        Public Property CalloutID As Guid Implements ICalloutBase.CalloutID

    End Class

End Namespace