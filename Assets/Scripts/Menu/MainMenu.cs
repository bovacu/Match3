using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.Menu {
    [Serializable]
    public class ButtonToTime {
        public Button button;
        public int timeInSeconds;
    }

    public class MainMenu : MonoBehaviour {
        [SerializeField]
        ButtonToTime[] timeButtons;

        [SerializeField]
        private Button playButton;

        [SerializeField]
        private Button exitButton;

        private ConfigHolder configHolder;
        private SoundSystem soundSystem;

        private void Start() {
            configHolder = FindObjectOfType<ConfigHolder>();
            soundSystem = FindObjectOfType<SoundSystem>();

            soundSystem.PlayBackgroundMusic(soundSystem.MUSIC_MENU);

            exitButton.onClick.AddListener(Application.Quit);

            playButton.onClick.AddListener(() => {
                bool active = timeButtons[0].button.gameObject.activeInHierarchy;
                foreach(ButtonToTime b in timeButtons) {
                    b.button.gameObject.SetActive(!active);
                }
            });

            foreach (ButtonToTime b in timeButtons) {
                string timeUnit = b.timeInSeconds < 60 ? "seconds" : (b.timeInSeconds < 3600 ? "minutes" : "hours");
                int totalTime = b.timeInSeconds < 60 ? b.timeInSeconds : (b.timeInSeconds < 3600 ? b.timeInSeconds / 60 : b.timeInSeconds / 3600);

                b.button.gameObject.SetActive(true);
                b.button.GetComponentInChildren<TMP_Text>().text = $"{totalTime} {timeUnit}";
                b.button.gameObject.SetActive(false);

                b.button.onClick.AddListener(() => {
                    b.timeInSeconds = Math.Clamp(b.timeInSeconds, configHolder.minTimeInSecondsAllowedInGame, configHolder.maxTimeInSecondsAllowedInGame);
                    configHolder.gameTimeInSeconds = b.timeInSeconds;
                    soundSystem.StopBackgroundMusic();
                    SceneManager.LoadScene("Game");
                });
            }
        }

        void OnValidate() {
            if (timeButtons.Length > 2 || timeButtons.Length < 1) {
                Debug.LogWarning("Maximum of 2 modes and a minimum of 1 is allowed for this demo");
                Array.Resize(ref timeButtons, 2);
            }
        }
    }
}
