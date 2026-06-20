# Game Prototype Context for AI Agents

Welcome, AI Agent. This document contains detailed context about the Unity First-Person Shooter (FPS) prototype project. Please refer to this guide to understand the architecture, core mechanics, and specific implementation details before making modifications.

## 1. Project Overview

This project is a 3D First-Person Shooter prototype built in Unity. It features advanced physics-based player movement, raycast shooting mechanics, NavMesh-based enemy AI with visual field-of-view (FOV) detection, a stealth sound-detection system, a CCTV hacking mechanic, a raycast-based interaction and inventory system, and level-progression doors with keypad authentication.

**Project Path:** `d:\D4 Polines Jaya jaya jaya\Prototype\`
**Language:** C#
**Engine:** Unity

## 2. Core Systems Architecture

### A. Player Controller (`PlayerMov.cs`)
The player uses a custom physics-based Rigidbody controller, completely bypassing Unity's standard `CharacterController` component.
- **Movement States:** Walking, Sprinting, Crouching, Sliding, Gliding, and in-Air.
- **Physics Handling:** Applies drag on the ground, custom slope handling to prevent bouncing (`OnSlope`, `GetSlopeMoveDirection`), and limits maximum velocity.
- **3D Model Integration (The "RobotKyle" Quirk):** The player capsule has a 3D model (RobotKyle) attached as a visual representation. To prevent physics conflicts, `PlayerMov.cs` programmatically disables all `Collider`, `CharacterController`, and `MonoBehaviour` (except Animator) components on the RobotKyle model at startup. The model is also forced to the "Ignore Raycast" layer. The Animator's rotation is driven by the camera's orientation.
- **Sound Integration:** Various movement states feed noise values into the `SoundMeter` system. Sprinting generates the most noise, walking generates moderate noise, crouching generates minimal noise, and jumping/sliding/landing produce burst noise spikes.

### B. Combat & Weapon System (`WeaponShooting.cs`, `WeaponSwitcher.cs`)
The combat system uses instantaneous hit-scan (Raycast) for the player and physical projectiles for the enemies.
- **Raycast Shooting:** Fires a ray from the center of the camera (`fpsCam`).
- **Damage & Hitboxes:** Checks for `BotHitbox` components. Implements a headshot multiplier if the hit collider has the tag "Head" or contains "head" in its name.
- **Visual Feedback:** Supports Muzzle Flashes, Impact Effects, floating Damage Text (`DamageText.cs`), and camera recoil via `PlayerCam.cs`.
- **Aiming State:** When the shoot button is held, the script hides the player model (`RobotKyle`) and reveals the weapon model to simulate aiming down sights or bringing the weapon up.
- **Weapon Switching:** Managed by `WeaponSwitcher.cs`, allowing swapping between an array of assigned weapon GameObjects using number keys (1-9).
- **Sound Integration:** Firing a weapon adds a significant noise spike to the `SoundMeter`, making gunfire a high-risk action in stealth scenarios.

### C. Enemy AI (`RandomAIFOV.cs`)
Enemies use `NavMeshAgent` for pathfinding and operate on a state machine logic.
- **States:** Wandering (patrolling random points within a radius) and Chasing/Attacking.
- **Field of View (FOV):** Uses a cone-shaped FOV (`Physics.OverlapSphere` + Angle checks + Line-of-sight Raycasts) to detect the player.
- **Visual Searchlight:** A `Light` component (Spotlight) acts as a physical representation of the FOV. It smoothly rotates to follow the enemy's velocity when wandering, and locks onto the player when chasing.
- **Projectile Combat:** When chasing, the AI instantiates and fires physical projectiles (capsules rotated to fly horizontally) at the player's predicted center-of-mass (with `aimOffsetHeight`). It uses `Physics.IgnoreCollision` to ensure the enemy doesn't shoot itself.
- **Sound-Based Detection:** Enemies subscribe to the `SoundMeter.OnMaxSoundReached` event. When the player's noise meter fills completely, all subscribed enemies immediately abandon their patrol and aggressively chase the player's last known position, bypassing the normal FOV requirement.

### D. Health Systems (`PlayerHealth.cs`, `BotHealth.cs`)
- **Player Health:** Standard integer-based health pool. Features a UI text update and a visual damage indicator (a red Vignette UI Image that flashes and fades out using `Color.Lerp`).
- **Bot Health (`BotHealth.cs`):** Float-based health pool with `maxHealth`. Receives damage via `TakeDamage(float)`. When health reaches zero, calls `Die()` which permanently destroys the enemy GameObject (`Destroy(gameObject)`). Dead enemies do **not** respawn — once destroyed, they are gone for good.

### E. Interaction System (`PlayerInteract.cs`)
A unified raycast-based interaction system that handles all player-world interactions through a single script.
- **Raycast Detection:** Fires a ray from the center of the FPS camera every frame within a configurable `interactDistance`.
- **Hint UI:** Dynamically displays a contextual `TextMeshProUGUI` hint (e.g., `[E] Pickup Keycard`, `[E] Inspect Document`, `[E] Hack CCTV System`) when the player's crosshair hovers over an interactable object. The hint automatically hides when looking away.
- **Supported Interactable Types (checked in order):**
    1. **`KeypadButton`** — Presses a button on the NavKeypad asset for keypad-code input.
    2. **`PickableItem`** — Picks up the item into the player's inventory (`PlayerInventory`) and destroys the world object.
    3. **`InspectableItem`** — Opens a full-screen inspect view of a document/image without picking it up. Plays an inspect sound effect.
    4. **`InteractableCCTV`** — Triggers the CCTV hacking mode via `CCTVManager.Instance.EnterCCTVMode()`, passing the terminal's target camera index.
- **Audio:** Has an `AudioSource` reference and `AudioClip` for inspect interaction sounds.
- **Interact Key:** Configurable, defaults to `KeyCode.E`.

### F. Inventory & Inspect System (`PlayerInventory.cs`, `PickableItem.cs`, `InspectableItem.cs`)
A singleton inventory system that manages item pickup, UI display, and a full-screen document inspect mode.
- **Singleton Pattern:** `PlayerInventory.Instance` allows global access from `PlayerInteract` and other scripts.
- **Item Slot UI:** When an item is picked up, a UI panel (`itemSlotUI`) becomes visible and displays the item's icon sprite in an `Image` component.
- **Inspect Mode:** Pressing `I` toggles a full-screen overlay (`inspectPanelUI`) showing the item's image at native aspect ratio (`preserveAspect = true`). Pressing `Escape` also closes inspect. External scripts can call `ShowInspect(Sprite)` to display any document image directly (used by `InspectableItem`).
- **Item Consumption:** `ConsumeItem()` is called by the keypad system's "On Access Granted" Unity Event. This clears the inventory, hides the UI slot, and closes any open inspect panel.
- **Data Components:**
    - **`PickableItem`:** Attached to 3D world objects. Holds `itemName` (string) and `itemIcon` (Sprite).
    - **`InspectableItem`:** Attached to 3D world objects for inspect-only items (documents, notes). Holds `itemName` (string) and `documentImage` (Sprite).

### G. CCTV Hacking System (`CCTVManager.cs`, `InteractableCCTV.cs`, `CCTVCameraController.cs`)
A surveillance camera viewing system that lets the player remotely access and cycle through CCTV cameras.
- **Singleton Pattern:** `CCTVManager.Instance` manages the entire CCTV viewing lifecycle.
- **Entry Flow (`EnterCCTVMode`):**
    1. Hides the player HUD/UI (`playerUI`).
    2. Disables player controls — specifically `PlayerMov`, `PlayerCam`, `WeaponShooting`, `WeaponSwitcher`, and `PlayerInteract` scripts are disabled by name via reflection. The player's `Rigidbody` is set to kinematic with velocity zeroed to prevent drift.
    3. Deactivates the player's FPS camera GameObject.
    4. Activates the target CCTV camera (with an `AudioListener` component added dynamically if missing).
    5. Plays an enter CCTV sound effect.
- **Camera Cycling:** While in CCTV mode, pressing `E` cycles to the next camera in the list (wrapping around). A cooldown timer (0.2–0.5s) prevents accidental double-presses. A switch sound effect plays on each transition.
- **Exit Flow (`ExitCCTVMode`):** Pressing `Escape` reverses everything — deactivates all CCTV cameras, re-enables the player camera, restores all player scripts, sets `Rigidbody.isKinematic = false`, and shows the HUD again.
- **Terminal Configuration (`InteractableCCTV`):** A lightweight data component attached to hackable terminal objects. Holds a `terminalName` (display name for the hint UI) and `targetCameraIndex` (which CCTV camera to show first when hacked).
- **`CCTVCameraController`:** Stub/empty script — CCTV cameras are now static (no panning/rotating). This script can safely remain on camera objects or be removed.

### H. Sound Meter / Stealth System (`SoundMeter.cs`)
A noise-tracking stealth mechanic that alerts enemies when the player generates too much sound.
- **Singleton Pattern:** `SoundMeter.Instance` for global access from `PlayerMov` and `WeaponShooting`.
- **Noise Accumulation:** `AddSound(float amount)` increases the current noise level, clamped to `maxSound` (default 100). Different actions generate different amounts of noise:
    - Sprinting: High continuous noise
    - Walking: Moderate continuous noise
    - Crouching: Low/minimal noise
    - Jumping/Landing: Burst noise spike
    - Firing Weapons: Large burst noise spike
- **Noise Decay:** When the player is quiet, `currentSound` decays at `soundDecayRate` per second back toward zero.
- **Detection Trigger:** When `currentSound >= maxSound`, `TriggerDetection()` fires. This:
    1. Sets the meter to a "detected" state with a `detectCooldown` timer (default 3s) during which the bar stays full and no new sound is added.
    2. Invokes the static event `OnMaxSoundReached(Transform playerTransform)`, broadcasting the player's position to all subscribed enemy AI.
- **UI:** A `Filled` type `Image` component displays the meter visually, with `fillAmount` mapped to `currentSound / maxSound`.
- **Player Detection:** Automatically finds the player at `Start()` via `FindWithTag("Player")` or by locating a `PlayerMov` component as fallback.

### I. Door & Level Progression System (`SimpleDoor.cs`)
An animated door system tied to keypad authentication for level gating.
- **Door Animation:** When `BukaPintu()` is called (triggered by keypad's "Access Granted" event), the door smoothly slides downward on the Y-axis using `Vector3.MoveTowards` at a configurable speed (`kecepatan`) and distance (`jarakTurun`).
- **Level Loading:** Supports a `levelSelanjutnya` reference to a disabled parent GameObject representing the next level section. When the door opens, this level object is activated via `SetActive(true)`, enabling on-demand level loading within the same scene.
- **Integration:** Works in tandem with the NavKeypad asset and `PlayerInventory.ConsumeItem()` — the keypad validates a code, opens the door, consumes the keycard, and loads the next level segment.

### J. Enemy Projectile System (`EnemyProjectile.cs`)
Handles the behavior of physical projectiles fired by enemy AI.
- **Damage Delivery:** On `OnCollisionEnter`, checks for `PlayerHealth` component on the hit object and calls `TakeDamage(damage)`.
- **Visual Trail:** Automatically adds a `TrailRenderer` in `Awake()` if one is not already present. Uses an unlit "Sprites/Default" material with a yellow-to-red gradient trail (`startColor: yellow, endColor: red/transparent`).
- **Lifetime:** Self-destructs after `lifetime` seconds (default 5s) via `Destroy(gameObject, lifetime)` to prevent memory leaks from missed shots.
- **Destruction on Impact:** Always destroyed on collision regardless of what was hit.

### K. Object Pooling (`ObjectPoolManager.cs`)
A generic singleton object pooling system to reduce runtime `Instantiate`/`Destroy` overhead.
- **Singleton Pattern:** `ObjectPoolManager.Instance` for global access.
- **Configuration:** A serializable `List<Pool>` where each entry defines a `poolName` (string key), `prefab` (GameObject), and `poolSize` (initial count).
- **Pool Structure:** Uses `Dictionary<string, Queue<GameObject>>`. Objects are pre-instantiated in `Awake()`, deactivated, and parented under the manager for hierarchy cleanliness.
- **Usage:** Call `SpawnFromPool(string poolName, Vector3 position, Quaternion rotation)` to fetch a recycled object. The object is activated, repositioned, and re-enqueued for future reuse (circular queue pattern).
- **Intended Use Cases:** Damage text popups, muzzle flash effects, enemy projectiles, and other frequently spawned/despawned objects.

### L. Main Menu System (`MainMenuManager.cs`)
Handles the game's main menu scene with basic navigation.
- **Play:** `PlayGame()` loads the game scene by name using `SceneManager.LoadScene(namaSceneGame)`. The scene name is configurable in the Inspector.
- **Exit:** `ExitGame()` calls `Application.Quit()` for builds and `EditorApplication.isPlaying = false` for the Unity Editor (wrapped in `#if UNITY_EDITOR`).
- **Video Background:** The main menu supports a video background via Unity's `VideoPlayer` rendering to a `RawImage` through a Render Texture, managed alongside `VideoThumbnailHider.cs`.

