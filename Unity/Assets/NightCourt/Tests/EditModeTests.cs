using System;
using System.Linq;
using NightCourt.Core;
using NightCourt.Runtime;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

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

        [Test]
        public void LifeHubBridgeImportsWithoutDuplicatingAndAppliesRemoteCompletion()
        {
            PlayerSave save = PlayerSave.CreateNew();
            var sync = new LifeHubSyncEngine();
            var incoming = new[]
            {
                new LifeHubQuestRecord { Id = "tasks:school-form", Title = "Return school form", Minutes = 5, Type = "micro" }
            };

            Assert.That(sync.MergeInto(save, incoming), Is.EqualTo(1));
            Assert.That(sync.MergeInto(save, incoming), Is.Zero);
            incoming[0].CompletedOn = "2026-08-22";
            Assert.That(sync.MergeInto(save, incoming), Is.EqualTo(1));
            Assert.That(save.Quests.Single(q => q.ExternalRef == "tasks:school-form").Completed, Is.True);
        }

        [Test]
        public void LifeHubBridgeExportsCompletionWithoutCredentials()
        {
            QuestState quest = QuestState.Open("lifehub:tasks:bins", "Put bins out", QuestType.Micro, 5);
            quest.ExternalRef = "tasks:bins";
            quest.Completed = true;
            quest.CompletedUtc = DateTimeOffset.Parse("2026-08-22T09:00:00Z");

            LifeHubQuestRecord outgoing = new LifeHubSyncEngine().ToLifeHubRecord(quest);
            Assert.That(outgoing.Id, Is.EqualTo("tasks:bins"));
            Assert.That(outgoing.CompletedOn, Is.EqualTo("2026-08-22"));
            Assert.That(outgoing.Title, Is.EqualTo("Put bins out"));
        }

        [Test]
        public void FaeCottageSceneHasPlayableThreeStationMilestone()
        {
            EditorSceneManager.OpenScene("Assets/NightCourt/Scenes/FaeCottage.unity");
            Assert.That(UnityEngine.Object.FindObjectsByType<CozyPlayerController>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<CozyCamera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<DragonCompanion>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<TaskStation3D>(FindObjectsSortMode.None), Has.Length.EqualTo(3));
            Assert.That(UnityEngine.Object.FindObjectsByType<LifeHubWebBridge>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }
    }
}
