using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModDiff.GuiMinilib
{
    public partial class CElement
    {
        public List<CElement> elements = new List<CElement>();
        public T AddElement<T>(T element) where T : CElement
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
    }
}
