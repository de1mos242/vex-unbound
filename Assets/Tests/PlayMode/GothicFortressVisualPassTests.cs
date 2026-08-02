using System.Collections;
using NUnit.Framework;
using UnityEngine;
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
                "Finish Platform"
            };

            for (int i = 0; i < route.Length - 1; i++)
            {
                Bounds current = GameObject.Find(route[i]).GetComponent<BoxCollider>().bounds;
                Bounds next = GameObject.Find(route[i + 1]).GetComponent<BoxCollider>().bounds;
                Assert.That(next.min.x - current.max.x, Is.LessThanOrEqualTo(0.6f), $"Gap after {route[i]} is too wide.");
                Assert.That(next.max.y - current.max.y, Is.LessThanOrEqualTo(0.6f), $"Rise after {route[i]} is too high.");
            }

            Bounds finishPlatform = GameObject.Find("Finish Platform").GetComponent<BoxCollider>().bounds;
            Assert.That(GameObject.Find("Finish").transform.position.x, Is.InRange(finishPlatform.min.x, finishPlatform.max.x));
        }

        [Test]
        public void MotionAndUiUseSmoothDimensionalPresentation()
        {
            GameObject hero = GameObject.Find("Hero");
            Assert.That(hero.GetComponent("PlayerPresentation"), Is.Not.Null);
            Assert.That(hero.GetComponent<Rigidbody>().interpolation, Is.EqualTo(RigidbodyInterpolation.Interpolate));
            Assert.That(hero.GetComponent<CapsuleCollider>().sharedMaterial.dynamicFriction, Is.Zero);
            Assert.That(Time.fixedDeltaTime, Is.EqualTo(1f / 60f).Within(0.0001f));

            GameObject gameUi = GameObject.Find("Game UI");
            Assert.That(gameUi.GetComponent("GraphicRaycaster"), Is.Not.Null);
            Assert.That(GameObject.Find("Event System"), Is.Not.Null);

            Transform left = gameUi.transform.Find("Touch Controls/Left");
            Assert.That(left, Is.Not.Null);
            Assert.That(left.GetComponent("TouchControlVisual"), Is.Not.Null);
            Assert.That(left.Find("Control Shadow"), Is.Not.Null);
            Assert.That(left.Find("Iron Frame"), Is.Not.Null);
            Assert.That(left.Find("Stone Face/Upper Edge"), Is.Not.Null);

            Transform restart = gameUi.transform.Find("Completion/Completion Panel/Restart");
            Assert.That(restart.Find("Button Shadow"), Is.Not.Null);
            Assert.That(restart.Find("Button Face/Button Highlight"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ReleasedJumpUsesAControlledShortArc()
        {
            GameObject hero = GameObject.Find("Hero");
            Rigidbody body = hero.GetComponent<Rigidbody>();
            Component controller = hero.GetComponent("PlayerController");
            Assert.That(body.useGravity, Is.False);

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
            float startHeight = body.position.y;
            controller.GetType()
                .GetMethod("QueueJump", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(controller, null);

            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocity.y, Is.GreaterThan(7.5f));

            float peakHeight = body.position.y;
            for (int i = 0; i < 36 && body.linearVelocity.y > 0f; i++)
            {
                yield return new WaitForFixedUpdate();
                peakHeight = Mathf.Max(peakHeight, body.position.y);
            }

            Assert.That(peakHeight - startHeight, Is.InRange(0.6f, 1.5f));
            for (int i = 0; i < 8; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.linearVelocity.y, Is.LessThan(-2f));
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
            Assert.That(rainObject.GetComponent<ParticleSystem>().shape.scale.x, Is.GreaterThanOrEqualTo(50f));
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
            Assert.That(finish.transform.position, Is.EqualTo(new Vector3(23.5f, 0.75f, 0f)));
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
    }
}
