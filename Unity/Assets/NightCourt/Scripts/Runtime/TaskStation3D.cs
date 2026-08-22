using System.Linq;
using NightCourt.Core;
using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class TaskStation3D : MonoBehaviour
    {
        [SerializeField] private string stationId, fallbackTitle;
        [SerializeField] private QuestType fallbackType;
        [SerializeField] private int fallbackMinutes = 5, xp = 15, coins = 3, affection = 2;
        public void Configure(string id, string title, QuestType type, int minutes, Reward reward)
        { stationId=id; fallbackTitle=title; fallbackType=type; fallbackMinutes=minutes; xp=reward.Xp; coins=reward.Coins; affection=reward.Affection; }
        public QuestState CurrentQuest()
        {
            GameBootstrap game=GameBootstrap.Instance; if(game==null)return null;
            QuestState synced=game.Save.Quests.Where(q=>!q.Completed).OrderBy(q=>q.DurationMinutes).FirstOrDefault(MatchesStation);
            if(synced!=null)return synced;
            string id="station:"+stationId;
            QuestState fallback=game.Save.Quests.FirstOrDefault(q=>q.Id==id&&!q.Completed);
            if(fallback==null){ fallback=QuestState.Open(id,fallbackTitle,fallbackType,fallbackMinutes); fallback.NodeId=stationId; game.Save.Quests.Add(fallback); game.Persist(); }
            return fallback;
        }
        public void Interact(){ QuestState value=CurrentQuest(); if(value!=null)CozyHud.Instance?.ShowQuest(this,value); }
        public void Complete(QuestState value)=>GameBootstrap.Instance?.CompleteQuest(value,new Reward(xp,coins,affection));
        private bool MatchesStation(QuestState value)
        { if(stationId=="focus")return value.Type==QuestType.Focus||value.DurationMinutes>=25; if(stationId=="care")return value.Type==QuestType.Ritual||value.Type==QuestType.PetCare; return value.Type==QuestType.Micro||value.DurationMinutes<25; }
        private void OnMouseUpAsButton()=>Interact();
    }
}
