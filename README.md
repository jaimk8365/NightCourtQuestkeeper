# 🌙 Night Court Questkeeper

An ADHD-friendly task-quest game. Your tasks are magical quests bestowed upon
you by the Night Court — completing them strengthens your powers.

Built with **F# + Fable 4 + Elmish + Feliz** (MVU architecture), Vite, and
localStorage persistence, shipped as an installable PWA.

## 🌐 Use it anywhere

The app is hosted on GitHub Pages:

**https://jaimk8365.github.io/NightCourtQuestkeeper/**

- **iPhone:** open the URL in Safari → Share → *Add to Home Screen*. It launches
  full-screen with the night theme, and works offline after the first visit to a session.
- **Any Mac (home or office):** open the URL in any browser; pin or add to Dock via
  Safari's *Add to Dock* if you like.

Each device keeps its own local data (nothing is stored on GitHub). To carry
progress between devices, use the **travelling scroll** (copy/paste export) or
the **cloud bond** (optional sync endpoint) on the Stars page.

### Deploying updates

After changing code on the Mac:

```sh
export PATH="$HOME/.dotnet:$PATH"
npm run deploy   # builds and force-pushes dist/ to the gh-pages branch
```

The site updates a minute or so later.

## Running the app locally

The .NET SDK lives at `~/.dotnet` (not on the default PATH):

```sh
export PATH="$HOME/.dotnet:$PATH"
cd ~/NightCourtQuestkeeper

npm start        # dev server with hot reload → http://localhost:5175
npm test         # run the 84-assertion self-test suite
npm run build    # production bundle → dist/
npm run preview  # serve the production bundle
```

`vite.config.js` sets `host: true`, so the dev server is reachable on the home
network. **On your iPhone:** open `http://<mac-ip>:5175` in Safari →
Share → *Add to Home Screen*. It launches full-screen with the night theme.

## Architecture

Strict MVU (Model–View–Update). Pure logic modules are shared verbatim with
the .NET test project; only `Storage.fs` downward touches the browser.

| Module | Responsibility |
|---|---|
| `Domain.fs` | All types (`Quest`, `Routine`, `DayPlan`, `WeekPlan`, `UserProfile`, `RewardState`, `AppData`) + day-key date helpers |
| `Catalog.fs` | Magical themes, moon phases, sigils, constellations, micro-copy, default routines |
| `Progression.fs` | XP, level curve, streaks, energy meter |
| `QuestEngine.fs` | Quest lifecycle + **Next Best Action** sorting |
| `RoutineEngine.fs` | Morning/Night rituals: daily completion log, bonuses, editing |
| `Planner.fs` | Week construction, intentions, lightest-day placement, **auto-suggest engine** |
| `Codec.fs` | Thoth.Json (auto) serialization — browser & .NET |
| `Storage.fs` | localStorage + optional cloud sync (`Sync.push` / `Sync.pull`) |
| `State.fs` | Elmish `Model`, `Msg`, `init`, `update` |
| `ViewShared.fs` … `ViewStars.fs` | Feliz views: Today, Week, Rituals, Stars |
| `Main.fs` | Onboarding gate, page router, `Program.mkProgram` |

## Game mechanics

- **XP**: Easy 15 / Medium 30 / Hard 60, plus a time bonus (5 XP per 15 min, capped at 20).
  Ritual steps earn 5 XP; a full ritual grants a +20 blessing.
- **Levels**: level *n* → *n+1* costs `80 + 40(n−1)` XP — fast early levels for early dopamine.
- **Streaks**: any completion feeds the day's streak; gaps reset gently to 1 (best streak is never lost).
- **Sigils**: nine cosmetic unlocks at levels 1–20 (see the Stars page).
- **Constellations**: every completed quest lights one star; constellations fill in sequence.
- **Energy meter**: each quest costs difficulty-weight + minutes/30 "spoons" against a
  daily budget of 10; days read *Gentle / Balanced / Heavy* to stop overwhelm before it happens.
- **Next Best Action**: overdue → due-today → planned-today → the rest, smallest-first
  within each band. One glowing card answers "what do I do now?".
- **Auto-suggest** (Sunday Ritual): unfinished quests + titles completed in ≥2 past
  weeks. Accepting a suggestion drops it on the **lightest-energy day** automatically —
  saying yes never requires another decision.

## Cross-device use & sync

- Each device keeps full local state (`localStorage`, key `nightcourt-data-v1`) — the app is 100% offline-capable.
- **Travelling scroll** (Stars page): copy/paste the JSON blob between devices.
- **Cloud bond** (Stars page): point at any URL that accepts `PUT`/`GET` of the JSON
  blob (a tiny Cloudflare Worker KV route, or a route on the Mac's home server).
  Every save pushes; "Pull from the cloud" fetches.

## Testing

`npm test` compiles the app's own logic modules for .NET and runs 84 assertions:
date math, XP/level curve monotonicity, streak semantics, energy bands, theme
determinism, next-best-action ordering, ritual toggling and bonuses, planner
suggestions, exact JSON round-trips, and a scripted multi-day simulation of the
update cycle.

## Design

See [STYLEGUIDE.md](STYLEGUIDE.md) for the Night Court UI system (tokens,
typography, motion, and voice).
