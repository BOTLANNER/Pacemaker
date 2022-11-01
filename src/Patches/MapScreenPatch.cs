using HarmonyLib;

using SandBox.View.Map;

using TaleWorlds.CampaignSystem;

namespace TimeLord.Patches
{
    // Suppport for resuming in fast forward after pausing
    [HarmonyPatch(typeof(MapScreen), "HandleMouse")]
    internal static class MapScreenPatch
    {
        private static void Postfix(CampaignTimeControlMode __state)
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

        private static void Prefix(ref CampaignTimeControlMode __state)
        {
            __state = Campaign.Current != null ? Campaign.Current.TimeControlMode : CampaignTimeControlMode.Stop;
        }
    }
}
