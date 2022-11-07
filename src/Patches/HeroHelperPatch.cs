using System.Runtime.CompilerServices;

using Helpers;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    internal sealed class HeroHelperPatch : Patch
    {
        private static readonly Reflect.Method TargetMethod = new(typeof(HeroHelper), "GetRandomBirthDayForAge");
        private static readonly Reflect.Method<HeroHelperPatch> PatchMethod = new(nameof(GetRandomBirthDayForAge));

        internal HeroHelperPatch() : base(Type.Prefix, TargetMethod, PatchMethod) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool GetRandomBirthDayForAge(float age, ref CampaignTime __result)
        {
            try
            {
                var now = CampaignTime.Now;
                float birthYear = now.GetYear - age;
                float randDayOfYear = MBRandom.RandomFloatRanged(1, Main.TimeParam.DayPerYear);

                if (randDayOfYear > now.GetDayOfYear)
                {
                    --birthYear;
                }

                __result = CampaignTime.Years(birthYear) + CampaignTime.Days(randDayOfYear);
                return false;
            }
            catch (System.Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                return true;
            }
        }
    }
}
