using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModDiff.GuiMinilib
{
    public partial class CElement
    {
        public Rect bounds { get; private set; }

        public virtual Vector2 IntrinsicSize() { return Vector2.zero; }

        public void DoElementContent()
        {
            DoContent();

            foreach (var element in elements)
            {
                element.DoElementContent();
            }
        }

        public virtual void DoContent() { }
    }
}
