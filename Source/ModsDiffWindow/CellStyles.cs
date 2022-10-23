using RWLayout.alpha2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModDiff
{
    public struct CellStyleData
    {
        public string marker;
        public Color? bgColorFar;
        public Color? bgColorNear;
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

    static class CellStyles
    {
        private static Dictionary<CellStyle, CellStyleData> styles = null;

        private static void InitStyles()
        {
            CellStyleData defaultModStyle;
            CellStyleData missingModStyle;
            CellStyleData removedModCellStyle;
            CellStyleData editRemovedModCellStyle;
            CellStyleData addedModCellStyle;
            CellStyleData editAddedModCellStyle;
            CellStyleData movedModCellStyle;
            CellStyleData editMovedModCellStyle;
            CellStyleData unavaliableModStyle;

            if (ModDiff.Settings.use_1_4_style)
            {
                defaultModStyle = new CellStyleData
                {
                    textColor = Color.white,
                };
            }
            else
            {
                defaultModStyle = new CellStyleData
                {
                    textColor = Color.white,
                    insets = new EdgeInsets(2, 2, 2, 5),
                };
            }

            unavaliableModStyle = new CellStyleData
            {
                textColor = new Color(1, 1, 1, 0.5f),
            };

            if (!ModDiff.Settings.alternativePallete)
            {
                if (ModDiff.Settings.use_1_4_style)
                {
                    removedModCellStyle = new CellStyleData()
                    {
                        marker = "-",
                        bgColorFar = new Color(0.38f, 0.07f, 0.09f),
                        outlineColor = new Color(0.5f, 0.17f, 0.17f, 0.70f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    editRemovedModCellStyle = new CellStyleData()
                    {
                        bgColorFar = new Color(0.38f, 0.07f, 0.09f, 0.30f),
                        outlineColor = new Color(0.5f, 0.17f, 0.17f, 0.30f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    addedModCellStyle = new CellStyleData()
                    {
                        marker = "+",
                        bgColorFar = new Color(32f / 255, 102f / 255, 26f / 255),
                        outlineColor = new Color(0.17f, 0.45f, 0.17f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    editAddedModCellStyle = new CellStyleData()
                    {
                        bgColorFar = new Color(32f / 255, 102f / 255, 26f / 255, 0.30f),
                        outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    movedModCellStyle = new CellStyleData()
                    {
                        marker = "*",
                        bgColorFar = new Color(102f / 255, 89f / 255, 24 / 255f),
                        outlineColor = new Color(0.38f, 0.36f, 0.15f, 0.70f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    editMovedModCellStyle = new CellStyleData()
                    {
                        bgColorFar = new Color(102f / 255, 89f / 255, 24 / 255f, 0.30f),
                        outlineColor = new Color(0.38f, 0.36f, 0.15f, 0.30f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    missingModStyle = new CellStyleData()
                    {
                        marker = "!",
                        bgColorFar = new Color(38f / 255, 7f / 255, 9f / 255),
                        outlineColor = new Color(0.4f, 0.10f, 0.10f, 0.70f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                }
                else
                {
                    removedModCellStyle = new CellStyleData()
                    {
                        marker = "-",
                        bgColorNear = new Color(0.5f, 0.17f, 0.17f, 0.70f),
                        outlineColor = new Color(0.5f, 0.17f, 0.17f, 0.70f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    editRemovedModCellStyle = new CellStyleData()
                    {
                        bgColorNear = new Color(0.5f, 0.17f, 0.17f, 0.30f),
                        outlineColor = new Color(0.5f, 0.17f, 0.17f, 0.30f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    addedModCellStyle = new CellStyleData()
                    {
                        marker = "+",
                        bgColorNear = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                        outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    editAddedModCellStyle = new CellStyleData()
                    {
                        bgColorNear = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                        outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    movedModCellStyle = new CellStyleData()
                    {
                        marker = "*",
                        bgColorNear = new Color(0.38f, 0.36f, 0.15f, 0.70f),
                        outlineColor = new Color(0.38f, 0.36f, 0.15f, 0.70f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    editMovedModCellStyle = new CellStyleData()
                    {
                        bgColorNear = new Color(0.38f, 0.36f, 0.15f, 0.30f),
                        outlineColor = new Color(0.38f, 0.36f, 0.15f, 0.30f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    missingModStyle = new CellStyleData()
                    {
                        marker = "!",
                        bgColorNear = new Color(0.2f, 0.05f, 0.05f, 0.70f),
                        outlineColor = new Color(0.4f, 0.10f, 0.10f, 0.70f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                }
            }
            else
            {
                if (ModDiff.Settings.use_1_4_style)
                {
                    removedModCellStyle = new CellStyleData()
                    {
                        marker = "-",
                        bgColorFar = new Color(0.45f, 0.10f, 0.45f, 0.70f),
                        outlineColor = new Color(0.45f, 0.10f, 0.45f, 0.70f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    editRemovedModCellStyle = new CellStyleData()
                    {
                        bgColorFar = new Color(0.45f, 0.10f, 0.45f, 0.30f),
                        outlineColor = new Color(0.45f, 0.10f, 0.45f, 0.30f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    addedModCellStyle = new CellStyleData()
                    {
                        marker = "+",
                        bgColorFar = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                        outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    editAddedModCellStyle = new CellStyleData()
                    {
                        bgColorFar = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                        outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    movedModCellStyle = new CellStyleData()
                    {
                        marker = "*",
                        bgColorFar = new Color(0.40f, 0.40f, 0.40f, 0.70f),
                        outlineColor = new Color(0.40f, 0.40f, 0.40f, 0.70f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    editMovedModCellStyle = new CellStyleData()
                    {
                        bgColorFar = new Color(0.40f, 0.40f, 0.40f, 0.30f),
                        outlineColor = new Color(0.40f, 0.40f, 0.40f, 0.30f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                    missingModStyle = new CellStyleData()
                    {
                        marker = "!",
                        bgColorFar = new Color(0.2f, 0.05f, 0.2f, 0.70f),
                        outlineColor = new Color(0.4f, 0.10f, 0.4f, 0.70f),
                        insets = new EdgeInsets(0, 0, 0, 0),
                        textColor = Color.white,
                    };
                }
                else
                {
                    removedModCellStyle = new CellStyleData()
                    {
                        marker = "-",
                        bgColorNear = new Color(0.45f, 0.10f, 0.45f, 0.70f),
                        outlineColor = new Color(0.45f, 0.10f, 0.45f, 0.70f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    editRemovedModCellStyle = new CellStyleData()
                    {
                        bgColorNear = new Color(0.45f, 0.10f, 0.45f, 0.30f),
                        outlineColor = new Color(0.45f, 0.10f, 0.45f, 0.30f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    addedModCellStyle = new CellStyleData()
                    {
                        marker = "+",
                        bgColorNear = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                        outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.70f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    editAddedModCellStyle = new CellStyleData()
                    {
                        bgColorNear = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                        outlineColor = new Color(0.17f, 0.45f, 0.17f, 0.30f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    movedModCellStyle = new CellStyleData()
                    {
                        marker = "*",
                        bgColorNear = new Color(0.40f, 0.40f, 0.40f, 0.70f),
                        outlineColor = new Color(0.40f, 0.40f, 0.40f, 0.70f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    editMovedModCellStyle = new CellStyleData()
                    {
                        bgColorNear = new Color(0.40f, 0.40f, 0.40f, 0.30f),
                        outlineColor = new Color(0.40f, 0.40f, 0.40f, 0.30f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                    missingModStyle = new CellStyleData()
                    {
                        marker = "!",
                        bgColorNear = new Color(0.2f, 0.05f, 0.2f, 0.70f),
                        outlineColor = new Color(0.4f, 0.10f, 0.4f, 0.70f),
                        insets = new EdgeInsets(2, 2, 2, 5),
                        textColor = Color.white,
                    };
                }
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

        public static CellStyleData GetCellStyleData(CellStyle style)
        {
            if (needToReinitStyles)
            {
                InitStyles();
            }
            return styles[style];
        }

        private static bool needToReinitStyles = true;
        internal static void setNeedToReinitStyles()
        {
            needToReinitStyles = true;
        }
    }
}
