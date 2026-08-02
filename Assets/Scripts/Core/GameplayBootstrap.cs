using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VexUnbound
{
    public sealed class GameplayBootstrap : MonoBehaviour
    {
        private static readonly Color PlayerColor = new(0.2f, 0.46f, 0.24f);
        private static readonly Color SkinColor = new(0.95f, 0.72f, 0.55f);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CreateLevel();
        }

        private static void CreateLevel()
        {
            if (FindFirstObjectByType<PlayerController>() != null)
            {
                return;
            }

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            CreatePlatform("Start Platform", new Vector3(-4f, -1f, 0f), new Vector3(8f, 1f, 3f));
            CreatePlatform("Middle Platform", new Vector3(2f, -0.65f, 0f), new Vector3(3f, 1f, 3f));
            CreatePlatform("Lower Rampart", new Vector3(7f, -1f, 0f), new Vector3(6f, 1f, 3f));
            CreatePlatform("Tower Approach", new Vector3(12.25f, -0.45f, 0f), new Vector3(3.5f, 1f, 3f));
            CreatePlatform("Broken Causeway", new Vector3(16.5f, -0.9f, 0f), new Vector3(4f, 1f, 3f));
            CreatePlatform("Finish Platform", new Vector3(22f, -1f, 0f), new Vector3(6f, 1f, 3f));

            GameObject player = CreatePlayer();
            GameSession session = CreateInterface();
            player.GetComponent<PlayerController>().Session = session;
            CreateFinish(new Vector3(23.5f, 0.75f, 0f), session);

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 3.5f;
                camera.transform.position = new Vector3(player.transform.position.x, 1.6f, -10f);
                camera.gameObject.AddComponent<FollowHero>().Target = player.transform;
                GothicFortressEnvironment.ConfigureScene(camera);
            }
        }

        private static void CreatePlatform(string name, Vector3 position, Vector3 scale)
        {
            GothicFortressEnvironment.CreatePlatform(name, position, scale);
        }

        private static GameObject CreatePlayer()
        {
            GameObject player = new("Hero");
            player.transform.position = new Vector3(-7f, -0.48f, 0f);

            CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.9f, 0f);
            collider.height = 1.8f;
            collider.radius = 0.34f;

            Rigidbody body = player.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            CreatePlayerVisual(player.transform);

            player.AddComponent<PlayerController>();
            return player;
        }

        private static void CreatePlayerVisual(Transform player)
        {
            GameObject character = Resources.Load<GameObject>("Characters/CrownedMarionette");
            if (character == null)
            {
                Debug.LogError("CrownedMarionette character asset is missing; using placeholder visuals.");
                CreateBodyPart(player, "Body", PrimitiveType.Capsule, new Vector3(0f, 0.72f, 0f), new Vector3(0.58f, 0.58f, 0.42f), PlayerColor);
                CreateBodyPart(player, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.55f, 0f), Vector3.one * 0.5f, SkinColor);
                CreateBodyPart(player, "Left Arm", PrimitiveType.Capsule, new Vector3(-0.42f, 0.78f, 0f), new Vector3(0.18f, 0.45f, 0.18f), SkinColor);
                CreateBodyPart(player, "Right Arm", PrimitiveType.Capsule, new Vector3(0.42f, 0.78f, 0f), new Vector3(0.18f, 0.45f, 0.18f), SkinColor);
                return;
            }

            GameObject visual = Instantiate(character, player);
            visual.name = "Crowned Marionette Visual";
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            visual.transform.localScale = Vector3.one * 0.95f;
        }

        private static void CreateBodyPart(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().material = CreateMaterial(color);
            Destroy(part.GetComponent<Collider>());
        }

        private static Material CreateMaterial(Color color)
        {
            Material template = Resources.Load<Material>("Materials/RuntimeUnlit");
            if (template == null)
            {
                Debug.LogError("RuntimeUnlit material is missing from Resources.");
                return new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            }

            Material material = new(template);
            material.SetColor("_BaseColor", color);
            return material;
        }

        private static void CreateFinish(Vector3 position, GameSession session)
        {
            GameObject finish = new("Finish");
            finish.transform.position = position;

            BoxCollider trigger = finish.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.2f, 2.5f, 2.5f);
            finish.AddComponent<FinishGoal>().Session = session;

            GothicFortressEnvironment.CreateFinishVisual(finish.transform);
        }

        private static GameSession CreateInterface()
        {
            GameObject canvasObject = new("Game UI", typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            GameObject controls = new("Touch Controls", typeof(RectTransform));
            controls.transform.SetParent(canvas.transform, false);
            StretchToParent(controls.GetComponent<RectTransform>());
            CreateControl(controls.transform, "Left", "<", new Vector2(40f, 40f), new Vector2(260f, 260f));
            CreateControl(controls.transform, "Right", ">", new Vector2(320f, 40f), new Vector2(260f, 260f));
            CreateControl(controls.transform, "Jump", "JUMP", new Vector2(-40f, 40f), new Vector2(300f, 260f), true);

            GameObject completion = new("Completion", typeof(RectTransform), typeof(Image));
            completion.transform.SetParent(canvas.transform, false);
            StretchToParent(completion.GetComponent<RectTransform>());
            completion.GetComponent<Image>().color = new Color(0.03f, 0.05f, 0.08f, 0.9f);

            Text completionTitle = CreateText(completion.transform, "Level Complete", new Vector2(0f, 120f), new Vector2(900f, 180f), 88);

            GameObject buttonObject = new("Restart", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(completion.transform, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, -100f);
            buttonRect.sizeDelta = new Vector2(520f, 150f);
            buttonObject.GetComponent<Image>().color = PlayerColor;
            CreateText(buttonObject.transform, "RESTART", Vector2.zero, Vector2.zero, 58, true);

            GameSession session = canvasObject.AddComponent<GameSession>();
            session.Configure(controls, completion, completionTitle, buttonObject.GetComponent<Button>());
            completion.SetActive(false);
            return session;
        }

        private static void CreateControl(Transform parent, string name, string label, Vector2 position, Vector2 size, bool anchorRight = false)
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
            CreateText(control.transform, label, Vector2.zero, Vector2.zero, 64, true);
        }

        private static Text CreateText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, bool stretch = false)
        {
            GameObject textObject = new("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            if (stretch)
            {
                StretchToParent(rect);
            }
            else
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }

            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class PlayerController : MonoBehaviour
    {
        private const float MoveSpeed = 6f;
        private const float JumpImpulse = 7f;

        private readonly RaycastHit[] groundHits = new RaycastHit[4];
        private Rigidbody body;
        private CapsuleCollider capsuleCollider;
        private float movement;
        private bool jumpQueued;
        private bool touchJumpHeld;

        public GameSession Session { private get; set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void Update()
        {
            if (transform.position.y < -4f)
            {
                Session.FailLevel(this);
                return;
            }

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

        public void Stop()
        {
            enabled = false;
            body.linearVelocity = Vector3.zero;
            body.constraints = RigidbodyConstraints.FreezeAll;
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
                capsuleCollider.bounds.center,
                Vector3.down,
                groundHits,
                capsuleCollider.bounds.extents.y + 0.15f);

            for (int i = 0; i < hitCount; i++)
            {
                if (groundHits[i].collider != capsuleCollider)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class FinishGoal : MonoBehaviour
    {
        public GameSession Session { private get; set; }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Session.CompleteLevel(player);
            }
        }
    }

    public sealed class GameSession : MonoBehaviour
    {
        private GameObject controls;
        private GameObject completion;
        private Text completionTitle;
        private Button restartButton;
        private bool completed;

        public void Configure(GameObject touchControls, GameObject completionScreen, Text title, Button restart)
        {
            controls = touchControls;
            completion = completionScreen;
            completionTitle = title;
            restartButton = restart;
            restartButton.onClick.AddListener(Restart);
        }

        public void CompleteLevel(PlayerController player)
        {
            if (completed)
            {
                return;
            }

            ShowEndScreen(player, "Level Complete");
        }

        public void FailLevel(PlayerController player)
        {
            if (completed)
            {
                return;
            }

            ShowEndScreen(player, "Try Again");
        }

        private void ShowEndScreen(PlayerController player, string title)
        {
            completed = true;
            player.Stop();
            controls.SetActive(false);
            completionTitle.text = title;
            completion.SetActive(true);
        }

        private void Update()
        {
            if (!completed)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            {
                restartButton.onClick.Invoke();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                TryRestart(mouse.position.ReadValue());
            }

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (TouchControl touch in touchscreen.touches)
                {
                    if (touch.press.wasPressedThisFrame)
                    {
                        TryRestart(touch.position.ReadValue());
                    }
                }
            }
        }

        private void TryRestart(Vector2 screenPosition)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    restartButton.GetComponent<RectTransform>(), screenPosition))
            {
                restartButton.onClick.Invoke();
            }
        }

        private static void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    public sealed class ParallaxLayer : MonoBehaviour
    {
        public Transform Target { private get; set; }
        public float Factor { private get; set; }

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            Vector3 position = transform.position;
            position.x = Target.position.x * Factor;
            transform.position = position;
        }
    }
}
