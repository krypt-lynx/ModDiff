using RWLayout.alpha2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModDiff
{

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
                StartDrag(observedElement.hitTest(Event.current.mousePosition));
                Event.current.Use();
            }
            else if (IsDragging)
            {
                if (!Input.GetMouseButton(0))
                {
                    EndDrag();
                    return;
                }

                if (Mouse.IsInputBlockedNow)
                {
                    return;
                }

                if (Event.current.type == EventType.MouseDrag)
                {
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
                    EndDrag();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    //Event.current.Use(); // TODO: why use is commented (there way uncommented log nearby, it was supposed to be commented

                    EndDrag();
                }
                else if (Event.current.type == EventType.MouseMove)
                {
                    EndDrag();
                }

            }

            postScroll = wasScrolled; // view geometly is ctually updated on next update
            wasScrolled = false;
        }
    }

}
