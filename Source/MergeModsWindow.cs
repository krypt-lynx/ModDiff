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
using Verse.Sound;

namespace ModDiff
{
    class MergeListRow : CListingRow
    {
        public DiffListItem Item;
        public int Index;
        public ModModel Model { get => Item.ModModel; }

        bool selected = false;
        bool Selected
        {
            set
            {
                selected = value;
                cCell.Hidden = !selected;
                if (cCellDeselected != null)
                {
                    cCellDeselected.Hidden = selected /*|| Model.Selected*/;
                }
            }
            get => selected;
        }

        CElement cCell;
        CElement cCellDeselected;


        public MergeListRow(DiffListItem item, int index)
        {
            Item = item;
            Index = index;
            selected = item.Selected;
            // We are not cleaning memory after window close, so, without weak reference item model will prevent row destruction by GC
            // with weak reference only closure will stay in memory until parent window is closed. We can live with this.
            var weakThis = new WeakReference(this, false); 
            item.OnSelectedChanged = (newSelected) => {
                if (weakThis.IsAlive)
                {
                    (weakThis.Target as MergeListRow).Selected = newSelected;
                }
            };

            Construct();
        }

        private void Construct()
        {
            string tip = "packadeId:\n" + Item.ModModel.PackageId;
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
            if (Item.Change != ChangeType.Added)
            {
                lCell = bg.AddElement(new ModDiffCell(Item.LeftCellStyle(), Item.ModModel.Left.name));
            }
            else
            {
                lCell = bg.AddElement(new CElement());
            }

            // center
            if (!Model.IsMissing)
            {
                cCell = bg.AddElement(new ModDiffCell(Item.MiddleCellStyle(), Item.ModModel.Name));
                cCellDeselected = bg.AddElement(new ModDiffCell(CellStyle.EditRemoved, ""));
            }
            else
            {
                cCell = bg.AddElement(new CElement());
                cCellDeselected = bg.AddElement(new ModDiffCell(CellStyle.Unavailable, "(unavailable)"));
            }
            cCell.Embed(cCellDeselected);
            cCell.Hidden = !Selected;
            cCellDeselected.Hidden = Selected;

            // right
            CElement rCell;
            if (Item.Change != ChangeType.Removed)
            {

                rCell = bg.AddElement(new ModDiffCell(Item.RightCellStyle(), Item.ModModel.Right.name));
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


    class DragTracker
    {
        CElement observedElement;
        public CElement Element { get; private set; } = null;
        public bool IsDragging { get; private set; } = false;
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
                //Log.Message($"MouseDown start");

                StartDrag(observedElement.hitTest(Event.current.mousePosition));
                Event.current.Use();
            }
            else if (IsDragging)
            {
                if (!Input.GetMouseButton(0))
                {
                    //Log.Message($"Mouse up outside");
                    EndDrag();
                    return;
                }

                if (Mouse.IsInputBlockedNow)
                {
                    return;
                }

                if (Event.current.type == EventType.MouseDrag)
                {
                    //Log.Message($"MouseDrag");

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
                    //Log.Message($"Scroll");

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
                    //Log.Message($"MouseUp canceled");
                    EndDrag();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    Log.Message($"MouseUp");
                    //Event.current.Use();

                    EndDrag();
                }
                else if (Event.current.type == EventType.MouseMove)
                {
                    //Log.Message($"MouseMove");
                    EndDrag();
                }

            }

            postScroll = wasScrolled; // view geometly is ctually updated on next update
            wasScrolled = false;
        }
    }

    class MergeModsWindow : CWindow, IListViewDataSource
    {

        private ModDiffModel model;
        DragTracker clickTracker;

        CListView_vNext modsList;

        public MergeModsWindow(ModDiffModel model)
        {
            this.model = model;
            clickTracker = new DragTracker(Gui);


            var cellSize = new Vector2(
                model.modsList.Max(x => Mathf.Max(
                    x.ModModel.Left != null ? Text.CalcSize(x.ModModel.Left.name).x : 0,
                    x.ModModel.Right != null ? Text.CalcSize(x.ModModel.Right.name).x : 0
                    ) + ModDiffCell.MarkerWidth + 7 + 8),
                Text.LineHeight);

            InnerSize = new Vector2(Math.Max(460, cellSize.x * 3 + 4 + 16), 800);
        }

