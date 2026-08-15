using System;
using System.Collections.Generic;
using System.Linq;

namespace NightCourt.Core
{
    public sealed class WorldGate
    {
        public string WorldId { get; set; } = "";
        public int RequiredLevel { get; set; } = 1;
        public string? RequiredStoryFlag { get; set; }
    }

    public sealed class WorldManager
    {
        private readonly List<WorldGate> gates;
        private readonly HashSet<string> unlocked;
        private readonly HashSet<string> storyFlags = new HashSet<string>();
        public WorldManager(IEnumerable<WorldGate> gates, IEnumerable<string>? unlocked = null)
        {
            this.gates = gates.ToList();
            this.unlocked = new HashSet<string>(unlocked ?? Array.Empty<string>());
        }
        public IReadOnlyCollection<string> Evaluate(int level)
        {
            var added = new List<string>();
            foreach (WorldGate gate in gates.Where(g => level >= g.RequiredLevel))
                if ((string.IsNullOrWhiteSpace(gate.RequiredStoryFlag) || storyFlags.Contains(gate.RequiredStoryFlag!)) && unlocked.Add(gate.WorldId)) added.Add(gate.WorldId);
            return added;
        }
        public void AddStoryFlag(string flag) { if (!string.IsNullOrWhiteSpace(flag)) storyFlags.Add(flag); }
    }
}
