/// App entry point: root view (onboarding gate + page router) and the
/// Elmish program wiring.
module NightCourt.Main

open Feliz
open Elmish
open Elmish.React
open NightCourt.State
open NightCourt.ViewShared

/// First-run naming ceremony.
let private onboarding (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "onboarding"
        prop.children [
            Html.div [
                prop.className "onboarding-card"
                prop.children [
                    Html.span [ prop.className "onboarding-moon"; prop.text "🌙" ]
                    Html.h1 [ prop.text "Night Court Questkeeper" ]
                    Html.p [
                        prop.className "ritual-copy"
                        prop.text "Your tasks are magical quests bestowed upon you by the Night Court. Completing them strengthens your powers."
                    ]
                    Html.input [
                        prop.className "input"
                        prop.type' "text"
                        prop.placeholder "What shall the Court call you?"
                        prop.value model.NameDraft
                        prop.autoFocus true
                        prop.onChange (SetNameDraft >> dispatch)
                        prop.onKeyUp (fun e -> if e.key = "Enter" then dispatch SubmitName)
                    ]
                    Html.button [
                        prop.className "btn btn-primary"
                        prop.disabled (model.NameDraft.Trim() = "")
                        prop.onClick (fun _ -> dispatch SubmitName)
                        prop.text "Enter the Court ✨"
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "app"
        prop.children [
            starfield
            match model.Data.Profile with
            | None -> onboarding model dispatch
            | Some _ ->
                Html.div [
                    prop.className "app-frame"
                    prop.children [
                        header model
                        Html.main [
                            prop.className "content"
                            prop.children [
                                match model.Page with
                                | TodayPage -> ViewToday.view model dispatch
                                | WeekPage -> ViewWeek.view model dispatch
                                | RitualsPage -> ViewRituals.view model dispatch
                                | StarsPage -> ViewStars.view model dispatch
                            ]
                        ]
                        navBar model dispatch
                    ]
                ]
            match model.Celebration with
            | Some c -> celebrationOverlay c dispatch
            | None -> Html.none
            match model.Toast with
            | Some t -> toast t dispatch
            | None -> Html.none
        ]
    ]

Program.mkProgram init update view
|> Program.withReactSynchronous "root"
|> Program.run
