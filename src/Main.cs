using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;


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
        public static readonly int SemVerPatch = 2;
        public static readonly string? SemVerSpecial = null;
        private static readonly string SemVerEnd = (SemVerSpecial is not null) ? "-" + SemVerSpecial : string.Empty;
        public static readonly string Version = $"{SemVerMajor}.{SemVerMinor}.{SemVerPatch}{SemVerEnd}";

        public static readonly string Name = typeof(Main).Namespace;
        public static readonly string DisplayName = Name; // to be shown to humans in-game
        public static readonly string HarmonyDomain = "com.b0tlanner.bannerlord." + Name.ToLower();

        internal static readonly Color ImportantTextColor = Color.FromUint(0x00F16D26); // orange

        internal static Settings? Settings;

        private readonly bool EnableTickTracer = false;

        private List<Exception> SetupExceptions = new List<Exception>();

        public static bool DelayedPatchesComplete = false;

        static Main()
        {
            try
            {
                //HarmonyPatches = new Patch[]
                //{
                //    //new Patches.HeroHelperPatch(),
                //};

                HarmonyOptionalPatches = new IOptionalPatch[]
                {
                    new MapTimeTrackerPatch(),
                    new FamilyControlSupportPatch(),
                    new ForcePregnancyModelPatch(),
                    new ForceCampaignTimeModelPatch(),

                    // Delayed
                    new AgentAndMissionRuntimePatch()
                };
            }
            catch (System.Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }

        }

        private static readonly IOptionalPatch[] HarmonyOptionalPatches = Array.Empty<IOptionalPatch>();

        protected override void OnSubModuleLoad()
        {
            try
            {
                base.OnSubModuleLoad();
                Util.EnableLog = true; // enable various debug logging
                Util.EnableTracer = true; // enable code event tracing (requires enabled logging)

                Util.Log.ToFile("Applying manual Harmony patches...");
                Harmony = new Harmony(HarmonyDomain);

                //foreach (var patch in HarmonyPatches)
                //{
                //    Util.Log.ToFile($"Applying: {patch}");
                //    patch.Apply(Harmony);
                //}

                foreach (var patch in HarmonyOptionalPatches)
                {
                    try
                    {
                        if (patch.TryPatch(Harmony))
                        {
                            var delayed = patch.DelayedPatch();
                            if (delayed != null) _delayedPatches.Add(delayed);
                        }
                    }
                    catch (Exception e)
                    {
                        PatchSafety.Rollback(Harmony, patch.GetType());
                        ReportSetupFailure(e);
                    }
                }

                // Process each class once; do not retry a partially successful PatchAll.
                foreach (var type in PatchSafety.LoadableTypes(Assembly.GetExecutingAssembly()))
                {
                    if (!type.IsDefined(typeof(HarmonyPatch), false)) continue;
                    try { Harmony.CreateClassProcessor(type).Patch(); }
                    catch (Exception e)
                    {
                        PatchSafety.Rollback(Harmony, type);
                        ReportSetupFailure(e);
                    }
                }
                DelayedPatchesComplete = _delayedPatches.Count == 0 && !SetupExceptions.Any();
                Util.Log.ToFile("Done.");
            }
            catch (System.Exception e)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            try
            {
                bool firstLoad = !_loaded;
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
                    if (Settings != null) Settings.PropertyChanged -= Settings_OnPropertyChanged;
                    Settings = Settings.Instance;

                    // register for settings property-changed events
                    Settings.PropertyChanged += Settings_OnPropertyChanged;

                    trace.Add("\nLoaded Settings");
                }

                if (!_loaded)
                {
                    if (Settings == null) throw new InvalidOperationException("TimeLord: MCM settings unavailable; cannot initialize.");
                    _loaded = true;

                    foreach (var patch in HarmonyOptionalPatches)
                    {
                        try
                        {
                            if (!patch.MenusInitialised(Harmony))
                                ReportSetupFailure(new InvalidOperationException($"TimeLord: {patch.GetType().Name} did not initialize."));
                        }
                        catch (Exception e) { ReportSetupFailure(e); }
                    }
                }

                ApplyDelayedPatches();
                Util.Log.ToFile(trace);


                if (SetupExceptions.Any())
                {
                    throw new AggregateException("TimeLord: Started with errors", SetupExceptions);
                }
                if (firstLoad && DelayedPatchesComplete)
                    InformationManager.DisplayMessage(new InformationMessage($"Loaded {DisplayName}", ImportantTextColor));
            }
            catch (System.Exception e)
            {
                TimeLord.Util.Log.ToFile(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);

                TimeLord.Util.Log.NotifyBad(e.Message);
                Debug.WriteDebugLineOnScreen(e.Message);
            }
        }

        protected override void OnGameStart(Game game, IGameStarter starterObject)
        {
            try
            {
                base.OnGameStart(game, starterObject);
                var trace = new List<string>();

                if (game.GameType is Campaign && starterObject is CampaignGameStarter initializer)
                {
                    //var initializer = (CampaignGameStarter) starterObject;
                    AddBehaviors(initializer, trace);
                }

                Util.EventTracer.Trace(trace);
            }
            catch (System.Exception e) { TimeLord.Util.Log.NotifyBad(e.ToString()); Debug.PrintError(e.Message, e.StackTrace); Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString()); Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); }
        }

        private void AddBehaviors(CampaignGameStarter gameInitializer, List<string> trace)
        {
            try
            {
                gameInitializer.AddBehavior(new PostCharacterCreationBehaviour());
                trace.Add($"Behavior added: {typeof(PostCharacterCreationBehaviour).FullName}");

                if (EnableTickTracer && Util.EnableTracer && Util.EnableLog)
                {
                    gameInitializer.AddBehavior(new TickTraceBehavior());
                    trace.Add($"Behavior added: {typeof(TickTraceBehavior).FullName}");
                }
            }
            catch (System.Exception e) { TimeLord.Util.Log.NotifyBad(e.ToString()); Debug.PrintError(e.Message, e.StackTrace); Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString()); Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); }
        }

        protected static void Settings_OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            try
            {
                if (sender is Settings settings && args.PropertyName == Settings.SaveTriggered)
                {
                    var trace = new List<string> { "Received save-triggered event from Settings..." };
                    trace.Add(string.Empty);
                    trace.Add("New Settings");
                    Util.EventTracer.Trace(trace);
                }
            }
            catch (System.Exception e) { TimeLord.Util.Log.NotifyBad(e.ToString()); Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString()); Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); }
        }

        private readonly List<Func<Harmony, bool>> _delayedPatches = new List<Func<Harmony, bool>>();

        private void ReportSetupFailure(Exception error)
        {
            SetupExceptions.Add(error);
            Util.Log.NotifyBad($"TimeLord compatibility error: {error}");
        }

        // Menu readiness replaces an arbitrary worker-thread delay. Never patch a live mission.
        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            ApplyDelayedPatches();
        }

        private void ApplyDelayedPatches()
        {
            if (!_loaded || Mission.Current != null || _delayedPatches.Count == 0) return;
            bool succeeded = true;
            foreach (var delayed in _delayedPatches)
            {
                try
                {
                    if (!delayed(Harmony)) throw new InvalidOperationException("TimeLord: delayed mission patch failed.");
                }
                catch (Exception e) { succeeded = false; ReportSetupFailure(e); }
            }
            _delayedPatches.Clear(); // Report once; do not retry an uncertain native patch.
            DelayedPatchesComplete = succeeded;
        }

        public override void OnGameEnd(Game game)
        {
            CampaignCalendar.Reset();
            base.OnGameEnd(game);
        }
        private bool _loaded;
        public static Harmony Harmony = null!;
    }
}
