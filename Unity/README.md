# Night Court — Unity First Playable

This folder is the Unity migration path for Night Court Questkeeper. The existing Fable/PWA app remains unchanged.

## Current milestone

- Engine-independent, compiled C# systems for XP, tasks, focus sessions, suggestions, saves, worlds, companions and crafting.
- Offline Travelling Scroll JSON export/import.
- Five Fae Cottage task nodes.
- An editor command that generates the Fae Cottage prototype scene.
- Separate core code that can be tested without launching Unity.

## Open in Unity

1. Install Unity Hub and Unity 2022.3 LTS.
2. In Unity Hub, choose **Add project from disk** and select this `Unity` folder.
3. Allow Unity to restore packages.
4. Choose **Night Court → Build Fae Cottage Prototype**.
5. Open `Assets/NightCourt/Scenes/FaeCottage.unity` and press Play.

The generated scene uses placeholder coloured rooms and text. Final character art, animation, touch movement and production UI are intentionally deferred until the interaction loop has been playtested.

## Run core tests without Unity

```sh
dotnet run --project Tests/NightCourt.Core.Tests.csproj
```

## Privacy

Save data is written to `Application.persistentDataPath/nightcourt-unity.json`. It is not uploaded. The Travelling Scroll is only created when the player explicitly exports it.

## Mobile notes

- Use IL2CPP and ARM64.
- Save on pause and quit.
- Replace `OnMouseUpAsButton` with the Input System interaction layer before production touch testing.
- Add a native file-share bridge for Travelling Scroll export/import on iOS and Android.
