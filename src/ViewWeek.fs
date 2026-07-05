/// The Week screen: Sunday Ritual planning, day-by-day energy overview,
/// auto-suggestions, and scheduling of unplanned quests.
module NightCourt.ViewWeek

open Feliz
open NightCourt.Domain
open NightCourt.Catalog
open NightCourt.Progression
open NightCourt.QuestEngine
open NightCourt.Planner
open NightCourt.State
open NightCourt.ViewShared

let private dayLetters = [ "M"; "T"; "W"; "T"; "F"; "S"; "S" ]

/// Compact per-quest day assigner: seven letter buttons + an unschedule dot.
let private dayPicker (weekOf: string) (q: Quest) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "day-picker"
        prop.children [
            for (i, day) in List.indexed (weekDays weekOf) do
                Html.button [
                    prop.className (if q.PlannedFor = Some day then "day-dot day-dot-active" else "day-dot")
                    prop.ariaLabel (sprintf "Plan for %s" (Dates.weekdayName day))
                    prop.onClick (fun _ -> dispatch (AssignQuest (q.Id, Some day)))
                    prop.text dayLetters.[i]
                ]
            if q.PlannedFor.IsSome then
                Html.button [
                    prop.className "day-dot day-dot-clear"
                    prop.ariaLabel "Unschedule"
                    prop.onClick (fun _ -> dispatch (AssignQuest (q.Id, None)))
                    prop.text "–"
                ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    let weekOf = model.PlannerWeek
    let overview = weekOverview model.Data.Quests weekOf
    let moonGlyph, moonLine = moonPhaseFor weekOf
    let suggestions = suggestionsFor model.Data weekOf
    let pool = unscheduled model.Data.Quests
    let isSundayRitual = Dates.isSunday model.Today && weekOf = ritualTargetWeek model.Today

    Html.div [
        prop.className "page"
        prop.children [
            // Sunday Ritual banner
            if isSundayRitual then
                Html.section [
                    prop.className "panel panel-ritual"
                    prop.children [
                        sectionTitle "🕯" "The Sunday Ritual"
                        Html.p [
                            prop.className "ritual-copy"
                            prop.text "Light a candle, breathe, and weave the week ahead. The Court has gathered suggestions below — accept what serves you, release the rest."
                        ]
                    ]
                ]

            // Week header: navigation + moon phase + intention
            Html.section [
                prop.className "panel"
                prop.children [
                    Html.div [
                        prop.className "week-nav"
                        prop.children [
                            Html.button [
                                prop.className "btn btn-small"
                                prop.onClick (fun _ -> dispatch (SetPlannerWeek (Dates.addDays -7 weekOf)))
                                prop.text "◂"
                            ]
                            Html.div [
                                prop.className "week-title"
                                prop.children [
                                    Html.h2 [ prop.text (sprintf "Week of %s" (Dates.shortLabel weekOf)) ]
                                    Html.p [ prop.className "moon-line"; prop.text (sprintf "%s %s" moonGlyph moonLine) ]
                                ]
                            ]
                            Html.button [
                                prop.className "btn btn-small"
                                prop.onClick (fun _ -> dispatch (SetPlannerWeek (Dates.addDays 7 weekOf)))
                                prop.text "▸"
                            ]
                        ]
                    ]
                    Html.div [
                        prop.className "intention-row"
                        prop.children [
                            Html.input [
                                prop.className "input"
                                prop.type' "text"
                                prop.placeholder "An intention for this week (optional)…"
                                prop.value model.IntentionDraft
                                prop.onChange (SetIntentionDraft >> dispatch)
                            ]
                            Html.button [
                                prop.className "btn btn-small"
                                prop.onClick (fun _ -> dispatch SaveIntention)
                                prop.text "Weave"
                            ]
                        ]
                    ]
                ]
            ]

            // Day-by-day overview with energy loads
            Html.section [
                prop.className "panel"
                prop.children [
                    sectionTitle "☾" "The week's shape"
                    Html.div [
                        prop.className "week-days"
                        prop.children [
                            for day in overview do
                                let isToday = day.Date = model.Today
                                Html.div [
                                    prop.className (if isToday then "week-day week-day-today" else "week-day")
                                    prop.children [
                                        Html.div [
                                            prop.className "week-day-head"
                                            prop.children [
                                                Html.span [ prop.className "week-day-name"; prop.text (Dates.shortLabel day.Date) ]
                                                if isToday then Html.span [ prop.className "chip chip-due"; prop.text "today" ]
                                            ]
                                        ]
                                        if day.Quests.IsEmpty then
                                            Html.p [ prop.className "week-day-empty"; prop.text "An open sky." ]
                                        else
                                            Html.div [
                                                prop.children [
                                                    for q in day.Quests do
                                                        let info = themeInfo q.Theme
                                                        Html.div [
                                                            prop.className "week-quest"
                                                            prop.children [
                                                                Html.span [ prop.style [ style.color info.Color ]; prop.text info.Glyph ]
                                                                Html.span [ prop.className "week-quest-title"; prop.text q.Title ]
                                                                Html.span [ prop.className "week-quest-mins"; prop.text (sprintf "%dm" q.Minutes) ]
                                                            ]
                                                        ]
                                                ]
                                            ]
                                        if not day.Quests.IsEmpty then energyMeter day.EnergyLoad
                                    ]
                                ]
                        ]
                    ]
                ]
            ]

            // Auto-suggestions
            if not suggestions.IsEmpty then
                Html.section [
                    prop.className "panel"
                    prop.children [
                        sectionTitle "✧" "The Court suggests"
                        Html.p [ prop.className "hint-copy"; prop.text "Accepted quests land on the lightest day — no scheduling needed." ]
                        Html.div [
                            prop.className "suggestion-list"
                            prop.children [
                                for s in suggestions do
                                    Html.div [
                                        prop.className "suggestion"
                                        prop.children [
                                            Html.div [
                                                prop.className "suggestion-body"
                                                prop.children [
                                                    Html.span [ prop.className "suggestion-title"; prop.text s.Title ]
                                                    Html.span [
                                                        prop.className "chip chip-dim"
                                                        prop.text (match s.Source with
                                                                   | UnfinishedQuest -> "awaiting completion"
                                                                   | RecurringPattern -> "returns each week")
                                                    ]
                                                ]
                                            ]
                                            Html.button [
                                                prop.className "btn btn-small btn-primary"
                                                prop.onClick (fun _ -> dispatch (AcceptSuggestion s))
                                                prop.text "Accept ✦"
                                            ]
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]

            // Unscheduled pool with day pickers
            if not pool.IsEmpty then
                Html.section [
                    prop.className "panel"
                    prop.children [
                        sectionTitle "◈" "Quests adrift — give them a day"
                        Html.div [
                            prop.className "quest-list"
                            prop.children [
                                for q in pool do
                                    Html.div [
                                        prop.className "pool-item"
                                        prop.children [
                                            Html.span [ prop.className "pool-title"; prop.text q.Title ]
                                            dayPicker weekOf q dispatch
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]
        ]
    ]
