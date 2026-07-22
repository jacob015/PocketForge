using System.Collections;
using PocketForge.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace PocketForge.Presentation
{
    public sealed class PositiveFeedbackBurst : MonoBehaviour
    {
        private const int ParticleCount = 8;
        private readonly RectTransform[] particles = new RectTransform[ParticleCount];
        private readonly Image[] images = new Image[ParticleCount];
        private RectTransform root;
        private Coroutine animation;

        public void Initialize(RectTransform effectRoot)
        {
            root = effectRoot;
            for (var index = 0; index < ParticleCount; index++)
            {
                var particle = new GameObject($"SuccessSpark{index + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                particle.transform.SetParent(root, false);
                var rect = particle.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(index % 2 == 0 ? 20f : 13f, index % 2 == 0 ? 20f : 13f);
                rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
                var image = particle.GetComponent<Image>();
                image.raycastTarget = false;
                image.color = Color.clear;
                particle.SetActive(false);
                particles[index] = rect;
                images[index] = image;
            }
        }

        public void Play(RectTransform target, Color accent)
        {
            if (root == null || target == null || GameSettingsService.ReduceMotion)
            {
                return;
            }

            if (animation != null)
            {
                StopCoroutine(animation);
            }

            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPoint, null, out var origin);
            animation = StartCoroutine(PlayRoutine(origin, accent));
        }

        private IEnumerator PlayRoutine(Vector2 origin, Color accent)
        {
            for (var index = 0; index < ParticleCount; index++)
            {
                particles[index].gameObject.SetActive(true);
                particles[index].anchoredPosition = origin;
                particles[index].localScale = Vector3.one * 0.35f;
                images[index].color = new Color(accent.r, accent.g, accent.b, 0f);
            }

            var elapsed = 0f;
            const float duration = 0.46f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var alpha = Mathf.Sin(t * Mathf.PI);
                for (var index = 0; index < ParticleCount; index++)
                {
                    var angle = (index / (float)ParticleCount) * Mathf.PI * 2f;
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var distance = Mathf.Lerp(22f, 118f + (index % 2) * 20f, t);
                    particles[index].anchoredPosition = origin + direction * distance + Vector2.up * (24f * t);
                    particles[index].localScale = Vector3.one * Mathf.Lerp(0.35f, 1f, alpha);
                    images[index].color = new Color(accent.r, accent.g, accent.b, alpha * 0.9f);
                }

                yield return null;
            }

            for (var index = 0; index < ParticleCount; index++)
            {
                particles[index].gameObject.SetActive(false);
            }

            animation = null;
        }
    }
}
