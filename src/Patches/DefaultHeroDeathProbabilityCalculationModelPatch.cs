using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Library;

namespace Pacemaker.Patches
{
    internal sealed class DefaultHeroDeathProbabilityCalculationModelPatch : Patch
    {
        internal DefaultHeroDeathProbabilityCalculationModelPatch()
            : base(Type.Prefix,
                   new Reflect.Method<DefaultHeroDeathProbabilityCalculationModel>("CalculateHeroDeathProbabilityInternal"),
                   new Reflect.Method<DefaultHeroDeathProbabilityCalculationModelPatch>(nameof(CalculateHeroDeathProbabilityInternal)),
                   HarmonyLib.Priority.HigherThanNormal)
        { }

        private static bool CalculateHeroDeathProbabilityInternal(ref float __result, Hero hero)
        {
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

                        // Transform for Pacemaker age factor
                        age *= Main.Settings!.AdultAgeFactor;

                        float single1 = 1f - MathF.Pow(1f - age, 0.0119047621f);
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
    }
}
