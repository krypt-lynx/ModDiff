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

            initSize = new Vector2(Math.Max(460, cellSize.x * 2 + markerWidth + vSpace * 2 + Margin * 2 + 20), 800);

            var lastNext = CElement.nextId;
            var timer = new Stopwatch();
            timer.Start();
            ConstructGui(confirmedAction);
            timer.Stop();
            Log.Message($"gui init in: {timer.Elapsed}");

            Log.Message($"Elements created: {CElement.nextId - lastNext}");
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

        private CElement ConstructDiffCell(CElement parent, string symbol, bool modified, string title)
        {
            var cell = parent.AddElement(new CElement());

            if (modified)
            {
                var highlight = cell.AddElement(new CWidget
                {
                    Do = bounds => Widgets.DrawAltRect(bounds)
                });
                cell.Embed(highlight);
            }

            var iconSlot = cell.AddElement(new CElement());
            if (modified)
            {
                var icon = iconSlot.AddElement(new CLabel { Title = symbol });
                iconSlot.EmbedH(icon);
                //iconSlot.EmbedW(icon);       
                iconSlot.solver.AddConstraint(iconSlot.centerX, icon.centerX, (a, b) => a == b);
                icon.solver.AddConstraint(icon.width, icon.intrinsicWidth, (a, b) => a == b);
            }

            var text = cell.AddElement(new CLabel
            {
                Title = title
            });

            cell.EmbedW(iconSlot, text);
            cell.solver.AddConstraint(iconSlot.width, w => w == 16);
            cell.EmbedH(iconSlot);
            cell.EmbedH(text);

            return cell;
        }

        private void CalculateDiff(ModInfo[] saveMods, ModInfo[] runningMods, Action confirmedAction)
        {
            //Log.Message($"save mods: {saveMods.Length}");
            //Log.Message($"running mods: {runningMods.Length}");

            this.confirmedAction = confirmedAction;
            var diff = new Myers<ModInfo>(saveMods, runningMods);
            diff.Compute();

            //Log.Message($"diff length: {diff.changeSet.Count}");

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
        }


    }
}
