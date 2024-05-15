using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace QuestReminder
{
    public class QuestReminderComp : GameComponent
    {
        private class TrackedQuest
        {
            public Quest quest;
            public bool toremove;
        }

        private readonly List<TrackedQuest> tquests = new List<TrackedQuest>();

        public QuestReminderComp(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            foreach (var q in tquests)
                q.toremove = true;

            var remindTicks = QuestReminder.settings.RemindTicks;
            var questManager = Find.QuestManager;
            Quest notify = null;
            foreach (var quest in questManager.questsInDisplayOrder)
            {
                if (IsQuestExpiring(quest, remindTicks))
                {
                    var tq = tquests.Find(q => q.quest == quest);
                    if (tq == null)
                    {
                        if (notify == null)
                        {
                            notify = quest;
                            tquests.Add(new TrackedQuest { quest = quest });
                        }
                    }
                    else
                    {
                        tq.toremove = false;
                    }
                }
            }

            CleanupQuests();

            if (notify != null)
            {
                Find.TickManager.Pause();
                NotifyExpiringQuest(notify);
            }
        }

        private static bool IsQuestExpiring(Quest quest, int remindTicks)
        {
            return !quest.dismissed && quest.State == QuestState.NotYetAccepted && quest.ticksUntilAcceptanceExpiry > 0 && quest.ticksUntilAcceptanceExpiry <= remindTicks;
        }

        private void CleanupQuests()
        {
            var nlast = tquests.Count - 1;
            for (var i = 0; i <= nlast; )
            {
                if (tquests[i].toremove)
                {
                    tquests[i] = tquests[nlast];
                    tquests.RemoveAt(nlast);
                    --nlast;
                }
                else
                {
                    ++i;
                }
            }
        }

        public override void LoadedGame()
        {
            base.LoadedGame();

            var remindTicks = QuestReminder.settings.RemindTicks;
            var questManager = Find.QuestManager;
            foreach (var quest in questManager.questsInDisplayOrder)
            {
                if (IsQuestExpiring(quest, remindTicks))
                {
                    tquests.Add(new TrackedQuest { quest = quest });
                }
            }
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
