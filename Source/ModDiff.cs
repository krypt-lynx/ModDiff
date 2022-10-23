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
        public bool steamSameAsLocal = false;
#if rw_1_3_or_later
        const bool use_1_4_style_default = true;
#else
        const bool use_1_4_style_default = false;
#endif
        public bool use_1_4_style = use_1_4_style_default;


        public override void ExposeData()
        {
            Scribe_Values.Look(ref ignoreSelf, "ignoreSelf", false);
            Scribe_Values.Look(ref selfPreservation, "selfPreservation", true);
            Scribe_Values.Look(ref alternativePallete, "alternativePallete", false);
            Scribe_Values.Look(ref steamSameAsLocal, "steamSameAsLocal", false);
            Scribe_Values.Look(ref use_1_4_style, "use_1_4_style", use_1_4_style_default);

            base.ExposeData();

            CellStyles.setNeedToReinitStyles();
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
                        
            harmony.Patch(
                AccessTools.Method(typeof(ScribeMetaHeaderUtility), 
                    nameof(ScribeMetaHeaderUtility.TryCreateDialogsForVersionMismatchWarnings)),             
                prefix: new HarmonyMethod(typeof(HarmonyPatches), 
                    nameof(HarmonyPatches.TryCreateDialogsForVersionMismatchWarnings_Prefix)));
            
            
            /*
            harmony.Patch(
                AccessTools.Method(typeof(ScribeMetaHeaderUtility),
                    nameof(ScribeMetaHeaderUtility.TryCreateDialogsForVersionMismatchWarnings)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches),
                    nameof(HarmonyPatches.TryCreateDialogsForVersionMismatchWarnings_transpiler)));
            */
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
                 Tip = "IgnoreSelfHint".Translate(),
                 Checked = Settings.ignoreSelf,
                 Changed = (_, value) => Settings.ignoreSelf = value,
             }), 2,
             Gui.AddElement(new CCheckboxLabeled
             {
                 Title = "SteamSameAsLocal".Translate(),
                 Checked = Settings.steamSameAsLocal,
                 Changed = (_, value) => Settings.steamSameAsLocal = value,
             }), 2,
             Gui.AddElement(new CCheckboxLabeled
             {
                 Title = "KeepSelfLoaded".Translate(),
                 Checked = Settings.selfPreservation,
                 Changed = (_, value) => Settings.selfPreservation = value,
             }), 10,
             Gui.AddElement(new CCheckboxLabeled
             {
                 Title = "Use_1_4_Style".Translate(),
                 Checked = Settings.use_1_4_style,
                 Changed = (_, value) => Settings.use_1_4_style = value,
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
