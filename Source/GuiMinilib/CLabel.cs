using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModDiff.GuiMinilib
{
    public class CLabel : CElement
    {
        public string Title;
        public GameFont Font = GameFont.Small;

        public override void DoContent()
        {
            TextTools.FontPush(Font);
            Widgets.Label(bounds, Title);
            TextTools.FontPop();
        }
    }
}
