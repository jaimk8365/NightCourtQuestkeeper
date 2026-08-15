using NightCourt.Core;
using UnityEngine;

namespace NightCourt.Runtime
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class TaskNodeBehaviour : MonoBehaviour
    {
        [SerializeField] private string nodeId = "node";
        [SerializeField] private string questTitle = "A tiny win";
        [SerializeField] private QuestType questType = QuestType.Micro;
        [SerializeField] private int minutes = 5;
        [SerializeField] private int xp = 12;
        [SerializeField] private int coins = 3;
        [SerializeField] private int affection = 2;
        private QuestState active;

        public void Configure(string id, string title, QuestType type, int duration, Reward reward)
        {
            nodeId = id; questTitle = title; questType = type; minutes = duration;
            xp = reward.Xp; coins = reward.Coins; affection = reward.Affection;
        }

        public void Interact()
        {
            GameBootstrap game = GameBootstrap.Instance;
            if (game == null) return;
            if (active == null || active.Completed)
            {
                active = game.Tasks.Accept(nodeId, questTitle, questType, minutes);
                Debug.Log($"Quest accepted: {active.Title} · {active.DurationMinutes} min");
                return;
            }
            Reward earned = game.Tasks.Complete(active.Id, new Reward(xp, coins, affection));
            if (earned.Xp > 0) game.ApplyReward(earned);
        }

        private void OnMouseUpAsButton() => Interact();
    }
}
