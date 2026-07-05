# Night Court Questkeeper — UI Style Guide

*Soft ACOTAR Night Court: velvet darkness, starlight, quiet magic. Calming
first, dopamine second, never loud.*

## Palette (CSS custom properties in `styles.css`)

| Token | Value | Use |
|---|---|---|
| `--night-deep` | `#0b0716` | App background |
| `--night-velvet` | `#150e2b` | Panels |
| `--night-raised` | `#1f1540` | Cards, inputs |
| `--night-edge` | `#2f2159` | Borders |
| `--starlight` | `#f4ecff` | Primary text |
| `--moonsilver` | `#b4a5d6` | Secondary text |
| `--amethyst` | `#8b5cf6` | Primary actions |
| `--amethyst-soft` | `#cdb6ff` | Accents, glows |
| `--dawn-rose` | `#f0b6e8` | Celebration accent |
| `--ember` | `#ffc9a3` | Warm attention (never red — "past due" is *still worthy*) |

Magical-school accents: Starweaving `#cdb6ff` ✦ · Moonbinding `#9fc2ff` ☾ ·
Shadowstep `#b39ddb` ◈ · Dreamtending `#f0b6e8` ❋ · Emberwarding `#ffc9a3` ✵ ·
Mistcalling `#a3e8e0` ≈ · Nightblooming `#c9a3ff` ✿

## Typography

System rounded stack (`SF Pro Rounded` on iOS). Base 16px / 1.45.
Headings 14.5–21px, weight 650–800. Micro-copy is italic `--moonsilver`.
Labels are small-caps-style: 10–12px, letter-spaced, uppercase.

## Shape & depth

- Radii: panels 18px, cards/inputs 12px, buttons/chips pill.
- Glow instead of shadow: `0 0 18px rgba(139,92,246,.35)`. The hero card,
  level orb, FAB, and active nav glyph glow; nothing else competes.
- Panels are translucent over a fixed starfield (60 CSS-animated twinkling stars).

## Motion

Gentle and brief — nothing bounces, nothing shakes:
- `twinkle` 3.5–4.5s ease-in-out (stars, lit constellation stars)
- `rise` 0.28s cubic-bezier(0.22,1,0.36,1) (sheets, toasts)
- `bloom` 0.4s (celebration card) with falling ✦ sparkles
- XP bar eases over 0.6s; complete-buttons scale to 0.9 on press

## ADHD-friendly rules baked in

1. **One glowing thing**: the Next Best Action is the only hero on screen.
2. **One-tap actions**: complete = tap the rune; ritual step = tap the row.
3. **Smallest-first sorting** makes starting cheap.
4. **No red, no guilt**: deletion is "release", overdue is "past due — still worthy",
   streak resets keep your best.
5. **Energy meter** warns *before* overplanning, in words not numbers.
6. **Decisions removed**: accepted suggestions self-place on the lightest day.
7. Touch targets ≥ 44px; bottom nav within thumb reach; safe-area insets respected.

## Voice

Whimsical in UI copy only, never in mechanics or docs. The Court *suggests*,
*bestows*, *weaves*; it never demands. Examples: "Bestow Quest ✨",
"An open sky." (empty day), "Rest, or bestow a new one — both are victories."
