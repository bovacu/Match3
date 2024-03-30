using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Game.UI {
    public class SoundControl : MonoBehaviour {
        [SerializeField]
        private Slider sfxSlider;

        [SerializeField]
        private Slider musicSlider;

        private SoundSystem soundSystem;

        private void Start() {
            soundSystem = FindObjectOfType<SoundSystem>();
            sfxSlider.value = soundSystem.GetSfxVolume();
            musicSlider.value = soundSystem.GetBackgroundVolume();

            sfxSlider.onValueChanged.AddListener((v) => {
                soundSystem.SetSfxVolume(v);
            });

            musicSlider.onValueChanged.AddListener((v) => {
                soundSystem.SetBackgroundVolume(v);
            });
        }
    }
}
