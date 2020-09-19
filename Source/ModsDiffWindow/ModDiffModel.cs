using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diff;
using RimWorld;
using Verse;

namespace ModDiff
{
    public class ModModel
    {
        public string PackageId;

        public ModInfo Left = null;
        public ModInfo Right = null;
        public int LeftIndex = -1;
        public int RightIndex = -1;

        //public ChangeType Change = ChangeType.Unmodified;
        public bool IsMoved = false;
        public bool IsMissing = false;

        //public bool Selected = true;

        public string Name { get => Right?.name ?? Left?.name; }
        public bool Selected = false;

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
        public ModModel ModModel;
        public ChangeType Change;
        private bool selected = false;
        public bool Selected {
            get => selected;
        }

        public void TrySetSelected(bool value)
        {
            selected = value && !ModModel.IsMissing;
            OnSelectedChanged?.Invoke(selected);
        }

        public Action<bool> OnSelectedChanged;
    }

    public class ModDiffModel
    {
        public ModInfo[] saveMods;
        public ModInfo[] runningMods;
        public DiffListItem[] modsList;

        public DiffListItem GetModListItem(int index)
        {
            if (index == -1)
            {
                return null;
            } else
            {
                return modsList[index];
            }
        }

        //public Dictionary<string, ModInfo> saveModByPackageId;
        //public Dictionary<string, ModInfo> runningModByPackageId;

        public Dictionary<string, ModModel> modModelByPackageId;

        //public int[][] indexesMap;
        //public bool[] Activated;

        //ModModel[] editListData;

        public void CalculateDiff()
        {
            CalculateDiff(saveMods, runningMods);
        }

        public bool HaveMissingMods = false;

        private void CalculateDiff(ModInfo[] saveMods, ModInfo[] runningMods)
        {
            var diff = new Myers<string>(saveMods.Select(x => x.packageId).ToArray(), runningMods.Select(x => x.packageId).ToArray());

            diff.Compute();


            modModelByPackageId = saveMods.Select(mod => new ModModel
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

            var movedIds = diff.changeSet.Where(x => x.change == ChangeType.Removed).Select(x => x.value).ToHashSet();
            movedIds.IntersectWith(diff.changeSet.Where(x => x.change == ChangeType.Added).Select(x => x.value));

            modsList = new DiffListItem[diff.changeSet.Count];

            var editListDataUnique = new HashSet<ModModel>();

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

        internal void ResetMerge()
        {
            foreach (var change in modsList) {
                change.TrySetSelected(change.Change != ChangeType.Removed);
            }
            
        }
    }
}
