using Assets.Scripts;
using Scripts.Game.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Game {
    public class TimerSystem : MonoBehaviour {
        [SerializeField]
        private GridSystem gridSystem;

        [SerializeField]
        private Slider timerBar;

        [SerializeField]
        private TMP_Text timeText;

        [SerializeField]
        private RectTransform timeRectTransform;

        [SerializeField]
        private Button pausePlayButton;

        [SerializeField]
        private int secondsToStartLowTimeAnimation = 10;

        [SerializeField]
        private ScorePanel scorePanelPref;

        [SerializeField]
        private RectTransform canvas;

        private ConfigHolder configHolder;
        private SoundSystem soundSystem;

        [NonSerialized]
        private bool runTime = true;
        public bool IsRunning => runTime;

        private int secondsRemaining;
        private float second = 0.0f;

        private void Start() {
            configHolder = FindObjectOfType<ConfigHolder>();
            soundSystem = FindObjectOfType<SoundSystem>();

            secondsRemaining = configHolder.gameTimeInSeconds;
            UpdateTimeString();
        }

        private IEnumerator TextAnimation() {
            timeText.color = Color.red;
            yield return Util.TweenScale(timeRectTransform, new Vector2(1.0f, 1.0f), new Vector2(1.25f, 1.25f), 0.5f);
            yield return Util.TweenScale(timeRectTransform, new Vector2(1.25f, 1.25f), new Vector2(1.0f, 1.0f), 0.5f);
        }

        private void UpdateTimeString() {
            TimeSpan t = TimeSpan.FromSeconds(secondsRemaining);
            if(t.Hours == 0) {
                timeText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            } else {
                timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
            }

            if (secondsRemaining <= secondsToStartLowTimeAnimation && secondsRemaining > 0) {
                StartCoroutine(TextAnimation());
            }
        }

        private void UpdateTimeBar() {
            timerBar.value = secondsRemaining / (float)configHolder.gameTimeInSeconds;
        }

        private void Update() {
            if(!runTime) {
                return;
            }

            if(second >= 1.0f) {
                second = 0.0f;
                secondsRemaining--;
                UpdateTimeString();
                UpdateTimeBar();

                if(secondsRemaining <= 0) {
                    gridSystem.SetGameEnded(true);
                    Instantiate(scorePanelPref, canvas);
                    runTime = false;
                    pausePlayButton.interactable = false;
                }
            }

            second += Time.deltaTime;
        }

        public void PauseTimer(bool pause) {
            runTime = !pause;
        }

        private void OnApplicationPause(bool pause) {
            if (FindObjectOfType<PauseMenu>() == null) {
                runTime = !pause;
            }

            if(pause) {
                soundSystem.PauseBackgroundMusic();
            } else {
                soundSystem.PlayBackgroundMusic(soundSystem.MUSIC_GAME);
            }
        }

        void OnApplicationLostFocus() {
            runTime = false;
            soundSystem.PauseBackgroundMusic();
        }

        private void OnApplicationResume() {
            if(FindObjectOfType<PauseMenu>() == null) {
                runTime = true;
            }
            soundSystem.PlayBackgroundMusic(soundSystem.MUSIC_GAME);
        }

        private void OnApplicationFocus(bool focus) {
            if (FindObjectOfType<PauseMenu>() == null) {
                runTime = focus;
            }

            if (!focus) {
                soundSystem.PauseBackgroundMusic();
            } else {
                soundSystem.PlayBackgroundMusic(soundSystem.MUSIC_GAME);
            }
        }
    }
}
