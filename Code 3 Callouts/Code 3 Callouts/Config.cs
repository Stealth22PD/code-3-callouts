using Rage;
using Stealth.Plugins.Code3Callouts.Models.Callouts;
using Stealth.Plugins.Code3Callouts.Util;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace Stealth.Plugins.Code3Callouts
{
    internal static class Config
    {

        internal const string INIFileName = "Code 3 Callouts.ini";
        internal const string INIFilePath = "Plugins\\LSPDFR\\" + INIFileName;

        internal static InitializationFile mINIFile = new InitializationFile(INIFilePath);
        internal static string BetaKey { get; set; } = "NULL";
        internal static bool CheckForUpdates { get; set; }
        internal static bool PlayAdam12intro { get; set; }
        internal static DISPATCH.DIVISION UnitDivision { get; set; }
        internal static DISPATCH.UNIT_TYPE UnitType { get; set; }
        internal static DISPATCH.BEAT UnitBeat { get; set; }
        internal static bool EnableDispatchStatusCheck { get; set; }
        internal static int MinTimeBetweenStatusChecks { get; set; }
        internal static int MaxTimeBetweenStatusChecks { get; set; }

        internal static Dictionary<string, bool> Callouts { get; set; } = new Dictionary<string, bool>();
        internal static List<string> RegisteredCallouts { get; set; } = new List<string>();

        internal static Keys InteractionMenuKey { get; set; }
        internal static Keys InteractionMenuModKey { get; set; }
        internal static Keys EndCallKey { get; set; }
        internal static Keys EndCallModKey { get; set; }
        internal static Keys SpeakKey { get; set; }
        internal static Keys AskToFollowKey { get; set; }
        internal static Keys AskToFollowModKey { get; set; }
        internal static Keys StatusRadioResponseOK { get; set; }
        internal static Keys StatusRadioResponseOKModKey { get; set; }
        internal static Keys StatusRadioResponseHelpMe { get; set; }
        internal static Keys StatusRadioResponseHelpMeModKey { get; set; }

        internal static bool AmbientEventsEnabled { get; set; }
        internal static bool AmbientPedBlipsEnabled { get; set; }
        internal static int MinTimeBetweenAmbientEvents { get; set; }
        internal static int MaxTimeBetweenAmbientEvents { get; set; }
        internal static bool CitizensCall911ForAmbientEvents { get; set; }

        internal static void Init()
        {
            if (mINIFile.Exists() == false)
            {
                CreateINI();
            }

            if (mINIFile.DoesSectionExist(ECfgSections.KEYS.ToString()) == false)
            {
                mINIFile.Delete();
                CreateINI();
                Logger.LogTrivial("Replaced old INI; re-created file");
            }

            ReadINI();
            Logger.LogTrivial("Settings loaded");
        }

        private static void CreateINI()
        {
            mINIFile.Create();

            //Settings
            mINIFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.CheckForUpdates.ToString(), false);
            mINIFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.PlayAdam12intro.ToString(), false);
            mINIFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.UnitDivision.ToString(), DISPATCH.DIVISION.DIV_01.ToString());
            mINIFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.UnitType.ToString(), DISPATCH.UNIT_TYPE.ADAM.ToString());
            mINIFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.UnitBeat.ToString(), DISPATCH.BEAT.BEAT_12.ToString());
            mINIFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.EnableDispatchStatusCheck.ToString(), true);
            mINIFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.MinTimeBetweenStatusChecks.ToString(), 30);
            mINIFile.Write(ECfgSections.SETTINGS.ToString(), ESettings.MaxTimeBetweenStatusChecks.ToString(), 60);

            //Callouts
            foreach (string calloutName in Common.GetCalloutTypeNames())
            {
                mINIFile.Write(ECfgSections.CALLOUTS.ToString(), calloutName, true);
            }

            //Keys
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.interactionMenu.ToString(), Keys.F9.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.interactionMenuMod.ToString(), Keys.None.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.EndCallout.ToString(), Keys.Y.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.EndCalloutMod.ToString(), Keys.ControlKey.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.Speak.ToString(), Keys.Y.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.AskToFollow.ToString(), Keys.T.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.AskToFollowMod.ToString(), Keys.ControlKey.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.StatusRadioResponseOK.ToString(), Keys.None.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.StatusRadioResponseOKMod.ToString(), Keys.None.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.StatusRadioResponseHelpMe.ToString(), Keys.None.ToString());
            mINIFile.Write(ECfgSections.KEYS.ToString(), EKeys.StatusRadioResponseHelpMeMod.ToString(), Keys.None.ToString());

            //Ambient Events
            mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.AmbientEventsEnabled.ToString(), true);
            mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.AmbientPedBlipsEnabled.ToString(), true);
            mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.MinTimeBetweenAmbientEvents.ToString(), 300);
            mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.MaxTimeBetweenAmbientEvents.ToString(), 600);
            mINIFile.Write(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.CitizensCall911ForAmbientEvents.ToString(), true);
        }

        private static void ReadINI()
        {
            //Settings
            BetaKey = mINIFile.ReadString(ECfgSections.SETTINGS.ToString(), "BetaKey", "NULL");

            CheckForUpdates = mINIFile.ReadBoolean(ECfgSections.SETTINGS.ToString(), ESettings.CheckForUpdates.ToString(), false);
            PlayAdam12intro = mINIFile.ReadBoolean(ECfgSections.SETTINGS.ToString(), ESettings.PlayAdam12intro.ToString(), false);
            UnitDivision = mINIFile.ReadEnum<DISPATCH.DIVISION>(ECfgSections.SETTINGS.ToString(), ESettings.UnitDivision.ToString(), DISPATCH.DIVISION.DIV_01);
            UnitType = mINIFile.ReadEnum<DISPATCH.UNIT_TYPE>(ECfgSections.SETTINGS.ToString(), ESettings.UnitType.ToString(), DISPATCH.UNIT_TYPE.ADAM);
            UnitBeat = mINIFile.ReadEnum<DISPATCH.BEAT>(ECfgSections.SETTINGS.ToString(), ESettings.UnitBeat.ToString(), DISPATCH.BEAT.BEAT_12);
            EnableDispatchStatusCheck = mINIFile.ReadBoolean(ECfgSections.SETTINGS.ToString(), ESettings.EnableDispatchStatusCheck.ToString(), true);
            MinTimeBetweenStatusChecks = Math.Abs(mINIFile.ReadInt32(ECfgSections.SETTINGS.ToString(), ESettings.MinTimeBetweenStatusChecks.ToString(), 30));
            MaxTimeBetweenStatusChecks = Math.Abs(mINIFile.ReadInt32(ECfgSections.SETTINGS.ToString(), ESettings.MaxTimeBetweenStatusChecks.ToString(), 60));

            //Callouts
            foreach (string calloutName in Common.GetCalloutTypeNames())
            {
                if (Callouts.ContainsKey(calloutName) == false)
                {
                    Callouts.Add(calloutName, mINIFile.ReadBoolean(ECfgSections.CALLOUTS.ToString(), calloutName, true));
                }
            }

            //Keys
            InteractionMenuKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.interactionMenu.ToString(), Keys.F9);
            InteractionMenuModKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.interactionMenuMod.ToString(), Keys.None);
            EndCallKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.EndCallout.ToString(), Keys.Y);
            EndCallModKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.EndCalloutMod.ToString(), Keys.ControlKey);
            SpeakKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.Speak.ToString(), Keys.Y);
            AskToFollowKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.AskToFollow.ToString(), Keys.T);
            AskToFollowModKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.AskToFollowMod.ToString(), Keys.ControlKey);
            StatusRadioResponseOK = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.StatusRadioResponseOK.ToString(), Keys.None);
            StatusRadioResponseOKModKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.StatusRadioResponseOKMod.ToString(), Keys.None);
            StatusRadioResponseHelpMe = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.StatusRadioResponseHelpMe.ToString(), Keys.None);
            StatusRadioResponseHelpMeModKey = mINIFile.ReadEnum<Keys>(ECfgSections.KEYS.ToString(), EKeys.StatusRadioResponseHelpMeMod.ToString(), Keys.None);

            //Ambient Events
            AmbientEventsEnabled = mINIFile.ReadBoolean(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.AmbientEventsEnabled.ToString(), true);
            AmbientPedBlipsEnabled = mINIFile.ReadBoolean(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.AmbientPedBlipsEnabled.ToString(), true);
            MinTimeBetweenAmbientEvents = mINIFile.ReadInt32(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.MinTimeBetweenAmbientEvents.ToString(), 300);
            MaxTimeBetweenAmbientEvents = mINIFile.ReadInt32(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.MaxTimeBetweenAmbientEvents.ToString(), 600);
            CitizensCall911ForAmbientEvents = mINIFile.ReadBoolean(ECfgSections.AMBIENTEVENTS.ToString(), EAmbientSettings.CitizensCall911ForAmbientEvents.ToString(), true);
        }

        internal static string GetinteractionMenuKey()
        {
            return GetKeyString(Config.InteractionMenuKey, Config.InteractionMenuModKey);
        }

        internal static string GetKeyString(Keys key, Keys modKey)
        {
            if (modKey == Keys.None)
            {
                return key.ToString();
            }
            else
            {
                string strmodKey = modKey.ToString();

                if (strmodKey.EndsWith("ControlKey") | strmodKey.EndsWith("ShiftKey"))
                {
                    strmodKey.Replace("Key", "");
                }

                if (strmodKey.Contains("ControlKey"))
                {
                    strmodKey = "CTRL";
                }
                else if (strmodKey.Contains("ShiftKey"))
                {
                    strmodKey = "Shift";
                }
                else if (strmodKey.Contains("Menu"))
                {
                    strmodKey = "ALT";
                }

                return string.Format("{0} + {1}", strmodKey, key.ToString());
            }
        }

        private enum ECfgSections
        {
            SETTINGS,
            CALLOUTS,
            KEYS,
            AMBIENTEVENTS
        }

        private enum ESettings
        {
            CheckForUpdates,
            PlayAdam12intro,
            UnitDivision,
            UnitType,
            UnitBeat,
            EnableDispatchStatusCheck,
            MinTimeBetweenStatusChecks,
            MaxTimeBetweenStatusChecks
        }

        private enum EKeys
        {
            interactionMenu,
            interactionMenuMod,
            EndCallout,
            EndCalloutMod,
            Speak,
            AskToFollow,
            AskToFollowMod,
            StatusRadioResponseOK,
            StatusRadioResponseOKMod,
            StatusRadioResponseHelpMe,
            StatusRadioResponseHelpMeMod
        }

        private enum EAmbientSettings
        {
            AmbientEventsEnabled,
            AmbientPedBlipsEnabled,
            MinTimeBetweenAmbientEvents,
            MaxTimeBetweenAmbientEvents,
            CitizensCall911ForAmbientEvents
        }

    }
}