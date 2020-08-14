// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

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

        public static void UseFont(GameFont font, Action action)
        {
            FontPush(font);
            action();
            FontPop();
        }

        public static T UseFont<T>(GameFont font, Func<T> func)
        {
            FontPush(font);
            T result = func();
            FontPop();
            return result;
        }
    }
}
