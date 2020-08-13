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


    public class CListingStandart : CElement
    {
        public CListingStandart() : base()
        {
            contentElement = new CListingContentElement(this);
        }

        Listing_Standard listing = new Listing_Standard();
        Rect innerRect;
        Vector2 scrollPosition = Vector2.zero;


        public override void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            base.UpdateLayoutConstraints(solver);
            contentElement.UpdateLayoutConstraints(solver);

            solver.AddConstraint(contentElement.width, width, (a, b) => a == b - 20);

            //solver.AddConstraint(contentElement.height, (a) => a == 10);
            //solver.AddConstraint();
        }

        public override void PostLayoutUpdate()
        {
            base.PostLayoutUpdate();
            contentElement.PostLayoutUpdate();
        }

        CElement contentElement;

        public override void DoContent()
        {
            listing.BeginScrollView(bounds, ref scrollPosition, ref innerRect);

            foreach (var element in contentElement.elements) {
                var rect = listing.GetRect(element.bounds.height);
                element.DoElementContent();
            }

            listing.EndScrollView(ref innerRect);
        }

        internal CElement NewRow()
        {
            var row = contentElement.AddElement(new CElement());
            contentElement.solver.AddConstraint(row.width, contentElement.width, (a, b) => a == b);
            contentElement.EmbedW(row, EdgeInsets.Zero);
            if (contentElement.elements.Count > 1)
            {
                contentElement.solver.AddConstraint(contentElement.elements[contentElement.elements.Count - 2].bottom, row.top,
                    (a, b) => a == b);
            }
            else
            {
                contentElement.solver.AddConstraint(contentElement.top, row.top, (a, b) => a == b);
            }
            
            return row;
        }
    }
}
