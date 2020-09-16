// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ModDiff
{
    public class Settings : ModSettings
    {
       
        public bool ignoreSelf = false;
        public bool selfPreservation = true;
        public bool alternativePallete = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ignoreSelf, "ignoreSelf", false);
            Scribe_Values.Look(ref selfPreservation, "selfPreservation", true);
            Scribe_Values.Look(ref alternativePallete, "alternativePallete", false);

            base.ExposeData();

            ModDiffCell.NeedInitStyles = true;
        }
    }

    public class ModDiff : Mod
    {
        public static string PackageIdOfMine = null;
        public static Settings Settings { get; private set; }

        public static bool CassowaryPackaged = true;

        public ModDiff(ModContentPack content) : base(content)
        {
            PackageIdOfMine = content.PackageId;
            Settings = GetSettings<Settings>();

            Harmony harmony = new Harmony(PackageIdOfMine);

            harmony.Patch(AccessTools.Method(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), "TryCreateDialogsForVersionMismatchWarnings_Prefix"));               

        }

        public override string SettingsCategory()
        {
            return "Mod Diff";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            var options = new Listing_Standard();
            options.maxOneColumn = true;

            options.Begin(inRect);

            options.CheckboxLabeled("IgnoreSelfTitle".Translate(), ref Settings.ignoreSelf, "IgnoreSelfHint".Translate());
            options.CheckboxLabeled("KeepSelfLoaded".Translate(), ref Settings.selfPreservation);
            options.CheckboxLabeled("AlternativePalette".Translate(), ref Settings.alternativePallete);

            options.End();
        }


    }



}
