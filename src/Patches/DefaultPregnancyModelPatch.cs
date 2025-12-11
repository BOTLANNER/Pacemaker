using System;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(DefaultPregnancyModel))]
    internal static class DefaultPregnancyModelPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch("get_PregnancyDurationInDays")]
        internal static bool PregnancyDurationInDays(ref float __result)
        {
            try
            {
                if (!Main.Settings!.EnablePregnancyTweaks)
                {
                    return true;
                }

                __result = Main.Settings.ScaledPregnancyDuration * Campaign.Current.Models.CampaignTimeModel.DaysInWeek * Campaign.Current.Models.CampaignTimeModel.WeeksInSeason * Campaign.Current.Models.CampaignTimeModel.SeasonsInYear;
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

        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch("GetDailyChanceOfPregnancyForHero")]
        public static bool GetDailyChanceOfPregnancyForHero(ref float __result, ref PregnancyModel __instance, Hero hero)
        {
            try
            {
                int count = hero.Children.Count + 1;
                float tier = (float) (4 + 4 * hero.Clan.Tier);
                float single = (hero == Hero.MainHero || hero.Spouse == Hero.MainHero ? 1f : Math.Min(1f, (2f * tier - (float) hero.Clan.AliveLords.Count) / tier));
                float age = (1.2f - (hero.Age - 18f) * 0.04f) / (float) (count * count) * 0.12f * single;
                bool suitable = false;
                ExplainedNumber explainedNumber = new ExplainedNumber((hero.Spouse == null || (!IsHeroAgeSuitableForPregnancy(ref __instance, hero, ref suitable) && !suitable) ? 0f : age), false, null);
                if (hero.GetPerkValue(DefaultPerks.Charm.Virile) || (hero.Spouse?.GetPerkValue(DefaultPerks.Charm.Virile) ?? false))
                {
                    explainedNumber.AddFactor(DefaultPerks.Charm.Virile.PrimaryBonus, DefaultPerks.Charm.Virile.Name);
                }
                __result = explainedNumber.ResultNumber;
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


        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch("IsHeroAgeSuitableForPregnancy")]
        internal static bool IsHeroAgeSuitableForPregnancy(ref PregnancyModel __instance, Hero hero, ref bool __result)
        {
            try
            {
                if (hero.Age < Main.Settings!.HeroComesOfAge)
                {
                    __result = false;
                }
                else
                {
                    __result = hero.Age <= Main.Settings!.BecomeOldAge;
                }

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
