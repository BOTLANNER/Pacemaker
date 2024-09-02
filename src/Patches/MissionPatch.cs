
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using NetworkMessages.FromServer;

using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

using Debugger = System.Diagnostics.Debugger;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(Mission))]
    public static class MissionPatch
    {
        static readonly MethodInfo CreateAgentMethod;
        static readonly MethodInfo BuildAgentMethod;
        static readonly MethodInfo CreateHorseAgentFromRosterElementsMethod;
        static readonly MethodInfo BodyPropertiesSeedSetMethod;
        static readonly MethodInfo SetMountAgentBeforeBuildMethod;
        static readonly MethodInfo BuildMethod;
        static readonly MethodInfo SetInitialAgentScaleMethod;
        static readonly MethodInfo InitializeAgentRecordMethod;
        static readonly MethodInfo InitializeComponentsMethod;

        static MissionPatch()
        {
            try
            {
                CreateAgentMethod = typeof(Mission).GetMethod("CreateAgent", BindingFlags.Instance | BindingFlags.NonPublic);
                BuildAgentMethod = typeof(Mission).GetMethod("BuildAgent", BindingFlags.Instance | BindingFlags.NonPublic);
                CreateHorseAgentFromRosterElementsMethod = typeof(Mission).GetMethod("CreateHorseAgentFromRosterElements", BindingFlags.Instance | BindingFlags.NonPublic);
                BodyPropertiesSeedSetMethod = typeof(Agent).GetProperty("BodyPropertiesSeed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetMethod;
                SetMountAgentBeforeBuildMethod = typeof(Agent).GetMethod("SetMountAgentBeforeBuild", BindingFlags.Instance | BindingFlags.NonPublic);
                BuildMethod = typeof(Agent).GetMethod("Build", BindingFlags.Instance | BindingFlags.NonPublic);
                SetInitialAgentScaleMethod = typeof(Agent).GetMethod("SetInitialAgentScale", BindingFlags.Instance | BindingFlags.NonPublic);
                InitializeAgentRecordMethod = typeof(Agent).GetMethod("InitializeAgentRecord", BindingFlags.Instance | BindingFlags.NonPublic);
                InitializeComponentsMethod = typeof(Agent).GetMethod("InitializeComponents", BindingFlags.Instance | BindingFlags.NonPublic);
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

        private static Agent CreateAgent(this Mission __instance, Monster monster, bool isFemale, int instanceNo, Agent.CreationType creationType, float stepSize, int forcedAgentIndex, int weight, BasicCharacterObject characterObject)
        {
            return (Agent) CreateAgentMethod.Invoke(__instance, new object[] { monster, isFemale, instanceNo, creationType, stepSize, forcedAgentIndex, weight, characterObject });
        }
        private static void BuildAgent(this Mission __instance, Agent agent, AgentBuildData agentBuildData)
        {
            BuildAgentMethod.Invoke(__instance, new object[] { agent, agentBuildData });
        }

        private static Agent CreateHorseAgentFromRosterElements(this Mission __instance, EquipmentElement mount, EquipmentElement mountHarness, ref Vec3 initialPosition, ref Vec2 initialDirection, int forcedAgentMountIndex, string horseCreationKey)
        {
            object[] parameters = new object[] { mount, mountHarness, initialPosition, initialDirection, forcedAgentMountIndex, horseCreationKey };
            var agent = (Agent) CreateHorseAgentFromRosterElementsMethod.Invoke(__instance, parameters);
            initialPosition = (Vec3) parameters[2];
            initialDirection = (Vec2) parameters[3];

            return agent;

        }


        internal static void Build(this Agent __instance, AgentBuildData agentBuildData, int creationIndex)
        {
            BuildMethod.Invoke(__instance, new object[] { agentBuildData, creationIndex });
        }
        internal static void InitializeAgentRecord(this Agent __instance)
        {
            InitializeAgentRecordMethod.Invoke(__instance, null);
        }
        internal static void InitializeComponents(this Agent __instance)
        {
            InitializeComponentsMethod.Invoke(__instance, null);
        }

        internal static void SetInitialAgentScale(this Agent __instance, float i)
        {
            SetInitialAgentScaleMethod.Invoke(__instance, new object[] { i });
        }

        private static void SetMountAgentBeforeBuild(this Agent __instance, Agent mount)
        {
            SetMountAgentBeforeBuildMethod.Invoke(__instance, new object[] { mount });
        }
        private static void SetBodyPropertiesSeed(this Agent __instance, int value)
        {
            BodyPropertiesSeedSetMethod.Invoke(__instance, new object[] { value });
        }

        [HarmonyPrefix]
        [HarmonyPatch("SpawnAgent")]


        public static bool SpawnAgent(ref Mission __instance, ref Agent __result, AgentBuildData agentBuildData, bool spawnFromAgentVisuals = false)
        {
            try
            {
                Equipment equipmentElement;
                WorldPosition worldPosition;
                Vec2 vec2;
                int countOfUnits;
                int agentFormationTroopSpawnCount;
                WorldPosition worldPosition1;
                EquipmentElement item;
                NetworkCommunicator networkPeer;
                int teamIndex;
                int index;
                int num;
                Equipment spawnEquipment;
                BasicCharacterObject agentCharacter = agentBuildData.AgentCharacter;
                if (agentCharacter == null)
                {
                    throw new MBNullParameterException("npcCharacterObject");
                }
                int agentIndex = -1;
                if (agentBuildData.AgentIndexOverriden)
                {
                    agentIndex = agentBuildData.AgentIndex;
                }
                Agent formationPositionPreference = __instance.CreateAgent(agentBuildData.AgentMonster, (agentBuildData.GenderOverriden ? agentBuildData.AgentIsFemale : agentCharacter.IsFemale), 0, Agent.CreationType.FromCharacterObj, agentCharacter.GetStepSize(), agentIndex, agentBuildData.AgentMonster.Weight, agentCharacter);
                formationPositionPreference.FormationPositionPreference = agentCharacter.FormationPositionPreference;
                float single = (agentBuildData.AgeOverriden ? (float) agentBuildData.AgentAge : agentCharacter.Age);
                if (single == 0f)
                {
                    agentBuildData.Age(29);
                }
                else if (MBBodyProperties.GetMaturityType(single) < BodyMeshMaturityType.Teenager && (__instance.Mode == MissionMode.Battle || __instance.Mode == MissionMode.Duel || __instance.Mode == MissionMode.Tournament || __instance.Mode == MissionMode.Stealth))
                {
                    agentBuildData.Age(27);
                }
                if (agentBuildData.BodyPropertiesOverriden)
                {
                    formationPositionPreference.UpdateBodyProperties(agentBuildData.AgentBodyProperties);
                    if (!agentBuildData.AgeOverriden)
                    {
                        formationPositionPreference.Age = agentCharacter.Age;
                    }
                }
                formationPositionPreference.SetBodyPropertiesSeed(agentBuildData.AgentEquipmentSeed);
                if (agentBuildData.AgeOverriden)
                {
                    formationPositionPreference.Age = (float) agentBuildData.AgentAge;
                }
                if (agentBuildData.GenderOverriden)
                {
                    formationPositionPreference.IsFemale = agentBuildData.AgentIsFemale;
                }
                formationPositionPreference.SetTeam(agentBuildData.AgentTeam, false);
                formationPositionPreference.SetClothingColor1(agentBuildData.AgentClothingColor1);
                formationPositionPreference.SetClothingColor2(agentBuildData.AgentClothingColor2);
                formationPositionPreference.SetRandomizeColors(agentBuildData.RandomizeColors);
                formationPositionPreference.Origin = agentBuildData.AgentOrigin;
                Formation agentFormation = agentBuildData.AgentFormation;
                if (agentFormation != null && !agentFormation.HasBeenPositioned)
                {
                    __instance.SetFormationPositioningFromDeploymentPlan(agentFormation);
                }

                MissionDeploymentPlan missionDeploymentPlan = (__instance.DeploymentPlan as MissionDeploymentPlan);
                if (!agentBuildData.AgentInitialPosition.HasValue)
                {
                    BattleSideEnum side = agentBuildData.AgentTeam.Side;
                    Vec3 invalid = Vec3.Invalid;
                    Vec2 invalid1 = Vec2.Invalid;
                    if (agentCharacter == Game.Current.PlayerTroop && missionDeploymentPlan.HasPlayerSpawnFrame(side))
                    {
                        missionDeploymentPlan.GetPlayerSpawnFrame(side, out worldPosition, out vec2);
                        invalid = worldPosition.GetGroundVec3();
                        invalid1 = vec2;
                    }
                    else if (agentFormation == null)
                    {
                        __instance.GetFormationSpawnFrame(side, FormationClass.NumberOfAllFormations, agentBuildData.AgentIsReinforcement, out worldPosition1, out invalid1);
                        invalid = worldPosition1.GetGroundVec3();
                    }
                    else
                    {
                        if (agentBuildData.AgentSpawnsIntoOwnFormation)
                        {
                            countOfUnits = agentFormation.CountOfUnits;
                            agentFormationTroopSpawnCount = countOfUnits + 1;
                        }
                        else if (agentBuildData.AgentFormationTroopSpawnIndex < 0 || agentBuildData.AgentFormationTroopSpawnCount <= 0)
                        {
                            countOfUnits = agentFormation.GetNextSpawnIndex();
                            agentFormationTroopSpawnCount = countOfUnits + 1;
                        }
                        else
                        {
                            countOfUnits = agentBuildData.AgentFormationTroopSpawnIndex;
                            agentFormationTroopSpawnCount = agentBuildData.AgentFormationTroopSpawnCount;
                        }
                        if (countOfUnits >= agentFormationTroopSpawnCount)
                        {
                            agentFormationTroopSpawnCount = countOfUnits + 1;
                        }
                        __instance.GetTroopSpawnFrameWithIndex(agentBuildData, countOfUnits, agentFormationTroopSpawnCount, out invalid, out invalid1);
                    }
                    agentBuildData.InitialPosition(invalid).InitialDirection(invalid1);
                }
                Vec3 valueOrDefault = agentBuildData.AgentInitialPosition.GetValueOrDefault();
                Vec2 valueOrDefault1 = agentBuildData.AgentInitialDirection.GetValueOrDefault();
                formationPositionPreference.SetInitialFrame(valueOrDefault, valueOrDefault1, agentBuildData.AgentCanSpawnOutsideOfMissionBoundary);
                if (agentCharacter.AllEquipments == null)
                {
                    Debug.Print(String.Concat("characterObject.AllEquipments is null for \"", agentCharacter.StringId, "\"."), 0, Debug.DebugColor.White, 17592186044416L);
                }
                if (agentCharacter.AllEquipments != null)
                {
                    if (agentCharacter.AllEquipments.Any<Equipment>((Equipment eq) => eq == null))
                    {
                        Debug.Print(String.Concat("Character with id \"", agentCharacter.StringId, "\" has a null equipment in its AllEquipments."), 0, Debug.DebugColor.White, 17592186044416L);
                    }
                }
                var query = (from eq in agentCharacter.AllEquipments
                             where eq.IsCivilian
                             select eq == null);
#if DEBUG
                if (Debugger.IsAttached)
                {
                    query = query.ToList();
                }
#endif
                if (query.All(b => b))
                {
                    agentBuildData.CivilianEquipment(false);
                }
                if (agentCharacter.IsHero)
                {
                    agentBuildData.FixedEquipment(true);
                }
                if (agentBuildData.AgentOverridenSpawnEquipment == null)
                {
                    equipmentElement = (agentBuildData.AgentFixedEquipment ? agentCharacter.GetFirstEquipment(agentBuildData.AgentCivilianEquipment).Clone(false) : Equipment.GetRandomEquipmentElements(formationPositionPreference.Character, !Game.Current.GameType.IsCoreOnlyGameMode, agentBuildData.AgentCivilianEquipment, agentBuildData.AgentEquipmentSeed));
                }
                else
                {
                    equipmentElement = agentBuildData.AgentOverridenSpawnEquipment.Clone(false);
                }
                Agent agent = null;
                if (agentBuildData.AgentNoHorses)
                {
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.ArmorItemEndSlot] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.HorseHarness] = item;
                }
                if (agentBuildData.AgentNoWeapons)
                {
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.WeaponItemBeginSlot] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Weapon1] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Weapon2] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Weapon3] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.ExtraWeaponSlot] = item;
                }
                if (agentCharacter.IsHero)
                {
                    ItemObject agentBannerItem = null;
                    item = equipmentElement[EquipmentIndex.ExtraWeaponSlot];
                    ItemObject itemObject = item.Item;
                    if (itemObject != null && itemObject.IsBannerItem && itemObject.BannerComponent != null)
                    {
                        agentBannerItem = itemObject;
                        item = new EquipmentElement();
                        equipmentElement[EquipmentIndex.ExtraWeaponSlot] = item;
                    }
                    else if (agentBuildData.AgentBannerItem != null)
                    {
                        agentBannerItem = agentBuildData.AgentBannerItem;
                    }
                    if (agentBannerItem != null)
                    {
                        formationPositionPreference.SetFormationBanner(agentBannerItem);
                    }
                }
                else if (agentBuildData.AgentBannerItem != null)
                {
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Weapon1] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Weapon2] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Weapon3] = item;
                    if (agentBuildData.AgentBannerReplacementWeaponItem == null)
                    {
                        item = new EquipmentElement();
                        equipmentElement[EquipmentIndex.WeaponItemBeginSlot] = item;
                    }
                    else
                    {
                        equipmentElement[EquipmentIndex.WeaponItemBeginSlot] = new EquipmentElement(agentBuildData.AgentBannerReplacementWeaponItem, null, null, false);
                    }
                    equipmentElement[EquipmentIndex.ExtraWeaponSlot] = new EquipmentElement(agentBuildData.AgentBannerItem, null, null, false);
                    if (agentBuildData.AgentOverridenSpawnMissionEquipment != null)
                    {
                        agentBuildData.AgentOverridenSpawnMissionEquipment[EquipmentIndex.ExtraWeaponSlot] = new MissionWeapon(agentBuildData.AgentBannerItem, null, agentBuildData.AgentBanner);
                    }
                }
                if (agentBuildData.AgentNoArmor)
                {
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Gloves] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Body] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Cape] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.NumAllWeaponSlots] = item;
                    item = new EquipmentElement();
                    equipmentElement[EquipmentIndex.Leg] = item;
                }
                for (int i = 0; i < 5; i++)
                {
                    item = equipmentElement[(EquipmentIndex) i];
                    if (!item.IsEmpty)
                    {
                        item = equipmentElement[(EquipmentIndex) i];
                        if (item.Item.ItemFlags.HasAnyFlag<ItemFlags>(ItemFlags.CannotBePickedUp))
                        {
                            item = new EquipmentElement();
                            equipmentElement[(EquipmentIndex) i] = item;
                        }
                    }
                }
                formationPositionPreference.InitializeSpawnEquipment(equipmentElement);
                formationPositionPreference.InitializeMissionEquipment(agentBuildData.AgentOverridenSpawnMissionEquipment, agentBuildData.AgentBanner);
                if (formationPositionPreference.RandomizeColors)
                {
                    formationPositionPreference.Equipment.SetGlossMultipliersOfWeaponsRandomly(agentBuildData.AgentEquipmentSeed);
                }
                item = equipmentElement[EquipmentIndex.ArmorItemEndSlot];
                ItemObject item1 = item.Item;
                if (item1 != null && item1.HasHorseComponent && item1.HorseComponent.IsRideable)
                {
                    int agentMountIndex = -1;
                    if (agentBuildData.AgentMountIndexOverriden)
                    {
                        agentMountIndex = agentBuildData.AgentMountIndex;
                    }
                    EquipmentElement equipmentElement1 = equipmentElement[EquipmentIndex.ArmorItemEndSlot];
                    EquipmentElement item2 = equipmentElement[EquipmentIndex.HorseHarness];
                    valueOrDefault = agentBuildData.AgentInitialPosition.GetValueOrDefault();
                    valueOrDefault1 = agentBuildData.AgentInitialDirection.GetValueOrDefault();
                    agent = __instance.CreateHorseAgentFromRosterElements(equipmentElement1, item2, ref valueOrDefault, ref valueOrDefault1, agentMountIndex, agentBuildData.AgentMountKey);
                    Equipment equipment = new Equipment();
                    equipment[EquipmentIndex.ArmorItemEndSlot] = equipmentElement[EquipmentIndex.ArmorItemEndSlot];
                    equipment[EquipmentIndex.HorseHarness] = equipmentElement[EquipmentIndex.HorseHarness];
                    agent.InitializeSpawnEquipment(equipment);
                    formationPositionPreference.SetMountAgentBeforeBuild(agent);
                }
                if (spawnFromAgentVisuals || !GameNetwork.IsClientOrReplay)
                {
                    formationPositionPreference.Equipment.CheckLoadedAmmos();
                }
                if (!agentBuildData.BodyPropertiesOverriden)
                {
                    formationPositionPreference.UpdateBodyProperties(agentCharacter.GetBodyProperties(equipmentElement, agentBuildData.AgentEquipmentSeed));
                }
                if (GameNetwork.IsServerOrRecorder && formationPositionPreference.RiderAgent == null)
                {
                    Vec3 vec3 = agentBuildData.AgentInitialPosition.GetValueOrDefault();
                    Vec2 vec21 = agentBuildData.AgentInitialDirection.GetValueOrDefault();
                    if (!formationPositionPreference.IsMount)
                    {
                        bool agentMissionPeer = agentBuildData.AgentMissionPeer != null;
                        if (agentMissionPeer)
                        {
                            networkPeer = agentBuildData.AgentMissionPeer.GetNetworkPeer();
                        }
                        else
                        {
                            MissionPeer owningAgentMissionPeer = agentBuildData.OwningAgentMissionPeer;
                            if (owningAgentMissionPeer != null)
                            {
                                networkPeer = owningAgentMissionPeer.GetNetworkPeer();
                            }
                            else
                            {
                                networkPeer = null;
                            }
                        }
                        NetworkCommunicator networkCommunicator = networkPeer;
                        bool flag = (formationPositionPreference.MountAgent == null ? false : formationPositionPreference.MountAgent.RiderAgent == formationPositionPreference);
                        GameNetwork.BeginBroadcastModuleEvent();
                        int index1 = formationPositionPreference.Index;
                        BasicCharacterObject character = formationPositionPreference.Character;
                        Monster monster = formationPositionPreference.Monster;
                        Equipment spawnEquipment1 = formationPositionPreference.SpawnEquipment;
                        MissionEquipment missionEquipment = formationPositionPreference.Equipment;
                        BodyProperties bodyPropertiesValue = formationPositionPreference.BodyPropertiesValue;
                        int bodyPropertiesSeed = formationPositionPreference.BodyPropertiesSeed;
                        bool isFemale = formationPositionPreference.IsFemale;
                        Team team = formationPositionPreference.Team;
                        if (team != null)
                        {
                            teamIndex = team.TeamIndex;
                        }
                        else
                        {
                            teamIndex = -1;
                        }
                        Formation formation = formationPositionPreference.Formation;
                        if (formation != null)
                        {
                            index = formation.Index;
                        }
                        else
                        {
                            index = -1;
                        }
                        uint clothingColor1 = formationPositionPreference.ClothingColor1;
                        uint clothingColor2 = formationPositionPreference.ClothingColor2;
                        num = (flag ? formationPositionPreference.MountAgent.Index : -1);
                        Agent mountAgent = formationPositionPreference.MountAgent;
                        if (mountAgent != null)
                        {
                            spawnEquipment = mountAgent.SpawnEquipment;
                        }
                        else
                        {
                            spawnEquipment = null;
                        }
                        GameNetwork.WriteMessage(new CreateAgent(index1, character, monster, spawnEquipment1, missionEquipment, bodyPropertiesValue, bodyPropertiesSeed, isFemale, teamIndex, index, clothingColor1, clothingColor2, num, spawnEquipment, agentMissionPeer, vec3, vec21, networkCommunicator));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                    }
                    else
                    {
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new CreateFreeMountAgent(formationPositionPreference, vec3, vec21));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                    }
                }
                MultiplayerMissionAgentVisualSpawnComponent missionBehavior = __instance.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>();
                if (missionBehavior != null && agentBuildData.AgentMissionPeer != null && agentBuildData.AgentMissionPeer.IsMine && agentBuildData.AgentVisualsIndex == 0)
                {
                    missionBehavior.OnMyAgentSpawned();
                }
                if (agent != null)
                {
                    __instance.BuildAgent(agent, agentBuildData);
                    foreach (MissionBehavior missionBehavior1 in __instance.MissionBehaviors)
                    {
                        missionBehavior1.OnAgentBuild(agent, null);
                    }
                }
                __instance.BuildAgent(formationPositionPreference, agentBuildData);
                if (agentBuildData.AgentMissionPeer != null)
                {
                    formationPositionPreference.MissionPeer = agentBuildData.AgentMissionPeer;
                }
                if (agentBuildData.OwningAgentMissionPeer != null)
                {
                    formationPositionPreference.OwningAgentMissionPeer = agentBuildData.OwningAgentMissionPeer;
                }
                foreach (MissionBehavior missionBehavior2 in __instance.MissionBehaviors)
                {
                    Agent agent1 = formationPositionPreference;
                    object agentBanner = agentBuildData.AgentBanner;
                    if (agentBanner == null)
                    {
                        Team agentTeam = agentBuildData.AgentTeam;
                        if (agentTeam != null)
                        {
                            agentBanner = agentTeam.Banner;
                        }
                        else
                        {
                            agentBanner = null;
                        }
                    }
                    missionBehavior2.OnAgentBuild(agent1, agentBanner as Banner);
                }
                formationPositionPreference.AgentVisuals.CheckResources(true);
                if (formationPositionPreference.IsAIControlled)
                {
                    if (agent == null)
                    {
                        formationPositionPreference.SetAgentFlags(formationPositionPreference.GetAgentFlags() & (AgentFlag.Mountable | AgentFlag.CanJump | AgentFlag.CanRear | AgentFlag.CanAttack | AgentFlag.CanDefend | AgentFlag.RunsAwayWhenHit | AgentFlag.CanCharge | AgentFlag.CanBeCharged | AgentFlag.CanClimbLadders | AgentFlag.CanBeInGroup | AgentFlag.CanSprint | AgentFlag.IsHumanoid | AgentFlag.CanGetScared | AgentFlag.CanWieldWeapon | AgentFlag.CanCrouch | AgentFlag.CanGetAlarmed | AgentFlag.CanWander | AgentFlag.CanKick | AgentFlag.CanRetreat | AgentFlag.MoveAsHerd | AgentFlag.MoveForwardOnly | AgentFlag.IsUnique | AgentFlag.CanUseAllBowsMounted | AgentFlag.CanReloadAllXBowsMounted | AgentFlag.CanDeflectArrowsWith2HSword));
                    }
                    else if (formationPositionPreference.Formation == null)
                    {
                        formationPositionPreference.SetRidingOrder(RidingOrder.RidingOrderEnum.Mount);
                    }
                }
                __result = formationPositionPreference;

                return false;
            }
            catch (Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
                return true;
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch("BuildAgent")]
        private static bool BuildAgent(ref Mission __instance, ref int ____agentCreationIndex, ref List<Agent> ____activeAgents, ref List<Agent> ____allAgents, Agent agent, AgentBuildData agentBuildData)
        {
            try
            {
                if (agent == null)
                {
                    throw new MBNullParameterException("agent");
                }
                agent.Build(agentBuildData, ____agentCreationIndex);
                ____agentCreationIndex++;
                if (!agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot].IsEmpty)
                {
                    EquipmentElement item = agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot];
                    if (item.Item.HorseComponent.BodyLength != 0)
                    {
                        agent.SetInitialAgentScale(0.01f * (float) item.Item.HorseComponent.BodyLength);
                    }
                }
                agent.EquipItemsFromSpawnEquipment(true);
                agent.InitializeAgentRecord();
                agent.AgentVisuals.BatchLastLodMeshes();
                agent.PreloadForRendering();
                ActionIndexValueCache currentActionValue = agent.GetCurrentActionValue(0);
                if (currentActionValue != ActionIndexValueCache.act_none)
                {
                    agent.SetActionChannel(0, currentActionValue, false, (ulong) 0, 0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloat * 0.8f, false, -0.2f, 0, true);
                }
                agent.InitializeComponents();
                if (agent.Controller == Agent.ControllerType.Player)
                {
                    __instance.ResetFirstThirdPersonView();
                }
                ____activeAgents.Add(agent);
                ____allAgents.Add(agent);


                agent.FixImmortality();
                agent.RebuildFromFix();
                return false;
            }
            catch (Exception e)
            {
                TimeLord.Util.Log.NotifyBad(e.ToString());
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
                return true;
            }
        }

    }
}
