Imports Rage
Imports Stealth.Plugins.Code3Callouts.Models.Callouts
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase
Imports System.Reflection
Imports System.Windows.Forms

Module Config

    Friend Const INIFileName As String = "Code 3 Callouts.ini"
    Friend Const INIFilePath As String = "Plugins\LSPDFR\" & INIFileName
    Friend mINIFile As New InitializationFile(INIFilePath)

    Friend Property PlayAdam12Intro As Boolean
    Friend Property UnitDivision As DISPATCH.DIVISION
    Friend Property UnitType As DISPATCH.UNIT_TYPE
    Friend Property UnitBeat As DISPATCH.BEAT
    Friend Property EnableDispatchStatusCheck As Boolean
    Friend Property MinTimeBetweenStatusChecks As Integer
    Friend Property MaxTimeBetweenStatusChecks As Integer

    Friend Property Callouts As New Dictionary(Of String, Boolean)
    Friend Property RegisteredCallouts As New List(Of String)

    Friend Property InteractionMenuKey As Keys
    Friend Property InteractionMenuModKey As Keys
    Friend Property EndCallKey As Keys
    Friend Property EndCallModKey As Keys
    Friend Property SpeakKey As Keys
    Friend Property AskToFollowKey As Keys
    Friend Property AskToFollowModKey As Keys
    Friend Property StatusRadioResponseOK As Keys
    Friend Property StatusRadioResponseOKModKey As Keys
    Friend Property StatusRadioResponseHelpMe As Keys
    Friend Property StatusRadioResponseHelpMeModKey As Keys

    Friend Property AmbientEventsEnabled As Boolean = True
    Friend Property AmbientPedBlipsEnabled As Boolean = True
    Friend Property MinTimeBetweenAmbientEvents As Integer
    Friend Property MaxTimeBetweenAmbientEvents As Integer
    Friend Property CitizensCall911ForAmbientEvents As Boolean = True

    Friend Sub Init()
        If mINIFile.Exists() = False Then
            CreateINI()
        End If

        If mINIFile.DoesSectionExist(ECfgSections.KEYS.ToString) = False Then
            mINIFile.Delete()
            CreateINI()
            Logger.LogTrivial("Replaced old INI; re-created file")
        End If

        ReadINI()
    End Sub

    Private Sub CreateINI()
        mINIFile.Create()

        'Settings
        mINIFile.Write(ECfgSections.SETTINGS.ToString, ESettings.PlayAdam12Intro.ToString, False)
        mINIFile.Write(ECfgSections.SETTINGS.ToString, ESettings.UnitDivision.ToString, DISPATCH.DIVISION.DIV_01.ToString)
        mINIFile.Write(ECfgSections.SETTINGS.ToString, ESettings.UnitType.ToString, DISPATCH.UNIT_TYPE.ADAM.ToString)
        mINIFile.Write(ECfgSections.SETTINGS.ToString, ESettings.UnitBeat.ToString, DISPATCH.BEAT.BEAT_12.ToString)
        mINIFile.Write(ECfgSections.SETTINGS.ToString, ESettings.EnableDispatchStatusCheck.ToString, True)
        mINIFile.Write(ECfgSections.SETTINGS.ToString, ESettings.MinTimeBetweenStatusChecks.ToString, 30)
        mINIFile.Write(ECfgSections.SETTINGS.ToString, ESettings.MaxTimeBetweenStatusChecks.ToString, 60)

        'Callouts
        For Each calloutName As String In Common.GetCalloutTypeNames()
            mINIFile.Write(ECfgSections.CALLOUTS.ToString, calloutName, True)
        Next

        'Keys
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.InteractionMenu.ToString, Keys.F9.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.InteractionMenuMod.ToString, Keys.None.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.EndCallout.ToString, Keys.Y.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.EndCalloutMod.ToString, Keys.ControlKey.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.Speak.ToString, Keys.Y.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.AskToFollow.ToString, Keys.T.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.AskToFollowMod.ToString, Keys.ControlKey.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.StatusRadioResponseOK.ToString, Keys.None.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.StatusRadioResponseOKMod.ToString, Keys.None.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.StatusRadioResponseHelpMe.ToString, Keys.None.ToString)
        mINIFile.Write(ECfgSections.KEYS.ToString, EKeys.StatusRadioResponseHelpMeMod.ToString, Keys.None.ToString)

        'Ambient Events
        mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.AmbientEventsEnabled.ToString, True)
        mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.AmbientPedBlipsEnabled.ToString, True)
        mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.MinTimeBetweenAmbientEvents.ToString, 300)
        mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.MaxTimeBetweenAmbientEvents.ToString, 600)
        mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.CitizensCall911ForAmbientEvents.ToString, True)
    End Sub

    Private Sub ReadINI()
        'Settings
        PlayAdam12Intro = mINIFile.ReadBoolean(ECfgSections.SETTINGS.ToString, ESettings.PlayAdam12Intro.ToString, False)
        UnitDivision = mINIFile.ReadEnum(Of DISPATCH.DIVISION)(ECfgSections.SETTINGS.ToString, ESettings.UnitDivision.ToString, DISPATCH.DIVISION.DIV_01)
        UnitType = mINIFile.ReadEnum(Of DISPATCH.UNIT_TYPE)(ECfgSections.SETTINGS.ToString, ESettings.UnitType.ToString, DISPATCH.UNIT_TYPE.ADAM)
        UnitBeat = mINIFile.ReadEnum(Of DISPATCH.BEAT)(ECfgSections.SETTINGS.ToString, ESettings.UnitBeat.ToString, DISPATCH.BEAT.BEAT_12)
        EnableDispatchStatusCheck = mINIFile.ReadBoolean(ECfgSections.SETTINGS.ToString, ESettings.EnableDispatchStatusCheck.ToString, True)
        MinTimeBetweenStatusChecks = Math.Abs(mINIFile.ReadInt32(ECfgSections.SETTINGS.ToString, ESettings.MinTimeBetweenStatusChecks.ToString, 30))
        MaxTimeBetweenStatusChecks = Math.Abs(mINIFile.ReadInt32(ECfgSections.SETTINGS.ToString, ESettings.MaxTimeBetweenStatusChecks.ToString, 60))

        'Callouts
        For Each calloutName As String In Common.GetCalloutTypeNames()
            If Callouts.ContainsKey(calloutName) = False Then
                Callouts.Add(calloutName, mINIFile.ReadBoolean(ECfgSections.CALLOUTS.ToString, calloutName, True))
            End If
        Next

        'Keys
        InteractionMenuKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.InteractionMenu.ToString, Keys.F9)
        InteractionMenuModKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.InteractionMenuMod.ToString, Keys.None)
        EndCallKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.EndCallout.ToString, Keys.Y)
        EndCallModKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.EndCalloutMod.ToString, Keys.ControlKey)
        SpeakKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.Speak.ToString, Keys.Y)
        AskToFollowKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.AskToFollow.ToString, Keys.T)
        AskToFollowModKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.AskToFollowMod.ToString, Keys.ControlKey)
        StatusRadioResponseOK = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.StatusRadioResponseOK.ToString, Keys.None)
        StatusRadioResponseOKModKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.StatusRadioResponseOKMod.ToString, Keys.None)
        StatusRadioResponseHelpMe = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.StatusRadioResponseHelpMe.ToString, Keys.None)
        StatusRadioResponseHelpMeModKey = mINIFile.ReadEnum(Of Keys)(ECfgSections.KEYS.ToString, EKeys.StatusRadioResponseHelpMeMod.ToString, Keys.None)

        'Ambient Events
        AmbientEventsEnabled = mINIFile.ReadBoolean(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.AmbientEventsEnabled.ToString, True)
        AmbientPedBlipsEnabled = mINIFile.ReadBoolean(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.AmbientPedBlipsEnabled.ToString, True)
        MinTimeBetweenAmbientEvents = mINIFile.ReadInt32(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.MinTimeBetweenAmbientEvents.ToString, 300)
        MaxTimeBetweenAmbientEvents = mINIFile.ReadInt32(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.MaxTimeBetweenAmbientEvents.ToString, 600)
        CitizensCall911ForAmbientEvents = mINIFile.ReadBoolean(ECfgSections.AMBIENTEVENTS.ToString, EAmbientSettings.CitizensCall911ForAmbientEvents.ToString, True)
    End Sub

    Friend Function GetInteractionMenuKey() As String
        Return GetKeyString(Config.InteractionMenuKey, Config.InteractionMenuModKey)
    End Function

    Friend Function GetKeyString(ByVal key As Keys, ByVal modKey As Keys) As String
        If modKey = Keys.None Then
            Return key.ToString()
        Else
            Dim strmodKey As String = modKey.ToString()

            If strmodKey.EndsWith("ControlKey") Or strmodKey.EndsWith("ShiftKey") Then
                strmodKey.Replace("Key", "")
            End If

            If strmodKey.Contains("ControlKey") Then
                strmodKey = "CTRL"
            ElseIf strmodKey.Contains("ShiftKey")
                strmodKey = "Shift"
            ElseIf strmodKey.Contains("Menu")
                strmodKey = "ALT"
            End If

            Return String.Format("{0} + {1}", strmodKey, key.ToString())
        End If
    End Function

    Private Enum ECfgSections
        SETTINGS
        CALLOUTS
        KEYS
        AMBIENTEVENTS
    End Enum

    Private Enum ESettings
        PlayAdam12Intro
        UnitDivision
        UnitType
        UnitBeat
        EnableDispatchStatusCheck
        MinTimeBetweenStatusChecks
        MaxTimeBetweenStatusChecks
    End Enum

    Private Enum EKeys
        InteractionMenu
        InteractionMenuMod
        EndCallout
        EndCalloutMod
        Speak
        AskToFollow
        AskToFollowMod
        StatusRadioResponseOK
        StatusRadioResponseOKMod
        StatusRadioResponseHelpMe
        StatusRadioResponseHelpMeMod
    End Enum

    Private Enum EAmbientSettings
        AmbientEventsEnabled
        AmbientPedBlipsEnabled
        MinTimeBetweenAmbientEvents
        MaxTimeBetweenAmbientEvents
        CitizensCall911ForAmbientEvents
    End Enum

End Module