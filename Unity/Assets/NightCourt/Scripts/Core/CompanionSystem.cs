using System;
using System.Collections.Generic;

namespace NightCourt.Core
{
    public sealed class CompanionSystem
    {
        public CompanionState AddAffection(CompanionState state, int amount)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            state.Affection = Math.Min(1000, Math.Max(0, state.Affection + Math.Max(0, amount)));
            state.EvolutionStage = state.Affection >= 1000 ? 3 : state.Affection >= 500 ? 2 : 1;
            Unlock(state, "warm_start", 250);
            Unlock(state, "hearth_boost", 750);
            return state;
        }
        private static void Unlock(CompanionState state, string perk, int threshold)
        {
            if (state.Affection >= threshold && !state.UnlockedPerks.Contains(perk)) state.UnlockedPerks.Add(perk);
        }
    }
}
