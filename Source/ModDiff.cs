using HarmonyLib;
using RWLayout.alpha2;
using System;
using System.Collections.Generic;
using System.IO;
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


    public class ModDiff : CMod
    {
        public static Settings Settings { get; private set; }

        private static bool debug = false;

        static string commitInfo = null;
        public static string CommitInfo => debug ? (commitInfo + "-dev") : commitInfo;
        public static bool CassowaryPackaged = true;

        public static CMod Instance = null;
        public static string PackageIdOfMine
        {
            get
            {
                return Instance.Content?.PackageId;
            }
        }

        public ModDiff(ModContentPack content) : base(content)
        {
            Instance = this;
            ReadModInfo(content);
            Settings = GetSettings<Settings>();

            Harmony harmony = new Harmony(PackageIdOfMine);

            harmony.Patch(AccessTools.Method(typeof(ScribeMetaHeaderUtility), "TryCreateDialogsForVersionMismatchWarnings"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), "TryCreateDialogsForVersionMismatchWarnings_Prefix"));
        }

        private static void ReadModInfo(ModContentPack content)
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;

            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream(name + ".git.txt"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    commitInfo = reader.ReadToEnd()?.TrimEndNewlines();
                }
            }
            catch
            {
                commitInfo = null;
            }

            debug = PackageIdOfMine.EndsWith(".dev");
        }

        public override string SettingsCategory()
        {
            return "Mod Diff";
        }


        public override void ConstructGui()
        {
            Gui.StackTop(StackOptions.Create(intrinsicIfNotSet: true, constrainEnd: false),
             Gui.AddElement(new CCheckboxLabeled
             {
                 Title = "IgnoreSelfTitle".Translate(),
                 Checked = Settings.ignoreSelf,
                 Changed = (_, value) => Settings.ignoreSelf = value,
             }), 2,
             Gui.AddElement(new CCheckboxLabeled
             {
                 Title = "KeepSelfLoaded".Translate(),
                 Checked = Settings.selfPreservation,
                 Changed = (_, value) => Settings.selfPreservation = value,
             }), 2,
             Gui.AddElement(new CCheckboxLabeled
             {
                 Title = "AlternativePalette".Translate(),
                 Checked = Settings.alternativePallete,
                 Changed = (_, value) => Settings.alternativePallete = value,
             })
            );

            var footer = Gui.AddElement(new CLabel
            {
                Title = $"Version: {CommitInfo}",
                TextAlignment = TextAnchor.LowerRight,
                Color = new Color(1, 1, 1, 0.5f),
                Font = GameFont.Tiny
            });

            Gui.AddConstraints(
                footer.top ^ Gui.bottom + 3,
                footer.width ^ footer.intrinsicWidth,
                footer.right ^ Gui.right,
                footer.height ^ footer.intrinsicHeight);

        }

    }



}
