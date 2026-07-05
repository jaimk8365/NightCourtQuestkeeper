/// Quest lifecycle and the Next Best Action sorter — the anti-decision-paralysis core.
/// Pure functions only — shared by the .NET test suite.
module NightCourt.QuestEngine

open System
open NightCourt.Domain
open NightCourt.Catalog
open NightCourt.Progression

// ---------------------------------------------------------------- lifecycle

let createQuest
    (id: Guid)
    (today: string)
    (title: string)
    (description: string option)
    (difficulty: Difficulty)
    (minutes: int)
    (category: Category)
    (deadline: string option)
    (plannedFor: string option)
    : Quest =
    { Id = id
      Title = title.Trim()
      Description = description |> Option.map (fun d -> d.Trim()) |> Option.filter (fun d -> d <> "")
      Difficulty = difficulty
      Minutes = max 1 minutes
      Category = category
      Theme = assignTheme title
      Deadline = deadline
      PlannedFor = plannedFor
      CreatedOn = today
      CompletedOn = None }

let isActive (q: Quest) = q.CompletedOn.IsNone

let complete (day: string) (q: Quest) : Quest =
    { q with CompletedOn = Some day }

let isOverdue (today: string) (q: Quest) : bool =
    isActive q && (match q.Deadline with Some d -> d < today | None -> false)

let isDueToday (today: string) (q: Quest) : bool =
    isActive q && q.Deadline = Some today

let isPlannedFor (day: string) (q: Quest) : bool =
    isActive q && q.PlannedFor = Some day

// ---------------------------------------------------------------- next best action

/// Priority sort for active quests. Urgency first, then smallest-first inside
/// each band — a tiny quest you'll actually start beats a big one you'll dread.
///   band 0: overdue
///   band 1: deadline today
///   band 2: planned for today
///   band 3: everything else (unscheduled or future)
let private urgencyBand (today: string) (q: Quest) : int =
    if isOverdue today q then 0
    elif isDueToday today q then 1
    elif isPlannedFor today q then 2
    else 3

let private difficultyRank =
    function
    | Easy -> 0
    | Medium -> 1
    | Hard -> 2

/// All active quests in "do this next" order.
let sortByNextBest (today: string) (quests: Quest list) : Quest list =
    quests
    |> List.filter isActive
    |> List.sortBy (fun q ->
        urgencyBand today q, q.Minutes, difficultyRank q.Difficulty, q.CreatedOn, q.Title)

/// The single quest the stars suggest doing right now.
let nextBestAction (today: string) (quests: Quest list) : Quest option =
    sortByNextBest today quests |> List.tryHead

// ---------------------------------------------------------------- queries

let activeQuests (quests: Quest list) = quests |> List.filter isActive

let completedOn (day: string) (quests: Quest list) =
    quests |> List.filter (fun q -> q.CompletedOn = Some day)

let totalCompleted (quests: Quest list) =
    quests |> List.filter (fun q -> q.CompletedOn.IsSome) |> List.length

let plannedFor (day: string) (quests: Quest list) =
    quests |> List.filter (isPlannedFor day)

/// Active quests with no planned day and no future deadline pull — the pool
/// the weekly planner offers for scheduling.
let unscheduled (quests: Quest list) =
    quests |> List.filter (fun q -> isActive q && q.PlannedFor.IsNone)

let dayEnergyLoad (day: string) (quests: Quest list) : float =
    quests |> plannedFor day |> energyLoad
