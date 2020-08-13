using Cassowary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModDiff.GuiMinilib
{
    public class CGuiRoot : CElement
    {
        public Rect _inRect;
        public Rect InRect
        {
            get { return _inRect; }
            set
            {
                needsUpdateLayout = _inRect != value;
                _inRect = value;
                UpdateLayoutConstraintsIfNeeded();
                UpdateLayoutIfNeeded();
            }
        }

        ClSimplexSolver solver_;
        public override ClSimplexSolver solver
        {
            get
            {
                return solver_;
            }
        }

        
        public CGuiRoot() : base()
        {
            solver_ = new ClSimplexSolver();
            solver.AutoSolve = false;
        }

        bool needsUpdateLayoutConstraints = true;
        public void UpdateLayoutConstraintsIfNeeded()
        {
            if (needsUpdateLayoutConstraints)
            {
                UpdateLayoutConstraints();
            }
        }

        bool needsUpdateLayout = true;
        public void UpdateLayoutIfNeeded()
        {
            if (needsUpdateLayout)
            {
                UpdateLayout();

                PostLayoutUpdate();
            }
        }

        bool fuse = true;
        public void UpdateLayoutConstraints()
        {
            needsUpdateLayoutConstraints = false;

            if (fuse)
            {
                fuse = false;
            }
            else
            {
                Log.Error("UpdateLayoutConstraints called more then once, layout constraint editing is not implemented yet");
                return;
            }

            var timer = new Stopwatch();
            timer.Start();
            UpdateLayoutConstraints(solver);
            timer.Stop();
            Log.Message($"generated in: {timer.Elapsed}");

            timer.Reset();
            timer.Start();
            solver.Solve();
            timer.Stop();
            Log.Message($"solved in: {timer.Elapsed}");
            //Log.Message($"solver: {solver}");
        }


        public override void UpdateLayoutConstraints(ClSimplexSolver solver)
        {
            left.Value = InRect.xMin;
            right.Value = InRect.xMax;
            top.Value = InRect.yMin;
            bottom.Value = InRect.yMax;

            // pinning region dimentions
            solver.AddStay(left);
            solver.AddStay(right);
            solver.AddStay(top);
            solver.AddStay(bottom);

            base.UpdateLayoutConstraints(solver);
        }

        public void UpdateLayout()
        {
            needsUpdateLayout = false;

            UpdateLayoutConstraintsIfNeeded(); // to do: fix unnesessary edit then UpdateLayoutConstraints actually do its thing
            solver
                .BeginEdit(left, right, top, bottom)
                .SuggestValue(left, InRect.xMin)
                .SuggestValue(right, InRect.xMax)
                .SuggestValue(top, InRect.yMin)
                .SuggestValue(bottom, InRect.yMax)
                .EndEdit();
        }

        public override void DoContent()
        {
            // nothing
        }

    }
}
