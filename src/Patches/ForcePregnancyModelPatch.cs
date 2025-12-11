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
                if (!Main.Settings!.EnablePregnancyTweaks || !Main.Settings!.OverridePregnancyMods)
                {
                    return false;
                }

                var baseType = typeof(PregnancyModel);
                var defaultType = typeof(DefaultPregnancyModel);
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

                                var pregnancyModelType = type;
                                if (pregnancyModelType != null && pregnancyModelType != defaultType)
                                {
                                    MethodInfo pregnancyDurationInDaysGetter = AccessTools.PropertyGetter(pregnancyModelType, "PregnancyDurationInDays");
                                    if (pregnancyDurationInDaysGetter.DeclaringType == pregnancyModelType)
                                    {
                                        harmony.Patch(pregnancyDurationInDaysGetter, prefix: new HarmonyMethod(typeof(DefaultPregnancyModelPatch), nameof(DefaultPregnancyModelPatch.PregnancyDurationInDays)));
                                    }

                                    MethodInfo getDailyChanceOfPregnancyForHeroMethod = AccessTools.Method(pregnancyModelType, "GetDailyChanceOfPregnancyForHero");
                                    if (getDailyChanceOfPregnancyForHeroMethod.DeclaringType == pregnancyModelType)
                                    {
                                        harmony.Patch(getDailyChanceOfPregnancyForHeroMethod, prefix: new HarmonyMethod(typeof(DefaultPregnancyModelPatch), nameof(DefaultPregnancyModelPatch.GetDailyChanceOfPregnancyForHero)));
                                    }

                                    MethodInfo isHeroAgeSuitableForPregnancyMethod = AccessTools.Method(pregnancyModelType, "IsHeroAgeSuitableForPregnancy");
                                    if (isHeroAgeSuitableForPregnancyMethod.DeclaringType == pregnancyModelType)
                                    {
                                        harmony.Patch(isHeroAgeSuitableForPregnancyMethod, prefix: new HarmonyMethod(typeof(DefaultPregnancyModelPatch), nameof(DefaultPregnancyModelPatch.IsHeroAgeSuitableForPregnancy)));
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