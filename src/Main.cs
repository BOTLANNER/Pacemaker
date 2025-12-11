using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

                //foreach (var patch in HarmonyPatches)
                //{
                //    Util.Log.ToFile($"Applying: {patch}");
                //    patch.Apply(Harmony);
                //}

                var delayedPatches = new List<Func<Harmony, bool>>();
                foreach (var patch in HarmonyOptionalPatches)
                {
                    Util.Log.ToFile($"Applying: {patch}");
                    if (patch.TryPatch(Harmony))
                    {
                        var delayed = patch.DelayedPatch();
                        if (delayed != null)
                        {
                            delayedPatches.Add(delayed);
                        }
                    }

                }

                Util.Log.ToFile("\nApplying standard Harmony patches in bulk...");
                try
                {
                    Harmony.PatchAll();
                }
                catch (System.Exception ex)
                {
                    Util.Log.NotifyBad("\nTimeLord: Patch all failed, attempting one by one...");
                    var assembly = Assembly.GetExecutingAssembly();
                    var types = AccessTools.GetTypesFromAssembly(assembly);
                    for (int i = 0; i < types.Length; i++)
                    {
                        try
                        {
                            var type = types[i];
                            Harmony.CreateClassProcessor(type).Patch();
                        }
                        catch (Exception e)
                        {
                            if (Debugger.IsAttached)
                            {
                                Debugger.Break();
                            }
                            SetupExceptions.Add(e);
                            Util.Log.NotifyBad($"\nTimeLord: {e.Message}");
                            Debug.PrintError(e.Message, e.StackTrace);
                            Debug.WriteDebugLineOnScreen(e.ToString());
                            Debug.SetCrashReportCustomString(e.Message);
                            Debug.SetCrashReportCustomStack(e.StackTrace);
                        }
                    }

                }

                if (delayedPatches.Count > 0)
                {
                    Util.Log.ToFile("\nStarting counter for delayed Harmony patches...");
                    Task.Run(async () =>
                    {
                        try
                        {
                            // TODO: Why? https://forums.taleworlds.com/index.php?threads/modding-critical-issues-with-agent-handleblow-and-visual-system-changes-in-1-3-5-1-3-6.467766/
                            await Task.Delay(8000); // 8 second delay

                            Util.Log.ToFile("\nForced wait period done for delayed Harmony patches...");

                            // Wait for any active missions to finish
                            int attempts = 0;
                            while (Mission.Current != null && attempts < 50)
                            {
                                await Task.Delay(100);
                                attempts++;
                            }

                            Util.Log.ToFile("\nNo more current Mission for delayed Harmony patches...");

                            for (int i = 0; i < delayedPatches.Count; i++)
                            {
                                try
                                {
                                    Func<Harmony, bool>? delayed = delayedPatches[i];
                                    if (delayed != null)
                                    {
                                        delayed.Invoke(Harmony);
                                    }
                                }
                                catch (Exception e)
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
                        }
                        catch (Exception e)
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

                        DelayedPatchesComplete = true;
                    });

                }
                else
                {
                    DelayedPatchesComplete = true;
                }

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

                    trace.Add("\nLoaded Settings");
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


                if (SetupExceptions.Any())
                {
                    throw new AggregateException("TimeLord: Started with errors", SetupExceptions);
                }
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

        private bool _loaded;
        public static Harmony Harmony;
    }
}
