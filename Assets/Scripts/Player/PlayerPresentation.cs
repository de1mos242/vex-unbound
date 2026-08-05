using System.Collections.Generic;
using UnityEngine;

namespace VexUnbound
{
    public enum LocomotionState
    {
        Idle,
        Run,
        Jump,
        Fall,
        Land,
        Turn
    }

    [DefaultExecutionOrder(50)]
    public sealed class PlayerPresentation : MonoBehaviour
    {
        private const float RunSpeed = 6f;

        private readonly Dictionary<string, Transform> bones = new();
        private readonly Dictionary<string, Quaternion> bindRotations = new();
        private Animation authoredAnimation;
        private PlayerController controller;
        private float landUntil;
        private float turnUntil;
        private float lastHorizontalDirection = 1f;
        private bool wasGrounded;

        public Transform Visual { private get; set; }
        public LocomotionState State { get; private set; }

        private void Start()
        {
            controller = GetComponent<PlayerController>();
            CacheBones(Visual);
            CreateAuthoredClips();
            wasGrounded = controller.Grounded;
            SetState(LocomotionState.Idle, 0f);
        }

        private void LateUpdate()
        {
            if (Visual == null)
            {
                return;
            }

            Vector3 velocity = controller.Velocity;
            float direction = Mathf.Sign(velocity.x);
            bool reversing = Mathf.Abs(velocity.x) > 0.25f && direction != lastHorizontalDirection;
            if (Mathf.Abs(velocity.x) > 0.25f)
            {
                lastHorizontalDirection = direction;
            }

            if (reversing && controller.Grounded)
            {
                turnUntil = Time.time + 0.12f;
            }

            LocomotionState nextState;
            if (!controller.Grounded)
            {
                nextState = velocity.y > 0.15f ? LocomotionState.Jump : LocomotionState.Fall;
            }
            else if (!wasGrounded)
            {
                landUntil = Time.time + 0.16f;
                nextState = LocomotionState.Land;
            }
            else if (Time.time < landUntil)
            {
                nextState = LocomotionState.Land;
            }
            else if (Time.time < turnUntil)
            {
                nextState = LocomotionState.Turn;
            }
            else
            {
                nextState = Mathf.Abs(velocity.x) > 0.2f ? LocomotionState.Run : LocomotionState.Idle;
            }

            SetState(nextState, 0.08f);
            if (nextState == LocomotionState.Run)
            {
                authoredAnimation[LocomotionState.Run.ToString()].speed = Mathf.Clamp(Mathf.Abs(velocity.x) / RunSpeed, 0.65f, 1.15f);
            }

            wasGrounded = controller.Grounded;
        }

