using HarmonyLib;
using HarmonyMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ModDiff
{
    public class ModDiff : Mod
    {
        public static string packageIdOfMine = "name.krypt.rimworld.moddiff";

        public ModDiff(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony(packageIdOfMine);

            harmony.Method(AccessTools.Method(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings"))
                .Prefix(new HarmonyMethod(typeof(HarmonyPatches), "MismatchDialogPrefix"))
                .Patch();

        }


    }



}
