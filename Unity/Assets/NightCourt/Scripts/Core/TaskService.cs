using System;
using System.Collections.Generic;
using System.Linq;

namespace NightCourt.Core
{
    public sealed class TaskService
    {
        private readonly List<QuestState> quests = new List<QuestState>();
        public IReadOnlyList<QuestState> Quests => quests;

        public QuestState Accept(string nodeId, string title, QuestType type, int minutes)
        {
            QuestState? existing = quests.FirstOrDefault(q => q.NodeId == nodeId && !q.Completed);
            if (existing != null) return existing;
            var quest = QuestState.Open(Guid.NewGuid().ToString("N"), title, type, minutes);
            quest.NodeId = nodeId;
            quest.AcceptedUtc = DateTimeOffset.UtcNow;
            quests.Add(quest);
            return quest;
        }

        public Reward Complete(string questId, Reward reward)
        {
            QuestState? quest = quests.FirstOrDefault(q => q.Id == questId);
            if (quest == null || quest.Completed) return Reward.None;
            quest.Completed = true;
            quest.CompletedUtc = DateTimeOffset.UtcNow;
            return reward;
        }

        public bool Shorten(string questId, int minutes)
        {
            QuestState? quest = quests.FirstOrDefault(q => q.Id == questId && !q.Completed);
            if (quest == null || minutes < 1 || minutes >= quest.DurationMinutes) return false;
            quest.DurationMinutes = minutes;
            return true;
        }
    }
}
