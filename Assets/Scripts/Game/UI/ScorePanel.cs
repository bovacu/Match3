using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scripts.Game.UI {
    public class ScorePanel : MonoBehaviour {
        [SerializeField]
        private Button quitButton;

        [SerializeField]
        private Button restartButton;

        [SerializeField]
        private ScoreLine scoreLinePref;

        [SerializeField]
        private Transform scoreLineParent;

        [SerializeField]
        private OrbsData orbsData;

        private ConfigHolder configHolder;
        private SoundSystem soundSystem;

        private void Start() {
            configHolder = FindObjectOfType<ConfigHolder>();
            soundSystem = FindObjectOfType<SoundSystem>();

            quitButton.onClick.AddListener(() => {
                soundSystem.StopBackgroundMusic();
                SceneManager.LoadScene("MainMenu"); 
            });

            restartButton.onClick.AddListener(() => {
                configHolder.ResetStatistics();
                soundSystem.StopBackgroundMusic();
                SceneManager.LoadScene("Game");
            });

            foreach(KeyValuePair<OrbType, int> entry in configHolder.gameStatistics.amountOfOrbsDestroyed) {
                Instantiate(scoreLinePref, scoreLineParent).Setup(orbsData.GetSprite(entry.Key), entry.Key, entry.Value);
            }
        }
    }
}
