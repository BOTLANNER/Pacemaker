using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    // TODO: Determine if needed? TickMapTime already increases speed...
    public sealed class MapTimeTrackerPatch : IOptionalPatch
    {
        private static readonly System.Type MapTimeTrackerType = typeof(Campaign).Assembly.GetType("TaleWorlds.CampaignSystem.MapTimeTracker");
        private static readonly MethodInfo TickMethod = AccessTools.Method(MapTimeTrackerType, nameof(Tick));

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Tick(ref float seconds)
        {
            try
            {
                seconds *= Main.Settings!.TimeMultiplier;
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

        public bool MenusInitialised(Harmony harmony)
        {
            return true;
        }

        public Func<Harmony, bool>? DelayedPatch()
        {
            return null;
        }

        public bool TryPatch(Harmony harmony)
        {
            try
            {
                harmony.Patch(TickMethod, prefix: new HarmonyMethod(this.GetType(), nameof(Tick)));
                return true;
            }
            catch(Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
                throw e;
            }
        }
    }
}
