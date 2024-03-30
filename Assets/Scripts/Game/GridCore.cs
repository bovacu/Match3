using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game {
    public enum OrbType {
        None = 0,
        Blue,
        Red,
        Yellow,
        Green,
        Orange,
        MAX
    }

    public enum OrbMovementResult {
        Ok,         // Valid movement + match happened
        BadMatch,   // Valid movement + match did not happen
        Invalid     // Invalid movement
    }

    public class Match {
        public Vector2Int origin;
        public int amountOfHorizontalOrbs;
        public int amountOfVerticalOrbs;
        public OrbType type;
    }

    public class FallingOrb {
        public Vector2Int emptyorbOrigin;
        public Vector2Int fallingorbOrigin;
    }

    public class RefillOrb {
        public Vector2Int position;
        public OrbType type;
    }

    public class ShuffledOrb {
        public Vector2Int origin;
        public Vector2Int newPos;
    }

    public class PossibleOrbMovements {
        public Vector2Int origin;
        public List<Vector2Int> directions;

        public PossibleOrbMovements() {
            directions = new List<Vector2Int>();
        }
    }

    public class GridCore {

        // First coordinate is Y, second is X
        private OrbType[,] orbs;
        private int width, height;

        private const int extraHeightRows = 10;
        public int ExtraHeightRows => extraHeightRows;

        public OrbType this[int x, int y] {
            get { return orbs[y, x]; }
        }

        public GridCore(int gridWidth, int gridHeight) {
            const int amountOforbTypes = (int)(OrbType.MAX - 1);
            if (gridWidth < amountOforbTypes || gridHeight < amountOforbTypes) {
                throw new Exception($"Grid width and height must be greater or equal to the amount of orbs, in this case '{amountOforbTypes}'");
            }

            width = gridWidth;
            height = gridHeight;

            orbs = new OrbType[height + extraHeightRows, width];
            GenerateRandomBoard();
        }

        // This constructir will only be provided on Unit Test cases basically, as random boards make a better fit on this kind of games.
        // Maybe also is useful if a server sent the board pre-made for specific reasons.
        // First entry of orbsList is Y and second is X.
        public GridCore(OrbType[,] orbsList) {
            const int amountOforbTypes = (int)(OrbType.MAX - 1);
            width = orbsList.GetLength(1);
            height = orbsList.GetLength(0);

            if (width < amountOforbTypes || height < amountOforbTypes) {
                throw new Exception($"Grid width and height must be greater or equal to the amount of orbs, in this case '{amountOforbTypes}'");
            }

            orbs = new OrbType[height + extraHeightRows, width];
            GenerateBoardFromData(orbsList);
        }

        // Checks if a specific type of orb at a given position (x, y) will create a match of any size (minimum 3)
        private bool WillOrbCreateMatch(OrbType orb, int x, int y) {
            bool matchOnAxisXRight = x < width - 2 && orbs[y, x + 1] == orb && orbs[y, x + 2] == orb;
            bool matchOnAxisXLeft = x > 1 && orbs[y, x - 1] == orb && orbs[y, x - 2] == orb;

            bool matchOnAxisYUp = y < height - 2 && orbs[y + 1, x] == orb && orbs[y + 2, x] == orb;
            bool matchOnAxisYDown = y > 1 && orbs[y - 1, x] == orb && orbs[y - 2, x] == orb;

            bool matchMiddleX = x > 0 && x < width - 1 && orbs[y, x - 1] == orb && orbs[y, x + 1] == orb;
            bool matchMiddleY = y > 0 && y < height - 1 && orbs[y - 1, x] == orb && orbs[y + 1, x] == orb;

            return matchOnAxisXRight || matchOnAxisXLeft || matchOnAxisYUp || matchOnAxisYDown || matchMiddleX || matchMiddleY;
        }

        // Utility function that temporarly changes two orbs position, applys some function and puts the orbs back to the original positions.
        private void SwapOrbsThenExecuteAndUndoSwap(Vector2Int orbAPos, Vector2Int orbBPos, Action callback) {
            OrbType orbA = orbs[orbAPos.y, orbAPos.x];
            OrbType orbB = orbs[orbBPos.y, orbBPos.x];

            orbs[orbAPos.y, orbAPos.x] = orbB;
            orbs[orbBPos.y, orbBPos.x] = orbA;

            callback();

            orbs[orbAPos.y, orbAPos.x] = orbA;
            orbs[orbBPos.y, orbBPos.x] = orbB;
        }

        // Checks if moving an orb at (x, y) on any allowed direction would make a match. If so, and directions is not null, all the matching
        // directions will be added to the List
        private bool WillExistingOrbMoveCreateMatch(int x, int y, List<Vector2Int> directions = null) {
            bool match = false;
            
            if(y > 0) {
                SwapOrbsThenExecuteAndUndoSwap(new Vector2Int(x, y), new Vector2Int(x, y - 1), () => { 
                    bool success = WillOrbCreateMatch(orbs[y, x], x, y);
                    
                    if(success) {
                        directions.Add(new Vector2Int(0, -1));
                    }
                    
                    match |= success;
                });
            }

            if(y < height - 1) {
                SwapOrbsThenExecuteAndUndoSwap(new Vector2Int(x, y), new Vector2Int(x, y + 1), () => {
                    bool success = WillOrbCreateMatch(orbs[y, x], x, y);

                    if (success) {
                        directions.Add(new Vector2Int(0, 1));
                    }

                    match |= success;
                });
            }

            if (x > 0) {
                SwapOrbsThenExecuteAndUndoSwap(new Vector2Int(x, y), new Vector2Int(x - 1, y), () => {
                    bool success = WillOrbCreateMatch(orbs[y, x], x, y);

                    if (success) {
                        directions.Add(new Vector2Int(-1, 0));
                    }

                    match |= success;
                });
            }

            if (x < width - 1) {
                SwapOrbsThenExecuteAndUndoSwap(new Vector2Int(x, y), new Vector2Int(x + 1, y), () => {
                    bool success = WillOrbCreateMatch(orbs[y, x], x, y);

                    if (success) {
                        directions.Add(new Vector2Int(1, 0));
                    }

                    match |= success;
                });
            }

            return match;
        }

        // This creates an orb on an empty position of the grid, that guarantees won't create a match with existing orbs.
        private OrbType GenerateNotConflictingOrbAtPosition(int x, int y) {
            OrbType orb = (OrbType)(UnityEngine.Random.Range(1, (int)OrbType.MAX));
            bool didNeworbCreateAutomaticMatch = WillOrbCreateMatch(orb, x, y);
            int orbTypeRotation = 0;

            while (didNeworbCreateAutomaticMatch) {
                // - 2 because both MAX and None are not valid states for this case
                if (orbTypeRotation == (int)OrbType.MAX - 2) {
                    throw new Exception("Rotated around all orbs and none of them made a valid match");
                }

                int mod = (int)OrbType.MAX;
                int nextType = (int)orb + 1;
                if (nextType == (int)OrbType.None || nextType == (int)OrbType.MAX) nextType = 1;

                orb = (OrbType)(nextType % mod);
                didNeworbCreateAutomaticMatch = WillOrbCreateMatch(orb, x, y);
                orbTypeRotation++;
            
                if(orb == OrbType.None) {
                    throw new Exception($"BadorbType -> Mod: {mod}, nextType: {nextType}, orb: {orb}, rotation: {orbTypeRotation}");
                }
            }

            return orb;
        }

        // Shuffles the board with Fisher-Yates a fixed amount of time to ensure a playable board. If after N shuffles it is not playable,
        // a new one is created.
        private void ShuffleWithFisherYates() {
            OrbType[] flatBoard = new OrbType[width * height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    flatBoard[y * width + x] = orbs[y, x];
                }
            }

            for (int i = flatBoard.Length - 1; i > 0; i--) {
                int j = UnityEngine.Random.Range(0, (int)OrbType.MAX);
                OrbType temp = flatBoard[i];
                flatBoard[i] = flatBoard[j];
                flatBoard[j] = temp;
            }

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    orbs[y, x] = flatBoard[y * width + x];
                }
            }
        }

        // Generates a playable board with random values.
        private void GenerateRandomBoard() {
            for (int y = 0; y < height + extraHeightRows; y++) {
                for (int x = 0; x < width; x++) {
                    orbs[y, x] = GenerateNotConflictingOrbAtPosition(x, y);
                }
            }

            while(GetPossibleMovementsOrigins().Count == 0) {
                ShuffleBoard();
            }
        }

        // Generates a board (assumed playable) from a given source of data.
        // First coordinate is Y, second is X
        private void GenerateBoardFromData(OrbType[,] orbsList) {
            if (orbsList.GetLength(1) != width) {
                throw new Exception($"width({width}) of the grid did not match width({orbsList.GetLength(1)}) of the incoming data");
            }

            for(int y = 0; y < orbsList.GetLength(0); y++) {
                for(int x = 0; x < orbsList.GetLength(1); x++) {
                    orbs[y, x] = orbsList[y, x];
                }
            }

            for (int y = orbsList.GetLength(0); y < orbsList.GetLength(0) + extraHeightRows; y++) {
                for (int x = 0; x < orbsList.GetLength(1); x++) {
                    orbs[y, x] = GenerateNotConflictingOrbAtPosition(x, y);
                }
            }

            while (GetPossibleMovementsOrigins().Count == 0) {
                ShuffleBoard();
            }
        }



        // Returns a list of orbes that when moved, would create a match of any size.
        public List<PossibleOrbMovements> GetPossibleMovementsOrigins() {
            List<PossibleOrbMovements> origins = new List<PossibleOrbMovements>();

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    List<Vector2Int> directions = new List<Vector2Int>();
                    if (WillExistingOrbMoveCreateMatch(x, y, directions)) {
                        origins.Add(new PossibleOrbMovements() {
                            origin = new Vector2Int(x, y),
                            directions = directions
                        });
                    }
                }
            }

            return origins;
        }

        // Checks if a movement is valid (is ok and creates a match), wrong (is ok but won't create a match) or invalid (outside of bounds).
        public OrbMovementResult IsMovementValid(Vector2Int orbA, Vector2Int orbB) {
            if(orbA.x < 0 || orbB.x < 0 || orbA.y < 0 || orbB.y < 0 ||
               orbA.x >= width || orbB.x >= width || orbA.y >= height || orbB.y >= height) {
                return OrbMovementResult.Invalid;
            }

            bool match = false;
            SwapOrbsThenExecuteAndUndoSwap(orbA, orbB, () => { 
                match = WillOrbCreateMatch(orbs[orbA.y, orbA.x], orbA.x, orbA.y) || WillOrbCreateMatch(orbs[orbB.y, orbB.x], orbB.x, orbB.y);
            });

            return match ? OrbMovementResult.Ok : OrbMovementResult.BadMatch;
        }

        // Persists a swap of orbs on the board
        public void ApplyMovement(Vector2Int orbA, Vector2Int orbB) {
            OrbType a = orbs[orbA.y, orbA.x];
            OrbType b = orbs[orbB.y, orbB.x];
            orbs[orbA.y, orbA.x] = b;
            orbs[orbB.y, orbB.x] = a;
        }

        // Returns a list of all the current matches of the board.
        public List<Match> GetBoardMatches() {
            List<Match> origins = new List<Match>();

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width - 2; x++) {
                    if (orbs[y, x] == orbs[y, x + 1] && orbs[y, x] == orbs[y, x + 2]) {
                        Match match = new Match {
                            origin = new Vector2Int(x, y),
                            amountOfHorizontalOrbs = 2,
                            amountOfVerticalOrbs = 0,
                            type = orbs[y, x]
                        };

                        int extended = 3;
                        while (x + extended < width && orbs[y, x] == orbs[y, x + extended]) {
                            match.amountOfHorizontalOrbs++;
                            extended++;
                        }

                        origins.Add(match);
                    }
                }
            }

            // Check for vertical matches
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height - 2; y++) {
                    if (orbs[y, x] == orbs[y + 1, x] && orbs[y, x] == orbs[y + 2, x]) {
                        Match match = new Match {
                            origin = new Vector2Int(x, y),
                            amountOfHorizontalOrbs = 0,
                            amountOfVerticalOrbs = 2,
                            type = orbs[y, x]
                        };

                        int extended = 3;
                        while (y + extended < height && orbs[y, x] == orbs[y + extended, x]) {
                            match.amountOfVerticalOrbs++;
                            extended++;
                        }

                        origins.Add(match);
                    }
                }
            }

            return origins;
        }
        
        // Persists matches on the board, removing matched orbs.
        public List<Match> ApplyMatches() {
            List<Match> matches = GetBoardMatches();

            foreach(Match m in matches) {
                for(int i = 0; i <= m.amountOfHorizontalOrbs; i++) {
                    orbs[m.origin.y, m.origin.x + i] = OrbType.None;
                }

                for (int i = 0; i <= m.amountOfVerticalOrbs; i++) {
                    orbs[m.origin.y + i, m.origin.x] = OrbType.None;
                }
            }

            return matches;
        }

        // Moves down all necessary orbs to fill the board after matches.
        public List<FallingOrb> ApplyFall() {
            List<FallingOrb> fallingBlocksNewPositions = new List<FallingOrb>();

            for (int y = 0; y < height + extraHeightRows; y++) {
                for (int x = 0; x < width; x++) {
                    if (orbs[y, x] != OrbType.None) {
                        continue;
                    }

                    int _y = y;
                    while(_y < height + extraHeightRows - 1 && orbs[_y, x] == OrbType.None) { _y++; }
                    OrbType falling = orbs[_y, x];
                    orbs[y, x] = falling;
                    orbs[_y, x] = OrbType.None;

                    fallingBlocksNewPositions.Add(new FallingOrb { fallingorbOrigin = new Vector2Int(x, _y), emptyorbOrigin = new Vector2Int(x, y) });
                }
            }

            return fallingBlocksNewPositions;
        }

        // Refills the top of the board so it always has orbs to move down after a match.
        public List<RefillOrb> RefillOrbs() {
            List<RefillOrb> refills = new List<RefillOrb>();

            for (int y = height; y < height + extraHeightRows; y++) {
                for (int x = 0; x < width; x++) {
                    if (orbs[y, x] == OrbType.None) {
                        orbs[y, x] = GenerateNotConflictingOrbAtPosition(x, y);
                        refills.Add(new RefillOrb() { position = new Vector2Int(x, y), type = orbs[y, x] });
                    }
                }
            }

            return refills;
        }

        // Shuffles the board.
        public void ShuffleBoard() {
            int maxTriesWithFisherYates = 25;
            bool newValidBoard = false;
            int shuffleAttemptsWithFisherYates = 0;

            // First we try to shuffle with the current orbs on the board, we want to get to a state where
            // no matches are created after shuffling, but valid matches will be created if orbs are swaped.
            // Using Fisher-Yates, we can achieve a good enought shuffling, but this method cannot 100% ensure
            // both conditions just mentioned. This is why we create a counter of maximum tries. If after that
            // no valid board can be generated, a new board will be created.
            do {
                ShuffleWithFisherYates();
                newValidBoard = GetPossibleMovementsOrigins().Count > 0 && GetBoardMatches().Count == 0;
                if(!newValidBoard) {
                    shuffleAttemptsWithFisherYates++;
                }
            } while (!newValidBoard && shuffleAttemptsWithFisherYates < maxTriesWithFisherYates);


            if (!newValidBoard) {
                GenerateRandomBoard();
            }
        }

        // Prints the board on Unity's console.
        public void PrintBoard() {
            for (int y = height - 1; y >= 0; y--) {
                string line = "";
                for (int x = 0; x < width; x++) {
                    line += $"{orbs[y, x].ToString()[0]}  ";
                }
                Debug.Log(line);
            }
        }
    }
}
 