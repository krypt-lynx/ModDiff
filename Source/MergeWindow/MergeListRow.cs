using Diff;
using RWLayout.moddiff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModDiff
{
    class MergeListRow : CListingRow
    {
        public DiffListItem Item;
        public int Index;
        public ModModel Model { get => Item.ModModel; }

        bool selected = false;
        bool Selected
        {
            set
            {
                selected = value;
                cCell.Hidden = !selected;
                if (cCellDeselected != null)
                {
                    cCellDeselected.Hidden = value || Model.IsMoved;
                }
            }
            get => selected;
        }

        CElement cCell;
        CElement cCellDeselected;


        public MergeListRow(DiffListItem item, int index)
        {
            Item = item;
            Index = index;
            selected = item.Selected;
            // We are not cleaning memory after window close, so, without weak reference item model will prevent row destruction by GC
            // with weak reference only closure will stay in memory until parent window is closed. We can live with this.
            var weakThis = new WeakReference(this, false);
            item.OnSelectedChanged = (newSelected) => {
                if (weakThis.IsAlive)
                {
                    (weakThis.Target as MergeListRow).Selected = newSelected;
                }
            };

            Construct();
        }

        private void Construct()
        {
            string tip = "packadeId:\n" + Item.ModModel.PackageId;
            CElement bg = null;
            if (Index % 2 == 1)
            {
                bg = AddElement(new CWidget
                {

                    DoWidgetContent = (_, bounds) =>
                    {
                        Widgets.DrawAltRect(bounds);
                        TooltipHandler.TipRegion(bounds, tip);
                    }
                });
            }
            else
            {
                bg = AddElement(new CWidget
                {
                    DoWidgetContent = (_, bounds) =>
                    {
                        TooltipHandler.TipRegion(bounds, tip);
                    }
                });
            }
            bg.userInteractionEnabled = false;
            this.Embed(bg);


            // left
            CElement lCell;
            if (Item.Change != ChangeType.Added)
            {
                lCell = bg.AddElement(new ModDiffCell(Item.LeftCellStyle(), Item.ModModel.Left.name));
            }
            else
            {
                lCell = bg.AddElement(new CElement());
            }

            // center
            if (Model.IsMoved)
            {
                cCell = bg.AddElement(new ModDiffCell(Item.MiddleCellStyle(), Item.ModModel.Name));
            } 
            else if (!Model.IsMissing)
            {
                cCell = bg.AddElement(new ModDiffCell(Item.MiddleCellStyle(), Item.ModModel.Name));
                cCellDeselected = bg.AddElement(new ModDiffCell(CellStyle.EditRemoved, ""));
            }
            else
            { 
                cCell = bg.AddElement(new CElement());
                cCellDeselected = bg.AddElement(new ModDiffCell(CellStyle.Unavailable, "(unavailable)"));
            }

            cCell.Hidden = !Selected;
            if (cCellDeselected != null)
            {
                cCell.Embed(cCellDeselected);
                cCellDeselected.Hidden = Selected;
            }

            // right
            CElement rCell;
            if (Item.Change != ChangeType.Removed)
            {

                rCell = bg.AddElement(new ModDiffCell(Item.RightCellStyle(), Item.ModModel.Right.name));
            }
            else
            {
                rCell = bg.AddElement(new CElement());
            }

            this.StackLeft(lCell, 2, (cCell, lCell.width), 2, (rCell, lCell.width));
        }

        public override void DoContent()
        {
            Widgets.DrawHighlightIfMouseover(BoundsRounded);

            base.DoContent();

        }
    }

}
