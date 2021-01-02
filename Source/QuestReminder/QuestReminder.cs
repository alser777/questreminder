using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace QuestReminder
{
    public class QuestReminder : GameComponent
    {
        private Dictionary<Quest, bool> questNotified = new Dictionary<Quest, bool>();

        private List<Quest> tmpQuests;
        private List<bool> tmpNotified;

        public QuestReminder(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            var toremove = new HashSet<Quest>(questNotified.Keys);

            var questManager = Find.QuestManager;
            Quest notify = null;
            foreach (var quest in questManager.questsInDisplayOrder)
            {
                if (!quest.dismissed && quest.State == QuestState.NotYetAccepted && quest.ticksUntilAcceptanceExpiry > 0)
                {
                    toremove.Remove(quest);

                    bool expiring = (quest.ticksUntilAcceptanceExpiry <= 2500);

                    bool notified;
                    if (!questNotified.TryGetValue(quest, out notified))
                    {
                        questNotified[quest] = expiring;

                        if (Prefs.LogVerbose)
                            Log.Message("Tracking quest: " + quest.name + " (" + expiring + ")");

                        continue;
                    }

                    if (notify == null)
                    {
                        if (expiring && !notified)
                        {
                            questNotified[quest] = true;
                            notify = quest;
                        }
                    }
                }
            }

            foreach (var q in toremove)
            {
                questNotified.Remove(q);

                if (Prefs.LogVerbose)
                    Log.Message("Untracking quest: " + q.name);
            }

            if (notify != null)
            {
                Find.TickManager.Pause();
                NotifyExpiringQuest(notify);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref this.questNotified, "questNotified", LookMode.Reference, LookMode.Value, ref tmpQuests, ref tmpNotified);
        }

        private void NotifyExpiringQuest(Quest quest)
        {
            DiaNode diaNode = new DiaNode("<b>" + quest.name + "</b>\n\n" + quest.description);
            DiaOption diaOption = new DiaOption("ViewQuest".Translate());
            diaOption.resolveTree = true;
            diaOption.action = delegate ()
            {
                Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests, true);
                ((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
            };
            diaNode.options.Add(diaOption);
            diaNode.options.Add(DiaOption.DefaultOK);

            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, "questreminder_expires".Translate()));
        }
    }
}
