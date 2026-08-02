using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VexUnbound
{
    public sealed class GameplayBootstrap : MonoBehaviour
    {
        private static readonly Color PlayerColor = new(0.2f, 0.46f, 0.24f);
        private static readonly Color SkinColor = new(0.95f, 0.72f, 0.55f);
        private static readonly Color UiIron = new(0.055f, 0.065f, 0.09f, 0.96f);
        private static readonly Color UiStone = new(0.19f, 0.23f, 0.34f, 0.96f);
        private static readonly Color UiGold = new(0.72f, 0.51f, 0.2f, 1f);

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
            Time.fixedDeltaTime = 1f / 60f;

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
            collider.sharedMaterial = new PhysicsMaterial("Frictionless Player")
            {
                staticFriction = 0f,
                dynamicFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0f,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };

            Rigidbody body = player.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            Transform presentationRoot = CreatePlayerVisual(player.transform);
            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerPresentation>().Visual = presentationRoot;
            return player;
        }

        private static Transform CreatePlayerVisual(Transform player)
        {
            Transform presentation = new GameObject("Hero Presentation").transform;
            presentation.SetParent(player, false);

            GameObject character = Resources.Load<GameObject>("Characters/CrownedMarionette");
            if (character == null)
            {
                Debug.LogError("CrownedMarionette character asset is missing; using placeholder visuals.");
                CreateBodyPart(presentation, "Body", PrimitiveType.Capsule, new Vector3(0f, 0.72f, 0f), new Vector3(0.58f, 0.58f, 0.42f), PlayerColor);
                CreateBodyPart(presentation, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.55f, 0f), Vector3.one * 0.5f, SkinColor);
                CreateBodyPart(presentation, "Left Arm", PrimitiveType.Capsule, new Vector3(-0.42f, 0.78f, 0f), new Vector3(0.18f, 0.45f, 0.18f), SkinColor);
                CreateBodyPart(presentation, "Right Arm", PrimitiveType.Capsule, new Vector3(0.42f, 0.78f, 0f), new Vector3(0.18f, 0.45f, 0.18f), SkinColor);
                return presentation;
            }

            GameObject visual = Instantiate(character, presentation);
            visual.name = "Crowned Marionette Visual";
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            visual.transform.localScale = Vector3.one * 0.95f;
            return presentation;
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
            GameObject canvasObject = new("Game UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;
            EnsureEventSystem();

            GameObject controls = new("Touch Controls", typeof(RectTransform));
            controls.transform.SetParent(canvas.transform, false);
            StretchToParent(controls.GetComponent<RectTransform>());
            TouchControlVisual left = CreateControl(controls.transform, "Left", new Vector2(40f, 40f), new Vector2(240f, 240f), false, -1);
            TouchControlVisual right = CreateControl(controls.transform, "Right", new Vector2(300f, 40f), new Vector2(240f, 240f), false, 1);
            TouchControlVisual jump = CreateControl(controls.transform, "Jump", new Vector2(-40f, 40f), new Vector2(300f, 240f), true, 0);

            GameObject completion = new("Completion", typeof(RectTransform), typeof(Image));
            completion.transform.SetParent(canvas.transform, false);
            StretchToParent(completion.GetComponent<RectTransform>());
            completion.GetComponent<Image>().color = new Color(0.015f, 0.02f, 0.045f, 0.78f);

            GameObject panel = new("Completion Panel", typeof(RectTransform));
            panel.transform.SetParent(completion.transform, false);
            ConfigureCenteredRect(panel.GetComponent<RectTransform>(), Vector2.zero, new Vector2(900f, 510f));
            CreateUiLayer(panel.transform, "Panel Shadow", new Vector2(0f, -24f), new Vector2(900f, 500f), new Color(0f, 0f, 0f, 0.62f));
            CreateUiLayer(panel.transform, "Iron Frame", Vector2.zero, new Vector2(900f, 500f), UiIron);
            CreateUiLayer(panel.transform, "Stone Face", new Vector2(0f, 6f), new Vector2(856f, 454f), new Color(0.12f, 0.15f, 0.24f, 0.99f));
            CreateUiLayer(panel.transform, "Gold Header", new Vector2(0f, 151f), new Vector2(780f, 8f), UiGold);

            Text completionTitle = CreateText(panel.transform, "LEVEL COMPLETE", new Vector2(0f, 92f), new Vector2(780f, 130f), 76);
            CreateText(panel.transform, "FORTRESS PASSAGE SECURED", new Vector2(0f, 22f), new Vector2(720f, 70f), 30, false, new Color(0.7f, 0.76f, 0.9f));

            GameObject buttonObject = new("Restart", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(panel.transform, false);
            ConfigureCenteredRect(buttonObject.GetComponent<RectTransform>(), new Vector2(0f, -130f), new Vector2(470f, 130f));
            Image buttonHitArea = buttonObject.GetComponent<Image>();
            buttonHitArea.color = Color.clear;
            buttonHitArea.raycastTarget = true;
            CreateUiLayer(buttonObject.transform, "Button Shadow", new Vector2(0f, -13f), new Vector2(470f, 120f), new Color(0f, 0f, 0f, 0.68f));
            CreateUiLayer(buttonObject.transform, "Button Frame", Vector2.zero, new Vector2(470f, 120f), UiIron);
            Image restartFace = CreateUiLayer(buttonObject.transform, "Button Face", new Vector2(0f, 6f), new Vector2(444f, 94f), new Color(0.28f, 0.18f, 0.13f, 1f));
            CreateUiLayer(restartFace.transform, "Button Highlight", new Vector2(0f, 38f), new Vector2(410f, 5f), UiGold);
            CreateText(restartFace.transform, "RESTART", Vector2.zero, Vector2.zero, 48, true);

            Button restartButton = buttonObject.GetComponent<Button>();
            restartButton.targetGraphic = restartFace;
            restartButton.transition = Selectable.Transition.None;
            buttonObject.AddComponent<TouchControlVisual>().Configure(restartFace.rectTransform, restartFace);

            GameSession session = canvasObject.AddComponent<GameSession>();
            session.Configure(controls, completion, completionTitle, restartButton, left, right, jump);
            completion.SetActive(false);
            return session;
        }

        private static TouchControlVisual CreateControl(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            bool anchorRight,
            int chevronDirection)
        {
            GameObject control = new(name, typeof(RectTransform), typeof(Image));
            control.transform.SetParent(parent, false);

            RectTransform rect = control.GetComponent<RectTransform>();
            rect.anchorMin = anchorRight ? Vector2.right : Vector2.zero;
            rect.anchorMax = rect.anchorMin;
            rect.pivot = anchorRight ? Vector2.right : Vector2.zero;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image hitArea = control.GetComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;

            CreateUiLayer(control.transform, "Control Shadow", new Vector2(0f, -14f), size - new Vector2(4f, 8f), new Color(0f, 0f, 0f, 0.6f));
            CreateUiLayer(control.transform, "Iron Frame", Vector2.zero, size - new Vector2(4f, 8f), UiIron);
            Image face = CreateUiLayer(control.transform, "Stone Face", new Vector2(0f, 7f), size - new Vector2(28f, 32f), UiStone);
            CreateUiLayer(face.transform, "Upper Edge", new Vector2(0f, size.y * 0.5f - 35f), new Vector2(size.x - 64f, 7f), new Color(0.48f, 0.56f, 0.75f, 0.8f));
            CreateUiLayer(face.transform, "Gold Inlay", new Vector2(0f, -size.y * 0.5f + 36f), new Vector2(size.x - 82f, 5f), UiGold);

            if (chevronDirection == 0)
            {
                CreateText(face.transform, "JUMP", Vector2.zero, Vector2.zero, 46, true);
            }
            else
            {
                CreateChevron(face.transform, chevronDirection);
            }

            TouchControlVisual visual = control.AddComponent<TouchControlVisual>();
            visual.Configure(face.rectTransform, face);
            return visual;
        }

        private static Text CreateText(
            Transform parent,
            string value,
            Vector2 position,
            Vector2 size,
            int fontSize,
            bool stretch = false,
            Color? color = null)
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
            text.color = color ?? new Color(0.92f, 0.94f, 1f);
            text.raycastTarget = false;
            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(3f, -4f);
            return text;
        }

        private static Image CreateUiLayer(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject layer = new(name, typeof(RectTransform), typeof(Image));
            layer.transform.SetParent(parent, false);
            ConfigureCenteredRect(layer.GetComponent<RectTransform>(), position, size);
            Image image = layer.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void CreateChevron(Transform parent, int direction)
        {
            float topRotation = direction < 0 ? 45f : -45f;
            float bottomRotation = -topRotation;
            Image top = CreateUiLayer(parent, "Chevron Top", new Vector2(0f, 22f), new Vector2(18f, 76f), UiGold);
            Image bottom = CreateUiLayer(parent, "Chevron Bottom", new Vector2(0f, -22f), new Vector2(18f, 76f), UiGold);
            top.rectTransform.localRotation = Quaternion.Euler(0f, 0f, topRotation);
            bottom.rectTransform.localRotation = Quaternion.Euler(0f, 0f, bottomRotation);
        }

        private static void ConfigureCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
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
        private const float JumpSpeed = 8.2f;
        private const float GroundAcceleration = 42f;
        private const float GroundDeceleration = 52f;
        private const float GroundTurnAcceleration = 70f;
        private const float AirAcceleration = 16f;
        private const float AirDeceleration = 4f;
        private const float AirTurnAcceleration = 26f;
        private const float RiseGravity = 18f;
        private const float ApexGravity = 14f;
        private const float JumpCutGravity = 36f;
        private const float FallGravity = 26f;
        private const float TerminalFallSpeed = 18f;
        private const float GroundStickSpeed = 2f;
        private const float CoyoteTime = 0.11f;
        private const float JumpBufferTime = 0.12f;

        private readonly RaycastHit[] groundHits = new RaycastHit[8];
        private Rigidbody body;
        private CapsuleCollider capsuleCollider;
        private float movement;
        private float jumpQueuedUntil = float.NegativeInfinity;
        private float lastGroundedTime = float.NegativeInfinity;
        private bool jumpHeld;
        private bool touchJumpHeld;

        public GameSession Session { private get; set; }
        public bool Grounded { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            body.useGravity = false;
            body.linearDamping = 0f;
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
            Grounded = IsGrounded();
            if (Grounded)
            {
                lastGroundedTime = Time.time;
            }

            Vector3 velocity = body.linearVelocity;
            float targetSpeed = movement * MoveSpeed;
            float speedChange = GetHorizontalAcceleration(velocity.x);
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, speedChange * Time.fixedDeltaTime);

            bool jumped = false;
            if (Time.time <= jumpQueuedUntil && Time.time - lastGroundedTime <= CoyoteTime)
            {
                velocity.y = JumpSpeed;
                jumpQueuedUntil = float.NegativeInfinity;
                lastGroundedTime = float.NegativeInfinity;
                Grounded = false;
                jumped = true;
            }

            if (!jumped)
            {
                if (Grounded && velocity.y <= 0f)
                {
                    velocity.y = -GroundStickSpeed;
                }
                else
                {
                    float gravity = GetGravity(velocity.y);
                    velocity.y = Mathf.Max(velocity.y - gravity * Time.fixedDeltaTime, -TerminalFallSpeed);
                }
            }

            body.linearVelocity = velocity;
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
            bool keyboardJumpHeld = false;

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

                if (keyboard.spaceKey.wasPressedThisFrame)
                {
                    QueueJump();
                }

                keyboardJumpHeld = keyboard.spaceKey.isPressed;
            }

            if (Session == null)
            {
                jumpHeld = keyboardJumpHeld;
                return;
            }

            movement += Session.LeftPressed ? -1f : 0f;
            movement += Session.RightPressed ? 1f : 0f;
            movement = Mathf.Clamp(movement, -1f, 1f);

            bool touchJumpIsHeld = Session.JumpPressed;
            if (touchJumpIsHeld && !touchJumpHeld)
            {
                QueueJump();
            }

            touchJumpHeld = touchJumpIsHeld;
            jumpHeld = keyboardJumpHeld || touchJumpIsHeld;
        }

        private float GetHorizontalAcceleration(float currentSpeed)
        {
            bool hasInput = Mathf.Abs(movement) > 0.01f;
            bool reversing = hasInput && Mathf.Abs(currentSpeed) > 0.1f && Mathf.Sign(movement) != Mathf.Sign(currentSpeed);
            if (Grounded)
            {
                return reversing ? GroundTurnAcceleration : hasInput ? GroundAcceleration : GroundDeceleration;
            }

            return reversing ? AirTurnAcceleration : hasInput ? AirAcceleration : AirDeceleration;
        }

        private float GetGravity(float verticalSpeed)
        {
            if (verticalSpeed <= 0f)
            {
                return FallGravity;
            }

            if (!jumpHeld)
            {
                return JumpCutGravity;
            }

            return verticalSpeed < 1.1f ? ApexGravity : RiseGravity;
        }

        private void QueueJump()
        {
            jumpQueuedUntil = Time.time + JumpBufferTime;
        }

        private bool IsGrounded()
        {
            if (body.linearVelocity.y > 0.1f)
            {
                return false;
            }

            float radius = Mathf.Min(capsuleCollider.bounds.extents.x, capsuleCollider.bounds.extents.z) * 0.82f;
            float distance = capsuleCollider.bounds.extents.y - radius + 0.08f;
            int hitCount = Physics.SphereCastNonAlloc(
                capsuleCollider.bounds.center,
                radius,
                Vector3.down,
                groundHits,
                distance,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (groundHits[i].collider != capsuleCollider && groundHits[i].normal.y >= 0.65f)
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
        private TouchControlVisual leftControl;
        private TouchControlVisual rightControl;
        private TouchControlVisual jumpControl;
        private bool completed;

        public bool LeftPressed => leftControl != null && leftControl.IsPressed;
        public bool RightPressed => rightControl != null && rightControl.IsPressed;
        public bool JumpPressed => jumpControl != null && jumpControl.IsPressed;

        public void Configure(
            GameObject touchControls,
            GameObject completionScreen,
            Text title,
            Button restart,
            TouchControlVisual left,
            TouchControlVisual right,
            TouchControlVisual jump)
        {
            controls = touchControls;
            completion = completionScreen;
            completionTitle = title;
            restartButton = restart;
            leftControl = left;
            rightControl = right;
            jumpControl = jump;
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
            completionTitle.text = title.ToUpperInvariant();
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
            }
        }

        private static void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    [DefaultExecutionOrder(100)]
    public sealed class FollowHero : MonoBehaviour
    {
        private float horizontalVelocity;

        public Transform Target { private get; set; }

        private void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            Vector3 position = transform.position;
            position.x = Mathf.SmoothDamp(
                position.x,
                Target.position.x,
                ref horizontalVelocity,
                0.055f,
                Mathf.Infinity,
                Time.deltaTime);
            transform.position = position;
        }
    }

    [DefaultExecutionOrder(200)]
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
