using NightCourt.Core;
using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class CozyHud : MonoBehaviour
    {
        public static CozyHud Instance{get;private set;}
        private TaskStation3D station; private QuestState quest; private string toast="Tap the floor to walk · tap a glowing station"; private float toastUntil=6f;
        private void Awake()=>Instance=this;
        public void ShowQuest(TaskStation3D source,QuestState value){station=source;quest=value;}
        public void Notify(string message){toast=message;toastUntil=Time.time+4f;}
        private void OnGUI()
        {
            float scale=Mathf.Clamp(Screen.width/900f,.75f,1.35f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);float width=Screen.width/scale,height=Screen.height/scale;
            GUIStyle title=new GUIStyle(GUI.skin.label){fontSize=24,fontStyle=FontStyle.Bold,normal={textColor=new Color(.22f,.12f,.3f)}};
            GUIStyle body=new GUIStyle(GUI.skin.label){fontSize=16,wordWrap=true,normal={textColor=new Color(.25f,.2f,.3f)}};
            GUIStyle button=new GUIStyle(GUI.skin.button){fontSize=17,fontStyle=FontStyle.Bold,fixedHeight=48};
            GUI.Box(new Rect(18,18,265,78),"");PlayerProgress p=GameBootstrap.Instance?.Save?.Progress;
            GUI.Label(new Rect(34,26,230,30),"Night Court",title);GUI.Label(new Rect(34,58,235,24),p==null?"Awakening…":$"Level {p.Level}   ✦ {p.CurrentXp} XP   ◉ {p.Coins}",body);
            if(Time.time<toastUntil){GUI.Box(new Rect(width/2-220,20,440,54),"");GUI.Label(new Rect(width/2-200,35,400,28),toast,body);} if(quest==null)return;
            Rect panel=new Rect(width/2-220,height-230,440,200);GUI.Box(panel,"");GUI.Label(new Rect(panel.x+24,panel.y+18,390,34),quest.Title,title);GUI.Label(new Rect(panel.x+24,panel.y+58,390,50),$"{quest.DurationMinutes} min · {quest.Type}\nOne small step is enough.",body);
            if(quest.Type==QuestType.Focus)
            {
                int[] presets={5,10,20,45,90};float bw=70;
                for(int i=0;i<presets.Length;i++)if(GUI.Button(new Rect(panel.x+20+i*82,panel.y+118,bw,44),presets[i]+"m",button)){FocusSessionController.Instance?.StartFocus(quest,presets[i]);station=null;quest=null;}
                if(GUI.Button(new Rect(panel.x+145,panel.y+166,150,28),"Not now",button)){station=null;quest=null;}
            }
            else
            {
                if(GUI.Button(new Rect(panel.x+24,panel.y+125,250,48),"Complete quest  ✦",button)){station.Complete(quest);Notify("Tiny win! The cottage grew brighter ✦");station=null;quest=null;}
                if(GUI.Button(new Rect(panel.x+290,panel.y+125,126,48),"Not now",button)){station=null;quest=null;}
            }
        }
    }
}
