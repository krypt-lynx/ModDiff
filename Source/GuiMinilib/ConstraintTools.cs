using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassowary;

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

    }

    public static class ConstraintTools
    {
        public static void Embed(this CElement parent, CElement child, EdgeInsets insets)
        {
            parent.solver.AddConstraint(
                parent.top, parent.right, parent.bottom, parent.left,
                child.top, child.right, child.bottom, child.left,
                (pt, pr, pb, pl, ct, cr, cb, cl) => ct == pt && pr == cr && pb == cb && pl == cl // todo: insets
                );
        }

        public static void EmbedW(this CElement parent, CElement child, EdgeInsets insets)
        {
            parent.solver.AddConstraint(
               parent.left, parent.right,
               child.left, child.right, 

               (pl, pr, cl, cr) => pl == cl && pr == cr // todo: insets
               );
        }

        public static void EmbedH(this CElement parent, CElement child, EdgeInsets insets)
        {
            parent.solver.AddConstraint(
               parent.top, parent.bottom,
               child.top, child.bottom,

               (pl, pr, cl, cr) => pl == cl && pr == cr // todo: insets
               );
        }
    }
}
