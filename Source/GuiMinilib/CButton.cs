// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModDiff.GuiMinilib
{
    public class CButton : CElement
    {
        public TaggedString Title { get; internal set; }
        public Action<CElement> Action { get; internal set; }

        public override void DoContent()
        {
            if (Widgets.ButtonText(bounds, Title, doMouseoverSound: true))
            {
                this.Action?.Invoke(this);
            }
        }
    }
}
