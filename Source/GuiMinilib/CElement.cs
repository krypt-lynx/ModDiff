// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using Cassowary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModDiff.GuiMinilib
{
    public class CElement
    {
        public static int nextId = 0;
        public string NamePrefix()
        {
            return $"{GetType().Name}_{id}";
        }

        public int id { get; }
        public CElement()
        {
            id = nextId++;

            string variableNameBase = NamePrefix();

            left = new ClVariable(variableNameBase + "_L");
            top = new ClVariable(variableNameBase + "_T");
            right = new ClVariable(variableNameBase + "_R");
            bottom = new ClVariable(variableNameBase + "_B");

            width = new ClVariable(variableNameBase + "_W");
            height = new ClVariable(variableNameBase + "_H");
        }

        public virtual ClSimplexSolver solver { get { return parent?.solver; } }

        public virtual void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            solver.AddConstraint(new ClLinearEquation(right, Cl.Plus(left, new ClLinearExpression(width))));
            solver.AddConstraint(new ClLinearEquation(bottom, Cl.Plus(top, new ClLinearExpression(height))));

            if (centerX_ != null)
            {
                solver.AddConstraint(new ClLinearEquation(centerX, Cl.Divide(Cl.Plus(left, new ClLinearExpression(right)), new ClLinearExpression(2))));
            }
            if (centerY_ != null)
            {
                solver.AddConstraint(new ClLinearEquation(centerY, Cl.Divide(Cl.Plus(top, new ClLinearExpression(bottom)), new ClLinearExpression(2))));
            }

            /*if (intrinsicHeight_ != null)
            {
                solver.AddConstraint(new ClEditConstraint(intrinsicHeight));
            }
            if (intrinsicWidth_ != null)
            {
                solver.AddConstraint(new ClEditConstraint(intrinsicWidth));
            }*/


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
            
            if (intrinsicWidth_ != null || intrinsicHeight_ != null)
            {
                var intrinsicSize = this.IntrinsicSize();

                if (intrinsicWidth_ != null)
                {
                    solver.BeginEdit(intrinsicWidth_)
                        .SuggestValue(intrinsicWidth_, intrinsicSize.x)
                        .EndEdit();

                }
                if (intrinsicHeight_ != null)
                {
                    solver.BeginEdit(intrinsicHeight_)
                        .SuggestValue(intrinsicHeight_, intrinsicSize.y)
                        .EndEdit();
                }
            }
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
        private ClVariable intrinsicWidth_;
        private ClVariable intrinsicHeight_;

        public ClVariable centerX
        {
            get
            {
                if (centerX_ == null)
                {
                    centerX_ = new ClVariable(NamePrefix() + "_cX");
                }
                return centerX_;
            }
        }
        public ClVariable centerY
        {
            get {
                if (centerY_ == null)
                {
                    centerY_ = new ClVariable(NamePrefix() + "_cY");
                }
                return centerY_;
            }
        }
        public ClVariable intrinsicWidth
        {
            get
            {
                if (intrinsicWidth_ == null)
                {

                    intrinsicWidth_ = new ClVariable(NamePrefix() + "_iW");
                    solver.AddStay(intrinsicWidth_);
                }
                return intrinsicWidth_;
            }
        }
        public ClVariable intrinsicHeight
        {
            get
            {
                if (intrinsicHeight_ == null)
                {
                    intrinsicHeight_ = new ClVariable(NamePrefix() + "_iH");
                    solver.AddStay(intrinsicHeight_);
                }
                return intrinsicHeight_;
            }
        }

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
        
        // todo: intristic size
    }
}
