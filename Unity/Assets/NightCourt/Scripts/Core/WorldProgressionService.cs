using System.Collections.Generic;
namespace NightCourt.Core
{
    public sealed class WorldProgressionService
    {
        private static readonly (int Level,string Id)[] Gates={
            (3,"moonbinding_hollow"),(5,"emberward_grove"),(10,"mistcaller_market"),(13,"starweaver_atelier"),(16,"dragonspire_sanctuary"),(20,"seasonal_portal")};
        public IReadOnlyList<string> Apply(PlayerSave save)
        {
            var added=new List<string>();
            foreach(var gate in Gates)if(save.Progress.Level>=gate.Level&&!save.UnlockedWorlds.Contains(gate.Id)){save.UnlockedWorlds.Add(gate.Id);added.Add(gate.Id);}
            return added;
        }
    }
}
