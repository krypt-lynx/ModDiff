using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassowary_moddiff;
using RWLayout.moddiff;
using UnityEngine;
using Verse;

namespace ModDiff
{
    public struct CellStyleData
    {
        public bool drawBg;
        public string marker;
        public Color bgColor;
        public Color outlineColor;
        public Color textColor;
        public EdgeInsets insets;
    }

    public enum CellStyle
    {
        Default,
        Missing,
        Removed,
        Added,
        Moved,

        Unavailable,
        EditAdded,
        EditRemoved,
        EditMoved,
    }


    public class ModDiffCell : CElement
    {
        static Dictionary<CellStyle, CellStyleData> styles = null;

        static public bool NeedInitStyles = true;
        static void InitStyles()
        {
            if (!NeedInitStyles)
            {
                return;
            }

            NeedInitStyles = false;

            CellStyleData defaultModStyle;
            CellStyleData missingModStyle;
            CellStyleData removedModCellStyle;
            CellStyleData editRemovedModCellStyle;
            CellStyleData addedModCellStyle;
            CellStyleData editAddedModCellStyle;
            CellStyleData movedModCellStyle;
            CellStyleData editMovedModCellStyle;
            CellStyleData unavaliableModStyle;

            defaultModStyle = new CellStyleData
            {
                textColor = Color.white,
            };

            unavaliableModStyle = new CellStyleData
            {
                textColor = new Color(1, 1, 1, 0.5f),
            };

            if (!ModDiff.Settings.alternativePallete)
            {
                removedModCellStyle = new CellStyleData()
                {
                    marker = "-",
                    drawBg = true,
                    bgColor = new Color(0.5f, 0.17f, 0.17f, 0.70f),
                    outlineColor = new Color(0.5f, 0.17f, 0.17f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                editRemovedModCellStyle = new CellStyleData()
                {
                    drawBg = true,
                    bgColor = new Color(0.5f, 0.17f, 0.17f, 0.30f),
                    outlineColor = new Color(0.5f, 0.17f, 0.17f, 0.30f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                addedModCellStyle = new CellStyleData()
                {
                    marker = "+",
                    drawBg = true,
                    bgColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                    outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                editAddedModCellStyle = new CellStyleData()
                {
                    drawBg = true,
                    bgColor = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                    outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                movedModCellStyle = new CellStyleData()
                {
                    marker = "*",
                    drawBg = true,
                    bgColor = new Color(0.38f, 0.36f, 0.15f, 0.70f),
                    outlineColor = new Color(0.38f, 0.36f, 0.15f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                editMovedModCellStyle = new CellStyleData()
                {
                    drawBg = true,
                    bgColor = new Color(0.38f, 0.36f, 0.15f, 0.30f),
                    outlineColor = new Color(0.38f, 0.36f, 0.15f, 0.30f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                missingModStyle = new CellStyleData()
                {
                    marker = "!",
                    drawBg = true,
                    bgColor = new Color(0.2f, 0.05f, 0.05f, 0.70f),
                    outlineColor = new Color(0.4f, 0.10f, 0.10f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
            }
            else
            {
                removedModCellStyle = new CellStyleData()
                {
                    marker = "-",
                    drawBg = true,
                    bgColor = new Color(0.45f, 0.10f, 0.45f, 0.70f),
                    outlineColor = new Color(0.45f, 0.10f, 0.45f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                editRemovedModCellStyle = new CellStyleData()
                {
                    drawBg = true,
                    bgColor = new Color(0.45f, 0.10f, 0.45f, 0.30f),
                    outlineColor = new Color(0.45f, 0.10f, 0.45f, 0.30f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                addedModCellStyle = new CellStyleData()
                {
                    marker = "+",
                    drawBg = true,
                    bgColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                    outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                editAddedModCellStyle = new CellStyleData()
                {
                    drawBg = true,
                    bgColor = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                    outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                movedModCellStyle = new CellStyleData()
                {
                    marker = "*",
                    drawBg = true,
                    bgColor = new Color(0.40f, 0.40f, 0.40f, 0.70f),
                    outlineColor = new Color(0.40f, 0.40f, 0.40f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                editMovedModCellStyle = new CellStyleData()
                {
                    drawBg = true,
                    bgColor = new Color(0.40f, 0.40f, 0.40f, 0.30f),
                    outlineColor = new Color(0.40f, 0.40f, 0.40f, 0.30f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
                missingModStyle = new CellStyleData()
                {
                    marker = "!",
                    drawBg = true,
                    bgColor = new Color(0.2f, 0.05f, 0.2f, 0.70f),
                    outlineColor = new Color(0.4f, 0.10f, 0.4f, 0.70f),
                    insets = new EdgeInsets(2, 2, 2, 5),
                    textColor = Color.white,
                };
            }

            styles = new Dictionary<CellStyle, CellStyleData>
            {
                { CellStyle.Default, defaultModStyle },
                { CellStyle.Missing, missingModStyle },
                { CellStyle.Removed, removedModCellStyle },
                { CellStyle.Added, addedModCellStyle },
                { CellStyle.Moved, movedModCellStyle },
                { CellStyle.Unavailable, unavaliableModStyle },
                { CellStyle.EditRemoved, editRemovedModCellStyle },
                { CellStyle.EditAdded, editAddedModCellStyle },
                { CellStyle.EditMoved, editMovedModCellStyle }, 
            };
        }

        public const int MarkerWidth = 16;

        public CellStyle style;
        private string title;

        static float defaultHeight = 0;
        public static float DefaultHeight { 
            get
            {
                if (defaultHeight == 0)
                {
                    GuiTools.PushFont(GameFont.Small);
                    defaultHeight = Text.CalcHeight(" ", 100);
                    GuiTools.PopFont();
                }
                return defaultHeight;
            }
        }

        CellStyleData styleData;
        Rect outlineRect;
        Rect iconRect;
        Rect titleRect;

        public override void PostLayoutUpdate()
        {
            base.PostLayoutUpdate();

            outlineRect = BoundsRounded;
            iconRect = new Rect(BoundsRounded.xMin + 5, BoundsRounded.yMin, 16, BoundsRounded.height);
            titleRect = new Rect(BoundsRounded.xMin + 5 + 16, BoundsRounded.yMin, BoundsRounded.width - 5 - 16 - 2, BoundsRounded.height);
        }

        public override void DoContent()
        {
            base.DoContent();

            if (styleData.drawBg)
            {
                Widgets.DrawBoxSolid(outlineRect, styleData.bgColor);
                GuiTools.UsingColor(styleData.outlineColor, () => GuiTools.Box(outlineRect, styleData.insets));
            }
            GuiTools.PushFont(GameFont.Small);
            GuiTools.PushTextAnchor(TextAnchor.UpperCenter);

            GuiTools.UsingColor(styleData.textColor, () => Widgets.Label(iconRect, styleData.marker));
            Text.Anchor = TextAnchor.UpperLeft;

            GuiTools.UsingColor(styleData.textColor, () => Widgets.Label(titleRect, title));
            GuiTools.PopTextAnchor();
            GuiTools.PopFont();
        }

        public ModDiffCell(CellStyle style, string title, string altIcon = null) : base()
        {
            InitStyles();

            this.style = style;
            this.title = title;

            styleData = styles[style];

            this.AddConstraint(this.height ^ DefaultHeight);
        }
    }
}
