using System.Collections.Generic;

namespace NightCourt.Core
{
    public sealed class NpcGuideService
    {
        public string BestNextLine(string npcName, IEnumerable<QuestState> quests, int availableMinutes)
        {
            QuestState next = new SuggestionEngine().Suggest(quests, new SuggestionContext(availableMinutes));
            if (next == null) return npcName + ": You are caught up. Wander, rest, or craft something lovely.";
            string line = npcName + ": Try “" + next.Title + "” next — about " + next.DurationMinutes + " min.";
            return line.Length <= 100 ? line : line.Substring(0, 97) + "…";
        }
    }
}
