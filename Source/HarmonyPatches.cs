// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using LinqExtansions;

namespace ModDiff
{
    public static class HarmonyPatches
    {
        public static bool TryCreateDialogsForVersionMismatchWarnings_Prefix(Action confirmedAction, ref bool __result)
        {


            if (!BackCompatibility.IsSaveCompatibleWith(ScribeMetaHeaderUtility.loadedGameVersion) && !ScribeMetaHeaderUtility_PatchTools.ScribeMetaHeaderUtility_VersionsMatch())
            {
                return true;
            }

            if (!ScribeMetaHeaderUtility_PatchTools.LoadedModsMatchesActiveMods())
            {
                ScribeMetaHeaderUtility_PatchTools.CreateModDiffDialog(confirmedAction);
                __result = true;
            }
            else
            {
                __result = false;
            }

            return false;
        }
    }

    public static class ScribeMetaHeaderUtility_PatchTools
    {
        public static bool ScribeMetaHeaderUtility_VersionsMatch()
        {
            var method = typeof(ScribeMetaHeaderUtility).GetMethod("VersionsMatch", BindingFlags.NonPublic | BindingFlags.Static);
            return (bool)method.Invoke(null, Array.Empty<object>());
        }

        public static void CreateModDiffDialog(Action confirmedAction)
        {
            var runningMods = LoadedModManager.RunningMods.Select(mod => new ModInfo { packageId = mod.PackageId, name = mod.Name }).ToArray();
            var saveMods = Enumerable.Zip(ScribeMetaHeaderUtility.loadedModIdsList, ScribeMetaHeaderUtility.loadedModNamesList, (modId, modMame) => new ModInfo { packageId = modId, name = modMame }).ToArray();


            var diffWindow = new ModsDiffWindow(saveMods, runningMods, confirmedAction);
            Find.WindowStack.Add(diffWindow);

        }

        public static bool LoadedModsMatchesActiveMods()
        {
            var runningIds = LoadedModManager.RunningMods.Select(mod => mod.PackageId);
            var saveIds = ScribeMetaHeaderUtility.loadedModIdsList.AsEnumerable();

            if (ModDiff.settings.ignoreSelf) // todo: option
            {
                runningIds = runningIds.Without(ModDiff.packageIdOfMine);
                saveIds = saveIds.Without(ModDiff.packageIdOfMine);
            }

            return runningIds.SequenceEqual(saveIds);
        }

    }
}
