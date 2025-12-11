using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(DefaultMobilePartyFoodConsumptionModel))]
    internal sealed class DefaultMobilePartyFoodConsumptionModelPatch
    {
        private static readonly TextObject Explanation = new($"[{Main.DisplayName}] Time Multiplier");

        [HarmonyPatch(nameof(CalculateDailyFoodConsumptionf))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void CalculateDailyFoodConsumptionf(MobileParty party, ref ExplainedNumber __result)
        {
            try
            {
                _ = (party);

                if (!Main.Settings!.EnableFoodTweaks)
                {
                    return;
                }

                var offset = (__result.ResultNumber / Main.Settings.TimeMultiplier) - __result.ResultNumber;

                if (!Util.NearEqual(offset, 0f, 1e-2f))
                {
                    __result.Add(offset, Explanation);
                }
            }
            catch (System.Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }
    }
}
