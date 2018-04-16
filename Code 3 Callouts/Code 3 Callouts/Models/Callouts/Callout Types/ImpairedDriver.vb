Imports LSPD_First_Response
Imports LSPD_First_Response.Mod.API
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Common.Extensions
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports System.Windows.Forms

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Impaired Driver", CalloutProbability.Medium)>
    Public Class ImpairedDriver
        Inherits CalloutBase

        Dim PedModels As List(Of String) = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01",
                                         "A_F_Y_GenHot_01", "A_F_Y_Hippie_01", "A_F_Y_Hipster_01", "A_F_Y_BevHills_01", "A_F_Y_BevHills_02", "A_F_M_Tourist_01", "A_F_M_FatWhite_01", "A_F_M_Business_02",
                                        "A_M_M_BevHills_01", "A_M_M_GenFat_01", "A_M_M_Business_01", "A_M_M_Golfer_01", "A_M_M_Skater_01", "A_M_M_Salton_01", "A_M_M_Tourist_01"}.ToList()

        Dim VehModels As List(Of String) = {"Blista", "Felon", "Jackal", "Oracle", "Asea", "Emperor", "Fugitive", "Ingot", "Premier", "Primo", "Stanier", "Stratum"}.ToList()
        Dim suspectVisual As Boolean = False
        Dim pursuit As LHandle
        Dim pursuitInitiated As Boolean = False
        Dim officerRespondedCode3 As Boolean = False

        Public Sub New()
            MyBase.New("Impaired Driver", CallResponseType.Code_2)
            RadioCode = 502
            CrimeEnums = {DISPATCH.CRIMES.A_DUI, DISPATCH.CRIMES.DRIVER_UNDER_INFLUENCE, DISPATCH.CRIMES.POSSIBLE_502, DISPATCH.CRIMES.CODE_502}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "Caller reports a driver who appears to be intoxicated, and driving all over the road."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine

            Objective = "Track down the ~y~driver.~n~~w~Stop them from hurting anyone!"
        End Sub

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim baseReturn As Boolean = MyBase.OnBeforeCalloutDisplayed()

            If baseReturn = False Then
                Return False
            End If

            SkipRespondingState = True

            Dim model1 As String = VehModels(gRandom.Next(VehModels.Count))

            Dim vehicle1 As New Vehicles.Vehicle(model1, World.GetNextPositionOnStreet(SpawnPoint), gRandom.Next(360))
            vehicle1.Name = "Vehicle1"
            vehicle1.MakePersistent()
            vehicle1.SetRandomColor()

            Dim driver1 As New Suspect("Driver1", PedModels(gRandom.Next(PedModels.Count)), vehicle1.Position.Around(3), 0, False)
            driver1.DisplayName = "Driver"
            driver1.SetIsDrunk(True)
            driver1.MakePersistent()
            driver1.Armor = 69
            driver1.SpeechLines = PedHelper.RandomizeImpairedDriverStory(gRandom.Next(15))

            Try
                Dim animSet As New AnimationSet("move_m@drunk@verydrunk")
                animSet.LoadAndWait()
                driver1.MovementAnimationSet = animSet
            Catch ex As Exception
                Logger.LogVerboseDebug("Error animating ped -- " & ex.Message)
            End Try

            Peds.Add(driver1)
            Vehicles.Add(vehicle1)

            If driver1.Exists AndAlso vehicle1.Exists Then driver1.WarpIntoVehicle(vehicle1, -1)
            If driver1.Exists AndAlso vehicle1.Exists Then driver1.Tasks.CruiseWithVehicle(vehicle1, 12, VehicleDrivingFlags.Normal)

            GameFiber.StartNew(
                Sub()
                    GameFiber.Sleep(10000)

                    While True
                        'driver1.Tasks.PerformDrivingManeuver(VehicleManeuver.SwerveLeft)
                        'GameFiber.Sleep(2000)
                        If driver1.Exists AndAlso vehicle1.Exists AndAlso driver1.IsInAnyVehicle(False) Then
                            If Functions.IsPlayerPerformingPullover() Then
                                Exit While
                            End If

                            If driver1.Exists AndAlso vehicle1.Exists AndAlso driver1.IsInAnyVehicle(False) Then driver1.Tasks.PerformDrivingManeuver(vehicle1, VehicleManeuver.SwerveRight)
                            GameFiber.Sleep(2000)
                            If driver1.Exists AndAlso vehicle1.Exists AndAlso driver1.IsInAnyVehicle(False) Then driver1.Tasks.CruiseWithVehicle(vehicle1, 12, VehicleDrivingFlags.Normal)

                            GameFiber.Sleep(10000)
                            GameFiber.Yield()
                        Else
                            Exit While
                        End If
                    End While
                End Sub)

            If PerformPedChecks() Then
                If Common.IsComputerPlusRunning() Then
                    AddVehicleToCallout(vehicle1)
                    AddPedToCallout(driver1)
                End If

                Return baseReturn
            Else
                Logger.LogVerboseDebug("basereturn = false")
                Return False
            End If
        End Function

        Public Overrides Function OnCalloutAccepted() As Boolean
            State = CalloutState.AtScene
            OnArrivalAtScene()

            Return MyBase.OnCalloutAccepted()
        End Function

        Public Overrides Sub OnArrivalAtScene()
            'MyBase.OnArrivalAtScene()

            Dim d1 As Suspect = GetPed("Driver1")
            If d1 IsNot Nothing AndAlso d1.Exists Then
                d1.CreateBlip()
            Else
                Game.DisplayNotification("Impaired Driver Callout crashed")
                Logger.LogVerboseDebug("d1 null or !exists")
                [End]()
                Exit Sub
            End If

            If Common.IsComputerPlusRunning() Then
                CADAtScene()
            End If

            Dim v As Vehicles.Vehicle = GetVehicle("Vehicle1")
            If v IsNot Nothing AndAlso v.Exists Then
                v.FillColorValues()
                CallDetails += String.Format("Suspect vehicle is a {0} {1}, License # {2}", v.PrimaryColorName, v.Model.Name, v.LicensePlate)
                Radio.DispatchMessage(String.Format("Suspect vehicle is a {0} {1}, License # {2}", v.PrimaryColorName, v.Model.Name, v.LicensePlate), True)
            End If

            GameFiber.StartNew(
                Sub()
                    GameFiber.Sleep(4000)
                    Game.DisplayHelp("Pull over the impaired driver.", 8000)
                    GameFiber.Sleep(8000)
                    Game.DisplayHelp("Press " & Config.SpeakKey.ToString() & " to talk to the driver.", 8000)
                End Sub)
        End Sub

        Private Sub CADAtScene()
            If Common.IsComputerPlusRunning() Then
                ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.At_Scene)
            End If
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            If Game.LocalPlayer.Character.IsDead Then
                Logger.LogVerboseDebug("player dead")
                Exit Sub
            End If

            Dim d1 As Suspect = GetPed("Driver1")
            Dim v As Vehicles.Vehicle = GetVehicle("Vehicle1")

            If Game.IsKeyDown(Config.SpeakKey) Then
                SpeakToSubject(d1)
            End If

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    Radio.CallIsCode4(Me.ScriptInfo.Name)
                    [End]()
                End If
            End If

            If suspectVisual = False Then
                If Game.LocalPlayer.Character.Position.DistanceTo(d1.Position) < 60 Then
                    suspectVisual = True
                    Radio.SuspectSpotted()

                    If Game.LocalPlayer.Character.IsInAnyVehicle(False) = True Then
                        Dim copcar As Vehicle = Game.LocalPlayer.Character.CurrentVehicle

                        If copcar IsNot Nothing Then
                            If copcar.Exists AndAlso copcar.HasSiren Then
                                If copcar.IsSirenOn = True AndAlso copcar.IsSirenSilent = False Then
                                    officerRespondedCode3 = True
                                    pursuitInitiated = True
                                    pursuit = Common.CreatePursuit()
                                    d1.AddToPursuit(pursuit)

                                    Game.DisplayNotification("The suspect heard your siren and is fleeing.")
                                    Radio.SergeantMessage("Exercise proper siren use on Code 2 calls, over")
                                End If
                            End If
                        End If
                    End If
                End If
            End If

            Dim vehTowed As Boolean = False
            If v IsNot Nothing Then
                If v.Exists = False Then
                    vehTowed = True
                    Logger.LogVerboseDebug("vehTowed = True")
                End If
            Else
                vehTowed = True
            End If

            If vehTowed = True AndAlso ArrestCheck(d1) Then
                Logger.LogVerboseDebug("vehTowed and ped arrested or dead")

                If d1.Exists() Then
                    Radio.CallIsCode4(Me.ScriptInfo.Name, d1.IsArrested)
                Else
                    Radio.CallIsCode4(Me.ScriptInfo.Name, False)
                End If

                [End]()
            End If
        End Sub

        Private Function ArrestCheck(ByVal s As Suspect) As Boolean
            If s.Exists() Then
                Return s.IsArrested() OrElse s.IsDead
            Else
                Return True
            End If
        End Function

        Public Overrides Sub [End]()
            Logger.LogVerboseDebug("ending call")

            Dim d1 As Suspect = GetPed("Driver1")
            Dim v As Vehicles.Vehicle = GetVehicle("Vehicle1")

            If d1 IsNot Nothing AndAlso d1.Exists Then
                Logger.LogVerboseDebug("deleting ped blip")
                d1.DeleteBlip()
                d1.Dismiss()
            End If

            If v IsNot Nothing AndAlso v.Exists Then
                Logger.LogVerboseDebug("deleting veh blip")
                v.Dismiss()
            End If

            MyBase.[End]()
        End Sub

        Private Sub SpeakToSubject(ByRef d1 As Suspect)
            If d1.Exists AndAlso Game.LocalPlayer.Character.Position.DistanceTo(d1.Position) < 3 Then
                d1.Speak()
                Exit Sub
            End If
        End Sub

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

    End Class

End Namespace