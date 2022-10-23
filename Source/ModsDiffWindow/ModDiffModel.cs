using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diff;
using ModDiff.MergeWindow;
using RimWorld;
using RWLayout.alpha2;
using Verse;

namespace ModDiff
{
    public class DiffListItem
    {
        /// <summary>
        /// Mod model
        /// </summary>
        public ModModel ModModel;
        /// <summary>
        /// Change in diff list
        /// </summary>
        public ChangeType Change;

        private bool selected = false;
        /// <summary>
        /// Item is selected
        /// </summary>
        public bool Selected {
            get => selected;
        }

        /// <summary>
        /// select item if possible
        /// </summary>
        /// <param name="value"></param>
        public bool TrySetSelected(bool value, bool force = false)
        {
            if (force)
            {
                selected = value;
            }
            else
            {
                selected = (value && !ModModel.IsMissing) || ModModel.IsRequired;
            }

            OnSelectedChanged?.Invoke(selected);
            return selected == value;
        }

        /// <summary>
        /// item was selected callback
        /// </summary>
        public Action<bool> OnSelectedChanged;
    }

    class IdentityComparer : EqualityComparer<ModInfo>
    {
        public override bool Equals(ModInfo x, ModInfo y) => x.PackageId == y.PackageId;
        public override int GetHashCode(ModInfo obj) => obj.PackageId.GetHashCode();
    }

    class NormalizedComparer : EqualityComparer<ModInfo>
    {
        public override bool Equals(ModInfo x, ModInfo y) => x.NormalizedId == y.NormalizedId;
        public override int GetHashCode(ModInfo obj) => obj.NormalizedId.GetHashCode();
    }

    public class ModDiffModel
    {
        /// <summary>
        /// mods info from save
        /// </summary>
        public ModInfo[] saveMods;
        /// <summary>
        /// mods info loaded by game
        /// </summary>
        public ModInfo[] runningMods;

        /// <summary>
        /// merged list
        /// </summary>
        public DiffListItem[] modsList;

        /// <summary>
        /// model for merge mods window
        /// </summary>
        internal MergeListDataSource MergeListDataSource;


       


        public void CalculateDiff()
        {
            CalculateDiff(saveMods, runningMods);

            MergeListDataSource = new MergeListDataSource();
            MergeListDataSource.ModDiffModel = this;
        }

        public bool HaveMissingMods = false;

        private void CalculateDiff(ModInfo[] saveMods, ModInfo[] runningMods)
        {
            var diff = new Myers<ModInfo>(saveMods, runningMods);

            diff.Compute();

            HashSet<string> requiredIds = new HashSet<string>();
            requiredIds.Add(coreMod);

            if (ModDiff.Settings.selfPreservation)
            {
                requiredIds.AddRange(requiredMods);
                requiredIds.Add(ModDiff.PackageIdOfMine);
            }

            // searching moved mods
            // dictionary of Mod -> old position
            var left = new Dictionary<ModInfo, int>();
            // dictionary of Mod -> new position
            var right = new Dictionary<ModInfo, int>();
            
            for (int i = 0; i < diff.changeSet.Count; i++)
            {
                var entry = diff.changeSet[i];
                if (entry.change == ChangeType.Removed)
                {
                    left[entry.left] = i;
                }
                if (entry.change == ChangeType.Added)
                {
                    right[entry.right] = i;
                }
            }

            // pairs old position -> new position
            List<(int left, int right)> moved = new List<(int left, int right)>();
            foreach (var kvp in left)
            {
                if (right.TryGetValue(kvp.Key, out var rVal))
                {
                    moved.Add((kvp.Value, rVal));
                    Log.Message($"found moved {kvp.Value} to {rVal}");
                }
            }

            modsList = new DiffListItem[diff.changeSet.Count];

            // bulding item models
            for (int i = 0; i < diff.changeSet.Count; i++)
            {
                var modChange = diff.changeSet[i];
                var modInfo = modChange.value;


                var modModel = new ModModel
                {
                    PackageId = modChange.value.PackageId,
                    NormalizedId = modChange.value.NormalizedId,
                    Left = modChange.left,
                    Right = modChange.right,
                };

                modModel.IsRequired = requiredIds.Contains(modModel.NormalizedId);

                var change = new DiffListItem
                {
                    ModModel = modModel,
                    Change = modChange.change,
                };
                
                modsList[i] = change;
                change.TrySetSelected(modChange.change != ChangeType.Removed, true);

                if (modChange.change != ChangeType.Added)
                {
                    modModel.LeftIndex = i;
                }
                if (modChange.change != ChangeType.Removed)
                {
                    modModel.RightIndex = i;
                }

                if (ModLister.GetModWithIdentifier(modInfo.PackageId, false) == null)
                {
                    modModel.IsMissing = true;
                    HaveMissingMods = true;
                }


            }

            // linking mods moved inside the list
            foreach (var indices in moved)
            {
                modsList[indices.left].ModModel.IsMoved = true;
                modsList[indices.right].ModModel.IsMoved = true;
                modsList[indices.left].ModModel.RightIndex = indices.right;
                modsList[indices.right].ModModel.RightIndex = indices.left;
            }

            for (int i = 0; i < diff.changeSet.Count; i++)
            {
                Log.Message($"PackageId: {modsList[i].ModModel.PackageId}; leftIndex: {modsList[i].ModModel.LeftIndex}; rightIndex: {modsList[i].ModModel.RightIndex}");
            }
        }


