/// The Rituals screen: edit the morning & night routines (set once, appear daily).
module NightCourt.ViewRituals

open Feliz
open NightCourt.Domain
open NightCourt.Progression
open NightCourt.RoutineEngine
open NightCourt.State
open NightCourt.ViewShared

let private editor (model: Model) (dispatch: Msg -> unit) (time: RitualTime) =
    let routine = routineFor time model.Data.Routines
    let title, glyph =
        match time with
        | Morning -> "Morning Ritual", "☀"
        | Evening -> "Night Ritual", "☾"
    let draft =
        match time with
        | Morning -> model.MorningStepDraft
        | Evening -> model.EveningStepDraft
    Html.section [
        prop.className "panel"
        prop.children [
            sectionTitle glyph title
            if routine.Steps.IsEmpty then
                Html.p [ prop.className "empty-copy"; prop.text "No steps yet — add the first below." ]
            Html.div [
                prop.className "ritual-edit-list"
                prop.children [
                    for step in routine.Steps do
                        Html.div [
                            prop.className "ritual-edit-item"
                            prop.children [
                                Html.span [ prop.className "ritual-label"; prop.text step.Label ]
                                Html.span [ prop.className "ritual-mins"; prop.text (sprintf "%d min" step.Minutes) ]
                                Html.button [
                                    prop.className "quest-release"
                                    prop.ariaLabel (sprintf "Remove %s" step.Label)
                                    prop.onClick (fun _ -> dispatch (RemoveRoutineStep (time, step.Id)))
                                    prop.text "×"
                                ]
                            ]
                        ]
                ]
            ]
            Html.div [
                prop.className "intention-row"
                prop.children [
                    Html.input [
                        prop.className "input"
                        prop.type' "text"
                        prop.placeholder "Add a step…"
                        prop.value draft
                        prop.onChange (fun (v: string) -> dispatch (SetStepDraft (time, v)))
                        prop.onKeyUp (fun e -> if e.key = "Enter" then dispatch (AddRoutineStep time))
                    ]
                    Html.button [
                        prop.className "btn btn-small"
                        prop.onClick (fun _ -> dispatch (AddRoutineStep time))
                        prop.text "Add"
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "page"
        prop.children [
            Html.section [
                prop.className "panel panel-ritual"
                prop.children [
                    sectionTitle "❋" "Your Daily Rituals"
                    Html.p [
                        prop.className "ritual-copy"
                        prop.text (sprintf
                            "Set these once; they return each dawn and dusk. Every step earns %d XP, and completing a full ritual grants a %d XP blessing plus streak light."
                            ritualStepXp fullRitualBonus)
                    ]
                ]
            ]
            editor model dispatch Morning
            editor model dispatch Evening
        ]
    ]
