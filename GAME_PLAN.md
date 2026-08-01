# Vex Unbound — Game Plan

## Vision

**Vex Unbound** is a small, finished 2.5D action platformer for Android.

The world and characters are rendered in 3D, while gameplay movement is restricted to a 2D plane. Platforming and combat have equal importance.

## Core Decisions

- **Engine:** Unity 6.3 LTS
- **Language:** C#
- **Rendering:** Universal Render Pipeline (URP)
- **Platform:** Android
- **Orientation:** Landscape
- **Visual style:** Modern stylized 3D
- **Camera:** Mostly fixed side view
- **Controls:** Touchscreen and gamepad
- **Difficulty:** Forgiving
- **Budget:** Free tools and assets only

## Core Gameplay

Vex fights with two swords and can:

- Run
- Jump
- Dash horizontally
- Perform light attacks
- Perform heavy attacks

The controls should feel responsive and include:

- Coyote time
- Jump input buffering
- Air control
- Fast respawning
- Frequent checkpoints
- Clear enemy attack warnings

## Touch Controls

- Virtual movement stick on the left
- Jump button
- Dash button
- Light attack button
- Heavy attack button

Gamepad controls expose the same actions through Unity's Input System.

## Initial Game Scope

The finished game should contain:

- One playable character
- One visual environment theme
- Several short level sections
- A small set of regular enemy types
- One boss
- Checkpoints
- Health and damage systems
- Collectibles
- One simple ability or upgrade progression
- Title screen
- Pause menu
- Settings menu
- Death and restart flow
- Ending screen
- Local save system

## Out of Scope

The first game will not include:

- Multiplayer
- Online services
- Crafting
- Quests
- Complex inventory
- Procedural levels
- Large skill trees
- Multiple playable characters

## Technical Stack

| Area | Technology |
|---|---|
| Engine | Unity 6.3 LTS |
| Language | C# |
| Rendering | URP |
| Player movement | Custom motor using `CharacterController` |
| Camera | Cinemachine |
| Input | Unity Input System |
| UI | uGUI and TextMeshPro |
| Animation | Unity Animator |
| Level creation | ProBuilder, prefabs, and modular assets |
| Game data | ScriptableObjects |
| Testing | Unity Test Framework |
| 3D editing | Blender |
| Character animation | Mixamo |
| Free assets | Kenney, Quaternius, and other compatible free packs |

## Code Architecture

Main gameplay components:

- `InputReader`
- `PlayerMotor`
- `PlayerCombat`
- `PlayerAnimation`
- `Health`
- `DamageDealer`
- `EnemyBrain`
- `Checkpoint`
- `RespawnController`
- `CameraController`
- `SaveSystem`
- `GameManager`

Configuration should use ScriptableObjects for:

- Player movement settings
- Attacks
- Enemy statistics
- Enemy behaviour settings
- Level configuration
- Audio and visual effects

Systems should communicate through narrow public APIs and events rather than direct dependencies wherever practical.

## Enemy Behaviour

The first enemy uses a small state machine:

1. Patrol
2. Notice player
3. Approach
4. Attack
5. Recover
6. Take damage
7. Die

Later enemies should reuse the same health, damage, and state-machine foundations.

## Graphics Workflow

AI image generation can create:

- Concept art
- Colour palettes
- Texture sources
- Decals
- UI icons
- Menu backgrounds
- VFX sprites

Reusable 3D assets should initially come from free asset packs. Blender scripts and procedural materials can customize their appearance.

Characters should use consistent proportions, materials, and colour rules. Generated imagery should serve as source material and reference rather than being inserted without review.

## LLM-Friendly Development Rules

- Keep scripts small and focused.
- Avoid large all-purpose managers.
- Store balancing values outside code.
- Use consistent naming and folder structure.
- Document non-obvious design decisions.
- Prefer reusable prefabs and ScriptableObjects.
- Create editor tools for repetitive scene and prefab changes.
- Avoid manually editing Unity scene or prefab YAML.
- Add tests for isolated gameplay logic.
- Keep every milestone buildable and playable.
- Commit small, coherent changes.

## Proposed Project Structure

```text
Assets/
  Art/
    Characters/
    Environments/
    Materials/
    Textures/
    UI/
    VFX/
  Audio/
  Prefabs/
    Characters/
    Enemies/
    Environment/
    Gameplay/
    UI/
  Scenes/
  Scripts/
    Camera/
    Combat/
    Core/
    Enemies/
    Input/
    Player/
    Saving/
    UI/
  Settings/
  Tests/
```

## Development Milestones

### 1. Project Foundation

- Create the Unity URP project.
- Establish the folder structure.
- Configure Android builds.
- Configure landscape orientation.
- Add the Input System and Cinemachine.
- Produce the first installable APK.

### 2. Movement Prototype

- Build a grey-box test level.
- Implement running and jumping.
- Add coyote time and jump buffering.
- Implement horizontal dash.
- Add the fixed side camera.
- Add touchscreen and gamepad input.

### 3. Combat Prototype

- Implement light and heavy attacks.
- Add hitboxes, damage, and knockback.
- Add health, death, and respawning.
- Create one basic enemy.
- Add a checkpoint.
- Produce a short playable test level.

### 4. Vertical Slice

- Replace the grey-box player with Vex.
- Add final animations for movement and attacks.
- Establish the final environment style.
- Add lighting, materials, particles, and audio.
- Create functional menus and settings.
- Validate performance on a real Android device.

### 5. Complete Game Content

- Build all level sections.
- Add the remaining enemies.
- Add collectibles and progression.
- Create the boss encounter.
- Add the ending.
- Complete the save system.

### 6. Release Pass

- Fix gameplay and visual bugs.
- Balance platforming and combat.
- Optimize rendering, memory, and loading.
- Test different screen resolutions.
- Test touchscreen and gamepad controls.
- Create the final Android build.

## First Playable Build

The first build is complete when it includes:

- Running
- Jumping
- Dashing
- Light attack
- Heavy attack
- One enemy
- Player damage and death
- One checkpoint
- A short grey-box level
- Touchscreen controls
- Gamepad controls
- Pause and restart
- A working Android APK

Movement and combat feel must be validated before producing the final art and full game content.
