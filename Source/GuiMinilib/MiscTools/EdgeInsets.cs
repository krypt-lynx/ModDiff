﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDiff.GuiMinilib
{
    public struct EdgeInsets
    {
        public EdgeInsets(double top, double right, double bottom, double left)
        {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }
        public EdgeInsets(double margin)
        {
            top = margin;
            right = margin;
            bottom = margin;
            left = margin;
        }
        public static EdgeInsets Zero = new EdgeInsets(0);

        public double top, right, bottom, left;

        public override string ToString()
        {
            return ((FormattableString)$"{{{top}, {right}, {bottom}, {left}}}").ToString(CultureInfo.InvariantCulture);
        }
    }
}
