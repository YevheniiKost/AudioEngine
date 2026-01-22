# Audio Engine for Unity

## Table of Contents

* [Overview](#overview)
* [Key Features](#key-features)
* [Content State](#content-state)
* [Installation & Setup](#installation--setup)
* [Sound Playback](#sound-playback)

    * [AudioEvent](#audioevent)
    * [Playing Sounds via Code](#playing-sounds-via-code)
    * [Positional and Following Audio](#positional-and-following-audio)
    * [Playback Control](#playback-control)
* [Music System](#music-system)

    * [MusicTrack](#musictrack)
    * [MusicPlayerComponent](#musicplayercomponent)
    * [Playing Music via Code](#playing-music-via-code)
* [Design Principles](#design-principles)
* [License](#license)

## Overview

**Audio Engine** is a lightweight and extensible audio framework for Unity that provides a structured, data-driven approach to sound effects and music playback.

It is designed to remove ad‑hoc audio logic from gameplay code and centralize audio behavior in ScriptableObjects, while still allowing precise runtime control.

Supported features:

* 2D audio (UI, menus, 2D games)
* 3D positional audio
* 3D audio that follows a transform
* Music playback with fading and transitions

---

## Key Features

* ScriptableObject–based configuration for sounds and music
* Randomized clip selection per sound event
* Per‑sound control over volume, pitch, spatialization, and mixer routing
* Runtime control via playback handles
* Dedicated music system with fading and track management
* Optional component‑based playback for scene‑driven audio

---

## Content State

This project is **feature‑complete for core audio playback** and considered **stable for use in personal and small‑to‑medium Unity projects**.

### Implemented

* AudioEvent system for sound effects
* 2D and 3D sound playback
* Transform‑following 3D audio
* Per‑play call parameter overrides (`PlayParams`)
* Runtime sound control via `PlayHandle`
* MusicTrack system with fading
* Music playback via code and `MusicPlayerComponent`
* AudioMixer integration

### Not Implemented / Out of Scope

* Audio streaming from external sources
* Timeline integration
* Audio occlusion or obstruction
* Automatic voice prioritization
* Editor tooling beyond basic asset creation

### Stability Notes

* API surface is stable but not versioned
* Backward compatibility is **not guaranteed** between major internal refactors
* No automated tests are currently included

---

## Installation & Setup

1. Add **AudioEngine** to your project as a Git submodule.
2. Create a `SoundSettings` asset:

    * Copy one from `AudioEngine/Samples`, **or**
    * Create a new one via:

      `Create → YeKostenko → AudioEngine → Sound Settings`
3. Place the `SoundSettings` asset inside a `Resources` folder.
4. Create an `AudioMixerController`:

    * A root **Master** mixer
    * Child mixers as needed (e.g. `SFX`, `UI`, `Ambience`, `Voice`)
5. Assign mixer groups to the corresponding fields in `SoundSettings` and adjust parameters as required.

---

## Sound Playback

### AudioEvent

All sound effects are defined using the `AudioEvent` ScriptableObject:

`Create → YeKostenko → AudioEngine → Audio Event`

Key properties:

* **Clips** – List of audio clips; one is selected randomly per playback
* **Spatial Settings** – Defines whether the sound is 2D or 3D
* **Spatial Blend** – Values greater than `0` are treated as 3D audio

Each distinct sound in the game should have its own `AudioEvent` to allow independent tuning and reuse.

---

### Playing Sounds via Code

The main API is exposed through `YeKostenko.AudioEngine.Sound`.

Example:

```csharp
using UnityEngine;
using YeKostenko.AudioEngine;

public class PlayerModel : MonoBehaviour
{
    [SerializeField] private AudioEvent jumpSound;
    [SerializeField] private AudioEvent shootSound;

    public void PlayJump()
    {
        Sound.Play(jumpSound);
    }

    public void PlayShoot()
    {
        Sound.Play(shootSound, new PlayParams
        {
            VolumeMul = 0.8f
        });
    }
}
```

`PlayParams` allows per‑call overrides without modifying the underlying `AudioEvent` asset.

---

### Positional and Following Audio

* `PlayAt` — plays a sound at a fixed world position
* `PlayFollow` — attaches the sound to a transform and updates its position

Example:

```csharp
using UnityEngine;
using YeKostenko.AudioEngine;

public class BombController : MonoBehaviour
{
    [SerializeField] private AudioEvent tickSound;
    [SerializeField] private AudioEvent explosionSound;
    [SerializeField] private Transform bombTransform;

    public void PlaceBomb()
    {
        Sound.PlayFollow(tickSound, bombTransform);
    }

    public void Explode()
    {
        Sound.PlayAt(explosionSound, bombTransform.position);
    }
}
```

---

### Playback Control

All sound playback methods return a `PlayHandle`.

The handle allows runtime control of the playing sound:

* `Stop()`
* `Pause()`
* `SetVolume(float)`
* `SetPitch(float)`

This avoids reliance on global state or scene‑bound references.

---

## Music System

### MusicTrack

Music is defined using the `MusicTrack` ScriptableObject:

`Create → YeKostenko → AudioEngine → Music Track`

Available settings include:

* Base volume
* Fade‑in and fade‑out durations
* Looping behavior

---

### MusicPlayerComponent

For scene‑driven music playback:

1. Create a GameObject
2. Add `MusicPlayerComponent`
3. Assign one or more `MusicTrack` assets

The component provides basic controls:

* Play
* Pause
* Stop
* Next Track

---

### Playing Music via Code

```csharp
using UnityEngine;
using YeKostenko.AudioEngine;

public class MusicController : MonoBehaviour
{
    [SerializeField] private MusicTrack musicTrack;

    public void PlayMusic()
    {
        Sound.Music(musicTrack);
    }
}
```

Optional `MusicParams` can be passed to override track settings at runtime.

The `Music` method returns a `MusicHandle`, which allows:

* `Stop()`
* `Pause()`
* `SetVolume(float)`
* `SetPitch(float)`

---

## Design Principles

* Audio behavior is **data‑driven**, not scene‑driven
* ScriptableObjects are the single source of truth
* Runtime control is explicit and handle‑based
* No hidden global state or implicit lifecycle management

---

## License

This project is licensed under the MIT License.

You are free to use, modify, merge, publish, distribute, sublicense, and/or sell copies of the software, provided that the original copyright notice and this permission notice are included in all copies or substantial portions of the software.

The software is provided "as is", without warranty of any kind, express or implied.
