using System.Collections;
using UnityEngine;

namespace Assets.Scripts {
    public static class Util {

        private static float EasyInBack(float t, float b, float c, float d, float s = 1.70158f) {
            return c * (t /= d) * t * ((s + 1) * t - s) + b;
        }

        public static IEnumerator TweenAnchoredPosition(RectTransform rectTransform, Vector2 start, Vector2 end, float duration) {
            float t = 0f;
            while (t < duration) {
                float sc = EasyInBack(t, 0f, 1f, duration);
                rectTransform.anchoredPosition = Vector3.LerpUnclamped(start, end, sc);

                yield return null;
                t += Time.deltaTime;
            }

            rectTransform.anchoredPosition = end;
        }

        public static IEnumerator TweenPosition(Transform transform, Vector2 start, Vector2 end, float duration) {
            float t = 0f;
            while (t < duration) {
                float sc = EasyInBack(t, 0f, 1f, duration);
                transform.position = Vector3.LerpUnclamped(start, end, sc);

                yield return null;
                t += Time.deltaTime;
            }

            transform.position = end;
        }

        public static IEnumerator TweenScale(Transform transform, Vector2 start, Vector2 end, float duration) {
            float t = 0f;
            while (t < duration) {
                float sc = EasyInBack(t, 0f, 1f, duration);
                transform.localScale = Vector3.LerpUnclamped(start, end, sc);

                yield return null;
                t += Time.deltaTime;
            }

            transform.localScale = end;
        }
    }
}
