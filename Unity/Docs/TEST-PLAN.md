# Test Plan

## Automated core tests

- XP overflow reaches the next level and retains excess XP.
- Task completion emits a reward once only.
- Focus timing uses UTC and survives background time.
- Travelling Scroll JSON restores progress and unlocked worlds.
- Suggested Next selects the smallest open task that fits available time.

## Unity EditMode checks

- Open the project without compiler errors.
- Run **Night Court → Build Fae Cottage Prototype**.
- Confirm a valid scene is saved at the documented path.
- Confirm every task node has a 2D collider and `TaskNodeBehaviour`.

## Mobile playtest

- Start a task within two taps.
- Use all interactions one-handed.
- Lock and unlock the phone during a focus session.
- Complete three tasks and confirm no celebration blocks the next action.
- Force-close and reopen; confirm progress restores.
- Import an invalid Travelling Scroll; confirm current save remains intact.
