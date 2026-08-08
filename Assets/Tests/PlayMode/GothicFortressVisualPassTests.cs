using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace VexUnbound.Tests
{
    public sealed class GothicFortressVisualPassTests
    {
        [UnitySetUp]
        public IEnumerator LoadLevel()
        {
            SceneManager.LoadScene("Main");
            yield return null;
        }

        [TestCase("Start Platform", -4f, -1f, 8f, 1f, 3f)]
        [TestCase("Middle Platform", 2f, -0.65f, 3f, 1f, 3f)]
        [TestCase("Lower Rampart", 7f, -1f, 6f, 1f, 3f)]
        [TestCase("Tower Approach", 12.25f, -0.45f, 3.5f, 1f, 3f)]
        [TestCase("Broken Causeway", 16.5f, -0.9f, 4f, 1f, 3f)]
        [TestCase("Finish Platform", 22f, -1f, 6f, 1f, 3f)]
        [TestCase("Collapsed Bridge", 27.5f, -0.35f, 2f, 1f, 3f)]
        [TestCase("Watchtower Ledge", 31f, 0.35f, 2.5f, 1f, 3f)]
        [TestCase("Rubble Run", 35f, -0.75f, 4.5f, 1f, 3f)]
        [TestCase("High Rampart", 40f, 0f, 3.5f, 1f, 3f)]
        [TestCase("Final Approach", 46f, -1f, 7f, 1f, 3f)]
        public void PlatformsUseContinuousVisualsAndExpectedCollision(
            string name,
            float x,
            float y,
            float width,
            float height,
            float depth)
        {
            GameObject platform = GameObject.Find(name);
            Assert.That(platform, Is.Not.Null);
            Assert.That(platform.transform.position, Is.EqualTo(new Vector3(x, y, 0f)));

            BoxCollider collider = platform.GetComponent<BoxCollider>();
            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.size, Is.EqualTo(new Vector3(width, height, depth)));
            Assert.That(platform.GetComponentsInChildren<Collider>(true), Has.Length.EqualTo(1));
            Transform foundation = platform.transform.Find("Visuals/Continuous Foundation");
            Assert.That(foundation, Is.Not.Null);
            Assert.That(foundation.localScale.x, Is.EqualTo(width));

            MaterialPropertyBlock properties = new();
            foundation.GetComponent<MeshRenderer>().GetPropertyBlock(properties);
            Assert.That(properties.GetVector("_BaseMap_ST").x, Is.GreaterThanOrEqualTo(width / 1.4f));
        }

        [Test]
        public void FortressLayersUseDifferentParallaxRates()
        {
            GameObject distant = GameObject.Find("Distant Keep");
            GameObject near = GameObject.Find("Near Ramparts");
            Assert.That(distant, Is.Not.Null);
            Assert.That(near, Is.Not.Null);

            Component distantParallax = distant.GetComponent("ParallaxLayer");
            Component nearParallax = near.GetComponent("ParallaxLayer");
            Assert.That(distantParallax, Is.Not.Null);
            Assert.That(nearParallax, Is.Not.Null);

            float distantFactor = ReadParallaxFactor(distantParallax);
            float nearFactor = ReadParallaxFactor(nearParallax);
            Assert.That(distantFactor, Is.InRange(0.05f, 0.15f));
            Assert.That(nearFactor, Is.GreaterThan(distantFactor * 2f));
        }

        [Test]
        public void ExtendedRouteKeepsEveryTransitionReachable()
        {
            string[] route =
            {
                "Start Platform",
                "Middle Platform",
                "Lower Rampart",
                "Tower Approach",
                "Broken Causeway",
                "Finish Platform",
                "Collapsed Bridge",
                "Watchtower Ledge",
                "Rubble Run",
                "High Rampart",
                "Final Approach"
            };

            for (int i = 0; i < route.Length - 1; i++)
            {
                Bounds current = GameObject.Find(route[i]).GetComponent<BoxCollider>().bounds;
                Bounds next = GameObject.Find(route[i + 1]).GetComponent<BoxCollider>().bounds;
                Assert.That(next.min.x - current.max.x, Is.LessThanOrEqualTo(1.75f), $"Gap after {route[i]} is too wide.");
                Assert.That(next.max.y - current.max.y, Is.LessThanOrEqualTo(0.8f), $"Rise after {route[i]} is too high.");
            }

            Bounds finishPlatform = GameObject.Find("Final Approach").GetComponent<BoxCollider>().bounds;
            Assert.That(GameObject.Find("Finish").transform.position.x, Is.InRange(finishPlatform.min.x, finishPlatform.max.x));
        }

        [TestCase("Lower Rampart Barricade", 0.65f, 0.9f)]
        [TestCase("Causeway Blockade", 0.7f, 0.9f)]
        [TestCase("Rubble Run Barricade", 0.8f, 0.8f)]
        [TestCase("Final Gate Debris", 0.75f, 1.1f)]
        public void BarricadesCreateJumpableCollision(string name, float width, float height)
        {
            GameObject obstacle = GameObject.Find(name);
            Assert.That(obstacle, Is.Not.Null);
            BoxCollider collider = obstacle.GetComponent<BoxCollider>();
            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.size.x, Is.EqualTo(width).Within(0.001f));
            Assert.That(collider.size.y, Is.EqualTo(height).Within(0.001f));
            Assert.That(height, Is.LessThan(1.2f));
            Assert.That(obstacle.GetComponentsInChildren<Collider>(true), Has.Length.EqualTo(1));
            MeshRenderer renderer = obstacle.transform.Find("Wooden Barricade").GetComponent<MeshRenderer>();
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.sharedMaterial.name, Is.EqualTo("Weathered Barricade Wood"));
            Assert.That(renderer.sharedMaterial.GetTexture("_BaseMap"), Is.Not.Null);
        }

        [Test]
        public void MotionAndUiUseSmoothDimensionalPresentation()
        {
            GameObject hero = GameObject.Find("Hero");
            Assert.That(hero.GetComponent("PlayerPresentation"), Is.Not.Null);
            Assert.That(hero.GetComponent<Rigidbody>(), Is.Null);
            CharacterController characterController = hero.GetComponent<CharacterController>();
            Assert.That(characterController, Is.Not.Null);
            Assert.That(characterController.slopeLimit, Is.EqualTo(50f));
            Assert.That(characterController.stepOffset, Is.EqualTo(0.32f).Within(0.001f));
            Assert.That(Time.fixedDeltaTime, Is.EqualTo(1f / 60f).Within(0.0001f));

            Animation animation = hero.GetComponentInChildren<Animation>();
            Assert.That(animation, Is.Not.Null);
            Renderer characterRenderer = hero.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.That(characterRenderer, Is.Not.Null);
            Assert.That(characterRenderer.bounds.size.y, Is.InRange(1f, 3f));
            Assert.That(characterRenderer.bounds.min.y, Is.EqualTo(hero.transform.position.y - 0.14f).Within(0.1f));
            Assert.That(characterRenderer.bounds.center.x, Is.EqualTo(hero.transform.position.x).Within(0.1f));
            Assert.That(
                Mathf.DeltaAngle(hero.transform.Find("Hero Presentation").localEulerAngles.y, 180f),
                Is.Zero.Within(0.1f));
            Vector3 characterViewport = Camera.main.WorldToViewportPoint(characterRenderer.bounds.center);
            Assert.That(characterViewport.x, Is.InRange(0f, 1f));
            Assert.That(characterViewport.y, Is.InRange(0f, 1f));
            Assert.That(characterViewport.z, Is.GreaterThan(0f));
            foreach (string state in new[] { "Idle", "Run", "Jump", "Fall", "Land", "Turn" })
            {
                Assert.That(animation.GetClip(state), Is.Not.Null, $"Missing authored {state} clip.");
                Assert.That(animation.GetClip(state).empty, Is.False, $"Authored {state} clip has no skeleton curves.");
            }

            Assert.That(Camera.main.GetComponent("CinemachineBrain"), Is.Not.Null);
            Assert.That(GameObject.Find("Player Follow Camera").GetComponent("CinemachineCamera"), Is.Not.Null);

            GameObject gameUi = GameObject.Find("Game UI");
            Assert.That(gameUi.GetComponent("GraphicRaycaster"), Is.Not.Null);
            Assert.That(GameObject.Find("Event System"), Is.Not.Null);

            Transform left = gameUi.transform.Find("Touch Controls/Left");
            Assert.That(left, Is.Not.Null);
            Assert.That(left.GetComponent("TouchControlVisual"), Is.Not.Null);
            Assert.That(left.Find("Control Shadow"), Is.Not.Null);
            Assert.That(left.Find("Iron Frame"), Is.Not.Null);
            Assert.That(left.Find("Stone Face/Upper Edge"), Is.Not.Null);
            Transform right = gameUi.transform.Find("Touch Controls/Right");
            Assert.That(Mathf.DeltaAngle(left.Find("Stone Face/Chevron Top").localEulerAngles.z, -45f), Is.Zero.Within(0.1f));
            Assert.That(Mathf.DeltaAngle(right.Find("Stone Face/Chevron Top").localEulerAngles.z, 45f), Is.Zero.Within(0.1f));

            Transform restart = gameUi.transform.Find("Completion/Completion Panel/Restart");
            Assert.That(restart.Find("Button Shadow"), Is.Not.Null);
            Assert.That(restart.Find("Button Face/Button Highlight"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator SettledCinemachineKeepsGameplayVisible()
        {
            for (int i = 0; i < 30; i++)
            {
                yield return null;
            }

            Camera camera = Camera.main;
            GameObject hero = GameObject.Find("Hero");
            Vector3 heroViewport = camera.WorldToViewportPoint(hero.transform.position + Vector3.up * 0.8f);
            Vector3 platformViewport = camera.WorldToViewportPoint(GameObject.Find("Start Platform").transform.position);
            Assert.That(heroViewport.x, Is.InRange(0.1f, 0.9f),
                $"Hero viewport={heroViewport}, camera={camera.transform.position}, size={camera.orthographicSize}");
            Assert.That(heroViewport.y, Is.InRange(0.1f, 0.9f),
                $"Hero viewport={heroViewport}, camera={camera.transform.position}, size={camera.orthographicSize}");
            Assert.That(heroViewport.z, Is.GreaterThan(0f),
                $"Hero viewport={heroViewport}, camera={camera.transform.position}, rotation={camera.transform.eulerAngles}");
            Assert.That(platformViewport.z, Is.GreaterThan(0f),
                $"Platform viewport={platformViewport}, camera={camera.transform.position}, rotation={camera.transform.eulerAngles}");
        }

        [UnityTest]
        public IEnumerator ReversingDirectionKeepsModelFacingCamera()
        {
            yield return new WaitForFixedUpdate();
            GameObject hero = GameObject.Find("Hero");
            Component controller = hero.GetComponent("PlayerController");
            Component presentation = hero.GetComponent("PlayerPresentation");
            Transform model = hero.GetComponentInChildren<Animation>().transform;
            Quaternion correctiveRotation = model.localRotation;
            Quaternion presentationRotation = model.parent.localRotation;
            System.Reflection.MethodInfo lateUpdate = presentation.GetType().GetMethod(
                "LateUpdate",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            SetField(controller, "velocity", Vector3.left * 4f);
            lateUpdate.Invoke(presentation, null);
            Assert.That(Quaternion.Angle(model.localRotation, correctiveRotation), Is.LessThan(0.01f));
            Assert.That(Quaternion.Angle(model.parent.localRotation, presentationRotation), Is.LessThan(0.01f));

            SetField(controller, "velocity", Vector3.right * 4f);
            lateUpdate.Invoke(presentation, null);
            Assert.That(Quaternion.Angle(model.localRotation, correctiveRotation), Is.LessThan(0.01f));
            Assert.That(Quaternion.Angle(model.parent.localRotation, presentationRotation), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator ReleasedJumpUsesAControlledShortArc()
        {
            GameObject hero = GameObject.Find("Hero");
            Component controller = hero.GetComponent("PlayerController");
            Assert.That(hero.GetComponent<CharacterController>(), Is.Not.Null);

            System.Reflection.FieldInfo jumpHeld = controller.GetType().GetField(
                "jumpHeld",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            System.Reflection.MethodInfo getGravity = controller.GetType().GetMethod(
                "GetGravity",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            jumpHeld.SetValue(controller, true);
            Assert.That((float)getGravity.Invoke(controller, new object[] { 4f }), Is.EqualTo(18f));
            jumpHeld.SetValue(controller, false);
            Assert.That((float)getGravity.Invoke(controller, new object[] { 4f }), Is.EqualTo(36f));

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            float startHeight = hero.transform.position.y;
            controller.GetType()
                .GetMethod("QueueJump", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(controller, null);

            yield return new WaitForFixedUpdate();
            Assert.That(ReadVelocity(controller).y, Is.GreaterThan(7.5f));

            float peakHeight = hero.transform.position.y;
            for (int i = 0; i < 36 && ReadVelocity(controller).y > 0f; i++)
            {
                yield return new WaitForFixedUpdate();
                peakHeight = Mathf.Max(peakHeight, hero.transform.position.y);
            }

            Assert.That(peakHeight - startHeight, Is.InRange(0.6f, 1.5f));
            for (int i = 0; i < 8; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(ReadVelocity(controller).y, Is.LessThan(-2f));
        }

        [UnityTest]
        public IEnumerator GroundingBufferCoyoteAndEdgesUseControllerCollisionState()
        {
            GameObject hero = GameObject.Find("Hero");
            Component player = hero.GetComponent("PlayerController");
            CharacterController characterController = hero.GetComponent<CharacterController>();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(ReadGrounded(player), Is.True);

            SetField(player, "lastGroundedTime", Time.time);
            SetField(player, "jumpQueuedUntil", Time.time + 0.12f);
            characterController.Move(Vector3.up * 0.12f);
            yield return new WaitForFixedUpdate();
            Assert.That(ReadVelocity(player).y, Is.GreaterThan(7.5f), "Coyote-time jump was not consumed.");

            SetField(player, "velocity", Vector3.down * 2f);
            SetField(player, "jumpQueuedUntil", Time.time + 0.12f);
            characterController.enabled = false;
            hero.transform.position = new Vector3(-4f, -0.3f, 0f);
            characterController.enabled = true;
            for (int i = 0; i < 10 && ReadVelocity(player).y <= 0f; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(ReadVelocity(player).y, Is.GreaterThan(0f), "Buffered jump did not fire on landing.");

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Grounding Test Wall";
            wall.transform.position = hero.transform.position + new Vector3(1f, 0.9f, 0f);
            wall.transform.localScale = new Vector3(0.2f, 4f, 3f);
            Physics.SyncTransforms();
            CollisionFlags wallContact = characterController.Move(Vector3.right * 2f);
            Assert.That((wallContact & CollisionFlags.Sides) != 0, Is.True);
            Assert.That((wallContact & CollisionFlags.Below) != 0, Is.False, "A side contact must not count as grounded.");
        }

        [Test]
        public void GamepadStickDpadAndSouthButtonDrivePlayerInput()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                Component player = GameObject.Find("Hero").GetComponent("PlayerController");
                System.Reflection.MethodInfo readInput = player.GetType().GetMethod(
                    "ReadInput",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                InputSystem.QueueStateEvent(gamepad, new GamepadState { leftStick = Vector2.right * 0.75f });
                InputSystem.Update();
                readInput.Invoke(player, null);
                Assert.That(ReadField<float>(player, "movement"), Is.EqualTo(0.75f).Within(0.001f));

                InputSystem.QueueStateEvent(gamepad, new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.DpadLeft
                });
                InputSystem.Update();
                readInput.Invoke(player, null);
                Assert.That(ReadField<float>(player, "movement"), Is.EqualTo(-1f).Within(0.001f));

                InputSystem.QueueStateEvent(gamepad, new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.South
                });
                InputSystem.Update();
                readInput.Invoke(player, null);
                Assert.That(ReadField<float>(player, "jumpQueuedUntil"), Is.GreaterThan(Time.time));
                Assert.That(ReadField<bool>(player, "jumpHeld"), Is.True);

                InputSystem.QueueStateEvent(gamepad, new GamepadState());
                InputSystem.Update();
                readInput.Invoke(player, null);
                Assert.That(ReadField<bool>(player, "jumpHeld"), Is.False);
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator CharacterControllerCanTraverseFinishTrigger()
        {
            GameObject hero = GameObject.Find("Hero");
            CharacterController characterController = hero.GetComponent<CharacterController>();
            characterController.enabled = false;
            hero.transform.position = new Vector3(47f, -0.48f, 0f);
            characterController.enabled = true;
            Physics.SyncTransforms();
            characterController.Move(Vector3.right * 1.2f);
            yield return new WaitForFixedUpdate();

            Assert.That(GameObject.Find("Game UI").transform.Find("Completion").gameObject.activeSelf, Is.True);
        }

        [Test]
        public void LightingAtmosphereAndFinishRemainBounded()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Assert.That(lights, Has.Some.Matches<Light>(light =>
                light.type == LightType.Directional && light.color.b > light.color.r && light.intensity >= 1.2f));
            Assert.That(lights, Has.Some.Matches<Light>(light =>
                light.type == LightType.Point && light.color.r > light.color.b));

            GameObject rainObject = GameObject.Find("Light Rain");
            Assert.That(rainObject, Is.Not.Null);
            Assert.That(rainObject.GetComponent<ParticleSystem>().main.maxParticles, Is.LessThanOrEqualTo(70));
            Assert.That(rainObject.GetComponent<ParticleSystem>().shape.scale.x, Is.GreaterThanOrEqualTo(80f));
            Assert.That(
                rainObject.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.name,
                Is.EqualTo("Universal Render Pipeline/Unlit"));
            Assert.That(
                rainObject.GetComponent<ParticleSystemRenderer>().sharedMaterial.IsKeywordEnabled("_SURFACE_TYPE_TRANSPARENT"),
                Is.True);
            Assert.That(
                Resources.Load<Material>("Environment/GothicFortress/Materials/WeatheredStoneSides").IsKeywordEnabled("_NORMALMAP"),
                Is.True);
            Assert.That(
                Resources.Load<Material>("Environment/GothicFortress/Materials/WeatheredStoneSides").IsKeywordEnabled("_OCCLUSIONMAP"),
                Is.True);
            Assert.That(
                Resources.Load<Material>("Environment/GothicFortress/Materials/WeatheredStoneTops").IsKeywordEnabled("_NORMALMAP"),
                Is.True);
            Assert.That(Resources.Load<Material>("Environment/GothicFortress/Materials/DarkFortressStone"), Is.Not.Null);
            Assert.That(Resources.Load<Material>("Environment/GothicFortress/Materials/BlackenedIron"), Is.Not.Null);
            Assert.That(RenderSettings.fog, Is.True);

            GameObject finish = GameObject.Find("Finish");
            Assert.That(finish.transform.position, Is.EqualTo(new Vector3(48f, 0.75f, 0f)));
            BoxCollider trigger = finish.GetComponent<BoxCollider>();
            Assert.That(trigger.isTrigger, Is.True);
            Assert.That(trigger.size, Is.EqualTo(new Vector3(1.2f, 2.5f, 2.5f)));
            Assert.That(finish.GetComponentsInChildren<Collider>(true), Has.Length.EqualTo(1));
        }

        private static float ReadParallaxFactor(Component parallax)
        {
            return (float)parallax.GetType()
                .GetProperty("Factor")
                .GetGetMethod(true)
                .Invoke(parallax, null);
        }

        private static Vector3 ReadVelocity(Component controller)
        {
            return (Vector3)controller.GetType().GetProperty("Velocity").GetValue(controller);
        }

        private static bool ReadGrounded(Component controller)
        {
            return (bool)controller.GetType().GetProperty("Grounded").GetValue(controller);
        }

        private static void SetField(Component controller, string name, object value)
        {
            controller.GetType()
                .GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(controller, value);
        }

        private static T ReadField<T>(Component controller, string name)
        {
            return (T)controller.GetType()
                .GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(controller);
        }
    }
}
