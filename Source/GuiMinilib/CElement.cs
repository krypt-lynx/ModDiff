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
        private static int lastId = 0;

        public int id { get; }
        public CElement()
        {
            id = lastId++;

            string variableNameBase = $"{GetType().Name}_{id}_";

            width = new ClVariable(variableNameBase + "W");
            height = new ClVariable(variableNameBase + "H");
            left = new ClVariable(variableNameBase + "L");
            top = new ClVariable(variableNameBase + "T");
            right = new ClVariable(variableNameBase + "R");
            bottom = new ClVariable(variableNameBase + "B");
            centerX = new ClVariable(variableNameBase + "cX");
            centerY = new ClVariable(variableNameBase + "cY");
        }

        public virtual ClSimplexSolver solver { get { return parent?.solver; } }

        public virtual void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            solver.AddConstraint(left, width, right, centerX,
                (l, w, r, c) => l + w == r && c == (l + r) / 2
                );
            solver.AddConstraint(top, height, bottom, centerY,
                (t, h, b, c) => t + h == b && c == (t + b) / 2
                );

            foreach (var constraint in constraints)
            {
                solver.AddConstraint(constraint);
            }
            
            foreach (var element in elements)
            {
                element.UpdateLayoutConstraints(solver);
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
        public ClVariable centerX;
        public ClVariable centerY;

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
