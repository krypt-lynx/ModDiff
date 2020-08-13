using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModDiff.GuiMinilib
{
    class CWidget : CElement
    {
        public Action<Rect> Do;

        public override void DoContent()
        {
            Do?.Invoke(bounds);
        }
    }
}
