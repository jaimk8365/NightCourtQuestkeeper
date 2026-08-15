using System;
using System.Collections.Generic;

namespace NightCourt.Core
{
    public sealed class LevelUpResult
    {
        public IReadOnlyList<int> LevelsGained { get; }
        public LevelUpResult(IReadOnlyList<int> levelsGained) => LevelsGained = levelsGained;
    }

    public sealed class XpManager
    {
        public PlayerProgress Progress { get; }
        public XpManager(PlayerProgress progress) => Progress = progress ?? throw new ArgumentNullException(nameof(progress));

        public static int RequiredXp(int level)
        {
            level = Math.Max(1, level);
            return (int)Math.Round(60d + 35d * Math.Pow(level, 1.35d), MidpointRounding.AwayFromZero);
        }

        public LevelUpResult AddXp(int amount)
        {
            var gained = new List<int>();
            if (amount <= 0) return new LevelUpResult(gained);
            Progress.Level = Math.Max(1, Progress.Level);
            Progress.CurrentXp = Math.Max(0, Progress.CurrentXp) + amount;
            while (Progress.CurrentXp >= RequiredXp(Progress.Level))
            {
                Progress.CurrentXp -= RequiredXp(Progress.Level);
                Progress.Level++;
                gained.Add(Progress.Level);
            }
            return new LevelUpResult(gained);
        }
    }
}
