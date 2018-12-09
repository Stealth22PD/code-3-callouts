using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Ambient;
using Stealth.Plugins.Code3Callouts.Models.Ambient.EventTypes;
using Stealth.Plugins.Code3Callouts.Models.Callouts;
using Stealth.Plugins.Code3Callouts.Util;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;

namespace Stealth.Plugins.Code3Callouts
{
    internal static class Driver
    {

        internal static MenuPool gMenuPool = new MenuPool();

        internal static UIMenu mInteractionMenu = null;
        private static List<object> mRadioOptions = new List<object>() {
        "Status OK",
        "Help Me!"
    };

        internal static UIMenuListItem mRadioDispatch = new UIMenuListItem("Radio Dispatch", mRadioOptions.ToList(), 0);
        internal static UIMenuItem mObserveSubject = new UIMenuItem("Observe Subject");
        internal static UIMenuItem mSpeakToSubject = new UIMenuItem("Speak to Subject");
        internal static UIMenuItem mQuestionSubject = new UIMenuItem("Question Subject");
        internal static UIMenuItem mAskForID = new UIMenuItem("Ask Subject for ID");

        internal static UIMenuItem mAskToFollow = new UIMenuItem("Ask Subject to Follow");
        static IEnumerable<object> callouts = Common.GetCalloutFriendlyNames();

        internal static UIMenuListItem mStartCallout = new UIMenuListItem("Start Callout", callouts.ToList(), 0);
        internal static UIMenuItem mEndCallout = new UIMenuItem("End Callout");

        internal static UIMenuItem mCloseMenu = new UIMenuItem("Close Menu");
        private static DateTime mLastStatusCheck;
        private static int mTimeUntilNextStatusCheck;
        private static bool mDispatchCallingPlayer = false;

        private static DateTime mDispatchCalled;
        internal static void RunAmbientEvents()
        {
            if (Config.AmbientEventsEnabled)
            {
                GameFiber.StartNew(() =>
                {
                    while (Common.gIsPlayerOnDuty)
                    {
                        if (Common.gActiveAmbientEvent == null)
                        {
                            TimeSpan ts = (DateTime.Now - Common.gLastAmbientEvent);

                            if (ts.TotalSeconds > Common.gTimeUntilNextAmbientEvent)
                            {
                                AmbientBase a = GetRandomAmbientEvent();
                                if (a != null)
                                {
                                    if (a.Start())
                                    {
                                        Common.gActiveAmbientEvent = a;
                                    }
                                    else
                                    {
                                        a = null;
                                        Common.gLastAmbientEvent = DateTime.Now;
                                        Common.RandomizeAmbientinterval();
                                    }
                                }
                                else
                                {
                                    Common.gLastAmbientEvent = DateTime.Now;
                                    Common.RandomizeAmbientinterval();
                                }
                            }
                        }
                        else
                        {
                            if (Common.gActiveAmbientEvent.Active == false)
                            {
                                Common.gActiveAmbientEvent = null;
                                Common.gLastAmbientEvent = DateTime.Now;
                                Common.RandomizeAmbientinterval();
                            }
                        }

                        GameFiber.Yield();
                    }
                });
            }
        }

        private static AmbientBase GetRandomAmbientEvent()
        {
            if (Functions.IsCalloutRunning() == false && Functions.IsPlayerPerformingPullover() == false)
            {
                int eventFactor = Common.gRandom.Next(1, 100);

                return new StolenVehicle();

                switch (eventFactor)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                        return new Mugging();
                    case 11: // TODO: to 40
                        return new FightinProgress();
                    case 41: // TODO: to 70
                        return new StolenVehicle();
                    default:
                        return new Carjacking();
                }

            }
            else
            {
                return null;
            }
        }

