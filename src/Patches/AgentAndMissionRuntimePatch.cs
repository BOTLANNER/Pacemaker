using System;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

using Debugger = System.Diagnostics.Debugger;

namespace TimeLord.Patches
{
    public class AgentAndMissionRuntimePatch : IOptionalPatch
    {
        // Mission
        static MethodInfo BuildAgentMethod = AccessTools.Method(typeof(Mission), nameof(BuildAgent));

        // Agent
        static MethodInfo UpdateSpawnEquipmentAndRefreshVisualsMethod = AccessTools.Method(typeof(Agent), nameof(UpdateSpawnEquipmentAndRefreshVisuals));

        public Func<Harmony, bool>? DelayedPatch()
        {
            return (harmony) =>
            {
                // TODO: Actual patch
                try
                {
                    harmony.Patch(BuildAgentMethod, postfix: new HarmonyMethod(typeof(AgentAndMissionRuntimePatch), nameof(BuildAgent)));
                    harmony.Patch(UpdateSpawnEquipmentAndRefreshVisualsMethod, postfix: new HarmonyMethod(typeof(AgentAndMissionRuntimePatch), nameof(UpdateSpawnEquipmentAndRefreshVisuals)));

                    return true;
                }
                catch (System.Exception e)
                {
                    Util.EventTracer.Trace(new List<string> { "UpdateSpawnEquipmentAndRefreshVisuals ", e.Message, e.StackTrace });
                    TimeLord.Util.Log.NotifyBad(e.ToString());
                    Debug.PrintError(e.Message, e.StackTrace);
                    Debug.WriteDebugLineOnScreen(e.ToString());
                    Debug.SetCrashReportCustomString(e.Message);
                    Debug.SetCrashReportCustomStack(e.StackTrace);

                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
                return false;
            };
        }

        public bool MenusInitialised(Harmony harmony)
        {
            return true;
        }

        public bool TryPatch(Harmony harmony)
        {
            if (BuildAgentMethod == null)
            {
                return false;
            }
            if (UpdateSpawnEquipmentAndRefreshVisualsMethod == null)
            {
                return false;
            }
            return true;
        }

        public static void UpdateSpawnEquipmentAndRefreshVisuals(ref Agent __instance)
        {
            try
            {
                Util.EventTracer.Trace("UpdateSpawnEquipmentAndRefreshVisuals");
                __instance.FixImmortality();
            }
            catch (System.Exception e)
            {
                Util.EventTracer.Trace(new List<string> { "UpdateSpawnEquipmentAndRefreshVisuals ", e.Message, e.StackTrace });
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        private static void BuildAgent(ref Mission __instance, ref int ____agentCreationIndex, ref List<Agent> ____activeAgents, ref List<Agent> ____allAgents, Agent agent, AgentBuildData agentBuildData)
        {
            try
            {
                Util.EventTracer.Trace("BuildAgent");
                agent.FixImmortality();
                agent.RebuildFromFix();
            }
            catch (Exception e)
            {
                Util.EventTracer.Trace(new List<string> { "BuildAgent ", e.Message, e.StackTrace });
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
            }
        }
    }
}