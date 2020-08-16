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
using GuiMinilib;
using Cassowary;
using System.Diagnostics;

namespace ModDiff
{
    public class ModInfo
    {
        public string packageId;
        public string name;
        public bool isMoved = false;

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


    struct CellStyleData
    {
        public string marker;
        public Texture2D bgTexture;
        public Color outlineColor;
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
                return initSize + new Vector2(Margin * 2, Margin * 2);
            }
        }

        private static readonly Texture2D RemovedModBg = SolidColorMaterials.NewSolidColorTexture(new Color(0.5f, 0.17f, 0.17f, 0.70f));
        private static readonly Texture2D AddedModBg = SolidColorMaterials.NewSolidColorTexture(new Color(0.17f, 0.45f, 0.17f, 0.70f));
        private static readonly Texture2D MovedModBg = SolidColorMaterials.NewSolidColorTexture(new Color(0.38f, 0.36f, 0.15f, 0.70f));


        CellStyleData removedModCellStyle = new CellStyleData()
        {
            marker = "-",
            bgTexture = RemovedModBg,
            outlineColor = new Color(0.5f, 0.17f, 0.17f, 0.70f),
        };

        CellStyleData addedModCellStyle = new CellStyleData()
        {
            marker = "+",
            bgTexture = AddedModBg,
            outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
        };

        CellStyleData movedModCellStyle = new CellStyleData()
        {
            marker = "*",
            bgTexture = MovedModBg,
            outlineColor = new Color(0.38f, 0.36f, 0.15f, 0.70f),
        };

