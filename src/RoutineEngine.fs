/// Morning & night routine engine — set once, appear daily as rituals.
/// Pure functions only — shared by the .NET test suite.
module NightCourt.RoutineEngine

open System
open NightCourt.Domain
open NightCourt.Progression

let routineFor (time: RitualTime) (routines: Routine list) : Routine =
    routines
    |> List.tryFind (fun r -> r.Time = time)
    |> Option.defaultValue { Time = time; Steps = [] }

let completedSteps (day: string) (log: Map<string, Guid list>) : Set<Guid> =
    log |> Map.tryFind day |> Option.defaultValue [] |> Set.ofList

let isStepDone (day: string) (log: Map<string, Guid list>) (stepId: Guid) : bool =
    completedSteps day log |> Set.contains stepId

let isRitualComplete (day: string) (log: Map<string, Guid list>) (routine: Routine) : bool =
    not routine.Steps.IsEmpty
    && (let doneSet = completedSteps day log
        routine.Steps |> List.forall (fun s -> doneSet.Contains s.Id))

/// Result of toggling one ritual step for a day.
type ToggleResult =
    { Log: Map<string, Guid list>
      /// XP gained by this toggle (0 when un-checking).
      XpGained: int
      /// True when this toggle completed the whole ritual.
      RitualCompleted: bool }

let toggleStep (day: string) (routine: Routine) (stepId: Guid) (log: Map<string, Guid list>) : ToggleResult =
    let before = completedSteps day log
    let after =
        if before.Contains stepId then Set.remove stepId before
        else Set.add stepId before
    let log' = log |> Map.add day (Set.toList after)
    let turnedOn = not (before.Contains stepId)
    let completedNow =
        turnedOn
        && not (isRitualComplete day log routine)
        && isRitualComplete day log' routine
    { Log = log'
      XpGained =
        if turnedOn then ritualStepXp + (if completedNow then fullRitualBonus else 0)
        else 0
      RitualCompleted = completedNow }

// ---------------------------------------------------------------- editing

let addStep (time: RitualTime) (label: string) (minutes: int) (routines: Routine list) : Routine list =
    let newStep = { Id = Guid.NewGuid(); Label = label.Trim(); Minutes = max 1 minutes }
    routines
    |> List.map (fun r -> if r.Time = time then { r with Steps = r.Steps @ [ newStep ] } else r)

let removeStep (time: RitualTime) (stepId: Guid) (routines: Routine list) : Routine list =
    routines
    |> List.map (fun r ->
        if r.Time = time then { r with Steps = r.Steps |> List.filter (fun s -> s.Id <> stepId) }
        else r)
