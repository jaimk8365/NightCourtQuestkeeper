/// The Stars screen: profile, level, sigils, constellation progress,
/// streaks, and the sync / export module.
module NightCourt.ViewStars

open Feliz
open NightCourt.Domain
open NightCourt.Catalog
open NightCourt.Progression
open NightCourt.QuestEngine
open NightCourt.State
open NightCourt.ViewShared

let view (model: Model) (dispatch: Msg -> unit) =
    let rewards = model.Data.Rewards
    let level = levelForXp rewards.Xp
    let stars = totalCompleted model.Data.Quests
    let constellation, lit, completedConstellations = constellationProgress stars
    let unlocked = sigilsUnlockedAt level

    Html.div [
        prop.className "page"
        prop.children [
            // profile summary
            Html.section [
                prop.className "panel"
                prop.children [
                    sectionTitle "✶" (match model.Data.Profile with
                                      | Some p -> sprintf "%s of the Night Court" p.Name
                                      | None -> "Keeper of the Night Court")
                    Html.div [
                        prop.className "stat-grid"
                        prop.children [
                            Html.div [ prop.className "stat"; prop.children [ Html.span [ prop.className "stat-num"; prop.text (string level) ]; Html.span [ prop.className "stat-label"; prop.text "level" ] ] ]
                            Html.div [ prop.className "stat"; prop.children [ Html.span [ prop.className "stat-num"; prop.text (string rewards.Xp) ]; Html.span [ prop.className "stat-label"; prop.text "total XP" ] ] ]
                            Html.div [ prop.className "stat"; prop.children [ Html.span [ prop.className "stat-num"; prop.text (string stars) ]; Html.span [ prop.className "stat-label"; prop.text "stars lit" ] ] ]
                            Html.div [ prop.className "stat"; prop.children [ Html.span [ prop.className "stat-num"; prop.text (string rewards.BestStreak) ]; Html.span [ prop.className "stat-label"; prop.text "best streak" ] ] ]
                        ]
                    ]
                ]
            ]

            // constellation progress
            Html.section [
                prop.className "panel"
                prop.children [
                    sectionTitle "🌌" (sprintf "Now weaving: %s" constellation.Name)
                    Html.div [
                        prop.className "constellation"
                        prop.children [
                            for i in 1 .. constellation.Stars do
                                Html.span [
                                    prop.className (if i <= lit then "cstar cstar-lit" else "cstar")
                                    prop.text "✦"
                                ]
                        ]
                    ]
                    Html.p [
                        prop.className "hint-copy"
                        prop.text (sprintf "%d of %d stars lit · %d constellation%s completed"
                                       lit constellation.Stars completedConstellations
                                       (if completedConstellations = 1 then "" else "s"))
                    ]
                ]
            ]

            // sigils
            Html.section [
                prop.className "panel"
                prop.children [
                    sectionTitle "❖" "Sigils earned"
                    Html.div [
                        prop.className "sigil-grid"
                        prop.children [
                            for s in sigils do
                                let owned = unlocked |> List.contains s
                                Html.div [
                                    prop.className (if owned then "sigil sigil-owned" else "sigil")
                                    prop.children [
                                        Html.span [ prop.className "sigil-glyph"; prop.text (if owned then s.Glyph else "?") ]
                                        Html.span [ prop.className "sigil-name"; prop.text (if owned then s.Name else sprintf "Unlocks at level %d" s.Level) ]
                                    ]
                                ]
                        ]
                    ]
                ]
            ]

            // cloud sync
            Html.section [
                prop.className "panel"
                prop.children [
                    sectionTitle "☁" "Cloud bond (optional)"
                    Html.p [
                        prop.className "hint-copy"
                        prop.text "Point at any URL that accepts GET and PUT of your data to carry your quests between devices. Leave empty to stay fully local."
                    ]
                    Html.div [
                        prop.className "intention-row"
                        prop.children [
                            Html.input [
                                prop.className "input"
                                prop.type' "url"
                                prop.placeholder "https://…"
                                prop.value model.SyncDraft
                                prop.onChange (SetSyncDraft >> dispatch)
                            ]
                            Html.button [
                                prop.className "btn btn-small"
                                prop.onClick (fun _ -> dispatch SaveSyncEndpoint)
                                prop.text "Bind"
                            ]
                        ]
                    ]
                    if model.Data.SyncEndpoint.IsSome then
                        Html.button [
                            prop.className "btn"
                            prop.onClick (fun _ -> dispatch PullFromCloud)
                            prop.text "Pull from the cloud ☁✦"
                        ]
                ]
            ]

            // export / import
            Html.section [
                prop.className "panel"
                prop.children [
                    sectionTitle "📜" "Travelling scroll"
                    Html.p [
                        prop.className "hint-copy"
                        prop.text "Copy this scroll to move your realm to another device, or paste one below to restore."
                    ]
                    Html.textarea [
                        prop.className "input input-area input-mono"
                        prop.readOnly true
                        prop.value (Codec.serialize model.Data)
                    ]
                    Html.textarea [
                        prop.className "input input-area input-mono"
                        prop.placeholder "Paste a scroll here to restore…"
                        prop.value model.ImportDraft
                        prop.onChange (SetImportDraft >> dispatch)
                    ]
                    Html.button [
                        prop.className "btn"
                        prop.disabled (model.ImportDraft.Trim() = "")
                        prop.onClick (fun _ -> dispatch ImportData)
                        prop.text "Unfurl scroll"
                    ]
                ]
            ]
        ]
    ]