### M. Utility Scripts

- **`CameraMov.cs`:** Simple camera position follower. Updates `transform.position` to match a target `cameraPosition` Transform every frame. Used to keep the camera holder synced with the player.
- **`VideoThumbnailHider.cs`:** Hides a placeholder thumbnail image once a `VideoPlayer` starts playing. Polls `videoTarget.isPlaying` in `Update()` and deactivates its own GameObject when the video is active. Used in the main menu to smoothly transition from a static poster to the live video background.
- **`WeaponRotation.cs`:** Handles procedural weapon positioning and rotation relative to the player camera. Features smooth position/rotation interpolation (`Lerp`/`Slerp` in `LateUpdate`) and optional mouse-driven sway with configurable intensity, max angle, and smoothing. Calculates weapon position using `positionOffset` relative to camera direction (right/up/forward) and supports a `rotationOffset` for model orientation correction.

## 3. Key Scripts Directory (`Assets/Scripts/`)

### Player Scripts
*   **`PlayerMov.cs`**: Handles all rigidbody player movement, input collection, state management, 3D model synchronization, and generates noise for the SoundMeter on movement actions.
*   **`PlayerCam.cs`**: Manages mouse look, camera rotation (yaw/pitch), and recoil application.
*   **`PlayerHealth.cs`**: Manages player HP, UI text, and damage vignette effects.
*   **`PlayerInteract.cs`**: Unified raycast interaction system — handles pickups, inspections, keypad buttons, and CCTV hacking via `[E]` key with dynamic hint text.
*   **`PlayerInventory.cs`**: Singleton inventory manager for item pickup, UI slot display, full-screen inspect mode (`[I]` key), and item consumption on keypad success.

