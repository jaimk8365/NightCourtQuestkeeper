/// Night Court Questkeeper self-test suite.
/// Runs the exact F# modules the app ships with (compiled for .NET),
/// including a scripted "week in the life" that simulates the update cycle
/// at the engine level.
module NightCourt.Tests

open System
open NightCourt.Domain
open NightCourt.Catalog
open NightCourt.Progression
open NightCourt.QuestEngine
open NightCourt.RoutineEngine
open NightCourt.Planner
open NightCourt.Codec

let mutable passed = 0
let mutable failed = 0

let check (name: string) (condition: bool) =
    if condition then
        passed <- passed + 1
        printfn "  ✓ %s" name
    else
        failed <- failed + 1
        printfn "  ✗ FAILED: %s" name

let section (name: string) = printfn "\n[%s]" name

// Fixed calendar for determinism: 2026-07-06 is a Monday.
let monday = "2026-07-06"
let tuesday = "2026-07-07"
let wednesday = "2026-07-08"
let saturday = "2026-07-11"
let sunday = "2026-07-12"
let nextMonday = "2026-07-13"

let mkQuest title difficulty minutes planned deadline =
    createQuest (Guid.NewGuid()) monday title None difficulty minutes Home deadline planned

[<EntryPoint>]
let main _ =
    printfn "Night Court Questkeeper self-test suite"
    printfn "======================================="

    // ------------------------------------------------------ dates
    section "Dates"
    check "dayKey round-trips" (Dates.dayKey (Dates.parse monday) = monday)
    check "addDays crosses month ends" (Dates.addDays 26 monday = "2026-08-01")
    check "mondayOf a Monday is itself" (Dates.mondayOf monday = monday)
    check "mondayOf a Sunday is the preceding Monday" (Dates.mondayOf sunday = monday)
    check "isSunday" (Dates.isSunday sunday && not (Dates.isSunday monday))
    check "weekdayName" (Dates.weekdayName wednesday = "Wednesday")

    // ------------------------------------------------------ progression
    section "Progression / XP & levels"
    check "easy quest base XP" (xpForDifficulty Easy = 15)
    check "hard beats easy" (xpForDifficulty Hard > xpForDifficulty Easy)
    check "time bonus capped at 20" (timeBonus 600 = 20)
    check "no time bonus under 15 min" (timeBonus 10 = 0)
    check "level 1 at 0 XP" (levelForXp 0 = 1)
    check "still level 1 just under threshold" (levelForXp 79 = 1)
    check "level 2 at 80 XP" (levelForXp 80 = 2)
    check "level 3 at 80+120 XP" (levelForXp 200 = 3)
    check "curve is monotonic over 0..5000" ([ 0 .. 5000 ] |> List.forall (fun x -> levelForXp x <= levelForXp (x + 1)))
    check "levelProgress consistent with level curve"
        (let gathered, needed = levelProgress 200 in gathered = 0 && needed = xpToNextLevel 3)

    section "Progression / streaks"
    let r0 = RewardState.Initial
    let r1 = feedStreak monday r0
    check "first feed starts streak at 1" (r1.CurrentStreak = 1 && r1.BestStreak = 1)
    check "same-day feed is idempotent" (feedStreak monday r1 = r1)
    let r2 = feedStreak tuesday r1
    check "consecutive day grows streak" (r2.CurrentStreak = 2)
    let r3 = feedStreak saturday r2
    check "gap resets streak to 1, keeps best" (r3.CurrentStreak = 1 && r3.BestStreak = 2)
    check "streak alive same day" (streakAlive tuesday r2)
    check "streak alive next day" (streakAlive wednesday r2)
    check "streak dead after a gap" (not (streakAlive saturday r2))

    section "Progression / energy meter"
    let light = mkQuest "Water one plant" Easy 5 (Some monday) None
    let heavy = mkQuest "Deep clean the kitchen" Hard 120 (Some monday) None
    check "hard+long costs more than easy+short" (energyCost heavy > energyCost light)
    check "empty day is Gentle" (energyBand (energyLoad []) = Gentle)
    check "stacked hard quests read Heavy"
        (energyBand (energyLoad [ heavy; heavy; heavy ]) = Heavy)

    // ------------------------------------------------------ catalog
    section "Catalog / themes & cosmetics"
    check "theme assignment is deterministic" (assignTheme "Fold laundry" = assignTheme "Fold laundry")
    check "theme ignores case and padding" (assignTheme "  FOLD LAUNDRY " = assignTheme "fold laundry")
    check "every theme has info" (themes |> List.forall (fun t -> (themeInfo t.Theme).Name <> ""))
    check "moon phase is stable per week" (moonPhaseFor monday = moonPhaseFor monday)
    check "moon phase varies across the cycle"
        ([ 0 .. 7 ] |> List.map (fun i -> moonPhaseFor (Dates.addDays (i * 7) monday) |> fst) |> List.distinct |> List.length > 1)
    check "level 1 already grants a sigil" (sigilsUnlockedAt 1 |> List.isEmpty |> not)
    check "sigil unlock detected on level-up" ((newlyUnlockedSigil 1 2).IsSome)
    check "no sigil when no threshold crossed" ((newlyUnlockedSigil 3 4).IsNone)
    let c0, lit0, done0 = constellationProgress 0
    check "constellation progress starts empty" (c0.Name = "The Ember Moth" && lit0 = 0 && done0 = 0)
    let c1, lit1, done1 = constellationProgress 6
    check "stars overflow into the next constellation" (c1.Name = "The Night Hound" && lit1 = 1 && done1 = 1)

    // ------------------------------------------------------ quest engine
    section "Quest engine"
    let q = mkQuest "  Feed the cat  " Easy 5 None None
    check "titles are trimmed" (q.Title = "Feed the cat")
    check "new quests are active" (isActive q)
    check "completion stamps the day" ((complete monday q).CompletedOn = Some monday)
    check "minutes floor at 1" ((mkQuest "x" Easy 0 None None).Minutes = 1)

    let overdueQ = mkQuest "Pay the water bill" Medium 10 None (Some "2026-07-01")
    let dueTodayQ = mkQuest "Post the parcel" Easy 20 None (Some tuesday)
    let plannedQ = mkQuest "Hoover the stairs" Medium 15 (Some tuesday) None
    let someQ = mkQuest "Sort the garage" Hard 90 None None
    let tinyQ = mkQuest "Take out recycling" Easy 5 (Some tuesday) None
    let pool = [ someQ; plannedQ; dueTodayQ; overdueQ; tinyQ ]

    check "overdue detection" (isOverdue tuesday overdueQ && not (isOverdue tuesday dueTodayQ))
    let sorted = sortByNextBest tuesday pool
    check "overdue outranks everything" ((List.item 0 sorted).Title = "Pay the water bill")
    check "deadline-today outranks planned-today" ((List.item 1 sorted).Title = "Post the parcel")
    let sortedTitles = sorted |> List.map (fun x -> x.Title)
    check "within a band, smallest quest first"
        (List.findIndex ((=) "Take out recycling") sortedTitles < List.findIndex ((=) "Hoover the stairs") sortedTitles)
    check "completed quests never surface" (sortByNextBest tuesday [ complete monday q ] = [])
    check "nextBestAction returns the head" ((nextBestAction tuesday pool |> Option.get).Title = "Pay the water bill")
    check "nextBestAction on empty pool" (nextBestAction tuesday [] = None)

    // ------------------------------------------------------ routine engine
    section "Routine engine"
    let routines = defaultRoutines ()
    let morning = routineFor Morning routines
    check "default morning ritual seeded" (morning.Steps.Length = 4)
    check "missing routine yields empty" ((routineFor Morning []).Steps = [])

    let s1 = morning.Steps.[0]
    let t1 = toggleStep monday morning s1.Id Map.empty
    check "first toggle earns step XP" (t1.XpGained = ritualStepXp)
    check "toggle marks the step done" (isStepDone monday t1.Log s1.Id)
    let t1off = toggleStep monday morning s1.Id t1.Log
    check "un-toggling earns nothing" (t1off.XpGained = 0 && not (isStepDone monday t1off.Log s1.Id))

    let fullLog =
        morning.Steps |> List.fold (fun log s -> (toggleStep monday morning s.Id log).Log) Map.empty
    check "all steps completes the ritual" (isRitualComplete monday fullLog morning)
    let lastStep = List.last morning.Steps
    let allButLast = morning.Steps |> List.take 3 |> List.fold (fun log s -> (toggleStep monday morning s.Id log).Log) Map.empty
    let finisher = toggleStep monday morning lastStep.Id allButLast
    check "final step grants the full-ritual bonus" (finisher.XpGained = ritualStepXp + fullRitualBonus && finisher.RitualCompleted)
    check "other days are unaffected" (not (isRitualComplete tuesday fullLog morning))

    let edited = addStep Evening "Stretch for two minutes" 2 routines
    check "addStep appends to the right ritual" ((routineFor Evening edited).Steps |> List.exists (fun s -> s.Label = "Stretch for two minutes"))
    check "addStep leaves the other ritual alone" ((routineFor Morning edited).Steps.Length = 4)
    let evening = routineFor Evening edited
    let removed = removeStep Evening evening.Steps.Head.Id edited
    check "removeStep removes exactly one" ((routineFor Evening removed).Steps.Length = evening.Steps.Length - 1)

    // ------------------------------------------------------ planner
    section "Planner"
    check "week has seven days" ((weekDays monday).Length = 7 && List.last (weekDays monday) = sunday)
    check "Sunday Ritual targets next week" (ritualTargetWeek sunday = nextMonday)
    check "mid-week planning targets current week" (ritualTargetWeek wednesday = monday)

    let baseData = { initialData () with Profile = Some { Name = "Jaimi"; JoinedOn = monday } }

    let plansWithIntention = setIntention monday "Gentle progress" []
    check "intention is stored" ((weekPlanFor monday plansWithIntention).Intention = Some "Gentle progress")
    check "blank intention clears" ((weekPlanFor monday (setIntention monday "  " plansWithIntention)).Intention = None)
    check "unknown week yields empty plan" ((weekPlanFor nextMonday plansWithIntention).Intention = None)

    let busyMon = mkQuest "Big report" Hard 120 (Some monday) None
    check "lightestDay avoids the loaded day" (lightestDay [ busyMon ] monday monday <> monday)
    check "lightestDay respects notBefore" (lightestDay [] monday wednesday = wednesday)

    // suggestions: one unfinished quest + one recurring pattern
    let oldUnfinished = mkQuest "Descale the kettle" Easy 10 None None
    let recur1 = { mkQuest "Water the plants" Easy 5 None None with CompletedOn = Some "2026-06-22"; CreatedOn = "2026-06-22" }
    let recur2 = { mkQuest "Water the plants" Easy 5 None None with CompletedOn = Some "2026-06-29"; CreatedOn = "2026-06-29" }
    let oneOff = { mkQuest "Assemble bookshelf" Hard 60 None None with CompletedOn = Some "2026-06-29" }
    let histData = { baseData with Quests = [ oldUnfinished; recur1; recur2; oneOff ] }
    let sugg = suggestionsFor histData nextMonday
    check "unfinished quest is suggested"
        (sugg |> List.exists (fun s -> s.Title = "Descale the kettle" && s.Source = UnfinishedQuest && s.ExistingId = Some oldUnfinished.Id))
    check "twice-completed title becomes recurring suggestion"
        (sugg |> List.exists (fun s -> s.Title = "Water the plants" && s.Source = RecurringPattern && s.ExistingId = None))
    check "one-off completion is not suggested" (sugg |> List.forall (fun s -> s.Title <> "Assemble bookshelf"))
    check "suggestions are capped at 8" ((suggestionsFor { histData with Quests = [ for i in 1 .. 20 -> mkQuest (sprintf "Task %d" i) Easy 5 None None ] } nextMonday).Length <= 8)

    let assigned = assignQuestToDay oldUnfinished.Id (Some tuesday) histData.Quests
    check "assignQuestToDay schedules the quest"
        ((assigned |> List.find (fun x -> x.Id = oldUnfinished.Id)).PlannedFor = Some tuesday)
    check "quest scheduled into target week stops being suggested"
        (suggestionsFor { histData with Quests = assignQuestToDay oldUnfinished.Id (Some (Dates.addDays 1 nextMonday)) histData.Quests } nextMonday
         |> List.forall (fun s -> s.Title <> "Descale the kettle"))

    // ------------------------------------------------------ codec
    section "Codec / persistence round-trip"
    let richData =
        { baseData with
            Quests = [ overdueQ; complete monday q ]
            RitualLog = Map.ofList [ monday, [ s1.Id ] ]
            WeekPlans = [ { WeekOf = monday; Intention = Some "Rest more" } ]
            Rewards = { Xp = 345; CurrentStreak = 4; BestStreak = 9; LastStreakDay = Some monday }
            SyncEndpoint = Some "https://example.test/blob" }
    match deserialize (serialize richData) with
    | Ok round ->
        check "round-trip preserves everything" (round = richData)
        check "round-trip preserves ritual log" (round.RitualLog = richData.RitualLog)
    | Error e ->
        check (sprintf "round-trip failed: %s" e) false
    check "garbage input is rejected" (match deserialize "not json" with Error _ -> true | Ok _ -> false)
    check "initial data round-trips" (match deserialize (serialize (initialData ())) with Ok d -> d.Profile = None | Error _ -> false)

    // ------------------------------------------------------ simulated week
    section "Simulated update cycle — a week in the life"
    // Drive the same pure transitions State.update delegates to, over several days.
    let mutable data = { baseData with Quests = [ overdueQ; dueTodayQ; plannedQ; tinyQ ] }
    let mutable celebrationCount = 0

    let completeNext (day: string) =
        match nextBestAction day data.Quests with
        | Some quest ->
            let quests' = data.Quests |> List.map (fun x -> if x.Id = quest.Id then complete day x else x)
            let rewards' = feedStreak day { data.Rewards with Xp = data.Rewards.Xp + questXp quest }
            data <- { data with Quests = quests'; Rewards = rewards' }
            celebrationCount <- celebrationCount + 1
        | None -> ()

    // Tuesday: burn through everything in next-best order.
    for _ in 1 .. 4 do completeNext tuesday
    check "all four quests completed in order" (activeQuests data.Quests = [])
    check "four celebrations fired" (celebrationCount = 4)
    check "XP accumulated across completions"
        (data.Rewards.Xp = ([ overdueQ; dueTodayQ; plannedQ; tinyQ ] |> List.sumBy questXp))
    check "streak fed exactly once for the day" (data.Rewards.CurrentStreak = 1)
    check "completing on the next day grows the streak"
        (data <- { data with Quests = mkQuest "One more thing" Easy 5 (Some wednesday) None :: data.Quests }
         completeNext wednesday
         data.Rewards.CurrentStreak = 2)
    check "stars lit equals quests completed" (totalCompleted data.Quests = 5)
    check "level rose from the week's work" (levelForXp data.Rewards.Xp >= 2)
    check "the whole simulated state still round-trips"
        (match deserialize (serialize data) with Ok d -> d = data | Error _ -> false)

    // ------------------------------------------------------ summary
    printfn "\n======================================="
    printfn "Passed: %d   Failed: %d" passed failed
    if failed = 0 then
        printfn "The Night Court is pleased. ✦"
        0
    else
        printfn "The stars found flaws."
        1