        public override void ConstructGui()
        {
            base.ConstructGui();
            this.absorbInputAroundWindow = true;

            var titleLabel = Gui.AddElement(new CLabel
            {
                Font = GameFont.Medium,
                Title = "EditModListTitle".Translate()
            });
            var disclaimerLabel = Gui.AddElement(new CLabel
            {
                Title = "EditModListText".Translate().RawText.Split('\n').FirstOrDefault(),
                WordWrap = true,
            });

            var headerPanel = Gui.AddElement(new CElement());
            var headerLeft = headerPanel.AddElement(new CLabel
            {
                Font = GameFont.Small,
                Color = new Color(1, 1, 1, 0.3f),
                Title = "SaveGameMods".Translate(), // "Savegame mods:"
            });
            var headerCenter = headerPanel.AddElement(new CLabel
            {
                Font = GameFont.Small,
                Color = new Color(1, 1, 1, 0.3f),
                Title = "MergedMods".Translate(), // "Running mods:"
            });
            var headerRight = headerPanel.AddElement(new CLabel
            {
                Font = GameFont.Small,
                Color = new Color(1, 1, 1, 0.3f),
                Title = "RunningMods".Translate(), // "Running mods:"
            });
            var headerSpacer = headerPanel.AddElement(new CWidget
            {
                TryFitContect = (_) => new Vector2(modsList.IsScrollBarVisible() ? 16 : 0, 0)
            });

            headerPanel.StackLeft(16 + 5, headerLeft, 2 + 16 + 5, 2, (headerCenter, headerLeft.width), 2 + 16 + 5, 2, (headerRight, headerLeft.width), 2, (headerSpacer, headerSpacer.intrinsicWidth));

            var headerLine = Gui.AddElement(new CWidget
            {
                DoWidgetContent = (_, bounds) => GuiTools.UsingColor(new Color(1f, 1f, 1f, 0.2f), () => Widgets.DrawLineHorizontal(bounds.x, bounds.y, bounds.width - (modsList.IsScrollBarVisible() ? 16 : 0)))
            });


            modsList = Gui.AddElement(new CListView_vNext());
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

            modsList.DataSource = this;
            //PopulateList(modsList);

            //Gui.Embed(modsList);
            //Gui.ConstrainSize(1000, 800);

            var buttonPanel = Gui.AddElement(new CElement());
            var backButton = buttonPanel.AddElement(new CButton
            {
                Title = "GoBack".Translate(),
                Action = (_) => Close(true)
            });
            var resetButton = buttonPanel.AddElement(new CButton
            {
                Title = "Reset".Translate(),
                Action = (_) => model.ResetMerge()
            });

            var continueButton = buttonPanel.AddElement(new CButton
            {
                Title = "Load".Translate(),
                Action = (_) =>
                {
                    //confirmedAction?.Invoke();
                    Close(true);
                }
            });

            buttonPanel.StackLeft(backButton, 10.0, resetButton, 20.0, continueButton);

            var guide = new CVarListGuide();
            buttonPanel.AddGuide(guide);
            var buttonsWidth = new ClVariable("buttonsWidth");
            guide.Variables.Add(buttonsWidth);

            buttonPanel.AddConstraint(backButton.width ^ buttonsWidth, ClStrength.Medium);
            buttonPanel.AddConstraint(resetButton.width ^ buttonsWidth, ClStrength.Medium);
            buttonPanel.AddConstraint(continueButton.width ^ buttonsWidth, ClStrength.Medium);

            buttonPanel.AddConstraint(backButton.width >= backButton.intrinsicWidth, ClStrength.Strong);
            buttonPanel.AddConstraint(resetButton.width >= resetButton.intrinsicWidth, ClStrength.Strong);
            buttonPanel.AddConstraint(continueButton.width >= continueButton.intrinsicWidth, ClStrength.Strong);

            modsList.AddConstraint(modsList.height <= modsList.intrinsicHeight);

            Gui.AddConstraint(Gui.width ^ InnerSize.x);
            Gui.AddConstraint(Gui.height <= Gui.AdjustedScreenSize.height * 0.8);


            Gui.StackTop((titleLabel, 42), (disclaimerLabel, disclaimerLabel.intrinsicHeight), 2, headerPanel, (headerLine, 1), 4, modsList, 12, (buttonPanel, 40));


            headerLeft.AddConstraint(headerLeft.height ^ headerLeft.intrinsicHeight);
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
                        DraggedFlag = row.Item.Selected;
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
            if (row.Model.IsMissing)
            {
                return;
            }

            SoundDef sound = SoundDefOf.Mouseover_Standard;

            bool deselect = DraggedFlag;
            if (deselect)
            {
                if (row.Item.Selected)
                {
                    row.Item.TrySetSelected(false);
                    row.Model.Selected = true;
                    sound.PlayOneShotOnCamera(null);
                }
            }
            else
            {
                var left = row.Model.LeftIndex;
                var right = row.Model.RightIndex;

                if (!row.Item.Selected)
                {
                    if (left != -1)
                    {
                        model.modsList[left].TrySetSelected(false);
                    }
                    if (right != -1)
                    {
                        model.modsList[right].TrySetSelected(false);
                    }

                    row.Item.TrySetSelected(true);
                    row.Model.Selected = true;


                    sound.PlayOneShotOnCamera(null);
                }
            }

        }

        public int NumberOfRows()
        {
            return model.modsList.Length;
        }

        public float HeightForRowAt(int index)
        {
            return ModDiffCell.DefaultHeight;
        }

        public CListingRow ListingRowForRowAt(int index)
        {
            var line = model.modsList[index];

            var row = new MergeListRow(line, index);

            return row;
        }
    }

    static class NullCheck
    {
        /// <summary>
        /// Performs action if obj is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void IfNotNull<T>(this T obj, Action<T> action)
        {
            if (obj != null)
            {
                action(obj);
            }
        }
    }
}
