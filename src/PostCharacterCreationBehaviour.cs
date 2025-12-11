
using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;

namespace TimeLord
{
    internal sealed class PostCharacterCreationBehaviour : CampaignBehaviorBase, ICharacterCreationContentHandler
    {
        public void AfterInitializeContent(CharacterCreationManager characterCreationManager)
        {
        }

        public void InitializeContent(CharacterCreationManager characterCreationManager)
        {
        }

        public void OnCharacterCreationFinalize(CharacterCreationManager characterCreationManager)
        {
            SetAge();
        }

        private static void SetAge()
        {
            if (Main.Settings!.EnableAgeStageTweaks)
            {
                Game.Current.PlayerTroop.Age = Main.Settings!.HeroStartingAge;
                CharacterObject.PlayerCharacter.Age = Main.Settings!.HeroStartingAge;
                CharacterObject.PlayerCharacter.HeroObject.SetBirthDay(CampaignTime.YearsFromNow(-Main.Settings!.HeroStartingAge));
            }
        }

        public void OnStageCompleted(CharacterCreationStageBase stage)
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationInitializedEvent.AddNonSerializedListener(this, new Action<CharacterCreationManager>(this.OnCharacterCreationInitialized));
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, new Action(this.OnCharacterCreationIsOver));
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, new Action(this.OnGameLoadFinished));
        }

        private void OnGameLoadFinished()
        {
            // 
        }

        private void OnCharacterCreationIsOver()
        {
            SetAge();
        }

        private void OnCharacterCreationInitialized(CharacterCreationManager characterCreationManager)
        {
            characterCreationManager.RegisterCharacterCreationContentHandler(this, 900000);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
