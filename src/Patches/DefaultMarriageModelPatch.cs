
using HarmonyLib;

using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(DefaultMarriageModel))]
    internal class DefaultMarriageModelPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("MinimumMarriageAgeFemale", MethodType.Getter)]
        internal static bool GetMinimumMarriageAgeFemale(ref int __result)
        {
            try
            {
                if (Main.Settings!.EnableAgeStageTweaks)
                {
                    __result = Main.Settings!.HeroComesOfAge;
                    return false;
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("MinimumMarriageAgeMale", MethodType.Getter)]
        internal static bool GetMinimumMarriageAgeMale(ref int __result)
        {
            try
            {
                if (Main.Settings!.EnableAgeStageTweaks)
                {
                    __result = Main.Settings!.HeroComesOfAge;
                    return false;
                }
                return true;
            }
            catch (System.Exception e) { Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  return true; }
        }
    }
}
