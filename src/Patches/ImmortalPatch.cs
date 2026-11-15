
using HarmonyLib;

using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TimeLord.Patches
{
    public static class ImmortalPatch
    {
        internal static void FixImmortality(this Agent agent)
        {
            try
            {
                if (agent?.Character != null && agent.AgentVisuals != null && Main.Settings != null && agent.IsHuman && agent.Age < 18f && agent.Age >= Main.Settings.HeroComesOfAge)
                {
                    float age = agent.Age;
                    float scale = agent.AgentScale;
                    var setScale = AccessTools.Method(typeof(Agent), "SetInitialAgentScale", new[] { typeof(float) });
                    if (setScale == null) return;
                    try
                    {
                        agent.Age = 18f;

                        SkinGenerationParams skinParams = new SkinGenerationParams(0, agent.SpawnEquipment.GetUnderwearType((!agent.IsFemale ? false : agent.Age >= 14f)), (int) agent.SpawnEquipment.BodyMeshType, (int) agent.SpawnEquipment.HairCoverType, (int) agent.SpawnEquipment.BeardCoverType, (int) agent.SpawnEquipment.BodyDeformType, agent == Agent.Main, agent.Character.FaceDirtAmount, (agent.IsFemale ? 1 : 0), agent.Character.Race, false, false, 0);
                        agent.AgentVisuals.AddSkinMeshes(skinParams, agent.BodyPropertiesValue, true, agent.Character.FaceMeshCache);
                        setScale.Invoke(agent, new object[] { scale });
                    }
                    finally
                    {
                        agent.Age = age;
                    }
                }
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

        internal static void RebuildFromFix(this Agent agent)
        {
            try
            {
                if (agent?.Character != null && agent.AgentVisuals != null && Main.Settings != null && agent.IsHuman && agent.Age < 18f && agent.Age >= Main.Settings.HeroComesOfAge)
                {
                    agent.AgentVisuals.BatchLastLodMeshes();
                    agent.PreloadForRendering();
                    agent.UpdateSpawnEquipmentAndRefreshVisuals(agent.SpawnEquipment);
                }
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
