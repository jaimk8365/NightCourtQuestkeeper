/// The Elmish MVU core: Model, Msg, init, update.
/// All domain work is delegated to the pure engine modules; this layer
/// orchestrates them and persists after every data change.
module NightCourt.State

open System
open Elmish
open NightCourt.Domain
open NightCourt.Catalog
open NightCourt.Progression
open NightCourt.QuestEngine
open NightCourt.RoutineEngine
open NightCourt.Planner
open NightCourt.Storage

// ---------------------------------------------------------------- model

type Page =
    | TodayPage
    | WeekPage
    | RitualsPage
    | StarsPage

type QuestForm =
    { Title: string
      Description: string
      Difficulty: Difficulty
      Minutes: int
      Category: Category
      Deadline: string // "" = no deadline
      PlanToday: bool }

    static member Empty =
        { Title = ""
          Description = ""
          Difficulty = Easy
          Minutes = 15
          Category = Home
          Deadline = ""
          PlanToday = true }

/// Full-screen dopamine moment shown after a completion.
type Celebration =
    { Heading: string
      Lines: string list
      Xp: int }

type Model =
    { Data: AppData
      Today: string
      Page: Page
      QuestForm: QuestForm option
      NameDraft: string
      IntentionDraft: string
      MorningStepDraft: string
      EveningStepDraft: string
      /// Monday key of the week shown in the planner.
      PlannerWeek: string
      Celebration: Celebration option
      Toast: string option
      SyncDraft: string
      ImportDraft: string }

type Msg =
    | SetPage of Page
    // onboarding
    | SetNameDraft of string
    | SubmitName
    // quest form
    | OpenQuestForm
    | CloseQuestForm
    | SetFormTitle of string
    | SetFormDescription of string
    | SetFormDifficulty of Difficulty
    | SetFormMinutes of int
    | SetFormCategory of Category
    | SetFormDeadline of string
    | ToggleFormPlanToday
    | SubmitQuest
    // quests
    | CompleteQuest of Guid
    | ReleaseQuest of Guid
    // rituals
    | ToggleRitualStep of RitualTime * Guid
    | SetStepDraft of RitualTime * string
    | AddRoutineStep of RitualTime
    | RemoveRoutineStep of RitualTime * Guid
    // planner
    | SetPlannerWeek of string
    | SetIntentionDraft of string
    | SaveIntention
    | AssignQuest of Guid * string option
    | AcceptSuggestion of Suggestion
    // overlays
    | DismissCelebration
    | DismissToast
    // sync & data
    | SetSyncDraft of string
    | SaveSyncEndpoint
    | PullFromCloud
    | CloudPulled of Result<AppData, string>
    | SetImportDraft of string
    | ImportData

// ---------------------------------------------------------------- init

let init () : Model * Cmd<Msg> =
    let today = Dates.dayKey DateTime.Now
    let data = loadData () |> Option.defaultValue (initialData ())
    { Data = data
      Today = today
      Page = TodayPage
      QuestForm = None
      NameDraft = ""
      IntentionDraft = (weekPlanFor (ritualTargetWeek today) data.WeekPlans).Intention |> Option.defaultValue ""
      MorningStepDraft = ""
      EveningStepDraft = ""
      PlannerWeek = ritualTargetWeek today
      Celebration = None
      Toast = None
      SyncDraft = data.SyncEndpoint |> Option.defaultValue ""
      ImportDraft = "" },
    Cmd.none

// ---------------------------------------------------------------- helpers

/// Commit a data change: store it in the model and persist (local + cloud push).
let private commit (data: AppData) (model: Model) : Model * Cmd<Msg> =
    { model with Data = data },
    Cmd.ofEffect (fun _ ->
        saveData data
        Sync.push data)

let private updateForm (f: QuestForm -> QuestForm) (model: Model) =
    { model with QuestForm = model.QuestForm |> Option.map f }, Cmd.none

