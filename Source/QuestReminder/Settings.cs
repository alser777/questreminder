using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace QuestReminder
{
    public class Settings : ModSettings
    {
        private const int RemindMinutesDefault = 60;

        public int RemindMinutes = RemindMinutesDefault;

        private string _remindMinutesBuffer;

        public int RemindTicks => RemindMinutes * 2500 / 60;

        public void DoWindowContents(Rect rect)
        {
            var options = new Listing_Standard();
            options.Begin(rect);
            options.TextFieldNumericLabeled("questreminder_remindminutes".Translate() + ": ", ref RemindMinutes, ref _remindMinutesBuffer, 1, 600);
            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref RemindMinutes, "RemindMinutes", RemindMinutesDefault);

            base.ExposeData();
        }
    }
}
