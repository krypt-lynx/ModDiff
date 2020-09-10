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
using RWLayout.moddiff;
using Cassowary_moddiff;
using System.Diagnostics;

namespace ModDiff
{
    public class ModInfo
    {
        public string packageId;
        public string name;
        public bool isMoved = false;
        public bool isMissing = false;

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

    public class ModDiffWindow : CWindow
    {
        const int vSpace = 8;

        CellStyleData missingModStyle;
        CellStyleData removedModCellStyle;
        CellStyleData addedModCellStyle;
        CellStyleData movedModCellStyle;


        ModDiffModel model;

        void InitStyles()
        {
            if (!ModDiff.settings.alternativePallete)
            {
                removedModCellStyle = new CellStyleData()
                {
                    marker = "-",
                    bgColor = new Color(0.5f, 0.17f, 0.17f, 0.70f),
                    outlineColor = new Color(0.5f, 0.17f, 0.17f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
                addedModCellStyle = new CellStyleData()
                {
                    marker = "+",
                    bgColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                    outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
                movedModCellStyle = new CellStyleData()
                {
                    marker = "*",
                    bgColor = new Color(0.38f, 0.36f, 0.15f, 0.70f),
                    outlineColor = new Color(0.38f, 0.36f, 0.15f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
                missingModStyle = new CellStyleData()
                {
                    marker = "!",
                    bgColor = new Color(0.2f, 0.05f, 0.05f, 0.70f),
                    outlineColor = new Color(0.4f, 0.10f, 0.10f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
            }
            else
            {
                removedModCellStyle = new CellStyleData()
                {
                    marker = "-",
                    bgColor = new Color(0.45f, 0.10f, 0.45f, 0.70f),
                    outlineColor = new Color(0.45f, 0.10f, 0.45f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
                addedModCellStyle = new CellStyleData()
                {
                    marker = "+",
                    bgColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                    outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
                movedModCellStyle = new CellStyleData()
                {
                    marker = "*",
                    bgColor = new Color(0.40f, 0.40f, 0.40f, 0.70f),
                    outlineColor = new Color(0.40f, 0.40f, 0.40f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
                missingModStyle = new CellStyleData()
                {
                    marker = "!",
                    bgColor = new Color(0.2f, 0.05f, 0.2f, 0.70f),
                    outlineColor = new Color(0.4f, 0.10f, 0.4f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
            }

        }


        Action confirmedAction = null;
        public ModDiffWindow(ModInfo[] saveMods, ModInfo[] runningMods, Action confirmedAction) : base()
        {
            InitStyles();
            this.confirmedAction = confirmedAction;

            model = new ModDiffModel();
            model.saveMods = saveMods;
            model.runningMods = runningMods;
            model.CalculateDiff();

            var cellSize = new Vector2(
                model.info.Max(x => Text.CalcSize(x.value.name).x + ModDiffCell.MarkerWidth),
                Text.LineHeight);

            InnerSize = new Vector2(Math.Max(460, cellSize.x * 2 + ModDiffCell.MarkerWidth + 20), 800);
        }

        public override void ConstructGui()
        {
            base.ConstructGui();


            this.absorbInputAroundWindow = true;

            var titleLabel = Gui.AddElement(new CLabel
            {
                Font = GameFont.Medium,
                Title = "ModsMismatchWarningTitle".Translate()
            });
            var disclaimerLabel = Gui.AddElement(new CLabel
            {
                Title = "ModsMismatchWarningText".Translate().RawText.Split('\n').FirstOrDefault(),
                // Multiline = true
                //lines_debug = 2
            });

            CListView diffList = null;

            var headerPanel = Gui.AddElement(new CElement());
            var headerLeft = headerPanel.AddElement(new CLabel {
                Font = GameFont.Small,
                Color = new Color(1, 1, 1, 0.3f),
                Title = "SaveGameMods".Translate(), // "Savegame mods:"
            });
            var headerRight = headerPanel.AddElement(new CLabel {
                Font = GameFont.Small,
                Color = new Color(1, 1, 1, 0.3f),
                Title = "RunningMods".Translate(), // "Running mods:"
            });
            var headerSpacer = headerPanel.AddElement(new CWidget
            {
                TryFitContect = (_) => new Vector2(diffList.IsScrollBarVisible() ? 20 : 0, 0)
            });

            var headerLine = Gui.AddElement(new CWidget {
                DoWidgetContent = (_, bounds) => GuiTools.UsingColor(new Color(1f, 1f, 1f, 0.2f), () => Widgets.DrawLineHorizontal(bounds.x, bounds.y, bounds.width - (diffList.IsScrollBarVisible() ? 20 : 0)))
            });

            diffList = Gui.AddElement(new CListView
            {
              
            });
            var buttonPanel = Gui.AddElement(new CElement());
            var backButton = buttonPanel.AddElement(new CButton
            {
                Title = "GoBack".Translate(),
                Action = (_) => Close(true)
            });
            var reloadButton = buttonPanel.AddElement(new CButton
            {
                Title = "ChangeLoadedMods".Translate(),
                Action = (_) => TrySetActiveMods(),
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
            Gui.StackTop((titleLabel, 42), (disclaimerLabel, disclaimerLabel.intrinsicHeight), 2, headerPanel, (headerLine, 1), 4, diffList, 12, (buttonPanel, 40));

            headerPanel.StackLeft(16 + 5, headerLeft, 16+5, (headerRight, headerLeft.width), (headerSpacer, headerSpacer.intrinsicWidth));
            headerLeft.Solver.AddConstraint(headerLeft.height ^ headerLeft.intrinsicHeight);

            buttonPanel.StackLeft(backButton, 10.0, reloadButton, 20.0, continueButton);

            buttonPanel.Solver.AddConstraint(backButton.width >= backButton.intrinsicWidth);
            buttonPanel.Solver.AddConstraint(reloadButton.width >= reloadButton.intrinsicWidth);
            buttonPanel.Solver.AddConstraint(continueButton.width >= continueButton.intrinsicWidth);

            // pairing all 3 buttons: if intrinsic width of one of them is too big - costraints of that button will be broken, so, we need to bind each to each

            buttonPanel.Solver.AddConstraints(ClStrength.Medium, backButton.width ^ reloadButton.width); 
            buttonPanel.Solver.AddConstraints(ClStrength.Medium, backButton.width ^ continueButton.width);
            buttonPanel.Solver.AddConstraints(ClStrength.Medium, reloadButton.width ^ continueButton.width);


            Gui.Solver.AddConstraint(diffList.height <= diffList.intrinsicHeight);

            ConstructDiffList(diffList);

            Gui.Solver.AddConstraint(Gui.height <= Gui.AdjustedScreenSize.height * 0.8); // TODO: LayoutGuide
            Gui.Solver.AddConstraint(Gui.width ^ InnerSize.x);

        }

        private void TrySetActiveMods()
        {
            if (!model.HaveMissingMods)
            {
                model.TrySetActiveMods();
                Close(true);
            }
            else
            {
                Find.WindowStack.Add(new MissingModsDialog(model.saveMods.Where(x => x.isMissing), () => model.TrySetActiveMods(), missingModStyle));
            }
        }

        private void ConstructDiffList(CListView diffList)
        {
            int i = 0;

            foreach (var line in model.info)
            {

                var row = new CListingRow();
                diffList.AppendRow(row);
                string tip = "packadeId:\n" + line.value.packageId;
                CElement bg = null;
                if (i % 2 == 1)
                {
                    bg = row.AddElement(new CWidget
                    {

                        DoWidgetContent = (_, bounds) => {
                            Widgets.DrawAltRect(bounds);
                            TooltipHandler.TipRegion(bounds, tip);
                        }
                    });

                }
                else
                {
                    bg = row.AddElement(new CWidget
                    {
                        DoWidgetContent = (_, bounds) => {
                            TooltipHandler.TipRegion(bounds, tip);
                        }
                    });
                }
                row.Embed(bg);

                // left
                CElement lCell;
                if (line.change != ChangeType.Added)
                {
                    CellStyleData style;

                    if (line.value.isMissing)
                    {
                        style = missingModStyle;
                    }
                    else if (line.value.isMoved)
                    {
                        style = movedModCellStyle;
                    }
                    else
                    {
                        style = removedModCellStyle;
                    }
                    lCell = row.AddElement(new ModDiffCell(
                        style, 
                        line.change == ChangeType.Removed, 
                        line.value.name
                        ));
                }
                else
                {
                    lCell = row.AddElement(new CElement());
                }

                // right
                CElement rCell;
                if (line.change != ChangeType.Removed)
                {
                    CellStyleData style;

                    if (line.value.isMoved)
                    {
                        style = movedModCellStyle;
                    }
                    else
                    {
                        style = addedModCellStyle;
                    }

                    rCell = row.AddElement(new ModDiffCell(
                        style,
                        line.change == ChangeType.Added,
                        line.value.name
                        ));
                }
                else
                {
                    rCell = row.AddElement(new CElement());
                }

                row.StackLeft(lCell, (rCell, lCell.width));
                
                //row.Solver.AddConstraint(row.height, h => h == cellSize.y);

                i++;
            }
        }
    }
}
