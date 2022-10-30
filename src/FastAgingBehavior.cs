using Pacemaker.Extensions;

using System;
using System.ComponentModel;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;

namespace Pacemaker
{
    internal sealed class FastAgingBehavior : CampaignBehaviorBase
    {
        ~FastAgingBehavior()
        {
            Settings.Instance!.PropertyChanged -= Settings_OnPropertyChanged;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHero);

            // register for settings property-changed events
            Settings.Instance!.PropertyChanged += Settings_OnPropertyChanged;
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            var agingBehavior = Campaign.Current.CampaignBehaviorManager.GetBehavior<AgingCampaignBehavior>();
            IsItTimeOfDeath = IsItTimeOfDeathRM.GetDelegate<IsItTimeOfDeathDelegate>(agingBehavior);

            CacheValues();
        }

        private void Settings_OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender is Settings && args.PropertyName == Settings.SaveTriggered)
            {
                CacheValues();
            }
        }

        private void CacheValues()
        {
            if (Campaign.Current != null && Campaign.Current.Models != null && Campaign.Current.Models.AgeModel != null)
            {
                // Save these for later:
                adultAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
                teenAge = Campaign.Current.Models.AgeModel.BecomeTeenagerAge;
                childAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
            }
            else if (Settings.Instance != null)
            {
                Settings.Instance.PropertyChanged -= Settings_OnPropertyChanged;
            }
        }

        private void OnDailyTickHero(Hero hero)
        {
            if (CampaignOptions.IsLifeDeathCycleDisabled)
            {
                return;
            }

            //New periodic death probability update per hero
            if (!CampaignOptions.IsLifeDeathCycleDisabled && !hero.IsTemplate)
            {
                if (hero.IsAlive && hero.CanDie(KillCharacterAction.KillCharacterActionDetail.DiedOfOldAge))
                {
                    if (hero.DeathMark == KillCharacterAction.KillCharacterActionDetail.None || hero.PartyBelongedTo != null && (hero.PartyBelongedTo.MapEvent != null || hero.PartyBelongedTo.SiegeEvent != null))
                    {
                        IsItTimeOfDeath!(hero);
                    }
                    else
                    {
                        KillCharacterAction.ApplyByDeathMark(hero, false);
                    }
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

            // And our new age, if different.
            int newAge = (int) hero.Age;

            // Did a relevant transition in age(s) occur?
            if (newAge > prevAge && prevAge < adultAge && !hero.IsTemplate)
            {
                ProcessAgeTransition(hero, prevAge, newAge);
            }
        }

        private void ProcessAgeTransition(Hero hero, int prevAge, int newAge)
        {
            // Loop over the aged years (extremely aggressive Days Per Season + AAF
            // could make it multiple), and thus we need to be able to handle the
            // possibility of multiple growth stage events needing to be fired.

            for (int age = prevAge + 1; age <= Math.Min(newAge, adultAge); ++age)
            {
                // This is a makeshift replacement for the interactive EducationCampaignBehavior,
                // but it applies to all children-- not just the player clan's:
                if (age <= adultAge)
                {
                    ChildhoodSkillGrowth(hero);
                }

                // This replaces AgingCampaignBehavior.OnDailyTickHero's campaign event triggers:

                if (age == childAge)
                {
                    CampaignEventDispatcher.Instance.OnHeroGrowsOutOfInfancy(hero);
                }

                if (age == teenAge)
                {
                    CampaignEventDispatcher.Instance.OnHeroReachesTeenAge(hero);
                }

                if (age == adultAge && !hero.IsActive)
                {
                    CampaignEventDispatcher.Instance.OnHeroComesOfAge(hero);
                }
            }
        }

        private void ChildhoodSkillGrowth(Hero child)
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

        // Year thresholds (cached):
        private int adultAge;
        private int teenAge;
        private int childAge;

        // Delegates, delegates, delegates...
        private delegate void IsItTimeOfDeathDelegate(Hero hero);
        private delegate void OnHeroComesOfAgeDelegate(Hero hero);
        private delegate void OnHeroReachesTeenAgeDelegate(Hero hero);
        private delegate void OnHeroGrowsOutOfInfancyDelegate(Hero hero);

        private IsItTimeOfDeathDelegate? IsItTimeOfDeath;

        // Reflection for triggering campaign events & death probability updates & childhood education stage processing:
        private static readonly Reflect.Method<AgingCampaignBehavior> IsItTimeOfDeathRM = new("IsItTimeOfDeath");
    }
}
