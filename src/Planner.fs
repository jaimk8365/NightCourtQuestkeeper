/// Weekly planning: week construction, energy overview, and the
/// auto-suggest engine that seeds the Sunday Ritual.
/// Pure functions only — shared by the .NET test suite.
module NightCourt.Planner

open NightCourt.Domain
open NightCourt.QuestEngine
open NightCourt.Progression

let weekDays (weekOf: string) : string list =
    [ 0 .. 6 ] |> List.map (fun i -> Dates.addDays i weekOf)

/// The week the Sunday Ritual plans: next week when today is Sunday,
/// otherwise the current week.
let ritualTargetWeek (today: string) : string =
    if Dates.isSunday today then Dates.mondayOf (Dates.addDays 1 today)
    else Dates.mondayOf today

let dayPlanFor (quests: Quest list) (day: string) : DayPlan =
    let planned = plannedFor day quests
    { Date = day
      Quests = planned
      EnergyLoad = energyLoad planned }

let weekOverview (quests: Quest list) (weekOf: string) : DayPlan list =
    weekDays weekOf |> List.map (dayPlanFor quests)

let weekPlanFor (weekOf: string) (plans: WeekPlan list) : WeekPlan =
    plans
    |> List.tryFind (fun p -> p.WeekOf = weekOf)
    |> Option.defaultValue { WeekOf = weekOf; Intention = None }

let setIntention (weekOf: string) (intention: string) (plans: WeekPlan list) : WeekPlan list =
    let cleaned = intention.Trim()
    let updated =
        { (weekPlanFor weekOf plans) with
            Intention = if cleaned = "" then None else Some cleaned }
    updated :: (plans |> List.filter (fun p -> p.WeekOf <> weekOf))

// ---------------------------------------------------------------- assignment

let assignQuestToDay (questId: System.Guid) (day: string option) (quests: Quest list) : Quest list =
    quests
    |> List.map (fun q -> if q.Id = questId then { q with PlannedFor = day } else q)

/// The lightest-energy day of a week — where an accepted suggestion lands,
/// so saying "yes" never requires a scheduling decision.
let lightestDay (quests: Quest list) (weekOf: string) (notBefore: string) : string =
    weekOverview quests weekOf
    |> List.filter (fun d -> d.Date >= notBefore)
    |> List.sortBy (fun d -> d.EnergyLoad, d.Date)
    |> List.tryHead
    |> Option.map (fun d -> d.Date)
    |> Option.defaultValue weekOf

// ---------------------------------------------------------------- auto-suggest

type SuggestionSource =
    | UnfinishedQuest
    | RecurringPattern

type Suggestion =
    { Title: string
      Source: SuggestionSource
      Difficulty: Difficulty
      Minutes: int
      Category: Category
      /// For UnfinishedQuest: the existing quest to (re)schedule.
      ExistingId: System.Guid option }

/// Suggestions for planning `weekOf`:
///  • active quests that aren't scheduled into that week yet (reschedule these)
///  • titles completed in ≥ 2 distinct past weeks (recurring patterns → new quest)
let suggestionsFor (data: AppData) (weekOf: string) : Suggestion list =
    let weekEnd = Dates.addDays 6 weekOf

    let unfinished =
        data.Quests
        |> List.filter (fun q ->
            isActive q
            && (match q.PlannedFor with
                | Some d -> d < weekOf || d > weekEnd // scheduled outside the target week
                | None -> true))
        |> List.map (fun q ->
            { Title = q.Title
              Source = UnfinishedQuest
              Difficulty = q.Difficulty
              Minutes = q.Minutes
              Category = q.Category
              ExistingId = Some q.Id })

    let activeTitles =
        data.Quests |> List.filter isActive |> List.map (fun q -> q.Title.ToLowerInvariant()) |> Set.ofList

    let recurring =
        data.Quests
        |> List.choose (fun q -> q.CompletedOn |> Option.map (fun d -> q, Dates.mondayOf d))
        |> List.filter (fun (_, w) -> w < weekOf) // only history before the target week
        |> List.groupBy (fun (q, _) -> q.Title.ToLowerInvariant())
        |> List.filter (fun (title, hits) ->
            (hits |> List.map snd |> List.distinct |> List.length) >= 2
            && not (activeTitles.Contains title))
        |> List.map (fun (_, hits) ->
            let q = hits |> List.map fst |> List.maxBy (fun q -> q.CreatedOn)
            { Title = q.Title
              Source = RecurringPattern
              Difficulty = q.Difficulty
              Minutes = q.Minutes
              Category = q.Category
              ExistingId = None })

    (unfinished @ recurring)
    |> List.distinctBy (fun s -> s.Title.ToLowerInvariant())
    |> List.truncate 8
