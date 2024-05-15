using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace QuestReminder
{
    public class QuestReminder : Mod
    {
        internal static Settings settings;

        public QuestReminder(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoWindowContents(inRect);
        }

        public override string SettingsCategory() => "Quest Reminder";
    }
}
