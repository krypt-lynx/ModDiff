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
        private static bool MismatchDialogPrefix(Action confirmedAction, ref bool __result)
        {
            __result = ScribeMetaHeaderUtility_Patch.TryCreateDialogsForVersionMismatchWarnings(confirmedAction);
            return false;
        }
    }

    public static class ScribeMetaHeaderUtility_Patch
    {

        private static bool ScribeMetaHeaderUtility_VersionsMatch()
        {
            var method = typeof(ScribeMetaHeaderUtility).GetMethod("VersionsMatch", BindingFlags.NonPublic | BindingFlags.Static);
            return (bool)method.Invoke(null, Array.Empty<object>());
        }

        private static ScribeMetaHeaderUtility.ScribeHeaderMode ScribeMetaHeaderUtility_LastMode()
        {
            var field = typeof(ScribeMetaHeaderUtility).GetField("lastMode", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Static);
            return (ScribeMetaHeaderUtility.ScribeHeaderMode)field.GetValue(null);
        }

        public static bool TryCreateDialogsForVersionMismatchWarnings(Action confirmedAction)
        {
            string message = null;
            string title = null;

            bool matchingVersion = CheckMatchingVersion(ref message, ref title);

            if (!matchingVersion)
            {
                bool matchingMods = CheckMatchingMods(ref message, ref title);

                if (message != null)
                {
                    CreateDefaultDialog(confirmedAction, message, title, matchingMods);
                    return true;
                }
            }
            else
            {
                if (!LoadedModsMatchesActiveMods())
                {
                    CreateModDiffDialog(confirmedAction);
                    return true;
                }
            }
            return false;
        }

        private static void CreateModDiffDialog(Action confirmedAction)
        {
            var runningMods = LoadedModManager.RunningMods.Select(mod => new ModInfo { packageId = mod.PackageId, name = mod.Name }).ToArray();
            var saveMods = Enumerable.Zip(ScribeMetaHeaderUtility.loadedModIdsList, ScribeMetaHeaderUtility.loadedModNamesList, (modId, modMame) => new ModInfo { packageId = modId, name = modMame }).ToArray();

            var diffWindow = new ModsDiffWindow(saveMods, runningMods, confirmedAction);

            Find.WindowStack.Add(diffWindow);
        }

        private static void CreateDefaultDialog(Action confirmedAction, string message, string title, bool matchingMods)
        {
            Dialog_MessageBox dialog = Dialog_MessageBox.CreateConfirmation(message, confirmedAction, false, title);
            dialog.buttonAText = "LoadAnyway".Translate();
            if (!matchingMods)
            {
                dialog.buttonCText = "ChangeLoadedMods".Translate();
                dialog.buttonCAction = delegate ()
                {
                    if (Current.ProgramState == ProgramState.Entry)
                    {
                        ModsConfig.SetActiveToList(ScribeMetaHeaderUtility.loadedModIdsList);
                    }
                    ModsConfig.SaveFromList(ScribeMetaHeaderUtility.loadedModIdsList);

                    IEnumerable<string> enumerable = Enumerable
                        .Range(0, ScribeMetaHeaderUtility.loadedModIdsList.Count)
                        .Where((int id) => ModLister.GetModWithIdentifier(ScribeMetaHeaderUtility.loadedModIdsList[id], false) == null)
                        .Select((int id) => ScribeMetaHeaderUtility.loadedModNamesList[id]);

                    if (enumerable.Any<string>())
                    {
                        Messages.Message(string.Format("{0}: {1}", "MissingMods".Translate(), enumerable.ToCommaList(false)), MessageTypeDefOf.RejectInput, false);
                        dialog.buttonCClose = false;
                    }
                    ModsConfig.RestartFromChangedMods();
                };
            }
            Find.WindowStack.Add(dialog);
        }

        private static bool CheckMatchingMods(ref string message, ref string title)
        {
            bool matchingMods = true;
            string loadedModsSummary = "";
            string runningModsSummary = "";
            if (!LoadedModsMatchesActiveMods(out loadedModsSummary, out runningModsSummary))
            {
                matchingMods = false;
                string mismatchMessage = "ModsMismatchWarningText".Translate(loadedModsSummary, runningModsSummary);
                if (message == null)
                {
                    message = mismatchMessage;
                }
                else
                {
                    message = message + "\n\n" + mismatchMessage;
                }
                if (title == null)
                {
                    title = "ModsMismatchWarningTitle".Translate();
                }
            }

            return matchingMods;
        }

        private static bool CheckMatchingVersion(ref string message, ref string title)
        {
            bool matchingVersion = true;
            if (!BackCompatibility.IsSaveCompatibleWith(ScribeMetaHeaderUtility.loadedGameVersion) && !/*ScribeMetaHeaderUtility.*/ScribeMetaHeaderUtility_VersionsMatch())
            {
                matchingVersion = false;

                title = "VersionMismatch".Translate();
                string saveVersion = ScribeMetaHeaderUtility.loadedGameVersion.NullOrEmpty() ? ("(" + "UnknownLower".TranslateSimple() + ")") : ScribeMetaHeaderUtility.loadedGameVersion;

                switch (ScribeMetaHeaderUtility_LastMode())
                {
                    case ScribeMetaHeaderUtility.ScribeHeaderMode.Map:
                        message = "SaveGameIncompatibleWarningText".Translate(saveVersion, VersionControl.CurrentVersionString);
                        break;
                    case ScribeMetaHeaderUtility.ScribeHeaderMode.World:
                        message = "WorldFileVersionMismatch".Translate(saveVersion, VersionControl.CurrentVersionString);
                        break;
                    default:
                        message = "FileIncompatibleWarning".Translate(saveVersion, VersionControl.CurrentVersionString);
                        break;
                }
            }

            return matchingVersion;
        }

        public static bool LoadedModsMatchesActiveMods()
        {
            var runningIds = LoadedModManager.RunningMods.Select(mod => mod.PackageId);
            var saveIds = ScribeMetaHeaderUtility.loadedModIdsList.AsEnumerable();

            if (false) // todo: option
            {
                runningIds = runningIds.Without(ModDiff.packageIdOfMine);
                saveIds = saveIds.Without(ModDiff.packageIdOfMine);
            }

            return runningIds.Count() == saveIds.Count() && // todo: performance
                runningIds.SequenceEqual(saveIds);
        }

        public static bool LoadedModsMatchesActiveMods(out string loadedModsSummary, out string runningModsSummary)
        {
            loadedModsSummary = null;
            runningModsSummary = null;
            List<string> list = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageId).ToList<string>();
            List<string> b = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.FolderName).ToList<string>();
            if (ScribeMetaHeaderUtility_ModListsMatch(ScribeMetaHeaderUtility.loadedModIdsList, list) ||
                ScribeMetaHeaderUtility_ModListsMatch(ScribeMetaHeaderUtility.loadedModIdsList, b))
            {
                return true;
            }
            if (ScribeMetaHeaderUtility.loadedModNamesList == null)
            {
                loadedModsSummary = "None".Translate();
            }
            else
            {
                loadedModsSummary = ScribeMetaHeaderUtility.loadedModNamesList.ToCommaList(false);
            }
            runningModsSummary = list.Select((string id) => ModLister.GetModWithIdentifier(id, false).Name).ToCommaList(false);
            return false;
        }

        private static bool ScribeMetaHeaderUtility_ModListsMatch(List<string> a, List<string> b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            if (a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}
