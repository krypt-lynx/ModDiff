﻿// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GuiMinilib
{
    public partial class CElement
    {
        public Rect bounds { get; private set; }

        public virtual Vector2 tryFit(Vector2 size) { return Vector2.zero; }

        public void DoElementContent()
        {
            DoContent();

            foreach (var element in elements)
            {
                element.DoElementContent();
            }
        }

        public virtual void DoContent() { }

        static Texture2D debugBg = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.3f));
        static Color debugTextColor = new Color(0, 0, 0, 0.5f);

        public virtual string debugDesc() { return ""; }

        public virtual void DoDebugOverlay()
        {

            GUI.DrawTexture(bounds, debugBg);
            GuiTools.UsingFont(GameFont.Small, () => GuiTools.UsingColor(debugTextColor, () => Widgets.Label(bounds, NamePrefix())));
            TooltipHandler.TipRegion(bounds, $"{NamePrefix()}{debugDesc()}:\n{bounds}");
            foreach (var element in elements)
            {
                element.DoDebugOverlay();
            }
        }
    }
}
