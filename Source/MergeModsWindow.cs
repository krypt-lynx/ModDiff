using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diff;
using RimWorld;
using RWLayout.moddiff;
using UnityEngine;
using Verse;
using Cassowary_moddiff;

namespace ModDiff
{
    class MergeListRow : CListingRow
    {
        public DiffListItem Change;
        public int Index;
        public ModModel Model { get => Change.ModModel; }

        bool activated = false;
        public bool Selected
        {
            set
            {
                activated = value;
                cCell.Hidden = !activated;
                if (cCellDeselected != null)
                {
                    cCellDeselected.Hidden = activated;
                }
            }
            get => activated;
        }

        ModDiffCell cCell;
        ModDiffCell cCellDeselected;


        public MergeListRow(DiffListItem change, int index)
        {
            Change = change;
            Index = index;
            activated = change.Selected;
            Construct();
        }

        private void Construct()
        {
            string tip = "packadeId:\n" + Change.ModModel.PackageId;
            CElement bg = null;
            if (Index % 2 == 1)
            {
                bg = AddElement(new CWidget
                {

                    DoWidgetContent = (_, bounds) =>
                    {
                        Widgets.DrawAltRect(bounds);
                        TooltipHandler.TipRegion(bounds, tip);
                    }
                });
            }
            else
            {
                bg = AddElement(new CWidget
                {
                    DoWidgetContent = (_, bounds) =>
                    {
                        TooltipHandler.TipRegion(bounds, tip);
                    }
                });
            }
            bg.userInteractionEnabled = false;
            this.Embed(bg);


            // left
            CElement lCell;
            if (Change.Change != ChangeType.Added)
            {
                lCell = bg.AddElement(new ModDiffCell(Change.LeftCellStyle(), Change.ModModel.Left.name));
            }
            else
            {
                lCell = bg.AddElement(new CElement());
            }

            // center
            if (!Model.IsMissing)
            {
                cCell = bg.AddElement(new ModDiffCell(Change.MiddleCellStyle(), Change.ModModel.Name));
                cCell.Hidden = !Selected;

                if (Change.Change != ChangeType.Unmodified && !Change.ModModel.IsMoved)
                {
                    cCellDeselected = bg.AddElement(new ModDiffCell(CellStyle.EditRemoved, ""));
                    cCellDeselected.Hidden = Selected;
                    cCell.Embed(cCellDeselected);
                }
            }
            else
            {
                cCell = bg.AddElement(new ModDiffCell(CellStyle.Unavailable, "(unavailable)"));
            }

            // right
            CElement rCell;
            if (Change.Change != ChangeType.Removed)
            {

                rCell = bg.AddElement(new ModDiffCell(Change.RightCellStyle(), Change.ModModel.Right.name));
            }
            else
            {
                rCell = bg.AddElement(new CElement());
            }

            this.StackLeft(lCell, 2, (cCell, lCell.width), 2, (rCell, lCell.width));
        }

        public override void DoContent()
        {
            Widgets.DrawHighlightIfMouseover(BoundsRounded);

            base.DoContent();
            
        }
    }


    class MergeModsWindow : CWindow
    {

        private ModDiffModel model;
        DragTracker clickTracker;

        CListView modsList;

        public MergeModsWindow(ModDiffModel model)
        {
            this.model = model;
            clickTracker = new DragTracker(Gui);
        }

        public override void ConstructGui()
        {
            base.ConstructGui();
            this.absorbInputAroundWindow = true;

            modsList = Gui.AddElement(new CListView());
            modsList.Margin = new EdgeInsets(2, 0, 2, 0);

            var activeFrame = modsList.Background.AddElement(new CFrame
            {
                Color = new Color(1,1,1, 0.5f),
                Insets = new EdgeInsets(2)
            });

            modsList.Background.AddConstraints(
                activeFrame.top ^ modsList.Background.top,
                activeFrame.bottom ^ modsList.Background.bottom,
                activeFrame.left ^ modsList.Background.left + (modsList.Background.width - 4) / 3 ,
                activeFrame.right ^ modsList.Background.right - (modsList.Background.width - 4) / 3
                );


            PopulateList(modsList);

            Gui.Embed(modsList);
            //Gui.ConstrainSize(1000, 800);


            modsList.AddConstraint(modsList.height <= modsList.intrinsicHeight);

            //Gui.AddConstraint(Gui.width ^ InnerSize.x);
            Gui.AddConstraint(Gui.height <= Gui.AdjustedScreenSize.height * 0.8); // TODO: LayoutGuide
        }

        private void PopulateList(CListView diffList)
        {
            for (int i = 0; i < model.info.Length; i++)
            {
                var line = model.info[i];

                var row = new MergeListRow(line, i);

                diffList.AppendRow(row);
            }
        }

        class ClickTracker
        {
            CElement element;
            CElement clickCandidate = null;

            public ClickTracker(CElement element)
            {
                this.element = element;
            }

            public CElement TrackEvents()
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Mouse.IsInputBlockedNow)
                {
                    clickCandidate = element.hitTest(Event.current.mousePosition);
                }

