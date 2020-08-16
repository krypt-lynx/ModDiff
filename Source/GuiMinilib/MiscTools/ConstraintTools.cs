// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassowary;
using Verse;

namespace GuiMinilib
{
    struct AnchorMapper
    {
        public Func<CElement, ClVariable> Leading;
        public Func<CElement, ClVariable> Trailing;
        public Func<CElement, ClVariable> SideA;
        public Func<CElement, ClVariable> SideB;
        public Func<CElement, ClVariable> Size;
        public double multipler;
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

        public static void StackLeft(this CElement parent, bool constrainSides = true, bool constrainEnd = true, params object[] items)
        {
            var mapper = new AnchorMapper
            {
                Leading = x => x.left,
                Trailing = x => x.right,
                SideA = x => x.top,
                SideB = x => x.bottom,
                Size = x => x.width,
                multipler = 1,
            };
            Stack(parent, mapper, items, constrainEnd, constrainSides, ClStrength.Strong);
        }

        public static void StackTop(this CElement parent, bool constrainSides, bool constrainEnd, params object[] items)
        {
            var mapper = new AnchorMapper
            {
                Leading = x => x.top,
                Trailing = x => x.bottom,
                SideA = x => x.left,
                SideB = x => x.right,
                Size = x => x.height,
                multipler = 1,
            };
            Stack(parent, mapper, items, constrainEnd, constrainSides, ClStrength.Strong);
        }

        public static void StackRight(this CElement parent, bool constrainSides, bool constrainEnd, params object[] items)
        {
            var mapper = new AnchorMapper
            {
                Leading = x => x.right,
                Trailing = x => x.left,
                SideA = x => x.bottom,
                SideB = x => x.top,
                Size = x => x.width,
                multipler = -1,
            };
            Stack(parent, mapper, items, constrainEnd, constrainSides, ClStrength.Strong);
        }

        public static void StackBottom(this CElement parent, bool constrainSides, bool constrainEnd, params object[] items)
        {
            var mapper = new AnchorMapper
            {
                Leading = x => x.bottom,
                Trailing = x => x.top,
                SideA = x => x.right,
                SideB = x => x.left,
                Size = x => x.height,
                multipler = -1,
            };
            Stack(parent, mapper, items, constrainEnd, constrainSides, ClStrength.Strong);
        }

        private static void Stack(CElement parent, AnchorMapper mapper, IEnumerable<object> items, bool constrainEnd, bool constrainSides, ClStrength strength)
        {
            ClLinearExpression trailing = new ClLinearExpression(mapper.Leading(parent));
            foreach (var item in items)
            {
                CElement element = null;
                ClLinearExpression size = null;
                
                if (item is CElement)
                {
                    element = item as CElement;
                    size = null;
                }
                else
                {
                    var type = item.GetType();
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                    {
                        var maybeElement = type.GetField("Item1").GetValue(item);
                        var maybeVar = type.GetField("Item2").GetValue(item);

                        element = maybeElement as CElement;
                        if (maybeVar is ClVariable)
                        {

                            size = new ClLinearExpression(maybeVar as ClVariable);
                        }
                        else
                        {
                            size = new ClLinearExpression(Convert.ToDouble(maybeVar)); // todo: Exception handling
                        }
                    }
                }

                if (element != null)
                {
                    var child = element;
                    parent.solver.AddConstraint(new ClLinearEquation(trailing, new ClLinearExpression(mapper.Leading(child)), strength));
                    trailing = new ClLinearExpression(mapper.Trailing(child));

                    if (size != null)
                    {
                        parent.solver.AddConstraint(new ClLinearEquation(mapper.Size(child), size, strength));
                    }
                    if (constrainSides)
                    {
                        parent.solver.AddConstraint(mapper.SideA(parent), mapper.SideB(parent), mapper.SideA(child), mapper.SideB(child),
                            (pa, pb, ca, cb) => pa == ca && pb == cb, strength);
                    }
                }
                else if (item is ClVariable)
                {
                    var space = item as ClVariable;
                    trailing = Cl.Plus(trailing, Cl.Times(new ClLinearExpression(space), mapper.multipler));
                }
                else 
                {
                    double space = Convert.ToDouble(item); // todo: Exception handling
                    trailing = Cl.Plus(trailing, new ClLinearExpression(space * mapper.multipler));
                }
            }

            if (constrainEnd)
            {
                parent.solver.AddConstraint(new ClLinearEquation(trailing, new ClLinearExpression(mapper.Trailing(parent)), strength));
            }
        }
    }
}
