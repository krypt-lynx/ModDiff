// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using Cassowary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GuiMinilib
{

    public class CListingRow : CElementHost
    {
        private ClStayConstraint leftStay = null;
        private ClStayConstraint rightStay = null;
        private ClStayConstraint topStay = null;


        public override void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            leftStay = CreateStayConstrait(left, InRect.xMin, ClStrength.Required);
            rightStay = CreateStayConstrait(right, InRect.xMax, ClStrength.Required);
            topStay = CreateStayConstrait(top, InRect.yMin, ClStrength.Required);

            base.UpdateLayoutConstraints(solver);
        }

        public override void UpdateLayout()
        {
            UpdateStayConstrait(ref leftStay, InRect.xMin);
            UpdateStayConstrait(ref rightStay, InRect.xMax);
            UpdateStayConstrait(ref topStay, InRect.yMin);

            base.UpdateLayout();

            solver.Solve();
        }
    }
}
