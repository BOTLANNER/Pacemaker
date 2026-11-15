using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace TimeLord
{
    // Session-only snapshot: no save schema changes or reinterpretation of saved timestamps.
    [HarmonyPatch(typeof(CampaignTime), nameof(CampaignTime.Initialize))]
    internal static class CampaignCalendar
    {
        private static Campaign? _campaign;
        private static int _days, _weeks, _seasons;

        [HarmonyPrefix]
        private static void Prefix()
        {
            if (Campaign.Current == null || Main.Settings == null || ReferenceEquals(_campaign, Campaign.Current)) return;
            _days = Main.Settings.DaysInWeek;
            _weeks = Main.Settings.WeeksInSeason;
            _seasons = Main.Settings.SeasonsInYear;
            _campaign = Campaign.Current;
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            // Respect the calendar actually selected by the model/other patches at initialization.
            if (Campaign.Current == null) return;
            _campaign = Campaign.Current;
            _days = CampaignTime.DaysInWeek;
            _weeks = CampaignTime.WeeksInSeason;
            _seasons = CampaignTime.SeasonsInYear;
        }

        internal static int Days => ReferenceEquals(_campaign, Campaign.Current) && _campaign != null ? _days : Main.Settings?.DaysInWeek ?? 7;
        internal static int Weeks => ReferenceEquals(_campaign, Campaign.Current) && _campaign != null ? _weeks : Main.Settings?.WeeksInSeason ?? 3;
        internal static int Seasons => ReferenceEquals(_campaign, Campaign.Current) && _campaign != null ? _seasons : Main.Settings?.SeasonsInYear ?? 4;

        internal static void Reset() { _campaign = null; }
    }
}
