using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using dnlib.DotNet;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TimeLord;

internal static class Program
{
    private static int _checks;
    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException("FAIL: " + message);
        _checks++;
        Console.WriteLine("PASS: " + message);
    }

    private static int Main(string[] args)
    {
        try
        {
            Metadata(args[0], args[1]);
            RuntimeDoubles();
            Console.WriteLine($"PASS: {_checks} checks. Test doubles only; no game execution/save validation.");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }

    private sealed class Target
    {
        public TypeDef? Type;
        public string? Name;
        public bool Getter;
        public void Merge(IEnumerable<CustomAttribute> attributes)
        {
            foreach (var a in attributes.Where(a => a.TypeFullName == "HarmonyLib.HarmonyPatch"))
                foreach (var arg in a.ConstructorArguments)
                {
                    if (arg.Value is TypeSig type) Type = type.ToTypeDefOrRef().ResolveTypeDef();
                    else if (arg.Type.FullName == "System.String") Name = arg.Value.ToString();
                    else if (arg.Type.FullName == "HarmonyLib.MethodType") Getter = Convert.ToInt32(arg.Value) == 1;
                }
        }
    }

    private static bool Compatible(TypeSig expected, TypeSig actual)
    {
        if (expected is ByRefSig er) expected = er.Next;
        if (actual is ByRefSig ar) actual = ar.Next;
        return expected.FullName == actual.FullName || expected.FullName == "System.Object";
    }

    private static void VerifyPatch(TypeDef targetType, string name, MethodDef patch)
    {
        var candidates = targetType.Methods.Where(m => m.Name == name).ToArray();
        Check(candidates.Length == 1, $"unique target {targetType.FullName}.{name}");
        var target = candidates.Single();
        foreach (var param in patch.Parameters.Where(p => p.IsNormalMethodParameter))
        {
            string n = param.Name;
            TypeSig? actual;
            if (n == "__state") continue;
            if (n == "__instance") actual = target.IsStatic ? null : targetType.ToTypeSig();
            else if (n == "__result") actual = target.ReturnType;
            else if (n.StartsWith("___")) actual = targetType.Fields.FirstOrDefault(f => f.Name == n.Substring(3))?.FieldSig.Type;
            else actual = target.Parameters.FirstOrDefault(p => p.Name == n)?.Type;
            Check(actual != null && Compatible(param.Type, actual), $"binding {patch.DeclaringType.Name}.{patch.Name}({n})");
        }
    }

    private static void Metadata(string modPath, string game)
    {
        var resolver = new AssemblyResolver();
        var context = new ModuleContext(resolver);
        resolver.DefaultModuleContext = context;
        resolver.PreSearchPaths.Add(Path.GetDirectoryName(modPath));
        resolver.PreSearchPaths.Add(Path.Combine(game, "bin", "Win64_Shipping_Client"));
        foreach (string name in new[] { "Native", "SandBox", "SandBoxCore", "StoryMode", "NavalDLC", "Bannerlord.Harmony", "Bannerlord.MBOptionScreen" })
            resolver.PreSearchPaths.Add(Path.Combine(game, "Modules", name, "bin", "Win64_Shipping_Client"));
        using var mod = ModuleDefMD.Load(modPath, context);
        foreach (var type in mod.GetTypes().Where(t => t.CustomAttributes.Any(a => a.TypeFullName == "HarmonyLib.HarmonyPatch")))
            foreach (var patch in type.Methods.Where(m => m.Name == "Prefix" || m.Name == "Postfix" ||
                m.CustomAttributes.Any(a => a.TypeFullName == "HarmonyLib.HarmonyPrefix" || a.TypeFullName == "HarmonyLib.HarmonyPostfix")))
            {
                var target = new Target();
                target.Merge(type.CustomAttributes);
                target.Merge(patch.CustomAttributes);
                Check(target.Type != null && target.Name != null, "resolve attributes for " + patch.FullName);
                VerifyPatch(target.Type!, (target.Getter ? "get_" : "") + target.Name, patch);
            }

        using var campaign = ModuleDefMD.Load(Path.Combine(game, "bin", "Win64_Shipping_Client", "TaleWorlds.CampaignSystem.dll"), context);
        using var missions = ModuleDefMD.Load(Path.Combine(game, "bin", "Win64_Shipping_Client", "TaleWorlds.MountAndBlade.dll"), context);
        TypeDef Find(ModuleDef module, string name) => module.GetTypes().Single(t => t.Name == name);
        MethodDef Patch(string type, string method) => Find(mod, type).Methods.Single(m => m.Name == method);
        VerifyPatch(Find(campaign, "MapTimeTracker"), "Tick", Patch("MapTimeTrackerPatch", "Tick"));
        VerifyPatch(Find(missions, "Mission"), "BuildAgent", Patch("AgentAndMissionRuntimePatch", "BuildAgent"));
        VerifyPatch(Find(missions, "Agent"), "UpdateSpawnEquipmentAndRefreshVisuals", Patch("AgentAndMissionRuntimePatch", "UpdateSpawnEquipmentAndRefreshVisuals"));
        foreach (string name in new[] { "DaysInWeek", "WeeksInSeason", "SeasonsInYear" })
            VerifyPatch(Find(campaign, "DefaultCampaignTimeModel"), "get_" + name, Patch("ForceCampaignTimeModelPatch", name));
        Check(Find(missions, "Agent").Methods.Any(m => m.Name == "SetInitialAgentScale" && m.MethodSig.Params.Count == 1 && m.MethodSig.Params[0].FullName == "System.Single"), "agent scale helper exists");
        var aging = Find(campaign, "AgingCampaignBehavior");
        Check(aging.Methods.Any(m => m.Name == "KillMainHeroWithIllness" && m.MethodSig.Params.Count == 0), "illness helper exists");
        Check(aging.Methods.Any(m => m.Name == "IsItTimeOfDeath" && m.MethodSig.Params.Count == 1), "death helper exists");
        var creationMax = Find(campaign, "CampaignEvents").Fields.Single(f => f.Name == "OnCharacterCreationIsOverEventIndexMax");
        var creationStage = Find(mod, "PostCharacterCreationBehaviour").Fields.Single(f => f.Name == "FinalCreationStage");
        Check(Convert.ToInt32(creationMax.Constant.Value) - 1 == Convert.ToInt32(creationStage.Constant.Value), "creation stage matches installed dispatch maximum");
        var after = Patch("AgingCampaignBehaviorPatch", "DailyTickHero").CustomAttributes.Single(a => a.TypeFullName == "HarmonyLib.HarmonyAfter");
        var owners = (IList<CAArgument>)after.ConstructorArguments[0].Value;
        Check(owners.Any(a => a.Value.ToString() == "vampireplayer.immortality"), "production aging follows Vampire Player owner");
        using var naval = ModuleDefMD.Load(Path.Combine(game, "Modules", "NavalDLC", "bin", "Win64_Shipping_Client", "NavalDLC.dll"), context);
        foreach (string name in new[] { "NavalDLCMobilePartyFoodConsumptionModel", "NavalDLCPartyHealingModel" })
            Check(Find(naval, name).Methods.Any(m => m.HasBody && m.Body.Instructions.Any(i => i.Operand?.ToString().Contains("::get_BaseModel()") == true)), name + " delegates to BaseModel");
        Check(!mod.GetAssemblyRefs().Any(a => a.Name == "VampirePlayer" || a.Name.ToString().StartsWith("NavalDLC")), "no new mandatory Vampire Player/Naval DLL dependency");
        Console.WriteLine("Compiled dependencies: " + string.Join("; ", mod.GetAssemblyRefs().Select(a => a.FullName)));
    }

    private static int _originalRuns, _timeLordRuns, _foreignRuns;
    private static bool _protected;
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Tick() { _originalRuns++; }
    private static bool Protect() => !_protected;
    private static bool Age() { _timeLordRuns++; return false; }
    private static void ForeignPostfix() { _foreignRuns++; }

    private static void RuntimeDoubles()
    {
        var harmony = new Harmony("timelord.validation");
        var vampire = new Harmony("vampireplayer.immortality");
        var foreign = new Harmony("timelord.validation.foreign");
        var tick = AccessTools.Method(typeof(Program), nameof(Tick));
        try
        {
            harmony.Patch(tick, prefix: new HarmonyMethod(typeof(Program), nameof(Age)) { priority = Priority.High, after = new[] { vampire.Id } });
            vampire.Patch(tick, prefix: new HarmonyMethod(typeof(Program), nameof(Protect)));
            foreign.Patch(tick, postfix: new HarmonyMethod(typeof(Program), nameof(ForeignPostfix)));
            _protected = true;
            Tick();
            Check(_timeLordRuns == 0 && _originalRuns == 0 && _foreignRuns == 1, "Vampire prefix prevents TimeLord mutation, foreign postfix retained");
            _protected = false;
            Tick();
            Check(_timeLordRuns == 1 && _originalRuns == 0, "ordinary hero retains TimeLord replacement");
            PatchSafety.Rollback(harmony, typeof(Program));
            Check(Harmony.GetPatchInfo(tick).Owners.Contains(foreign.Id) && Harmony.GetPatchInfo(tick).Owners.Contains(vampire.Id) && !Harmony.GetPatchInfo(tick).Owners.Contains(harmony.Id), "rollback preserves other owners");
            PatchSafety.PrefixOnce(harmony, tick, typeof(Program), nameof(Age));
            PatchSafety.PrefixOnce(harmony, tick, typeof(Program), nameof(Age));
            Check(Harmony.GetPatchInfo(tick).Prefixes.Count(p => p.owner == harmony.Id) == 1, "manual prefix is idempotent");
            bool rejected = false;
            try { PatchSafety.PrefixOnce(harmony, null, typeof(Program), nameof(Age)); }
            catch (MissingMethodException) { rejected = true; }
            Check(rejected, "missing manual target rejected before patching");

            harmony.CreateClassProcessor(typeof(CampaignCalendar)).Patch();
            TimeLord.Main.Settings = new Settings { DaysInWeek = 5, WeeksInSeason = 2, SeasonsInYear = 4 };
            CampaignTime.Initialize();
            Check(CampaignTime.DaysInWeek * CampaignTime.WeeksInSeason * CampaignTime.SeasonsInYear == 40, "configured calendar initialization");
            TimeLord.Main.Settings.DaysInWeek = 7;
            Check(CampaignCalendar.Days == 5, "mid-session settings cannot change calendar getter");
            CampaignTime.Initialize();
            Check(CampaignTime.DaysInWeek == 5, "repeat initialization retains active calendar");
            Campaign.Current = new Campaign();
            CampaignTime.Initialize();
            Check(CampaignTime.DaysInWeek == 7, "different campaign captures its selected settings");
            CampaignCalendar.Reset();

            CampaignEvents.Reset();
            var behavior = new PostCharacterCreationBehaviour();
            behavior.RegisterEvents();
            var manager = new CharacterCreationManager();
            CampaignEvents.OnCharacterCreationInitializedEvent.Raise(manager);
            behavior.OnCharacterCreationFinalize(manager);
            for (int i = 0; i < 9; i++) CampaignEvents.OnCharacterCreationIsOverEvent.Raise(i);
            Check(CharacterObject.PlayerCharacter.HeroObject.BirthdayWrites == 0, "no premature starting-age mutation");
            CampaignEvents.OnCharacterCreationIsOverEvent.Raise(9);
            CampaignEvents.OnCharacterCreationIsOverEvent.Raise(9);
            Check(CharacterObject.PlayerCharacter.HeroObject.BirthdayWrites == 1 && CharacterObject.PlayerCharacter.HeroObject.BirthDay.Years == -22, "starting age applied exactly once at final stage");
            CampaignEvents.Reset();
            behavior = new PostCharacterCreationBehaviour();
            behavior.RegisterEvents();
            CampaignEvents.OnGameLoadFinishedEvent.Raise();
            for (int i = 0; i < 10; i++) CampaignEvents.OnCharacterCreationIsOverEvent.Raise(i);
            Check(CharacterObject.PlayerCharacter.HeroObject.BirthdayWrites == 0, "load path does not reset existing hero birthday");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            vampire.UnpatchAll(vampire.Id);
            foreign.UnpatchAll(foreign.Id);
        }
    }
}
