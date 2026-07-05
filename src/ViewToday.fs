/// The Today screen: Next Best Action hero, daily rituals, today's quests,
/// and the "bestow a quest" form.
module NightCourt.ViewToday

open Feliz
open NightCourt.Domain
open NightCourt.Catalog
open NightCourt.Progression
open NightCourt.QuestEngine
open NightCourt.RoutineEngine
open NightCourt.State
open NightCourt.ViewShared

// ---------------------------------------------------------------- rituals

let private ritualSection (model: Model) (dispatch: Msg -> unit) (time: RitualTime) =
    let routine = routineFor time model.Data.Routines
    if routine.Steps.IsEmpty then Html.none
    else
        let title, glyph =
            match time with
            | Morning -> "Morning Ritual", "☀"
            | Evening -> "Night Ritual", "☾"
        let complete = isRitualComplete model.Today model.Data.RitualLog routine
        Html.section [
            prop.className "panel"
            prop.children [
                sectionTitle glyph (if complete then title + " — woven ✓" else title)
                Html.div [
                    prop.className "ritual-steps"
                    prop.children [
                        for step in routine.Steps do
                            let isDone = isStepDone model.Today model.Data.RitualLog step.Id
                            Html.button [
                                prop.className (if isDone then "ritual-step ritual-step-done" else "ritual-step")
                                prop.onClick (fun _ -> dispatch (ToggleRitualStep (time, step.Id)))
                                prop.children [
                                    Html.span [ prop.className "ritual-check"; prop.text (if isDone then "✦" else "○") ]
                                    Html.span [ prop.className "ritual-label"; prop.text step.Label ]
                                    Html.span [ prop.className "ritual-mins"; prop.text (sprintf "%d min" step.Minutes) ]
                                ]
                            ]
                    ]
                ]
            ]
        ]

// ---------------------------------------------------------------- quest form

let private questForm (form: QuestForm) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "modal-overlay"
        prop.children [
            Html.div [
                prop.className "modal"
                prop.children [
                    Html.h2 [ prop.text "Bestow a Quest" ]
                    Html.input [
                        prop.className "input"
                        prop.type' "text"
                        prop.placeholder "What must be done?"
                        prop.value form.Title
                        prop.autoFocus true
                        prop.onChange (SetFormTitle >> dispatch)
                    ]
                    Html.textarea [
                        prop.className "input input-area"
                        prop.placeholder "Details, if the quest needs them (optional)"
                        prop.value form.Description
                        prop.onChange (SetFormDescription >> dispatch)
                    ]

                    Html.label [ prop.className "form-label"; prop.text "Difficulty" ]
                    Html.div [
                        prop.className "seg-row"
                        prop.children [
                            for d in allDifficulties do
                                Html.button [
                                    prop.className (if form.Difficulty = d then "seg seg-active" else "seg")
                                    prop.onClick (fun _ -> dispatch (SetFormDifficulty d))
                                    prop.text (difficultyLabel d)
                                ]
                        ]
                    ]

                    Html.label [ prop.className "form-label"; prop.text "How long might it take?" ]
                    Html.div [
                        prop.className "seg-row"
                        prop.children [
                            for m in [ 5; 15; 30; 60; 120 ] do
                                Html.button [
                                    prop.className (if form.Minutes = m then "seg seg-active" else "seg")
                                    prop.onClick (fun _ -> dispatch (SetFormMinutes m))
                                    prop.text (if m >= 60 then sprintf "%dh" (m / 60) else sprintf "%dm" m)
                                ]
                        ]
                    ]

                    Html.label [ prop.className "form-label"; prop.text "Realm" ]
                    Html.div [
                        prop.className "seg-row seg-row-wrap"
                        prop.children [
                            for c in allCategories do
                                Html.button [
                                    prop.className (if form.Category = c then "seg seg-active" else "seg")
                                    prop.onClick (fun _ -> dispatch (SetFormCategory c))
                                    prop.text (categoryLabel c)
                                ]
                        ]
                    ]

                    Html.label [ prop.className "form-label"; prop.text "Deadline (optional)" ]
                    Html.input [
                        prop.className "input"
                        prop.type' "date"
                        prop.value form.Deadline
                        prop.onChange (SetFormDeadline >> dispatch)
                    ]

                    Html.button [
                        prop.className (if form.PlanToday then "toggle toggle-on" else "toggle")
                        prop.onClick (fun _ -> dispatch ToggleFormPlanToday)
                        prop.text (if form.PlanToday then "✦ Woven into today" else "○ Save for the week ahead")
                    ]

                    Html.div [
                        prop.className "modal-actions"
                        prop.children [
                            Html.button [
                                prop.className "btn"
                                prop.onClick (fun _ -> dispatch CloseQuestForm)
                                prop.text "Not now"
                            ]
                            Html.button [
                                prop.className "btn btn-primary"
                                prop.disabled (form.Title.Trim() = "")
                                prop.onClick (fun _ -> dispatch SubmitQuest)
                                prop.text "Bestow Quest ✨"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// ---------------------------------------------------------------- page

let view (model: Model) (dispatch: Msg -> unit) =
    let quests = model.Data.Quests
    let next = nextBestAction model.Today quests
    let ordered = sortByNextBest model.Today quests
    let rest = match next with Some n -> ordered |> List.filter (fun q -> q.Id <> n.Id) | None -> ordered
    let doneToday = completedOn model.Today quests
    let todayLoad = dayEnergyLoad model.Today quests

    Html.div [
        prop.className "page"
        prop.children [
            // Next Best Action — the single answer to "what do I do?"
            Html.section [
                prop.className "panel panel-hero"
                prop.children [
                    sectionTitle "✦" "The stars suggest…"
                    match next with
                    | Some q -> questCard model.Today true q dispatch
                    | None ->
                        Html.p [
                            prop.className "empty-copy"
                            prop.text "No quests await. Rest, or bestow a new one below — both are victories."
                        ]
                    if not (List.isEmpty (plannedFor model.Today quests)) then
                        energyMeter todayLoad
                ]
            ]

            ritualSection model dispatch Morning

            if not rest.IsEmpty then
                Html.section [
                    prop.className "panel"
                    prop.children [
                        sectionTitle "◈" "Quests awaiting"
                        Html.div [
                            prop.className "quest-list"
                            prop.children [ for q in rest -> questCard model.Today false q dispatch ]
                        ]
                    ]
                ]

            ritualSection model dispatch Evening

            if not doneToday.IsEmpty then
                Html.section [
                    prop.className "panel panel-done"
                    prop.children [
                        sectionTitle "✓" (sprintf "Woven today — %d quest%s" doneToday.Length (if doneToday.Length = 1 then "" else "s"))
                        Html.div [
                            prop.className "done-list"
                            prop.children [
                                for q in doneToday do
                                    let info = themeInfo q.Theme
                                    Html.div [
                                        prop.className "done-item"
                                        prop.children [
                                            Html.span [ prop.style [ style.color info.Color ]; prop.text info.Glyph ]
                                            Html.span [ prop.text q.Title ]
                                            Html.span [ prop.className "done-xp"; prop.text (sprintf "+%d XP" (questXp q)) ]
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]

            Html.button [
                prop.className "fab"
                prop.ariaLabel "Bestow a new quest"
                prop.onClick (fun _ -> dispatch OpenQuestForm)
                prop.text "+"
            ]

            match model.QuestForm with
            | Some form -> questForm form dispatch
            | None -> Html.none
        ]
    ]
