using System;
using System.IO;
using NightCourt.Core;
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

        public void ApplyReward(Reward reward)
        {
            LevelUpResult levelUp = Xp.AddXp(reward.Xp);
            Save.Progress.Coins += reward.Coins;
            if (Save.Companions.Count > 0) Companions.AddAffection(Save.Companions[0], reward.Affection);
            Persist();
            RewardPresenter.Instance?.Celebrate(reward, levelUp.LevelsGained.Count > 0);
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
    }
}
