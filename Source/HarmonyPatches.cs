using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using LinqExtansions;
using RWLayout.alpha2;

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

    static class WindowStackAddPatches
    {
        static int inOnGuiCounter = 0;
        static Queue<Window> windowsQueue = new Queue<Window>();

        static bool WindowStack_Add_prefix(Window window)
        {
            if (inOnGuiCounter == 0 && NeedToInvoke(window))
            {
                //Log.Message($"window {window.GetType().Name} queued");
                windowsQueue.Enqueue(window);
                return false;
            }
            else
            {
                //Log.Message($"window {window.GetType().Name} added");
                return true;
            }
        }

        private static bool NeedToInvoke(Window window)
        {
            if (window is IWindow wnd)
            {
                return wnd.ForceOnGUI;
            }
            return false;
        }

        static void UIRoot_UIRootOnGUI_prefix()
        {
            inOnGuiCounter++;
            while (windowsQueue.Count > 0)
            {
                Find.WindowStack.Add(windowsQueue.Dequeue());
            }
        }

        static void UIRoot_UIRootOnGUI_postfix()
        {
            inOnGuiCounter--;
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


            var diffWindow = new ModDiffWindow(saveMods, runningMods, confirmedAction);
            Find.WindowStack.Add(diffWindow);

        }

        public static bool LoadedModsMatchesActiveMods()
        {
            var runningIds = LoadedModManager.RunningMods.Select(mod => mod.PackageId);
            var saveIds = ScribeMetaHeaderUtility.loadedModIdsList.AsEnumerable();

            if (ModDiff.Settings.ignoreSelf) 
            {
                runningIds = runningIds.Without(ModDiff.PackageIdOfMine);
                saveIds = saveIds.Without(ModDiff.PackageIdOfMine);
            }

            return runningIds.SequenceEqual(saveIds);
        }

    }
}
