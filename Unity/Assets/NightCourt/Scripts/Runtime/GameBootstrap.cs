using System;
using System.IO;
using NightCourt.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace NightCourt.Runtime
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        public PlayerSave Save { get; private set; }
        public XpManager Xp { get; private set; }
        public TaskService Tasks { get; private set; }
        public CompanionSystem Companions { get; private set; }
        public event Action StateChanged;

        private readonly SaveLoadManager saveLoad = new SaveLoadManager();
        private string SavePath => Path.Combine(Application.persistentDataPath, "nightcourt-unity.json");

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Save = saveLoad.LoadOrCreate(SavePath);
            Xp = new XpManager(Save.Progress);
            Tasks = new TaskService();
            Companions = new CompanionSystem();
        }

        public int MergeLifeHubTasks(string json)
        {
            try
            {
                var envelope = JsonConvert.DeserializeObject<LifeHubEnvelope>(json);
                int changed = new LifeHubSyncEngine().MergeInto(Save, envelope?.Quests);
                if (changed > 0) { Persist(); StateChanged?.Invoke(); }
                return changed;
            }
            catch (Exception ex) { Debug.LogWarning("Life Hub task update was ignored: " + ex.Message); return 0; }
        }

        public Reward CompleteQuest(QuestState quest, Reward reward)
        {
            if (quest == null || quest.Completed) return Reward.None;
            quest.Completed = true;
            quest.CompletedUtc = DateTimeOffset.UtcNow;
            ApplyReward(reward);
            LifeHubWebBridge.Instance?.SendCompletion(new LifeHubSyncEngine().ToLifeHubRecord(quest));
            StateChanged?.Invoke();
            return reward;
        }

        public void ApplyReward(Reward reward)
        {
            LevelUpResult levelUp = Xp.AddXp(reward.Xp);
            Save.Progress.Coins += reward.Coins;
            if (Save.Companions.Count > 0) Companions.AddAffection(Save.Companions[0], reward.Affection);
            Persist();
            RewardPresenter.Instance?.Celebrate(reward, levelUp.LevelsGained.Count > 0);
            StateChanged?.Invoke();
        }

        public string ExportTravellingScroll() => saveLoad.Export(Save);

        public bool ImportTravellingScroll(string json, out string error)
        {
            try { Save = saveLoad.Import(json); Xp = new XpManager(Save.Progress); Persist(); error = ""; return true; }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        public void Persist() => saveLoad.SaveAtomic(SavePath, Save);
        private void OnApplicationPause(bool paused) { if (paused) Persist(); }
        private void OnApplicationQuit() => Persist();

        [Serializable]
        private sealed class LifeHubEnvelope { public LifeHubQuestRecord[] Quests; }
    }
}
