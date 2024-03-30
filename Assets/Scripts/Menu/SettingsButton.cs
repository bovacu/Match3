using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu {
    public class SettingsButton : MonoBehaviour {
        [SerializeField]
        private Button settingsButton;

        [SerializeField]
        private SettingsMenu settingsMenuPref;

        [SerializeField]
        private RectTransform canvas;

        private void Start() {
            settingsButton.onClick.AddListener(() => Instantiate(settingsMenuPref, canvas));
        }
    }
}
