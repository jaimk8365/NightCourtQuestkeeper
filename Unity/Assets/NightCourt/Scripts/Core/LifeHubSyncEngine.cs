using System;
using System.Collections.Generic;
using System.Linq;

namespace NightCourt.Core
{
    public sealed class LifeHubSyncEngine
    {
        public int MergeInto(PlayerSave save, IEnumerable<LifeHubQuestRecord> incoming)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            if (incoming == null) return 0;
            int changed = 0;

            foreach (LifeHubQuestRecord record in incoming.Where(IsUsable))
            {
                QuestState quest = save.Quests.FirstOrDefault(q => q.ExternalRef == record.Id);
                if (quest == null)
                {
                    quest = QuestState.Open(
                        "lifehub:" + record.Id,
                        record.Title,
                        ParseType(record.Type),
                        record.Minutes
                    );
                    quest.ExternalRef = record.Id;
                    save.Quests.Add(quest);
                    changed++;
                }

                if (!quest.Completed && !string.IsNullOrWhiteSpace(record.CompletedOn))
                {
                    quest.Completed = true;
                    quest.CompletedUtc = ParseCompletion(record.CompletedOn);
                    changed++;
                }
            }

            return changed;
        }

        public LifeHubQuestRecord ToLifeHubRecord(QuestState quest)
        {
            if (quest == null) throw new ArgumentNullException(nameof(quest));
            return new LifeHubQuestRecord
            {
                Id = string.IsNullOrWhiteSpace(quest.ExternalRef) ? quest.Id : quest.ExternalRef,
                Title = quest.Title,
                Type = quest.Type.ToString().ToLowerInvariant(),
                Minutes = quest.DurationMinutes,
                CompletedOn = quest.CompletedUtc?.UtcDateTime.ToString("yyyy-MM-dd")
            };
        }

        private static bool IsUsable(LifeHubQuestRecord record) =>
            record != null && !string.IsNullOrWhiteSpace(record.Id) && !string.IsNullOrWhiteSpace(record.Title);

        private static QuestType ParseType(string value) =>
            Enum.TryParse(value, true, out QuestType parsed) ? parsed : QuestType.Micro;

        private static DateTimeOffset ParseCompletion(string value)
        {
            if (DateTimeOffset.TryParse(value, out DateTimeOffset parsed)) return parsed;
            return DateTimeOffset.UtcNow;
        }
    }
}
