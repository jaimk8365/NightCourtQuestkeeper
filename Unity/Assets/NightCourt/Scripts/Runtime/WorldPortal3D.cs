using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class WorldPortal3D:MonoBehaviour
    {
        [SerializeField]private string worldId;[SerializeField]private string displayName;[SerializeField]private int requiredLevel;[SerializeField]private Vector3 destination;
        public void Configure(string id,string label,int level,Vector3 target){worldId=id;displayName=label;requiredLevel=level;destination=target;}
        private bool IsUnlocked()=>requiredLevel<=1||(GameBootstrap.Instance!=null&&GameBootstrap.Instance.Save.UnlockedWorlds.Contains(worldId));
        private void Update(){float pulse=1f+Mathf.Sin(Time.time*2f)*.035f;transform.localScale=new Vector3(pulse,1f,pulse);}
        private void OnMouseUpAsButton()
        {
            if(!IsUnlocked()){CozyHud.Instance?.Notify(displayName+" unlocks at level "+requiredLevel);return;}
            CozyPlayerController player=FindFirstObjectByType<CozyPlayerController>();
            if(player!=null){CharacterController controller=player.GetComponent<CharacterController>();if(controller!=null)controller.enabled=false;player.transform.position=destination;if(controller!=null)controller.enabled=true;}
            CozyHud.Instance?.Notify("Arrived in "+displayName+" ✨");
        }
    }
}
