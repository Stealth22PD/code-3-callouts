using LSPD_First_Response.Mod.API;
using Rage;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Stealth.Common.Functions;
using System.IO;

namespace Stealth.Plugins.Code3Callouts
{
    public sealed class Main : Plugin
    {
        public Main()
        {
        }

        public override void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((sender, e) => LSPDFRResolveEventHandler(sender, e));
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;

            Config.Init();

            if (Config.PlayAdam12intro == true)
            {
                AudioPlayerEngine.PlayAudio(Common.Unit_1ADAM12);
            }
        }

        public override void Finally()
        {
            Common.gIsPlayerOnDuty = false;
        }

        static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            Common.gIsPlayerOnDuty = onDuty;

            if (Common.PreloadChecks() == false)
            {
                return;
            }

            if (Common.IsBeta && onDuty)
            {
                Logger.LogTrivial("This is a beta build; authenticating beta key...");

                // Beta funcs missing from my version of Stealth.Common??

                //System.Threading.Tasks.Task.Run(async () =>
                //{
                //    string fileSecretUUID = "7b42cf6a-cbe3-4dba-99eb-938ceedb7918";

                //    bool isBetaKeyValid = await BetaFuncs.IsValidKey(Constants.LCPDFRDownloadID, fileSecretUUID, Config.BetaKey);

                //    if (!isBetaKeyValid)
                //    {
                //        Logger.LogTrivial("ERROR: Beta key authorization failed!");
                //        GameFuncs.DisplayNotification(Common.VersionInfo.ProductName, "BETA KEY CHECK", "~r~AUTHENTICATION FAILED!");
                //        return;
                //    }
                //    else
                //    {
                //        Logger.LogTrivial("Beta key authorization succeeded!");
                //        StartPlugin(onDuty);
                //    }
                //});
            }
            else if (!Common.IsBeta && onDuty)
            {
                Logger.LogTrivial("This is a release build.");
                StartPlugin(onDuty);
            }
        }

        private static void StartPlugin(bool onDuty)
        {
            if (onDuty == true)
            {
                GameFiber.StartNew(() =>
                {
                    Common.RegisterCallouts();
                    GameFiber.Sleep(2000);

                    Common.Init();

                    string initMsg = string.Format("Loaded {0} v{1}.{2}.{3}", Common.VersionInfo.ProductName, Common.Version.Major, Common.Version.Minor, Common.Version.Build); //, Common.Version.Revision
                    string initMsg2 = string.Format("Loaded ~b~v{0}.{1}.{2}", Common.Version.Major, Common.Version.Minor, Common.Version.Build); //, Common.Version.Revision

                    Logger.LogTrivial(initMsg);

                    string mTitle = "CODE 3 CALLOUTS";
                    string mSubtitle = "~b~Developed By Stealth22";
                    Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", mTitle, mSubtitle, initMsg2);

                    Common.CheckForUpdates();

                }, "StartPlugin");
            }
        }

        static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs e)
        {
            foreach (Assembly a in Functions.GetAllUserPlugins())
            {
                if (e.Name.ToLower().Contains(a.GetName().Name.ToLower()))
                {
                    return a;
                }
            }

            return null;
        }

    }
}