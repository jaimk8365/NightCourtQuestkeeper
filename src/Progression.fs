/// The reward system: XP, levels, streaks, and the energy meter.
/// Pure functions only — shared verbatim by the .NET test suite.
module NightCourt.Progression

open NightCourt.Domain

// ---------------------------------------------------------------- XP

let xpForDifficulty =
    function
    | Easy -> 15
    | Medium -> 30
    | Hard -> 60

/// Longer quests earn a small time bonus, capped so marathon tasks don't
/// dwarf the quick wins ADHD brains need to celebrate.
let timeBonus (minutes: int) : int =
    min 20 (minutes / 15 * 5)

let questXp (q: Quest) : int =
    xpForDifficulty q.Difficulty + timeBonus q.Minutes

let ritualStepXp = 5
/// Extra XP for finishing every step of a ritual.
let fullRitualBonus = 20

// ---------------------------------------------------------------- levels

/// XP needed to go from `level` to `level + 1`. Gentle early curve:
/// fast first levels for early dopamine, slowing steadily after.
let xpToNextLevel (level: int) : int = 80 + 40 * (level - 1)

let levelForXp (xp: int) : int =
    let rec go level remaining =
        let needed = xpToNextLevel level
        if remaining < needed then level else go (level + 1) (remaining - needed)
    go 1 (max 0 xp)

/// (xp gathered inside the current level, xp needed for the next).
let levelProgress (xp: int) : int * int =
    let rec go level remaining =
        let needed = xpToNextLevel level
        if remaining < needed then remaining, needed
        else go (level + 1) (remaining - needed)
    go 1 (max 0 xp)

// ---------------------------------------------------------------- streaks

/// Feed the streak for `day`. Idempotent within a day; consecutive days
/// grow it; a gap resets to 1 (gently — no punishment copy anywhere).
let feedStreak (day: string) (r: RewardState) : RewardState =
    match r.LastStreakDay with
    | Some last when last = day -> r
    | Some last when Dates.addDays 1 last = day ->
        let streak = r.CurrentStreak + 1
        { r with CurrentStreak = streak; BestStreak = max r.BestStreak streak; LastStreakDay = Some day }
    | _ ->
        { r with CurrentStreak = 1; BestStreak = max r.BestStreak 1; LastStreakDay = Some day }

/// True when the streak is still alive as seen from `today`
/// (fed today or yesterday).
let streakAlive (today: string) (r: RewardState) : bool =
    match r.LastStreakDay with
    | Some d -> d = today || Dates.addDays 1 d = today
    | None -> false

// ---------------------------------------------------------------- energy

/// Rough spoons-cost of a quest: difficulty weight plus time weight.
let energyCost (q: Quest) : float =
    let w =
        match q.Difficulty with
        | Easy -> 1.0
        | Medium -> 2.0
        | Hard -> 3.5
    w + float q.Minutes / 30.0

/// A sustainable day. Loads are shown against this budget to prevent
/// overwhelm before it happens.
let dailyEnergyBudget = 10.0

let energyLoad (quests: Quest list) : float =
    (quests |> List.sumBy energyCost) / dailyEnergyBudget

type EnergyBand =
    | Gentle
    | Balanced
    | Heavy

let energyBand (load: float) : EnergyBand =
    if load < 0.5 then Gentle
    elif load <= 0.9 then Balanced
    else Heavy

let energyLabel =
    function
    | Gentle -> "Gentle — room to breathe"
    | Balanced -> "Balanced — a good day's magic"
    | Heavy -> "Heavy — the stars counsel moving something"
