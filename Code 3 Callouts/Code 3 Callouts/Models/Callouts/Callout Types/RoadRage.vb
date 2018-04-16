Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Common.Extensions
Imports Stealth.Common.Models
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Util.Extensions
Imports System.Windows.Forms
Imports LSPD_First_Response.Mod.API
Imports Stealth.Common.Models.QuestionWindow

Namespace Models.Callouts.CalloutTypes

    <CalloutInfo("Road Rage In Progress", CalloutProbability.Medium)>
    Public Class RoadRage
        Inherits CalloutBase

        Dim VehModels As List(Of String) = {"Blista", "Jackal", "Oracle", "Asea", "Emperor", "Fugitive", "Ingot", "Premier", "Primo", "Stanier", "Stratum", "Asterope", "Baller", "Bison", "Cavalcade2", "Exemplar", "F620", "Felon", "FQ2", "Gresley", "Habanero", "Intruder", "Landstalker", "Mesa", "Minivan", "Patriot", "Radi", "Regina", "schafter2", "Seminole", "Sentinel", "Serrano", "Speedo", "Surge", "Tailgater", "Washington", "Zion"}.ToList()
        Dim PedModels As List(Of String) = {"A_M_Y_SouCent_01", "A_M_Y_StWhi_01", "A_M_Y_StBla_01", "A_M_Y_Downtown_01", "A_M_Y_BevHills_01", "G_M_Y_MexGang_01", "G_M_Y_MexGoon_01", "G_M_Y_StrPunk_01",
                                        "A_M_M_BevHills_01", "A_M_M_GenFat_01", "A_M_M_Business_01", "A_M_M_Golfer_01", "A_M_M_Skater_01", "A_M_M_Salton_01", "A_M_M_Tourist_01"}.ToList()

        Dim mIsSuspectHonking As Boolean = True
        Dim mOfficerFoundVehicles As Boolean = False
        Dim mSuspectArrested As Boolean = False
        Dim mOfficerWithVictim As Boolean = False

        Dim vehVictim As Vehicles.Vehicle
        Dim pedVictim As Victim

        Dim vehSuspect As Vehicles.Vehicle
        Dim pedSuspect As Suspect
        Dim endTipDisplayed As Boolean = False

        Public Sub New()
            MyBase.New("Road Rage In Progress", CallResponseType.Code_3)
            RadioCode = 0
            CrimeEnums = {DISPATCH.CRIMES.CIV_ASSISTANCE}.ToList()

            CallDetails = String.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"))
            CallDetails += "Caller is being harassed by another driver. Suspect is currently chasing RP's vehicle and attempting to ram them."
            CallDetails += Environment.NewLine
            CallDetails += Environment.NewLine
            CallDetails += "Suspect is yelling and giving obscene gestures. Suspect possibly armed. Proceed with caution."

            Objective = "Track down both vehicles!~n~Pull over the ~r~suspect!"
        End Sub

        Public Overrides Function OnBeforeCalloutDisplayed() As Boolean
            Dim baseReturn As Boolean = MyBase.OnBeforeCalloutDisplayed()

            If baseReturn = False Then
                Return False
            End If

            SkipRespondingState = True

            Dim position As Vector3 = World.GetNextPositionOnStreet(SpawnPoint.Around(5))
            Dim node As VehicleNode = Stealth.Common.Natives.Vehicles.GetClosestVehicleNodeWithHeading(position)

            If node.Position = Vector3.Zero Then
                node.Position = position
                node.Heading = gRandom.Next(360)
            End If

            AddMinimumDistanceCheck(30, position)

            vehVictim = New Vehicles.Vehicle(VehModels(gRandom.Next(VehModels.Count)), position, node.Heading)
            pedVictim = New Victim("Victim1", PedModels(gRandom.Next(PedModels.Count)), vehVictim.GetOffsetPosition(Vector3.RelativeLeft * 3.0F), 0)
            pedVictim.DisplayName = "Victim"

            vehSuspect = New Vehicles.Vehicle(VehModels(gRandom.Next(VehModels.Count)), vehVictim.GetOffsetPosition(Vector3.RelativeBack * 10.0F), node.Heading)
            pedSuspect = New Suspect("Suspect1", PedModels(gRandom.Next(PedModels.Count)), vehSuspect.GetOffsetPosition(Vector3.RelativeLeft * 3.0F), 0, True)
            pedSuspect.DisplayName = "Suspect"

            If pedVictim.Exists() AndAlso vehVictim.Exists() AndAlso pedSuspect.Exists() AndAlso vehSuspect.Exists() Then
                vehVictim.Name = "VictimCar1"
                vehVictim.MakePersistent()
                vehVictim.SetRandomColor()
                Vehicles.Add(vehVictim)

                pedVictim.MakePersistent()
                pedVictim.BlockPermanentEvents = True
                pedVictim.WarpIntoVehicle(vehVictim, -1)
                Peds.Add(pedVictim)
                pedVictim.CreateBlip()

                If Common.IsComputerPlusRunning() Then
                    AddPedToCallout(pedVictim)
                    AddVehicleToCallout(vehVictim)
                    AddVehicleToCallout(vehSuspect)
                End If

                With pedVictim.SpeechLines
                    .Add("Thank you so much, Officer!!")
                    .Add("Your response was really fast! You saved my life!!")
                    .Add("What about my car, though?")
                    .Add("Mors Mutual will take care of the damage, but...")
                    .Add("Can I drive it home, or do you need to tow it away?")
                    .Add("Can you check it over for me?")
                    .Add("You can tow it away if you need to.")
                End With

                Dim pDataVictim As LSPD_First_Response.Engine.Scripting.Entities.Persona = Functions.GetPersonaForPed(pedVictim)
                If pDataVictim.Wanted Or pDataVictim.LicenseState <> LSPD_First_Response.Engine.Scripting.Entities.ELicenseState.Valid Then
                    pDataVictim = New LSPD_First_Response.Engine.Scripting.Entities.Persona(pedVictim, pDataVictim.Gender, pDataVictim.BirthDay, pDataVictim.Citations, pDataVictim.Forename, pDataVictim.Surname, pDataVictim.LicenseState = LSPD_First_Response.Engine.Scripting.Entities.ELicenseState.Valid, pDataVictim.TimesStopped, False, False, False)
                    Functions.SetPersonaForPed(pedVictim, pDataVictim)
                End If

                vehSuspect.Name = "SuspectCar1"
                vehSuspect.MakePersistent()
                vehSuspect.SetRandomColor()
                Vehicles.Add(vehSuspect)

                pedSuspect.MakePersistent()
                pedSuspect.BlockPermanentEvents = True
                pedSuspect.WarpIntoVehicle(vehSuspect, -1)
                pedSuspect.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_KNIFE"), 56, False)
                pedSuspect.SetDrunkRandom()

                pedSuspect.QAItems = New List(Of QAItem)
                pedSuspect.QAItems.Add(New QAItem("What the hell did you think you were doing?!", "Teaching that idiot a lesson!"))
                pedSuspect.QAItems.Add(New QAItem("Why were you chasing the other driver?", "That fucker cut me off!! They deserve to die!!"))
                pedSuspect.QAItems.Add(New QAItem("Do you realize you put other drivers in danger?", "Oh spare me the speech, pig!"))
                pedSuspect.QAItems.Add(New QAItem("Don't you think you should calm down?", "Why don't you take off that badge and gun, so I can kick your ass?"))
                pedSuspect.QAItems.Add(New QAItem("Anything else you want to say?", "Yeah. Tell your wife she owes me for last night, bitch."))

                Peds.Add(pedSuspect)
                pedSuspect.CreateBlip()

                'AI::_TASK_VEHICLE_FOLLOW
                'void _TASK_VEHICLE_FOLLOW(Ped driver, Vehicle vehicle, Entity targetEntity,
                'int drivingStyle, float speed, float minDistance)
                '// 0xFC545A9F0626E3B6

                'Dim p As New Ped(Vector3.Zero)

                'Dim pHash As ULong = &HFC545A9F0626E3B6UL
                'Dim args As Native.NativeArgument() = {pedSuspect, vehSuspect, pedVictim, 3, 12, 10}
                'Dim a As New Native.NativeArgument(CType(pedSuspect, IHandleable))
                'Rage.Native.NativeFunction.CallByHash(Of ULong)(pHash, pedSuspect, vehSuspect, pedVictim, 3, 12, 10)

                Try
                    GameFiber.StartNew(
                        Sub()
                            pedVictim.Tasks.CruiseWithVehicle(12, VehicleDrivingFlags.Emergency)
                            'Stealth.Common.Natives.Peds.FollowEntityInVehicle(s, susVeh, v, 3, 12, 10)
                            'Stealth.Common.Natives.Peds.EscortVehicle(pedSuspect, vehSuspect, vehVictim, 0, 12, 0, 10, 0, 0.1)
                            Stealth.Common.Natives.Peds.ChaseEntityInVehicle(pedSuspect, pedVictim)
                        End Sub)

                    GameFiber.StartNew(
                        Sub()
                            Dim pHash As ULong = &H9C8C6504B5B63D2CUL

                            While mIsSuspectHonking
                                Rage.Native.NativeFunction.CallByHash(Of ULong)(pHash, GetNativeArgument(vehSuspect), 1000, Game.GetHashKey("HELDDOWN"), 0)
                                GameFiber.Sleep(1500)
                            End While
                        End Sub)
                Catch ex As Exception
                    Logger.LogVerbose("Exception when calling road rage native")
                    Logger.LogVerbose("s -- " & pedSuspect.ToString())
                    Logger.LogVerbose("susVeh -- " & vehSuspect.ToString())
                    Logger.LogVerbose("p -- " & pedVictim.ToString())
                    Logger.LogVerbose(ex.ToString())
                End Try

                Return True
            Else
                Logger.LogVerbose("Road rage aborted")
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

            If Common.IsComputerPlusRunning() Then
                CADAtScene()
            End If

            GameFiber.StartNew(
               Sub()
                   GameFiber.Sleep(4000)
                   Game.DisplayHelp("Dispatch is on the phone with the victim.", 8000)
                   GameFiber.Sleep(8000)
                   Game.DisplayHelp("Track down the vehicles, and pull over the suspect.", 8000)
                   GameFiber.Sleep(8000)
                   Game.DisplayHelp("Deal with the situation as you see fit.", 8000)
               End Sub)
        End Sub

        Private Sub CADAtScene()
            If Common.IsComputerPlusRunning() Then
                ComputerPlusFunctions.UpdateCallStatus(CalloutID, ComputerPlus.ECallStatus.At_Scene)
            End If
        End Sub

        Public Overrides Sub Process()
            MyBase.Process()

            If (pedSuspect Is Nothing OrElse pedSuspect.Exists() = False) Or (pedVictim Is Nothing OrElse pedVictim.Exists() = False) Then
                Exit Sub
            End If

            If Game.IsKeyDown(Config.SpeakKey) Then
                SpeakToSubject()
            End If

            If Game.IsKeyDown(Config.EndCallKey) Then
                If Config.EndCallModKey = Keys.None OrElse Game.IsKeyDownRightNow(Config.EndCallModKey) Then
                    Radio.CallIsCode4(Me.ScriptInfo.Name)
                    [End]()
                End If
            End If

            If pedSuspect.IsArrested() OrElse pedSuspect.IsDead Then
                'Radio.CallIsCode4(Me.ScriptInfo.Name, pedSuspect.IsArrested)
                '[End]()

                If mSuspectArrested = False Then
                    mSuspectArrested = True

                    If Common.IsComputerPlusRunning() Then
                        AddPedToCallout(pedSuspect)
                    End If

                    If pedSuspect.Inventory.Weapons.Count > 0 Then
                        pedSuspect.Inventory.Weapons.Clear()
                        Game.DisplayNotification("While searching the suspect, you find/remove a ~r~knife~w~.")
                    End If

                    GameFiber.StartNew(
                    Sub()
                        If pedVictim.Exists() Then
                            Radio.SergeantMessage("Please return to the victim and ensure they're okay.")

                            If pedVictim.IsDead = False Then
                                Radio.DispatchMessage("We have the RP on the phone; they are awaiting police arrival")

                                Game.DisplayHelp("Return to the victim's location.", 8000)
                                GameFiber.Sleep(8000)
                                Game.DisplayHelp("Press " & Config.SpeakKey.ToString() & " to speak to them, and ensure they are okay.", 8000)
                                GameFiber.Sleep(8000)
                                Game.DisplayHelp("Call EMS for the victim if necessary.", 8000)
                                GameFiber.Sleep(8000)

                                If pedSuspect.IsAlive Then
                                    Game.DisplayHelp("Also, ensure that you question the suspect using the interaction menu.", 8000)
                                    GameFiber.Sleep(8000)
                                End If

                                Game.DisplayHelp("Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout when the situation is over.", 8000)
                            Else
                                Game.DisplayHelp("Return to the victim's location, and ensure they are okay.", 8000)
                                GameFiber.Sleep(8000)
                                Game.DisplayHelp("Call EMS for the victim if necessary.", 8000)
                                GameFiber.Sleep(8000)

                                If pedSuspect.IsAlive Then
                                    Game.DisplayHelp("Also, ensure that you question the suspect using the interaction menu.", 8000)
                                    GameFiber.Sleep(8000)
                                End If
                                Game.DisplayHelp("Press " & Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) & " to end this callout when the situation is over.", 8000)
                            End If
                        End If
                    End Sub)
                End If
            End If

            If mOfficerFoundVehicles = False Then
                If (vehVictim IsNot Nothing AndAlso vehVictim.Exists()) AndAlso (vehSuspect IsNot Nothing AndAlso vehSuspect.Exists()) Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(vehVictim.Position) < 20 Or Game.LocalPlayer.Character.Position.DistanceTo(vehSuspect.Position) < 20 Then
                        mOfficerFoundVehicles = True
                        Radio.SuspectSpotted()

                        GameFiber.StartNew(
                            Sub()
                                GameFiber.Sleep(6000)
                                mIsSuspectHonking = False
                            End Sub)

                        GameFiber.StartNew(
                            Sub()
                                pedVictim.Tasks.Clear()
                                pedVictim.Tasks.ParkVehicle(vehVictim.Position, vehVictim.Heading)
                                'GameFiber.StartNew(
                                '    Sub()
                                '        pedVictim.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait)
                                '        GameFiber.Sleep(2000)
                                '        pedVictim.Tasks.ParkVehicle(vehVictim.Position, vehVictim.Heading)
                                '    End Sub)

                                pedSuspect.Tasks.Clear()

                                If pedSuspect.IsInAnyVehicle(False) AndAlso pedSuspect.CurrentVehicle IsNot Nothing AndAlso pedSuspect.CurrentVehicle.Exists() Then
                                    pedSuspect.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait)
                                End If

                                Dim susReaction As Integer = gRandom.Next(10)
                                If susReaction >= 6 Then
                                    'Drive away casually
                                    If pedSuspect.IsInAnyVehicle(False) AndAlso pedSuspect.CurrentVehicle IsNot Nothing AndAlso pedSuspect.CurrentVehicle.Exists() Then
                                        pedSuspect.Tasks.CruiseWithVehicle(12, VehicleDrivingFlags.Normal)
                                    End If

                                ElseIf susReaction >= 2 And susReaction <= 5 Then
                                    'Flee
                                    Dim pursuit As LHandle = Common.CreatePursuit()
                                    Functions.AddPedToPursuit(pursuit, pedSuspect)

                                Else
                                    'Attack victim
                                    If pedSuspect.IsOnFoot = False Then pedSuspect.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen)
                                    If pedVictim.IsOnFoot = False Then pedVictim.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen)
                                    pedSuspect.AttackPed(pedVictim)
                                End If
                            End Sub)
                    End If
                End If
            End If

            If pedVictim.Exists() = True AndAlso pedVictim.IsDead = False Then
                If mSuspectArrested = True AndAlso mOfficerWithVictim = False Then
                    If Game.LocalPlayer.Character.Position.DistanceTo(pedVictim.Position) < 20 Then
                        mOfficerWithVictim = True

                        If pedVictim.IsInAnyVehicle(True) = True Then
                            GameFiber.StartNew(
                                Sub()
                                    pedVictim.Tasks.LeaveVehicle(LeaveVehicleFlags.None)
                                    GameFiber.Sleep(2000)
                                    pedVictim.TurnToFaceEntity(Game.LocalPlayer.Character)
                                End Sub)
                        Else
                            pedVictim.Tasks.Clear()
                            pedVictim.TurnToFaceEntity(Game.LocalPlayer.Character)
                        End If
                    End If
                End If
            End If
        End Sub

        Private Sub SpeakToSubject()
            If pedVictim.Exists() Then
                If Game.LocalPlayer.Character.Position.DistanceTo(pedVictim.Position) < 3 Then
                    pedVictim.Speak()
                    Exit Sub
                End If
            End If
        End Sub

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