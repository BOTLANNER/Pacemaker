using HarmonyLib;

using SandBox.View.Map;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    // Suppport for resuming in fast forward after pausing
    [HarmonyPatch(typeof(MapScreen), "HandleMouse")]
    internal static class MapScreenPatch
    {
        private static void Postfix(CampaignTimeControlMode __state)
        {
            try
            {
                if (__state == CampaignTimeControlMode.StoppableFastForward)
                {
                    Campaign.Current.TimeControlMode = CampaignTimeControlMode.StoppableFastForward;
                }
                else if (__state == CampaignTimeControlMode.UnstoppableFastForward && Campaign.Current != null && Campaign.Current.TimeControlMode == CampaignTimeControlMode.StoppablePlay)
                {
                    Campaign.Current.TimeControlMode = CampaignTimeControlMode.StoppableFastForward;
                }
            }
            catch (System.Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }
        }

        private static void Prefix(ref CampaignTimeControlMode __state)
        {
            try
            {
                __state = Campaign.Current != null ? Campaign.Current.TimeControlMode : CampaignTimeControlMode.Stop;
            }
            catch (System.Exception e) { Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  }
        }
    }
}
