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
    public class ModModel
    {
        /// <summary>
        /// mod's package id
        /// </summary>
        public string PackageId;

        /// <summary>
        /// mod info stored if the save
        /// </summary>
        public ModInfo Left = null;
        /// <summary>
        /// mod info loaded by game
        /// </summary>
        public ModInfo Right = null;

        /// <summary>
        /// index of left entry in merged list
        /// </summary>
        public int LeftIndex = -1;
        /// <summary>
        /// index of right entry in merged list
        /// </summary>
        public int RightIndex = -1;

        /// <summary>
        /// mod was moved
        /// </summary>
        public bool IsMoved = false;
        /// <summary>
        /// mod is no available
        /// </summary>
        public bool IsMissing = false;

        public bool IsRequired = false;

        /// <summary>
        /// Name for item in merged list
        /// </summary>
        public string Name { get => Right?.name ?? Left?.name; }

        public override int GetHashCode()
        {
            return PackageId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ModModel other)
            {
                return PackageId.Equals(other.PackageId);
            }
            else
            {
                return false;
            }
        }
    }

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
            // merging/calculeting diffs lists 
            var diff = new Myers<string>(saveMods.Select(x => x.packageId).ToArray(), runningMods.Select(x => x.packageId).ToArray());

            diff.Compute();

            // mod info by packageId for fast access
            var modModelByPackageId = saveMods.Select(mod => new ModModel
            {
                PackageId = mod.packageId,
                Left = mod
            }).ToDictionary(x => x.PackageId);

            foreach (var mod in runningMods)
            {
                if (modModelByPackageId.ContainsKey(mod.packageId))
                {
                    modModelByPackageId[mod.packageId].Right = mod;
                }
                else
                {
                    modModelByPackageId[mod.packageId] = new ModModel
                    {
                        PackageId = mod.packageId,
                        Right = mod
                    };                    
                }
            }

            HashSet<string> requiredIds = new HashSet<string>();
            requiredIds.Add(coreMod);
            //requiredIds.AddRange(ModDiff.Settings.LockedMods);
            if (ModDiff.Settings.selfPreservation)
            {
                requiredIds.AddRange(requiredMods);
                requiredIds.Add(ModDiff.PackageIdOfMine);
            }

            // searching moved mods
            var movedIds = diff.changeSet.Where(x => x.change == ChangeType.Removed).Select(x => x.value).ToHashSet();
            movedIds.IntersectWith(diff.changeSet.Where(x => x.change == ChangeType.Added).Select(x => x.value));

            modsList = new DiffListItem[diff.changeSet.Count];

            // bulding item models
            for (int i = 0; i < diff.changeSet.Count; i++)
            {
                var packageIdChange = diff.changeSet[i];
                var packageId = packageIdChange.value;
                var modModel = modModelByPackageId[packageId];
                modModel.IsRequired = requiredIds.Contains(packageId);

                var change = new DiffListItem
                {
                    ModModel = modModel,
                    Change = packageIdChange.change,
                };
                
                modsList[i] = change;
                change.TrySetSelected(packageIdChange.change != ChangeType.Removed, true);

                if (packageIdChange.change != ChangeType.Added)
                {
                    modModel.LeftIndex = i;
                }
                if (packageIdChange.change != ChangeType.Removed)
                {
                    modModel.RightIndex = i;
                }

                if (movedIds.Contains(packageId))
                {
                    modModel.IsMoved = true;
                }

                if (ModLister.GetModWithIdentifier(packageId, false) == null)
                {
                    modModel.IsMissing = true;
                    HaveMissingMods = true;
                }
            }
        }


        internal bool MergedListIsValid()
        {
            var selectedMods = modsList.Where(mod => mod.Selected).Select(mod => mod.ModModel.PackageId).ToHashSet();

            return
                requiredMods.All(x => selectedMods.Contains(x)) &&
                selectedMods.Contains(ModDiff.PackageIdOfMine);
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
                        if (left != -1)
                        {
                            modsList[left].TrySetSelected(false, true);
                        }
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
