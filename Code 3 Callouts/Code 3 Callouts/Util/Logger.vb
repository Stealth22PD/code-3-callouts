Imports Rage

Namespace Util

    Friend Module Logger

        Friend Sub LogTrivial(ByVal pMessage As String)
            Try
                Game.LogTrivial(FormatMessage(pMessage))
            Catch ex As Exception
            End Try
        End Sub

        Friend Sub LogVerbose(ByVal pMessage As String)
            Try
                Game.LogVerbose(FormatMessage(pMessage))
            Catch ex As Exception
            End Try
        End Sub

        Friend Sub LogVeryVerbose(ByVal pMessage As String)
            Try
                Game.LogVeryVerbose(FormatMessage(pMessage))
            Catch ex As Exception
            End Try
        End Sub

        Friend Sub LogExtremelyVerbose(ByVal pMessage As String)
            Try
                Game.LogExtremelyVerbose(FormatMessage(pMessage))
            Catch ex As Exception
            End Try
        End Sub

        Friend Sub LogTrivialDebug(ByVal pMessage As String)
            Try
                Game.LogTrivialDebug(FormatMessage(pMessage))
            Catch ex As Exception
            End Try
        End Sub

        Friend Sub LogVerboseDebug(ByVal pMessage As String)
            Try
                Game.LogVerboseDebug(FormatMessage(pMessage))
            Catch ex As Exception
            End Try
        End Sub

        Friend Sub LogExtremelyVerboseDebug(ByVal pMessage As String)
            Try
                Game.LogExtremelyVerboseDebug(FormatMessage(pMessage))
            Catch ex As Exception
            End Try
        End Sub

        Private Function FormatMessage(ByVal pMessage As String) As String
            Return String.Format("[{0}] {1}", My.Application.Info.Title, pMessage)
        End Function

    End Module

End Namespace