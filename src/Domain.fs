/// Core domain types for Night Court Questkeeper.
/// Dates are stored as "yyyy-MM-dd" day keys throughout — timezone-proof,
/// trivially serializable, and identical semantics in Fable and .NET.
module NightCourt.Domain

open System

// ---------------------------------------------------------------- dates

/// Day-key helpers. A DayKey is a "yyyy-MM-dd" string.
module Dates =
    let dayKey (d: DateTime) : string = d.ToString("yyyy-MM-dd")

    let parse (key: string) : DateTime =
        match key.Split('-') with
        | [| y; m; d |] -> DateTime(int y, int m, int d)
        | _ -> DateTime.MinValue

    let addDays (n: int) (key: string) : string = dayKey ((parse key).AddDays(float n))

    /// Monday of the week containing this day — weeks run Mon..Sun,
    /// so the Sunday Ritual plans the week that starts tomorrow.
    let mondayOf (key: string) : string =
        let d = parse key
        let offset = (int d.DayOfWeek + 6) % 7 // Monday=0 .. Sunday=6
        dayKey (d.AddDays(float -offset))

    let isSunday (key: string) : bool = (parse key).DayOfWeek = DayOfWeek.Sunday

    let weekdayName (key: string) : string =
        match (parse key).DayOfWeek with
        | DayOfWeek.Monday -> "Monday"
        | DayOfWeek.Tuesday -> "Tuesday"
        | DayOfWeek.Wednesday -> "Wednesday"
        | DayOfWeek.Thursday -> "Thursday"
        | DayOfWeek.Friday -> "Friday"
        | DayOfWeek.Saturday -> "Saturday"
        | _ -> "Sunday"

    let shortLabel (key: string) : string =
        let d = parse key
        sprintf "%s %d" (weekdayName key |> fun n -> n.Substring(0, 3)) d.Day

    /// Days since a fixed epoch Monday — used for deterministic cycles (moon phases).
    let daysSinceEpoch (key: string) : int =
        int ((parse key) - DateTime(2026, 1, 5)).TotalDays // 2026-01-05 is a Monday

// ---------------------------------------------------------------- quests

type Difficulty =
    | Easy
    | Medium
    | Hard

type Category =
    | Home
    | Work
    | Personal
    | Errands
    | Wellbeing

/// Auto-assigned magical flavour for each quest.
type MagicalTheme =
    | Starweaving
    | Moonbinding
    | Shadowstep
    | Dreamtending
    | Emberwarding
    | Mistcalling
    | Nightblooming

type Quest =
    { Id: Guid
      Title: string
      Description: string option
      Difficulty: Difficulty
      /// Estimated minutes to complete.
      Minutes: int
      Category: Category
      Theme: MagicalTheme
      /// Optional hard deadline (day key).
      Deadline: string option
      /// Day this quest is planned for (day key); None = unscheduled.
      PlannedFor: string option
      CreatedOn: string
      /// Some dayKey when completed; None while active.
      CompletedOn: string option }

// ---------------------------------------------------------------- rituals

type RitualTime =
    | Morning
    | Evening

type RoutineStep =
    { Id: Guid
      Label: string
      Minutes: int }

/// A routine is set once and appears every day as a "Daily Ritual".
type Routine =
    { Time: RitualTime
      Steps: RoutineStep list }

// ---------------------------------------------------------------- planning

/// Persisted per-week metadata; the quests themselves carry their PlannedFor day.
type WeekPlan =
    { /// Monday day-key identifying the week.
      WeekOf: string
      /// Optional intention set during the Sunday Ritual.
      Intention: string option }

/// Derived (not persisted) view of one day used by the planner.
type DayPlan =
    { Date: string
      Quests: Quest list
      /// 0.0 .. 1.0+ fraction of the daily energy budget.
      EnergyLoad: float }

// ---------------------------------------------------------------- profile & rewards

type UserProfile =
    { Name: string
      JoinedOn: string }

type RewardState =
    { Xp: int
      CurrentStreak: int
      BestStreak: int
      /// Last day the streak was fed (day key).
      LastStreakDay: string option }

    static member Initial =
        { Xp = 0; CurrentStreak = 0; BestStreak = 0; LastStreakDay = None }

// ---------------------------------------------------------------- app data

/// Everything that persists. One record → one JSON blob → localStorage / cloud.
type AppData =
    { Version: int
      Profile: UserProfile option
      Quests: Quest list
      Routines: Routine list
      /// dayKey → ritual step ids completed that day.
      RitualLog: Map<string, Guid list>
      WeekPlans: WeekPlan list
      Rewards: RewardState
      /// Optional cloud-sync endpoint (any URL accepting GET/PUT of the JSON blob).
      SyncEndpoint: string option }