/// Award XP (and feed the streak) after any completion, producing the
/// celebration details: level-ups, sigil unlocks, constellation stars.
let private award (xp: int) (heading: string) (extraLines: string list) (model: Model) : Model * Cmd<Msg> =
    let rewards = model.Data.Rewards
    let oldLevel = levelForXp rewards.Xp
    let rewards' = feedStreak model.Today { rewards with Xp = rewards.Xp + xp }
    let newLevel = levelForXp rewards'.Xp

    let levelLines =
        if newLevel > oldLevel then
            [ sprintf "Level %d — your powers deepen." newLevel ]
            @ (match newlyUnlockedSigil oldLevel newLevel with
               | Some s -> [ sprintf "%s  %s unlocked!" s.Glyph s.Name ]
               | None -> [])
        else []

    let streakLine =
        if rewards'.CurrentStreak > rewards.CurrentStreak && rewards'.CurrentStreak > 1 then
            [ sprintf "✨ %d-day streak — the sky remembers." rewards'.CurrentStreak ]
        else []

    let celebration =
        { Heading = heading
          Lines = extraLines @ levelLines @ streakLine
          Xp = xp }

    let model', cmd = commit { model.Data with Rewards = rewards' } model
    { model' with Celebration = Some celebration }, cmd

// ---------------------------------------------------------------- update

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | SetPage page ->
        { model with Page = page }, Cmd.none

    // ---------------------------------------------------- onboarding
    | SetNameDraft name ->
        { model with NameDraft = name }, Cmd.none

    | SubmitName ->
        let name = model.NameDraft.Trim()
        if name = "" then model, Cmd.none
        else
            let profile = { Name = name; JoinedOn = model.Today }
            commit { model.Data with Profile = Some profile } model

    // ---------------------------------------------------- quest form
    | OpenQuestForm ->
        { model with QuestForm = Some QuestForm.Empty }, Cmd.none

    | CloseQuestForm ->
        { model with QuestForm = None }, Cmd.none

    | SetFormTitle v -> updateForm (fun f -> { f with Title = v }) model
    | SetFormDescription v -> updateForm (fun f -> { f with Description = v }) model
    | SetFormDifficulty v -> updateForm (fun f -> { f with Difficulty = v }) model
    | SetFormMinutes v -> updateForm (fun f -> { f with Minutes = v }) model
    | SetFormCategory v -> updateForm (fun f -> { f with Category = v }) model
    | SetFormDeadline v -> updateForm (fun f -> { f with Deadline = v }) model
    | ToggleFormPlanToday -> updateForm (fun f -> { f with PlanToday = not f.PlanToday }) model

    | SubmitQuest ->
        match model.QuestForm with
        | Some form when form.Title.Trim() <> "" ->
            let quest =
                createQuest
                    (Guid.NewGuid())
                    model.Today
                    form.Title
                    (if form.Description.Trim() = "" then None else Some form.Description)
                    form.Difficulty
                    form.Minutes
                    form.Category
                    (if form.Deadline = "" then None else Some form.Deadline)
                    (if form.PlanToday then Some model.Today else None)
            let model', cmd = commit { model.Data with Quests = quest :: model.Data.Quests } model
            { model' with QuestForm = None }, cmd
        | _ -> model, Cmd.none

    // ---------------------------------------------------- quests
    | CompleteQuest id ->
        match model.Data.Quests |> List.tryFind (fun q -> q.Id = id && isActive q) with
        | None -> model, Cmd.none
        | Some quest ->
            let quests' = model.Data.Quests |> List.map (fun q -> if q.Id = id then complete model.Today q else q)
            let xp = questXp quest
            let stars = totalCompleted quests'
            let constellation, lit, _ = constellationProgress stars
            let starLine =
                if lit >= constellation.Stars then
                    [ sprintf "🌌 %s is complete — a new constellation awaits." constellation.Name ]
                else
                    [ sprintf "A star lights in %s (%d of %d)." constellation.Name lit constellation.Stars ]
            let info = themeInfo quest.Theme
            let model', cmd1 = commit { model.Data with Quests = quests' } model
            let model'', cmd2 =
                award xp
                    (sprintf "%s  %s complete!" info.Glyph info.Name)
                    ((cheerFor stars) :: starLine)
                    model'
            model'', Cmd.batch [ cmd1; cmd2 ]

    | ReleaseQuest id ->
        // "Release" rather than delete — no guilt, the quest returns to the mist.
        commit { model.Data with Quests = model.Data.Quests |> List.filter (fun q -> q.Id <> id) } model

    // ---------------------------------------------------- rituals
    | ToggleRitualStep (time, stepId) ->
        let routine = routineFor time model.Data.Routines
        let result = toggleStep model.Today routine stepId model.Data.RitualLog
        let model', cmd = commit { model.Data with RitualLog = result.Log } model
        if result.RitualCompleted then
            let name = match time with Morning -> "Morning Ritual" | Evening -> "Night Ritual"
            let model'', cmd2 =
                award result.XpGained
                    (sprintf "☾ %s complete!" name)
                    [ "Your daily magic is woven." ]
                    model'
            model'', Cmd.batch [ cmd; cmd2 ]
        elif result.XpGained > 0 then
            let model'', cmd2 =
                commit { model'.Data with Rewards = feedStreak model.Today { model'.Data.Rewards with Xp = model'.Data.Rewards.Xp + result.XpGained } } model'
            model'', Cmd.batch [ cmd; cmd2 ]
        else
            model', cmd

    | SetStepDraft (Morning, v) -> { model with MorningStepDraft = v }, Cmd.none
    | SetStepDraft (Evening, v) -> { model with EveningStepDraft = v }, Cmd.none

    | AddRoutineStep time ->
        let draft = match time with Morning -> model.MorningStepDraft | Evening -> model.EveningStepDraft
        if draft.Trim() = "" then model, Cmd.none
        else
            let model', cmd = commit { model.Data with Routines = addStep time draft 5 model.Data.Routines } model
            (match time with
             | Morning -> { model' with MorningStepDraft = "" }
             | Evening -> { model' with EveningStepDraft = "" }),
            cmd

    | RemoveRoutineStep (time, stepId) ->
        commit { model.Data with Routines = removeStep time stepId model.Data.Routines } model

    // ---------------------------------------------------- planner
    | SetPlannerWeek weekOf ->
        { model with
            PlannerWeek = weekOf
            IntentionDraft = (weekPlanFor weekOf model.Data.WeekPlans).Intention |> Option.defaultValue "" },
        Cmd.none

    | SetIntentionDraft v ->
        { model with IntentionDraft = v }, Cmd.none

    | SaveIntention ->
        let model', cmd =
            commit { model.Data with WeekPlans = setIntention model.PlannerWeek model.IntentionDraft model.Data.WeekPlans } model
        { model' with Toast = Some "Intention woven into the week." }, cmd

    | AssignQuest (id, day) ->
        commit { model.Data with Quests = assignQuestToDay id day model.Data.Quests } model

    | AcceptSuggestion s ->
        // Accepted suggestions land on the lightest-energy day automatically —
        // saying "yes" never requires another decision.
        let notBefore = max model.PlannerWeek model.Today
        let day = lightestDay model.Data.Quests model.PlannerWeek notBefore
        let quests' =
            match s.ExistingId with
            | Some id -> assignQuestToDay id (Some day) model.Data.Quests
            | None ->
                let q = createQuest (Guid.NewGuid()) model.Today s.Title None s.Difficulty s.Minutes s.Category None (Some day)
                q :: model.Data.Quests
        let model', cmd = commit { model.Data with Quests = quests' } model
        { model' with Toast = Some (sprintf "Placed on %s — the lightest day." (Dates.weekdayName day)) }, cmd

    // ---------------------------------------------------- overlays
    | DismissCelebration ->
        { model with Celebration = None }, Cmd.none

    | DismissToast ->
        { model with Toast = None }, Cmd.none

    // ---------------------------------------------------- sync & data
    | SetSyncDraft v ->
        { model with SyncDraft = v }, Cmd.none

    | SaveSyncEndpoint ->
        let endpoint = if model.SyncDraft.Trim() = "" then None else Some (model.SyncDraft.Trim())
        let model', cmd = commit { model.Data with SyncEndpoint = endpoint } model
        { model' with
            Toast = Some (match endpoint with
                          | Some _ -> "Cloud bond formed — your quests will follow you."
                          | None -> "Cloud bond released.") },
        cmd

    | PullFromCloud ->
        match model.Data.SyncEndpoint with
        | Some url ->
            model, Cmd.ofEffect (fun dispatch -> Sync.pull url (CloudPulled >> dispatch))
        | None ->
            { model with Toast = Some "Set a cloud endpoint first." }, Cmd.none

    | CloudPulled (Ok data) ->
        let merged = { data with SyncEndpoint = model.Data.SyncEndpoint }
        let model', cmd = commit merged model
        { model' with Toast = Some "Your quests have crossed the night sky." }, cmd

    | CloudPulled (Error e) ->
        { model with Toast = Some e }, Cmd.none

    | SetImportDraft v ->
        { model with ImportDraft = v }, Cmd.none

    | ImportData ->
        match Codec.deserialize (model.ImportDraft.Trim()) with
        | Ok data ->
            let model', cmd = commit data model
            { model' with ImportDraft = ""; Toast = Some "Scroll unfurled — data restored." }, cmd
        | Error _ ->
            { model with Toast = Some "That scroll could not be read." }, Cmd.none
