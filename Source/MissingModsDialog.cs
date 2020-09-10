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
    class MissingModsDialog : CWindow
    {
        private ModInfo[] missingMods;
        private Action AcceptAction;
        private CellStyleData cellStyle;
        public MissingModsDialog(IEnumerable<ModInfo> missingMods, Action acceptAction, CellStyleData cellStyle)
        {
            this.AcceptAction = acceptAction;
            this.missingMods = missingMods.ToArray();
            this.cellStyle = cellStyle;

            var cellSize = new Vector2(
                missingMods.Max(x => Text.CalcSize(x.name).x + ModDiffCell.MarkerWidth),
                Text.LineHeight);


            InnerSize = new Vector2(Math.Max(350, cellSize.x + ModDiffCell.MarkerWidth + 20), 800);
        }

        public override void ConstructGui()
        {
            base.ConstructGui();

            
            this.absorbInputAroundWindow = true;

            var titleLabel = Gui.AddElement(new CLabel
            {
                Font = GameFont.Medium,
                Title = "MissingModsWarningTitle".Translate()
            });
            var disclaimerLabel = Gui.AddElement(new CLabel
            {
                Title = "MissingModsWarningText".Translate(),
                //Multiline = true
            });

            var missingList = Gui.AddElement(new CListView());

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

            buttonPanel.Solver.AddConstraint(backButton.width >= backButton.intrinsicWidth);
            buttonPanel.Solver.AddConstraint(reloadButton.width >= reloadButton.intrinsicWidth);

            buttonPanel.Solver.AddConstraints(ClStrength.Medium, backButton.width + 20 ^ reloadButton.width);


            GenerateRows(missingList);
            missingList.Solver.AddConstraint(missingList.height <= missingList.intrinsicHeight);

            Gui.Solver.AddConstraint(Gui.width ^ InnerSize.x);
            Gui.Solver.AddConstraint(Gui.height <= Gui.AdjustedScreenSize.height * 0.8); // TODO: LayoutGuide
        }

        private void GenerateRows(CListView missingList)
        {
            foreach (var mod in missingMods)
            {
                var row = new CListingRow();
                missingList.AppendRow(row);
                var cell = row.AddElement(new ModDiffCell(cellStyle, true, mod.name));
                row.Embed(cell);
            }
        }
    }
}