### Combat & Weapon Scripts
*   **`WeaponShooting.cs`**: Handles raycast firing logic, damage calculation, visual effects (muzzle flash, impact), hiding/showing the player model, and adds noise to SoundMeter on fire.
*   **`WeaponSwitcher.cs`**: Allows changing active weapons via keyboard input (1-9).
*   **`WeaponRotation.cs`**: Procedural weapon sway, position, and rotation based on camera movement in `LateUpdate`.

### Enemy & AI Scripts
*   **`RandomAIFOV.cs`**: The brain of the enemy. Handles NavMesh roaming, FOV detection, spotlight orientation, projectile firing, and subscribes to `SoundMeter.OnMaxSoundReached` for sound-based chasing.
*   **`BotHealth.cs`**: Manages enemy HP with permanent death on zero (no respawn). `TakeDamage(float)` + `Die()` → `Destroy(gameObject)`.
*   **`BotHitbox.cs`**: Attached to enemy colliders to act as a receiver for raycast damage, forwards damage to parent `BotHealth`.
*   **`EnemyProjectile.cs`**: Physical projectile behavior — delivers damage on collision with player, auto-generates a glowing trail, self-destructs after timeout.

### Interaction & World Scripts
*   **`PickableItem.cs`**: Data component for pickup items. Holds `itemName` and `itemIcon` sprite.
*   **`InspectableItem.cs`**: Data component for inspect-only items. Holds `itemName` and `documentImage` sprite.
*   **`InteractableCCTV.cs`**: Data component for CCTV terminals. Holds `terminalName` and `targetCameraIndex`.
*   **`SimpleDoor.cs`**: Animated door that slides down when triggered, with optional next-level activation.

