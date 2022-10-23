using RWLayout.alpha2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModDiff
{
    class BgElement : CElement
    {
        private bool isEven;
        private bool interactive;

        public BgElement(bool isEven, bool interactive)
        {
            this.isEven = isEven;
            this.interactive = interactive;
        }

        public override void DoContent()
        {
            base.DoContent();

            if (Event.current.type == EventType.Repaint)
            {
                if (!isEven)
                {
                    Widgets.DrawAltRect(BoundsRounded);
                }

                if (interactive)
                {
                    if (Mouse.IsOver(Parent.BoundsRounded))
                    {
                        Widgets.DrawHighlight(BoundsRounded);
                    }
                }            }

        }
    }
}
