using System;
namespace NightCourt.Core
{
    public sealed class FocusRewardService
    {
        private readonly PlayerProgress progress;
        public FocusRewardService(PlayerProgress progress)=>this.progress=progress??throw new ArgumentNullException(nameof(progress));
        public Reward Resolve(FocusResult result)
        {
            double ratio=Math.Max(0d,Math.Min(1d,result.CompletionRatio));
            int xp=(int)Math.Round(50d*ratio,MidpointRounding.AwayFromZero);
            if(result.EarnedStar)progress.Stars++;
            return new Reward(xp,(int)Math.Floor(6d*ratio),2+(int)Math.Floor(3d*ratio));
        }
    }
}
