using Assets.Scripts;
using Assets.Scripts.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Game {

    public class GridSystem : MonoBehaviour {
        [Header("Width And Height of grid")]
        [SerializeField]
        private int[] gridSize;


        [Header("Deepest Grid background")]
        [SerializeField]
        private RectTransform backgroundImage;


        [Header("Grid Background Tiles")]
        [SerializeField]
        private GridLayoutGroup backgroundGridParent;

        [SerializeField]
        private Image backgroundGridImagePref;


        [Header("Grid orbs Tiles")]
        [SerializeField]
        private OrbsData orbsData;

        [SerializeField]
        private Transform orbsGridParent;

        [SerializeField]
        private RectTransform visibleGridPanel;

        [SerializeField]
        private Orb orbPrefab;

        [SerializeField]
        private RectTransform canvas;

        [SerializeField]
        private GameObject shufflePref;

        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float orbSizeRelativeToBackgroundTile = 0.95f;

        [Header("Score System")]
        [SerializeField]
        private ScoreSystem scoreSystem;

        [Header("Sound System")]
        private SoundSystem soundSystem;

        private ConfigHolder configHolder;
        private GridCore gridCore;
        private Pool<Orb> orbPool;
        private float tileSize;
        private Dictionary<Vector2Int, Orb> orbsDict;
        private bool blockInput = false;
        private bool gameEnded = false;

        public GridCore Core => gridCore;

        void Start() {
            configHolder = FindObjectOfType<ConfigHolder>();
            soundSystem = FindObjectOfType<SoundSystem>();

            soundSystem.PlayBackgroundMusic(soundSystem.MUSIC_GAME);

            if (configHolder.randomSeed != -1) {
                UnityEngine.Random.InitState(configHolder.randomSeed);
            }

            gridCore = new GridCore(gridSize[0], gridSize[1]);

            orbPool = new Pool<Orb>(gridSize[0] * gridSize[1] * 2, () => {
                Orb orb = Instantiate(orbPrefab, orbsGridParent);
                orb.gameObject.SetActive(false);
                return orb;
            });

            backgroundGridParent.cellSize = new Vector2(backgroundGridImagePref.GetComponent<RectTransform>().sizeDelta.x, backgroundGridImagePref.GetComponent<RectTransform>().sizeDelta.x);
            tileSize = backgroundGridParent.cellSize.x;
            BuildBackgroundAndGridTiles();
            BuildorbsOnGrid();
        }

        // This is a really simple way of blocking input. I did it like this as if a more complex way if needed, like enabling and disabling an invisible panel
        // is needed, the main change is done just in these 2 functions, no need to change in many places.
        public void BlockInput(bool blocked) {
            if(gameEnded) {
                return;
            }

            blockInput = blocked;
        }

        public bool IsInputBlocked() {
            return gameEnded || blockInput;
        }

        public void SetGameEnded(bool ended) {
            gameEnded = ended;
        }

        private void BuildBackgroundAndGridTiles() {
            backgroundImage.sizeDelta = new Vector2(tileSize * gridSize[0] + tileSize * 0.5f, tileSize * gridSize[1] + tileSize * 0.5f);

            backgroundGridParent.constraintCount = gridSize[0];
            for (int i = 0; i < gridSize[0] * gridSize[1]; i++) {
                Instantiate(backgroundGridImagePref, backgroundGridParent.transform);
            }
        }

        // Gets half of the size of the board, to center it on the screen.
        private Vector2 CalculateBoardOffset() {
            return new Vector2(tileSize * gridSize[0] / 2.0f, tileSize * gridSize[1] / 2.0f);
        }

        private void InitAndPositionOrb(Orb orb, int x, int y, Vector2 offset) {
            orb.gameObject.SetActive(true);
            orb.Init(new Vector2Int(x, y), gridCore[x, y], orbsData.GetSprite(gridCore[x, y]), OnSwapped);

            RectTransform t = orb.GetComponent<RectTransform>();
            t.sizeDelta = new Vector2(tileSize * orbSizeRelativeToBackgroundTile, tileSize * orbSizeRelativeToBackgroundTile);
            t.anchoredPosition = new Vector2(-offset.x + x * tileSize + tileSize * 0.5f, -offset.y + y * tileSize + tileSize * 0.5f);
        }

        private void BuildorbsOnGrid() {
            Vector2 offset = CalculateBoardOffset();

            visibleGridPanel.sizeDelta = offset * 2;
            orbsDict = new Dictionary<Vector2Int, Orb>();

            for (int y = 0; y < gridSize[1] + gridCore.ExtraHeightRows; y++) {
                for (int x = 0; x < gridSize[0]; x++) {
                    Orb orb = orbPool.Get();
                    InitAndPositionOrb(orb, x, y, offset);
                    orbsDict[new Vector2Int(x, y)] = orb;
                }
            }
        }

        private void UpdateVisualOrbs(Vector2Int posOfSwappingOrb, Vector2Int swapDir, Orb orbSwapping, Orb orbSwapped) {
            orbsDict[posOfSwappingOrb] = orbSwapped;
            orbsDict[posOfSwappingOrb + swapDir] = orbSwapping;
            orbSwapping.Position = posOfSwappingOrb + swapDir;
            orbSwapped.Position = posOfSwappingOrb;
        }

        private IEnumerator SwapTwoOrbs(Vector2Int swapDir, Orb orbSwapping, Orb orbSwapped, int amount = 1) {
            RectTransform rectSwapping = orbSwapping.GetComponent<RectTransform>();
            RectTransform rectSwapped = orbSwapped.GetComponent<RectTransform>();

            Coroutine a = StartCoroutine(Util.TweenAnchoredPosition(rectSwapping, rectSwapping.anchoredPosition, new Vector2(rectSwapping.anchoredPosition.x + swapDir.x * tileSize, rectSwapping.anchoredPosition.y + swapDir.y * tileSize), 0.25f));
            Coroutine b = StartCoroutine(Util.TweenAnchoredPosition(rectSwapped, rectSwapped.anchoredPosition, new Vector2(rectSwapped.anchoredPosition.x + -swapDir.x * tileSize, rectSwapped.anchoredPosition.y + -swapDir.y * tileSize), 0.25f));

            yield return a;
            yield return b;
        }

        // This function called from an Action on Orb.cs, that triggers when some input is provided.
        public IEnumerator OnSwapped(Vector2Int posOfSwappingOrb, Vector2Int swapDir) {
            if (IsInputBlocked()) {
                yield break;
            }
            BlockInput(true);

            OrbMovementResult res = gridCore.IsMovementValid(posOfSwappingOrb, posOfSwappingOrb + swapDir);
            if (res == OrbMovementResult.Invalid) {
                PlaySfxOfMatch(-1);
                Debug.LogWarning("Invalid movement. Game won't crush, it is a warn debug log");
                BlockInput(false);
                yield break;
            }

            Orb orbSwapping = orbsDict[posOfSwappingOrb];
            Orb orbSwapped = orbsDict[posOfSwappingOrb + swapDir];

            yield return StartCoroutine(SwapTwoOrbs(swapDir, orbSwapping, orbSwapped));

            switch (res) {
                case OrbMovementResult.Ok: {
                        gridCore.ApplyMovement(posOfSwappingOrb, posOfSwappingOrb + swapDir);
                        UpdateVisualOrbs(posOfSwappingOrb, swapDir, orbSwapping, orbSwapped);
                        yield return StartCoroutine(MatchOrbs());
                    }
                    break;
                case OrbMovementResult.BadMatch: {
                        PlaySfxOfMatch(-1);
                        yield return StartCoroutine(SwapTwoOrbs(-swapDir, orbSwapping, orbSwapped));
                        BlockInput(false);
                    }
                    break;
            }
        }

        private IEnumerator DestroyOrb(Orb orb, Vector2Int pos) {
            orbPool.Return(orb);
            orbsDict[pos] = null;
            yield return orb.Matched();
            orb.gameObject.SetActive(false);
            orb.gameObject.transform.localScale = Vector2.one;
        }

        private void PlaySfxOfMatch(int amount) {
            switch(amount) {
                case 3: soundSystem.PlaySfx(soundSystem.AUDIO_COMBO_3); break;
                case 4: soundSystem.PlaySfx(soundSystem.AUDIO_COMBO_4); break;
                case 5: soundSystem.PlaySfx(soundSystem.AUDIO_COMBO_5); break;
                default: soundSystem.PlaySfx(soundSystem.AUDIO_WRONG); break;
            }
        }

        private IEnumerator MatchOrbs() {
            yield return new WaitForSecondsRealtime(0.15f);

            List<Match> matches = gridCore.ApplyMatches();

            // Dstroy (recycle) orbs inside matches, both horizontally and vertically. Add score to the ScoreSystem and play match sfx
            soundSystem.StartSfxLimit(1);
            foreach (Match m in matches) {
                if (m.amountOfHorizontalOrbs > 0) {
                    if (orbsDict[m.origin] != null) {
                        StartCoroutine(scoreSystem.Score(m.amountOfHorizontalOrbs + 1, m.type, orbsDict[m.origin].GetComponent<RectTransform>()));
                        PlaySfxOfMatch(m.amountOfHorizontalOrbs + 1);
                    }

                    for (int i = 0; i <= m.amountOfHorizontalOrbs; i++) {
                        Vector2Int pos = new Vector2Int(m.origin.x + i, m.origin.y);

                        if (gridCore[pos.x, pos.y] == OrbType.None && orbsDict[pos] != null) {
                            Orb o = orbsDict[pos];
                            StartCoroutine(DestroyOrb(o, pos));
                        }
                    }
                }

                if (m.amountOfVerticalOrbs > 0) {
                    if(orbsDict[m.origin] != null) {
                        StartCoroutine(scoreSystem.Score(m.amountOfVerticalOrbs + 1, m.type, orbsDict[m.origin].GetComponent<RectTransform>()));
                        PlaySfxOfMatch(m.amountOfVerticalOrbs + 1);
                    }

                    for (int i = 0; i <= m.amountOfVerticalOrbs; i++) {
                        Vector2Int pos = new Vector2Int(m.origin.x, m.origin.y + i);

                        if (gridCore[pos.x, pos.y] == OrbType.None && orbsDict[pos] != null) {
                            Orb o = orbsDict[pos];
                            StartCoroutine(DestroyOrb(o, pos));
                        }
                    }
                }
            }
            soundSystem.EndSfxLimit();

            // Make the top orbs fall and replace destroyed orbs of matches
            List<FallingOrb> fallingorbs = gridCore.ApplyFall();
            List<Coroutine> coroutines = new List<Coroutine>();
            foreach (FallingOrb fallingorb in fallingorbs) {
                Orb orb = orbsDict[fallingorb.fallingorbOrigin];
                if (orb == null) {
                    continue;
                }

                int distanceToFall = fallingorb.fallingorbOrigin.y - fallingorb.emptyorbOrigin.y;
                RectTransform rectSwapping = orb.GetComponent<RectTransform>();

                Coroutine a = StartCoroutine(Util.TweenAnchoredPosition(rectSwapping, rectSwapping.anchoredPosition, new Vector2(rectSwapping.anchoredPosition.x, rectSwapping.anchoredPosition.y - distanceToFall * tileSize), 0.25f));
                orbsDict[fallingorb.emptyorbOrigin] = orbsDict[fallingorb.fallingorbOrigin];
                orbsDict[fallingorb.emptyorbOrigin].Position = fallingorb.emptyorbOrigin;
                orbsDict[fallingorb.fallingorbOrigin] = null;

                coroutines.Add(a);
            }

            foreach (Coroutine c in coroutines) {
                yield return c;
            }

            // Refill top orbs and position them to the correct cell of the board.
            List<RefillOrb> refills = gridCore.RefillOrbs();
            Vector2 offset = CalculateBoardOffset();
            foreach (RefillOrb refill in refills) {
                Orb o = orbPool.Get();
                InitAndPositionOrb(o, refill.position.x, refill.position.y, offset);
                orbsDict[refill.position] = o;
            }

            // If after current match, fall down and refill, there are more matches, repeat.
            if (gridCore.GetBoardMatches().Count > 0) {
                yield return MatchOrbs();
            } else {

                // This case should not happen really often, it is possible to even never happen, but if after a match
                // there is no more possible matches (somehow a really strange combination), the board must be shuffled
                // to fix this problem.
                if(gridCore.GetPossibleMovementsOrigins().Count == 0) {
                    GameObject shuffle = Instantiate(shufflePref, canvas);

                    yield return new WaitForSecondsRealtime(2);

                    gridCore.ShuffleBoard();
                    foreach(KeyValuePair<Vector2Int, Orb> orb in orbsDict) {
                        orb.Value.gameObject.SetActive(false);
                        orbPool.Return(orb.Value);
                    }
                    BuildorbsOnGrid();

                    Destroy(shuffle);
                }

                BlockInput(false);
            }
        }
    }

}