### System & Manager Scripts
*   **`CCTVManager.cs`**: Singleton managing CCTV hack mode — camera cycling, player control disable/enable, audio feedback.
*   **`SoundMeter.cs`**: Singleton noise-tracking stealth meter with decay, detection events, and UI fill bar.
*   **`ObjectPoolManager.cs`**: Generic singleton object pooling with dictionary-based named pools and circular queue reuse.
*   **`MainMenuManager.cs`**: Main menu scene manager with Play (scene load) and Exit functions.

### Utility Scripts
*   **`CameraMov.cs`**: Simple camera position syncing to a target Transform.
*   **`VideoThumbnailHider.cs`**: Hides a thumbnail overlay once a VideoPlayer starts playing.
*   **`DamageText.cs`**: Controls the floating text that appears when an enemy takes damage.
*   **`Sliding.cs` / `Gliding.cs`**: Modular movement abilities that hook into the `PlayerMov` state machine.

## 4. Important Implementation Quirks to Remember

1.  **Do not use `CharacterController` on the Player.** The movement relies entirely on a non-kinematic `Rigidbody` with `freezeRotation` enabled.
2.  **RobotKyle Model Hack:** If you add new components to the visual player model, be aware that `PlayerMov.Start()` might aggressively disable them to prevent conflicts.
3.  **Enemy Projectiles are Physical:** Enemies shoot physical Rigidbodies (with `EnemyProjectile.cs`), while the player uses Raycasts.
4.  **Tagging is crucial for Combat:** Headshots rely on the "Head" tag or object naming convention. Ensure all enemy rigs are configured correctly.
5.  **Enemies do NOT respawn.** `BotHealth.Die()` calls `Destroy(gameObject)` permanently. There is no pooling or respawn logic for enemies.
6.  **CCTV disables player via script name strings.** `CCTVManager.DisablePlayerControls()` identifies scripts by `GetType().Name` string comparison (e.g., `"PlayerCam"`, `"WeaponShooting"`). If you rename these scripts, the CCTV system will fail to disable them properly.
7.  **Singleton dependencies.** Multiple systems rely on Singleton pattern (`Instance` static properties): `CCTVManager`, `SoundMeter`, `PlayerInventory`, `ObjectPoolManager`. Ensure exactly one instance of each exists in the scene.
8.  **PlayerInteract checks interactable types in priority order.** `KeypadButton` > `PickableItem` > `InspectableItem` > `InteractableCCTV`. If an object has multiple interactable components, only the first match in this order will be processed.
9.  **NavKeypad integration.** The project uses the third-party `NavKeypad` asset (namespace `NavKeypad`). `PlayerInteract` references `KeypadButton` from this package. The keypad's "On Access Granted" event should call both `SimpleDoor.BukaPintu()` and `PlayerInventory.ConsumeItem()`.
10. **Sound system event-driven AI.** `SoundMeter.OnMaxSoundReached` is a static C# event. Enemy AI scripts subscribe/unsubscribe in `OnEnable`/`OnDisable`. Destroying an enemy automatically unsubscribes it (via `OnDisable`). No manual cleanup is needed.

