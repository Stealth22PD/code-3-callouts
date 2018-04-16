Imports LSPD_First_Response.Mod.API
Imports LSPD_First_Response.Mod.Callouts
Imports Rage
Imports Stealth.Plugins.Code3Callouts.Models.Ambient
Imports Stealth.Plugins.Code3Callouts.Models.Callouts
Imports Stealth.Plugins.Code3Callouts.Util
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports System.Reflection
Imports Stealth.Plugins.Code3Callouts.Models.Peds
Imports System.Windows.Forms

Public Module Common

    Friend Property gIsPlayerOnDuty As Boolean = False
    Friend Property gLastAmbientEvent As DateTime
    Friend Property gTimeUntilNextAmbientEvent As Integer
    Friend Property gActiveAmbientEvent As AmbientBase = Nothing
    Friend Property ClosestPed As PedBase = Nothing
    Friend Property FollowMePed As Ped = Nothing
    Friend Property IsCalloutActive As Boolean = False

    Friend ReadOnly Property AmbientBlipColor() As Drawing.Color
        Get
            Return Drawing.Color.LightYellow
        End Get
    End Property

    Public gRandom As New Random()

    Public Property gLCPDFRDownloadID As Integer = 8082

    Public Sub Init()
        Logger.LogTrivial(String.Format("Loading {0}...", My.Application.Info.Title))
        Config.Init()
        Logger.LogTrivial("Settings loaded")
        gLastAmbientEvent = DateTime.Now
        RandomizeAmbientInterval()
    End Sub

    Public Sub RandomizeAmbientInterval()
        gTimeUntilNextAmbientEvent = gRandom.Next(Config.MinTimeBetweenAmbientEvents, Config.MaxTimeBetweenAmbientEvents)
    End Sub

    Public Sub RegisterCallouts()
        RegisterCalloutTypes()
        Logger.LogTrivial("All callouts registered")
    End Sub

    Friend Function GetCalloutTypes() As List(Of Type)
        Dim calloutClasses As List(Of Type) = (From x In Assembly.GetExecutingAssembly().GetTypes()
                                               Where x.IsClass AndAlso x.Namespace = "Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes" AndAlso x.BaseType = GetType(CalloutBase)
                                               Select x).ToList()

        Return calloutClasses
    End Function

    Friend Function GetCalloutTypeNames() As List(Of String)
        Return (From x In GetCalloutTypes() Select x.Name).ToList()
    End Function

    Friend Function GetCalloutFriendlyNames() As List(Of String)
        Dim names As New List(Of String)
        Dim calloutClasses As List(Of Type) = GetCalloutTypes()

        For Each x In calloutClasses
            Try
                Dim attribs As Object() = x.GetCustomAttributes(GetType(CalloutInfoAttribute), True)

                If attribs.Count > 0 Then
                    Dim calloutAttrib As CalloutInfoAttribute = CType((From a In attribs Select a).FirstOrDefault(), CalloutInfoAttribute)

                    If calloutAttrib IsNot Nothing Then
                        names.Add(calloutAttrib.Name)
                    End If
                End If
            Catch ex As Exception
            End Try
        Next

        names.Sort()
        Return names
    End Function

    Private Sub RegisterCalloutTypes()
        Dim calloutClasses As List(Of Type) = GetCalloutTypes()

        For Each x In calloutClasses
            Dim calloutName As String = x.Name
            Dim friendlyName As String = calloutName

            Try
                Dim attribs As Object() = x.GetCustomAttributes(GetType(CalloutInfoAttribute), True)

                If attribs.Count > 0 Then
                    Dim calloutAttrib As CalloutInfoAttribute = CType((From a In attribs Select a).FirstOrDefault(), CalloutInfoAttribute)

                    If calloutAttrib IsNot Nothing Then
                        friendlyName = calloutAttrib.Name
                    End If
                End If
            Catch ex As Exception
                friendlyName = calloutName
            End Try

            If Config.Callouts.ContainsKey(calloutName) AndAlso Config.Callouts(calloutName) Then
                Functions.RegisterCallout(x)
                Config.RegisteredCallouts.Add(friendlyName)
                Logger.LogTrivial(String.Format("Registered Callout: {0}", friendlyName))
            Else
                Logger.LogTrivial(String.Format("Callout '{0}' is disabled via the configuration file", friendlyName))
            End If
        Next
    End Sub

    Private Sub RegisterIndividualCalloutTypes()
        'Functions.RegisterCallout(GetType(CalloutTypes.UnknownTrouble))
        'Logger.LogTrivial("Registered Callout: Unknown Trouble")

        'Functions.RegisterCallout(GetType(CalloutTypes.PersonWithWeapon))
        'Logger.LogTrivial("Registered Callout: Person With a Gun")

        'Functions.RegisterCallout(GetType(CalloutTypes.Assault))
        'Logger.LogTrivial("Registered Callout: Assault")

        'Functions.RegisterCallout(GetType(CalloutTypes.IntoxicatedPerson))
        'Logger.LogTrivial("Registered Callout: Intoxicated Person")

        'Functions.RegisterCallout(GetType(CalloutTypes.ImpairedDriver))
        'Logger.LogTrivial("Registered Callout: Impaired Driver")

        'Functions.RegisterCallout(GetType(CalloutTypes.HitAndRun))
        'Logger.LogTrivial("Registered Callout: Hit And Run")
    End Sub

    Public Async Sub CheckForUpdates()
        Await Threading.Tasks.Task.Factory.StartNew(AddressOf SendAPIWebRequest)
    End Sub

    Private Async Sub SendAPIWebRequest()
        Dim mApiURL As String = String.Format("http://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={0}&textOnly=1", gLCPDFRDownloadID)
        Dim mApiString As String = ""

        Try
            Using wc As New System.Net.WebClient()
                mApiString = Await wc.DownloadStringTaskAsync(mApiURL)
            End Using
        Catch ex As Exception
            mApiString = ""
            Logger.LogVeryVerbose("Error getting newest version: " & ex.ToString())
        End Try

        Try
            If mApiString <> "" Then
                Dim webVersion As Version = Version.Parse(mApiString)
                Dim myVersion As Version = My.Application.Info.Version

                Dim versionFactor As Integer = webVersion.CompareTo(myVersion)

                If webVersion > myVersion Then
                    Game.DisplayNotification(String.Format("~g~NOTE: ~w~There is a newer version of ~b~{0} ~w~available.", My.Application.Info.Title))
                    Logger.LogTrivial(String.Format("There is a newer version of {0} available. Please visit LCPDFR.com and download it.", My.Application.Info.Title))
                End If
            End If
        Catch ex As Exception
            Logger.LogVeryVerbose("Error comparing versions: " & ex.ToString())
        End Try
    End Sub

    Public Function PreloadChecks() As Boolean
        Return IsRPHVersionRecentEnough() AndAlso IsLSPDFRVersionRecentEnough() AndAlso IsCommonDLLValid() AndAlso CheckRAGENativeUIVersion()
    End Function

    Public Function IsCommonDLLValid() As Boolean
        Return CheckAssemblyVersion("Stealth.Common.dll", "Stealth.Common DLL", "1.6.0.1")
    End Function

    Public Function IsRPHVersionRecentEnough() As Boolean
        Return CheckAssemblyVersion("RAGEPluginHook.exe", "RAGE Plugin Hook", "0.39.946.8098")
    End Function

    Public Function IsLSPDFRVersionRecentEnough() As Boolean
        Return CheckAssemblyVersion("Plugins\LSPD First Response.dll", "LSPDFR", "0.3.38.5436")
    End Function

    Public Function CheckRAGENativeUIVersion() As Boolean
        Return CheckAssemblyVersion("RAGENativeUI.dll", "RAGENativeUI DLL", "1.4.1.0")
    End Function

    Private Function CheckAssemblyVersion(ByVal pFilePath As String, ByVal pFileAlias As String, ByVal pRequiredVersion As String) As Boolean
        Dim isValid As Boolean = True

        Try
            If System.IO.File.Exists(pFilePath) Then
                Dim mRequiredVersion As Version = Version.Parse(pRequiredVersion)
                Dim mInstalledVersion As Version = Nothing

                If Version.TryParse(FileVersionInfo.GetVersionInfo(pFilePath).FileVersion, mInstalledVersion) = True Then
                    If mRequiredVersion.CompareTo(mInstalledVersion) > 0 Then
                        Game.DisplayNotification(String.Format("~r~ERROR: ~w~{0} requires v{1} of ~b~{2}~w~; v{3} found.", My.Application.Info.Title, pRequiredVersion, pFileAlias, mInstalledVersion.ToString()))
                        Logger.LogTrivial(String.Format("ERROR: {0} requires at least v{1} of {2}. Older version ({3}) found; {0} cannot run.", My.Application.Info.Title, pRequiredVersion, pFileAlias, mInstalledVersion.ToString()))
                        isValid = False
                    End If
                End If
            Else
                Game.DisplayNotification(String.Format("~r~ERROR: ~b~{0} ~w~missing; {1} cannot run without this file.", pFileAlias, My.Application.Info.Title))
                Logger.LogTrivial(String.Format("ERROR: {0} requires at least v{1} of {2}. {2} not found; {0} cannot run.", My.Application.Info.Title, pRequiredVersion, pFileAlias))
                isValid = False
            End If
        Catch ex As Exception
            Logger.LogVerboseDebug(String.Format("Error while checking for {0}: {1}", pFileAlias, ex.ToString()))
        End Try

        Return isValid
    End Function

    Friend Function IsComputerPlusRunning() As Boolean
        Return IsLSPDFRPluginRunning("ComputerPlus", New Version(1, 3, 2, 0))
    End Function

    Friend Function IsTrafficPolicerRunning() As Boolean
        Return IsLSPDFRPluginRunning("Traffic Policer", New Version(6, 9, 5, 0))
    End Function

    Friend Function IsLSPDFRPluginRunning(ByVal pName As String, Optional ByVal pMinVersion As Version = Nothing) As Boolean
        Try
            If DoesLSPDFRPluginExist(pName, pMinVersion) Then
                Logger.LogTrivialDebug("Plugin exists")

                For Each a As Assembly In Functions.GetAllUserPlugins()
                    Dim an As AssemblyName = a.GetName()
                    If an.Name.ToLower() = pName.ToLower() Then
                        If pMinVersion Is Nothing OrElse an.Version.CompareTo(pMinVersion) >= 0 Then
                            Logger.LogTrivialDebug("Plugin is running")
                            Return True
                        End If
                    End If
                Next

                Logger.LogTrivialDebug("Plugin is not running")
                Return False
            Else
                Logger.LogTrivialDebug("Plugin does not exist")
                Return False
            End If
        Catch ex As Exception
            Logger.LogTrivialDebug("Error getting plugin -- returning false")
            Return False
        End Try
    End Function

    Private Function DoesLSPDFRPluginExist(ByVal pName As String, Optional ByVal pMinVersion As Version = Nothing) As Boolean
        Dim mFilePath As String = String.Format("Plugins\LSPDFR\{0}.dll", pName)

        If System.IO.File.Exists(mFilePath) Then
            If pMinVersion Is Nothing Then
                Return True
            Else
                Dim mInstalledVersion As Version = Nothing

                If Version.TryParse(FileVersionInfo.GetVersionInfo(mFilePath).FileVersion, mInstalledVersion) = True Then
                    If pMinVersion.CompareTo(mInstalledVersion) > 0 Then
                        Return False
                    Else
                        Return True
                    End If
                Else
                    Return False
                End If
            End If
        Else
            Return False
        End If
    End Function

    Public Function ResponseString(ByVal pResponse As CallResponseType) As String
        Return pResponse.ToString().Replace("_", " ")
    End Function

    Public Function GetRandomDateOfBirth() As Date
        Dim today As Date = Date.Today
        Dim year As Integer = Date.Today.Year - (gRandom.Next(32, 45))
        Dim month As Integer = gRandom.Next(1, 13)
        Dim day As Integer = gRandom.Next(1, 31)
        If month = 2 AndAlso day > 28 Then
            day = 28
        End If

        Return New Date(year, month, day)
    End Function

    Public Function CreatePursuit(Optional ByVal pActiveForPlayer As Boolean = True, Optional ByVal pAIDisabled As Boolean = False, Optional ByVal pCopsCanJoin As Boolean = True) As LHandle
        Dim mPursuitHandle As LHandle = Functions.CreatePursuit()
        Functions.SetPursuitIsActiveForPlayer(mPursuitHandle, pActiveForPlayer)
        Functions.SetPursuitDisableAI(mPursuitHandle, pAIDisabled)
        Functions.SetPursuitCopsCanJoin(mPursuitHandle, pCopsCanJoin)
        Return mPursuitHandle
    End Function

    Public Function GetNativeArgument(ByVal pEntity As Entity) As Native.NativeArgument
        Return New Native.NativeArgument(CType(pEntity, IHandleable))
    End Function

    Public Function IsPlayerInLosSantos() As Boolean
        Dim areaHash As UInteger = Rage.Native.NativeFunction.CallByName(Of UInteger)("GET_HASH_OF_MAP_AREA_AT_COORDS", Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y, Game.LocalPlayer.Character.Position.Z)

        If areaHash = Game.GetHashKey("city") Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function GetHeadingToPoint(ByVal pOriginPoint As Vector3, ByVal pDestinationPoint As Vector3) As Single
        Dim mDirection As Vector3 = (pDestinationPoint - pOriginPoint)
        mDirection.Normalize()

        Return MathHelper.ConvertDirectionToHeading(mDirection)
    End Function

    Friend ReadOnly Property gUnitNumber() As String
        Get
            Dim div As String = UnitDivision.ToString.Replace("DIV_", "")
            Dim beat As String = UnitBeat.ToString.Replace("BEAT_", "")
            Return div & "-" & UnitType.ToString & "-" & beat
        End Get
    End Property

    Friend ReadOnly Property UnitAudio() As List(Of AudioFile)
        Get
            Dim unit As New List(Of AudioFile)
            unit.Add(New AudioFile("DISPATCH", UnitDivision))
            unit.Add(New AudioFile("DISPATCH", UnitType))
            unit.Add(New AudioFile("DISPATCH", UnitBeat))
            Return unit
        End Get
    End Property

    Friend ReadOnly Property Unit_1ADAM12() As List(Of AudioFile)
        Get
            Dim pAudio As New List(Of AudioFile)
            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.DIVISION.DIV_01))
            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.UNIT_TYPE.ADAM))
            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.BEAT.BEAT_12))
            pAudio.Add(New AudioFile("DISPATCH", AudioDatabase.DISPATCH.REPORTING.WEVE_GOT))
            pAudio.Add(New AudioFile("DISPATCH", AudioDatabase.DISPATCH.CRIMES.CODE_211))
            pAudio.Add(New AudioFile("DISPATCH", AudioDatabase.DISPATCH.RESPONSE_TYPES.RESPOND_CODE_3))
            pAudio.Add(New AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.COPY_IN_VICINITY))
            Return pAudio
        End Get
    End Property

    Friend Function GetDirectionAudioFromHeading(ByVal pHeading As Single) As String
        Select Case pHeading
            Case 0 To 45
                Return "DIR_NORTHBOUND"
            Case 46 To 135
                Return "DIR_WESTBOUND"
            Case 136 To 225
                Return "DIR_SOUTHBOUND"
            Case 226 To 315
                Return "DIR_EASTBOUND"
            Case 316 To 360
                Return "DIR_NORTHBOUND"
            Case Else
                Return ""
        End Select
    End Function

    Private _AudioFolder As String = "Plugins\LSPDFR\Code 3 Callouts\Audio"
    Public ReadOnly Property AudioFolder() As String
        Get
            Return _AudioFolder
        End Get
    End Property

    Public Enum CalloutState
        Cancelled = -1
        Created = 0
        Dispatched = 1
        UnitResponding = 2
        AtScene = 3
        Completed = 4
    End Enum

    Public Enum CallResponseType
        Code_2 = 2
        Code_3 = 3
    End Enum

    Public Enum PedType
        Unknown = 0
        Witness = 1
        Victim = 2
        Suspect = 3
        Cop = 4
    End Enum

End Module