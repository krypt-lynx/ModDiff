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
        private string Title;

        public ModDiffCell(CellStyle style, string title) : base()
        {
            InitStyles();

            this.style = style;
            this.Title = title;

            var styleData = styles[style];

            if (styleData.drawBg)
            {
                var highlight = this.AddElement(new CWidget
                {
                    DoWidgetContent = (_, bounds) =>
                    {
                        Widgets.DrawBoxSolid(bounds, styleData.bgColor);
                        GuiTools.UsingColor(styleData.outlineColor, () => GuiTools.Box(bounds, styleData.insets));                        
                    }
                });
                this.Embed(highlight);
            }

            var iconSlot = this.AddElement(new CElement());
            if (styleData.marker != null)
            {
                var icon = iconSlot.AddElement(new CLabel { Title = styleData.marker });
                iconSlot.StackTop(StackOptions.Create(constrainSides: false), icon);
                iconSlot.AddConstraint(iconSlot.centerX ^ icon.centerX);
                icon.AddConstraint(icon.width ^ icon.intrinsicWidth);
            }

            var text = this.AddElement(new CLabel
            {
                Title = Title,
                Color = styleData.textColor,
            });

            this.StackLeft(5, (iconSlot, 16), text, 2);

            this.AddConstraint(this.height ^ text.intrinsicHeight);
        }
    }
}
