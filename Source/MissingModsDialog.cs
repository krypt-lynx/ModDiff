using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassowary_moddiff;
using RWLayout.moddiff;
using UnityEngine;
using Verse;

namespace ModDiff
{
    class MissingModsDialog : CWindow, IListViewDataSource
    {
        private ModModel[] missingMods;
        private Action AcceptAction;

        public MissingModsDialog(IEnumerable<ModModel> missingMods, Action acceptAction)
        {
            this.AcceptAction = acceptAction;
            this.missingMods = missingMods.ToArray();
        }

        public override void ConstructGui()
        {
            base.ConstructGui();

            var cellSize = new Vector2(
                missingMods.Max(x => Text.CalcSize(x.Name).x + ModDiffCell.MarkerWidth),
                Text.LineHeight);

            InnerSize = new Vector2(Math.Max(350, cellSize.x + ModDiffCell.MarkerWidth + 16), 800);

            this.absorbInputAroundWindow = true;



            var titleLabel = Gui.AddElement(new CLabel
            {
                Font = GameFont.Medium,
                Title = "MissingModsWarningTitle".Translate()
            });
            var disclaimerLabel = Gui.AddElement(new CLabel
            {
                Title = "MissingModsWarningText".Translate(),
                WordWrap = true,
                //Multiline = true
            });

            var missingList = Gui.AddElement(new CListView_vNext());

            var buttonPanel = Gui.AddElement(new CElement());
            var backButton = buttonPanel.AddElement(new CButton
            {
                Title = "GoBack".Translate(),
                Action = (_) => Close(true)
            });
            var reloadButton = buttonPanel.AddElement(new CButton
            {
                Title = "ChangeLoadedMods".Translate(),
                Action = (_) => AcceptAction(),
            });

            Gui.StackTop((titleLabel, 42), (disclaimerLabel, disclaimerLabel.intrinsicHeight), 10, missingList, 12, (buttonPanel, 40));
            buttonPanel.StackLeft(backButton, 20, reloadButton);

            buttonPanel.AddConstraint(backButton.width >= backButton.intrinsicWidth);
            buttonPanel.AddConstraint(reloadButton.width >= reloadButton.intrinsicWidth);

            buttonPanel.AddConstraints(ClStrength.Medium, backButton.width + 20 ^ reloadButton.width);

            missingList.DataSource = this;

            missingList.AddConstraint(missingList.height <= missingList.intrinsicHeight);

            //Gui.AddConstraint(Gui.width ^ InnerSize.x);
            Gui.AddConstraint(Gui.height <= Gui.AdjustedScreenSize.height * 0.8); // TODO: LayoutGuide
        }

        public float HeightForRowAt(int index)
        {
            return ModDiffCell.DefaultHeight;
        }

        public CListingRow ListingRowForRowAt(int index)
        {
            var mod = missingMods[index];
            var row = new CListingRow();
            var cell = row.AddElement(new ModDiffCell(CellStyle.Missing, mod.Name));
            row.Embed(cell);

            return row;
        }

        public int NumberOfRows()
        {
            return missingMods.Length;
        }
    }
}