        internal bool MergedListIsValid()
        {
            var selectedMods = modsList.Where(mod => mod.Selected).Select(mod => mod.ModModel.NormalizedId).ToHashSet();

            return
                requiredMods.All(x => selectedMods.Contains(x)) &&
                modsList.Select(mod => mod.ModModel.PackageId).Contains(ModDiff.PackageIdOfMine);
        }


        static string coreMod = "ludeon.rimworld";
        static string harmonyId = "brrainz.harmony";
        static string[] requiredMods = {
            harmonyId, // Harmony
            //"name.krypt.rimworld.rwlayout.alpha2.dev", // RWLayout
        };

        public void TrySetActiveModsFromSamegame()
        {
            var loadedModIdsList = new List<string>(ScribeMetaHeaderUtility.loadedModIdsList);
            TrySetActiveMods(loadedModIdsList, true);
        }

        public void TrySetActiveModsFromMerge()
        {
            var editedList = modsList.Where(mod => mod.Selected).Select(mod => mod.ModModel.PackageId).ToList();
            TrySetActiveMods(editedList, false);
        }

        private void TrySetActiveMods(List<string> activeMods, bool insertSelfIfNeeded = false)
        {
            if (insertSelfIfNeeded && ModDiff.Settings.selfPreservation)
            {
                int index = -1;
                int indexToInsert = 0;
                foreach (var modId in requiredMods/*.Concat(ModDiff.Settings.LockedMods)*/)
                {
                    index = activeMods.IndexOf(modId);
                    if (index == -1)
                    {
                        activeMods.Insert(indexToInsert, modId);
                        index = indexToInsert;
                    }
                    indexToInsert = index + 1;
                }
                if (!activeMods.Contains(ModDiff.PackageIdOfMine))
                {
                    activeMods.Insert(indexToInsert, ModDiff.PackageIdOfMine);
                }                
            }

            if (Current.ProgramState == ProgramState.Entry)
            {
                ModsConfig.SetActiveToList(activeMods);
            }
            ModsConfig.SaveFromList(activeMods);

            ModsConfig.RestartFromChangedMods();
        }


        /// <summary>
        /// reset selection
        /// </summary>
        internal void UseLeftList()
        {
            foreach (var change in modsList) {
                change.TrySetSelected(
                    (change.Change != ChangeType.Added && !change.ModModel.IsMissing) ||
                    (!change.ModModel.IsMoved && change.ModModel.IsRequired) 
                    , true);
            }            
        }

        internal void UseRightList()
        {
            foreach (var change in modsList)
            {
                change.TrySetSelected(change.Change != ChangeType.Removed, true);
            }
        }

        internal bool TrySetSelected(MergeListRow row, bool select)
        {

            if (!select)
            {
                if (row.Item.Selected)
                {
                    return row.Item.TrySetSelected(false);                    
                }
            }
            else
            {
                var left = row.Model.LeftIndex;
                var right = row.Model.RightIndex;

                if (!row.Item.Selected)
                {
                    if (row.Model.IsMoved)
                    {
                        Log.Message($"remove left {left}");
                        if (left != -1)
                        {
                            modsList[left].TrySetSelected(false, true);
                        }
                        Log.Message($"remove right {left}");
                        if (right != -1)
                        {
                            modsList[right].TrySetSelected(false, true);
                        }
                    }
                    return row.Item.TrySetSelected(true);
                }
            }

            return false;
        }
    }
}
