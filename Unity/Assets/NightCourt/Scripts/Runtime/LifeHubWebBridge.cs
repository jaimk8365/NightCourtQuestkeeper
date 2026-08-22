using System.Runtime.InteropServices;
using Newtonsoft.Json;
using NightCourt.Core;
using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class LifeHubWebBridge:MonoBehaviour
    {
        public static LifeHubWebBridge Instance{get;private set;}
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]private static extern void NC_RequestQuestKeys();
        [DllImport("__Internal")]private static extern void NC_SaveQuestCompletion(string json);
#endif
        private void Awake()=>Instance=this;
        private void Start(){
#if UNITY_WEBGL && !UNITY_EDITOR
            NC_RequestQuestKeys();
#endif
        }
        public void ReceiveLifeHubQuests(string json){int changed=GameBootstrap.Instance?.MergeLifeHubTasks(json)??0;CozyHud.Instance?.Notify(changed>0?$"{changed} Life Hub quest update{(changed==1?"":"s")} arrived":"Life Hub quests are up to date");}
        public void SendCompletion(LifeHubQuestRecord record){if(record==null||string.IsNullOrWhiteSpace(record.Id))return;
#if UNITY_WEBGL && !UNITY_EDITOR
            NC_SaveQuestCompletion(JsonConvert.SerializeObject(record));
#endif
        }
    }
}
