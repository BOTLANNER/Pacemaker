
using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;

namespace TimeLord
{
    internal sealed class PostCharacterCreationBehaviour : CampaignBehaviorBase, ICharacterCreationContentHandler
    {
        private bool _creationStarted;
        private bool _startingAgeApplied;
        // 1.5.7 dispatches indices 0..9; the engine's maximum constant is private.
        private const int FinalCreationStage = 9;
        public void AfterInitializeContent(CharacterCreationManager characterCreationManager)
        {
        }

        public void InitializeContent(CharacterCreationManager characterCreationManager)
        {
        }

        public void OnCharacterCreationFinalize(CharacterCreationManager characterCreationManager)
        {
            // Apply after the game's staged finalization, not twice through both callbacks.
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
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationIsOver);
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, new Action(this.OnGameLoadFinished));
        }

        private void OnGameLoadFinished()
        {
            _creationStarted = false;
        }

        private void OnCharacterCreationIsOver(int stage)
        {
            if (stage != FinalCreationStage ||
                !_creationStarted || _startingAgeApplied || Main.Settings == null ||
                Game.Current?.PlayerTroop == null || CharacterObject.PlayerCharacter?.HeroObject == null)
                return;

            SetAge();
            _startingAgeApplied = true;
            _creationStarted = false;
        }

        private void OnCharacterCreationInitialized(CharacterCreationManager characterCreationManager)
        {
            _creationStarted = true;
            characterCreationManager.RegisterCharacterCreationContentHandler(this, 900000);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}
