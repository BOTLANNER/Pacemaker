using System;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

using TimeLord.Extensions;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(Campaign))]
    internal class CampaignPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("TickMapTime")]
        static bool TickMapTime(ref float realDt, ref Campaign __instance)
        {
            try
            {
                switch (__instance.TimeControlMode)
                {
                    case CampaignTimeControlMode.StoppablePlay:
                    case CampaignTimeControlMode.UnstoppablePlay:
                        {
                            realDt *= Main.Settings!.PlayTimeMultiplier;
                            break;
                        }
                    case CampaignTimeControlMode.UnstoppableFastForward:
                    case CampaignTimeControlMode.UnstoppableFastForwardForPartyWaitTime:
                    case CampaignTimeControlMode.StoppableFastForward:
                        {
                            realDt *= Main.Settings!.FastForwardTimeMultiplier;
                            break;
                        }
                }
                return true;
            }
            catch (Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
                return true;
            }
        }
    }
}
