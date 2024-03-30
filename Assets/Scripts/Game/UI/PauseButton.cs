using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Game.UI {

    [RequireComponent(typeof(Button))]
    public class PauseButton : MonoBehaviour {
        [SerializeField]
        private GameObject pauseMenuPrefab;

        [SerializeField]
        private TimerSystem timer;

        [SerializeField]
        private RectTransform canvasTransform;

        [SerializeField]
        private Button button;

        private void Start() {
            button.onClick.AddListener(() => {
                timer.PauseTimer(true);
                Instantiate(pauseMenuPrefab, canvasTransform);
            });
        }
    }
}