        internal static void RunDispatchStatusCheck()
        {
            if (Config.EnableDispatchStatusCheck)
            {
                ListenForResponseKeys();
                mLastStatusCheck = DateTime.Now;
                mTimeUntilNextStatusCheck = Common.gRandom.Next(15, 30);

                GameFiber.StartNew(() =>
                {
                    while (Common.gIsPlayerOnDuty)
                    {
                        TimeSpan ts = (DateTime.Now - mLastStatusCheck);

                        if (ts.TotalMinutes > mTimeUntilNextStatusCheck)
                        {
                            if (Functions.GetActivePursuit() == null)
                            {
                                AskPlayerForStatus();
                            }
                            else
                            {
                                mLastStatusCheck = DateTime.Now;
                                mTimeUntilNextStatusCheck = Common.gRandom.Next(Config.MinTimeBetweenStatusChecks, Config.MaxTimeBetweenAmbientEvents);
                            }
                        }

                        GameFiber.Yield();
                    }
                });
            }
        }

        private static void ListenForResponseKeys()
        {
            if (Config.StatusRadioResponseOK == Keys.None || Config.StatusRadioResponseHelpMe == Keys.None)
                return;

            GameFiber.StartNew(() =>
            {
                while (Common.gIsPlayerOnDuty)
                {
                    if (mDispatchCallingPlayer)
                    {
                        if (Config.StatusRadioResponseOK != Keys.None && Config.StatusRadioResponseOK.IsKeyPressedWithModKey(Config.StatusRadioResponseOKModKey))
                        {
                            mDispatchCallingPlayer = false;
                            OfficerRespondOK();

                            //if (Game.IsKeyDown(Config.StatusRadioResponseOK))
                            //{
                            //    if (Config.StatusRadioResponseOKModKey == Keys.None || Game.IsKeyDownRightNow(Config.StatusRadioResponseOKModKey))
                            //    {
                                    
                            //    }
                            //}
                        }

                        if (Config.StatusRadioResponseHelpMe != Keys.None && Config.StatusRadioResponseHelpMe.IsKeyPressedWithModKey(Config.StatusRadioResponseHelpMeModKey))
                        {
                            mDispatchCallingPlayer = false;
                            OfficerRespondHelpMe();

                            //if (Game.IsKeyDown(Config.StatusRadioResponseHelpMe))
                            //{
                            //    if (Config.StatusRadioResponseHelpMeModKey == Keys.None || Game.IsKeyDownRightNow(Config.StatusRadioResponseHelpMeModKey))
                            //    {
                                    
                            //    }
                            //}
                        }
                    }

                    GameFiber.Yield();
                }
            });
        }

        private static void AskPlayerForStatus()
        {
            mLastStatusCheck = DateTime.Now;
            CallPlayerForStatus();
            mTimeUntilNextStatusCheck = Common.gRandom.Next(Config.MinTimeBetweenStatusChecks, Config.MaxTimeBetweenAmbientEvents);

            GameFiber.StartNew(() =>
            {
                Game.DisplayHelp("Use the interaction menu to respond to Dispatch.", 10000);
                GameFiber.Sleep(10000);
                Game.DisplayHelp(string.Format("You can open the menu by pressing ~b~{0}.", Config.GetinteractionMenuKey()), 10000);
                GameFiber.Sleep(10000);

                if (mDispatchCallingPlayer)
                {
                    Game.DisplayHelp("If you don't respond, Dispatch will assume you need backup!", 10000);
                    GameFiber.Sleep(10000);
                }
            });

            GameFiber.StartNew(() =>
            {
                GameFiber.Sleep(30000);

                if (mDispatchCallingPlayer)
                {
                    CallPlayerForStatus();
                    Game.DisplayHelp("Dispatch is waiting for your response!!", 10000);
                    GameFiber.Sleep(10000);

                    if (mDispatchCallingPlayer)
                    {
                        Game.DisplayHelp(string.Format("You can open the menu by pressing ~b~{0}.", Config.GetinteractionMenuKey()), 10000);
                        GameFiber.Sleep(10000);
                    }
                }
            });

            GameFiber.StartNew(() =>
            {
                mDispatchCallingPlayer = true;
                mDispatchCalled = DateTime.Now;

                GameFiber.Sleep(60000);

                if (mDispatchCallingPlayer)
                {
                    mDispatchCallingPlayer = false;
                    CallBackupForPlayer(true);
                }
            });
        }

        private static void CallPlayerForStatus()
        {
            Radio.DispatchCallingUnit();
            Game.DisplayNotification(string.Format("~b~Dispatch~w~: ~w~{0}, what's your status?", Common.gUnitNumber));
        }

