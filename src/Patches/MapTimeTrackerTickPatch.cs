using System.Runtime.CompilerServices;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    internal sealed class MapTimeTrackerTickPatch : Patch
    {
        private static readonly System.Type MapTimeTrackerT = typeof(Campaign).Assembly.GetType("TaleWorlds.CampaignSystem.MapTimeTracker");
        private static readonly Reflect.Method TargetRM = new(MapTimeTrackerT, "Tick");
        private static readonly Reflect.Method<MapTimeTrackerTickPatch> PatchRM = new(nameof(TickPrefix));

        internal MapTimeTrackerTickPatch() : base(Type.Prefix, TargetRM, PatchRM) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TickPrefix(ref float seconds)
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
    }
}
