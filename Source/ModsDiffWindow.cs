using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Diff;
using RimWorld;

namespace ModDiff
{
    public class ModInfo
    {
        public string packageId;
        public string name;

        public override bool Equals(object obj)
        {
            if (!(obj is ModInfo))
            {
                return false;
            }
            var modInfo = (ModInfo)obj;

            return packageId == modInfo.packageId;
        }

        public override int GetHashCode()
        {
            return packageId.GetHashCode();
        }

        public override string ToString()
        {
            return name ?? "<none>";
        }
    }

    public class ModsDiffWindow : Window
    {
        string message;
        string cancelBtnText;
        string reloadBtnText;
        string continueBtnText;

        List<Change<ModInfo>> info;
        Action confirmedAction;

        const int markerWidth = 16;
        const int vSpace = 8;

        private Vector2 scrollPosition = Vector2.zero;
        Vector2 initSize = new Vector2(1000, 800);
        Vector2 cellSize;

        public override Vector2 InitialSize
        {
            get
            {
                return initSize;
            }
        }

        public ModsDiffWindow(ModInfo[] saveMods, ModInfo[] runningMods, Action confirmedAction) : base()
        {
            Log.Message($"save mods: {saveMods.Length}");
            Log.Message($"running mods: {runningMods.Length}");

            this.confirmedAction = confirmedAction;
            var diff = new Myers<ModInfo>(saveMods, runningMods);
            diff.Compute();

            Log.Message($"diff length: {diff.changeSet.Count}");

            info = diff.changeSet;

            cellSize = new Vector2(
                info.Max(x => Text.CalcSize(x.value.name).x + markerWidth),
                Text.LineHeight);

            initTexts();

            initSize = new Vector2(Math.Max(460, cellSize.x * 2 + markerWidth + vSpace * 2 + Margin * 2), 800);
        }

        private void initTexts()
        {
            this.optionalTitle = "ModsMismatchWarningTitle".Translate();
            message = "ModsMismatchWarningText".Translate().RawText.Split('\n').FirstOrDefault();
            cancelBtnText = "GoBack".Translate();
            continueBtnText = "LoadAnyway".Translate();
            reloadBtnText = "ChangeLoadedMods".Translate();
        }

        Listing_Standard diffList = new Listing_Standard();
        Rect innerRect;
        public override void DoWindowContents(Rect inRect)
        {
            var verticalSpacing = 2;
            var minLineWidth = cellSize.x * 2;
            var msgHeight = Text.CalcHeight(message, inRect.width);
            
            Widgets.Label(new Rect(inRect.xMin, inRect.yMin, inRect.xMax, msgHeight), message);
            var outerRect = Rect.MinMaxRect(inRect.xMin, inRect.yMin + msgHeight + 10, inRect.xMax, inRect.yMax - 40);
            //var innerRect = new Rect(0, 0, Math.Max(inRect.width - 16, minLineWidth), Text.LineHeight * info.Count);

            //Widgets.BeginScrollView(outerSize, ref scrollPosition, innerSize, true);

            //var diffList = new Listing_Standard();
            diffList.verticalSpacing = verticalSpacing;
            diffList.BeginScrollView(outerRect, ref scrollPosition, ref innerRect);
            //diffList.Begin(innerSize);

            var plusW = Text.CalcSize("+").x;
            var minusW = Text.CalcSize("-").x;

            int i = 0;
            foreach (var line in info)
            {
                var lineRect = diffList.GetRect(cellSize.y);

                if (i % 2 == 0)
                {
                    Widgets.DrawAltRect(lineRect);
                }

                // left
                if (line.change != ChangeType.Added)
                {
                    if (line.change == ChangeType.Removed)
                    {
                        Widgets.Label(new Rect((markerWidth - minusW) /2, lineRect.yMin, lineRect.width / 2, lineRect.height), "-");
                    }
                    Widgets.Label(new Rect(markerWidth, lineRect.yMin, lineRect.width / 2, lineRect.height), line.value.name);
                }

                // right
                if (line.change != ChangeType.Removed)
                {
                    if (line.change == ChangeType.Added)
                    {
                        Widgets.Label(new Rect((markerWidth - plusW) / 2 + lineRect.width / 2, lineRect.yMin, lineRect.width / 2, lineRect.height), "+");
                    }
                    Widgets.Label(new Rect(markerWidth + lineRect.width / 2, lineRect.yMin, lineRect.width / 2, lineRect.height), line.value.name);
                }
                // diffList.Label(line);
                i++;
            }
            //diffList.End();
            diffList.EndScrollView(ref innerRect);


            float slotSize = inRect.width / 3f;
            float btnWidth = slotSize - 10f;

            if (Widgets.ButtonText(new Rect(slotSize * 2 + 10f, inRect.height - 35f, btnWidth, 35f), continueBtnText, true, true, true))
            {
                if (confirmedAction!= null)
                {
                    confirmedAction();
                }
                this.Close(true);
            }
            GUI.color = Color.white;
            if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, btnWidth, 35f), cancelBtnText, true, true, true))
            {             
                this.Close(true);
            }
            if (Widgets.ButtonText(new Rect(slotSize, inRect.height - 35f, btnWidth, 35f), reloadBtnText, true, true, true))
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
                }
                else
                {
                    ModsConfig.RestartFromChangedMods();
                }

                this.Close(true);
            }
            // Widgets.EndScrollView();
        }
    }
}
