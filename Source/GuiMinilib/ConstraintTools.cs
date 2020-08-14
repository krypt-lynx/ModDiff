// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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
        public static void Embed(this CElement parent, CElement child)
        {
            parent.solver.AddConstraint(new ClLinearEquation(parent.top, new ClLinearExpression(child.top)));
            parent.solver.AddConstraint(new ClLinearEquation(parent.right, new ClLinearExpression(child.right)));
            parent.solver.AddConstraint(new ClLinearEquation(parent.bottom, new ClLinearExpression(child.bottom)));
            parent.solver.AddConstraint(new ClLinearEquation(parent.left, new ClLinearExpression(child.left)));
        }
        
        public static void Embed(this CElement parent, CElement child, EdgeInsets insets)
        {
            parent.solver.AddConstraint(new ClLinearEquation(parent.top, Cl.Minus(child.top, insets.top)));
            parent.solver.AddConstraint(new ClLinearEquation(parent.right, Cl.Plus(child.right, insets.right)));
            parent.solver.AddConstraint(new ClLinearEquation(parent.bottom, Cl.Plus(child.bottom, insets.bottom)));
            parent.solver.AddConstraint(new ClLinearEquation(parent.left, Cl.Minus(child.left, insets.left)));
        }

        public static void EmbedW(this CElement parent, CElement child, EdgeInsets insets)
        {
            parent.solver.AddConstraint(new ClLinearEquation(parent.left, Cl.Minus(child.left, insets.left)));
            parent.solver.AddConstraint(new ClLinearEquation(parent.right, Cl.Plus(child.right, insets.right)));
        }

        public static void EmbedH(this CElement parent, CElement child, EdgeInsets insets)
        {
            parent.solver.AddConstraint(new ClLinearEquation(parent.top, Cl.Minus(child.top, insets.top)));
            parent.solver.AddConstraint(new ClLinearEquation(parent.bottom, Cl.Plus(child.bottom, insets.bottom)));
        }


        public static void EmbedW(this CElement parent, params CElement[] children)
        {
            ClVariable right = parent.left;
            foreach (var child in children)
            {
                parent.solver.AddConstraint(new ClLinearEquation(right, new ClLinearExpression(child.left)));
                right = child.right;
            }

            if (right != parent.left)
            {
                parent.solver.AddConstraint(new ClLinearEquation(right, new ClLinearExpression(parent.right)));
            }            
        }

        public static void EmbedH(this CElement parent, params CElement[] children)
        {
            ClVariable bottom = parent.top;
            foreach (var child in children)
            {
                parent.solver.AddConstraint(new ClLinearEquation(bottom, new ClLinearExpression(child.top)));
                bottom = child.bottom;
            }

            if (bottom != parent.top)
            {
                parent.solver.AddConstraint(new ClLinearEquation(bottom, new ClLinearExpression(parent.bottom)));
            }
        }
    }
}
