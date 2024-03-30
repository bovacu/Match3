using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Game;

namespace Scripts {

    public class GameStatistics {
        public int totalScore;
        public Dictionary<OrbType, int> amountOfOrbsDestroyed;

        public GameStatistics() {
            amountOfOrbsDestroyed = new Dictionary<OrbType, int>();
            totalScore = 0;
        }
    }

    public class ConfigHolder : MonoBehaviour {
        [SerializeField]
        public int minTimeInSecondsAllowedInGame = 15;

        [SerializeField]
        public int maxTimeInSecondsAllowedInGame = 3600;

        [Tooltip("This sets the seed for RNG, if -1 is set, then a new random seed will be used each time")]
        [SerializeField]
        public int randomSeed = -1;

        [NonSerialized]
        public int gameTimeInSeconds;

        [NonSerialized]
        public GameStatistics gameStatistics;

        private static ConfigHolder selfRef = null;

        private void MakeUnique() {
            DontDestroyOnLoad(gameObject);

            if (selfRef == null) {
                selfRef = this;
            } else {
                Destroy(gameObject);
            }
        }

        private void Awake() {
            MakeUnique();
            ResetStatistics();
        }

        public void ResetStatistics() {
            gameStatistics = new GameStatistics();
        }
    }
}
