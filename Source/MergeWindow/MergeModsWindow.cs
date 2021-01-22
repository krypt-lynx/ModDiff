using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diff;
using RimWorld;
using RWLayout.alpha2;
using UnityEngine;
using Verse;
using Cassowary;
using Verse.Sound;

namespace ModDiff
{

    class MergeModsWindow : CWindow
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
                TryFitContent = (_) => new Vector2(modsList.IsScrollBarVisible() ? 16 : 0, 0)
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
                Color = new Color(1, 1, 1, 0.5f),
                Insets = new EdgeInsets(2)
            });

            modsList.Background.AddConstraints(
                activeFrame.top ^ modsList.Background.top,
                activeFrame.bottom ^ modsList.Background.bottom,
                activeFrame.left ^ modsList.Background.left + (modsList.Background.width - 4) / 3,
                activeFrame.right ^ modsList.Background.right - (modsList.Background.width - 4) / 3
                );

            modsList.DataSource = model.MergeListDataSource;
            //PopulateList(modsList);

            //Gui.Embed(modsList);
            //Gui.ConstrainSize(1000, 800);

            var buttonPanel = Gui.AddElement(new CElement());
            var backButton = buttonPanel.AddElement(new CButtonText
            {
                Title = "GoBack".Translate(),
                Action = (_) => Close(true)
            });
            var leftButton = buttonPanel.AddElement(new CButtonText
            {
                Title = "LeftMods".Translate(),
                Action = (_) => model.UseLeftList()
            });
            var rightButton = buttonPanel.AddElement(new CButtonText
            {
                Title = "RightMods".Translate(),
                Action = (_) => model.UseRightList()
            });

            var continueButton = buttonPanel.AddElement(new CButtonText
            {
                Title = "LoadModList".Translate(),
                Action = (_) =>
                {
                    if (model.MergedListIsValid())
                    {
                        model.TrySetActiveModsFromMerge();
                        Close(true);
                    }
                    else
                    {
                        Find.WindowStack.Add(
                            Dialog_MessageBox.CreateConfirmation("ModDiffWillBeDisabledText".Translate(), () =>
                            {
                                model.TrySetActiveModsFromMerge();
                                Close(true);
                            }, false, "ModDiffWillBeDisabledTitle")
                        );
                    }
                }
            });

            buttonPanel.StackLeft(backButton, 20.0, leftButton, 10, rightButton, 20.0, continueButton);

            var guide = new CVarListGuide();
            buttonPanel.AddGuide(guide);
            var buttonsWidth = new ClVariable("buttonsWidth");
            guide.Variables.Add(buttonsWidth);

            buttonPanel.AddConstraint(backButton.width ^ buttonsWidth, ClStrength.Medium);
            buttonPanel.AddConstraint(leftButton.width ^ buttonsWidth / 2, ClStrength.Medium);
            buttonPanel.AddConstraint(rightButton.width ^ buttonsWidth / 2, ClStrength.Medium);
            buttonPanel.AddConstraint(continueButton.width ^ buttonsWidth, ClStrength.Medium);

            buttonPanel.AddConstraint(backButton.width >= backButton.intrinsicWidth, ClStrength.Strong);
            buttonPanel.AddConstraint(leftButton.width >= leftButton.intrinsicWidth, ClStrength.Strong);
            buttonPanel.AddConstraint(rightButton.width >= rightButton.intrinsicWidth, ClStrength.Strong);
            buttonPanel.AddConstraint(continueButton.width >= continueButton.intrinsicWidth, ClStrength.Strong);

            buttonPanel.AddConstraint(leftButton.width ^ rightButton.width, ClStrength.Strong);

            modsList.AddConstraint(modsList.height <= modsList.intrinsicHeight);

            Gui.AddConstraint(Gui.width ^ InnerSize.x, ClStrength.Medium);
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

            bool select = !DraggedFlag;

            if (model.TrySetSelected(row, select))
            {
                SoundDefOf.Mouseover_Standard.PlayOneShotOnCamera(null);
            }
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
