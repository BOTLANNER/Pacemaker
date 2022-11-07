using TimeLord.Extensions;

using HarmonyLib;

using System;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Library;

namespace TimeLord.Patches
{
    [HarmonyPatch(typeof(DefaultAgeModel))]
    internal static class DefaultAgeModelPatch
    {

        [HarmonyPrefix]
        [HarmonyPatch("BecomeChildAge", MethodType.Getter)]
        static bool BecomeChildAge(ref int __result)
        {
            try
            {
                if (!Main.Settings!.EnableAgeStageTweaks)
                {
                    return true;
                }

                __result = Main.Settings!.BecomeChildAge;
                return false;
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("BecomeInfantAge", MethodType.Getter)]
        static bool BecomeInfantAge(ref int __result)
        {
            try
            {
                if (!Main.Settings!.EnableAgeStageTweaks)
                {
                    return true;
                }

                __result = Main.Settings!.BecomeInfantAge;
                return false;
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("BecomeOldAge", MethodType.Getter)]
        static bool BecomeOldAge(ref int __result)
        {
            try
            {
                if (!Main.Settings!.EnableAgeStageTweaks)
                {
                    return true;
                }
                __result = Main.Settings!.BecomeOldAge;
                return false;
            }
            catch (Exception e) { Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace);  return true; }
        }

        [HarmonyPrefix]
        [HarmonyPatch("BecomeTeenagerAge", MethodType.Getter)]
        static bool BecomeTeenagerAge(ref int __result)
        {try
            {
                if (!Main.Settings!.EnableAgeStageTweaks)
                {
                    return true;
                }
                __result = Main.Settings!.BecomeTeenagerAge;
                return false;
            }
            catch (Exception e) { throw; }
        }

        [HarmonyPrefix]
        [HarmonyPatch("HeroComesOfAge", MethodType.Getter)]
        static bool HeroComesOfAge(ref int __result)
        {try
            {
                if (!Main.Settings!.EnableAgeStageTweaks)
                {
                    return true;
                }
                __result = Main.Settings!.HeroComesOfAge;
                return false;
            }
            catch (Exception e) { throw; }
        }

        [HarmonyPrefix]
        [HarmonyPatch("MaxAge", MethodType.Getter)]
        static bool MaxAge(ref int __result)
        {try
            {
                if (!Main.Settings!.EnableAgeStageTweaks)
                {
                    return true;
                }
                __result = Main.Settings!.MaxAge;
                return false;
            }
            catch (Exception e) { throw; }
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetAgeLimitForLocation")]
        static bool GetAgeLimitForLocation(ref DefaultAgeModel __instance, CharacterObject character, out int minimumAge, out int maximumAge, string additionalTags = "")
        {
            try
            {
                if (!Main.Settings!.EnableAgeOccupationTweaks)
                {
                    minimumAge = __instance.HeroComesOfAge;
                    maximumAge = __instance.MaxAge;
                    return true;
                }

                if (character.Occupation == Occupation.TavernWench)
                {
                    minimumAge = Main.Settings!.TavernWenchMinAge;
                    maximumAge = Main.Settings!.TavernWenchMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.Townsfolk)
                {
                    if (additionalTags == DefaultAgeModel.TavernVisitorTag)
                    {
                        minimumAge = Main.Settings!.TavernVisitorMinAge;
                        maximumAge = Main.Settings!.TavernVisitorMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.TavernDrinkerTag)
                    {
                        minimumAge = Main.Settings!.TavernDrinkerMinAge;
                        maximumAge = Main.Settings!.TavernDrinkerMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.SlowTownsmanTag)
                    {
                        minimumAge = Main.Settings!.SlowTownsmanMinAge;
                        maximumAge = Main.Settings!.SlowTownsmanMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.TownsfolkCarryingStuffTag)
                    {
                        minimumAge = Main.Settings!.TownsfolkCarryingStuffMinAge;
                        maximumAge = Main.Settings!.TownsfolkCarryingStuffMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.BroomsWomanTag)
                    {
                        minimumAge = Main.Settings!.BroomsWomanMinAge;
                        maximumAge = Main.Settings!.BroomsWomanMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.DancerTag)
                    {
                        minimumAge = Main.Settings!.DancerMinAge;
                        maximumAge = Main.Settings!.DancerMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.BeggarTag)
                    {
                        minimumAge = Main.Settings!.BeggarMinAge;
                        maximumAge = Main.Settings!.BeggarMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.ChildTag)
                    {
                        minimumAge = __instance.BecomeChildAge;
                        maximumAge = __instance.BecomeTeenagerAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.TeenagerTag)
                    {
                        minimumAge = __instance.BecomeTeenagerAge;
                        maximumAge = __instance.HeroComesOfAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.InfantTag)
                    {
                        minimumAge = __instance.BecomeInfantAge;
                        maximumAge = __instance.BecomeChildAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.BarberTag)
                    {
                        minimumAge = Main.Settings!.BarberMinAge;
                        maximumAge = Main.Settings!.BarberMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.NotaryTag)
                    {
                        minimumAge = Main.Settings!.NotaryMinAge;
                        maximumAge = Main.Settings!.NotaryMaxAge;
                        return false;
                    }
                    minimumAge = Main.Settings!.TownsfolkMinAge > 0 ? Main.Settings!.TownsfolkMinAge : __instance.HeroComesOfAge;
                    maximumAge = Main.Settings!.TownsfolkMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.Villager)
                {
                    if (additionalTags == DefaultAgeModel.TownsfolkCarryingStuffTag)
                    {
                        minimumAge = Main.Settings!.TownsfolkCarryingStuffMinAge;
                        maximumAge = Main.Settings!.TownsfolkCarryingStuffMaxAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.ChildTag)
                    {
                        minimumAge = __instance.BecomeChildAge;
                        maximumAge = __instance.BecomeTeenagerAge;
                        return false;
                    }
                    if (additionalTags == DefaultAgeModel.TeenagerTag)
                    {
                        minimumAge = __instance.BecomeTeenagerAge;
                        maximumAge = __instance.HeroComesOfAge;
                        return false;
                    }
                    if (additionalTags != DefaultAgeModel.InfantTag)
                    {
                        minimumAge = Main.Settings!.TownsfolkMinAge > 0 ? Main.Settings!.TownsfolkMinAge : __instance.HeroComesOfAge;
                        maximumAge = Main.Settings!.TownsfolkMaxAge;
                        return false;
                    }
                    minimumAge = __instance.BecomeInfantAge;
                    maximumAge = __instance.BecomeChildAge;
                    return false;
                }
                if (character.Occupation == Occupation.TavernGameHost)
                {
                    minimumAge = Main.Settings!.TavernGameHostMinAge;
                    maximumAge = Main.Settings!.TavernGameHostMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.Musician)
                {
                    minimumAge = Main.Settings!.MusicianMinAge;
                    maximumAge = Main.Settings!.MusicianMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.ArenaMaster)
                {
                    minimumAge = Main.Settings!.ArenaMasterMinAge;
                    maximumAge = Main.Settings!.ArenaMasterMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.ShopWorker)
                {
                    minimumAge = Main.Settings!.ShopWorkerMinAge;
                    maximumAge = Main.Settings!.ShopWorkerMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.Tavernkeeper)
                {
                    minimumAge = Main.Settings!.TavernkeeperMinAge;
                    maximumAge = Main.Settings!.TavernkeeperMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.RansomBroker)
                {
                    minimumAge = Main.Settings!.RansomBrokerMinAge;
                    maximumAge = Main.Settings!.RansomBrokerMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.Blacksmith)
                {
                    minimumAge = Main.Settings!.BlacksmithMinAge;
                    maximumAge = Main.Settings!.BlacksmithMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.GoodsTrader)
                {
                    minimumAge = Main.Settings!.GoodsTraderMinAge;
                    maximumAge = Main.Settings!.GoodsTraderMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.HorseTrader)
                {
                    minimumAge = Main.Settings!.HorseTraderMinAge;
                    maximumAge = Main.Settings!.HorseTraderMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.Armorer)
                {
                    minimumAge = Main.Settings!.ArmorerMinAge;
                    maximumAge = Main.Settings!.ArmorerMaxAge;
                    return false;
                }
                if (character.Occupation == Occupation.Weaponsmith)
                {
                    minimumAge = Main.Settings!.WeaponsmithMinAge;
                    maximumAge = Main.Settings!.WeaponsmithMaxAge;
                    return false;
                }
                if (additionalTags == DefaultAgeModel.AlleyGangMemberTag)
                {
                    minimumAge = Main.Settings!.AlleyGangMemberMinAge;
                    maximumAge = Main.Settings!.AlleyGangMemberMaxAge;
                    return false;
                }
                minimumAge = __instance.HeroComesOfAge;
                maximumAge = __instance.MaxAge;
                return false;
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace); Debug.WriteDebugLineOnScreen(e.ToString());  Debug.SetCrashReportCustomString(e.Message); Debug.SetCrashReportCustomStack(e.StackTrace); 
                minimumAge = 18;
                maximumAge = 128;
                return true;
            }
        }

    }
}
