// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using Cassowary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModDiff.GuiMinilib
{
    public class CElement
    {
        public static int nextId = 0;

        public int id { get; }
        public CElement()
        {
            id = nextId++;

            string variableNameBase = $"{GetType().Name}_{id}_";

            width = new ClVariable(variableNameBase + "W");
            height = new ClVariable(variableNameBase + "H");
            left = new ClVariable(variableNameBase + "L");
            top = new ClVariable(variableNameBase + "T");
            right = new ClVariable(variableNameBase + "R");
            bottom = new ClVariable(variableNameBase + "B");

        }

        public virtual ClSimplexSolver solver { get { return parent?.solver; } }

        public virtual void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            /*
            solver.AddConstraint(left, width, right, centerX,
                (l, w, r, c) => l + w == r && c == (l + r) / 2
                );
            solver.AddConstraint(top, height, bottom, centerY,
                (t, h, b, c) => t + h == b && c == (t + b) / 2
                );
                */

            solver.AddConstraint(new ClLinearEquation(right, Cl.Plus(left, new ClLinearExpression(width))));
            solver.AddConstraint(new ClLinearEquation(bottom, Cl.Plus(top, new ClLinearExpression(height))));
            if (centerX != null)
            {
                solver.AddConstraint(new ClLinearEquation(centerX, Cl.Divide(Cl.Plus(left, new ClLinearExpression(right)), new ClLinearExpression(2))));
            }
            if (centerY != null)
            {
                solver.AddConstraint(new ClLinearEquation(centerY, Cl.Divide(Cl.Plus(top, new ClLinearExpression(bottom)), new ClLinearExpression(2))));
            }


            foreach (var constraint in constraints)
            {
                solver.AddConstraint(constraint);
            }
            
            foreach (var element in elements)
            {
                element.UpdateLayoutConstraints(solver);
            }            
        }

        public virtual void PostConstraintsUpdate() {
            foreach (var element in elements)
            {
                element.PostConstraintsUpdate();
            }
        }
        public virtual void UpdateLayout() {
            foreach (var element in elements)
            {
                element.UpdateLayout();
            }
        }
        public virtual void PostLayoutUpdate()
        {
            bounds = new Rect((float)left.Value, (float)top.Value, (float)width.Value, (float)height.Value);

            foreach (var element in elements)
            {
                element.PostLayoutUpdate();
            }
        }


        public List<CElement> elements = new List<CElement>();
        public T AddElement<T>(T element) where T: CElement
        {
            elements.Add(element);
            element.parent_ = new WeakReference(this, false);
            return element;
        }

        WeakReference parent_ = null;
        public CElement parent
        {
            get { return parent_?.IsAlive ?? false ? parent_.Target as CElement : null; }
        }
        // todo: RemoveElement

        public List<ClConstraint> constraints = new List<ClConstraint>();

        public ClVariable width;
        public ClVariable height;
        public ClVariable left;
        public ClVariable top;
        public ClVariable right;
        public ClVariable bottom;

        // non-esential variables
        private ClVariable centerX_;
        private ClVariable centerY_;

        public ClVariable centerX {
            get
            {
                if (centerX_ == null)
                {
                    string variableNameBase = $"{GetType().Name}_{id}_";
                    centerX_ = new ClVariable(variableNameBase + "cX");
                }
                return centerX_;
            }
        }
        public ClVariable centerY
        {
            get {
                if (centerY_ == null)
                {
                    string variableNameBase = $"{GetType().Name}_{id}_";
                    centerY_ = new ClVariable(variableNameBase + "cY");
                }
                return centerY_;
            }
        }

        public Rect bounds { get; private set; }

        public void DoElementContent()
        {
            DoContent();

            foreach (var element in elements)
            {
                element.DoElementContent();
            }
        }

        public virtual void DoContent() { }

        public virtual Rect IntristicSize() { return Rect.zero; }

        // todo: intristic size
    }
}
