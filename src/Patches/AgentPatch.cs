
using HarmonyLib;

using System.Reflection;

using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(Agent))]
    public static class AgentPatch
    {
        static MethodInfo SpawnEquipmentSetMethod = typeof(Agent).GetProperty("SpawnEquipment", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetMethod;
        private static void SetSpawnEquipment(this Agent __instance, Equipment newSpawnEquipment)
        {
            SpawnEquipmentSetMethod.Invoke(__instance, new object[] { newSpawnEquipment });
        }

        [HarmonyPrefix]
        [HarmonyPatch("UpdateSpawnEquipmentAndRefreshVisuals")]
        public static bool UpdateSpawnEquipmentAndRefreshVisuals(ref Agent __instance,Equipment newSpawnEquipment)
        {
            try
            {
                Banner banner;
                __instance.SetSpawnEquipment(newSpawnEquipment);
                __instance.AgentVisuals.ClearVisualComponents(false);
                __instance.Mission.OnEquipItemsFromSpawnEquipment(__instance, Agent.CreationType.FromCharacterObj);
                __instance.AgentVisuals.ClearAllWeaponMeshes();
                MissionEquipment equipment = __instance.Equipment;
                Equipment spawnEquipment = __instance.SpawnEquipment;
                IAgentOriginBase origin = __instance.Origin;
                if (origin != null)
                {
                    banner = origin.Banner;
                }
                else
                {
                    banner = null;
                }
                equipment.FillFrom(spawnEquipment, banner);
                __instance.CheckEquipmentForCapeClothSimulationStateChange();
                __instance.EquipItemsFromSpawnEquipment(true);
                __instance.UpdateAgentProperties();
                __instance.WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp);
                __instance.PreloadForRendering();


                __instance.FixImmortality();

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
