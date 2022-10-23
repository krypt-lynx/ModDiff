using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassowary;
using RimWorld.IO;
using RWLayout.alpha2;
using UnityEngine;
using Verse;

namespace ModDiff
{
    //[StaticConstructorOnStartup]
    public class ModDiffCell : CElement
    {
        static Resource<Texture2D> lockIcon = new Resource<Texture2D>("UI/Icons/Diff_Lock");
        static Resource<Texture2D> warningOverlay = new Resource<Texture2D>("UI/Icons/Diff_Warning");

        public const int MarkerWidth = 16;
        private CellStyle style;
        private bool isEven;
        private readonly bool interactive;
        private string title;
        private bool drawLock;

        public Resource<Texture2D> infoIcon = null;// new Resource<Texture2D>("UI/Icons/ContentSources/OfficialModsFolder");
        public bool showWarning = false;
        static float minReasonableHeight = 0;
        public static float MinReasonableHeight
        {
            get
            {
                if (minReasonableHeight == 0)
                {
                    GuiTools.PushFont(GameFont.Small);
                    minReasonableHeight = Text.CalcHeight(" ", 100);
                    GuiTools.PopFont();
                }                    
                return minReasonableHeight;
            }
        }

        static float defaultHeight = 0;
        public static float DefaultHeight { 
            get
            {
                if (ModDiff.Settings.use_1_4_style)
                {
                    return 20;
                }
                else
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
        }

        CellStyleData styleData;
        Rect outlineRect;
        Rect diffIconRect;
        Rect titleRect;
        Rect lockRect;
        Rect infoIconRect;
        Rect warningOverlayRect;

        public override void PostLayoutUpdate()
        {
            base.PostLayoutUpdate();

            outlineRect = BoundsRounded;
            var innerRect = BoundsRounded.ContractedBy(styleData.insets);
            var textFix = Math.Max(0, MinReasonableHeight - innerRect.height);

            diffIconRect = new Rect(innerRect.xMin + 3, innerRect.yMin - (textFix / 2), MarkerWidth + 1, innerRect.height + textFix);

            var infoIconOriginRect = new Rect(diffIconRect.xMax, innerRect.yMin, 16, innerRect.height);
            infoIconRect = GuiTools.SizeCenteredIn(
                infoIconOriginRect,
                new EdgeInsets(-1, 1, 1, 0),
                new Vector2(16, 16));

            if (showWarning)
            {
                warningOverlayRect = new Rect(infoIconRect.xMax - 11, infoIconRect.yMin - 3, 13, 13);
            }

            titleRect = new Rect(infoIconOriginRect.xMax, innerRect.yMin - (textFix / 2), innerRect.xMax - infoIconOriginRect.xMax, innerRect.height + textFix);

            if (drawLock)
            {
                lockRect = GuiTools.SizeCenteredIn(diffIconRect, new EdgeInsets(-1, 0, 1, 0), lockIcon.Value.Size());
            }
        }

        public override void DoContent()
        {
            base.DoContent();

            if (Event.current.type == EventType.Repaint)
            {
                if (styleData.bgColorFar.HasValue)
                {
                    Widgets.DrawBoxSolid(outlineRect, styleData.bgColorFar.Value);
                }
                GuiTools.UsingColor(styleData.outlineColor, () => GuiTools.Box(outlineRect, styleData.insets));
                if (styleData.bgColorNear.HasValue)
                {
                    Widgets.DrawBoxSolid(outlineRect, styleData.bgColorNear.Value);
                }

                if (!isEven)
                {
                    Widgets.DrawAltRect(outlineRect);
                }

                if (interactive)
                {
                    if (Mouse.IsOver(Parent.BoundsRounded))
                    {
                        Widgets.DrawHighlight(BoundsRounded);
                    }
                }

                GuiTools.PushFont(GameFont.Small);


                if (drawLock)
                {
                    GUI.DrawTexture(lockRect, lockIcon.Value);
                }
                else
                {
                    GuiTools.PushTextAnchor(TextAnchor.UpperCenter);
                    GuiTools.UsingColor(styleData.textColor, () => Widgets.Label(diffIconRect, styleData.marker));
                    GuiTools.PopTextAnchor();

                }

                if (infoIcon != null)
                {
                    GUI.DrawTexture(infoIconRect, infoIcon.Value);
                }

                if (showWarning)
                {
                    GUI.DrawTexture(warningOverlayRect, warningOverlay.Value);
                }

                GuiTools.PushTextAnchor(TextAnchor.UpperLeft);
                GuiTools.UsingColor(styleData.textColor, () => Widgets.Label(titleRect, title));
                GuiTools.PopTextAnchor();
                GuiTools.PopFont();
            }
        }

        public ModDiffCell(CellStyle style, string title, bool isEven, bool interactive = false, string tip = null, bool altIcon = false, Texture2D infoIcon = null) : base()
        {
            this.style = style;
            this.isEven = isEven;
            this.interactive = interactive;
            this.title = title;
            this.drawLock = altIcon;
            this.infoIcon = infoIcon == null ? null : new Resource<Texture2D>(infoIcon);
            this.Tip = tip;

            styleData = CellStyles.GetCellStyleData(style);

            this.AddConstraint(this.height ^ DefaultHeight);
        }
    }
}
