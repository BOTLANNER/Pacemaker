
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
                if (agent.IsHuman && agent.Age < 18f && agent.Age >= Main.Settings!.HeroComesOfAge)
                {
                    float age = agent.Age;
                    float scale = agent.AgentScale;
                    agent.Age = 18f;
                    SkinGenerationParams skinParams = new SkinGenerationParams(0, agent.SpawnEquipment.GetUnderwearType((!agent.IsFemale ? false : agent.Age >= 14f)), (int) agent.SpawnEquipment.BodyMeshType, (int) agent.SpawnEquipment.HairCoverType, (int) agent.SpawnEquipment.BeardCoverType, (int) agent.SpawnEquipment.BodyDeformType, agent == Agent.Main, agent.Character.FaceDirtAmount, (agent.IsFemale ? 1 : 0), agent.Character.Race, false, false);
                    agent.AgentVisuals.AddSkinMeshes(skinParams, agent.BodyPropertiesValue, true, (agent.Character == null ? false : agent.Character.FaceMeshCache));
                    AccessTools.Method(typeof(Agent), "SetInitialAgentScale", null, null).Invoke(agent, new object[] { scale });
                    agent.Age = age;
                }
            }
            catch (System.Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }
        }

        internal static void RebuildFromFix(this Agent agent)
        {
            try
            {
                if (agent.IsHuman && agent.Age < 18f && agent.Age >= Main.Settings!.HeroComesOfAge)
                {
                    agent.AgentVisuals.BatchLastLodMeshes();
                    agent.PreloadForRendering();
                    agent.UpdateSpawnEquipmentAndRefreshVisuals(agent.SpawnEquipment);
                }
            }
            catch (System.Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
            }
        }
    }
}