        public ModsDiffWindow(ModInfo[] saveMods, ModInfo[] runningMods, Action confirmedAction) : base()
        {
            CalculateDiff(saveMods, runningMods, confirmedAction);

            cellSize = new Vector2(
                info.Max(x => Text.CalcSize(x.value.name).x + markerWidth),
                Text.LineHeight);
            initSize = new Vector2(Math.Max(460, cellSize.x * 2 + markerWidth + vSpace * 2 + 20), 800);


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
                Title = "ModsMismatchWarningText".Translate().RawText.Split('\n').FirstOrDefault(),
                Multiline = true
                //lines_debug = 2
            });

            CListingStandart diffList = null;

            var headerPanel = gui.AddElement(new CElement());
            var headerLeft = headerPanel.AddElement(new CLabel {
                Font = GameFont.Small,
                Color = new Color(1, 1, 1, 0.3f),
                Title = "Savegame mods:"
            });
            var headerRight = headerPanel.AddElement(new CLabel {
                Font = GameFont.Small,
                Color = new Color(1, 1, 1, 0.3f),
                Title = "Running mods:"
            });
            var headerSpacer = headerPanel.AddElement(new CWidget
            {
                TryFitContect = (_) => new Vector2(diffList.IsScrollBarVisible() ? 20 : 0, 0)
            });

            var headerLine = gui.AddElement(new CWidget {
                DoWidgetContent = bounds => GuiTools.UsingColor(new Color(1f, 1f, 1f, 0.2f), () => Widgets.DrawLineHorizontal(bounds.x, bounds.y, bounds.width - (diffList.IsScrollBarVisible() ? 20 : 0)))
            });
            diffList = gui.AddElement(new CListingStandart
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
            gui.StackTop(true, true, (titleLabel, 42), (disclaimerLabel, disclaimerLabel.intrinsicHeight), 2, headerPanel, (headerLine, 1), 4, diffList, 10, (buttonPanel, 40));

            headerPanel.StackLeft(true, true, 16+5, headerLeft, 16+5, (headerRight, headerLeft.width), (headerSpacer, headerSpacer.intrinsicWidth));
            headerLeft.solver.AddConstraint(headerLeft.height, headerLeft.intrinsicHeight, (a, b) => a == b);

            buttonPanel.StackLeft(true, true,
                backButton, 10.0, reloadButton, 20.0, continueButton);

            buttonPanel.solver.AddConstraint(backButton.width, backButton.intrinsicWidth, (a, b) => a >= b, ClStrength.Strong);
            buttonPanel.solver.AddConstraint(reloadButton.width, reloadButton.intrinsicWidth, (a, b) => a >= b, ClStrength.Strong);
            buttonPanel.solver.AddConstraint(continueButton.width, continueButton.intrinsicWidth, (a, b) => a >= b, ClStrength.Strong);

            // pairing all 3 buttons: if intrinsic width of one of them is too big - costraints of that button will be bocken, so, we need to bind each to each

            buttonPanel.solver.AddConstraint(backButton.width, reloadButton.width, (a, b) => a == b, ClStrength.Medium); 
            buttonPanel.solver.AddConstraint(backButton.width, continueButton.width, (a, b) => a == b, ClStrength.Medium);
            buttonPanel.solver.AddConstraint(reloadButton.width, continueButton.width, (a, b) => a == b, ClStrength.Medium);

            //           gui.solver.AddConstraint(diffList.height, diffList.intrinsicHeight, (a, b) => a >= b, ClStrength.Weak);

            gui.FlexibleHeight = true;
            gui.solver.AddConstraint(new ClLinearEquation(diffList.height, new ClLinearExpression(diffList.intrinsicHeight), ClStrength.Weak));

            ConstructDiffList(diffList);


            gui.LayoutUpdated = () =>
            {
                this.initSize = new Vector2(initSize.x, gui.bounds.height);
                Log.Message($"LayoutUpdated callback initSize: {this.initSize}");
//                gui.InRect = this.initSize;
            };

            Log.Message($"ConstructGui initSize: {this.initSize}");
            gui.InRect = new Rect(Vector2.zero, initSize);
        }

        private void ConstructDiffList(CListingStandart diffList)
        {
            int i = 0;

            foreach (var line in info)
            {
                var row = diffList.NewRow();
                string tip = "packadeId:\n" + line.value.packageId;
                CElement bg = null;
                if (i % 2 == 1)
                {
                    bg = row.AddElement(new CWidget
                    {
                        DoWidgetContent = bounds => {
                            Widgets.DrawAltRect(bounds);
                            TooltipHandler.TipRegion(bounds, tip);
                        }
                    });

                }
                else
                {
                    bg = row.AddElement(new CWidget
                    {
                        DoWidgetContent = bounds => {
                            TooltipHandler.TipRegion(bounds, tip);
                        }
                    });
                }
                row.Embed(bg);

                bool isMoved = line.value.isMoved;

                // left
                CElement lCell;
                if (line.change != ChangeType.Added)
                {
                    
                    lCell = ConstructDiffCell(row, isMoved ? movedModCellStyle : removedModCellStyle, 
                        line.change == ChangeType.Removed, 
                        line.value.name
                        );
                }
                else
                {
                    lCell = row.AddElement(new CElement());
                }

                // right
                CElement rCell;
                if (line.change != ChangeType.Removed)
                {
                    rCell = ConstructDiffCell(row, isMoved ? movedModCellStyle : addedModCellStyle,
                        line.change == ChangeType.Added,
                        line.value.name
                        );
                }
                else
                {
                    rCell = row.AddElement(new CElement());
                }

                row.StackLeft(true, true, lCell, rCell);
                row.solver.AddConstraint(lCell.width, rCell.width, (a, b) => a == b);
                
                //row.solver.AddConstraint(row.height, h => h == cellSize.y);

                i++;
            }
        }

        private CElement ConstructDiffCell(CElement parent, CellStyleData style, bool modified, string title)
        {
            var cell = parent.AddElement(new CElement());

            if (modified)
            {
                var highlight = cell.AddElement(new CWidget
                {
                    DoWidgetContent = bounds =>
                    {
                        GUI.DrawTexture(bounds, style.bgTexture);
                        GuiTools.UsingColor(style.outlineColor, () =>
                        {
                            GuiTools.Box(bounds, new EdgeInsets(2, 2, 2, 5));
                        });
                    }
                });
                cell.Embed(highlight);
            }

            var iconSlot = cell.AddElement(new CElement());
            if (modified)
            {
                var icon = iconSlot.AddElement(new CLabel { Title = style.marker });
                iconSlot.StackTop(false, true, icon);
                iconSlot.solver.AddConstraint(iconSlot.centerX, icon.centerX, (a, b) => a == b);
                icon.solver.AddConstraint(icon.width, icon.intrinsicWidth, (a, b) => a == b);
            }

            var text = cell.AddElement(new CLabel
            {
                Title = title
            });

            cell.StackLeft(true, true, 5, (iconSlot, 16), text, 2);

            cell.solver.AddConstraint(cell.height, text.intrinsicHeight, (a, b) => a == b);

            return cell;
        }

        private void CalculateDiff(ModInfo[] saveMods, ModInfo[] runningMods, Action confirmedAction)
        {
            this.confirmedAction = confirmedAction;
            var diff = new Myers<ModInfo>(saveMods, runningMods);
            diff.Compute();

            info = diff.changeSet;

            foreach (var x in diff.changeSet)
            {
         //       Log.Message(x.value.packageId + "|" + x.value.name + "|" + x.change.ToString());
            }

            var moved = info.Where(x => x.change == ChangeType.Removed).Select(x => x.value).ToHashSet();
            moved.IntersectWith(info.Where(x => x.change == ChangeType.Added).Select(x => x.value));

            foreach (var change in diff.changeSet)
            {
                if (moved.Contains(change.value))
                {
                    change.value.isMoved = true;
                }
            }
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

        static Texture2D debugBg = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.5f));
        public override void DoWindowContents(Rect inRect)
        {
            gui.InRect = inRect;
            gui.DoElementContent();
            GUI.DrawTexture(inRect, debugBg);
            gui.DoDebugOverlay();
        }


    }
}
