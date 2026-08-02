using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace VexUnbound
{
    public sealed class GameplayBootstrap : MonoBehaviour
    {
        private static readonly Color PlayerColor = new(0.9f, 0.06f, 0.08f);
        private static readonly Color PlatformColor = new(0.18f, 0.22f, 0.27f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateLevel()
        {
            if (FindFirstObjectByType<BallController>() != null)
            {
                return;
            }

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Platform";
            platform.transform.SetPositionAndRotation(new Vector3(0f, -1f, 0f), Quaternion.identity);
            platform.transform.localScale = new Vector3(18f, 1f, 3f);
            platform.GetComponent<Renderer>().material.color = PlatformColor;

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            player.name = "Hero";
            player.transform.position = new Vector3(0f, 0.1f, 0f);
            player.GetComponent<Renderer>().material.color = PlayerColor;

            Rigidbody body = player.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.constraints = RigidbodyConstraints.FreezePositionZ |
                               RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationY;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            player.AddComponent<BallController>();

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0f, 2f, -10f);
                camera.backgroundColor = new Color(0.08f, 0.12f, 0.18f);
                camera.gameObject.AddComponent<FollowHero>().Target = player.transform;
            }

            CreateTouchControls();
        }

        private static void CreateTouchControls()
        {
            GameObject canvasObject = new("Touch Controls", typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            CreateControl(canvas.transform, "Left", "<", new Vector2(40f, 40f), new Vector2(260f, 260f));
            CreateControl(canvas.transform, "Right", ">", new Vector2(320f, 40f), new Vector2(260f, 260f));
            CreateControl(canvas.transform, "Jump", "JUMP", new Vector2(-40f, 40f), new Vector2(300f, 260f), true);
        }

        private static void CreateControl(
            Transform parent,
            string name,
            string label,
            Vector2 position,
            Vector2 size,
            bool anchorRight = false)
        {
            GameObject control = new(name, typeof(RectTransform), typeof(Image));
            control.transform.SetParent(parent, false);

            RectTransform rect = control.GetComponent<RectTransform>();
            rect.anchorMin = anchorRight ? Vector2.right : Vector2.zero;
            rect.anchorMax = rect.anchorMin;
            rect.pivot = anchorRight ? Vector2.right : Vector2.zero;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = control.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.18f);
            image.raycastTarget = false;

            GameObject textObject = new("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(control.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 64;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 1f, 1f, 0.8f);
            text.raycastTarget = false;
        }
    }

    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class BallController : MonoBehaviour
    {
        private const float MoveSpeed = 6f;
        private const float JumpImpulse = 7f;

        private readonly RaycastHit[] groundHits = new RaycastHit[4];
        private Rigidbody body;
        private SphereCollider sphereCollider;
        private float movement;
        private bool jumpQueued;
        private bool touchJumpHeld;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            sphereCollider = GetComponent<SphereCollider>();
        }

        private void Update()
        {
            ReadInput();
        }

        private void FixedUpdate()
        {
            Vector3 velocity = body.linearVelocity;
            velocity.x = movement * MoveSpeed;
            body.linearVelocity = velocity;

            if (jumpQueued && IsGrounded())
            {
                body.AddForce(Vector3.up * JumpImpulse, ForceMode.Impulse);
            }

            jumpQueued = false;
        }

        private void ReadInput()
        {
            movement = 0f;
            bool jumpHeld = false;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    movement -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    movement += 1f;
                }

                jumpQueued |= keyboard.spaceKey.wasPressedThisFrame;
            }

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (TouchControl touch in touchscreen.touches)
                {
                    if (!touch.press.isPressed)
                    {
                        continue;
                    }

                    Vector2 position = touch.position.ReadValue();
                    if (position.x < Screen.width * 0.32f && position.y < Screen.height * 0.42f)
                    {
                        movement = position.x < Screen.width * 0.16f ? -1f : 1f;
                    }
                    else if (position.x > Screen.width * 0.68f && position.y < Screen.height * 0.42f)
                    {
                        jumpHeld = true;
                    }
                }
            }

            jumpQueued |= jumpHeld && !touchJumpHeld;
            touchJumpHeld = jumpHeld;
        }

        private bool IsGrounded()
        {
            int hitCount = Physics.RaycastNonAlloc(
                transform.position,
                Vector3.down,
                groundHits,
                sphereCollider.radius * transform.lossyScale.y + 0.15f);

            for (int i = 0; i < hitCount; i++)
            {
                if (groundHits[i].collider != sphereCollider)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class FollowHero : MonoBehaviour
    {
        public Transform Target { private get; set; }

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            Vector3 position = transform.position;
            position.x = Target.position.x;
            transform.position = position;
        }
    }
}
