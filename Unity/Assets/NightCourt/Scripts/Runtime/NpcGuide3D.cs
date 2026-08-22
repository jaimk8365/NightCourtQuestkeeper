using NightCourt.Core;
using UnityEngine;

namespace NightCourt.Runtime
{
    public sealed class NpcGuide3D : MonoBehaviour
    {
        [SerializeField] private string npcName = "Guide";
        [SerializeField] private int availableMinutes = 10;

        public void Configure(string name, int minutes = 10)
        {
            npcName = name;
            availableMinutes = minutes;
        }

        private void OnMouseUpAsButton()
        {
            if (GameBootstrap.Instance == null) return;
            string line = new NpcGuideService().BestNextLine(npcName, GameBootstrap.Instance.Save.Quests, availableMinutes);
            CozyHud.Instance?.Notify(line);
        }
    }
}
