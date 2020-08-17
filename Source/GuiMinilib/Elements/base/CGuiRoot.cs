﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassowary;
using Verse;

namespace GuiMinilib
{
    public class CGuiRoot : CElementHost
    {
        public bool WeakWidth = false;
        public bool WeakHeight = false;

        public Action LayoutUpdated;

        private ClStayConstraint leftStay = null;
        private ClStayConstraint rightStay = null;
        private ClStayConstraint topStay = null;
        private ClStayConstraint bottomStay = null;

        public override void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            leftStay = CreateStayConstrait(left, InRect.xMin, ClStrength.Required);
            rightStay = CreateStayConstrait(right, InRect.xMax, WeakWidth ? ClStrength.Weak : ClStrength.Required);
            topStay = CreateStayConstrait(top, InRect.yMin, ClStrength.Required);
            bottomStay = CreateStayConstrait(bottom, InRect.yMax, WeakHeight ? ClStrength.Weak : ClStrength.Required);

            base.UpdateLayoutConstraints(solver);
        }

        public override void UpdateLayout()
        {
            Debug.WriteLine(InRect.width);

            UpdateStayConstrait(ref leftStay, InRect.xMin);
            UpdateStayConstrait(ref rightStay, InRect.xMax);
            UpdateStayConstrait(ref topStay, InRect.yMin);
            UpdateStayConstrait(ref bottomStay, InRect.yMax);

            base.UpdateLayout();

            solver.Solve();
        }

        public override void PostLayoutUpdate()
        {
            base.PostLayoutUpdate();
            Debug.WriteLine(bounds);
            //Log.Message($"CGuiRoot.PostLayoutUpdate: InRect: {InRect}; bounds: {bounds}");
            //Log.Message($"solver state of {NamePrefix()}:\n{solver}");
            LayoutUpdated?.Invoke();
        }
    }

}