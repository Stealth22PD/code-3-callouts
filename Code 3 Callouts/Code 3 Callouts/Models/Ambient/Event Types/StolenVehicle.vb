Imports Rage
Imports Stealth.Common
Imports Stealth.Common.Scripting.Vehicles
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Models.Peds

Namespace Models.Ambient.EventTypes

    Public Class StolenVehicle
        Inherits AmbientBase
        Implements IAmbientBase

        Private mState As EState = EState.LookingForVehicle
        Private mVehicle As Vehicles.Vehicle = Nothing

        Public Sub New()
            RadioCode = 487
            CrimeEnums = {DISPATCH.CRIMES.PERSON_STEALING_A_CAR}.ToList()
        End Sub

        Protected Overrides Function IsEventStarted() As Boolean
            Logger.LogTrivialDebug("starting event")
            Dim baseReturn As Boolean = MyBase.IsEventStarted()

            If baseReturn Then
                Logger.LogTrivialDebug("base return true")
                Dim p1 As PedBase = GetPed("Ped1")
                Logger.LogTrivialDebug("p1.exists=" & p1.Exists.ToString)
                Return p1.Exists()
            Else
                Logger.LogTrivialDebug("base return false")
                Return False
            End If
        End Function

        Protected Overrides Sub Process()
            MyBase.Process()
            Dim p1 As PedBase = GetPed("Ped1")

            If p1.Exists() Then
                Select Case mState
                    Case EState.LookingForVehicle
                        mVehicle = GetNearbyVehicle(p1.Position, 50)

                        If mVehicle Is Nothing Then
                            mVehicle = GetNearbyVehicle(p1.Position, 100)
                        End If

                        If mVehicle IsNot Nothing AndAlso p1.Exists() Then
                            mVehicle.MakePersistent()
                            p1.MakePersistent()
                            mState = EState.VehicleFound
                        Else
                            [End]()
                        End If

                    Case EState.VehicleFound
                        If mVehicle.Exists() AndAlso p1.Exists() Then
                            p1.Tasks.FollowNavigationMeshToPosition(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2.0F), Common.GetHeadingToPoint(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2.0F), mVehicle.Position), 2.3F)
                            mVehicle.LockStatus = VehicleLockStatus.LockedButCanBeBrokenInto
                            mState = EState.WalkingToVehicle
                        Else
                            [End]()
                        End If

                    Case EState.WalkingToVehicle
                        If mVehicle.Exists() AndAlso p1.Exists() Then
                            If p1.Position.DistanceTo(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2.0F)) < 4 Then
                                mState = EState.BreakingWindow
                                p1.Tasks.Clear()
                                GameFiber.StartNew(
                                    Sub()
                                        If mVehicle.Exists() AndAlso p1.Exists() Then
                                            p1.Tasks.EnterVehicle(mVehicle, -1).WaitForCompletion()

                                            If mVehicle.Exists() AndAlso p1.Exists() Then
                                                mState = EState.VehicleStolen
                                                mVehicle.IsStolen = True
                                                Natives.Functions.CallByHash(&HB8FF7AB45305C345UL, Common.GetNativeArgument(mVehicle))
                                                p1.Tasks.CruiseWithVehicle(mVehicle, 15, (VehicleDrivingFlags.DriveAroundVehicles Or VehicleDrivingFlags.DriveAroundPeds Or VehicleDrivingFlags.DriveAroundObjects))
                                                Dispatch911Call(p1.Position)
                                            Else
                                                [End]()
                                            End If
                                        Else
                                            [End]()
                                        End If
                                    End Sub)
                            End If
                        Else
                            [End]()
                        End If

                    Case EState.VehicleStolen
                        If mVehicle.Exists() Then
                            If mVehicle.DistanceTo(Game.LocalPlayer.Character.Position) > 250 Then
                                [End]()
                            End If
                        Else
                            [End]()
                        End If

                End Select
            Else
                [End]()
            End If
        End Sub

        Private Function GetNearbyVehicle(ByVal pPosition As Vector3, ByVal pRadius As Single) As Vehicles.Vehicle
            Dim mClosestVehicles As List(Of Rage.Vehicle) = GetVehiclesNearPosition(pPosition, pRadius, GetEntitiesFlags.ConsiderCars Or GetEntitiesFlags.ExcludeOccupiedVehicles Or GetEntitiesFlags.ExcludePlayerVehicle).ToList()
            Dim mVeh As Rage.Vehicle = (From x In mClosestVehicles Order By x.DistanceTo(pPosition) Select x).FirstOrDefault()

            If mVeh IsNot Nothing AndAlso mVeh.Exists() Then
                Return New Vehicles.Vehicle(mVeh.Handle)
            Else
                Return Nothing
            End If
        End Function

        Protected Overrides ReadOnly Property CanUseExistingPeds As Boolean
            Get
                Return True
            End Get
        End Property

        Protected Overrides ReadOnly Property PedsRequired As Integer
            Get
                Return 1
            End Get
        End Property

        Private Enum EState
            LookingForVehicle
            VehicleFound
            WalkingToVehicle
            BreakingWindow
            VehicleStolen
        End Enum

    End Class

End Namespace