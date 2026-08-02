using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VexUnbound
{
    public sealed class TouchControlVisual : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private readonly HashSet<int> activePointers = new();
        private RectTransform face;
        private Image faceImage;
        private Vector2 restingPosition;
        private Color restingColor;

        public bool IsPressed => activePointers.Count > 0;

        public void Configure(RectTransform movingFace, Image movingFaceImage)
        {
            face = movingFace;
            faceImage = movingFaceImage;
            restingPosition = face.anchoredPosition;
            restingColor = faceImage.color;
        }

        private void Update()
        {
            if (face == null)
            {
                return;
            }

            float blend = 1f - Mathf.Exp(-20f * Time.unscaledDeltaTime);
            Vector3 targetScale = IsPressed ? Vector3.one * 0.95f : Vector3.one;
            face.localScale = Vector3.Lerp(face.localScale, targetScale, blend);
            face.anchoredPosition = Vector2.Lerp(
                face.anchoredPosition,
                restingPosition + (IsPressed ? Vector2.down * 9f : Vector2.zero),
                blend);
            faceImage.color = Color.Lerp(
                faceImage.color,
                IsPressed ? Color.Lerp(restingColor, Color.white, 0.18f) : restingColor,
                blend);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointers.Add(eventData.pointerId);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            activePointers.Remove(eventData.pointerId);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            activePointers.Remove(eventData.pointerId);
        }

        private void OnDisable()
        {
            activePointers.Clear();
            if (face != null)
            {
                face.localScale = Vector3.one;
                face.anchoredPosition = restingPosition;
                faceImage.color = restingColor;
            }
        }
    }
}
