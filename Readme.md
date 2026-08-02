# Vex Unbound

A 2.5D action platformer for Android built with Unity 6.5 and the Universal Render Pipeline.

## Development

- Open the repository root with Unity `6000.5.6f1`.
- Use **Vex Unbound > Build Development APK** to create `Builds/Android/VexUnbound.apk`.
- Install Unity's Android Build Support, Android SDK & NDK Tools, and OpenJDK modules before building.

## Continuous Integration

The **Build Android APK** GitHub Actions workflow builds a development APK on every push to `master`. It can also be started from the repository's **Actions** tab with **Run workflow**.

Configure these repository Actions secrets before running the workflow with a Unity Personal license:

- `UNITY_LICENSE`: the complete contents of the Unity `.ulf` license file
- `UNITY_EMAIL`: the email address for the Unity account
- `UNITY_PASSWORD`: the password for the Unity account

See the [GameCI activation guide](https://game.ci/docs/github/activation) for license setup. Do not commit Unity credentials or license files.

After a successful run, open it in the **Actions** tab and download the `VexUnbound-Android-<commit SHA>` artifact. The artifact contains `VexUnbound.apk` and is retained for 14 days.

## Controls

- Move with the on-screen left and right controls, the arrow keys, or `A` and `D`.
- Jump with the on-screen **JUMP** control or the Space key.
- Reach the gold gate to finish the level, then select **RESTART** or press `R` to play again.
