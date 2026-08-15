using NightCourt.Core;

static class Assert
{
    public static void Equal<T>(T expected, T actual, string name)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"{name}: expected {expected}, got {actual}");
    }

    public static void True(bool value, string name)
    {
        if (!value) throw new Exception($"{name}: expected true");
    }
}

sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }
}

static class Program
{
    private static int passed;

    private static void Run(string name, Action test)
    {
        test();
        passed++;
        Console.WriteLine($"PASS {name}");
    }

    public static void Main()
    {
        Run("XP overflow unlocks level", () =>
        {
            var xp = new XpManager(new PlayerProgress { Level = 1, CurrentXp = 90 });
            LevelUpResult result = xp.AddXp(20);
            Assert.Equal(2, xp.Progress.Level, "level");
            Assert.Equal(15, xp.Progress.CurrentXp, "overflow xp");
            Assert.True(result.LevelsGained.SequenceEqual(new[] { 2 }), "level event");
        });

        Run("Task completion rewards exactly once", () =>
        {
            var tasks = new TaskService();
            QuestState quest = tasks.Accept("node-hearth", "Reset the bench", QuestType.Micro, 5);
            Reward first = tasks.Complete(quest.Id, new Reward(12, 3, 2));
            Reward second = tasks.Complete(quest.Id, new Reward(12, 3, 2));
            Assert.Equal(12, first.Xp, "first reward");
            Assert.Equal(0, second.Xp, "duplicate reward");
            Assert.True(tasks.Quests.Single().Completed, "completed state");
        });

        Run("Focus timer survives background time", () =>
        {
            var clock = new FakeClock { UtcNow = DateTimeOffset.Parse("2026-08-15T00:00:00Z") };
            var timer = new FocusTimer(clock);
            timer.Start(10);
            clock.UtcNow = clock.UtcNow.AddMinutes(4);
            Assert.Equal(360, timer.RemainingSeconds, "remaining seconds");
            FocusResult result = timer.FinishEarly();
            Assert.Equal(0.4, Math.Round(result.CompletionRatio, 2), "partial credit");
        });

        Run("Travelling Scroll round trips state", () =>
        {
            var codec = new SaveLoadManager();
            var save = PlayerSave.CreateNew();
            save.Progress.Level = 3;
            save.UnlockedWorlds.Add("moonbinding_hollow");
            string json = codec.Export(save);
            PlayerSave restored = codec.Import(json);
            Assert.Equal(3, restored.Progress.Level, "restored level");
            Assert.True(restored.UnlockedWorlds.Contains("moonbinding_hollow"), "restored world");
        });

        Run("Suggestion chooses smallest task that fits", () =>
        {
            var engine = new SuggestionEngine();
            QuestState[] quests =
            {
                QuestState.Open("long", "Deep clean", QuestType.Focus, 45),
                QuestState.Open("tiny", "Put away five things", QuestType.Micro, 5),
                QuestState.Open("medium", "Make appointment", QuestType.Errand, 20)
            };
            QuestState? next = engine.Suggest(quests, new SuggestionContext(10));
            Assert.Equal("tiny", next?.Id, "suggestion");
        });

        Console.WriteLine($"ALL {passed} TESTS PASSED");
    }
}
