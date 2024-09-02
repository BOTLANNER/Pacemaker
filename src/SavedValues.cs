using System.Text;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace TimeLord
{
    internal sealed class SavedValues
    {
        [SaveableProperty(1)]
        public int DaysPerSeason { get; set; }

        [SaveableProperty(3)]
        public float PregnancyDuration { get; set; }

        public SavedValues() { }

        internal void Snapshot()
        {
            try
            {
                if (DaysPerSeason == default) // Only set this upon first save
                {
                    DaysPerSeason = Main.TimeParam.DayPerSeason;
                }

                PregnancyDuration = Campaign.Current.Models.PregnancyModel.PregnancyDurationInDays;

                Main.ExternalSavedValues.Set(Hero.MainHero.Name.ToString(), Clan.PlayerClan.Name.ToString(), this);
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

        public override string ToString()
        {
            try
            {
                StringBuilder builder = new("{\n");
                builder.AppendFormat("  {0} = {1}\n", nameof(DaysPerSeason), DaysPerSeason);
                builder.AppendFormat("  {0} = {1}\n", nameof(PregnancyDuration), PregnancyDuration);
                builder.Append("}");
                return builder.ToString();
            }
            catch (System.Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
                return base.ToString();
            }
        }
    }
}
