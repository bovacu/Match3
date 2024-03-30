using Scripts.Game.Extra;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Game.UI {
    public class BotButton : MonoBehaviour {
        [SerializeField]
        private Button runBotButton;

        [SerializeField]
        private TMP_Text buttonText;

        [SerializeField]
        private Bot bot;

        private void Start() {
            runBotButton.onClick.AddListener(() => {
                bot.RunBot = !bot.RunBot;
                buttonText.text = bot.RunBot ? "Disable Automatic" : "Enable Automatic";
            });
        }
    }
}
