/// Shared UI components: starfield, header, navigation, quest cards,
/// energy meter, celebration overlay, toast.
module NightCourt.ViewShared

open Feliz
open NightCourt.Domain
open NightCourt.Catalog
open NightCourt.Progression
open NightCourt.State

// ---------------------------------------------------------------- labels

let difficultyLabel =
    function
    | Easy -> "Easy"
    | Medium -> "Medium"
    | Hard -> "Hard"

let categoryLabel =
    function
    | Home -> "Home"
    | Work -> "Work"
    | Personal -> "Personal"
    | Errands -> "Errands"
    | Wellbeing -> "Wellbeing"

let allDifficulties = [ Easy; Medium; Hard ]
let allCategories = [ Home; Work; Personal; Errands; Wellbeing ]

// ---------------------------------------------------------------- starfield

/// Deterministic pseudo-random starfield rendered once behind everything.
let starfield =
    Html.div [
        prop.className "starfield"
        prop.children [
            for i in 0 .. 59 do
                let x = (i * 37 + 11) % 100
                let y = (i * 53 + 7) % 100
                let size = 1 + (i % 3)
                let delay = (i * 13) % 70
                Html.span [
                    prop.className "star"
                    prop.style [
                        style.left (length.percent x)
                        style.top (length.percent y)
                        style.width size
                        style.height size
                        style.custom ("animationDelay", sprintf "%.1fs" (float delay / 10.0))
                    ]
                ]
        ]
    ]

// ---------------------------------------------------------------- header

