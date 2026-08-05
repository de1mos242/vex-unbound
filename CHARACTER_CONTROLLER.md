# Character Controller Selection

## Selection

Vex Unbound uses Unity 6's built-in `CharacterController` as its kinematic motor.
It is part of the supported Unity Physics module, has no additional runtime or
package dependency, and uses the same native collision implementation on Android.
The project configures its capsule, skin width, 50-degree slope limit, and 0.32 m
step offset while retaining game-specific acceleration, braking, air control,
coyote time, jump buffering, jump cutting, and terminal velocity.

The controller is the sole movement authority. Authored clips only rotate bones
under the visual child, contain no root translation curves, and do not use root
motion, so they cannot displace the gameplay capsule.

## Alternatives Evaluated

- [OpenKCC](https://github.com/nicholas-maltbie/OpenKCC): MIT licensed and GameObject based, but its latest repository code was pushed in September 2023. It adds package and maintenance surface without a required feature that Unity 6's controller lacks for this 2.5D level.
- [Unity Character Controller samples](https://github.com/Unity-Technologies/CharacterControllerSamples): actively maintained Unity examples, but based on the Entities character-controller package. Adopting ECS for one player would conflict with this small GameObject-based project and increase Android build/runtime complexity.
- Unity 6 `CharacterController`: selected because it is maintained with the pinned editor, supports slopes, steps, overlap recovery, and collision flags directly, and is available on the existing Android target without extra code or licensing.

## Runtime Architecture

- Input is sampled every rendered frame from keyboard and independent UI pointers.
- Velocity is integrated at the fixed 60 Hz gameplay rate and passed to `CharacterController.Move`.
- Only `CollisionFlags.Below` establishes grounded state; wall contacts cannot grant a jump.
- Movement remains on the XY plane and Z is explicitly fixed to zero.
- Cinemachine 3 follows the gameplay transform with horizontal look-ahead and damped vertical/horizontal composition.
