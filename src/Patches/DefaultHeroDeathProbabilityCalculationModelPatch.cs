using System;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(DefaultHeroDeathProbabilityCalculationModel))]
    public class DefaultHeroDeathProbabilityCalculationModelPatch
    {
        [HarmonyPriority(HarmonyLib.Priority.HigherThanNormal)]
        [HarmonyAfter("vampireplayer.immortality")]
        [HarmonyPatch(nameof(CalculateHeroDeathProbabilityInternal))]
        [HarmonyPrefix]
        private static bool CalculateHeroDeathProbabilityInternal(ref float __result, Hero hero)
        {
            try
            {
                if (Main.Settings == null || Campaign.Current == null || hero == null) return true;
                float single = 0f;
                if (!CampaignOptions.IsLifeDeathCycleDisabled)
                {
                    int becomeOldAge = Campaign.Current.Models.AgeModel.BecomeOldAge;
                    int maxAge = Campaign.Current.Models.AgeModel.MaxAge - 1;
                    if (hero.Age > (float) becomeOldAge)
                    {
                        if (hero.Age < (float) maxAge)
                        {
                            float age = 0.3f * ((hero.Age - (float) becomeOldAge) / (float) (Campaign.Current.Models.AgeModel.MaxAge - becomeOldAge));

                            // Transform for TimeLord age factor
                            age *= Main.Settings!.AdultAgeFactor;

                            if (float.IsNaN(age) || float.IsInfinity(age) || CampaignTime.DaysInYear <= 0) return true;
                            // Keep the existing curve in its valid domain; avoid fractional powers of a negative base.
                            age = Math.Max(0f, Math.Min(1f, age));
                            float single1 = 1f - MathF.Pow(1f - age, 1f / (float) CampaignTime.DaysInYear);
                            single += single1;
                        }
                        else if (hero.Age >= (float) maxAge)
                        {
                            single += 1f;
                        }
                    }
                }
                __result = single;

                // Prevent running default function
                return false;
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
