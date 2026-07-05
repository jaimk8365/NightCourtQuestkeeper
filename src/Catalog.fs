/// Static magical content: themes, moon phases, sigils, constellations,
/// encouraging micro-copy, and the default routines seeded on first run.
module NightCourt.Catalog

open System
open NightCourt.Domain

// ---------------------------------------------------------------- themes

type ThemeInfo =
    { Theme: MagicalTheme
      Name: string
      Glyph: string
      /// CSS accent colour for the quest card rune.
      Color: string }

let themes: ThemeInfo list =
    [ { Theme = Starweaving;  Name = "Starweaving";  Glyph = "✦"; Color = "#cdb6ff" }
      { Theme = Moonbinding;  Name = "Moonbinding";  Glyph = "☾"; Color = "#9fc2ff" }
      { Theme = Shadowstep;   Name = "Shadowstep";   Glyph = "◈"; Color = "#b39ddb" }
      { Theme = Dreamtending; Name = "Dreamtending"; Glyph = "❋"; Color = "#f0b6e8" }
      { Theme = Emberwarding; Name = "Emberwarding"; Glyph = "✵"; Color = "#ffc9a3" }
      { Theme = Mistcalling;  Name = "Mistcalling";  Glyph = "≈"; Color = "#a3e8e0" }
      { Theme = Nightblooming; Name = "Nightblooming"; Glyph = "✿"; Color = "#c9a3ff" } ]

let themeInfo (t: MagicalTheme) : ThemeInfo =
    themes |> List.find (fun i -> i.Theme = t)

/// Deterministic own-rolled string hash — String.GetHashCode differs between
/// .NET and Fable, and the tests must agree with the app.
let private stableHash (s: string) : int =
    let mutable h = 7
    for c in s do
        h <- (h * 31 + int c) % 1000003
    abs h

/// Every quest gets a magical school assigned from its title — stable, so the
/// same chore always carries the same magic.
let assignTheme (title: string) : MagicalTheme =
    let i = stableHash (title.Trim().ToLowerInvariant()) % themes.Length
    themes.[i].Theme

// ---------------------------------------------------------------- moon phases

let moonPhases =
    [ "🌑", "New Moon — a week for quiet beginnings"
      "🌒", "Waxing Crescent — small steps gather light"
      "🌓", "First Quarter — momentum is on your side"
      "🌔", "Waxing Gibbous — your power is swelling"
      "🌕", "Full Moon — a week of bright magic"
      "🌖", "Waning Gibbous — harvest what you began"
      "🌗", "Last Quarter — release what no longer serves"
      "🌘", "Waning Crescent — rest is also magic" ]

/// The moon phase that themes a given week (cycles deterministically).
let moonPhaseFor (weekOf: string) : string * string =
    let idx = (Dates.daysSinceEpoch weekOf / 7) % moonPhases.Length
    moonPhases.[abs idx]

// ---------------------------------------------------------------- sigils

/// Cosmetic unlocks earned by levelling up.
type Sigil =
    { Level: int
      Name: string
      Glyph: string }

let sigils: Sigil list =
    [ { Level = 1;  Name = "Sigil of the First Star";   Glyph = "✧" }
      { Level = 2;  Name = "Rune of Gentle Beginnings"; Glyph = "ᚱ" }
      { Level = 3;  Name = "Mark of the Crescent";      Glyph = "☾" }
      { Level = 5;  Name = "Ward of Quiet Courage";     Glyph = "❖" }
      { Level = 7;  Name = "Seal of the Twin Moons";    Glyph = "☽☾" }
      { Level = 10; Name = "Crest of the Night Court";  Glyph = "♆" }
      { Level = 13; Name = "Glyph of Woven Starlight";  Glyph = "✺" }
      { Level = 16; Name = "Emblem of the Dreamweaver"; Glyph = "❈" }
      { Level = 20; Name = "Crown of the Starkeeper";   Glyph = "✶" } ]

let sigilsUnlockedAt (level: int) : Sigil list =
    sigils |> List.filter (fun s -> s.Level <= level)

/// The sigil newly earned by moving from oldLevel to newLevel, if any.
let newlyUnlockedSigil (oldLevel: int) (newLevel: int) : Sigil option =
    sigils |> List.tryFind (fun s -> s.Level > oldLevel && s.Level <= newLevel)

// ---------------------------------------------------------------- constellations

/// Each completed quest lights one star. Stars fill constellations in order.
type Constellation =
    { Name: string
      Stars: int }

let constellations: Constellation list =
    [ { Name = "The Ember Moth";     Stars = 5 }
      { Name = "The Night Hound";    Stars = 7 }
      { Name = "The Silver Loom";    Stars = 9 }
      { Name = "The Dreaming Doe";   Stars = 11 }
      { Name = "The Star Serpent";   Stars = 13 }
      { Name = "The Moon Harp";      Stars = 15 }
      { Name = "The Court Ascendant"; Stars = 20 } ]

/// Where totalStars falls: (constellation, stars lit within it, fully-completed count).
let constellationProgress (totalStars: int) : Constellation * int * int =
    let rec go remaining completed cs =
        match cs with
        | [] ->
            // Beyond the last constellation the sky simply keeps filling.
            let last = List.last constellations
            last, last.Stars, completed
        | (c: Constellation) :: rest ->
            if remaining < c.Stars then c, remaining, completed
            else go (remaining - c.Stars) (completed + 1) rest
    go (max 0 totalStars) 0 constellations

// ---------------------------------------------------------------- micro-copy

let private pick (xs: string list) (seed: int) = xs.[abs seed % xs.Length]

let encouragements =
    [ "A gentle nudge from the stars…"
      "The Night Court believes in you."
      "Small magic is still magic."
      "One quest at a time — the sky fills itself star by star."
      "Even the moon waxes slowly."
      "Your power grows with every small victory." ]

let completionCheers =
    [ "The stars flare in celebration!"
      "Another thread woven into the night sky."
      "The Court applauds softly from the shadows."
      "Your magic deepens."
      "A star ignites in your name." ]

let encouragementFor (dayKey: string) = pick encouragements (Dates.daysSinceEpoch dayKey)
let cheerFor (n: int) = pick completionCheers n

// ---------------------------------------------------------------- defaults

let private step (label: string) (minutes: int) : RoutineStep =
    { Id = Guid.NewGuid(); Label = label; Minutes = minutes }

/// Seeded on first run; fully editable in the Rituals screen.
let defaultRoutines () : Routine list =
    [ { Time = Morning
        Steps =
          [ step "Drink a glass of water" 2
            step "Take morning meds / vitamins" 2
            step "Get dressed" 10
            step "Eat something" 15 ] }
      { Time = Evening
        Steps =
          [ step "Tidy one small thing" 5
            step "Set out tomorrow's essentials" 5
            step "Screens away, lights low" 5 ] } ]

let initialData () : AppData =
    { Version = 1
      Profile = None
      Quests = []
      Routines = defaultRoutines ()
      RitualLog = Map.empty
      WeekPlans = []
      Rewards = RewardState.Initial
      SyncEndpoint = None }
