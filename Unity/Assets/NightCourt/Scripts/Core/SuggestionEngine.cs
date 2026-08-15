using System;
using System.Collections.Generic;
using System.Linq;

namespace NightCourt.Core
{
    public sealed class SuggestionContext
    {
        public int AvailableMinutes { get; }
        public IReadOnlyCollection<QuestType> RecentTypes { get; }
        public SuggestionContext(int availableMinutes, IReadOnlyCollection<QuestType>? recentTypes = null)
        {
            AvailableMinutes = Math.Max(1, availableMinutes);
            RecentTypes = recentTypes ?? Array.Empty<QuestType>();
        }
    }

    public sealed class SuggestionEngine
    {
        public QuestState? Suggest(IEnumerable<QuestState> quests, SuggestionContext context) => quests
            .Where(q => q.Accepted && !q.Completed)
            .Select(q => new { Quest = q, Score = Score(q, context) })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Quest.DurationMinutes)
            .Select(x => x.Quest)
            .FirstOrDefault();

        private static double Score(QuestState quest, SuggestionContext context)
        {
            double score = 100d - quest.DurationMinutes * 1.5d;
            score += quest.DurationMinutes <= context.AvailableMinutes ? 35d : -60d;
            if (quest.Type == QuestType.Micro) score += 20d;
            if (quest.Type == QuestType.Ritual) score += 8d;
            if (context.RecentTypes.Contains(quest.Type)) score -= 12d;
            return score;
        }
    }
}