        internal static void CallBackupForPlayer(bool pNoResponse)
        {
            GameFiber.StartNew(() =>
            {
                if (Game.LocalPlayer.Character.Exists())
                {
                    Vector3 destPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position);

                    Radio.OfficerCode99(pNoResponse);
                    GameFiber.Sleep(5000);

                    Functions.RequestBackup(destPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                    Functions.RequestBackup(destPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                    Functions.RequestBackup(destPoint, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                }
            });
        }

        private static void OfficerRespondOK()
        {
            GameFiber.StartNew(() =>
            {
                if (Functions.IsPlayerPerformingPullover())
                {
                    Radio.UnitMessage(string.Format("{0} is Code 6 on a traffic stop", Common.gUnitNumber));
                }
                else if (Functions.IsCalloutRunning())
                {
                    if (Game.LocalPlayer.Character.IsOnFoot)
                    {
                        Radio.UnitMessage(string.Format("{0}, show me Code 6", Common.gUnitNumber));
                    }
                    else
                    {
                        Radio.UnitMessage(string.Format("{0} currently 10-8", Common.gUnitNumber));
                    }
                }
                else
                {
                    if (Game.LocalPlayer.Character.IsOnFoot)
                    {
                        Radio.UnitMessage(string.Format("{0}, show me Code 6", Common.gUnitNumber));
                    }
                    else
                    {
                        Radio.UnitMessage(string.Format("{0} currently 10-8", Common.gUnitNumber));
                    }
                }

                GameFiber.Sleep(1000);
                Radio.DispatchMessage("10-4", true);
                Radio.DispatchAcknowledge();
            });
        }

        private static void OfficerRespondHelpMe()
        {
            GameFiber.StartNew(() =>
            {
                Radio.UnitMessage(string.Format("{0}, I'm Code 99 at this time", Common.gUnitNumber));
                GameFiber.Sleep(1000);
                Radio.DispatchMessage("10-4", true);
                Radio.DispatchAcknowledge();
                GameFiber.Sleep(2000);
                CallBackupForPlayer(false);
            });
        }

        private static void StartCallout(string name)
        {
            GameFiber.StartNew(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(name) == false)
                    {
                        if (Config.RegisteredCallouts.Contains(name))
                        {
                            if (Functions.IsCalloutRunning())
                            {
                                Functions.StopCurrentCallout();
                            }

                            Functions.StartCallout(name);
                        }
                        else
                        {
                            Game.DisplayNotification(string.Format("~r~ERROR: ~b~{0} ~w~ is ~r~disabled ~w~in the config file.", name));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Game.DisplayNotification("~r~ERROR: ~w~" + ex.Message);
                    Util.Logger.LogTrivial("Error starting callout -- " + ex.ToString());
                }
            });
        }

        internal static void InitializeMenu()
        {
            gMenuPool = new MenuPool();
            mInteractionMenu = new UIMenu("Code 3 Callouts", "~b~INTERACTION MENU");
            mInteractionMenu.MouseControlsEnabled = false;
            mInteractionMenu.AllowCameraMovement = true;

            mInteractionMenu.AddItem(mRadioDispatch);
            mInteractionMenu.AddItem(mObserveSubject);
            mInteractionMenu.AddItem(mSpeakToSubject);
            mInteractionMenu.AddItem(mQuestionSubject);
            mInteractionMenu.AddItem(mAskForID);
            mInteractionMenu.AddItem(mAskToFollow);
            mInteractionMenu.AddItem(mStartCallout);
            mInteractionMenu.AddItem(mEndCallout);
            mInteractionMenu.AddItem(mCloseMenu);

            mInteractionMenu.RefreshIndex();

            gMenuPool.Add(mInteractionMenu);

            Game.FrameRender += ProcessMenu;
            RegisterMenuEvents();
            ProcessMenuItems();
        }

        private static void RegisterMenuEvents()
        {
            mInteractionMenu.OnItemSelect += minteractionMenu_OnItemselect;
        }

        private static void UnregisterMenuEvents()
        {
            mInteractionMenu.OnItemSelect -= minteractionMenu_OnItemselect;
        }

        private static void ProcessMenuItems()
        {
            GameFiber.StartNew(() =>
            {
                while (Common.gIsPlayerOnDuty)
                {
                    GameFiber.Yield();

                    mRadioDispatch.Enabled = false;
                    mObserveSubject.Enabled = false;
                    mSpeakToSubject.Enabled = false;
                    mQuestionSubject.Enabled = false;
                    mAskForID.Enabled = false;
                    mAskToFollow.Enabled = false;
                    mEndCallout.Enabled = false;

                    if (mDispatchCallingPlayer)
                        mRadioDispatch.Enabled = true;

                    if (Common.ClosestPed != null && Common.ClosestPed.Exists())
                    {
                        mAskForID.Enabled = true;

                        if (!string.IsNullOrEmpty(Common.ClosestPed.PhysicalCondition))
                        {
                            mObserveSubject.Enabled = true;
                        }

                        if (Common.ClosestPed.IsAlive)
                        {
                            if (Common.ClosestPed.SpeechLines.Count > 0)
                            {
                                mSpeakToSubject.Enabled = true;
                            }

                            if (Common.ClosestPed.QAItems != null)
                            {
                                mQuestionSubject.Enabled = true;
                            }

                            mAskForID.Text = "Ask Subject for ID";

                            if (Common.ClosestPed.IsOnFoot == true & Common.ClosestPed.IsInCombat == false)
                            {
                                if (Game.LocalPlayer.Character.IsOnFoot)
                                {
                                    mAskToFollow.Enabled = true;

                                    if (Common.FollowMePed != null & Common.FollowMePed.Exists())
                                    {
                                        mAskToFollow.Text = "Stop Following";
                                    }
                                    else
                                    {
                                        mAskToFollow.Text = "Ask Subject to Follow";
                                    }
                                }
                                else
                                {
                                    mAskToFollow.Enabled = false;
                                }
                            }
                        }
                        else
                        {
                            mAskForID.Text = "Check Subject for ID";
                        }
                    }

                    if (Functions.IsCalloutRunning())
                    {
                        mEndCallout.Enabled = true;
                    }
                }
            });
        }

        private static void ProcessMenu(object sender, GraphicsEventArgs e)
        {
            if (gMenuPool == null)
            {
                return;
            }

            if (Common.IsKeyDown(Config.InteractionMenuKey, Config.InteractionMenuModKey))
            {
                if (mInteractionMenu.Visible == false)
                {
                    mInteractionMenu.Visible = true;
                }
                else
                {
                    mInteractionMenu.Visible = false;
                }

                //if (Config.InteractionMenuModKey == Keys.None || Game.IsKeyDownRightNow(Config.InteractionMenuModKey))
                //{
                    
                //}
            }

            gMenuPool.ProcessMenus();
        }

        private static void minteractionMenu_OnItemselect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender.Equals(mInteractionMenu) == true)
            {
                if (object.ReferenceEquals(selectedItem, mRadioDispatch))
                {
                    if (mDispatchCallingPlayer)
                    {
                        mDispatchCallingPlayer = false;

                        switch (mRadioDispatch.Index)
                        {
                            case 0:
                                //OK
                                OfficerRespondOK();
                                break;

                            case 1:
                                //HELP!
                                OfficerRespondHelpMe();
                                break;
                        }

                    }

                }
                else if (object.ReferenceEquals(selectedItem, mStartCallout))
                {
                    //Start callout
                    string mCalloutToStart = mStartCallout.IndexToItem(mStartCallout.Index);
                    StartCallout(mCalloutToStart);

                }
                else if (object.ReferenceEquals(selectedItem, mEndCallout))
                {
                    if (Functions.IsCalloutRunning())
                    {
                        Functions.StopCurrentCallout();
                    }

                }
                else if (object.ReferenceEquals(selectedItem, mCloseMenu))
                {
                    mInteractionMenu.Visible = false;

                }
            }
        }

        internal static void MakePedImmuneToTrafficEvents(Ped p)
        {
            try
            {
                if (Common.IsTrafficPolicerRunning())
                {
                    if (p != null && p.Exists())
                    {
                        TrafficPolicerFunctions.MakePedImmuneToAmbientEvents(p);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogVerboseDebug("Error making ped immune to Traffic Policer events -- " + ex.ToString());
            }
        }

    }
}