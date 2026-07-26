using System.Collections;
using PocketForge.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace PocketForge.Presentation
{
    [RequireComponent(typeof(Text), typeof(CanvasGroup))]
    public sealed class CasualFeedbackText : MonoBehaviour
    {
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 restingPosition;
        private Coroutine animation;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnDisable()
        {
            if (animation != null)
            {
                StopCoroutine(animation);
                animation = null;
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = restingPosition;
                rectTransform.localScale = Vector3.one;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        public void Show()
        {
            EnsureInitialized();
            gameObject.SetActive(true);
            if (!Application.isPlaying)
            {
                rectTransform.anchoredPosition = restingPosition;
                rectTransform.localScale = Vector3.one;
                canvasGroup.alpha = 1f;
                return;
            }

            if (animation != null)
            {
                StopCoroutine(animation);
            }

            animation = StartCoroutine(PlayRoutine());
        }

        public void HideImmediate()
        {
            EnsureInitialized();
            if (animation != null)
            {
                StopCoroutine(animation);
                animation = null;
            }

            gameObject.SetActive(false);
        }

        private void EnsureInitialized()
        {
            if (rectTransform != null)
            {
                return;
            }

            rectTransform = (RectTransform)transform;
            canvasGroup = GetComponent<CanvasGroup>();
            restingPosition = rectTransform.anchoredPosition;
        }

        private IEnumerator PlayRoutine()
        {
            rectTransform.anchoredPosition = restingPosition;
            rectTransform.localScale = GameSettingsService.ReduceMotion ? Vector3.one : Vector3.one * 0.72f;
            canvasGroup.alpha = 1f;

            if (!GameSettingsService.ReduceMotion)
            {
                yield return Animate(0.14f, Vector3.one * 1.08f, restingPosition);
                yield return Animate(0.09f, Vector3.one, restingPosition + Vector2.up * 4f);
            }

            yield return new WaitForSecondsRealtime(0.48f);

            if (GameSettingsService.ReduceMotion)
            {
                yield return new WaitForSecondsRealtime(0.32f);
            }
            else
            {
                var elapsed = 0f;
                const float duration = 0.42f;
                var startPosition = rectTransform.anchoredPosition;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    var t = Mathf.Clamp01(elapsed / duration);
                    var eased = 1f - Mathf.Pow(1f - t, 3f);
                    rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, restingPosition + Vector2.up * 44f, eased);
                    canvasGroup.alpha = 1f - t;
                    yield return null;
                }
            }

            animation = null;
            gameObject.SetActive(false);
        }

        private IEnumerator Animate(float duration, Vector3 targetScale, Vector2 targetPosition)
        {
            var startScale = rectTransform.localScale;
            var startPosition = rectTransform.anchoredPosition;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.localScale = Vector3.LerpUnclamped(startScale, targetScale, eased);
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, eased);
                yield return null;
            }

            rectTransform.localScale = targetScale;
            rectTransform.anchoredPosition = targetPosition;
        }
    }
}
