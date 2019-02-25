using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Ambient;
using Stealth.Plugins.Code3Callouts.Models.Callouts;
using Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System.Reflection;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioPlayerEngine;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System.Windows.Forms;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace Stealth.Plugins.Code3Callouts
{
    internal static class Common
    {
        internal const string cDLLPath = @"Plugins\LSPDFR\Code 3 Callouts.dll";

        private static FileVersionInfo mVersionInfo = null;
        internal static FileVersionInfo VersionInfo
        {
            get
            {
                if (mVersionInfo == null)
                    mVersionInfo = FileVersionInfo.GetVersionInfo(cDLLPath);

                return mVersionInfo;
            }
        }

        internal static Version Version
        {
            get
            {
                return Version.Parse(VersionInfo.FileVersion);
            }
        }

        internal static bool IsBeta
        {
            get
            {
#if BETA
                return true;
#else
                return false;
#endif
            }
        }

        static internal bool gIsPlayerOnDuty { get; set; }
        static internal DateTime gLastAmbientEvent { get; set; }
        static internal int gTimeUntilNextAmbientEvent { get; set; }
        static internal AmbientBase gActiveAmbientEvent { get; set; }
        static internal PedBase ClosestPed { get; set; }
        static internal Ped FollowMePed { get; set; }
        static internal bool IsCalloutActive { get; set; }

        static internal Color AmbientBlipColor {
            get { return Color.LightYellow; }
        }

        internal static Random gRandom = new Random();

        internal static void Init()
        {
            Logger.LogTrivial(string.Format("Loading {0}...", mVersionInfo.ProductName));

            if (Config.EnableDispatchStatusCheck)
            {
                Driver.RunDispatchStatusCheck();
            }

            gLastAmbientEvent = DateTime.Now;
            RandomizeAmbientinterval();

            Driver.RunAmbientEvents();
            Driver.InitializeMenu();
        }

        internal static bool IsKeyDown(Keys key)
        {
            return key.IsKeyPressed();
        }

        internal static bool IsKeyDown(Keys key, Keys modKey)
        {
            return key.IsKeyPressedWithModKey(modKey);
        }

        internal static void RandomizeAmbientinterval()
        {
            gTimeUntilNextAmbientEvent = Common.gRandom.Next(Config.MinTimeBetweenAmbientEvents, Config.MaxTimeBetweenAmbientEvents);
        }

        internal static void RegisterCallouts()
        {
            RegisterCalloutTypes();
            Logger.LogTrivial("All callouts registered");
        }

        static internal List<Type> GetCalloutTypes()
        {
            //Logger.LogTrivial("classes: " + Assembly.GetExecutingAssembly().GetTypes().Count());

            // && x.Namespace == "Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes"
            List<Type> types = (from x in Assembly.GetExecutingAssembly().GetTypes()
                    where x.IsClass && x.BaseType == typeof(CalloutBase)
                    select x).ToList();

            //Logger.LogTrivial("Callout classes: " + types.Count());

            return types;
        }

        internal static List<string> GetCalloutTypeNames()
        {
            return (from x in GetCalloutTypes() select x.Name).ToList();
        }

        internal static List<string> GetCalloutFriendlyNames()
        {
            List<string> names = new List<string>();
            List<Type> calloutClasses = GetCalloutTypes();

            foreach (var x in calloutClasses) {
                try {
                    object[] attribs = x.GetCustomAttributes(typeof(CalloutInfoAttribute), true);

                    if (attribs.Count() > 0) {
                        CalloutInfoAttribute calloutAttrib = (CalloutInfoAttribute)(from a in attribs select a).FirstOrDefault();

                        if (calloutAttrib != null) {
                            names.Add(calloutAttrib.Name);
                        }
                    }
                } catch {
                }
            }

            names.Sort();
            return names;
        }

        private static void RegisterCalloutTypes()
        {
            List<Type> calloutClasses = GetCalloutTypes();

            foreach (var x in calloutClasses) {
                string calloutName = x.Name;
                string friendlyName = calloutName;

                try {
                    object[] attribs = x.GetCustomAttributes(typeof(CalloutInfoAttribute), true);

                    if (attribs.Count() > 0) {
                        CalloutInfoAttribute calloutAttrib = (CalloutInfoAttribute)(from a in attribs select a).FirstOrDefault();

                        if (calloutAttrib != null) {
                            friendlyName = calloutAttrib.Name;
                        }
                    }
                } catch {
                    friendlyName = calloutName;
                }

                if (Config.Callouts.ContainsKey(calloutName) && Config.Callouts[calloutName]) {
                    Functions.RegisterCallout(x);
                    Config.RegisteredCallouts.Add(friendlyName);
                    Logger.LogTrivial(string.Format("Registered Callout: {0}", friendlyName));
                } else {
                    Logger.LogTrivial(string.Format("Callout '{0}' is disabled via the configuration file", friendlyName));
                }
            }
        }

        private static void RegisterindividualCalloutTypes()
        {
            //Functions.RegisterCallout(typeof(CalloutTypes.UnknownTrouble))
            //Logger.LogTrivial("Registered Callout: Unknown Trouble")

            //Functions.RegisterCallout(typeof(CalloutTypes.PersonWithWeapon))
            //Logger.LogTrivial("Registered Callout: Person With a Gun")

            //Functions.RegisterCallout(typeof(CalloutTypes.Assault))
            //Logger.LogTrivial("Registered Callout: Assault")

            //Functions.RegisterCallout(typeof(CalloutTypes.intoxicatedPerson))
            //Logger.LogTrivial("Registered Callout: Intoxicated Person")

            //Functions.RegisterCallout(typeof(CalloutTypes.ImpairedDriver))
            //Logger.LogTrivial("Registered Callout: Impaired Driver")

            //Functions.RegisterCallout(typeof(CalloutTypes.HitAndRun))
            //Logger.LogTrivial("Registered Callout: Hit and Run")
        }

        internal static void CheckForUpdates()
        {
            if (Config.CheckForUpdates == true) {
                Stealth.Common.Functions.UpdateFuncs.CheckForUpdates(Constants.LCPDFRDownloadID, Common.Version, Common.VersionInfo.ProductName, true);
            }
        }

        internal static bool PreloadChecks()
        {
            return IsRPHVersionRecentEnough() && IsLSPDFRVersionRecentEnough() && IsCommonDLLValid() && CheckRAGENativeUIVersion();
        }

        internal static bool IsCommonDLLValid()
        {
            return CheckAssemblyVersion("Stealth.Common.dll", "Stealth.Common DLL", Constants.ReqCommonVersion);
        }

        internal static bool IsRPHVersionRecentEnough()
        {
            return CheckAssemblyVersion("RAGEPluginHook.exe", "RAGE Plugin Hook", Constants.ReqRPHVersion);
        }

        internal static bool IsLSPDFRVersionRecentEnough()
        {
            return CheckAssemblyVersion("Plugins\\LSPD First Response.dll", "LSPDFR", Constants.ReqLSPDFRVersion);
        }

        internal static bool CheckRAGENativeUIVersion()
        {
            return CheckAssemblyVersion("RAGENativeUI.dll", "RAGENativeUI DLL", Constants.ReqRNUIVersion);
        }

        private static bool CheckAssemblyVersion(string pFilePath, string pFileAlias, string pRequiredVersion)
        {
            bool isValid = true;

            try {
                if (System.IO.File.Exists(pFilePath)) {
                    Version mRequiredVersion = Version.Parse(pRequiredVersion);

                    if (Version.TryParse(FileVersionInfo.GetVersionInfo(pFilePath).FileVersion, out Version mInstalledVersion) == true)
                    {
                        if (mRequiredVersion.CompareTo(mInstalledVersion) > 0)
                        {
                            Game.DisplayNotification(string.Format("~r~ERROR: ~w~{0} requires v{1} of ~b~{2}~w~; v{3} found.", mVersionInfo.ProductName, pRequiredVersion, pFileAlias, mInstalledVersion.ToString()));
                            Logger.LogTrivial(string.Format("ERROR: {0} requires at least v{1} of {2}. Older version ({3}) found; {0} cannot run.", mVersionInfo.ProductName, pRequiredVersion, pFileAlias, mInstalledVersion.ToString()));
                            isValid = false;
                        }
                    }
                } else {
                    Game.DisplayNotification(string.Format("~r~ERROR: ~b~{0} ~w~missing; {1} cannot run without this file.", pFileAlias, mVersionInfo.ProductName));
                    Logger.LogTrivial(string.Format("ERROR: {0} requires at least v{1} of {2}. {2} not found; {0} cannot run.", mVersionInfo.ProductName, pRequiredVersion, pFileAlias));
                    isValid = false;
                }
            } catch (Exception ex) {
                Logger.LogVerboseDebug(string.Format("Error while checking for {0}: {1}", pFileAlias, ex.ToString()));
            }

            return isValid;
        }

        static internal bool IsBetterEMSRunning()
        {
            return IsLSPDFRPluginRunning("BetterEMS", new Version(3, 0, 0, 0));
        }

        static internal bool IsComputerPlusRunning()
        {
            return IsLSPDFRPluginRunning("ComputerPlus", new Version(1, 3, 3, 0));
        }

        static internal bool IsCustomBackupRunning()
        {
            return IsLSPDFRPluginRunning("CustomBackupLocations", new Version(3, 3, 0, 0)); 
        }

        static internal bool IsTrafficPolicerRunning()
        {
            return IsLSPDFRPluginRunning("Traffic Policer", new Version(6, 13, 6, 3));
        }

        static internal bool IsLSPDFRPluginRunning(string pName, Version pMinVersion = null)
        {
            try {
                if (DoesLSPDFRPluginExist(pName, pMinVersion)) {
                    Logger.LogTrivialDebug("Plugin exists");

                    foreach (Assembly a in Functions.GetAllUserPlugins()) {
                        AssemblyName an = a.GetName();
                        if (an.Name.ToLower() == pName.ToLower()) {
                            if (pMinVersion == null || an.Version.CompareTo(pMinVersion) >= 0) {
                                Logger.LogTrivialDebug("Plugin is running");
                                return true;
                            }
                        }
                    }

                    Logger.LogTrivialDebug("Plugin is not running");
                    return false;
                } else {
                    Logger.LogTrivialDebug("Plugin does not exist");
                    return false;
                }
            } catch {
                Logger.LogTrivialDebug("Error getting plugin -- returning false");
                return false;
            }
        }

        private static bool DoesLSPDFRPluginExist(string pName, Version pMinVersion = null)
        {
            string mFilePath = string.Format("Plugins\\LSPDFR\\{0}.dll", pName);

            if (System.IO.File.Exists(mFilePath)) {
                if (pMinVersion == null) {
                    return true;
                } else {
                    Version minstalledVersion = null;

                    if (Version.TryParse(FileVersionInfo.GetVersionInfo(mFilePath).FileVersion, out minstalledVersion) == true) {
                        if (pMinVersion.CompareTo(minstalledVersion) > 0) {
                            return false;
                        } else {
                            return true;
                        }
                    } else {
                        return false;
                    }
                }
            } else {
                return false;
            }
        }

        internal static string ResponseString(CallResponseType pResponse)
        {
            return pResponse.ToString().Replace("_", " ");
        }

        internal static System.DateTime GetRandomDateOfBirth()
        {
            System.DateTime today = System.DateTime.Today;
            int year = System.DateTime.Today.Year - (Common.gRandom.Next(32, 45));
            int month = Common.gRandom.Next(1, 13);
            int day = Common.gRandom.Next(1, 31);
            if (month == 2 && day > 28) {
                day = 28;
            }

            return new System.DateTime(year, month, day);
        }

        internal static LHandle CreatePursuit(bool pActiveForPlayer = true, bool pAIDisabled = false, bool pCopsCanJoin = true)
        {
            LHandle mPursuitHandle = Functions.CreatePursuit();
            Functions.SetPursuitIsActiveForPlayer(mPursuitHandle, pActiveForPlayer);
            Functions.SetPursuitDisableAI(mPursuitHandle, pAIDisabled);
            Functions.SetPursuitCopsCanJoin(mPursuitHandle, pCopsCanJoin);
            return mPursuitHandle;
        }

        internal static void RequestBackupToLocation(Vector3 destPoint, LSPD_First_Response.EBackupResponseType responseType, LSPD_First_Response.EBackupUnitType unitType, int unitCount = 1)
        {
            if(unitType == LSPD_First_Response.EBackupUnitType.Ambulance) {
                if (Common.IsBetterEMSRunning()) {
                    BetterEMSFunctions.RequestBackup(destPoint, unitCount);
                }

                else {
                    if (unitCount == 1) {
                        Functions.RequestBackup(destPoint, responseType, unitType);
                    }

                    else {
                        for (int loopCount = 0; loopCount < unitCount; loopCount++) {
                            Functions.RequestBackup(destPoint, responseType, unitType);
                        }

                    }
                }
            }

            else {
                if (Common.IsCustomBackupRunning()) {
                    CustomBackupFunctions.RequestBackup(destPoint, unitType, responseType, unitCount);
                }

                else {
                    if (unitCount == 1) {
                        Functions.RequestBackup(destPoint, responseType, unitType);
                    }

                    else {
                        for (int loopCount = 0; loopCount < unitCount; loopCount++) {
                            Functions.RequestBackup(destPoint, responseType, unitType);
                        }
                    }
                }
            }
        }

        internal static bool IsPlayerinLosSantos()
        {
            uint areaHash = Rage.Native.NativeFunction.Natives.GetHashOfMapAreaAtCoords<uint>(Game.LocalPlayer.Character.Position.X, Game.LocalPlayer.Character.Position.Y, Game.LocalPlayer.Character.Position.Z);

            if (areaHash == Game.GetHashKey("city")) {
                return true;
            } else {
                return false;
            }
        }

        internal static Persona BuildPersona(Ped Ped, LSPD_First_Response.Gender Gender, DateTime Birthday, int Citations, string Forename, string Surname, ELicenseState LicenseState, int TimesStopped, bool IsWanted, bool IsAgent, bool IsCop)
        {
            Persona Persona = Functions.GetPersonaForPed(Ped);
            Persona.Gender = Gender;
            Persona.Birthday = Birthday;
            Persona.Citations = Citations;
            Persona.Forename = Forename;
            Persona.Surname = Surname;
            Persona.ELicenseState = LicenseState;
            Persona.TimesStopped = TimesStopped;
            Persona.Wanted = IsWanted;

            if (IsAgent || IsCop)
                Functions.SetPedAsCop(Ped);

            return Persona;
        }

        internal static float GetHeadingToPoint(Vector3 pOriginPoint, Vector3 pDestinationPoint)
        {
            Vector3 mDirection = (pDestinationPoint - pOriginPoint);
            mDirection.Normalize();

            return MathHelper.ConvertDirectionToHeading(mDirection);
        }

        static internal string gUnitNumber {
            get {
                string div = Config.UnitDivision.ToString().Replace("DIV_", "");
                string beat = Config.UnitBeat.ToString().Replace("BEAT_", "");
                return div + "-" + Config.UnitType.ToString() + "-" + beat;
            }
        }

        static internal List<AudioFile> UnitAudio {
            get {
                List<AudioFile> unit = new List<AudioFile>();
                unit.Add(new AudioFile("DISPATCH", Config.UnitDivision));
                unit.Add(new AudioFile("DISPATCH", Config.UnitType));
                unit.Add(new AudioFile("DISPATCH", Config.UnitBeat));
                return unit;
            }
        }

        static internal List<AudioFile> Unit_1ADAM12 {
            get {
                List<AudioFile> pAudio = new List<AudioFile>();
                pAudio.Add(new AudioFile("DISPATCH", DISPATCH.DIVISION.DIV_01));
                pAudio.Add(new AudioFile("DISPATCH", DISPATCH.UNIT_TYPE.ADAM));
                pAudio.Add(new AudioFile("DISPATCH", DISPATCH.BEAT.BEAT_12));
                pAudio.Add(new AudioFile("DISPATCH", AudioDatabase.DISPATCH.REPORTING.WEVE_GOT));
                pAudio.Add(new AudioFile("DISPATCH", AudioDatabase.DISPATCH.CRIMES.CODE_211));
                pAudio.Add(new AudioFile("DISPATCH", AudioDatabase.DISPATCH.RESPONSE_TYPES.RESPOND_CODE_3));
                pAudio.Add(new AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.COPY_IN_VICINITY));
                return pAudio;
            }
        }

        static internal string GetDirectionAudiofromHeading(float pHeading)
        {
            switch (pHeading) {
                case 0: // TODO: to 45
                    return "DIR_NORTHBOUND";
                case 46: // TODO: to 135
                    return "DIR_WESTBOUND";
                case 136: // TODO: to 225
                    return "DIR_SOUTHBOUND";
                case 226: // TODO: to 315
                    return "DIR_EASTBOUND";
                case 316: // TODO: to 360
                    return "DIR_NORTHBOUND";
                default:
                    return "";
            }
        }

        private static string _AudioFolder = "Plugins\\LSPDFR\\Code 3 Callouts\\Audio";
        internal static string AudioFolder {
            get { return _AudioFolder; }
        }

        internal enum CalloutState
        {
            Cancelled = -1,
            Created = 0,
            Dispatched = 1,
            UnitResponding = 2,
            AtScene = 3,
            Completed = 4
        }

        internal enum CallResponseType
        {
            Code_2 = 2,
            Code_3 = 3
        }

        internal enum PedType
        {
            Unknown = 0,
            Witness = 1,
            Victim = 2,
            Suspect = 3,
            Cop = 4
        }

    }
}
