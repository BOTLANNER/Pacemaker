using System;
using System.Reflection;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Library;

using TimeLord.Extensions;

namespace TimeLord.Patches
{
    public class ForceCampaignTimeModelPatch : IOptionalPatch
    {
        public bool TryPatch(Harmony harmony)
        {
            return true;
        }

        public Func<Harmony, bool>? DelayedPatch()
        {
            return null;
        }

        public bool MenusInitialised(Harmony harmony)
        {
            try
            {
                var baseType = typeof(CampaignTimeModel);
                bool result = true;

                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    foreach (var type in PatchSafety.LoadableTypes(assembly))
                    {
                        try
                        {
                            if (!type.IsAbstract && type.IsSubclassOf(baseType))
                            {

                                var campaignTimeModelType = type;
                                if (campaignTimeModelType != null)
                                {
                                    MethodInfo seasonsInYearGetter = AccessTools.PropertyGetter(campaignTimeModelType, "SeasonsInYear");
                                    if (seasonsInYearGetter != null && !seasonsInYearGetter.IsAbstract && seasonsInYearGetter.DeclaringType == campaignTimeModelType)
                                    {
                                        PatchSafety.PrefixOnce(harmony, seasonsInYearGetter, typeof(ForceCampaignTimeModelPatch), nameof(SeasonsInYear));
                                    }

                                    MethodInfo weeksInSeasonGetter = AccessTools.PropertyGetter(campaignTimeModelType, "WeeksInSeason");
                                    if (weeksInSeasonGetter != null && !weeksInSeasonGetter.IsAbstract && weeksInSeasonGetter.DeclaringType == campaignTimeModelType)
                                    {
                                        PatchSafety.PrefixOnce(harmony, weeksInSeasonGetter, typeof(ForceCampaignTimeModelPatch), nameof(WeeksInSeason));
                                    }

                                    MethodInfo daysInWeekGetter = AccessTools.PropertyGetter(campaignTimeModelType, "DaysInWeek");
                                    if (daysInWeekGetter != null && !daysInWeekGetter.IsAbstract && daysInWeekGetter.DeclaringType == campaignTimeModelType)
                                    {
                                        PatchSafety.PrefixOnce(harmony, daysInWeekGetter, typeof(ForceCampaignTimeModelPatch), nameof(DaysInWeek));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteDebugLineOnScreen(e.ToString());
                            result = false;
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteDebugLineOnScreen(e.ToString());
            }
            return false;
        }

        private static bool SeasonsInYear(ref int __result)
        {
            if (Main.Settings == null) return true;
            __result = CampaignCalendar.Seasons;
            return false;
        }
        private static bool WeeksInSeason(ref int __result)
        {
            if (Main.Settings == null) return true;
            __result = CampaignCalendar.Weeks;
            return false;
        }
        private static bool DaysInWeek(ref int __result)
        {
            if (Main.Settings == null) return true;
            __result = CampaignCalendar.Days;
            return false;
        }

    }
}
