using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;

using TimeLord.Extensions;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(AgingCampaignBehavior))]
    internal static class AgingCampaignBehaviorPatch
    {
        internal static class ForOptimizer
        {
            internal static void DailyTickHero() => AgingCampaignBehaviorPatch.DailyTickHero(null!, null!, null!, null!, 0);
        }

        private delegate void IsItTimeOfDeathDelegate(AgingCampaignBehavior instance, Hero hero);
        private static readonly Reflect.Method<AgingCampaignBehavior> IsItTimeOfDeathRM = new("IsItTimeOfDeath");
        private static readonly IsItTimeOfDeathDelegate IsItTimeOfDeath = IsItTimeOfDeathRM.GetOpenDelegate<IsItTimeOfDeathDelegate>();


        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        [HarmonyPatch("DailyTickHero")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool DailyTickHero(Hero hero,
                                          AgingCampaignBehavior __instance,
                                          Dictionary<Hero, int> ____extraLivesContainer,
                                          Dictionary<Hero, int> ____heroesYoungerThanHeroComesOfAge,
                                          int ____gameStartDay)
        {

            try
            {
                bool toDays = (int) CampaignTime.Now.ToDays == ____gameStartDay;
                if (CampaignOptions.IsLifeDeathCycleDisabled || hero.IsTemplate || toDays)
                {
                    return true;
                }

                if (hero.IsAlive && hero.CanDie(KillCharacterAction.KillCharacterActionDetail.DiedOfOldAge))
                {
                    if (hero.DeathMark != KillCharacterAction.KillCharacterActionDetail.None
                        && (hero.PartyBelongedTo is null
                            || (hero.PartyBelongedTo.MapEvent is null && hero.PartyBelongedTo.SiegeEvent is null)))
                    {
                        KillCharacterAction.ApplyByDeathMark(hero, false);
                    }
                    else
                    {
                        IsItTimeOfDeath(__instance, hero);
                    }
                }


                bool adultAafEnabled = Main.Settings!.AdultAgeFactor > 1.02f;
                bool childAafEnabled = Main.Settings!.ChildAgeFactor > 1.02f;

                /* Send childhood growth stage transition events & perform AAF if enabled */

                // Subtract 1 for the daily tick's implicitly-aged day & the rest is
                // explicit, incremental age to add.
                var adultAgeDelta = CampaignTime.Days(Main.Settings.AdultAgeFactor - 1f);
                var childAgeDelta = CampaignTime.Days(Main.Settings.ChildAgeFactor - 1f);

                var oneDay = CampaignTime.Days(1f);

                // When calculating the prevAge, we must take care to include the day
                // which the daily tick implicitly aged us since we last did this, or
                // else we could miss age transitions. Ergo, prevAge is the age we
                // were as if we were one day younger than our current BirthDay.
                int prevAge = (int) (hero.BirthDay + oneDay).ElapsedYearsUntilNow;

                if (adultAafEnabled && !hero.IsChild)
                {
                    hero.SetBirthDay(hero.BirthDay - adultAgeDelta);
                }
                else if (childAafEnabled && hero.IsChild)
                {
                    hero.SetBirthDay(hero.BirthDay - childAgeDelta);
                }

                hero.CharacterObject.Age = hero.Age;

                int age = (int) hero.Age;

                if (____heroesYoungerThanHeroComesOfAge.TryGetValue(hero, out var storedAge) && storedAge != age)
                {
                    if (age >= Campaign.Current.Models.AgeModel.HeroComesOfAge)
                    {
                        ____heroesYoungerThanHeroComesOfAge.Remove(hero);
                    }
                    else
                    {
                        ____heroesYoungerThanHeroComesOfAge[hero] = age;
                    }
                }

                // Did a relevant transition in age(s) occur?
                if (age > prevAge && prevAge < Campaign.Current.Models.AgeModel.HeroComesOfAge)
                {
                    ProcessAgeTransition(hero, prevAge, age);
                }

                if (hero == Hero.MainHero && Hero.IsMainHeroIll && Hero.MainHero.HeroState != Hero.CharacterStates.Dead)
                {
                    Campaign.Current.MainHeroIllDays++;

                    if (Campaign.Current.MainHeroIllDays > 3)
                    {
                        Hero.MainHero.HitPoints -= (int) Math.Ceiling(Hero.MainHero.HitPoints * (0.05f * Campaign.Current.MainHeroIllDays));

                        if (Hero.MainHero.HitPoints <= 1 && Hero.MainHero.DeathMark == KillCharacterAction.KillCharacterActionDetail.None)
                        {
                            if (____extraLivesContainer.TryGetValue(Hero.MainHero, out int extraLives) && extraLives > 0)
                            {
                                Campaign.Current.MainHeroIllDays = -1;
                                --extraLives;

                                if (extraLives == 0)
                                {
                                    ____extraLivesContainer.Remove(Hero.MainHero);
                                }
                                else
                                {
                                    ____extraLivesContainer[Hero.MainHero] = extraLives;
                                }

                                return false;
                            }

                            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                            KillCharacterAction.ApplyByOldAge(Hero.MainHero, true);
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                return true;
            }
        }

        private static void ProcessAgeTransition(Hero hero, int prevAge, int newAge)
        {
            try
            {
                // Loop over the aged years (extremely aggressive Days Per Season + AAF
                // could make it multiple), and thus we need to be able to handle the
                // possibility of multiple growth stage events needing to be fired.

                for (int age = prevAge + 1; age <= Math.Min(newAge, Campaign.Current.Models.AgeModel.HeroComesOfAge); ++age)
                {
                    // This is a makeshift replacement for the interactive EducationCampaignBehavior,
                    // but it applies to all children-- not just the player clan's:
                    if (Main.Settings!.CustomSkillGrowth && age <= Campaign.Current.Models.AgeModel.HeroComesOfAge)
                    {
                        ChildhoodSkillGrowth(hero);
                    }

                    // This replaces AgingCampaignBehavior.OnDailyTickHero's campaign event triggers:

                    if (age == Campaign.Current.Models.AgeModel.BecomeChildAge)
                    {
                        CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(hero);
                    }

                    if (age == Campaign.Current.Models.AgeModel.BecomeTeenagerAge)
                    {
                        CampaignEventDispatcher.Instance.OnHeroReachesTeenAge(hero);
                    }

                    if (age >= Campaign.Current.Models.AgeModel.HeroComesOfAge && !hero.IsActive)
                    {
                        CampaignEventDispatcher.Instance.OnHeroComesOfAge(hero);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }
        }

        private static void ChildhoodSkillGrowth(Hero child)
        {
            try
            {
                var skill = Skills.All
                        .Where(s => child.GetAttributeValue(s.CharacterAttribute) < 3)
                        .RandomPick();

                if (skill is null)
                {
                    return;
                }

                child.HeroDeveloper.ChangeSkillLevel(skill, MBRandom.RandomInt(4, 6), false);
                child.HeroDeveloper.AddAttribute(skill.CharacterAttribute, 1, false);

                if (child.HeroDeveloper.CanAddFocusToSkill(skill))
                {
                    child.HeroDeveloper.AddFocus(skill, 1, false);
                }
            }
            catch (Exception e )
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }
        }
    }
}
