using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModDiff.GuiMinilib
{
    public static class TextTools
    {
        static Stack<GameFont> fonts = new Stack<GameFont>();
        public static void FontPush(GameFont font)
        {
            fonts.Push(Text.Font);
            Text.Font = font;
        }
        public static void FontPop()
        {
            Text.Font = fonts.Pop();
        }

    }
}
