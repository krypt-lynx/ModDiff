using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassowary;
using Verse;

namespace GuiMinilib
{
    public class CGuiRoot : CElementHost
    {
        public bool FlexibleWidth = false;
        public bool FlexibleHeight = false;

        public Action LayoutUpdated;

       // private ClConstraint topStay = null;
        public override void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            //left.Value = InRect.xMin;
            //topStay = new ClStayConstraint(top, ClStrength.Required);
            solver.AddStay(left, ClStrength.Required);

            right.Value = InRect.xMax;
            solver.AddStay(right, FlexibleWidth ? ClStrength.Weak : ClStrength.Required);


            top.Value = InRect.yMin;
            solver.AddStay(top, ClStrength.Required);

            bottom.Value = InRect.yMax;
            solver.AddStay(bottom, FlexibleHeight ? ClStrength.Weak : ClStrength.Required);

            base.UpdateLayoutConstraints(solver);
        }

        public override void UpdateLayout()
        {
            var edit = solver
                 .BeginEdit(left, right, top, bottom)
                 .SuggestValue(left, InRect.xMin)
                 .SuggestValue(right, InRect.xMax)
                 .SuggestValue(top, InRect.yMin);

            if (!float.IsNaN(InRect.yMax)) // todo: separate row host class
            {
                edit.SuggestValue(bottom, InRect.yMax);
            }


            solver.Solve();

            base.UpdateLayout();
        }

        public override void PostLayoutUpdate()
        {
            base.PostLayoutUpdate();
            Log.Message($"CGuiRoot.PostLayoutUpdate: InRect: {InRect}; bounds: {bounds}");
            Log.Message($"solver state of {NamePrefix()}:\n{solver}");
            LayoutUpdated?.Invoke();
        }
    }

}
