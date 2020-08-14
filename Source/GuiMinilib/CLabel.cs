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

        public override Vector2 IntrinsicSize()
        {   
            return TextTools.UseFont(Font, () => Text.CalcSize(Title));
        }

        public override void DoContent()
        {
            TextTools.UseFont(Font, () => {
                Widgets.Label(bounds, Title);
            });
        }
    }
}
