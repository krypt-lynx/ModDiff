using RWLayout.moddiff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDiff.MergeWindow
{
    class MergeListDataSource : IListViewDataSource
    {
        Verse.WeakReference<ModDiffModel> modDiffModel;
        MergeListRow[] rows;
        int rowsGenerated ;

        public ModDiffModel ModDiffModel
        {
            get => modDiffModel?.Target;
            set 
            {
                modDiffModel = new Verse.WeakReference<ModDiffModel>(value);
                InitRows();
            }
        }

        private void InitRows()
        {
            rows = new MergeListRow[ModDiffModel.modsList.Length];
            rowsGenerated = 0;
        }

        public int NumberOfRows()
        {
            return ModDiffModel.modsList.Length;
        }

        public float HeightForRowAt(int index)
        {
            return ModDiffCell.DefaultHeight;
        }

        public CListingRow ListingRowForRowAt(int index)
        {
            var line = ModDiffModel.modsList[index];

            var row = rows[index];
            if (row == null)
            {
                row = new MergeListRow(line, index);
                rows[index] = row;
            }

            return row;
        }

        public void GenItem()
        {

            if (rowsGenerated < ModDiffModel.modsList.Length)
            {
                if (rows[rowsGenerated] == null)
                {
                    var line = ModDiffModel.modsList[rowsGenerated];
                    var row = new MergeListRow(line, rowsGenerated);
                    rows[rowsGenerated] = row;
                    row.InRect = new UnityEngine.Rect(0, 0, 30, 100);
                    row.UpdateLayoutIfNeeded();
                    rowsGenerated++;
                }
            }
            
        }
    }
}
