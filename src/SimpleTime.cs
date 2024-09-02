using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

using TimeLord.Extensions;

namespace TimeLord
{
    internal sealed class SimpleTime
    {
        [SaveableProperty(1)]
        public int Year { get; set; } = 0;

        [SaveableProperty(2)]
        public int Season { get; set; } = 0;

        [SaveableProperty(3)]
        public int Day { get; set; } = 0;

        [SaveableProperty(4)]
        public double FractionalDay { get; set; } = 0;

        public SimpleTime(CampaignTime ct)
        {
            try
            {
                double fracDays = ct.ToDays;

                Year = ct.GetYear;
                fracDays -= Year * Main.TimeParam.DayPerYear;

                Season = (int) ct.GetSeasonOfYear;
                fracDays -= Season * Main.TimeParam.DayPerSeason;

                Day = ct.GetDayOfSeason;
                fracDays -= Day;

                FractionalDay = Math.Min(0.999999, Math.Max(+0.0, fracDays)); // clamp to [+0, 0.999999]
            }
            catch (Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }

        private bool IsSeasonValid => Season >= 0 && Season < TimeParams.SeasonPerYear;

        public override string ToString()
        {
            try
            {
                // only intended for debugging
                var ct = CampaignTimeExtensions.DaysD(FractionalDay);
                var hour = (int) ct.ToHours;
                var min = (int) ct.ToMinutes % TimeParams.MinPerHour;
                var sec = (int) ct.ToSeconds % TimeParams.SecPerMin;
                var season = !IsSeasonValid ? $"[BAD_SEASON: {Season}]" : _seasonNames[Season];

                return $"{season} {Day + 1}, {Year} at {hour:D2}:{min:D2}:{sec:D2} ({(100.0 * FractionalDay):F2}% of the day)";
            }
            catch (Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
                return base.ToString();
            }
        }

        private static readonly string[] _seasonNames = new[] { "Spring", "Summer", "Autumn", "Winter", };
    }
}
