using System;

namespace NightCourt.Core
{
    public interface IClock { DateTimeOffset UtcNow { get; } }
    public sealed class SystemClock : IClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }
    public readonly struct FocusResult
    {
        public double CompletionRatio { get; }
        public bool EarnedStar => CompletionRatio >= 0.8d;
        public FocusResult(double ratio) => CompletionRatio = Math.Max(0d, Math.Min(1d, ratio));
    }

    public sealed class FocusTimer
    {
        private readonly IClock clock;
        private DateTimeOffset endUtc;
        public int PlannedSeconds { get; private set; }
        public bool IsRunning { get; private set; }
        public FocusTimer(IClock clock) => this.clock = clock ?? throw new ArgumentNullException(nameof(clock));

        public int RemainingSeconds => !IsRunning ? 0 : Math.Max(0, (int)Math.Ceiling((endUtc - clock.UtcNow).TotalSeconds));

        public void Start(int minutes)
        {
            PlannedSeconds = Math.Max(1, minutes) * 60;
            endUtc = clock.UtcNow.AddSeconds(PlannedSeconds);
            IsRunning = true;
        }

        public FocusResult FinishEarly()
        {
            if (!IsRunning || PlannedSeconds == 0) return new FocusResult(0);
            double ratio = 1d - RemainingSeconds / (double)PlannedSeconds;
            IsRunning = false;
            return new FocusResult(ratio);
        }

        public FocusResult TickToCompletion()
        {
            if (!IsRunning || RemainingSeconds > 0) return new FocusResult(0);
            IsRunning = false;
            return new FocusResult(1);
        }
    }
}
