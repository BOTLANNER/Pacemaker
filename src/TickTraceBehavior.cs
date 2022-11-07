using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace TimeLord
{
    internal sealed class TickTraceBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
        }

        public override void SyncData(IDataStore dataStore)	{ }

        public void OnWeeklyTick() => Util.EventTracer.Trace();

        public void OnDailyTick() => Util.EventTracer.Trace();

        public void OnDailyTickClan(Clan clan)
        {
            try
            {
                if (clan != Clan.PlayerClan)
                {
                    return;
                }

                Util.EventTracer.Trace($"Fired for Player Clan: {clan.Name}");
            }
            catch (System.Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }
        }

        public void OnHourlyTick() => Util.EventTracer.Trace();
    }
}
