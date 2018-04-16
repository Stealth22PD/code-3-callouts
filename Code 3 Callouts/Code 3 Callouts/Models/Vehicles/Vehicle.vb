Imports Rage
Imports Stealth.Common.Models
Imports Stealth.Common.Natives.Vehicles
Imports Stealth.Plugins.Code3Callouts.Util
Imports System.Drawing

Namespace Models.Vehicles

    Public Class Vehicle
        Inherits Rage.Vehicle
        Implements IVehicle, IHandleable

        Public Sub New(model As Rage.Model, position As Rage.Vector3)
            MyBase.New(model, position)
            Init()
        End Sub

        Public Sub New(model As Rage.Model, position As Rage.Vector3, heading As Single)
            MyBase.New(model, position, heading)
            Init()
        End Sub

        Protected Friend Sub New(handle As Rage.PoolHandle)
            MyBase.New(handle)
            Init()
        End Sub

        Public Sub Init() Implements IVehicle.Init
            _Colors = New VehicleColor()
            _Colors.PrimaryColor = EPaint.Unknown
            _Colors.SecondaryColor = EPaint.Unknown
        End Sub

        Public Sub FillColorValues() Implements IVehicle.FillColorValues
            Try
                If Me IsNot Nothing Then
                    If Me.Exists Then
                        _Colors = VehicleHelper.GetVehicleColors(Me)
                    Else
                        Logger.LogVerboseDebug("Error getting vehicle colors -- Vehicle does not exist")
                    End If
                Else
                    Logger.LogVerboseDebug("Error getting vehicle colors -- Vehicle is null")
                End If
            Catch ex As Exception
                Logger.LogVerboseDebug("Error getting vehicle colors -- " & ex.Message)
                _Colors = New VehicleColor()
                _Colors.PrimaryColor = EPaint.Unknown
                _Colors.SecondaryColor = EPaint.Unknown
            End Try
        End Sub

        Public Overrides Sub Dismiss()
            Logger.LogVerboseDebug("Deleting blip and dismissing vehicle")
            DeleteBlip()
            MyBase.Dismiss()
        End Sub

        Public Overrides Sub Delete()
            Logger.LogVerboseDebug("Deleting blip and vehicle")
            DeleteBlip()
            MyBase.Delete()
        End Sub

        Sub CreateBlip(Optional ByVal color As Drawing.Color = Nothing) Implements IVehicle.CreateBlip
            If color = Nothing Then
                color = Drawing.Color.Red
            End If

            If Me.Exists Then
                Me.Blip = New Blip(Me)
                Me.Blip.Color = color
            End If
        End Sub

        Sub DeleteBlip() Implements IVehicle.DeleteBlip
            Try
                If Me IsNot Nothing AndAlso Me.Exists Then
                    If Me.Blip IsNot Nothing Then
                        If Me.Blip.Exists Then
                            Me.Blip.Delete()
                        End If
                    End If
                End If
            Catch ex As Exception
                Logger.LogVerboseDebug("Error deleting Vehicle blip -- " & ex.Message)
            End Try
        End Sub

        Private _Colors As VehicleColor
        Public ReadOnly Property Colors As VehicleColor Implements IVehicle.Colors
            Get
                Return _Colors
            End Get
        End Property

        Public ReadOnly Property PrimaryColorEnum As EPaint Implements IVehicle.PrimaryColorEnum
            Get
                Return _Colors.PrimaryColor
            End Get
        End Property

        Public ReadOnly Property PrimaryColorName As String Implements IVehicle.PrimaryColorName
            Get
                Return _Colors.PrimaryColorName
            End Get
        End Property

        Public ReadOnly Property SecondaryColorEnum As EPaint Implements IVehicle.SecondaryColorEnum
            Get
                Return _Colors.SecondaryColor
            End Get
        End Property

        Public ReadOnly Property SecondaryColorName As String Implements IVehicle.SecondaryColorName
            Get
                Return _Colors.SecondaryColorName
            End Get
        End Property

        Public Property Blip As Blip Implements IVehicle.Blip
        Public Property Name As String Implements IVehicle.Name

    End Class

End Namespace