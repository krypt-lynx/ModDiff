// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Diff;
using RimWorld;
using ModDiff.GuiMinilib;
using Cassowary;
using System.Diagnostics;

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
        List<Change<ModInfo>> info;
        Action confirmedAction;

        const int markerWidth = 16;
        const int vSpace = 8;

        CGuiRoot gui = null;
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
            CalculateDiff(saveMods, runningMods, confirmedAction);

            cellSize = new Vector2(
                info.Max(x => Text.CalcSize(x.value.name).x + markerWidth),
                Text.LineHeight);

            initSize = new Vector2(Math.Max(460, cellSize.x * 2 + markerWidth + vSpace * 2 + Margin * 2), 800);

            var timer = new Stopwatch();
            timer.Start();
            ConstructGui(confirmedAction);
            timer.Stop();
            Log.Message($"gui init in: {timer.Elapsed}");

            Log.Message($"Elements created: {CElement.nextId}");

        }

        private void ConstructGui(Action confirmedAction)
        {
            gui = new CGuiRoot();
            var titleLabel = gui.AddElement(new CLabel
            {
                Font = GameFont.Medium,
                Title = "ModsMismatchWarningTitle".Translate()
            });
            var disclaimerLabel = gui.AddElement(new CLabel
            {
                Title = "ModsMismatchWarningText".Translate().RawText.Split('\n').FirstOrDefault()
            });
            var diffList = gui.AddElement(new CListingStandart
            {
            });
            var buttonPanel = gui.AddElement(new CElement());
            var backButton = buttonPanel.AddElement(new CButton
            {
                Title = "GoBack".Translate(),
                Action = (_) => Close(true)
            });
            var reloadButton = buttonPanel.AddElement(new CButton
            {
                Title = "ChangeLoadedMods".Translate(),
                Action = (_) =>
                {
                    TrySetActiveMods();
                    Close(true);
                }
            });
            var continueButton = buttonPanel.AddElement(new CButton
            {
                Title = "LoadAnyway".Translate(),
                Action = (_) =>
                {
                    confirmedAction?.Invoke();
                    Close(true);
                }
            });

            // root constraints
            // horizontal
            gui.EmbedW(titleLabel);
            gui.EmbedW(disclaimerLabel);
            gui.EmbedW(diffList);
            gui.EmbedW(buttonPanel);
            
            // vertical 
            gui.solver.AddConstraint(gui.top, titleLabel.top, titleLabel.height, titleLabel.bottom, disclaimerLabel.top, disclaimerLabel.height,
                (t, tt, th, tb, dt, dh) => t == tt && th == 42 && tb == dt && dh == 50);
            gui.solver.AddConstraint(gui.bottom, disclaimerLabel.bottom, diffList.top, diffList.bottom, buttonPanel.top, buttonPanel.height, buttonPanel.bottom,
                (b, db, lt, lb, bt, bh, bb) => db == lt && lb + 10 == bt && bh == 40 && b == bb);

            // buttons panel constraints
            // horizontal
            gui.solver.AddConstraint(
                buttonPanel.left, buttonPanel.right,
                backButton.left, backButton.right,
                reloadButton.left, reloadButton.right,
                continueButton.left, continueButton.right,
                (l, r, bl, br, rl, rr, cl, cr) => l == bl && br + 10 == rl && rr + 20 == cl && cr == r);
            gui.solver.AddConstraint(backButton.width, reloadButton.width, continueButton.width,
                (b, r, c) => b == r && r == c);

            // vertical 
            buttonPanel.EmbedH(backButton);
            buttonPanel.EmbedH(reloadButton);
            buttonPanel.EmbedH(continueButton);

            ConstructDiffList(diffList);

            gui.UpdateLayoutConstraintsIfNeeded();
            // I'm not doing math for that, at least
        }

        private void ConstructDiffList(CListingStandart diffList)
        {
            int i = 0;

            foreach (var line in info)
            {
                var row = diffList.NewRow();
                CElement bg = null;
                if (i % 2 == 0)
                {
                    bg = row.AddElement(new CWidget
                    {
                        Do = bounds => Widgets.DrawAltRect(bounds)
                    });
                    row.Embed(bg);
                }

                // left
                CElement lCell;
                if (line.change != ChangeType.Added)
                {
                    lCell = ConstructDiffCell(row, "-", line.change == ChangeType.Removed, line.value.name);
                }
                else
                {
                    lCell = row.AddElement(new CElement());
                }

                // right
                CElement rCell;
                if (line.change != ChangeType.Removed)
                {
                    rCell = ConstructDiffCell(row, "+", line.change == ChangeType.Added, line.value.name);
                }
                else
                {
                    rCell = row.AddElement(new CElement());
                }

                row.EmbedW(lCell, rCell);
                row.solver.AddConstraint(lCell.width, rCell.width, (a, b) => a == b);
                row.EmbedH(lCell);
                row.EmbedH(rCell);
                
                row.solver.AddConstraint(row.height, h => h == cellSize.y);

                i++;
            }
        }

        private CElement ConstructDiffCell(CElement parent, string symbol, bool showSymbol, string title)
        {
            var cell = parent.AddElement(new CElement());
            var icon = cell.AddElement(
                showSymbol ? new CLabel { Title = symbol } : new CElement()
                );
            var text = cell.AddElement(new CLabel
            {
                Title = title
            });

            cell.EmbedW(icon, text);
            cell.solver.AddConstraint(icon.width, w => w == 16);
            cell.EmbedH(icon);
            cell.EmbedH(text);

            return cell;
        }

        private void CalculateDiff(ModInfo[] saveMods, ModInfo[] runningMods, Action confirmedAction)
        {
            Log.Message($"save mods: {saveMods.Length}");
            Log.Message($"running mods: {runningMods.Length}");

            this.confirmedAction = confirmedAction;
            var diff = new Myers<ModInfo>(saveMods, runningMods);
            diff.Compute();

            Log.Message($"diff length: {diff.changeSet.Count}");

            info = diff.changeSet;
        }

        private static void TrySetActiveMods()
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
        }


        public override void DoWindowContents(Rect inRect)
        {
            gui.InRect = inRect;
            gui.DoElementContent();

            /*
            var verticalSpacing = 2;
            var minLineWidth = cellSize.x * 2;
            var msgHeight = Text.CalcHeight(message, inRect.width);
            
            Widgets.Label(new Rect(inRect.xMin, inRect.yMin, inRect.xMax, msgHeight), message);
            var outerRect = Rect.MinMaxRect(inRect.xMin, inRect.yMin + msgHeight + 10, inRect.xMax, inRect.yMax - 40);

            diffList.verticalSpacing = verticalSpacing;
            diffList.BeginScrollView(outerRect, ref scrollPosition, ref innerRect);

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
            
            if (Widgets.ButtonText(new Rect(slotSize * 2 + 10f, inRect.height - 35f, btnWidth, 35f), continueBtnText, doMouseoverSound: true))
            {
                if (confirmedAction!= null)
                {
                    confirmedAction();
                }
                this.Close(true);
            }
            GUI.color = Color.white;
            if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, btnWidth, 35f), cancelBtnText, doMouseoverSound: true))
            {             
                this.Close(true);
            }
            if (Widgets.ButtonText(new Rect(slotSize, inRect.height - 35f, btnWidth, 35f), reloadBtnText, doMouseoverSound: true))
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
            */
        }


    }
}
