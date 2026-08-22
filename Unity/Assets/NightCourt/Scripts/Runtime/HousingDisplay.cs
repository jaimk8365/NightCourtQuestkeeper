using System.Linq;
using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class HousingDisplay:MonoBehaviour
    {
        public static HousingDisplay Instance{get;private set;}
        [SerializeField]private GameObject glowShelf;
        private void Awake(){Instance=this;Refresh();}
        private void Start()=>Refresh();
        public void Configure(GameObject shelf)=>glowShelf=shelf;
        public void Refresh(){if(glowShelf==null||GameBootstrap.Instance==null)return;bool owned=GameBootstrap.Instance.Save.Inventory.Any(x=>x.ItemId=="furniture.glow_shelf"&&x.Quantity>0);glowShelf.SetActive(owned);}
    }
}
