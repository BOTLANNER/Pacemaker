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
                    foreach (var type in assembly.GetTypes())
                    {
                        try
                        {
                            if (type.IsSubclassOf(baseType))
                            {

                                var campaignTimeModelType = type;
                                if (campaignTimeModelType != null)
                                {
                                    MethodInfo seasonsInYearGetter = AccessTools.PropertyGetter(campaignTimeModelType, "SeasonsInYear");
                                    if (seasonsInYearGetter.DeclaringType == campaignTimeModelType)
                                    {
                                        harmony.Patch(seasonsInYearGetter, prefix: new HarmonyMethod(typeof(ForceCampaignTimeModelPatch), nameof(ForceCampaignTimeModelPatch.SeasonsInYear)));
                                    }

                                    MethodInfo weeksInSeasonGetter = AccessTools.PropertyGetter(campaignTimeModelType, "WeeksInSeason");
                                    if (weeksInSeasonGetter.DeclaringType == campaignTimeModelType)
                                    {
                                        harmony.Patch(weeksInSeasonGetter, prefix: new HarmonyMethod(typeof(ForceCampaignTimeModelPatch), nameof(ForceCampaignTimeModelPatch.WeeksInSeason)));
                                    }

                                    MethodInfo daysInWeekGetter = AccessTools.PropertyGetter(campaignTimeModelType, "DaysInWeek");
                                    if (daysInWeekGetter.DeclaringType == campaignTimeModelType)
                                    {
                                        harmony.Patch(daysInWeekGetter, prefix: new HarmonyMethod(typeof(ForceCampaignTimeModelPatch), nameof(ForceCampaignTimeModelPatch.DaysInWeek)));
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
            __result = Main.Settings!.SeasonsInYear;
            return false;
        }
        private static bool WeeksInSeason(ref int __result)
        {
            __result = Main.Settings!.WeeksInSeason;
            return false;
        }
        private static bool DaysInWeek(ref int __result)
        {
            __result = Main.Settings!.DaysInWeek;
            return false;
        }

    }
}