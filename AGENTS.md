# Game Prototype Context for AI Agents

Welcome, AI Agent. This document contains detailed context about the Unity First-Person Shooter (FPS) prototype project. Please refer to this guide to understand the architecture, core mechanics, and specific implementation details before making modifications.

## 1. Project Overview

This project is a 3D First-Person Shooter prototype built in Unity. It features advanced physics-based player movement, raycast shooting mechanics, NavMesh-based enemy AI with visual field-of-view (FOV) detection, a stealth sound-detection system, a CCTV hacking mechanic, a raycast-based interaction and inventory system, and level-progression doors with keypad authentication. It also includes new features like CRT screen effects, a floppy disk video playback system, a collectible flashlight, and multiple enemy AI variants.

**Project Path:** `d:\D4 Polines Jaya jaya jaya\Prototype\`
**Language:** C#
**Engine:** Unity

## 2. Core Systems Architecture

### A. Player Controller (`PlayerMov.cs`, `Sliding.cs`, `Gliding.cs`)
The player uses a custom physics-based Rigidbody controller, completely bypassing Unity's standard `CharacterController` component.
- **Movement States:** Walking, Sprinting, Crouching, Sliding, Gliding, and in-Air.
- **Physics Handling:** Applies drag on the ground, custom slope handling to prevent bouncing (`OnSlope`, `GetSlopeMoveDirection`), and limits maximum velocity.
- **3D Model Integration (The "RobotKyle" Quirk):** The player capsule has a 3D model (RobotKyle) attached as a visual representation. To prevent physics conflicts, `PlayerMov.cs` programmatically disables all `Collider`, `CharacterController`, and `MonoBehaviour` (except Animator) components on the RobotKyle model at startup. The model is also forced to the "Ignore Raycast" layer. The Animator's rotation is driven by the camera's orientation.
- **Sound Integration:** Various movement states feed noise values into the `SoundMeter` system. Sprinting generates the most noise, walking generates moderate noise, crouching generates minimal noise, and jumping/sliding/landing produce burst noise spikes.
- **Sliding & Gliding:** Handled by separate scripts hooked into the state machine. Sliding allows fast movement under obstacles while Gliding reduces fall speed and adds forward thrust.

### B. Combat & Weapon System (`WeaponShooting.cs`, `WeaponSwitcher.cs`, `WeaponRotation.cs`)
The combat system uses instantaneous hit-scan (Raycast) for the player and physical projectiles for the enemies.
- **Raycast Shooting:** Fires a ray from the center of the camera (`fpsCam`).
- **Damage & Hitboxes:** Checks for `BotHitbox` components. Implements a headshot multiplier if the hit collider has the tag "Head". Damage texts spawn using `ObjectPoolManager`.
- **Visual Feedback:** Supports Muzzle Flashes, Impact Effects, floating Damage Text (`DamageText.cs`), and camera recoil via `PlayerCam.cs`.
- **Aiming State:** When the shoot button is held, the script hides the player model (`RobotKyle`) and reveals the weapon model to simulate aiming down sights or bringing the weapon up.
- **Weapon Switching:** Managed by `WeaponSwitcher.cs`, allowing swapping between an array of assigned weapon GameObjects using number keys (1-9) or Mouse ScrollWheel. Updates the UI with weapon icons.
- **Weapon Rotation & Sway:** `WeaponRotation.cs` positions the weapon relative to the camera and applies procedural sway based on mouse movement.
- **Sound Integration:** Firing a weapon adds a significant noise spike to the `SoundMeter`, making gunfire a high-risk action in stealth scenarios.

### C. Enemy AI Variations
Enemies use `NavMeshAgent` for pathfinding and operate on state machine logic.
1. **`RandomAIFOV.cs` (Normal Enemy):**
   - **States:** Wandering (patrolling random points within a radius) and Chasing/Attacking.
   - **Field of View (FOV):** Uses a cone-shaped FOV (`Physics.OverlapSphere` + Angle checks + Line-of-sight Raycasts) to detect the player.
   - **Visual Searchlight:** A `Light` component (Spotlight) acts as a physical representation of the FOV.
   - **Sound Detection:** Subscribes to `SoundMeter.OnMaxSoundReached` event to bypass FOV and chase the player directly.
2. **`HardEnemyAI.cs` (Unstoppable Hard Enemy):**
   - **Behavior:** Constantly knows the player's location via `GameObject.FindGameObjectWithTag("Player")` bypassing FOV completely. Moves slowly but surely.
   - **Combat:** Shoots powerful projectiles at a lower fire rate but with high damage. Stops to shoot when within range.
3. **`BlindSoundAI.cs` (Blind Demon AI):**
   - **Detection:** Entirely blind. Relies exclusively on the SoundMeter threshold (`hearingThreshold`) or max sound events.
   - **Audio Integration:** Features dynamic ambient sound and chase sound logic (`stress ambient`) to heighten tension.
   - **Combat:** Uses animation triggers ("Shoot" or "Throw") to instantiate and fire high-damage physical projectiles.

### D. Health Systems (`PlayerHealth.cs`, `BotHealth.cs`, `BotHitbox.cs`)
- **Player Health:** Float-based health pool (`maxHealth = 100f`). Features UI text update and a visual damage indicator (a red Vignette UI Image that flashes and fades out using `Color.Lerp`). Uses `SceneManager.LoadScene` to restart on death.
- **Bot Health (`BotHealth.cs`):** Float-based health pool with `maxHealth`. Receives damage via `TakeDamage(float)`. When health reaches zero, calls `Die()` which permanently destroys the enemy GameObject (`Destroy(gameObject)`). Dead enemies do **not** respawn.
- **BotHitbox (`BotHitbox.cs`):** Attached to enemy colliders to act as a receiver for raycast damage, forwards damage to parent `BotHealth`. Can be marked as `isHead` to apply headshot multipliers.

### E. Interaction System (`PlayerInteract.cs`)
A unified raycast-based interaction system that handles all player-world interactions through a single script.
- **Raycast Detection:** Fires a ray from the center of the FPS camera every frame within `interactDistance`.
- **Hint UI:** Dynamically displays a contextual `TextMeshProUGUI` hint.
- **Supported Interactable Types (checked in order):**
    1. **`KeypadButton`** — Presses a button on the NavKeypad asset.
    2. **`PickableItem`** — Picks up standard items or Floppy Disks into the `PlayerInventory`.
    3. **`InspectableItem`** — Opens a full-screen inspect view of a document.
    4. **`InteractableCCTV`** — Triggers CCTV hacking mode, passing the terminal's index and `targetGroupID`.
    5. **`FlashlightItem`** — Collects the flashlight item and enables the `PlayerFlashlight` system.
    6. **`InteractableComputer`** — Uses a Floppy Disk from inventory to play a video.
- **Audio:** Plays sounds for inspect interactions.

### F. Inventory & Inspect System (`PlayerInventory.cs`, `PickableItem.cs`)
A singleton inventory system that manages item pickup, UI display, full-screen document inspect mode, and floppy disks.
- **Singleton Pattern:** `PlayerInventory.Instance` allows global access.
- **Standard Items:** Picking up an item shows the UI slot. Consumed when using the keypad.
- **Floppy Disks:** If a `PickableItem` has "floppy" or "disk" in its name, it stores a `VideoClip`. Displays in a separate UI slot. Consumed when interacting with an `InteractableComputer`.
- **Inspect Mode:** Pressing `I` toggles a full-screen overlay showing the item's image at native aspect ratio (`preserveAspect = true`).

### G. CCTV Hacking System (`CCTVManager.cs`, `InteractableCCTV.cs`)
A surveillance camera viewing system that lets the player remotely access and cycle through grouped CCTV cameras.
- **Singleton Pattern:** `CCTVManager.Instance`.
- **Camera Groups:** Cameras are assigned a `groupID` (e.g., "A"). Terminals (`InteractableCCTV`) only grant access to cameras with a matching `targetGroupID`.
- **Entry Flow (`EnterCCTVMode`):**
    1. Hides player UI.
    2. Disables player controls (`PlayerMov`, `PlayerCam`, `WeaponShooting`, `WeaponSwitcher`, `PlayerInteract`). Rigidbody set to kinematic.
    3. Activates the target CCTV camera and its dedicated spotlight (`cctvLight`).
    4. Adds `AudioListener` dynamically.
- **Camera Cycling:** Pressing `E` cycles to the next camera in the specific group.

### H. Sound Meter / Stealth System (`SoundMeter.cs`)
A noise-tracking stealth mechanic that alerts enemies when the player generates too much sound.
- **Noise Accumulation:** `AddSound(float amount)` increases noise level. Sprinting, jumping, and shooting generate high noise. Crouching generates minimal noise.
- **Noise Decay:** Decays back to zero over time when quiet.
- **Detection Trigger:** When `currentSound >= maxSound`, `TriggerDetection()` fires. Sets a cooldown during which the bar stays full. Invokes the static event `OnMaxSoundReached(Transform playerTransform)`, broadcasting position to all subscribed enemy AI.

### I. Environmental Systems
- **Door System (`SimpleDoor.cs`):** Animated door that slides down smoothly (`Vector3.MoveTowards`) when triggered by a keypad. Can activate the next level segment (`levelSelanjutnya.SetActive(true)`) for optimization.
- **Flashlight System (`PlayerFlashlight.cs`, `FlashlightItem.cs`):** A collectible flashlight. Once picked up, the player can toggle a Spotlight attached to the camera using the '4' key.
- **Computer Video System (`InteractableComputer.cs`):** A computer terminal that plays a specific `VideoClip` onto a Render Texture or `VideoPlayer` when the player uses a collected Floppy Disk on it.
- **Intro Fader (`IntroFader.cs`):** A simple CanvasGroup fader that fades out the UI (like a black screen or title) at the start of a scene.

### J. Object Pooling (`ObjectPoolManager.cs`)
A generic singleton object pooling system to reduce runtime `Instantiate`/`Destroy` overhead.
- **Singleton Pattern:** `ObjectPoolManager.Instance`.
- **Pool Structure:** Uses `Dictionary<string, Queue<GameObject>>`. Objects are pre-instantiated, deactivated, and parented under the manager.
- **Usage:** Call `SpawnFromPool(string poolName, position, rotation)` to fetch a recycled object. Used primarily for `DamageText` popups and effects.

### K. Dialogue & Subtitle System (`DialogueManager.cs`)
A global singleton system for playing voice-over audio and displaying synchronized on-screen subtitles.
- **Singleton Pattern:** `DialogueManager.Instance`.
- **UI Integration:** Displays subtitles using `TextMeshProUGUI` and automatically hides the UI panel after the audio clip finishes playing. Includes a configurable `dialogueVolume` control.
- **Item Integration:** `PickableItem`, `InspectableItem`, and `FlashlightItem` can trigger dialogue automatically upon pickup/interaction via `PlayerInteract`.
- **Event Triggering:** `DialogueTrigger.cs` provides a reusable `TriggerDialogue()` method designed to hook into generic Unity Events (e.g., Keypad's "On Access Granted").

## 3. Key Scripts Directory (`Assets/Scripts/`)

### Player Scripts
*   **`PlayerMov.cs`**: Handles all rigidbody movement, slope physics, input collection, state management, RobotKyle 3D model synchronization, and feeds noise to the SoundMeter.
*   **`PlayerCam.cs`**: Manages mouse look, camera rotation, scope zoom (FOV), and procedural recoil application.
*   **`PlayerHealth.cs`**: Manages float-based HP, UI fill bar, damage vignette effects, and scene reload on death.
*   **`PlayerInteract.cs`**: Unified raycast interaction system.
*   **`PlayerInventory.cs`**: Singleton inventory manager for standard items, floppy disks, and inspect mode.
*   **`PlayerFlashlight.cs`**: Controls toggling the player's spotlight after collecting the item.
*   **`Sliding.cs` / `Gliding.cs`**: Modular movement abilities that hook into the `PlayerMov` state machine.

### Combat & Weapon Scripts
*   **`WeaponShooting.cs`**: Handles raycast firing logic, damage delivery (headshots), visual effects (muzzle flash, impact, damage text pool), and adds noise to SoundMeter.
*   **`WeaponSwitcher.cs`**: Handles switching active weapons via keyboard (1-9) or scroll wheel, updating UI icons.
*   **`WeaponRotation.cs`**: Procedural weapon sway, position, and rotation based on camera movement (`LateUpdate`).

### Enemy & AI Scripts
*   **`RandomAIFOV.cs`**: Standard enemy AI with visual cone FOV, wandering, and sound-based detection override.
*   **`HardEnemyAI.cs`**: Slow, high-damage, unstoppable enemy that tracks the player without needing line-of-sight.
*   **`BlindSoundAI.cs`**: Blind demon that reacts strictly to SoundMeter levels and global sound events.
*   **`BotHealth.cs`**: Manages enemy HP with permanent death (`Destroy(gameObject)`).
*   **`BotHitbox.cs`**: Receives raycast damage and applies headshot multipliers.
*   **`EnemyProjectile.cs`**: Physical projectile behavior delivering damage on collision with player.

### Interaction & World Scripts
*   **`PickableItem.cs`**: Data component for pickup items (Name, Icon, VideoClip for Floppy).
*   **`InspectableItem.cs`**: Data component for inspect-only items (Name, Document Image).
*   **`InteractableCCTV.cs`**: Data component for CCTV terminals (Terminal Name, Group ID, Target Index).
*   **`InteractableComputer.cs`**: Computer terminal that receives a Floppy Disk to play video.
*   **`FlashlightItem.cs`**: Marker script for the collectible flashlight object in the world. Includes optional fields for pickup dialogues.
*   **`DialogueTrigger.cs`**: Reusable component that exposes a `TriggerDialogue()` method for Unity Events to play a specific voice clip and subtitle.
*   **`SimpleDoor.cs`**: Animated door that slides down when triggered.
*   **`IntroFader.cs`**: Fades a CanvasGroup (like a black screen) at scene start.

### System & Manager Scripts
*   **`CCTVManager.cs`**: Singleton managing CCTV hacking mode, camera groups (`groupID`), control disabling, and audio.
*   **`SoundMeter.cs`**: Singleton noise-tracking stealth meter with decay and detection events.
*   **`ObjectPoolManager.cs`**: Generic singleton object pooling with dictionary-based named pools.
*   **`DialogueManager.cs`**: Singleton manager handling voice-over audio playback and subtitle UI (`TextMeshProUGUI`) synchronization.
*   **`MainMenuManager.cs`**: Main menu scene manager with Play and Exit functions.
*   **`CameraMov.cs`**: Syncs a camera holder to a target position.
*   **`VideoThumbnailHider.cs`**: Hides a thumbnail overlay once a VideoPlayer starts playing.
*   **`DamageText.cs`**: Controls floating damage numbers with pooling and billboard logic.

## 4. Important Implementation Quirks to Remember

1.  **Rigidbody Player:** Do not use `CharacterController`. Movement relies entirely on a non-kinematic `Rigidbody` with `freezeRotation` enabled.
2.  **RobotKyle Model Hack:** `PlayerMov.Start()` aggressively disables colliders, character controllers, and Monobehaviours on the player model to prevent conflicts.
3.  **Enemy Projectiles are Physical:** Enemies shoot physical Rigidbodies, while the player uses Raycasts. Enemy scripts manually ignore collision between their own colliders and their projectiles (`Physics.IgnoreCollision`).
4.  **Tagging is crucial:** Headshots rely on the `"Head"` tag. The player must be tagged `"Player"` for `HardEnemyAI` and global search functions to work.
5.  **Enemies do NOT respawn.** `BotHealth.Die()` calls `Destroy(gameObject)` permanently.
6.  **CCTV disables player via script name strings.** `CCTVManager.DisablePlayerControls()` identifies scripts by `GetType().Name` string comparison (e.g., `"PlayerCam"`, `"WeaponShooting"`). Do not rename these scripts without updating CCTVManager.
7.  **Singleton dependencies.** `CCTVManager`, `SoundMeter`, `PlayerInventory`, `ObjectPoolManager` all rely on the Singleton pattern (`Instance`). Ensure exactly one exists in the scene.
8.  **PlayerInteract priority.** Interaction checks happen in a specific `if-else if` order (`KeypadButton` > `PickableItem` > `InspectableItem` > `InteractableCCTV` > `FlashlightItem` > `InteractableComputer`).
9.  **NavKeypad Integration.** Uses the third-party `NavKeypad` asset. Keypad's "On Access Granted" event should call both `SimpleDoor.BukaPintu()` and `PlayerInventory.ConsumeItem()`.
10. **Floppy Disk Logic:** `PickableItem` is treated as a Floppy Disk if its `itemName` contains "floppy" or "disk" (case-insensitive).
11. **Event-driven AI:** `SoundMeter.OnMaxSoundReached` is a static C# event. AI scripts subscribe in `OnEnable` and unsubscribe in `OnDisable`.

## 5. System Integration Maps

### A. General Gameplay & Stealth
```
PlayerMov ──(noise)──► SoundMeter ──(OnMaxSoundReached)──► RandomAIFOV / BlindSoundAI (force chase)
    │                       │
    │                       └──► UI Fill Bar
    │