## 5. System Integration Map

```
PlayerMov ──(noise)──► SoundMeter ──(OnMaxSoundReached event)──► RandomAIFOV (force chase)
    │                       │
    │                       └──► UI Fill Bar
    │
PlayerInteract ──► PickableItem ──► PlayerInventory ──(ConsumeItem)──► KeypadButton (NavKeypad)
    │                                    │                                     │
    │               InspectableItem ─────┘ (ShowInspect)                       ▼
    │                                                                    SimpleDoor.BukaPintu()
    │                                                                          │
    └──► InteractableCCTV ──► CCTVManager                              levelSelanjutnya.SetActive(true)
                                  │
                                  ├── Disable PlayerMov, PlayerCam, WeaponShooting, WeaponSwitcher
                                  ├── Disable PlayerInteract
                                  ├── Set Rigidbody.isKinematic = true
                                  ├── Hide Player UI
                                  └── Activate CCTV Camera + AudioListener

WeaponShooting ──(noise)──► SoundMeter
    │
    └──(raycast hit)──► BotHitbox ──► BotHealth ──(Die)──► Destroy(gameObject)

RandomAIFOV ──(fire projectile)──► EnemyProjectile ──(OnCollisionEnter)──► PlayerHealth.TakeDamage()
```

## 6. Potential Areas for Future Work / Polish

*   **Animation Blending:** Smoothing out the transitions between walking, sprinting, and the "FreeFall" state in `PlayerMov.cs`.
*   **Weapon Recoil Recovery:** Implementing a system where the camera smoothly returns to its original position after firing.
*   **Full Object Pool Integration:** `ObjectPoolManager` exists but not all systems use it yet. Converting `EnemyProjectile`, muzzle flashes, damage text, and impact effects to use the pool would improve performance.
*   **Audio Manager:** Centralizing audio playback instead of relying solely on localized `AudioSource.PlayOneShot` calls scattered across scripts.
*   **Multi-Item Inventory:** Currently `PlayerInventory` only holds one item at a time. Expanding to a multi-slot inventory with item stacking could be valuable.
*   **CCTV Camera Movement:** `CCTVCameraController` is currently a stub. Adding slow panning or player-controlled rotation of CCTV cameras would enhance the hacking mechanic.
*   **Enemy Respawn System:** Currently enemies are permanently destroyed. A spawner or wave-based system could add replayability.
*   **Save/Load System:** No persistence layer exists. Player progress, door states, and inventory are lost on scene reload.
*   **Pause Menu:** No in-game pause menu exists yet — only the main menu scene has UI navigation.

---
*Generated by Antigravity.*
