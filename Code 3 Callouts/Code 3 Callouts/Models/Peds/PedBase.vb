Imports Rage
Imports Stealth.Common
Imports Stealth.Common.Extensions
Imports Stealth.Common.Models
Imports Stealth.Common.Models.QuestionWindow
Imports Stealth.Plugins.Code3Callouts.Util

Namespace Models.Peds

    Public Class PedBase
        Inherits Ped
        Implements IPedBase, IHandleable

        Public Property Type As PedType Implements IPedBase.Type
        Public Property Name As String Implements IPedBase.Name
        Public Property DisplayName As String Implements IPedBase.DisplayName
        Public Property Blip As Blip Implements IPedBase.Blip
        'Public Property SearchArea As Blip Implements IPedBase.SearchArea
        Public Property OriginalSpawnPoint As Vector3 Implements IPedBase.OriginalSpawnPoint
        Public Property SpeechLines As List(Of String) Implements IPedBase.SpeechLines
        Public Property SpeechIndex As Integer Implements IPedBase.SpeechIndex
        Public Property PhysicalCondition As String Implements IPedBase.PhysicalCondition
        Public Property QAItems As List(Of QAItem) = Nothing

        Public Property HasSpoken As Boolean = False
        Private mIsModalActive As Boolean = False

        Public Sub New(ByVal pName As String, ByVal position As Rage.Vector3)
            Me.New(pName, PedType.Unknown, position)
        End Sub

        Public Sub New(ByVal pName As String, ByVal model As Rage.Model, ByVal position As Rage.Vector3, ByVal heading As Single)
            Me.New(pName, PedType.Unknown, model, position, heading)
        End Sub

        Protected Friend Sub New(ByVal pName As String, ByVal handle As Rage.PoolHandle)
            Me.New(pName, PedType.Unknown, handle)
        End Sub

        Public Sub New(ByVal pName As String, ByVal pType As PedType, ByVal position As Rage.Vector3)
            MyBase.New(position)
            Name = pName
            Type = pType
            Init()
        End Sub

        Public Sub New(ByVal pName As String, ByVal pType As PedType, ByVal model As Rage.Model, ByVal position As Rage.Vector3, ByVal heading As Single)
            MyBase.New(model, position, heading)
            Name = pName
            Type = pType
            Init()
        End Sub

        Protected Friend Sub New(ByVal pName As String, ByVal pType As PedType, ByVal handle As Rage.PoolHandle)
            MyBase.New(handle)
            Name = pName
            Type = pType
            Init()
        End Sub

        Protected Friend Sub Init()
            OriginalSpawnPoint = Me.Position
            'SpeechIndex = -1
            SpeechLines = New List(Of String)

            If Common.IsTrafficPolicerRunning() Then
                Driver.MakePedImmuneToTrafficEvents(Me)
            End If
        End Sub

        Public Overrides Sub Dismiss()
            DeleteBlip()
            MyBase.Dismiss()
        End Sub

        Public Overrides Sub Delete()
            DeleteBlip()
            MyBase.Delete()
        End Sub

        Public Overridable Sub Speak() Implements IPedBase.Speak
            SpeakSubtitle()
        End Sub

        Private Sub SpeakModal()
            If Me.IsDead Then
                SpeechLines.Clear()
                Exit Sub
            End If

            If SpeechLines.Count < 1 Then
                Exit Sub
            End If

            If Me.HasAttachedBlip() = False Then
                Exit Sub
            End If

            If mIsModalActive = False Then
                GameFiber.StartNew(
                    Sub()
                        Dim mModal As ModalBase = Nothing

                        mIsModalActive = True

                        mModal = New SpeechModal(DisplayName, SpeechLines, True)
                        mModal.Show()

                        mModal = Nothing
                        mIsModalActive = False
                        HasSpoken = True
                    End Sub)
            End If
        End Sub

        Private Sub SpeakSubtitle()
            If SpeechLines.Count < 1 Then
                Exit Sub
            End If

            If Me.IsDead Then
                SpeechLines.Clear()
                Exit Sub
            End If

            'If Me.HasAttachedBlip() = False Then
            '    Exit Sub
            'End If

            If SpeechIndex = -1 Then
                SpeechIndex = 0
            End If

            If SpeechIndex < SpeechLines.Count Then
                Dim pedName As String
                If DisplayName = "" Then
                    pedName = Name
                Else
                    pedName = DisplayName
                End If

                Dim colorCode As String = "~w~"
                If Type = PedType.Victim Or Type = PedType.Witness Then
                    colorCode = "~o~"
                ElseIf Type = PedType.Cop
                    colorCode = "~b~"
                Else
                    colorCode = "~y~"
                End If

                Dim speech As String = String.Format("{2}{0}: ~w~{1}", pedName, SpeechLines(SpeechIndex), colorCode)

                If SpeechLines.Count > 1 Then
                    speech += String.Format(" ({0}/{1})", (SpeechIndex + 1), SpeechLines.Count)
                End If

                Game.DisplaySubtitle(speech, 8000)

                SpeechIndex += 1
            Else
                HasSpoken = True
                SpeechIndex = -1
            End If
        End Sub

        Sub CreateBlip(Optional ByVal pColor As Drawing.Color = Nothing) Implements IPedBase.CreateBlip
            If Me.Exists Then
                Dim color As Drawing.Color

                If pColor = Nothing Then
                    Select Case Type
                        Case PedType.Suspect
                            color = Drawing.Color.Red
                        Case PedType.Unknown
                            color = Drawing.Color.Yellow
                        Case PedType.Cop
                            color = Drawing.Color.LightBlue
                        Case Else
                            color = Drawing.Color.Orange
                    End Select
                Else
                    color = pColor
                End If

                Me.Blip = New Blip(Me)
                Me.Blip.Color = color
                Me.Blip.Scale = 0.75
            End If
        End Sub

        Sub DeleteBlip() Implements IPedBase.DeleteBlip
            Try
                If Me.Blip IsNot Nothing Then
                    If Me.Blip.Exists = True Then
                        Me.Blip.Delete()
                        'Me.Blip = Nothing
                    End If
                Else
                    Logger.LogVerboseDebug("Tried to delete Ped blip, but it was null")
                End If
            Catch ex As Exception
                Logger.LogVerboseDebug("Error deleting Ped blip -- " & ex.Message)
            End Try
        End Sub

        Sub SetDrunkRandom()
            If Common.IsTrafficPolicerRunning() Then
                If gRandom.Next(2) = 0 Then
                    SetIsDrunk(False)
                Else
                    SetIsDrunk(True)
                End If
            End If
        End Sub

        Sub SetIsDrunk(ByVal pValue As Boolean) Implements IPedBase.SetIsDrunk
            Try
                If Common.IsTrafficPolicerRunning() Then
                    Logger.LogTrivialDebug("Traffic Policer running; setting ped as drunk")
                    SetBACDrunk(pValue)
                Else
                    Logger.LogTrivialDebug("Traffic Policer not running; not setting ped as drunk")
                End If

                If pValue = True Then
                    'Rage.Native.NativeFunction.CallByName("SET_PED_IS_DRUNK", GetType(Object), Me, pValue)
                    'Rage.Native.NativeFunction.CallByName(Of UInteger)("SET_PED_IS_DRUNK", Me, pValue)
                    Natives.Peds.SetPedIsDrunk(Me, pValue)
                End If
            Catch ex As Exception
                Logger.LogVerboseDebug(String.Format("Error setting Ped.Drunk({0}) -- {1}", pValue, ex.Message))
            End Try
        End Sub

        Private Sub SetBACDrunk(ByVal pValue As Boolean)
            If pValue = True Then
                TrafficPolicerFunctions.SetPedAlcoholLevel(Me, TrafficPolicerFunctions.GetRandomOverTheLimitAlcoholLevel())
            Else
                TrafficPolicerFunctions.SetPedAlcoholLevel(Me, TrafficPolicerFunctions.GetRandomUnderTheLimitAlcoholLevel())
            End If
        End Sub

        Sub AttackPed(ByVal pTargetPed As Ped) Implements IPedBase.AttackPed
            Try
                Natives.Peds.AttackPed(Me, pTargetPed)
            Catch ex As Exception
                Logger.LogVerboseDebug("Error triggering ped combat -- " & ex.Message)
            End Try
        End Sub

        Sub TurnToFaceEntity(ByVal pTarget As Entity, Optional ByVal pTimeout As Integer = 5000) Implements IPedBase.TurnToFaceEntity
            Try
                Dim pHash As ULong = &H5AD23D40115353ACUL
                Rage.Native.NativeFunction.CallByHash(Of UInteger)(pHash, Common.GetNativeArgument(Me), Common.GetNativeArgument(pTarget), pTimeout)
            Catch ex As Exception
                Logger.LogVerboseDebug("Error calling native -- " & ex.Message)
            End Try
        End Sub

    End Class

End Namespace