                if (Event.current.type == EventType.MouseDown && (Event.current.button != 0 || Mouse.IsInputBlockedNow))
                {
                    clickCandidate = null;
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    if (Mouse.IsInputBlockedNow)
                    {
                        clickCandidate = null;
                    }
                    else
                    {
                        var underCursor = element.hitTest(Event.current.mousePosition);
                        if (clickCandidate == underCursor)
                        {
                            return clickCandidate;
                        }
                    }
                }

                return null;
            }
        }

        class DragTracker
        {
            CElement observedElement;
            public CElement Element { get; private set; } = null;
            public bool IsDragging { get; private set; }  = false;
            public bool JustStarted { get; private set; } = false;
            public bool JustEnded { get; private set; } = false;
            public bool Entered { get; private set; } = false;

            public DragTracker(CElement element)
            {
                this.observedElement = element;
            }

            void StartDrag(CElement element)
            {
                Element = element;
                JustStarted = true;
                IsDragging = true;
            }

            void EndDrag()
            {
                IsDragging = false;
                JustEnded = true;
                Element = null;
            }


            bool wasScrolled = false;
            bool postScroll = false;

            public void RegisterEvents()
            {
                wasScrolled = Event.current.type == EventType.ScrollWheel;
            }

            public void ProcessEvents()
            {
                JustStarted = false;
                JustEnded = false;
                Entered = false;

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Mouse.IsInputBlockedNow)
                {
                    Log.Message($"MouseDown start");

                    StartDrag(observedElement.hitTest(Event.current.mousePosition));
                    Event.current.Use();
                }
                else if (IsDragging)
                {
                    if (!Input.GetMouseButton(0))
                    {
                        Log.Message($"Mouse up outside");
                        EndDrag();
                        return;
                    }

                    if (Mouse.IsInputBlockedNow)
                    {
                        return;
                    }

                    if (Event.current.type == EventType.MouseDrag)
                    {
                        Log.Message($"MouseDrag");

                        var newDrag = observedElement.hitTest(Event.current.mousePosition);
                        Event.current.Use();
                        if (newDrag != null && Element != newDrag)
                        {
                            Entered = true;
                            Element = newDrag;
                        }
                    }
                    else if (postScroll)
                    {
                        Log.Message($"Scroll");

                        var newDrag = observedElement.hitTest(Event.current.mousePosition);
                        if (newDrag != null && Element != newDrag)
                        {
                            Entered = true;
                            Element = newDrag;
                        }
                        postScroll = false;
                    }
                    else if (Event.current.type == EventType.MouseDown && (Event.current.button != 0 || Mouse.IsInputBlockedNow))
                    {
                        Log.Message($"MouseUp canceled");
                        EndDrag();
                    }
                    else if (Event.current.type == EventType.MouseUp)
                    {
                        Log.Message($"MouseUp");
                        Event.current.Use();

                        EndDrag();
                    }
                    else if (Event.current.type == EventType.MouseMove)
                    {
                        Log.Message($"MouseMove");
                        EndDrag();
                    }

                }

                postScroll = wasScrolled; // view geometly is ctually updated on next update
                wasScrolled = false;
            }
        }

        bool DraggedFlag;
        public override void DoWindowContents(Rect inRect)
        {
            clickTracker.RegisterEvents();

            base.DoWindowContents(inRect);

            ProcessListClick();
        }

        int lastInteractedIndex = -1;
        private void ProcessListClick()
        {
            clickTracker.ProcessEvents();

            if (clickTracker.JustEnded)
            {
                lastInteractedIndex = -1;
            }

            {
                if (clickTracker.Element is MergeListRow row) // todo: mousedowm outside of a row
                {
                    if (clickTracker.JustStarted)
                    {
                        DraggedFlag = row.Change.Selected;
                        lastInteractedIndex = row.Index;
                    }
                }
                else
                {
                    lastInteractedIndex = -1;
                }
            }

            {
                var element = clickTracker.Element;

                if (element is MergeListRow row && !row.Model.IsMissing)
                {
                    //Log.Message($"lastInteractedIndex = {lastInteractedIndex }; row.Index {row.Index}");
                    if (lastInteractedIndex == -1)
                    {
                        ProcessSelection(row);
                    }
                    else if (lastInteractedIndex <= row.Index)
                    {
                        for (int i = lastInteractedIndex; i <= row.Index; i++)
                        {
                            ProcessSelection(modsList.Rows[i] as MergeListRow);
                        }
                    }
                    else
                    {
                        for (int i = lastInteractedIndex; i >= row.Index; i--)
                        {
                            ProcessSelection(modsList.Rows[i] as MergeListRow);
                        }
                    }

                    lastInteractedIndex = row.Index;
                }
            }
        }

        private void ProcessSelection(MergeListRow row)
        {

            bool wasSelected = DraggedFlag;
            if (wasSelected)
            {
                row.Change.Selected = false;
                row.Selected = row.Change.Selected;
            }
            else
            {
                var left = row.Model.LeftIndex;
                var right = row.Model.RightIndex;

                if (left != -1)
                {
                    model.info[left].Selected = false;
                }
                if (right != -1)
                {
                    model.info[right].Selected = false;
                }

                row.Change.Selected = true;

                if (left != -1)
                {
                    (modsList.Rows[left] as MergeListRow).Selected = model.info[left].Selected;
                }
                if (right != -1)
                {
                    (modsList.Rows[right] as MergeListRow).Selected = model.info[right].Selected;
                }

            }

        }
    }
}
