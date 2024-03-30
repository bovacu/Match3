using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game {
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/OrbsData", order = 1)]
    public class OrbsData : ScriptableObject {
        public OrbTypeToImage[] orbs;
        private Dictionary<OrbType, Sprite> dict = null;

        private void InitDict() {
            dict = new Dictionary<OrbType, Sprite>();
            for (int i = 0; i < orbs.Length; i++) {
                OrbTypeToImage oToI = orbs[i];

                if (dict.ContainsKey(oToI.orbType)) {
                    throw new Exception($"There is an error on the orbs images in GridView Script, '{oToI.orbType}' is duplicated, no duplications allowed");
                }

                dict[oToI.orbType] = oToI.image;
            }
        }

        public Sprite GetSprite(OrbType type) {
            if(dict == null) {
                InitDict();
            }

            return dict[type];
        }

        void OnValidate() {
            if(orbs.Length != (int)(OrbType.MAX - 1)) {
                Array.Resize(ref orbs, (int)(OrbType.MAX - 1));
            }

            foreach (OrbTypeToImage o in orbs) {
                if (o.orbType == OrbType.MAX || o.orbType == OrbType.None) {
                    Debug.LogWarning("orbImages elements cannot be set neither to None nor MAX! Defaulting to blue");
                    o.orbType = OrbType.Blue;
                }
            }
        }
    }

    [Serializable]
    public class OrbTypeToImage {
        public OrbType orbType;
        public Sprite image;
    }
}
