using UnityEngine;

namespace VexUnbound
{
    [DefaultExecutionOrder(50)]
    public sealed class PlayerPresentation : MonoBehaviour
    {
        private Rigidbody body;
        private PlayerController controller;
        private Vector3 restingPosition;
        private Quaternion restingRotation;
        private Vector3 restingScale;
        private float stridePhase;
        private float landingSquash;
        private bool wasGrounded;

        public Transform Visual { private get; set; }

        private void Start()
        {
            body = GetComponent<Rigidbody>();
            controller = GetComponent<PlayerController>();
            restingPosition = Visual.localPosition;
            restingRotation = Visual.localRotation;
            restingScale = Visual.localScale;
            wasGrounded = controller.Grounded;
        }

        private void LateUpdate()
        {
            if (Visual == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            Vector3 velocity = body.linearVelocity;
            float runAmount = Mathf.Clamp01(Mathf.Abs(velocity.x) / 6f);
            stridePhase += Mathf.Abs(velocity.x) * deltaTime * 1.8f;

            if (controller.Grounded && !wasGrounded && velocity.y <= 0.5f)
            {
                landingSquash = 0.14f;
            }

            wasGrounded = controller.Grounded;
            landingSquash = Mathf.MoveTowards(landingSquash, 0f, deltaTime * 0.9f);

            float idleBreath = Mathf.Sin(Time.time * 2.2f) * 0.012f;
            float runBob = controller.Grounded ? Mathf.Abs(Mathf.Sin(stridePhase)) * 0.055f * runAmount : 0f;
            float airStretch = controller.Grounded ? 0f : Mathf.Clamp(velocity.y * 0.012f, -0.04f, 0.07f);
            float lean = Mathf.Clamp(-velocity.x * 0.75f, -5f, 5f);
            float stepRoll = controller.Grounded ? Mathf.Sin(stridePhase) * 1.8f * runAmount : 0f;

            Vector3 targetPosition = restingPosition + Vector3.up * (idleBreath + runBob - landingSquash * 0.18f);
            Vector3 targetScale = new(
                restingScale.x * (1f + landingSquash * 0.34f - airStretch * 0.2f),
                restingScale.y * (1f - landingSquash + airStretch),
                restingScale.z * (1f + landingSquash * 0.34f - airStretch * 0.2f));
            Quaternion targetRotation = restingRotation * Quaternion.Euler(0f, 0f, lean + stepRoll);

            float blend = 1f - Mathf.Exp(-14f * deltaTime);
            Visual.localPosition = Vector3.Lerp(Visual.localPosition, targetPosition, blend);
            Visual.localScale = Vector3.Lerp(Visual.localScale, targetScale, blend);
            Visual.localRotation = Quaternion.Slerp(Visual.localRotation, targetRotation, blend);
        }
    }
}