        private void CacheBones(Transform root)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                bones[child.name] = child;
                bindRotations[child.name] = child.localRotation;
            }
        }

        private void CreateAuthoredClips()
        {
            Animator animator = Visual.GetComponent<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.enabled = false;
            }

            authoredAnimation = Visual.gameObject.AddComponent<Animation>();
            AddClip(LocomotionState.Idle, 2f, true,
                Track("mixamorig:Spine2", new[] { 0f, 1f, 2f }, new[] { Vector3.zero, new Vector3(2f, 0f, 1.5f), Vector3.zero }),
                Track("mixamorig:Head", new[] { 0f, 1f, 2f }, new[] { Vector3.zero, new Vector3(-2f, 1f, 0f), Vector3.zero }));
            AddClip(LocomotionState.Run, 0.52f, true,
                Swing("mixamorig:LeftUpLeg", 34f), Swing("mixamorig:RightUpLeg", -34f),
                Swing("mixamorig:LeftArm", -38f), Swing("mixamorig:RightArm", 38f),
                Track("mixamorig:Spine", new[] { 0f, 0.13f, 0.26f, 0.39f, 0.52f },
                    new[] { Vector3.zero, new Vector3(0f, 0f, 3f), Vector3.zero, new Vector3(0f, 0f, -3f), Vector3.zero }));
            AddClip(LocomotionState.Jump, 0.35f, false,
                Hold("mixamorig:LeftUpLeg", new Vector3(-28f, 0f, 0f), 0.35f),
                Hold("mixamorig:RightUpLeg", new Vector3(18f, 0f, 0f), 0.35f),
                Hold("mixamorig:LeftArm", new Vector3(-42f, 0f, -12f), 0.35f),
                Hold("mixamorig:RightArm", new Vector3(-42f, 0f, 12f), 0.35f));
            AddClip(LocomotionState.Fall, 0.7f, true,
                Hold("mixamorig:LeftUpLeg", new Vector3(12f, 0f, -8f), 0.7f),
                Hold("mixamorig:RightUpLeg", new Vector3(-8f, 0f, 8f), 0.7f),
                Track("mixamorig:LeftArm", new[] { 0f, 0.35f, 0.7f }, new[] { new Vector3(28f, 0f, -18f), new Vector3(34f, 0f, -21f), new Vector3(28f, 0f, -18f) }),
                Track("mixamorig:RightArm", new[] { 0f, 0.35f, 0.7f }, new[] { new Vector3(28f, 0f, 18f), new Vector3(34f, 0f, 21f), new Vector3(28f, 0f, 18f) }));
            AddClip(LocomotionState.Land, 0.22f, false,
                Track("mixamorig:Hips", new[] { 0f, 0.08f, 0.22f }, new[] { Vector3.zero, new Vector3(18f, 0f, 0f), Vector3.zero }),
                Track("mixamorig:LeftUpLeg", new[] { 0f, 0.08f, 0.22f }, new[] { Vector3.zero, new Vector3(-24f, 0f, 0f), Vector3.zero }),
                Track("mixamorig:RightUpLeg", new[] { 0f, 0.08f, 0.22f }, new[] { Vector3.zero, new Vector3(-24f, 0f, 0f), Vector3.zero }));
            AddClip(LocomotionState.Turn, 0.18f, false,
                Track("mixamorig:Hips", new[] { 0f, 0.09f, 0.18f }, new[] { Vector3.zero, new Vector3(0f, 16f, 0f), Vector3.zero }),
                Track("mixamorig:Spine2", new[] { 0f, 0.09f, 0.18f }, new[] { Vector3.zero, new Vector3(0f, -20f, 0f), Vector3.zero }));
        }

        private BoneTrack Swing(string bone, float angle)
        {
            return Track(bone, new[] { 0f, 0.13f, 0.26f, 0.39f, 0.52f },
                new[] { new Vector3(angle, 0f, 0f), Vector3.zero, new Vector3(-angle, 0f, 0f), Vector3.zero, new Vector3(angle, 0f, 0f) });
        }

        private BoneTrack Hold(string bone, Vector3 rotation, float duration)
        {
            return Track(bone, new[] { 0f, duration }, new[] { rotation, rotation });
        }

        private BoneTrack Track(string bone, float[] times, Vector3[] rotations)
        {
            return new BoneTrack(bone, times, rotations);
        }

        private void AddClip(LocomotionState state, float duration, bool loop, params BoneTrack[] tracks)
        {
            AnimationClip clip = new() { name = state.ToString(), legacy = true, wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever };
            foreach (BoneTrack track in tracks)
            {
                if (!bones.TryGetValue(track.Bone, out Transform bone))
                {
                    Debug.LogWarning($"Rig bone '{track.Bone}' is missing; {state} will omit that track.");
                    continue;
                }

                string path = GetPath(Visual, bone);
                Quaternion bind = bindRotations[track.Bone];
                Keyframe[] x = new Keyframe[track.Times.Length];
                Keyframe[] y = new Keyframe[track.Times.Length];
                Keyframe[] z = new Keyframe[track.Times.Length];
                Keyframe[] w = new Keyframe[track.Times.Length];
                for (int i = 0; i < track.Times.Length; i++)
                {
                    Quaternion rotation = bind * Quaternion.Euler(track.Rotations[i]);
                    x[i] = new Keyframe(track.Times[i], rotation.x);
                    y[i] = new Keyframe(track.Times[i], rotation.y);
                    z[i] = new Keyframe(track.Times[i], rotation.z);
                    w[i] = new Keyframe(track.Times[i], rotation.w);
                }

                clip.SetCurve(path, typeof(Transform), "localRotation.x", new AnimationCurve(x));
                clip.SetCurve(path, typeof(Transform), "localRotation.y", new AnimationCurve(y));
                clip.SetCurve(path, typeof(Transform), "localRotation.z", new AnimationCurve(z));
                clip.SetCurve(path, typeof(Transform), "localRotation.w", new AnimationCurve(w));
            }

            clip.EnsureQuaternionContinuity();
            authoredAnimation.AddClip(clip, clip.name);
        }

        private void SetState(LocomotionState state, float fadeLength)
        {
            if (State == state && authoredAnimation.isPlaying)
            {
                return;
            }

            State = state;
            authoredAnimation.CrossFade(state.ToString(), fadeLength);
        }

        private static string GetPath(Transform root, Transform child)
        {
            string path = child.name;
            while (child.parent != root)
            {
                child = child.parent;
                path = child.name + "/" + path;
            }

            return path;
        }

        private readonly struct BoneTrack
        {
            public BoneTrack(string bone, float[] times, Vector3[] rotations)
            {
                Bone = bone;
                Times = times;
                Rotations = rotations;
            }

            public string Bone { get; }
            public float[] Times { get; }
            public Vector3[] Rotations { get; }
        }
    }
}
