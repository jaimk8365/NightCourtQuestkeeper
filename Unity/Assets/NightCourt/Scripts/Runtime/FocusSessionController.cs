using NightCourt.Core;
using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class FocusSessionController:MonoBehaviour
    {
        public static FocusSessionController Instance{get;private set;}
        private readonly FocusTimer timer=new FocusTimer(new SystemClock());
        private QuestState quest; private bool rewarded;
        public bool IsRunning=>timer.IsRunning;
        public int RemainingSeconds=>timer.RemainingSeconds;
        private void Awake()=>Instance=this;
        public void StartFocus(QuestState value,int minutes){quest=value;rewarded=false;timer.Start(minutes);CozyHud.Instance?.Notify("Quiet Hour started · the dragon is keeping watch");}
        private void Update(){if(!timer.IsRunning||timer.RemainingSeconds>0)return;FocusResult result=timer.TickToCompletion();Award(result,true);}
        public void FinishEarly(){if(!timer.IsRunning)return;Award(timer.FinishEarly(),false);}
        private void Award(FocusResult result,bool completeQuest)
        {
            if(rewarded)return;rewarded=true;Reward reward=new FocusRewardService(GameBootstrap.Instance.Save.Progress).Resolve(result);
            if(completeQuest)GameBootstrap.Instance.CompleteQuest(quest,reward);else GameBootstrap.Instance.ApplyReward(reward);
            CozyHud.Instance?.Notify(result.EarnedStar?"Focus star earned! ✦":"Partial credit earned · no effort was wasted");
        }
        private void OnGUI()
        {
            if(!timer.IsRunning)return;int seconds=timer.RemainingSeconds;GUI.Box(new Rect(Screen.width-270,18,250,105),"");
            GUIStyle style=new GUIStyle(GUI.skin.label){fontSize=22,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
            GUI.Label(new Rect(Screen.width-255,28,220,40),$"Quiet Hour  {seconds/60:00}:{seconds%60:00}",style);
            if(GUI.Button(new Rect(Screen.width-245,73,200,38),"Stop · keep partial credit"))FinishEarly();
        }
    }
}
