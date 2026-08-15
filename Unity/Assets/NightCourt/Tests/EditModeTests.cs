using System;
using System.Linq;
using NightCourt.Core;
using NUnit.Framework;

namespace NightCourt.Tests
{
    public sealed class EditModeTests
    {
        private sealed class FakeClock : IClock { public DateTimeOffset UtcNow { get; set; } }

        [Test]
        public void XpOverflowLevelsAndRetainsRemainder()
        {
            var xp = new XpManager(new PlayerProgress { Level = 1, CurrentXp = 90 });
            LevelUpResult result = xp.AddXp(20);
            Assert.That(xp.Progress.Level, Is.EqualTo(2));
            Assert.That(xp.Progress.CurrentXp, Is.EqualTo(15));
            Assert.That(result.LevelsGained, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void TaskCompletionRewardsOnlyOnce()
        {
            var tasks = new TaskService();
            QuestState quest = tasks.Accept("hearth", "Tiny reset", QuestType.Micro, 5);
            Assert.That(tasks.Complete(quest.Id, new Reward(12, 3, 2)).Xp, Is.EqualTo(12));
            Assert.That(tasks.Complete(quest.Id, new Reward(12, 3, 2)).Xp, Is.Zero);
        }

        [Test]
        public void FocusTimerUsesUtcBackgroundTime()
        {
            var clock = new FakeClock { UtcNow = DateTimeOffset.Parse("2026-08-15T00:00:00Z") };
            var timer = new FocusTimer(clock); timer.Start(10); clock.UtcNow = clock.UtcNow.AddMinutes(4);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(360));
            Assert.That(timer.FinishEarly().CompletionRatio, Is.EqualTo(.4d).Within(.001));
        }

        [Test]
        public void TravellingScrollRoundTrips()
        {
            var codec = new SaveLoadManager(); PlayerSave save = PlayerSave.CreateNew(); save.Progress.Level = 3;
            PlayerSave restored = codec.Import(codec.Export(save));
            Assert.That(restored.Progress.Level, Is.EqualTo(3));
            Assert.That(restored.UnlockedWorlds, Does.Contain("fae_cottage"));
        }

        [Test]
        public void SuggestionIsSmallestQuestThatFits()
        {
            var quests = new[] { QuestState.Open("long","Deep clean",QuestType.Focus,45), QuestState.Open("tiny","Five things",QuestType.Micro,5) };
            Assert.That(new SuggestionEngine().Suggest(quests,new SuggestionContext(10))?.Id, Is.EqualTo("tiny"));
        }
    }
}
