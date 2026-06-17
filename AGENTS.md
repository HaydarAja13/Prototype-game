# Game Prototype Context for AI Agents

Welcome, AI Agent. This document contains detailed context about the Unity First-Person Shooter (FPS) prototype project. Please refer to this guide to understand the architecture, core mechanics, and specific implementation details before making modifications.

## 1. Project Overview

This project is a 3D First-Person Shooter prototype built in Unity. It features advanced physics-based player movement, raycast shooting mechanics, and NavMesh-based enemy AI with visual field-of-view (FOV) detection.

**Project Path:** `d:\D4 Polines Jaya jaya jaya\Prototype\`
**Language:** C#
**Engine:** Unity

## 2. Core Systems Architecture

### A. Player Controller (`PlayerMov.cs`)
The player uses a custom physics-based Rigidbody controller, completely bypassing Unity's standard `CharacterController` component.
- **Movement States:** Walking, Sprinting, Crouching, Sliding, Gliding, and in-Air.
- **Physics Handling:** Applies drag on the ground, custom slope handling to prevent bouncing (`OnSlope`, `GetSlopeMoveDirection`), and limits maximum velocity.
- **3D Model Integration (The "RobotKyle" Quirk):** The player capsule has a 3D model (RobotKyle) attached as a visual representation. To prevent physics conflicts, `PlayerMov.cs` programmatically disables all `Collider`, `CharacterController`, and `MonoBehaviour` (except Animator) components on the RobotKyle model at startup. The model is also forced to the "Ignore Raycast" layer. The Animator's rotation is driven by the camera's orientation.

### B. Combat & Weapon System (`WeaponShooting.cs`, `WeaponSwitcher.cs`)
The combat system uses instantaneous hit-scan (Raycast) for the player and physical projectiles for the enemies.
- **Raycast Shooting:** Fires a ray from the center of the camera (`fpsCam`).
- **Damage & Hitboxes:** Checks for `BotHitbox` components. Implements a headshot multiplier if the hit collider has the tag "Head" or contains "head" in its name.
- **Visual Feedback:** Supports Muzzle Flashes, Impact Effects, floating Damage Text (`DamageText.cs`), and camera recoil via `PlayerCam.cs`.
- **Aiming State:** When the shoot button is held, the script hides the player model (`RobotKyle`) and reveals the weapon model to simulate aiming down sights or bringing the weapon up.
- **Weapon Switching:** Managed by `WeaponSwitcher.cs`, allowing swapping between an array of assigned weapon GameObjects using number keys (1-9).

### C. Enemy AI (`RandomAIFOV.cs`)
Enemies use `NavMeshAgent` for pathfinding and operate on a state machine logic.
- **States:** Wandering (patrolling random points within a radius) and Chasing/Attacking.
- **Field of View (FOV):** Uses a cone-shaped FOV (`Physics.OverlapSphere` + Angle checks + Line-of-sight Raycasts) to detect the player.
- **Visual Searchlight:** A `Light` component (Spotlight) acts as a physical representation of the FOV. It smoothly rotates to follow the enemy's velocity when wandering, and locks onto the player when chasing.
- **Projectile Combat:** When chasing, the AI instantiates and fires physical projectiles (capsules rotated to fly horizontally) at the player's predicted center-of-mass (with `aimOffsetHeight`). It uses `Physics.IgnoreCollision` to ensure the enemy doesn't shoot itself.

### D. Health Systems (`PlayerHealth.cs`, `BotHealth.cs`)
- **Player Health:** Standard integer-based health pool. Features a UI text update and a visual damage indicator (a red Vignette UI Image that flashes and fades out using `Color.Lerp`).
- **Bot Health:** (Implied by `BotHitbox.cs`) Handles receiving damage from the player's raycasts.

## 3. Key Scripts Directory (`Assets/Scripts/`)

*   **`PlayerMov.cs`**: Handles all rigidbody player movement, input collection, state management, and 3D model synchronization.
*   **`PlayerCam.cs`**: Manages mouse look, camera rotation (yaw/pitch), and recoil application.
*   **`PlayerHealth.cs`**: Manages player HP, UI text, and damage vignette effects.
*   **`WeaponShooting.cs`**: Handles raycast firing logic, damage calculation, visual effects (muzzle flash, impact), and hiding/showing the player model.
*   **`WeaponSwitcher.cs`**: Allows changing active weapons via keyboard input.
*   **`WeaponRotation.cs`**: (Context implied) Likely handles weapon sway or procedural animation based on camera movement.
*   **`RandomAIFOV.cs`**: The brain of the enemy. Handles NavMesh roaming, FOV detection, spotlight orientation, and projectile firing.
*   **`BotHitbox.cs`**: Attached to enemy colliders to act as a receiver for raycast damage.
*   **`DamageText.cs`**: Controls the floating text that appears when an enemy takes damage.
*   **`Sliding.cs` / `Gliding.cs`**: Modular movement abilities that hook into the `PlayerMov` state machine.

## 4. Important Implementation Quirks to Remember

1.  **Do not use `CharacterController` on the Player.** The movement relies entirely on a non-kinematic `Rigidbody` with `freezeRotation` enabled.
2.  **RobotKyle Model Hack:** If you add new components to the visual player model, be aware that `PlayerMov.Start()` might aggressively disable them to prevent conflicts.
3.  **Enemy Projectiles are Physical:** Enemies shoot physical Rigidbodies, while the player uses Raycasts. 
4.  **Tagging is crucial for Combat:** Headshots rely on the "Head" tag or object naming convention. Ensure all enemy rigs are configured correctly.

## 5. Potential Areas for Future Work / Polish

*   **Animation Blending:** Smoothing out the transitions between walking, sprinting, and the "FreeFall" state in `PlayerMov.cs`.
*   **Weapon Recoil Recovery:** Implementing a system where the camera smoothly returns to its original position after firing.
*   **Object Pooling:** Currently, bullets, muzzle flashes, and damage text are `Instantiated` and `Destroyed`. Converting these to Object Pools would significantly improve performance.
*   **Audio Manager:** Centralizing audio playback instead of relying solely on localized `AudioSource.PlayOneShot`.

---
*Generated by Antigravity.*
