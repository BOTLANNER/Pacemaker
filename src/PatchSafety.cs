using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace TimeLord
{
    internal static class PatchSafety
    {
        internal static IEnumerable<Type> LoadableTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e)
            {
                Util.Log.ToFile($"TimeLord: partial type discovery in {assembly.FullName}: {e.Message}");
                return e.Types.Where(t => t != null).Cast<Type>();
            }
            catch (Exception e)
            {
                Util.Log.ToFile($"TimeLord: cannot inspect {assembly.FullName}: {e.Message}");
                return Array.Empty<Type>();
            }
        }

        // Remove only this class's patches, never another mod's or another TimeLord feature's.
        internal static void Rollback(Harmony harmony, Type patchClass)
        {
            foreach (var target in Harmony.GetAllPatchedMethods().ToArray())
            {
                var info = Harmony.GetPatchInfo(target);
                if (info == null) continue;
                foreach (var patch in info.Prefixes.Concat(info.Postfixes).Concat(info.Transpilers).Concat(info.Finalizers)
                    .Where(p => p.owner == harmony.Id && p.PatchMethod.DeclaringType == patchClass).ToArray())
                    harmony.Unpatch(target, patch.PatchMethod);
            }
        }

        internal static void PrefixOnce(Harmony harmony, MethodInfo? target, Type patchClass, string patchName)
        {
            if (target == null || target.IsAbstract || target.ContainsGenericParameters)
                throw new MissingMethodException($"TimeLord: invalid target for {patchClass.Name}.{patchName}");
            var patch = AccessTools.Method(patchClass, patchName);
            if (Harmony.GetPatchInfo(target)?.Prefixes.Any(p => p.owner == harmony.Id && p.PatchMethod == patch) == true)
                return;
            harmony.Patch(target, prefix: new HarmonyMethod(patch));
        }
    }
}
