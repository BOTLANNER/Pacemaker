
using HarmonyLib;

using TaleWorlds.CampaignSystem.GameComponents;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(DefaultMarriageModel))]
    internal class DefaultMarriageModelPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("MinimumMarriageAgeFemale", MethodType.Getter)]
        internal static bool GetMinimumMarriageAgeFemale(ref int __result)
        {
            if (Main.Settings!.EnableAgeStageTweaks)
            {
                __result = Main.Settings!.HeroComesOfAge;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("MinimumMarriageAgeMale", MethodType.Getter)]
        internal static bool GetMinimumMarriageAgeMale(ref int __result)
        {
            if (Main.Settings!.EnableAgeStageTweaks)
            {
                __result = Main.Settings!.HeroComesOfAge;
                return false;
            }
            return true;
        }
    }
}
