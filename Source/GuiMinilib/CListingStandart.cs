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
    public class CListingStandart : CElement
    {
        Listing_Standard listing = new Listing_Standard();
        Rect innerRect;
        Vector2 scrollPosition = Vector2.zero;

        List<CGuiRoot> rows = new List<CGuiRoot>();

        public bool ShowScroll = true;

        public override void PostConstraintsUpdate()
        {
            base.PostConstraintsUpdate();
            
            float y = 0;
            foreach (var row in rows)
            {
                row.solver.AddConstraint(row.height, h => h == 20, ClStrength.Weak);
                row.InRect = new Rect(0, y, bounds.width - (ShowScroll ? 20 : 0), float.NaN);
                y += row.bounds.height;
            }
        }

        public override void PostLayoutUpdate()
        {
            base.PostLayoutUpdate();

            float y = 0;
            foreach (var row in rows)
            {
                row.InRect = new Rect(0, y, bounds.width - (ShowScroll ? 20 : 0), float.NaN);
                y += row.bounds.height;
            }
        }

        public override void DoContent()
        {
            if (ShowScroll)
            {
                listing.BeginScrollView(bounds, ref scrollPosition, ref innerRect);
            }
            else
            {
                listing.Begin(bounds);
            }

            foreach (var element in rows) {
                var rect = listing.GetRect(element.bounds.height);
                element.DoElementContent();
            }

            if (ShowScroll)
            {
                listing.EndScrollView(ref innerRect);
            } else
            {
                listing.End();
            }
        }

        internal CElement NewRow()
        {
            var row = new CGuiRoot();
            rows.Add(row);

            return row;
        }
    }
}
