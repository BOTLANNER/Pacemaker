using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

using TimeLord.Patches;

using Debug = TaleWorlds.Library.Debug;

namespace TimeLord
{
    public class Main : MBSubModuleBase
    {
        /* Semantic Versioning (https://semver.org): */
        public static readonly int SemVerMajor = 1;
        public static readonly int SemVerMinor = 2;
        public static readonly int SemVerPatch = 0;
        public static readonly string? SemVerSpecial = null;
        private static readonly string SemVerEnd = (SemVerSpecial is not null) ? "-" + SemVerSpecial : string.Empty;
        public static readonly string Version = $"{SemVerMajor}.{SemVerMinor}.{SemVerPatch}{SemVerEnd}";

        public static readonly string Name = typeof(Main).Namespace;
        public static readonly string DisplayName = Name; // to be shown to humans in-game
        public static readonly string HarmonyDomain = "com.b0tlanner.bannerlord." + Name.ToLower();

        internal static readonly Color ImportantTextColor = Color.FromUint(0x00F16D26); // orange

        internal static Settings? Settings;
        internal static TimeParams TimeParam = new();
        internal static ExternalSavedValues ExternalSavedValues = new(Name);

        private readonly bool EnableTickTracer = false;

        static Main()
        {
            try
            {
                HarmonyPatches = new Patch[]
                {
                    new Patches.DefaultMobilePartyFoodConsumptionModelPatch(),
                    new Patches.DefaultHeroDeathProbabilityCalculationModelPatch(),
                    new Patches.HeroHelperPatch(),
                    new Patches.MapTimeTrackerTickPatch(),
                };

                HarmonyOptionalPatches = new IOptionalPatch[]
                {
                    new FamilyControlSupportPatch(),
                };
            }
            catch (System.Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }

        }

        private static readonly Patch[] HarmonyPatches;

        private static readonly IOptionalPatch[] HarmonyOptionalPatches;

        protected override void OnSubModuleLoad()
        {
            try
            {
                base.OnSubModuleLoad();
                Util.EnableLog = true; // enable various debug logging
                Util.EnableTracer = true; // enable code event tracing (requires enabled logging)

                Util.Log.ToFile("Applying manual Harmony patches...");
                Harmony = new Harmony(HarmonyDomain);

                foreach (var patch in HarmonyPatches)
                {
                    Util.Log.ToFile($"Applying: {patch}");
                    patch.Apply(Harmony);
                }

                foreach (var patch in HarmonyOptionalPatches)
                {
                    Util.Log.ToFile($"Applying: {patch}");
                    patch.TryPatch(Harmony);
                }

                Util.Log.ToFile("\nApplying standard Harmony patches in bulk...");
                Harmony.PatchAll();
                Util.Log.ToFile("Done.");
            }
            catch (System.Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            try
            {
                var trace = new List<string>();

                if (_loaded)
                {
                    trace.Add("\nModule was already loaded.");
                }
                else
                {
                    trace.Add("\nModule is loading for the first time...");
                }

                if (Settings.Instance is not null && Settings.Instance != Settings)
                {
                    Settings = Settings.Instance;

                    // register for settings property-changed events
                    Settings.PropertyChanged += Settings_OnPropertyChanged;

                    trace.Add("\nLoaded Settings:");
                    trace.AddRange(Settings.ToStringLines(indentSize: 4));
                    trace.Add(string.Empty);

                    SetTimeParams(new TimeParams(Settings.DaysPerSeason), trace);
                }

                if (!_loaded)
                {
                    InformationManager.DisplayMessage(new InformationMessage($"Loaded {DisplayName}", ImportantTextColor));
                    _loaded = true;


                    foreach (var patch in HarmonyOptionalPatches)
                    {
                        patch.MenusInitialised(Harmony);
                    }
                }

                Util.Log.ToFile(trace);
            }
            catch (System.Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }
        }

        protected override void OnGameStart(Game game, IGameStarter starterObject)
        {
            try
            {
                base.OnGameStart(game, starterObject);
                var trace = new List<string>();

                if (game.GameType is Campaign)
                {
                    var initializer = (CampaignGameStarter) starterObject;
                    AddBehaviors(initializer, trace);
                }

                Util.EventTracer.Trace(trace);
            }
            catch (System.Exception e) { TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  }
        }

        private void AddBehaviors(CampaignGameStarter gameInitializer, List<string> trace)
        {
            try
            {
                gameInitializer.AddBehavior(new SaveBehavior());
                trace.Add($"Behavior added: {typeof(SaveBehavior).FullName}");

                if (EnableTickTracer && Util.EnableTracer && Util.EnableLog)
                {
                    gameInitializer.AddBehavior(new TickTraceBehavior());
                    trace.Add($"Behavior added: {typeof(TickTraceBehavior).FullName}");
                }
            }
            catch (System.Exception e) { TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  }
        }

        internal static TimeParams SetTimeParams(TimeParams newParams, List<string> trace)
        {
            try
            {
                trace.Add($"Setting time parameters for {newParams.DayPerSeason} days/season...");

                var oldParams = TimeParam;
                TimeParam = newParams;

                trace.Add(string.Empty);
                trace.AddRange(TimeParam.ToStringLines(indentSize: 4));

                return oldParams;
            }
            catch (System.Exception e)
            {
                TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                return TimeParam;
            }
        }

        protected static void Settings_OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            try
            {
                if (sender is Settings settings && args.PropertyName == Settings.SaveTriggered)
                {
                    var trace = new List<string> { "Received save-triggered event from Settings..." };
                    trace.Add(string.Empty);
                    trace.Add("New Settings:");
                    trace.AddRange(settings.ToStringLines(indentSize: 4));
                    trace.Add(string.Empty);
                    SetTimeParams(new TimeParams(settings.DaysPerSeason), trace);
                    Util.EventTracer.Trace(trace);
                }
            }
            catch (System.Exception e) { TaleWorlds.Library.Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  }
        }

        private bool _loaded;
        public static Harmony Harmony;
    }
}
