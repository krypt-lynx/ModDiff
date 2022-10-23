using Diff;
using RWLayout.alpha2;
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
                    cCellDeselected.Hidden = value;
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
            bool isEven = Index % 2 == 0;


            // left
            CElement lCell;
            if (Item.Change != ChangeType.Added)
            {
                var c = new ModDiffCell(
                    Item.LeftCellStyle(),
                    Item.ModModel.Left.Name,
                    isEven,
                    true,
                    "packadeId:\n" + Item.ModModel.Left.PackageId);
                if (Item.ModModel.Left.Source != ContentSource.Undefined)
                {
                    c.infoIcon = Item.ModModel.Left.Source.GetIcon();
                    c.showWarning = !Item.ModModel.Left.Compatible;
                }

                lCell = AddElement(c);

            }
            else
            {
                lCell = AddElement(new BgElement(isEven, true));
            }

            // center
            if (Model.IsMoved)
            {
                var c = new ModDiffCell(
                    Item.MiddleCellStyle(),
                    Item.ModModel.Name,
                    isEven,
                    true,
                    "packadeId:\n" + Item.ModModel.PackageId,
                    Item.ModModel.IsRequired);
                if (Item.ModModel.Any.Source != ContentSource.Undefined)
                {
                    c.infoIcon = Item.ModModel.Any.Source.GetIcon();
                    c.showWarning = !Item.ModModel.Any.Compatible;
                }

                cCell = AddElement(c);
                cCellDeselected = AddElement(new ModDiffCell(CellStyle.EditRemoved, "", isEven, true));
            }
            else if (!Model.IsMissing)
            {
                var c = new ModDiffCell(
                    Item.MiddleCellStyle(),
                    Item.ModModel.Name,
                    isEven,
                    true,
                    "packadeId:\n" + Item.ModModel.PackageId,
                    Item.ModModel.IsRequired);
                if (Item.ModModel.Any.Source != ContentSource.Undefined)
                {
                    c.infoIcon = Item.ModModel.Any.Source.GetIcon();
                    c.showWarning = !Item.ModModel.Any.Compatible;
                }

                cCell = AddElement(c);

                cCellDeselected = AddElement(new ModDiffCell(CellStyle.EditRemoved, "", isEven, true));
            }
            else
            { 
                cCell = AddElement(new BgElement(isEven, true));
                cCellDeselected = AddElement(new ModDiffCell(CellStyle.Unavailable, "(unavailable)", isEven, true));
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
                var c = new ModDiffCell(
                    Item.RightCellStyle(),
                    Item.ModModel.Right.Name,
                    isEven,
                    true,
                    "packadeId:\n" + Item.ModModel.Right.PackageId);
                if (Item.ModModel.Right.Source != ContentSource.Undefined)
                {
                    c.infoIcon = Item.ModModel.Right.Source.GetIcon();
                    c.showWarning = !Item.ModModel.Right.Compatible;
                }

                rCell = AddElement(c);
            }
            else
            {
                rCell = AddElement(new BgElement(isEven, true));
            }

            lCell.userInteractionEnabled = false;
            cCell.userInteractionEnabled = false;
            cCellDeselected.userInteractionEnabled = false;
            rCell.userInteractionEnabled = false;

            this.StackLeft(lCell, 2, (cCell, lCell.width), 2, (rCell, lCell.width));
        }

        public override void DoContent()
        {
            base.DoContent();
        }
    }

}
