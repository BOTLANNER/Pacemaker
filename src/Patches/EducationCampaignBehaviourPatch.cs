using HarmonyLib;

using Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(EducationCampaignBehavior))]
    internal static class EducationCampaignBehaviourPatch
    {
        private static readonly System.Type ChildAgeStateT;
        private static readonly Reflect.Method DoStage;
        private static readonly Reflect.Method GetStage;

        static EducationCampaignBehaviourPatch()
        {
            ChildAgeStateT = typeof(EducationCampaignBehavior).Assembly.GetType("TaleWorlds.CampaignSystem.CampaignBehaviors.EducationCampaignBehavior+ChildAgeState");
            DoStage = new(typeof(EducationCampaignBehavior), "DoStage");
            GetStage = new(typeof(EducationCampaignBehavior), "GetStage", new[] { typeof(Hero), ChildAgeStateT });
        }

        private enum ChildAgeState : short
        {
            Invalid = -1,
            First = 0,
            Year2 = 0,
            Year5 = 1,
            Year8 = 2,
            Year11 = 3,
            Year14 = 4,
            Last = 5,
            Year16 = 5,
            Count = 6
        }

        [HarmonyPrefix]
        [HarmonyPatch("ChildStateToAge")]
        private static bool ChildStateToAge(ref int __result, object state)
        {
            try
            {
                if (Main.Settings!.EnableAgeStageTweaks)
                {
                    switch ((ChildAgeState) state)
                    {
                        case ChildAgeState.Year2:
                            {
                                __result = Main.Settings!.BecomeInfantAge > 1 ? Main.Settings!.BecomeInfantAge - 1 : Main.Settings!.BecomeInfantAge;
                                return false;
                            }
                        case ChildAgeState.Year5:
                            {
                                __result = Main.Settings!.BecomeChildAge > 1 ? Main.Settings!.BecomeChildAge - 1 : Main.Settings!.BecomeChildAge;
                                return false;
                            }
                        case ChildAgeState.Year8:
                            {
                                var teenDiff = Main.Settings!.BecomeTeenagerAge - Main.Settings!.BecomeChildAge;
                                if (teenDiff >= 3)
                                {

                                    __result = Main.Settings!.BecomeChildAge + ((int) teenDiff / 3);
                                }
                                else if (teenDiff > 1)
                                {
                                    __result = Main.Settings!.BecomeChildAge + 1;
                                }
                                else
                                {
                                    __result = Main.Settings!.BecomeChildAge;
                                }
                                return false;
                            }
                        case ChildAgeState.Year11:
                            {
                                var teenDiff = Main.Settings!.BecomeTeenagerAge - Main.Settings!.BecomeChildAge;
                                if (teenDiff >= 3)
                                {

                                    __result = Main.Settings!.BecomeChildAge + (((int) teenDiff / 3) * 2);
                                }
                                else if (teenDiff > 2)
                                {
                                    __result = Main.Settings!.BecomeChildAge + 2;
                                }
                                else if (teenDiff > 1)
                                {
                                    __result = Main.Settings!.BecomeChildAge + 1;
                                }
                                else
                                {
                                    __result = Main.Settings!.BecomeChildAge;
                                }
                                return false;
                            }
                        case ChildAgeState.Year14:
                            {
                                __result = Main.Settings!.BecomeTeenagerAge;
                                var adultDiff = Main.Settings!.HeroComesOfAge - Main.Settings!.BecomeTeenagerAge;
                                var teenDiff = Main.Settings!.BecomeTeenagerAge - Main.Settings!.BecomeChildAge;
                                if (adultDiff < 1 && teenDiff > 0)
                                {
                                    __result = Main.Settings!.BecomeTeenagerAge - 1;
                                }
                                else
                                {
                                    __result = Main.Settings!.BecomeTeenagerAge;
                                }
                                return false;
                            }
                        case ChildAgeState.Year16:
                            {
                                var adultDiff = Main.Settings!.HeroComesOfAge - Main.Settings!.BecomeTeenagerAge;
                                if (adultDiff > 2)
                                {
                                    __result = Main.Settings!.BecomeTeenagerAge + 2;
                                }
                                else if (adultDiff > 1)
                                {
                                    __result = Main.Settings!.BecomeTeenagerAge + 1;
                                }
                                else
                                {
                                    __result = Main.Settings!.BecomeTeenagerAge;
                                }
                                return false;
                            }
                    }
                }

                switch ((ChildAgeState) state)
                {
                    case ChildAgeState.Year2:
                        {
                            __result = 2;
                            return false;
                        }
                    case ChildAgeState.Year5:
                        {
                            __result = 5;
                            return false;
                        }
                    case ChildAgeState.Year8:
                        {
                            __result = 8;
                            return false;
                        }
                    case ChildAgeState.Year11:
                        {
                            __result = 11;
                            return false;
                        }
                    case ChildAgeState.Year14:
                        {
                            __result = 14;
                            return false;
                        }
                    case ChildAgeState.Year16:
                        {
                            __result = 16;
                            return false;
                        }
                }
                __result = -1;
                return true;
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                return true;
            }
        }

        // // Uncomment below for additional debug hits.

        //[HarmonyPrefix]
        //[HarmonyPatch("DoEducationUntil")]

        //private static bool DoEducationUntil(ref Dictionary<Hero, short> ____previousEducations, ref EducationCampaignBehavior __instance, Hero child, object childAgeState)
        //{
        //    short num;
        //    if (!____previousEducations.TryGetValue(child, out num))
        //    {
        //        num = -1;
        //    }
        //    for (short i = (short) (num + 1); i < (short) childAgeState; i = (short) ((short) i + (short) ChildAgeState.Year5))
        //    {
        //        if (i != (short) (ChildAgeState.Invalid | ChildAgeState.Year5 | ChildAgeState.Year8 | ChildAgeState.Year11 | ChildAgeState.Year14 | ChildAgeState.Year16 | ChildAgeState.Count | ChildAgeState.Last))
        //        {
        //            object stage = GetStage.MethodInfo.Invoke(__instance, new object[] { child, i });
        //            DoStage.MethodInfo.Invoke(__instance, new[] { child, stage });
        //        }
        //    }
        //    return false;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch("GetClosestStage")]

        //private static bool GetClosestStage(Hero child, ref object __result)
        //{
        //    ChildAgeState childAgeState = ChildAgeState.Year2;
        //    int num = MathF.Round(child.Age);
        //    for (ChildAgeState i = ChildAgeState.Year2; i <= ChildAgeState.Year16; i = (ChildAgeState) ((short) i + (short) ChildAgeState.Year5))
        //    {
        //        int ageResult = 0;
        //        ChildStateToAge(ref ageResult, (short) i);
        //        if (num >= ageResult)
        //        {
        //            childAgeState = i;
        //        }
        //    }
        //    __result = (short) childAgeState;
        //    return false;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch("GetLastDoneStage")]

        //private static bool GetLastDoneStage(ref Dictionary<Hero, short> ____previousEducations, Hero child, ref object __result)
        //{
        //    short num;
        //    if (____previousEducations.TryGetValue(child, out num))
        //    {
        //        __result = (short) num;
        //        return false;
        //    }
        //    __result = (short) (ChildAgeState.Invalid | ChildAgeState.Year5 | ChildAgeState.Year8 | ChildAgeState.Year11 | ChildAgeState.Year14 | ChildAgeState.Year16 | ChildAgeState.Count | ChildAgeState.Last);
        //    return false;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch("HasNotificationForAge")]

        //private static bool HasNotificationForAge(Hero child, int age, ref bool __result)
        //{
        //    __result = Campaign.Current.CampaignInformationManager.InformationDataExists<EducationMapNotification>((EducationMapNotification notification) =>
        //    {
        //        if (notification.Child != child)
        //        {
        //            return false;
        //        }
        //        return notification.Age == age;
        //    });
        //    return false;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch("IsHeroChildOfPlayer")]

        //private static bool IsHeroChildOfPlayer(Hero child, ref bool __result)
        //{
        //    __result = Hero.MainHero.Children.Contains(child);
        //    return false;
        //}


        //[HarmonyPrefix]
        //[HarmonyPatch("OnCharacterCreationOver")]
        //private static bool OnCharacterCreationOver(ref EducationCampaignBehavior __instance)
        //{
        //    if (CampaignOptions.IsLifeDeathCycleDisabled)
        //    {
        //        CampaignEventDispatcher.Instance.RemoveListeners(__instance);
        //    }
        //    return false;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch("OnDailyTick")]
        //private static bool OnDailyTick(ref Dictionary<Hero, short> ____previousEducations, ref EducationCampaignBehavior __instance)
        //{
        //    if (MapEvent.PlayerMapEvent == null)
        //    {
        //        foreach (Hero hero in Clan.PlayerClan.Heroes)
        //        {
        //            if (!hero.IsAlive || hero == Hero.MainHero || hero.Age >= (float) Campaign.Current.Models.AgeModel.HeroComesOfAge)
        //            {
        //                continue;
        //            }
        //            object lastDoneStage = default(short);
        //            GetLastDoneStage(ref ____previousEducations, hero, ref lastDoneStage);
        //            if ((ChildAgeState) lastDoneStage == ChildAgeState.Year16)
        //            {
        //                continue;
        //            }

        //            object closest = default(short);
        //            GetClosestStage(hero, ref closest);
        //            ChildAgeState childAgeState = (ChildAgeState) MathF.Max((short) lastDoneStage + (short) ChildAgeState.Year5, (short) closest);
        //            int age = 0;
        //            ChildStateToAge(ref age, (short) childAgeState);
        //            bool hasNot = false;
        //            if (!(hero.BirthDay + CampaignTime.Years((float) age)).IsPast || (!HasNotificationForAge(hero, age, ref hasNot) && hasNot))
        //            {
        //                continue;
        //            }
        //            DoEducationUntil(ref ____previousEducations, ref __instance, hero, (short) childAgeState);

        //            ChildStateToAge(ref age, (short) childAgeState);
        //            ShowEducationNotification(ref ____previousEducations, hero, age);
        //        }
        //    }
        //    return false;
        //}


        //[HarmonyPrefix]
        //[HarmonyPatch("OnHeroComesOfAge")]
        //private static bool OnHeroComesOfAge(ref Dictionary<Hero, short> ____previousEducations, ref EducationCampaignBehavior __instance, Hero hero)
        //{
        //    if (hero.Clan == Clan.PlayerClan)
        //    {
        //        DoEducationUntil(ref ____previousEducations, ref __instance, hero, (short) ChildAgeState.Count);
        //        ____previousEducations.Remove(hero);
        //    }
        //    return false;
        //}


        //[HarmonyPrefix]
        //[HarmonyPatch("OnHeroKilled")]
        //private static bool OnHeroKilled(ref Dictionary<Hero, short> ____previousEducations, ref EducationCampaignBehavior __instance, Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail details, bool showNotifications)
        //{
        //    if (victim.Clan == Clan.PlayerClan && ____previousEducations.ContainsKey(victim))
        //    {
        //        ____previousEducations.Remove(victim);
        //    }
        //    return false;
        //}


        //[HarmonyPrefix]
        //[HarmonyPatch("ShowEducationNotification")]
        //private static bool ShowEducationNotification(ref Dictionary<Hero, short> ____previousEducations, Hero child, int age)
        //{
        //    TextObject textObject = GameTexts.FindText("str_education_notification_right", null);
        //    textObject.SetCharacterProperties("CHILD", child.CharacterObject, true);
        //    Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(new EducationMapNotification(child, age, textObject));
        //    Debug.Print(String.Format("ShowEducationNotification, Hero: {0} - Age: {1}.", child.Name, age), 0, Debug.DebugColor.White, 17592186044416L);
        //    if (!____previousEducations.ContainsKey(child))
        //    {
        //        ____previousEducations.Add(child, -1);
        //    }

        //    return false;
        //}



        [HarmonyPrefix]
        [HarmonyPatch("RegisterEvents")]
        private static bool RegisterEvents()
        {
            try
            {
                return !Main.Settings!.CustomSkillGrowth;
            }
            catch (Exception e) { Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  return true; }
        }
    }
}