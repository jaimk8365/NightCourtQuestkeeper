using System;
using System.Collections.Generic;

namespace NightCourt.Core
{
    public enum QuestType { Micro, Focus, Ritual, Craft, Social, Errand, Boss, Exploration, PetCare, Learning }

    public sealed class PlayerProgress
    {
        public int Level { get; set; } = 1;
        public int CurrentXp { get; set; }
        public int Coins { get; set; }
    }

    public readonly struct Reward
    {
        public static readonly Reward None = new Reward(0, 0, 0);
        public int Xp { get; }
        public int Coins { get; }
        public int Affection { get; }
        public Reward(int xp, int coins, int affection)
        {
            Xp = Math.Max(0, xp); Coins = Math.Max(0, coins); Affection = Math.Max(0, affection);
        }
    }

    public sealed class QuestState
    {
        public string Id { get; set; } = "";
        public string NodeId { get; set; } = "";
        public string Title { get; set; } = "";
        public QuestType Type { get; set; }
        public int DurationMinutes { get; set; }
        public bool Accepted { get; set; }
        public bool Completed { get; set; }
        public DateTimeOffset? AcceptedUtc { get; set; }
        public DateTimeOffset? CompletedUtc { get; set; }

        public static QuestState Open(string id, string title, QuestType type, int minutes) => new QuestState
        {
            Id = id, NodeId = id, Title = title.Trim(), Type = type,
            DurationMinutes = Math.Max(1, minutes), Accepted = true
        };
    }

    public sealed class CompanionState
    {
        public string Id { get; set; } = "";
        public int Affection { get; set; }
        public int EvolutionStage { get; set; } = 1;
        public List<string> UnlockedPerks { get; set; } = new List<string>();
    }

    public sealed class InventoryEntry
    {
        public string ItemId { get; set; } = "";
        public int Quantity { get; set; }
    }

    public sealed class PlayerSave
    {
        public int SchemaVersion { get; set; } = 1;
        public PlayerProgress Progress { get; set; } = new PlayerProgress();
        public List<string> UnlockedWorlds { get; set; } = new List<string>();
        public List<string> UnlockedAreas { get; set; } = new List<string>();
        public List<QuestState> Quests { get; set; } = new List<QuestState>();
        public List<CompanionState> Companions { get; set; } = new List<CompanionState>();
        public List<InventoryEntry> Inventory { get; set; } = new List<InventoryEntry>();
        public DateTimeOffset SavedUtc { get; set; }

        public static PlayerSave CreateNew()
        {
            var save = new PlayerSave();
            save.UnlockedWorlds.Add("fae_cottage");
            save.UnlockedAreas.AddRange(new[] { "hearth", "closet", "workbench" });
            save.Companions.Add(new CompanionState { Id = "hearth_dragon" });
            return save;
        }
    }
}
