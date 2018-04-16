Imports LSPD_First_Response.Mod.API
Imports Rage
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports System.Reflection

Public Class Main
    Inherits Plugin

    Public Sub New()

    End Sub

    Public Overrides Sub Initialize()
        AddHandler AppDomain.CurrentDomain.AssemblyResolve, New ResolveEventHandler(Function(sender, e) LSPDFRResolveEventHandler(sender, e))
        AddHandler Functions.OnOnDutyStateChanged, AddressOf Functions_OnOnDutyStateChanged

        If Config.PlayAdam12Intro = True Then
            AudioPlayerEngine.PlayAudio(Unit_1ADAM12)
        End If
    End Sub

    Public Overrides Sub [Finally]()
    End Sub

    Shared Sub Functions_OnOnDutyStateChanged(ByVal onDuty As Boolean)
        If Common.PreloadChecks() = False Then
            Exit Sub
        End If

        gIsPlayerOnDuty = onDuty

        If onDuty = True Then
            Dim initMsg As String = String.Format("Loaded {0} v{1}.{2}.{3}.{4}",
                                              My.Application.Info.Title,
                                              My.Application.Info.Version.Major, My.Application.Info.Version.Minor, My.Application.Info.Version.Build, My.Application.Info.Version.Revision)

            Dim initMsg2 As String = String.Format("Loaded ~b~v{0}.{1}.{2}.{3}",
                                              My.Application.Info.Version.Major, My.Application.Info.Version.Minor, My.Application.Info.Version.Build, My.Application.Info.Version.Revision)

            Common.Init()
            Logger.LogTrivial(initMsg)

            Dim mTitle As String = "CODE 3 CALLOUTS"
            Dim mSubtitle As String = "~b~Developed By Stealth22"
            Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", mTitle, mSubtitle, initMsg2)

            Common.CheckForUpdates()
            Common.RegisterCallouts()
            Driver.RunAmbientEvents()
            Driver.InitializeMenu()

            If Config.EnableDispatchStatusCheck Then
                Driver.RunDispatchStatusCheck()
            End If
        End If
    End Sub

    Shared Function LSPDFRResolveEventHandler(ByVal sender As Object, ByVal e As ResolveEventArgs) As Assembly
        For Each a As Assembly In Functions.GetAllUserPlugins()
            If e.Name.ToLower().Contains(a.GetName().Name.ToLower()) Then
                Return a
            End If
        Next

        Return Nothing
    End Function

End Class