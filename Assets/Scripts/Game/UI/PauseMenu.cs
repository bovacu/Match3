using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.Game.UI {
    public class PauseMenu : MonoBehaviour {
        [SerializeField]
        private Button resumeButton;

        [SerializeField]
        private Button quitButton;

        private TimerSystem timer;
        private SoundSystem soundSystem;

        private void Start() {
            timer = FindObjectOfType<TimerSystem>();
            soundSystem = FindObjectOfType<SoundSystem>();

            resumeButton.onClick.AddListener(() => {
                timer.PauseTimer(false);
                Destroy(gameObject);
            });

            quitButton.onClick.AddListener(() => {
                soundSystem.StopBackgroundMusic();
                SceneManager.LoadScene("MainMenu");
            });
        }
    }
}
