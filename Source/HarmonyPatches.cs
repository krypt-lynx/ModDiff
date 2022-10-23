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
using RWLayout.alpha2.FastAccess;
using System.Reflection.Emit;
using HarmonyLib;
using System.IO;
namespace ModDiff
{
    /*
    public static class HarmonyPatches
    {
        // To hell with this. It is unstable.
        enum TranspilerState
        {
            beforeActiveModsCall,
            beforeFindWindowStack,
            beforeWindowStackAdd,
            patched,
        }

        internal static IEnumerable<CodeInstruction> TryCreateDialogsForVersionMismatchWarnings_transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
           var state = TranspilerState.beforeActiveModsCall;

            MethodInfo LoadedModsMatchesActiveModsMethod = typeof(ScribeMetaHeaderUtility).GetMethod(nameof(ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods), BindingFlags.Static | BindingFlags.Public);
            MethodInfo FindGet_WindowStack = typeof(Find).GetProperty(nameof(Find.WindowStack), BindingFlags.Static | BindingFlags.Public).GetGetMethod();
            MethodInfo WindowStackAdd = typeof(WindowStack).GetMethod(nameof(WindowStack.Add), BindingFlags.Instance | BindingFlags.Public);
      
            MethodInfo Patch_LoadedModsMatchesActiveModsMethod = typeof(ScribeMetaHeaderUtility_PatchTools).GetMethod(nameof(ScribeMetaHeaderUtility_PatchTools.LoadedModsMatchesActiveMods), BindingFlags.Static | BindingFlags.Public);
            MethodInfo Patch_CreateModDiffDialog = typeof(ScribeMetaHeaderUtility_PatchTools).GetMethod(nameof(ScribeMetaHeaderUtility_PatchTools.CreateModDiffDialog), BindingFlags.Static | BindingFlags.Public);


            foreach (var op in instructions)
            {
                switch (state)
                {
                    case TranspilerState.beforeActiveModsCall:
                        {
                            if (op.opcode == OpCodes.Call &&
                                (MethodInfo)op.operand == LoadedModsMatchesActiveModsMethod)
                            {
                                yield return new CodeInstruction(OpCodes.Call, Patch_LoadedModsMatchesActiveModsMethod);
                                state = TranspilerState.beforeFindWindowStack;
                            }
                            else
                            {
                                yield return op;
                            }
                        } break;
                    case TranspilerState.beforeFindWindowStack:
                        {
                            if (op.opcode == OpCodes.Call &&
                                (MethodInfo)op.operand == FindGet_WindowStack)
                            {
                                state = TranspilerState.beforeWindowStackAdd;                                
                            }
                            else
                            {
                                yield return op;
                            }
                        } break;
                    case TranspilerState.beforeWindowStackAdd:
                        {
                            if (op.opcode == OpCodes.Callvirt &&
                                (MethodInfo)op.operand == WindowStackAdd)
                            {
                                yield return new CodeInstruction(OpCodes.Ldarg_0);
                                yield return new CodeInstruction(OpCodes.Call, Patch_CreateModDiffDialog);

                                state = TranspilerState.patched;
                            }
                        } break;
                    case TranspilerState.patched:
                        {
                            yield return op;
                        } break;
                }                

            }            
        }
    }
     */
            
    public static class HarmonyPatches
    {
        static Func<bool> scribeMetaHeaderUtility_VersionsMatch = Dynamic.StaticRetMethod<ScribeMetaHeaderUtility, bool>("VersionsMatch");
        static Func<ScribeMetaHeaderUtility.ScribeHeaderMode> scribeMetaHeaderUtility_lastMode = Dynamic.StaticGetField<ScribeMetaHeaderUtility, ScribeMetaHeaderUtility.ScribeHeaderMode>("lastMode");

        // Token: 0x06001B13 RID: 6931 RVA: 0x0013D2B4 File Offset: 0x0013B4B4
        public static bool TryCreateDialogsForVersionMismatchWarnings(Action confirmedAction)
        {
            //Verse.Log.Message("patched TryCreateDialogsForVersionMismatchWarnings");
            string text = null;
            string title = null;
            if (!BackCompatibility.IsSaveCompatibleWith(ScribeMetaHeaderUtility.loadedGameVersion) && !scribeMetaHeaderUtility_VersionsMatch())
            {
                title = "VersionMismatch".Translate();
                string value = (ScribeMetaHeaderUtility.loadedGameVersion.NullOrEmpty() ? ("(" + "UnknownLower".TranslateSimple() + ")") : ScribeMetaHeaderUtility.loadedGameVersion);
                if (scribeMetaHeaderUtility_lastMode() == ScribeMetaHeaderUtility.ScribeHeaderMode.Map)
                {
                    text = "SaveGameIncompatibleWarningText".Translate(value, VersionControl.CurrentVersionString);
                }
                else if (scribeMetaHeaderUtility_lastMode() == ScribeMetaHeaderUtility.ScribeHeaderMode.World)
                {
                    text = "WorldFileVersionMismatch".Translate(value, VersionControl.CurrentVersionString);
                }
                else
                {
                    text = "FileIncompatibleWarning".Translate(value, VersionControl.CurrentVersionString);
                }
            }

            if (!ScribeMetaHeaderUtility_PatchTools.LoadedModsMatchesActiveMods(out var str1, out var str2))
            {
                ScribeMetaHeaderUtility_PatchTools.CreateModDiffDialog(confirmedAction);

                return true;
            }
            if (text != null)
            {
                Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation(text, confirmedAction, false, title);
                dialog_MessageBox.buttonAText = "LoadAnyway".Translate();
                Find.WindowStack.Add(dialog_MessageBox);
                return true;
            }
            return false;
        }

        public static bool TryCreateDialogsForVersionMismatchWarnings_Prefix(Action confirmedAction, ref bool __result)
        {
            __result = TryCreateDialogsForVersionMismatchWarnings(confirmedAction);
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
            var runningMods = LoadedModManager.RunningMods
                .Select(mod => new ModInfo(mod.Name, mod.PackageId)).ToArray();
            var saveMods = Enumerable.Zip(
                    ScribeMetaHeaderUtility.loadedModIdsList,
                    ScribeMetaHeaderUtility.loadedModNamesList,
                    (modId, modName) => new ModInfo(modName, modId)
                ).ToArray();
            
            var diffWindow = new ModDiffWindow(saveMods, runningMods, confirmedAction);
            Find.WindowStack.Add(diffWindow);
        }

        public static bool LoadedModsMatchesActiveMods(out string loadedModsSummary, out string runningModsSummary)
        {
            loadedModsSummary = "";
            runningModsSummary = "";


            var runningIds = LoadedModManager.RunningMods.Select(mod => mod.PackageId);
            var saveIds = ScribeMetaHeaderUtility.loadedModIdsList.AsEnumerable();

            if (ModDiff.Settings.steamSameAsLocal)
            {
                runningIds = runningIds.Select(x => x.Split('_').FirstOrFallback(""));
                saveIds = saveIds.Select(x => x.Split('_').FirstOrFallback(""));
            }

            if (ModDiff.Settings.ignoreSelf) 
            {
                runningIds = runningIds.Without(ModDiff.PackageIdOfMine);
                saveIds = saveIds.Without(ModDiff.PackageIdOfMine);
            }

            return runningIds.SequenceEqual(saveIds);
        }

    }
}
