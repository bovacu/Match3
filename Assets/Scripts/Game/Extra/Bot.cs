using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Extra {
    public class Bot : MonoBehaviour {
        [SerializeField]
        private GridSystem gridSystem;

        [SerializeField]
        private TimerSystem timeSystem;

        [SerializeField]
        private uint secondsBetweenMatches = 0;

        private bool runBot;

        public bool RunBot { 
            get {
                return runBot;
            }

            set {
                runBot = value;
                if(!runBot) {
                    gridSystem.BlockInput(false);
                }
            }        
        }

        private IEnumerator run() {
            List<PossibleOrbMovements> possibleMovements = gridSystem.Core.GetPossibleMovementsOrigins();
            PossibleOrbMovements move = possibleMovements[UnityEngine.Random.Range(0, possibleMovements.Count)];
            yield return StartCoroutine(gridSystem.OnSwapped(move.origin, move.directions[UnityEngine.Random.Range(0, move.directions.Count)]));

            gridSystem.BlockInput(true);
            if(secondsBetweenMatches > 0) {
                yield return new WaitForSecondsRealtime(secondsBetweenMatches);
            }
            gridSystem.BlockInput(false);
        }

        private void Update() {
            if(!runBot || !timeSystem.IsRunning) {
                return;
            }

            if(!gridSystem.IsInputBlocked()) {
                StartCoroutine(run());
            }
        }
    }
}
