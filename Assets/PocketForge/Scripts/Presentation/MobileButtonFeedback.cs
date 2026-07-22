using System.Collections;
using PocketForge.Audio;
using PocketForge.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PocketForge.Presentation
{
    [RequireComponent(typeof(Button))]
    public sealed class MobileButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private RectTransform rectTransform;
        private Button button;
        private Outline outline;
        private Vector3 restingScale;
        private Coroutine motion;
        private Coroutine celebration;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            button = GetComponent<Button>();
            outline = GetComponent<Outline>();
            restingScale = rectTransform.localScale;
            button.onClick.AddListener(PlayClickSound);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClickSound);
            }
        }

        private void OnDisable()
        {
            if (rectTransform != null)
            {
                rectTransform.localScale = restingScale;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button == null || !button.interactable)
            {
                return;
            }

            AnimateTo(restingScale * 0.96f, 0.055f);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void OnPointerExit(PointerEventData eventData) => Release();

        public void Celebrate(Color accent)
        {
            if (celebration != null)
            {
                StopCoroutine(celebration);
            }

            celebration = StartCoroutine(CelebrateRoutine(accent));
        }

        private void Release()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (motion != null)
            {
                StopCoroutine(motion);
            }

            motion = StartCoroutine(ReleaseRoutine());
        }

        private void AnimateTo(Vector3 target, float duration)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (motion != null)
            {
                StopCoroutine(motion);
            }

            if (GameSettingsService.ReduceMotion)
            {
                rectTransform.localScale = target;
                return;
            }

            motion = StartCoroutine(ScaleRoutine(target, duration));
        }

        private IEnumerator ReleaseRoutine()
        {
            if (GameSettingsService.ReduceMotion)
            {
                rectTransform.localScale = restingScale;
                yield break;
            }

            yield return ScaleRoutine(restingScale * 1.035f, 0.07f);
            yield return ScaleRoutine(restingScale, 0.09f);
        }

        private IEnumerator ScaleRoutine(Vector3 target, float duration)
        {
            var start = rectTransform.localScale;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                rectTransform.localScale = Vector3.LerpUnclamped(start, target, t);
                yield return null;
            }

            rectTransform.localScale = target;
        }

        private IEnumerator CelebrateRoutine(Color accent)
        {
            if (outline == null || GameSettingsService.ReduceMotion)
            {
                yield break;
            }

            var originalEnabled = outline.enabled;
            var originalColor = outline.effectColor;
            outline.enabled = true;
            var elapsed = 0f;
            const float duration = 0.38f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var pulse = Mathf.Sin(t * Mathf.PI);
                rectTransform.localScale = restingScale * (1f + pulse * 0.055f);
                outline.effectColor = new Color(accent.r, accent.g, accent.b, pulse * 0.82f);
                yield return null;
            }

            rectTransform.localScale = restingScale;
            outline.effectColor = originalColor;
            outline.enabled = originalEnabled;
            celebration = null;
        }

        private static void PlayClickSound() => GameAudioController.Instance?.PlayUiClick();
    }
}
