using Scripts;
using Scripts.Game;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Game {

    public class ScoreSystem : MonoBehaviour {
        [SerializeField]
        private int pointsForMatch3 = 30;

        [SerializeField]
        private int pointsForMatch4 = 50;

        [SerializeField]
        private int pointsForMatch5 = 70;

        [SerializeField]
        private TMP_Text scoreText;
        [SerializeField]
        private RectTransform scoreTextRectTransform;

        [SerializeField]
        private TMP_Text newScorePref;

        [SerializeField]
        private RectTransform canvas;

        private Pool<TMP_Text> scoreTextPool;
        private ConfigHolder configHolder;

        private int slowedScore = 0;

        private void Start() {
            configHolder = FindObjectOfType<ConfigHolder>();

            scoreTextPool = new Pool<TMP_Text>(15, () => {
                TMP_Text t = Instantiate(newScorePref, transform);
                t.gameObject.SetActive(false);
                return t;
            });
        }

        public IEnumerator Score(int amountOfOrbs, OrbType type, RectTransform originOfMatch) {
            int points = amountOfOrbs switch {
                3 => pointsForMatch3,
                4 => pointsForMatch4,
                5 => pointsForMatch5,
                _ => pointsForMatch5
            };

            configHolder.gameStatistics.totalScore += points;
            
            if(!configHolder.gameStatistics.amountOfOrbsDestroyed.ContainsKey(type)) {
                configHolder.gameStatistics.amountOfOrbsDestroyed[type] = amountOfOrbs;
            } else {
                configHolder.gameStatistics.amountOfOrbsDestroyed[type] += amountOfOrbs;
            }

            TMP_Text t = scoreTextPool.Get();
            t.gameObject.SetActive(true);
            t.text = $"+{points}";

            RectTransform rt = GetComponent<RectTransform>();
            yield return Util.TweenPosition(t.GetComponent<RectTransform>(), originOfMatch.position, rt.position, 1.5f);
            t.gameObject.SetActive(false);

            slowedScore += points;
            scoreText.text = $"Score: {slowedScore}";
            yield return Util.TweenScale(scoreTextRectTransform, new Vector2(1.0f, 1.0f), new Vector2(1.25f, 1.25f), 0.15f);
            yield return Util.TweenScale(scoreTextRectTransform, new Vector2(1.25f, 1.25f), new Vector2(1.0f, 1.0f), 0.15f);

            scoreTextPool.Return(t);
        }
    }
}
