// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModDiff.GuiMinilib
{
    public class CLabel : CElement
    {
        public string Title;
        public GameFont Font = GameFont.Small;
        public int lines_debug = 1;
        internal Color? Color = null;

        public override Vector2 IntrinsicSize()
        {   
            var result = GuiTools.UsingFont(Font, () => Text.CalcSize(Title));
            result.y = result.y * lines_debug;
            Log.Message($"intinsic size of {NamePrefix()} (\"{Title}\"): {result}");
            return result;
        }

        public override void DoContent()
        {
            GuiTools.UsingFont(Font, () =>
            {
                GuiTools.UsingColor(Color, () => Widgets.Label(bounds, Title));
            });
        }
    }
}
