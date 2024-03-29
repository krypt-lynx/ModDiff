﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Diff;
using RimWorld;
using RWLayout.alpha2;
using Cassowary;
using System.Diagnostics;

namespace ModDiff
{
    public static class ModInfoToStyle
    {
        public static CellStyle LeftCellStyle(this DiffListItem info)
        {
            CellStyle style;

            if (info.Change == ChangeType.Unmodified)
            {
                style = CellStyle.Default;
            }
            else if (info.ModModel.IsMissing)
            {
                style = CellStyle.Missing;
            }
            else if (info.ModModel.IsMoved)
            {
                style = CellStyle.Moved;
            }
            else
            {
                style = CellStyle.Removed;
            }

            return style;
        }

        public static CellStyle RightCellStyle(this DiffListItem info)
        {
            CellStyle style;

            if (info.Change == ChangeType.Unmodified)
            {
                style = CellStyle.Default;
            }
            else if (info.ModModel.IsMoved)
            {
                style = CellStyle.Moved;
            }
            else
            {
                style = CellStyle.Added;
            }

            return style;
        }

        public static CellStyle MiddleCellStyle(this DiffListItem info)
        {
            if (info.ModModel.IsMoved)
            {
                return CellStyle.EditMoved;
            }
            else if (info.ModModel.IsMissing)
            {
                return CellStyle.Unavailable;
            }

            switch (info.Change)
            {
                case ChangeType.Added:
                    return CellStyle.EditAdded;
                case ChangeType.Removed:
                    return CellStyle.EditAdded;
                case ChangeType.Unmodified:
                    return CellStyle.Default;
                default:
                    throw new Exception();
            }
        }
    }

    public class CIconButton : CButtonText
    {
        public Texture2D Icon = null;
        public EdgeInsets IconInsets = EdgeInsets.Zero;
        public Color IconTint = UnityEngine.Color.white;

        Rect iconRect;

        public override void PostLayoutUpdate()
        {
            base.PostLayoutUpdate();
            //GuiTools.GUIRounded

            iconRect = GuiTools.SizeCenteredIn(BoundsRounded, IconInsets, Icon.Size());
        }

        public override void DoContent()
        {
            base.DoContent();

            GuiTools.UsingColor(IconTint, () =>
            {
                GUI.DrawTexture(iconRect, Icon);
            });
        }
    }

    //[StaticConstructorOnStartup]
    public class ModDiffWindow : CWindow, IListViewDataSource
    {
        const int vSpace = 8;

       // public static readonly Texture2D EditIcon = ContentFinder<Texture2D>.Get("UI/Icons/DiffEdit", true);


        ModDiffModel model;


        Action confirmedAction = null;
        public ModDiffWindow(ModInfo[] saveMods, ModInfo[] runningMods, Action confirmedAction) : base()
        {
            this.confirmedAction = confirmedAction;

            model = new ModDiffModel();
            model.saveMods = saveMods;
            model.runningMods = runningMods;
            model.CalculateDiff();

            var cellSize = new Vector2(
                model.modsList.Max(x => Mathf.Max(
                    x.ModModel.Left != null ? Text.CalcSize(x.ModModel.Left.Name).x : 0, 
                    x.ModModel.Right != null ? Text.CalcSize(x.ModModel.Right.Name).x : 0
                    ) + 5 + 3 + ModDiffCell.MarkerWidth + 1 + 16 + 2),
                ModDiffCell.DefaultHeight);

            InnerSize = new Vector2(Math.Max(460, cellSize.x * 2 + 16), 800);
        }

        public override void ConstructGui()
        {
            base.ConstructGui();

            var guide = new CVarListGuide();
            Gui.AddGuide(guide);

            this.absorbInputAroundWindow = true;

            var titleLabel = Gui.AddElement(new CLabel
            {
                Font = GameFont.Medium,
                Title = "ModsMismatchWarningTitle".Translate()
            });
            var disclaimerLabel = Gui.AddElement(new CLabel
            {
                Title = "ModsMismatchWarningText".Translate().RawText.Split('\n').FirstOrDefault(),
                WordWrap = true,
            });

            CListView_vNext diffList = null;

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
                TryFitContent = (_) => new Vector2(diffList.IsScrollBarVisible() ? 16 : 0, 0)
            });

            var headerLine = Gui.AddElement(new CWidget {
                DoWidgetContent = (_, bounds) => GuiTools.UsingColor(new Color(1f, 1f, 1f, 0.2f), () => Widgets.DrawLineHorizontal(bounds.x, bounds.y, bounds.width - (diffList.IsScrollBarVisible() ? 16 : 0)))
            });

            diffList = Gui.AddElement(new CListView_vNext());

