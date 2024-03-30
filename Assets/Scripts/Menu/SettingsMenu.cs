using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu {
    public class SettingsMenu : MonoBehaviour {
        [SerializeField]
        private Button closeButton;

        private void Start() {
            closeButton.onClick.AddListener(() => Destroy(gameObject));
        }
    }
}
