using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Game.UI {
    public class ScoreLine : MonoBehaviour {
        [SerializeField]
        private Image image;

        [SerializeField]
        private TMP_Text text;

        public void Setup(Sprite sprite, OrbType type, int amount) {
            image.sprite = sprite;
            text.text = $"{type}: {amount}";
        }
    }
}