            var buttonPanel = Gui.AddElement(new CElement());
            var backButton = buttonPanel.AddElement(new CButtonText
            {
                Title = "GoBack".Translate(),
                Action = (_) => Close(true)
            });
            var reloadButton = buttonPanel.AddElement(new CButtonText
            {
                Title = "ChangeLoadedMods".Translate(),
                Action = (_) => TrySetActiveMods(),
            });
            var editButton = buttonPanel.AddElement(new CIconButton
            {
                Icon = ContentFinder<Texture2D>.Get("UI/Icons/DiffEdit"),
                IconTint = new Color(1, 1, 1, 0.83f),
                Action = (_) => MergeMods(),
            });
            
            var continueButton = buttonPanel.AddElement(new CButtonText
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

            headerPanel.StackLeft(16 + 5, headerLeft, 2+16+5, (headerRight, headerLeft.width), 2, (headerSpacer, headerSpacer.intrinsicWidth));
            headerLeft.AddConstraint(headerLeft.height ^ headerLeft.intrinsicHeight);

            buttonPanel.StackLeft(backButton, 10.0, reloadButton, 5, (editButton, editButton.height), 20.0, continueButton);

            var buttonsWidth = new ClVariable("buttonsWidth");
            guide.Variables.Add(buttonsWidth);

            buttonPanel.AddConstraint(backButton.width >= backButton.intrinsicWidth);
            buttonPanel.AddConstraint(reloadButton.width >= reloadButton.intrinsicWidth);
            buttonPanel.AddConstraint(continueButton.width >= continueButton.intrinsicWidth);

            buttonPanel.AddConstraint(backButton.width ^ buttonsWidth, ClStrength.Medium);
            buttonPanel.AddConstraint(reloadButton.width ^ buttonsWidth, ClStrength.Medium);
            buttonPanel.AddConstraint(continueButton.width ^ buttonsWidth, ClStrength.Medium);


            Gui.AddConstraint(diffList.height <= diffList.intrinsicHeight);

            diffList.DataSource = this;

            Gui.AddConstraint(Gui.height <= Gui.AdjustedScreenSize.height * 0.8); // TODO: LayoutGuide
            Gui.AddConstraint(Gui.width ^ InnerSize.x, ClStrength.Medium);
            Gui.AddConstraint(Gui.width <= Gui.AdjustedScreenSize.width * 0.9);

        }

        private void MergeMods()
        {                        
            Find.WindowStack.Add(new MergeModsWindow(model));
        }

        private void TrySetActiveMods()
        {
            if (!model.HaveMissingMods)
            {
                model.TrySetActiveModsFromSamegame();
                Close(true);
            }
            else
            {
                Find.WindowStack.Add(new MissingModsDialog(model.modsList.Where(x => x.ModModel.IsMissing).Select(x => x.ModModel), model.TrySetActiveModsFromSamegame));
            }
        }

        public int NumberOfRows()
        {
            return model.modsList.Length;
        }

        public float HeightForRowAt(int index)
        {
            return ModDiffCell.DefaultHeight;
        }

        public CListingRow ListingRowForRowAt(int index)
        {
            var line = model.modsList[index];

            var row = new CListingRow();

            bool isEven = index % 2 == 0;
            CElement bg = null;
            
            bg = row.AddElement(new CElement());

            row.Embed(bg);

            // left
            CElement lCell;
            if (line.Change != ChangeType.Added)
            {
                var c = new ModDiffCell(
                    line.LeftCellStyle(),
                    line.ModModel.Left.Name,
                    isEven,
                    false,
                    "packadeId:\n" + line.ModModel.Left.PackageId);
                if (line.ModModel.Left.Source != ContentSource.Undefined)
                {
                    c.infoIcon = line.ModModel.Left.Source.GetIcon();
                    c.showWarning = !line.ModModel.Left.Compatible;
                }
                lCell = row.AddElement(c);
            }
            else
            {
                lCell = row.AddElement(new BgElement(isEven, false));
            }

            // right
            CElement rCell;
            if (line.Change != ChangeType.Removed)
            {
                var c = new ModDiffCell(
                    line.RightCellStyle(),
                    line.ModModel.Right.Name,
                    isEven,
                    false,
                    "packadeId:\n" + line.ModModel.Right.PackageId);
                if (line.ModModel.Right.Source != ContentSource.Undefined)
                {
                    c.infoIcon = line.ModModel.Right.Source.GetIcon();
                    c.showWarning = !line.ModModel.Right.Compatible;
                }
                rCell = row.AddElement(c);
            }
            else
            {
                rCell = row.AddElement(new BgElement(isEven, false));
            }

            row.StackLeft(lCell, (rCell, lCell.width));

            return row;
        }
    }
}
