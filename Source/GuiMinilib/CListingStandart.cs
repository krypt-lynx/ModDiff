// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassowary;
using UnityEngine;
using Verse;

namespace ModDiff.GuiMinilib
{
/*
    class CListingContentElement : CElement
    {

        public CListingContentElement(CListingStandart owner)
        {
            owner_ = new WeakReference(owner, false);
        }

        WeakReference owner_ = null;
        public CListingStandart owner
        {
            get { return owner_?.IsAlive ?? false ? owner_.Target as CListingStandart : null; }
        }

        public override ClSimplexSolver solver
        {
            get
            {
                return owner?.solver;
            }
        }

        public override void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            left.Value = 0;
            top.Value = 0;
            
            // pinning region dimentions
            solver.AddStay(left);
            solver.AddStay(top);

            base.UpdateLayoutConstraints(solver);
        }
    }
    */
    public class CListingStandart : CElement
    {
        public CListingStandart() : base()
        {
            //contentElement = new CListingContentElement(this);
        }

        Listing_Standard listing = new Listing_Standard();
        Rect innerRect;
        Vector2 scrollPosition = Vector2.zero;

        List<CGuiRoot> rows = new List<CGuiRoot>();

        public override void PostConstraintsUpdate()
        {
            base.PostConstraintsUpdate();
            
            float y = 0;
            foreach (var row in rows)
            {
                row.solver.AddConstraint(row.height, h => h == 20, ClStrength.Weak);
                row.InRect = new Rect(0, y, bounds.width - 20, float.NaN);
                y += row.bounds.height;
            }
        }

        public override void PostLayoutUpdate()
        {
            base.PostLayoutUpdate();

            float y = 0;
            foreach (var row in rows)
            {
                row.InRect = new Rect(0, y, bounds.width - 20, float.NaN);
                y += row.bounds.height;
            }
        }

        //CElement contentElement;

        public override void DoContent()
        {
            listing.BeginScrollView(bounds, ref scrollPosition, ref innerRect);
            
            foreach (var element in rows) {
                var rect = listing.GetRect(element.bounds.height);
                element.DoElementContent();
            }
            
            listing.EndScrollView(ref innerRect);
        }

        internal CElement NewRow()
        {
            var row = new CGuiRoot();
            rows.Add(row);
            //var row = contentElement.AddElement(new CElement());
            //contentElement.solver.AddConstraint(new ClLinearEquation(row.width, new ClLinearExpression(contentElement.width)));
            /*
            contentElement.EmbedW(row);
            if (contentElement.elements.Count > 1)
            {
                contentElement.solver.AddConstraint(new ClLinearEquation(contentElement.elements[contentElement.elements.Count - 2].bottom,
                    new ClLinearExpression(row.top)));
            }
            else
            {
                contentElement.solver.AddConstraint(new ClLinearEquation(contentElement.top, new ClLinearExpression(row.top)));
            }*/
            return row;
        }
    }
}
