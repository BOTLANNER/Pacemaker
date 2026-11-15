// Test doubles only. This executable does not load or patch any game assembly.
using System;
using System.Runtime.CompilerServices;

namespace TimeLord
{
    internal static class Main { internal static Settings? Settings = new Settings(); }
    internal sealed class Settings
    {
        public int DaysInWeek = 7, WeeksInSeason = 3, SeasonsInYear = 4, HeroStartingAge = 22;
        public bool EnableAgeStageTweaks = true;
    }
    internal static class Util { internal static readonly TestLog Log = new TestLog(); }
    internal sealed class TestLog { public void ToFile(string value) => Console.WriteLine(value); }
}
namespace TaleWorlds.Core
{
    public class Game
    {
        public static Game Current = new Game();
        public TaleWorlds.CampaignSystem.CharacterObject PlayerTroop = TaleWorlds.CampaignSystem.CharacterObject.PlayerCharacter;
    }
}
namespace TaleWorlds.CampaignSystem
{
    public sealed class Campaign { public static Campaign Current = new Campaign(); }
    public struct CampaignTime
    {
        public static int DaysInWeek, WeeksInSeason, SeasonsInYear;
        public float Years;
        public static CampaignTime YearsFromNow(float value) => new CampaignTime { Years = value };
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Initialize()
        {
            DaysInWeek = TimeLord.CampaignCalendar.Days;
            WeeksInSeason = TimeLord.CampaignCalendar.Weeks;
            SeasonsInYear = TimeLord.CampaignCalendar.Seasons;
        }
    }
    public sealed class Hero
    {
        public int BirthdayWrites;
        public CampaignTime BirthDay;
        public void SetBirthDay(CampaignTime value) { BirthDay = value; BirthdayWrites++; }
    }
    public sealed class CharacterObject
    {
        public static CharacterObject PlayerCharacter = new CharacterObject();
        public float Age;
        public Hero HeroObject = new Hero();
    }
    public interface IDataStore { }
    public abstract class CampaignBehaviorBase
    {
        public abstract void RegisterEvents();
        public abstract void SyncData(IDataStore store);
    }
    public sealed class Event<T>
    {
        private Action<T>? _listeners;
        public void AddNonSerializedListener(object owner, Action<T> action) => _listeners += action;
        public void Raise(T value) => _listeners?.Invoke(value);
    }
    public sealed class Event
    {
        private Action? _listeners;
        public void AddNonSerializedListener(object owner, Action action) => _listeners += action;
        public void Raise() => _listeners?.Invoke();
    }
    public static class CampaignEvents
    {
        public static Event<CharacterCreationContent.CharacterCreationManager> OnCharacterCreationInitializedEvent = new Event<CharacterCreationContent.CharacterCreationManager>();
        public static Event<int> OnCharacterCreationIsOverEvent = new Event<int>();
        public static Event OnGameLoadFinishedEvent = new Event();
        public static void Reset()
        {
            OnCharacterCreationInitializedEvent = new Event<CharacterCreationContent.CharacterCreationManager>();
            OnCharacterCreationIsOverEvent = new Event<int>();
            OnGameLoadFinishedEvent = new Event();
            CharacterObject.PlayerCharacter = new CharacterObject();
            TaleWorlds.Core.Game.Current = new TaleWorlds.Core.Game();
        }
    }
}
namespace TaleWorlds.CampaignSystem.CharacterCreationContent
{
    public class CharacterCreationStageBase { }
    public interface ICharacterCreationContentHandler
    {
        void AfterInitializeContent(CharacterCreationManager manager);
        void InitializeContent(CharacterCreationManager manager);
        void OnCharacterCreationFinalize(CharacterCreationManager manager);
        void OnStageCompleted(CharacterCreationStageBase stage);
    }
    public sealed class CharacterCreationManager
    {
        public void RegisterCharacterCreationContentHandler(ICharacterCreationContentHandler handler, int priority) { }
    }
}
