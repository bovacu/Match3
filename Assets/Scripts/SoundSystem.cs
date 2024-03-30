using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts {

    [Serializable]
    public class SfxToName {
        public string name;
        public AudioClip audio;
    }

    public class SoundSystem : MonoBehaviour {

        public readonly string AUDIO_WRONG = "comboWrong";
        public readonly string AUDIO_COMBO_3 = "combo3";
        public readonly string AUDIO_COMBO_4 = "combo4";
        public readonly string AUDIO_COMBO_5 = "combo5";
        public readonly string MUSIC_MENU = "mainMenu";
        public readonly string MUSIC_GAME = "game";

        private readonly string SFX_V_KEY = "sfxVolumeKey";
        private readonly string MUSIC_V_KEY = "musicVolumeKey";

        [SerializeField]
        private SfxToName[] audios;

        [SerializeField]
        private AudioSource sfxSource;

        [SerializeField]
        private AudioSource backgroundSource;

        private Dictionary<string, AudioClip> audioDict;
        private Dictionary<string, int> audioAllowedOnceShots;
        private int currentOneShotLimit = -1;

        private static SoundSystem selfRef = null;

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

            if(audioDict == null) {
                audioDict = new Dictionary<string, AudioClip>();
                audioAllowedOnceShots = new Dictionary<string, int>();

                foreach (SfxToName sfx in audios) {
                    if (audioDict.ContainsKey(sfx.name)) {
                        Debug.LogWarning($"Duplicate key for sound {sfx.name}");
                    }
                    audioDict[sfx.name] = sfx.audio;
                }
            }

            if(PlayerPrefs.HasKey(SFX_V_KEY)) {
                sfxSource.volume = PlayerPrefs.GetFloat(SFX_V_KEY);
            }

            if (PlayerPrefs.HasKey(MUSIC_V_KEY)) {
                backgroundSource.volume = PlayerPrefs.GetFloat(MUSIC_V_KEY);
            }
        }

        public AudioClip GetAudio(string name) {
            if(audioDict.ContainsKey(name)) {
                return audioDict[name];
            }

            return null;
        }

        public void StartSfxLimit(uint amount) {
            currentOneShotLimit = (int)amount;
        }

        public void PlaySfx(string name) {
            if (!audioDict.ContainsKey(name)) {
                Debug.LogWarning($"Tried to play an unknow audio '{name}'");
                return;
            }

            if(currentOneShotLimit != -1) {
                if(!audioAllowedOnceShots.ContainsKey(name)) {
                    audioAllowedOnceShots[name] = 1;
                } else {
                    if(audioAllowedOnceShots[name] >= currentOneShotLimit) {
                        return;
                    }
                    audioAllowedOnceShots[name] += 1;
                }


            }

            sfxSource.PlayOneShot(audioDict[name]);
        }

        public void EndSfxLimit() {
            audioAllowedOnceShots.Clear();
            currentOneShotLimit = -1;
        }

        public void PlayBackgroundMusic(string name) {
            if (!audioDict.ContainsKey(name)) {
                Debug.LogWarning($"Tried to play an unknow audio '{name}'");
                return;
            }

            backgroundSource.clip = audioDict[name];
            backgroundSource.Play();
        }

        public void PauseBackgroundMusic() {
            backgroundSource.Pause();
        }

        public void StopBackgroundMusic() {
            backgroundSource.Stop();
        }

        public float GetSfxVolume() {
            return sfxSource.volume;
        }

        public void SetSfxVolume(float volume) {
            volume = Math.Clamp(volume, 0.0f, 1.0f);
            sfxSource.volume = volume;
            PlayerPrefs.SetFloat(SFX_V_KEY, volume);
        }

        public float GetBackgroundVolume() {
            return backgroundSource.volume;
        }

        public void SetBackgroundVolume(float volume) {
            volume = Math.Clamp(volume, 0.0f, 1.0f);
            backgroundSource.volume = volume;
            PlayerPrefs.SetFloat(MUSIC_V_KEY, volume);
        }
    }
}
