using System;

using HarmonyLib;

namespace TimeLord.Patches
{
    public interface IOptionalPatch
    {
        public bool TryPatch(Harmony harmony);

        public bool MenusInitialised(Harmony harmony);

        public Func<Harmony,bool>? DelayedPatch();
    }
}