WeaponShooting ──(noise)──► SoundMeter
    │
    └──(raycast)──► BotHitbox ──► BotHealth ──(Die)──► Destroy
```

### B. Interactions & Inventory
```
PlayerInteract ──► PickableItem ("Floppy") ──► PlayerInventory ──(ConsumeFloppy)──► InteractableComputer
    │                                              │                                       │
    │              PickableItem ("Keycard") ───────┤ (ConsumeItem)                         ▼
    │                                              ▼                                 videoPlayer.Play()
    │                                          KeypadButton
    │                                              │
    │              InspectableItem ──────────► (ShowInspect)
    │                                              │
    └──► InteractableCCTV ──► CCTVManager          ▼
                                              SimpleDoor.BukaPintu()
```

### C. CCTV State Overrides
```
InteractableCCTV ──(EnterCCTVMode)──► CCTVManager
                                          │
                                          ├── Disable PlayerMov, PlayerCam, WeaponShooting, WeaponSwitcher, PlayerInteract
                                          ├── Set Rigidbody.isKinematic = true
                                          ├── Hide Player UI
                                          └── Activate Camera group based on targetGroupID
```

## 6. Potential Areas for Future Work / Polish

*   **Animation Blending:** Smoothing out the transitions between walking, sprinting, and the "FreeFall" state in `PlayerMov.cs`.
*   **Full Object Pool Integration:** Currently only `DamageText` uses the pool reliably. Converting `EnemyProjectile`, muzzle flashes, and impact effects to use the pool would improve performance.
*   **Audio Manager:** Centralizing audio playback instead of relying solely on localized `AudioSource.PlayOneShot` scattered across scripts.
*   **Multi-Item Inventory:** Currently `PlayerInventory` only holds one key item and one floppy disk at a time. Expanding to a multi-slot inventory could be valuable.
*   **Save/Load System:** No persistence layer exists. Player progress, door states, and inventory are lost on scene reload.
*   **Pause Menu:** No in-game pause menu exists yet — only the main menu scene has UI navigation.
*   **CCTV Camera Movement:** Implementing slow panning or player-controlled rotation while in CCTV mode.

---
*Generated by Antigravity.*
