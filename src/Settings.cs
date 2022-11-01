using System.Collections.Generic;

using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
#if MCM_v5
using MCM.Abstractions.Base.Global;
#else
using MCM.Abstractions.Settings.Base.Global;
#endif

namespace Pacemaker
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => $"{Main.Name}_v1";
        public override string DisplayName => Main.DisplayName;
        public override string FolderName => Main.Name;
        public override string FormatType => "json";

        private const string DaysPerSeason_Hint = "Alters the length of a season (and a year). Vanilla uses " +
            "21. NOTE: Once you start a game, this value is permanently set for that campaign. [ Default: 7 ]";

        private const string TimeMultiplier_Hint = "Multiplies the rate at which campaign time passes. " +
            "Note that the same general pace is maintained: days simply pass more quickly/slowly. [ Default: 1.75 ]";

        private const string PlayTimeMultiplier_Hint = "Multiplies map general pace when playing without fast forward. [ Default: 1.00 ]";

        private const string FastForwardTimeMultiplier_Hint = "Multiplies map general pace when fast forwarding. [ Default: 1.00 ]";

        private const string AdultAgeFactor_Hint = "Multiplies the number of effective \"human years\" ADULTS age " +
            "in one calendar year. It's disabled at 100%, but at, e.g., 200%, all ADULT characters age twice as fast. " +
            "[ Default: 200% ]";

        private const string ChildAgeFactor_Hint = "Same as the Adult Accelerated Aging Factor, but it " +
            "applies to children instead. Some might use this to, e.g., make children age faster than adults. " +
            "[ Default: 200% ]";

        private const string EnablePregnancyTweaks_Hint = "Adjust the duration of pregnancies. [ Default: ON ]";

        private const string ScaledPregnancyDuration_Hint = "Scale pregnancy duration to this proportion of a " +
            "year. [ Default: 75% ]";

        private const string AdjustPregnancyDueDates_Hint = "Auto-adjust in-progress pregnancies' due dates to " +
            "match settings upon load of a game. Still works correctly if another mod is overriding " +
            "this mod's pregnancy duration setting. [ Default: ON ]";

        private const string EnableHealingTweaks_Hint = "Auto-calibrate hero & troop healing rate to the Time " +
            "Multiplier in order to maintain vanilla pacing. [ Default: ON ]";

        private const string HealingRateAdjustmentFactor_Hint = "Additional factor to apply to healing rates " +
            "if the default auto-calibration isn't quite right for you. Higher than 100% causes faster healing; " +
            "lower will cause slower. [ Default: 100% ]";

        private const string EnableFoodTweaks_Hint = "Auto-calibrate party food consumption rate to the Time " +
            "Multiplier in order to maintain vanilla pacing. When the Time Multiplier is more than 1.0, as usual, " +
            "parties can otherwise run out of food too quickly. [ Default: ON ]";

        private const string EnableAgeStageTweaks_Hint = "Adjust required values for aging stages. [ Default: ON ]";

        private const string BecomeChildAge_Hint = "Age at which an infant becomes a child. [ Default: 6 ]";

        private const string BecomeInfantAge_Hint = "Age at which a newborn becomes an infant. [ Default: 3 ]";

        private const string BecomeOldAge_Hint = "Age at which an adult becomes old. This will allow for dying of old age after this point [ Default: 47 ]";

        private const string BecomeTeenagerAge_Hint = "Age at which a child becomes a teenager. [ Default: 14 ]";

        private const string HeroComesOfAge_Hint = "Age at which a hero comes of age. After this age heroes are eligible and will spawn in the world [ Default: 18 ]";

        private const string MaxAge_Hint = "Maximum age for heroes. All heroes will die of old age by this age if they have not died before reaching it [ Default: 128 ]";

        private const string HeroStartingAge_Hint = "Starting age for hero on new campaign [ Default: 22 ]";

        private const string EnableAgeOccupationTweaks_Hint = "Adjust age ranges for occupations. [ Default: ON ]";

        private const string TavernWenchMinAge_Hint = "Minimum age for tavern wenches. [ Default: 20 ]";

        private const string TavernWenchMaxAge_Hint = "Maximum age for tavern wenches. [ Default: 28 ]";

        private const string TavernVisitorMinAge_Hint = "Minimum age for tavern visitors. [ Default: 20 ]";

        private const string TavernVisitorMaxAge_Hint = "Maximum age for tavern visitors. [ Default: 60 ]";

        private const string TavernDrinkerMinAge_Hint = "Minimum age for tavern drinkers. [ Default: 20 ]";

        private const string TavernDrinkerMaxAge_Hint = "Maximum age for tavern drinkers. [ Default: 40 ]";

        private const string SlowTownsmanMinAge_Hint = "Minimum age for slow townsmen. [ Default: 50 ]";

        private const string SlowTownsmanMaxAge_Hint = "Maximum age for slow townsmen. [ Default: 70 ]";

        private const string TownsfolkCarryingStuffMinAge_Hint = "Minimum age for townsfolk carrying stuff. [ Default: 20 ]";

        private const string TownsfolkCarryingStuffMaxAge_Hint = "Maximum age for townsfolk carrying stuff. [ Default: 40 ]";

        private const string BroomsWomanMinAge_Hint = "Minimum age for brooms womens. [ Default: 30 ]";

        private const string BroomsWomanMaxAge_Hint = "Maximum age for brooms women. [ Default: 45 ]";

        private const string DancerMinAge_Hint = "Minimum age for dancers. [ Default: 20 ]";

        private const string DancerMaxAge_Hint = "Maximum age for dancers. [ Default: 28 ]";

        private const string BeggarMinAge_Hint = "Minimum age for beggars. [ Default: 60 ]";

        private const string BeggarMaxAge_Hint = "Maximum age for beggars. [ Default: 90 ]";

        private const string NotaryMinAge_Hint = "Minimum age for notaries. [ Default: 30 ]";

        private const string NotaryMaxAge_Hint = "Maximum age for notaries. [ Default: 80 ]";

        private const string BarberMinAge_Hint = "Minimum age for barbers. [ Default: 30 ]";

        private const string BarberMaxAge_Hint = "Maximum age for barbers. [ Default: 80 ]";

        private const string TownsfolkMinAge_Hint = "Minimum age for townsfolk. [ Default: (Adult age setting) ]";

        private const string TownsfolkMaxAge_Hint = "Maximum age for townsfolk. [ Default: 70 ]";

        private const string TavernGameHostMinAge_Hint = "Minimum age for tavern game hosts. [ Default: 30 ]";

        private const string TavernGameHostMaxAge_Hint = "Maximum age for tavern game hosts. [ Default: 40 ]";

        private const string MusicianMinAge_Hint = "Minimum age for musicians. [ Default: 20 ]";

        private const string MusicianMaxAge_Hint = "Maximum age for musicians. [ Default: 40 ]";

        private const string ArenaMasterMinAge_Hint = "Minimum age for arena masters. [ Default: 30 ]";

        private const string ArenaMasterMaxAge_Hint = "Maximum age for arena masters. [ Default: 60 ]";

        private const string ShopWorkerMinAge_Hint = "Minimum age for shop workers. [ Default: 18 ]";

        private const string ShopWorkerMaxAge_Hint = "Maximum age for shop workers. [ Default: 50 ]";

        private const string TavernkeeperMinAge_Hint = "Minimum age for tavern keepers. [ Default: 40 ]";

        private const string TavernkeeperMaxAge_Hint = "Maximum age for tavern keepers. [ Default: 80 ]";

        private const string RansomBrokerMinAge_Hint = "Minimum age for ransom brokers. [ Default: 30 ]";

        private const string RansomBrokerMaxAge_Hint = "Maximum age for ransom brokers. [ Default: 60 ]";

        private const string BlacksmithMinAge_Hint = "Minimum age for blacksmiths. [ Default: 30 ]";

        private const string BlacksmithMaxAge_Hint = "Maximum age for blacksmiths. [ Default: 80 ]";

        private const string GoodsTraderMinAge_Hint = "Minimum age for goods traders. [ Default: 30 ]";

        private const string GoodsTraderMaxAge_Hint = "Maximum age for goods traders. [ Default: 80 ]";

        private const string HorseTraderMinAge_Hint = "Minimum age for horse traders. [ Default: 30 ]";

        private const string HorseTraderMaxAge_Hint = "Maximum age for horse traders. [ Default: 80 ]";

        private const string ArmorerMinAge_Hint = "Minimum age for armorers. [ Default: 30 ]";

        private const string ArmorerMaxAge_Hint = "Maximum age for armorers. [ Default: 80 ]";

        private const string WeaponsmithMinAge_Hint = "Minimum age for weaponsmiths. [ Default: 30 ]";

        private const string WeaponsmithMaxAge_Hint = "Maximum age for weaponsmiths. [ Default: 80 ]";

        private const string AlleyGangMemberMinAge_Hint = "Minimum age for alley gang members. [ Default: 30 ]";

        private const string AlleyGangMemberMaxAge_Hint = "Maximum age for alley gang members. [ Default: 40 ]";

        [SettingPropertyInteger("Days Per Season", 1, 90, HintText = DaysPerSeason_Hint, RequireRestart = false, Order = 0)]
        [SettingPropertyGroup("General Settings", GroupOrder = 0)]
        public int DaysPerSeason { get; set; } = 7;

        [SettingPropertyFloatingInteger("Time Multiplier", 0.3f, 6f, HintText = TimeMultiplier_Hint, RequireRestart = false, Order = 1)]
        [SettingPropertyGroup("General Settings")]
        public float TimeMultiplier { get; set; } = 1.75f;

        [SettingPropertyFloatingInteger("Play Time Multiplier", 0.3f, 10f, HintText = PlayTimeMultiplier_Hint, RequireRestart = false, Order = 2)]
        [SettingPropertyGroup("General Settings")]
        public float PlayTimeMultiplier { get; set; } = 1.00f;

        [SettingPropertyFloatingInteger("Fast Forward Time Multiplier", 0.3f, 10f, HintText = FastForwardTimeMultiplier_Hint, RequireRestart = false, Order = 3)]
        [SettingPropertyGroup("General Settings")]
        public float FastForwardTimeMultiplier { get; set; } = 1.00f;

        [SettingPropertyFloatingInteger("Accelerated Aging Factor (Adults)", 1f, 10f, "#0%", HintText = AdultAgeFactor_Hint, RequireRestart = false, Order = 5)]
        [SettingPropertyGroup("General Settings")]
        public float AdultAgeFactor { get; set; } = 2f;

        [SettingPropertyFloatingInteger("Accelerated Aging Factor (Children)", 1f, 10f, "#0%", HintText = ChildAgeFactor_Hint, RequireRestart = false, Order = 6)]
        [SettingPropertyGroup("General Settings")]
        public float ChildAgeFactor { get; set; } = 2f;

        [SettingPropertyBool("Pregnancy Duration", HintText = EnablePregnancyTweaks_Hint, RequireRestart = false, IsToggle = true, Order = 0)]
        [SettingPropertyGroup("Pregnancy Duration", GroupOrder = 1)]
        public bool EnablePregnancyTweaks { get; set; } = true;

        [SettingPropertyFloatingInteger("Year-Scaled Pregnancy Duration Factor", 0.2f, 4f, "#0%", HintText = ScaledPregnancyDuration_Hint, RequireRestart = false, Order = 1)]
        [SettingPropertyGroup("Pregnancy Duration")]
        public float ScaledPregnancyDuration { get; set; } = 0.75f;

        [SettingPropertyBool("Adjust In-Progress Pregnancy Due Dates", HintText = AdjustPregnancyDueDates_Hint, RequireRestart = false, Order = 2)]
        [SettingPropertyGroup("Pregnancy Duration")]
        public bool AdjustPregnancyDueDates { get; set; } = true;

        [SettingPropertyBool("Healing Rate Auto-Calibration", HintText = EnableHealingTweaks_Hint, RequireRestart = false, IsToggle = true, Order = 0)]
        [SettingPropertyGroup("Healing Rate Auto-Calibration", GroupOrder = 2)]
        public bool EnableHealingTweaks { get; set; } = true;

        [SettingPropertyFloatingInteger("Healing Rate Adjustment Factor", 0.25f, 4f, "#0%", HintText = HealingRateAdjustmentFactor_Hint, RequireRestart = false, Order = 1)]
        [SettingPropertyGroup("Healing Rate Auto-Calibration")]
        public float HealingRateFactor { get; set; } = 1f;

        [SettingPropertyBool("Food Consumption Auto-Calibration", HintText = EnableFoodTweaks_Hint, RequireRestart = false, IsToggle = true, Order = 0)]
        [SettingPropertyGroup("Food Consumption Auto-Calibration", GroupOrder = 3)]
        public bool EnableFoodTweaks { get; set; } = true;


        [SettingPropertyBool("Age Stages", HintText = EnableAgeStageTweaks_Hint, RequireRestart = false, IsToggle = true, Order = 0)]
        [SettingPropertyGroup("Age Stages", GroupOrder = 4)]
        public bool EnableAgeStageTweaks { get; set; } = true;

        [SettingPropertyInteger("Infant Age", 1, 500, HintText = BecomeInfantAge_Hint, RequireRestart = false, Order = 1)]
        [SettingPropertyGroup("Age Stages")]
        public int BecomeInfantAge { get; set; } = 3;

        [SettingPropertyInteger("Child Age", 1, 500, HintText = BecomeChildAge_Hint, RequireRestart = false, Order = 2)]
        [SettingPropertyGroup("Age Stages")]
        public int BecomeChildAge { get; set; } = 6;

        [SettingPropertyInteger("Teenager Age", 1, 500, HintText = BecomeTeenagerAge_Hint, RequireRestart = false, Order = 3)]
        [SettingPropertyGroup("Age Stages")]
        public int BecomeTeenagerAge { get; set; } = 14;

        [SettingPropertyInteger("Adult Age", 1, 500, HintText = HeroComesOfAge_Hint, RequireRestart = false, Order = 4)]
        [SettingPropertyGroup("Age Stages")]
        public int HeroComesOfAge { get; set; } = 18;

        [SettingPropertyInteger("Old Age", 1, 500, HintText = BecomeOldAge_Hint, RequireRestart = false, Order = 5)]
        [SettingPropertyGroup("Age Stages")]
        public int BecomeOldAge { get; set; } = 47;

        [SettingPropertyInteger("Max Age", 1, 1000, HintText = MaxAge_Hint, RequireRestart = false, Order = 6)]
        [SettingPropertyGroup("Age Stages")]
        public int MaxAge { get; set; } = 128;

        [SettingPropertyInteger("Hero Starting Age", 1, 1000, HintText = HeroStartingAge_Hint, RequireRestart = false, Order = 7)]
        [SettingPropertyGroup("Age Stages")]
        public int HeroStartingAge { get; set; } = 22;

        [SettingPropertyBool("Occupation Ages", HintText = EnableAgeOccupationTweaks_Hint, RequireRestart = false, IsToggle = true, Order = 0)]
        [SettingPropertyGroup("Occupation Ages", GroupOrder = 6)]
        public bool EnableAgeOccupationTweaks { get; set; } = true;

        [SettingPropertyInteger("Tavern Wench Min Age", 1, 500, HintText = TavernWenchMinAge_Hint, RequireRestart = false, Order = 1)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernWenchMinAge { get; set; } = 20;

        [SettingPropertyInteger("Tavern Wench Max Age", 1, 500, HintText = TavernWenchMaxAge_Hint, RequireRestart = false, Order = 2)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernWenchMaxAge { get; set; } = 28;

        [SettingPropertyInteger("Tavern Visitor Min Age", 1, 500, HintText = TavernVisitorMinAge_Hint, RequireRestart = false, Order = 3)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernVisitorMinAge { get; set; } = 20;

        [SettingPropertyInteger("Tavern Visitor Max Age", 1, 500, HintText = TavernVisitorMaxAge_Hint, RequireRestart = false, Order = 4)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernVisitorMaxAge { get; set; } = 60;

        [SettingPropertyInteger("Tavern Drinker Min Age", 1, 500, HintText = TavernDrinkerMinAge_Hint, RequireRestart = false, Order = 5)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernDrinkerMinAge { get; set; } = 20;

        [SettingPropertyInteger("Tavern Drinker Max Age", 1, 500, HintText = TavernDrinkerMaxAge_Hint, RequireRestart = false, Order = 6)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernDrinkerMaxAge { get; set; } = 40;

        [SettingPropertyInteger("Slow Townsman Min Age", 1, 500, HintText = SlowTownsmanMinAge_Hint, RequireRestart = false, Order = 7)]
        [SettingPropertyGroup("Occupation Ages")]
        public int SlowTownsmanMinAge { get; set; } = 50;

        [SettingPropertyInteger("Slow Townsman Max Age", 1, 500, HintText = SlowTownsmanMaxAge_Hint, RequireRestart = false, Order = 8)]
        [SettingPropertyGroup("Occupation Ages")]
        public int SlowTownsmanMaxAge { get; set; } = 70;

        [SettingPropertyInteger("Townsfolk Carrying Stuff Min Age", 1, 500, HintText = TownsfolkCarryingStuffMinAge_Hint, RequireRestart = false, Order = 9)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TownsfolkCarryingStuffMinAge { get; set; } = 20;

        [SettingPropertyInteger("Townsfolk Carrying Stuff Max Age", 1, 500, HintText = TownsfolkCarryingStuffMaxAge_Hint, RequireRestart = false, Order = 10)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TownsfolkCarryingStuffMaxAge { get; set; } = 40;

        [SettingPropertyInteger("Brooms Woman Min Age", 1, 500, HintText = BroomsWomanMinAge_Hint, RequireRestart = false, Order = 11)]
        [SettingPropertyGroup("Occupation Ages")]
        public int BroomsWomanMinAge { get; set; } = 30;

        [SettingPropertyInteger("Brooms Woman Max Age", 1, 500, HintText = BroomsWomanMaxAge_Hint, RequireRestart = false, Order = 12)]
        [SettingPropertyGroup("Occupation Ages")]
        public int BroomsWomanMaxAge { get; set; } = 45;

        [SettingPropertyInteger("Dancer Min Age", 1, 500, HintText = DancerMinAge_Hint, RequireRestart = false, Order = 13)]
        [SettingPropertyGroup("Occupation Ages")]
        public int DancerMinAge { get; set; } = 20;

        [SettingPropertyInteger("Dancer Max Age", 1, 500, HintText = DancerMaxAge_Hint, RequireRestart = false, Order = 14)]
        [SettingPropertyGroup("Occupation Ages")]
        public int DancerMaxAge { get; set; } = 28;

        [SettingPropertyInteger("Beggar Min Age", 1, 500, HintText = BeggarMinAge_Hint, RequireRestart = false, Order = 15)]
        [SettingPropertyGroup("Occupation Ages")]
        public int BeggarMinAge { get; set; } = 60;

        [SettingPropertyInteger("Beggar Max Age", 1, 500, HintText = BeggarMaxAge_Hint, RequireRestart = false, Order = 16)]
        [SettingPropertyGroup("Occupation Ages")]
        public int BeggarMaxAge { get; set; } = 90;

        [SettingPropertyInteger("Notary Min Age", 1, 500, HintText = NotaryMinAge_Hint, RequireRestart = false, Order = 17)]
        [SettingPropertyGroup("Occupation Ages")]
        public int NotaryMinAge { get; set; } = 30;

        [SettingPropertyInteger("Notary Max Age", 1, 500, HintText = NotaryMaxAge_Hint, RequireRestart = false, Order = 18)]
        [SettingPropertyGroup("Occupation Ages")]
        public int NotaryMaxAge { get; set; } = 80;

        [SettingPropertyInteger("Barber Min Age", 1, 500, HintText = BarberMinAge_Hint, RequireRestart = false, Order = 19)]
        [SettingPropertyGroup("Occupation Ages")]
        public int BarberMinAge { get; set; } = 30;

        [SettingPropertyInteger("Barber Max Age", 1, 500, HintText = BarberMaxAge_Hint, RequireRestart = false, Order = 20)]
        [SettingPropertyGroup("Occupation Ages")]
        public int BarberMaxAge { get; set; } = 80;

        [SettingPropertyInteger("Townsfolk Min Age", -1, 500, HintText = TownsfolkMinAge_Hint, RequireRestart = false, Order = 21)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TownsfolkMinAge { get; set; } = -1;

        [SettingPropertyInteger("Townsfolk Max Age", 1, 500, HintText = TownsfolkMaxAge_Hint, RequireRestart = false, Order = 22)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TownsfolkMaxAge { get; set; } = 70;

        [SettingPropertyInteger("Tavern Game Host Min Age", 1, 500, HintText = TavernGameHostMinAge_Hint, RequireRestart = false, Order = 23)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernGameHostMinAge { get; set; } = 30;

        [SettingPropertyInteger("Tavern Game Host Max Age", 1, 500, HintText = TavernGameHostMaxAge_Hint, RequireRestart = false, Order = 24)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernGameHostMaxAge { get; set; } = 40;

        [SettingPropertyInteger("Musician Min Age", 1, 500, HintText = MusicianMinAge_Hint, RequireRestart = false, Order = 25)]
        [SettingPropertyGroup("Occupation Ages")]
        public int MusicianMinAge { get; set; } = 20;

        [SettingPropertyInteger("Musician Max Age", 1, 500, HintText = MusicianMaxAge_Hint, RequireRestart = false, Order = 26)]
        [SettingPropertyGroup("Occupation Ages")]
        public int MusicianMaxAge { get; set; } = 40;

        [SettingPropertyInteger("Arena Master Min Age", 1, 500, HintText = ArenaMasterMinAge_Hint, RequireRestart = false, Order = 27)]
        [SettingPropertyGroup("Occupation Ages")]
        public int ArenaMasterMinAge { get; set; } = 30;

        [SettingPropertyInteger("Arena Master Max Age", 1, 500, HintText = ArenaMasterMaxAge_Hint, RequireRestart = false, Order = 28)]
        [SettingPropertyGroup("Occupation Ages")]
        public int ArenaMasterMaxAge { get; set; } = 60;

        [SettingPropertyInteger("Shop Worker Min Age", 1, 500, HintText = ShopWorkerMinAge_Hint, RequireRestart = false, Order = 29)]
        [SettingPropertyGroup("Occupation Ages")]
        public int ShopWorkerMinAge { get; set; } = 18;

        [SettingPropertyInteger("Shop Worker Max Age", 1, 500, HintText = ShopWorkerMaxAge_Hint, RequireRestart = false, Order = 30)]
        [SettingPropertyGroup("Occupation Ages")]
        public int ShopWorkerMaxAge { get; set; } = 50;

        [SettingPropertyInteger("Tavernkeeper Min Age", 1, 500, HintText = TavernkeeperMinAge_Hint, RequireRestart = false, Order = 31)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernkeeperMinAge { get; set; } = 40;

        [SettingPropertyInteger("Tavernkeeper Max Age", 1, 500, HintText = TavernkeeperMaxAge_Hint, RequireRestart = false, Order = 32)]
        [SettingPropertyGroup("Occupation Ages")]
        public int TavernkeeperMaxAge { get; set; } = 80;

        [SettingPropertyInteger("Ransom Broker Min Age", 1, 500, HintText = RansomBrokerMinAge_Hint, RequireRestart = false, Order = 33)]
        [SettingPropertyGroup("Occupation Ages")]
        public int RansomBrokerMinAge { get; set; } = 30;

        [SettingPropertyInteger("Ransom Broker Max Age", 1, 500, HintText = RansomBrokerMaxAge_Hint, RequireRestart = false, Order = 34)]
        [SettingPropertyGroup("Occupation Ages")]
        public int RansomBrokerMaxAge { get; set; } = 60;

        [SettingPropertyInteger("Blacksmith Min Age", 1, 500, HintText = BlacksmithMinAge_Hint, RequireRestart = false, Order = 35)]
        [SettingPropertyGroup("Occupation Ages")]
        public int BlacksmithMinAge { get; set; } = 30;

        [SettingPropertyInteger("Blacksmith Max Age", 1, 500, HintText = BlacksmithMaxAge_Hint, RequireRestart = false, Order = 36)]
        [SettingPropertyGroup("Occupation Ages")]
        public int BlacksmithMaxAge { get; set; } = 80;

        [SettingPropertyInteger("Goods Trader Min Age", 1, 500, HintText = GoodsTraderMinAge_Hint, RequireRestart = false, Order = 37)]
        [SettingPropertyGroup("Occupation Ages")]
        public int GoodsTraderMinAge { get; set; } = 30;

        [SettingPropertyInteger("Goods Trader Max Age", 1, 500, HintText = GoodsTraderMaxAge_Hint, RequireRestart = false, Order = 38)]
        [SettingPropertyGroup("Occupation Ages")]
        public int GoodsTraderMaxAge { get; set; } = 80;

        [SettingPropertyInteger("Horse Trader Min Age", 1, 500, HintText = HorseTraderMinAge_Hint, RequireRestart = false, Order = 39)]
        [SettingPropertyGroup("Occupation Ages")]
        public int HorseTraderMinAge { get; set; } = 30;

        [SettingPropertyInteger("Horse Trader Max Age", 1, 500, HintText = HorseTraderMaxAge_Hint, RequireRestart = false, Order = 40)]
        [SettingPropertyGroup("Occupation Ages")]
        public int HorseTraderMaxAge { get; set; } = 80;

        [SettingPropertyInteger("Armorer Min Age", 1, 500, HintText = ArmorerMinAge_Hint, RequireRestart = false, Order = 41)]
        [SettingPropertyGroup("Occupation Ages")]
        public int ArmorerMinAge { get; set; } = 30;

        [SettingPropertyInteger("Armorer Max Age", 1, 500, HintText = ArmorerMaxAge_Hint, RequireRestart = false, Order = 42)]
        [SettingPropertyGroup("Occupation Ages")]
        public int ArmorerMaxAge { get; set; } = 80;

        [SettingPropertyInteger("Weaponsmith Min Age", 1, 500, HintText = WeaponsmithMinAge_Hint, RequireRestart = false, Order = 43)]
        [SettingPropertyGroup("Occupation Ages")]
        public int WeaponsmithMinAge { get; set; } = 30;

        [SettingPropertyInteger("Weaponsmith Max Age", 1, 500, HintText = WeaponsmithMaxAge_Hint, RequireRestart = false, Order = 44)]
        [SettingPropertyGroup("Occupation Ages")]
        public int WeaponsmithMaxAge { get; set; } = 80;

        [SettingPropertyInteger("Alley Gang Member Min Age", 1, 500, HintText = AlleyGangMemberMinAge_Hint, RequireRestart = false, Order = 45)]
        [SettingPropertyGroup("Occupation Ages")]
        public int AlleyGangMemberMinAge { get; set; } = 30;

        [SettingPropertyInteger("Alley Gang Member Max Age", 1, 500, HintText = AlleyGangMemberMaxAge_Hint, RequireRestart = false, Order = 46)]
        [SettingPropertyGroup("Occupation Ages")]
        public int AlleyGangMemberMaxAge { get; set; } = 40;

        public List<string> ToStringLines(uint indentSize = 0)
        {
            string prefix = string.Empty;

            for (uint i = 0; i < indentSize; ++i)
            {
                prefix += " ";
            }

            return new List<string>
            {
                $"{prefix}{nameof(DaysPerSeason)}           = {DaysPerSeason}",
                $"{prefix}{nameof(TimeMultiplier)}          = {TimeMultiplier}",
                $"{prefix}{nameof(PlayTimeMultiplier)}        = {PlayTimeMultiplier}",
                $"{prefix}{nameof(FastForwardTimeMultiplier)} = {FastForwardTimeMultiplier}",
                $"{prefix}{nameof(AdultAgeFactor)}          = {AdultAgeFactor}",
                $"{prefix}{nameof(ChildAgeFactor)}          = {ChildAgeFactor}",
                $"{prefix}{nameof(EnablePregnancyTweaks)}   = {EnablePregnancyTweaks}",
                $"{prefix}{nameof(ScaledPregnancyDuration)} = {ScaledPregnancyDuration}",
                $"{prefix}{nameof(AdjustPregnancyDueDates)} = {AdjustPregnancyDueDates}",
                $"{prefix}{nameof(EnableHealingTweaks)}     = {EnableHealingTweaks}",
                $"{prefix}{nameof(HealingRateFactor)}       = {HealingRateFactor}",
                $"{prefix}{nameof(EnableFoodTweaks)}        = {EnableFoodTweaks}",
                $"{prefix}{nameof(EnableAgeStageTweaks)}    = {EnableAgeStageTweaks}",
                $"{prefix}{nameof(BecomeInfantAge)}         = {BecomeInfantAge}",
                $"{prefix}{nameof(BecomeChildAge)}          = {BecomeChildAge}",
                $"{prefix}{nameof(BecomeTeenagerAge)}       = {BecomeTeenagerAge}",
                $"{prefix}{nameof(HeroComesOfAge)}          = {HeroComesOfAge}",
                $"{prefix}{nameof(BecomeOldAge)}            = {BecomeOldAge}",
                $"{prefix}{nameof(MaxAge)}                  = {MaxAge}",
            };
        }
    }
}
