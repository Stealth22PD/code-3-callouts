Imports Rage
Imports LSPD_First_Response.Engine.Scripting.Entities
Imports LSPD_First_Response.Mod.API
Imports Stealth.Common
Imports Stealth.Common.Scripting.Vehicles
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports Stealth.Plugins.Code3Callouts.Models.Peds

Namespace Models.Ambient.EventTypes

    Public Class Carjacking
        Inherits AmbientBase
        Implements IAmbientBase

        Private mState As EState = EState.LookingForVehicle
        Private mVehicle As Vehicles.Vehicle = Nothing
        Private mPlayerConfrontingSuspect As Boolean = False
        Private mSuspectAttackingPlayer As Boolean = False

        Public Sub New()
            RadioCode = 211
            CrimeEnums = {DISPATCH.CRIMES.CARJACKING}.ToList()
        End Sub

        Protected Overrides Function IsEventStarted() As Boolean
            Dim baseReturn As Boolean = MyBase.IsEventStarted()

            If baseReturn Then
                Dim p1 As PedBase = GetPed("Ped1")
                Return p1.Exists()
            Else
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
                        mState = EState.WalkingToVehicle

                        GameFiber.StartNew(
                            Sub()
                                If mVehicle.Exists() AndAlso mVehicle.HasDriver AndAlso mVehicle.Driver.Exists() AndAlso p1.Exists() Then
                                    mVehicle.Driver.Tasks.PerformDrivingManeuver(VehicleManeuver.Wait)
                                    mVehicle.Driver.KeepTasks = True
                                    p1.Tasks.FollowNavigationMeshToPosition(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2.0F), Common.GetHeadingToPoint(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2.0F), mVehicle.Position), 2.3F, 5.0F).WaitForCompletion()

                                    If p1.Exists() Then
                                        p1.Tasks.FollowToOffsetFromEntity(mVehicle, (Vector3.RelativeLeft * 2.0F))
                                        mVehicle.LockStatus = VehicleLockStatus.Unlocked
                                        mState = EState.AtVehicle
                                    Else
                                        [End]()
                                    End If
                                Else
                                    [End]()
                                End If
                            End Sub)

                    Case EState.AtVehicle
                        If mVehicle.Exists() AndAlso p1.Exists() Then
                            If p1.Position.DistanceTo(mVehicle.GetOffsetPosition(Vector3.RelativeLeft * 2.0F)) < 4 Then
                                mState = EState.JackingVehicle
                                p1.Tasks.Clear()
                                p1.Inventory.GiveNewWeapon(New WeaponDescriptor("WEAPON_PISTOL"), 56, True)
                                p1.Tasks.AimWeaponAt(mVehicle.GetOffsetPosition(Vector3.RelativeLeft), 5000)
                                mVehicle.IsPositionFrozen = True

                                GameFiber.StartNew(
                                    Sub()
                                        If mVehicle.Exists() AndAlso p1.Exists() Then
                                            Dim mVictimPed As Ped = Nothing

                                            If mVehicle.HasDriver AndAlso mVehicle.Driver.Exists() Then
                                                mVictimPed = mVehicle.Driver
                                            End If

                                            If mVictimPed IsNot Nothing AndAlso mVictimPed.Exists() Then
                                                GameFiber.Sleep(2000)
                                                mVictimPed.Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen).WaitForCompletion()

                                                If mVictimPed.Exists() AndAlso p1.Exists() Then
                                                    Natives.Peds.ReactAndFleePed(mVictimPed, p1)
                                                    GameFiber.Sleep(1000)
                                                Else
                                                    [End]()
                                                End If
                                            End If

                                            mVehicle.IsPositionFrozen = False
                                            GameFiber.Sleep(1000)
                                            p1.Tasks.EnterVehicle(mVehicle, -1)
                                            GameFiber.Sleep(1000)

                                            If mVictimPed.Exists() AndAlso mVehicle.Exists() Then
                                                Dim mVicPersona As Persona = Functions.GetPersonaForPed(mVictimPed)
                                                Functions.SetVehicleOwnerName(mVehicle, mVicPersona.FullName)
                                            End If

                                            mState = EState.VehicleStolen
                                            mVehicle.IsStolen = True

                                            p1.Tasks.CruiseWithVehicle(mVehicle, 15, (VehicleDrivingFlags.Emergency))

                                            GameFiber.Sleep(1000)

                                            If Config.CitizensCall911ForAmbientEvents Then
                                                Dispatch911Call(p1.Position)
                                                GameFiber.Sleep(1000)
                                            End If

                                            mState = EState.VictimCalled911
                                        Else
                                            [End]()
                                        End If
                                    End Sub)
                            End If
                        Else
                            [End]()
                        End If

                    Case EState.VictimCalled911
                        If mVehicle.Exists() AndAlso p1.Exists() Then
                            If mVehicle.DistanceTo(Game.LocalPlayer.Character.Position) > 250 Then
                                [End]()
                            Else
                                If mPlayerConfrontingSuspect = False Then
                                    If p1.Exists() AndAlso p1.IsOnFoot AndAlso p1.DistanceTo(Game.LocalPlayer.Character.Position) < 20 Then
                                        mPlayerConfrontingSuspect = True

                                        If mSuspectAttackingPlayer = False Then
                                            mSuspectAttackingPlayer = True
                                            p1.AttackPed(Game.LocalPlayer.Character)
                                        End If
                                    End If
                                End If
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
            Dim mClosestVehicles As List(Of Rage.Vehicle) = GetVehiclesNearPosition(pPosition, pRadius, GetEntitiesFlags.ConsiderCars Or GetEntitiesFlags.ExcludeEmptyVehicles Or GetEntitiesFlags.ExcludeEmergencyVehicles Or GetEntitiesFlags.ExcludePlayerVehicle).ToList()
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
            AtVehicle
            JackingVehicle
            VehicleStolen
            VictimCalled911
        End Enum

    End Class

End Namespace