let header (model: Model) =
    let rewards = model.Data.Rewards
    let level = levelForXp rewards.Xp
    let intoLevel, needed = levelProgress rewards.Xp
    let moonGlyph, _ = moonPhaseFor (Dates.mondayOf model.Today)
    let name = model.Data.Profile |> Option.map (fun p -> p.Name) |> Option.defaultValue ""
    Html.header [
        prop.className "header"
        prop.children [
            Html.div [
                prop.className "header-row"
                prop.children [
                    Html.span [ prop.className "header-moon"; prop.text moonGlyph ]
                    Html.div [
                        prop.className "header-title"
                        prop.children [
                            Html.h1 [ prop.text (if name = "" then "Night Court Questkeeper" else sprintf "Questkeeper %s" name) ]
                            Html.p [ prop.className "header-sub"; prop.text (encouragementFor model.Today) ]
                        ]
                    ]
                    Html.div [
                        prop.className "level-orb"
                        prop.children [
                            Html.span [ prop.className "level-orb-num"; prop.text (string level) ]
                            Html.span [ prop.className "level-orb-label"; prop.text "lvl" ]
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className "xp-bar"
                prop.children [
                    Html.div [
                        prop.className "xp-bar-fill"
                        prop.style [ style.width (length.percent (int (100.0 * float intoLevel / float needed))) ]
                    ]
                ]
            ]
            Html.div [
                prop.className "header-meta"
                prop.children [
                    Html.span [ prop.text (sprintf "%d / %d XP to level %d" intoLevel needed (level + 1)) ]
                    if rewards.CurrentStreak > 0 && streakAlive model.Today rewards then
                        Html.span [ prop.className "streak"; prop.text (sprintf "✨ %d-day streak" rewards.CurrentStreak) ]
                ]
            ]
        ]
    ]

// ---------------------------------------------------------------- navigation

let navBar (model: Model) (dispatch: Msg -> unit) =
    let tab (page: Page) (glyph: string) (label: string) =
        Html.button [
            prop.className (if model.Page = page then "nav-tab nav-tab-active" else "nav-tab")
            prop.onClick (fun _ -> dispatch (SetPage page))
            prop.children [
                Html.span [ prop.className "nav-glyph"; prop.text glyph ]
                Html.span [ prop.className "nav-label"; prop.text label ]
            ]
        ]
    Html.nav [
        prop.className "nav-bar"
        prop.children [
            tab TodayPage "✦" "Today"
            tab WeekPage "☾" "Week"
            tab RitualsPage "❋" "Rituals"
            tab StarsPage "✶" "Stars"
        ]
    ]

// ---------------------------------------------------------------- quest card

/// One-tap quest card. `hero` styles the Next Best Action.
let questCard (today: string) (hero: bool) (q: Quest) (dispatch: Msg -> unit) =
    let info = themeInfo q.Theme
    Html.div [
        prop.className (if hero then "quest-card quest-card-hero" else "quest-card")
        prop.children [
            Html.button [
                prop.className "quest-complete"
                prop.ariaLabel (sprintf "Complete %s" q.Title)
                prop.onClick (fun _ -> dispatch (CompleteQuest q.Id))
                prop.children [
                    Html.span [ prop.className "quest-rune"; prop.style [ style.color info.Color ]; prop.text info.Glyph ]
                ]
            ]
            Html.div [
                prop.className "quest-body"
                prop.children [
                    Html.div [ prop.className "quest-title"; prop.text q.Title ]
                    match q.Description with
                    | Some d -> Html.div [ prop.className "quest-desc"; prop.text d ]
                    | None -> Html.none
                    Html.div [
                        prop.className "quest-meta"
                        prop.children [
                            Html.span [ prop.className "chip"; prop.text info.Name ]
                            Html.span [ prop.className "chip"; prop.text (difficultyLabel q.Difficulty) ]
                            Html.span [ prop.className "chip"; prop.text (sprintf "%d min" q.Minutes) ]
                            Html.span [ prop.className "chip chip-dim"; prop.text (categoryLabel q.Category) ]
                            match q.Deadline with
                            | Some d when d < today ->
                                Html.span [ prop.className "chip chip-overdue"; prop.text "past due — still worthy" ]
                            | Some d when d = today ->
                                Html.span [ prop.className "chip chip-due"; prop.text "due today" ]
                            | Some d ->
                                Html.span [ prop.className "chip chip-dim"; prop.text (sprintf "due %s" (Dates.shortLabel d)) ]
                            | None -> Html.none
                        ]
                    ]
                ]
            ]
            Html.button [
                prop.className "quest-release"
                prop.ariaLabel "Release quest"
                prop.title "Release this quest back to the mist"
                prop.onClick (fun _ -> dispatch (ReleaseQuest q.Id))
                prop.text "×"
            ]
        ]
    ]

// ---------------------------------------------------------------- energy meter

let energyMeter (load: float) =
    let band = energyBand load
    let cls =
        match band with
        | Gentle -> "energy-fill energy-gentle"
        | Balanced -> "energy-fill energy-balanced"
        | Heavy -> "energy-fill energy-heavy"
    Html.div [
        prop.className "energy"
        prop.children [
            Html.div [
                prop.className "energy-track"
                prop.children [
                    Html.div [
                        prop.className cls
                        prop.style [ style.width (length.percent (min 100 (int (load * 100.0)))) ]
                    ]
                ]
            ]
            Html.span [ prop.className "energy-label"; prop.text (energyLabel band) ]
        ]
    ]

// ---------------------------------------------------------------- overlays

let celebrationOverlay (c: Celebration) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "celebrate-overlay"
        prop.onClick (fun _ -> dispatch DismissCelebration)
        prop.children [
            Html.div [
                prop.className "celebrate-card"
                prop.children [
                    Html.div [
                        prop.className "celebrate-sparkles"
                        prop.children [
                            for i in 0 .. 11 do
                                Html.span [
                                    prop.className "sparkle"
                                    prop.style [
                                        style.left (length.percent ((i * 29 + 5) % 100))
                                        style.custom ("animationDelay", sprintf "%.1fs" (float (i * 2) / 10.0))
                                    ]
                                    prop.text "✦"
                                ]
                        ]
                    ]
                    Html.h2 [ prop.text c.Heading ]
                    Html.div [ prop.className "celebrate-xp"; prop.text (sprintf "+%d XP" c.Xp) ]
                    for line in c.Lines do
                        Html.p [ prop.className "celebrate-line"; prop.text line ]
                    Html.button [
                        prop.className "btn btn-primary"
                        prop.onClick (fun _ -> dispatch DismissCelebration)
                        prop.text "Onward ✨"
                    ]
                ]
            ]
        ]
    ]

let toast (message: string) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "toast"
        prop.onClick (fun _ -> dispatch DismissToast)
        prop.text message
    ]

let sectionTitle (glyph: string) (title: string) =
    Html.h2 [
        prop.className "section-title"
        prop.children [
            Html.span [ prop.className "section-glyph"; prop.text glyph ]
            Html.span [ prop.text title ]
        ]
    ]
