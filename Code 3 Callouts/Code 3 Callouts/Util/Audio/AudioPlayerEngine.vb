Imports Rage
Imports System.IO
Imports System.Threading.Tasks

Namespace Util.Audio

    Module AudioPlayerEngine

        Friend Sub PlayDispatchAudio(ByVal pEnum As [Enum])
            Dim a As New AudioFile("DISPATCH", pEnum)
            PlayAudio(a)
        End Sub

        Friend Sub PlayOfficerAudio(ByVal pEnum As [Enum])
            Dim a As New AudioFile("OFFICER", pEnum)
            PlayAudio(a)
        End Sub

        Friend Sub PlayAudio(ByVal pAudioFile As AudioFile)
            Dim list As New List(Of AudioFile)
            list.Add(pAudioFile)
            PlayAudio(list)
        End Sub

        Friend Sub PlayAudio(ByVal pAudioFiles As List(Of AudioFile), Optional ByVal pLocation As Vector3 = Nothing)
            Try
                GameFiber.StartNew(
                    Sub()
                        Dim mAudioNames As String() = pAudioFiles.Select(Function(x) x.FileEnum.ToString()).ToArray()
                        Dim audio As String = String.Join(" ", mAudioNames)
                        Logger.LogVerboseDebug("Audio String -- " & audio)

                        If pLocation = Nothing Then
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio(audio)
                        Else
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition(audio, pLocation)
                        End If
                    End Sub)
            Catch ex As Exception
                Logger.LogVerbose("Error playing audio -- " & ex.Message)
            End Try
        End Sub

        Friend Sub PlayAudio(ByVal pAudioFiles As List(Of String), Optional ByVal pLocation As Vector3 = Nothing)
            Try
                GameFiber.StartNew(
                    Sub()
                        Dim audio As String = String.Join(" ", pAudioFiles)

                        If pLocation = Nothing Then
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio(audio)
                        Else
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition(audio, pLocation)
                        End If
                    End Sub)
            Catch ex As Exception
                Logger.LogVerbose("Error playing audio -- " & ex.Message)
            End Try
        End Sub

        Friend Sub PlayAudio(ByVal pAudio As String, Optional ByVal pLocation As Vector3 = Nothing)
            Try
                GameFiber.StartNew(
                    Sub()
                        If pLocation = Nothing Then
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudio(pAudio)
                        Else
                            LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition(pAudio, pLocation)
                        End If
                    End Sub)
            Catch ex As Exception
                Logger.LogVerbose("Error playing audio -- " & ex.Message)
            End Try
        End Sub

        Friend Class AudioFile
            Public Sub New(ByVal pClassName As String, ByVal pAudioFile As [Enum])
                _class = pClassName
                _fileEnum = pAudioFile
            End Sub

            Private _fileEnum As [Enum]
            Public Property FileEnum() As [Enum]
                Get
                    Return _fileEnum
                End Get
                Set(ByVal value As [Enum])
                    _fileEnum = value
                End Set
            End Property

            Private _class As String
            Public Property AudioClass() As String
                Get
                    Return _class
                End Get
                Set(ByVal value As String)
                    _class = value
                End Set
            End Property
        End Class

    End Module

End Namespace