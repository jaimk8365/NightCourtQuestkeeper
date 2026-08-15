using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NightCourt.Core
{
    public sealed class SaveLoadManager
    {
        public const int CurrentSchema = 1;
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public string Export(PlayerSave save)
        {
            if (save == null) throw new ArgumentNullException(nameof(save));
            Validate(save);
            save.SchemaVersion = CurrentSchema;
            save.SavedUtc = DateTimeOffset.UtcNow;
            return JsonConvert.SerializeObject(save, Settings);
        }

        public PlayerSave Import(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new InvalidDataException("Travelling Scroll is empty.");
            PlayerSave? save = JsonConvert.DeserializeObject<PlayerSave>(json, Settings);
            if (save == null) throw new InvalidDataException("Travelling Scroll could not be read.");
            Validate(save);
            return save;
        }

        public void SaveAtomic(string path, PlayerSave save)
        {
            string temp = path + ".tmp";
            File.WriteAllText(temp, Export(save));
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }

        public PlayerSave LoadOrCreate(string path) => File.Exists(path) ? Import(File.ReadAllText(path)) : PlayerSave.CreateNew();

        private static void Validate(PlayerSave save)
        {
            if (save.SchemaVersion > CurrentSchema) throw new InvalidDataException("Save belongs to a newer game version.");
            save.Progress ??= new PlayerProgress();
            save.Progress.Level = Math.Max(1, save.Progress.Level);
            save.Progress.CurrentXp = Math.Max(0, save.Progress.CurrentXp);
            save.Progress.Coins = Math.Max(0, save.Progress.Coins);
            save.UnlockedWorlds ??= new List<string>();
            save.UnlockedAreas ??= new List<string>();
            save.Quests ??= new List<QuestState>();
            save.Companions ??= new List<CompanionState>();
            save.Inventory ??= new List<InventoryEntry>();
        }
    }
}
