using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModDiff
{
    public class HarmonyPatchBuilder
    {
        private Harmony harmony;
        private MethodInfo method;

        private HarmonyMethod prefix;
        private HarmonyMethod postfix;
        private HarmonyMethod transpiller;
        private HarmonyMethod finalizer;

        public HarmonyPatchBuilder(Harmony harmony, MethodInfo method)
        {
            this.harmony = harmony;
            this.method = method;
        }

        public HarmonyPatchBuilder Prefix(HarmonyMethod prefix)
        {
            this.prefix = prefix;
            return this;
        }

        public HarmonyPatchBuilder Postfix(HarmonyMethod postfix)
        {
            this.postfix = postfix;
            return this;
        }

        public HarmonyPatchBuilder Transpiller(HarmonyMethod transpiller)
        {
            this.transpiller = transpiller;
            return this;
        }

        public HarmonyPatchBuilder Finalizer(HarmonyMethod finalizer)
        {
            this.finalizer = finalizer;
            return this;
        }

        public void Patch()
        {
            harmony.Patch(method, prefix, postfix, transpiller, finalizer);
        }
    }

    public static class HarmonyExtension
    {
        public static HarmonyPatchBuilder Method(this Harmony harmony, MethodInfo method)
        {
            return new HarmonyPatchBuilder(harmony, method);
        }
    }

}
