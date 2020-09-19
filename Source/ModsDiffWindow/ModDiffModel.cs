using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diff;
using ModDiff.MergeWindow;
using RimWorld;
using RWLayout.moddiff;
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

        /// <summary>
        /// Name for item in m,erged list
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
        public void TrySetSelected(bool value)
        {
            selected = value && !ModModel.IsMissing;

            OnSelectedChanged?.Invoke(selected);
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

                var change = new DiffListItem
                {
                    ModModel = modModel,
                    Change = packageIdChange.change,
                };
                
                modsList[i] = change;
                change.TrySetSelected(packageIdChange.change != ChangeType.Removed);

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


        static string harmonyId = "brrainz.harmony";

        public void TrySetActiveMods()
        {
            var loadedModIdsList = new List<string>(ScribeMetaHeaderUtility.loadedModIdsList);

            if (ModDiff.Settings.selfPreservation && !loadedModIdsList.Contains(ModDiff.PackageIdOfMine))
            {
                
                var index = loadedModIdsList.IndexOf(harmonyId);

                if (index != -1)
                {
                    loadedModIdsList.Insert(index + 1, ModDiff.PackageIdOfMine);
                }
                else
                {
                    loadedModIdsList.Insert(0, harmonyId);
                    loadedModIdsList.Insert(1, ModDiff.PackageIdOfMine);
                }
            }

            if (Current.ProgramState == ProgramState.Entry)
            {
                ModsConfig.SetActiveToList(loadedModIdsList);
            }
            ModsConfig.SaveFromList(loadedModIdsList);

            // "MissingMods".Translate(),
            /*IEnumerable<string> enumerable = Enumerable
                .Range(0, ScribeMetaHeaderUtility.loadedModIdsList.Count)
                .Where((int id) => ModLister.GetModWithIdentifier(ScribeMetaHeaderUtility.loadedModIdsList[id], false) == null)
                .Select((int id) => ScribeMetaHeaderUtility.loadedModNamesList[id]);
            */

            ModsConfig.RestartFromChangedMods();
        }

        /// <summary>
        /// reset selection
        /// </summary>
        internal void ResetMerge()
        {
            foreach (var change in modsList) {
                change.TrySetSelected(change.Change != ChangeType.Removed);
            }            
        }


        public void DoBGThings() // cheating a bit to hide edit window lag :(
        {
            MergeListDataSource.GenNextItem();
        }
    }
}
