using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    public class FamilyControlSupportPatch : IOptionalPatch
    {
        private Assembly? familyControlAssembly = null;

        public bool TryPatch(Harmony harmony)
        {
            try
            {
                familyControlAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("FamilyControl, "));

                if (familyControlAssembly != null)
                {
                    var FamilyControlBehaviorType = familyControlAssembly.GetType("FamilyControl.FamilyControlBehavior", false, true);
                    //var type = Type.GetType("FamilyControl.FamilyControlBehavior", AssemblyResolver, TypeResolver, false, true);
                    if (FamilyControlBehaviorType != null)
                    {
                        harmony.Patch(AccessTools.Method(FamilyControlBehaviorType, "ShowAdultHeroes"), prefix: new HarmonyMethod(typeof(FamilyControlSupportPatch), nameof(ShowAdultHeroes)));
                        harmony.Patch(AccessTools.Method(FamilyControlBehaviorType, "RecordPrePregnantInfo"), prefix: new HarmonyMethod(typeof(FamilyControlSupportPatch), nameof(RecordPrePregnantInfo)));
                        harmony.Patch(AccessTools.Method(FamilyControlBehaviorType, "IsTimeToAddNewPregnant"), prefix: new HarmonyMethod(typeof(FamilyControlSupportPatch), nameof(IsTimeToAddNewPregnant)));
                    }
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.WriteDebugLineOnScreen(e.ToString());
                return false;
            }
        }

        public bool MenusInitialised(Harmony harmony)
        {
            try
            {
                if (familyControlAssembly != null)
                {
                    var ConfigType = familyControlAssembly.GetType("FamilyControl.Config", false, true);
                    if (ConfigType != null && Main.Settings != null)
                    {
                        //harmony.Patch(AccessTools.Constructor(ConfigType, searchForStatic: true), postfix: new HarmonyMethod(typeof(FamilyControlSupportPatch), nameof(ConfigStaticCtor))); 
                        Config.MinAge = Main.Settings.HeroComesOfAge;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteDebugLineOnScreen(e.ToString());
            }
            return false;
        }

        public static bool IsTimeToAddNewPregnant(ref object __instance,ref bool __result, ref Dictionary<Hero, object> ___m_prePregnancyInfoMap, Hero mother, Hero father = null)
        {
            try
            {
                bool result;
                bool flag;
                if ((mother == null ? false : ___m_prePregnancyInfoMap.ContainsKey(mother)))
                {
                    Type mapInfoType = __instance.GetType().Assembly.GetType("FamilyControl.FamilyControlBehavior+PrePregnancyInfo");
                    var m_pregnantDateField = AccessTools.Field(mapInfoType, "m_pregnantDate");
                    var m_fatherField = AccessTools.Field(mapInfoType, "m_father");

                    var prePregnancyInfo = ___m_prePregnancyInfoMap[mother];
                    if (!mother.IsFemale || mother.IsDead || (double) mother.Age <= (double) Config.MinAge)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = (father == null ? false : (object) m_fatherField.GetValue(prePregnancyInfo) != (object) father);
                    }
                    if (!flag)
                    {
                        CampaignTime pregnantDate = (CampaignTime) m_pregnantDateField.GetValue(prePregnancyInfo);
                        result = (double) ((CampaignTime) m_pregnantDateField.GetValue(prePregnancyInfo)).ElapsedDaysUntilNow >= 0;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
                __result = result;

                return false;
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                return true;
            }
        }

        public static bool RecordPrePregnantInfo(ref object __instance, ref Dictionary<Hero, object> ___m_prePregnancyInfoMap, Hero sexPartner1, Hero sexPartner2)
        {
            try
            {
                bool flag;
                if ((double) MBRandom.RandomFloatRanged(0f, 1f) > (double) Config.PregnancyChance || sexPartner1 == null || sexPartner2 == null)
                {
                    flag = true;
                }
                else
                {
                    flag = (Config.LesbianPregnancy || !sexPartner1.IsFemale ? false : sexPartner2.IsFemale);
                }
                if (!flag)
                {
                    Hero key = null;
                    Hero father = null;
                    bool oneIsLady = (!sexPartner1.IsAlive || !sexPartner1.IsFemale || sexPartner1.IsPregnant ? false : (double) sexPartner1.Age > (double) Config.MinAge);
                    bool TwoIsLady = (!sexPartner2.IsAlive || !sexPartner2.IsFemale || sexPartner2.IsPregnant ? false : (double) sexPartner2.Age > (double) Config.MinAge);
                    if (oneIsLady & TwoIsLady)
                    {
                        if (((object) sexPartner1 == (object) Hero.MainHero ? true : (object) sexPartner2 == (object) Hero.MainHero))
                        {
                            if (Config.LesbianPregnancyOn == "Player Only")
                            {
                                if ((object) sexPartner1 != (object) Hero.MainHero)
                                {
                                    key = sexPartner2;
                                    father = sexPartner1;
                                }
                                else
                                {
                                    key = sexPartner1;
                                    father = sexPartner2;
                                }
                            }
                            else if (Config.LesbianPregnancyOn == "NPC Only")
                            {
                                if ((object) sexPartner1 != (object) Hero.MainHero)
                                {
                                    key = sexPartner1;
                                    father = sexPartner2;
                                }
                                else
                                {
                                    key = sexPartner2;
                                    father = sexPartner1;
                                }
                            }
                            else if (MBRandom.RandomInt(0, 1) != 0)
                            {
                                key = sexPartner1;
                                father = sexPartner2;
                            }
                            else
                            {
                                key = sexPartner1;
                                father = sexPartner2;
                            }
                        }
                    }
                    else if (oneIsLady)
                    {
                        key = sexPartner1;
                        father = sexPartner2;
                    }
                    else if (TwoIsLady)
                    {
                        key = sexPartner2;
                        father = sexPartner1;
                    }
                    if ((key == null ? false : father != null))
                    {
                        Type mapInfoType = __instance.GetType().Assembly.GetType("FamilyControl.FamilyControlBehavior+PrePregnancyInfo");
                        var m_pregnantDateField = AccessTools.Field(mapInfoType, "m_pregnantDate");
                        var m_fatherField = AccessTools.Field(mapInfoType, "m_father");

                        CampaignTime pregnantDate = CampaignTime.DaysFromNow((float) MBRandom.RandomInt(Config.MinPregnancyDelay, Config.MaxPregnancyDelay));
                        if (!___m_prePregnancyInfoMap.ContainsKey(key))
                        {
                            ConstructorInfo constructorInfo = AccessTools.Constructor(mapInfoType, new[] { typeof(Hero), typeof(CampaignTime) });
                            ___m_prePregnancyInfoMap.Add(key, constructorInfo.Invoke(parameters: new object[] { father, pregnantDate }));
                        }
                        else if (((CampaignTime) m_pregnantDateField.GetValue(___m_prePregnancyInfoMap[key])) > pregnantDate)
                        {
                            m_fatherField.SetValue(___m_prePregnancyInfoMap[key], father);
                            m_pregnantDateField.SetValue(___m_prePregnancyInfoMap[key], pregnantDate);
                        }
                        Type utilityType = __instance.GetType().Assembly.GetType("FamilyControl.Utillty");
                        MethodInfo RealDisplayMessage = AccessTools.Method(utilityType, "RealDisplayMessage");
                        RealDisplayMessage.Invoke(null, new object[] { string.Concat(new string[] { "_recordPrePregnantInfo Success! Mother : ", key.Name.ToString(), " Father : ", father.Name.ToString(), " PregnantDate : ", pregnantDate.ToString() }) });
                    }
                }

                return false;
            }
            catch (Exception e) { Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  return true; }
        }

        //private static void ConfigStaticCtor()
        //{
        //    ___MinAge = Main.Settings!.HeroComesOfAge;
        //}

        private static bool ShowAdultHeroes(object __instance)
        {
            try
            {
                foreach (Hero hero in Hero.AllAliveHeroes)
                {
                    if (hero.Age >= Main.Settings!.HeroComesOfAge)
                    {
                        bool exspouse = false;
                        string str = hero.Name.ToString();
                        float age = hero.Age;
                        Type utilityType = __instance.GetType().Assembly.GetType("FamilyControl.Utillty");
                        MethodInfo RealDisplayMessage = AccessTools.Method(utilityType, "RealDisplayMessage");
                        RealDisplayMessage.Invoke(null, new object[] { string.Concat("Child: ", str, "| Age: ", age.ToString()) });
                        RealDisplayMessage.Invoke(null, new object[] { string.Concat("Mother: ", hero.Mother.Name.ToString(), "| Father: ", hero.Father.Name.ToString()) });
                        if (hero.Mother.ExSpouses != null)
                        {
                            foreach (Hero hero2 in hero.Mother.ExSpouses)
                            {
                                if ((object) hero2 == (object) hero.Father)
                                {
                                    exspouse = true;
                                }
                            }
                        }
                        if (((object) hero.Mother == (object) hero.Father.Spouse ? false : !exspouse))
                        {
                            RealDisplayMessage.Invoke(null, new object[] { "THIS IS A CUSTOM HERO." });
                        }
                        RealDisplayMessage.Invoke(null, new object[] { "...................................." });
                    }
                }
                return false;
            }
            catch (Exception e) { Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  return true; }
        }

        //private static Type TypeResolver(Assembly arg1, string arg2, bool arg3)
        //{
        //    throw new NotImplementedException();
        //}

        //private static Assembly AssemblyResolver(AssemblyName arg)
        //{
        //    throw new NotImplementedException();
        //}

        public static class Config
        {
            static Type _realType;
            static Assembly _familyControlAssembly;

            static Config()
            {
                try
                {
                    _familyControlAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith("FamilyControl, "));

                    _realType = _familyControlAssembly.GetType("FamilyControl.Config", false, true);
                }
                catch (Exception e)
                {
                    Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                }
                
            }

            internal static bool SameGenderInteractions => (bool) _realType.GetField("SameGenderInteractions", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static bool IncestInteractions => (bool) _realType.GetField("IncestInteractions", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static bool PregnancyTimeInteractions => (bool) _realType.GetField("PregnancyTimeInteractions", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static int InteractionLimit => (int) _realType.GetField("InteractionLimit", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static int RelationSpouse => (int) _realType.GetField("RelationSpouse", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static int RelationSingle => (int) _realType.GetField("RelationSingle", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static int RelationMarried => (int) _realType.GetField("RelationMarried", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static int MinDaysToPass => (int) _realType.GetField("MinDaysToPass", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static int MaxDaysToPass => (int) _realType.GetField("MaxDaysToPass", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static float PregnancyChance => (float) _realType.GetField("PregnancyChance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static int MinPregnancyDelay => (int) _realType.GetField("MinPregnancyDelay", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static int MaxPregnancyDelay => (int) _realType.GetField("MaxPregnancyDelay", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static bool LesbianPregnancy => (bool) _realType.GetField("LesbianPregnancy", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static string LesbianPregnancyOn => _realType.GetField("LesbianPregnancyOn", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as string;

            internal static bool DisableDefaultPregnancyMethod => (bool) _realType.GetField("DisableDefaultPregnancyMethod", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static bool AllPregnancyLogNotification => (bool) _realType.GetField("AllPregnancyLogNotification", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static bool BlackScreenEffect => (bool) _realType.GetField("BlackScreenEffect", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static string SoundSet => _realType.GetField("SoundSet", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as string;

            internal static bool NPCSelfAbortionAllow => (bool) _realType.GetField("NPCSelfAbortionAllow", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static bool NPCAbortionDecline => (bool) _realType.GetField("NPCAbortionDecline", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            internal static bool BattleAbortion => (bool) _realType.GetField("BattleAbortion", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            private static bool ageInit = false;
            internal static float MinAge
            {
                get
                {
                    if (!ageInit && Main.Settings != null)
                    {
                        ageInit = true;
                        _realType.GetField("MinAge", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, Main.Settings.HeroComesOfAge);
                    }
                    return (float) _realType.GetField("MinAge", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                }
                set
                {
                    _realType.GetField("MinAge", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
                }
            }
        }
    }
}
