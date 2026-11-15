using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    public class ForcePregnancyModelPatch : IOptionalPatch
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
                if (Main.Settings == null || !Main.Settings.EnablePregnancyTweaks || !Main.Settings.OverridePregnancyMods)
                {
                    return true;
                }

                var baseType = typeof(PregnancyModel);
                var defaultType = typeof(DefaultPregnancyModel);
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

                                var pregnancyModelType = type;
                                if (pregnancyModelType != null && pregnancyModelType != defaultType)
                                {
                                    MethodInfo pregnancyDurationInDaysGetter = AccessTools.PropertyGetter(pregnancyModelType, "PregnancyDurationInDays");
                                    if (pregnancyDurationInDaysGetter != null && !pregnancyDurationInDaysGetter.IsAbstract && pregnancyDurationInDaysGetter.DeclaringType == pregnancyModelType)
                                    {
                                        PatchSafety.PrefixOnce(harmony, pregnancyDurationInDaysGetter, typeof(DefaultPregnancyModelPatch), nameof(DefaultPregnancyModelPatch.PregnancyDurationInDays));
                                    }

                                    MethodInfo getDailyChanceOfPregnancyForHeroMethod = AccessTools.Method(pregnancyModelType, "GetDailyChanceOfPregnancyForHero", new[] { typeof(Hero) });
                                    if (getDailyChanceOfPregnancyForHeroMethod != null && !getDailyChanceOfPregnancyForHeroMethod.IsAbstract && getDailyChanceOfPregnancyForHeroMethod.DeclaringType == pregnancyModelType)
                                    {
                                        PatchSafety.PrefixOnce(harmony, getDailyChanceOfPregnancyForHeroMethod, typeof(DefaultPregnancyModelPatch), nameof(DefaultPregnancyModelPatch.GetDailyChanceOfPregnancyForHero));
                                    }

                                    MethodInfo isHeroAgeSuitableForPregnancyMethod = AccessTools.Method(pregnancyModelType, "IsHeroAgeSuitableForPregnancy", new[] { typeof(Hero) });
                                    if (isHeroAgeSuitableForPregnancyMethod != null && !isHeroAgeSuitableForPregnancyMethod.IsAbstract && isHeroAgeSuitableForPregnancyMethod.DeclaringType == pregnancyModelType)
                                    {
                                        PatchSafety.PrefixOnce(harmony, isHeroAgeSuitableForPregnancyMethod, typeof(DefaultPregnancyModelPatch), nameof(DefaultPregnancyModelPatch.IsHeroAgeSuitableForPregnancy));
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
    }
}
