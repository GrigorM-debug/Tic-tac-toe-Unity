using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameController : MonoBehaviour
{
    public int whoseTurn; // 0 = X and 1 = O
    public int turnCount;
    public GameObject[] turnIcons;
    public Sprite[] playerIcons;
    public Button[] tictactoeSpaces; // Playable space for our game
    public int[,] markedFields; // 2D array for marked fields

    public TextMeshProUGUI resultText;
    public AudioSource winSound;
    public AudioSource loseSound;
    public AudioSource moveSound;
    public AudioSource timeIsEndingSound;

    public bool hasPlayedEndingSound = false;
    public float playerTimeLimit = 10f; // 10 seconds
    public float currentPlayerTime;
    public bool isTimerRunning = false;
    public TextMeshProUGUI timerText;
    public Button rematchButton;

    public TextMeshProUGUI XWinsCount;
    public TextMeshProUGUI OWinsCounter;
    public TextMeshProUGUI TiesCounter;

    public int currXWinCount = 0;
    public int currOWinsCounter = 0;
    public int tiesCount = 0;

    Dictionary<Vector2Int, int> moveFrequency = new Dictionary<Vector2Int, int>();

    List<List<Vector2Int>> playerWinningSequences = new List<List<Vector2Int>>();


    void Start()
    {
        GameInitialize();

        AudioSource[] audioSources = GetComponents<AudioSource>();
        moveSound = audioSources[2];
        winSound = audioSources[0];
        loseSound = audioSources[1];
        timeIsEndingSound = audioSources[4];
    }

    void GameInitialize()
    {
        whoseTurn = 0;
        turnCount = 0;
        turnIcons[0].SetActive(true);
        turnIcons[1].SetActive(false);
        markedFields = new int[3, 3];
        resultText.gameObject.SetActive(false);
        isTimerRunning = true;
        currentPlayerTime = playerTimeLimit;
        rematchButton.gameObject.SetActive(false);
        hasPlayedEndingSound = false;

        XWinsCount.text = currXWinCount.ToString();
        OWinsCounter.text = currOWinsCounter.ToString();
        TiesCounter.text = tiesCount.ToString();

        for (int i = 0; i < tictactoeSpaces.Length; i++)
        {
            tictactoeSpaces[i].interactable = true;
            tictactoeSpaces[i].GetComponent<Image>().sprite = null;

            // Assign button click events programmatically
            int index = i; // Capture index in local variable for lambda
            tictactoeSpaces[i].onClick.RemoveAllListeners(); // Clear any old listeners
            tictactoeSpaces[i].onClick.AddListener(() => OnButtonClick(index));
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                markedFields[i, j] = -1;
            }
        }
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentPlayerTime -= Time.deltaTime;
            timerText.text = Mathf.Ceil(currentPlayerTime).ToString();

            if (currentPlayerTime <= 0)
            {
                isTimerRunning = false;
                ComputerWinsWhenTimeIsOver();
            }
            else if (currentPlayerTime <= 3 && !hasPlayedEndingSound)
            {
                timeIsEndingSound.Play();
                hasPlayedEndingSound = true;
            }
        }
    }

    void ComputerWinsWhenTimeIsOver()
    {
        foreach (var t in tictactoeSpaces) { t.interactable = false; }
        loseSound.Play();
        resultText.text = "Time's Up! O wins!";
        resultText.gameObject.SetActive(true);
        rematchButton.gameObject.SetActive(true);

        currOWinsCounter++;
        OWinsCounter.text = currOWinsCounter.ToString();
    }

    public void OnButtonClick(int index)
    {
        int x = index / 3;
        int y = index % 3;
        TicTacToePlayableButtons(x, y);
    }

    public void TicTacToePlayableButtons(int x, int y)
    {
        if (timeIsEndingSound.isPlaying)
        {
            timeIsEndingSound.Stop();
        }

        // Update the board and button
        tictactoeSpaces[x * 3 + y].image.sprite = playerIcons[whoseTurn];
        tictactoeSpaces[x * 3 + y].interactable = false;

        turnCount++;
        markedFields[x, y] = whoseTurn;

        RecordPlayerMove(new Vector2Int(x, y));

        moveSound.Play();

        if (CheckWin())
        {
            EndGame();
        }
        else if (turnCount >= 9)
        {
            DrawGame();
        }
        else
        {
            whoseTurn = (whoseTurn == 0) ? 1 : 0;
            turnIcons[0].SetActive(whoseTurn == 0);
            turnIcons[1].SetActive(whoseTurn == 1);

            if (whoseTurn == 1) // Assuming the computer is O
            {
                ComputerMove();
                moveSound.Play();
            }
        }

        currentPlayerTime = playerTimeLimit;
        hasPlayedEndingSound = false;
    }


    void ComputerMove()
    {
        int bestScore = int.MinValue;
        int alpha = int.MinValue;
        int beta = int.MaxValue;

        List<Vector2Int> bestMoves = new List<Vector2Int>();

        Vector2Int? patternBlockMove = GetBlockingPatternMove();
        if (patternBlockMove.HasValue)
        {
            TicTacToePlayableButtons(patternBlockMove.Value.x, patternBlockMove.Value.y);
            return;
        }

        // Block the player if they have a winning move
        Vector2Int? blockingMove = GetBlockingMove();

        if (blockingMove.HasValue)
        {
            TicTacToePlayableButtons(blockingMove.Value.x, blockingMove.Value.y);
            return;
        }

        // Consider the most frequent move
        Vector2Int frequentMove = GetMostFrequentMove();
        bool frequentMoveIsAvailable = markedFields[frequentMove.x, frequentMove.y] == -1;

        if (frequentMoveIsAvailable)
        {
            // Use the frequent move if it's available
            TicTacToePlayableButtons(frequentMove.x, frequentMove.y);
            return;
        }

        // Use MiniMax to find the best move
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (markedFields[i, j] == -1)
                {
                    markedFields[i, j] = 1; // AI move
                    //int depth = (currOWinsCounter - currXWinCount) > 3 ? 10 : 8;
                    int depth = 10 - (turnCount / 2);
                    int score = MiniMax(markedFields, depth, false, alpha, beta);
                    markedFields[i, j] = -1;

                    foreach (var m in moveFrequency)
                    {
                        if (moveFrequency[m.Key] > 3)
                        {
                            score -= 10;
                        }
                    }


                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMoves.Clear();
                        bestMoves.Add(new Vector2Int(i, j));
                    }
                    else if (score == bestScore)
                    {
                        bestMoves.Add(new Vector2Int(i, j));
                    }

                    alpha = Mathf.Max(alpha, bestScore);
                    if (beta <= alpha) break;
                }
            }
        }

        Vector2Int move;

        if (bestMoves.Count > 0)
        {
            move = bestMoves[Random.Range(0, bestMoves.Count)];
            TicTacToePlayableButtons(move.x, move.y);
            return;
        }

        if (Random.Range(0, 100) < 10)
        {
            move = bestMoves[Random.Range(0, bestMoves.Count)];
        }
        else
        {
            move = bestMoves.First();  // Choose the best move
        }

        if (moveFrequency.ContainsKey(move) && moveFrequency[move] > 5)
        {
            move = bestMoves.FirstOrDefault(m => !moveFrequency.ContainsKey(m) || moveFrequency[m] < 5);
        }



        TicTacToePlayableButtons(move.x, move.y);

        currentPlayerTime = playerTimeLimit;
    }

    Vector2Int? GetBlockingMove()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (markedFields[i, j] == -1)
                {
                    markedFields[i, j] = 0; // Player move
                    bool block = CheckPlayerWinningMove(markedFields, 0);
                    markedFields[i, j] = -1;

                    if (block) return new Vector2Int(i, j);
                }
            }
        }

        return null;
    }

    Vector2Int GetMostFrequentMove()
    {
        //return moveFrequency.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;

        var frequentMoves = moveFrequency.Select(kvp => kvp.Key).ToList();
        return frequentMoves.FirstOrDefault();
    }

    int AdjustedEvaluateBoard(int[,] markedFields)
    {
        int score = 0;
        int ai = 1;
        int player = 0;

        // Check for potential winning moves for AI
        if (CheckPlayerWinningMove(markedFields, ai))
            score += 100; // AI win
        if (CheckPlayerWinningMove(markedFields, player))
            score -= 100; // Player win

        // Center is valuable
        if (markedFields[1, 1] == ai) score += 80;

        // Corners are valuable
        int[][] corners = new int[][]
        {
        new int[] { 0, 0 },
        new int[] { 0, 2 },
        new int[] { 2, 0 },
        new int[] { 2, 2 }
        };

        foreach (var corner in corners)
        {
            //if (markedFields[corner[0], corner[1]] == ai) score += 5;
            //if (markedFields[corner[0], corner[1]] == player) score -= 5;

            if (markedFields[corner[0], corner[1]] == ai) score += 80;
            if (markedFields[corner[0], corner[1]] == player) score -= 80;
        }

        // Sides are less valuable
        int[][] sides = new int[][]
        {
        new int[] { 0, 1 },
        new int[] { 1, 0 },
        new int[] { 1, 2 },
        new int[] { 2, 1 }
        };

        foreach (var side in sides)
        {
            //if (markedFields[side[0], side[1]] == ai) score += 2;
            //if (markedFields[side[0], side[1]] == player) score -= 2;

            if (markedFields[side[0], side[1]] == ai) score += 80;
            if (markedFields[side[0], side[1]] == player) score -= 80;
        }

        //Diagonals
        int[][] diagonals = new int[][]
        {
            new int[] { 0, 0 }, // Top-left corner for primary diagonal
            new int[] { 1, 1 }, // Center (shared by both diagonals)
            new int[] { 2, 2 }, // Bottom-right corner for primary diagonal
            new int[] { 0, 2 }, // Top-right corner for secondary diagonal
            new int[] { 2, 0 }
        };

        foreach (var diagonal in diagonals)
        {
            if (markedFields[diagonal[0], diagonal[1]] == ai) score += 80;
            if (markedFields[diagonal[0], diagonal[1]] == player) score -= 80;
        }

        // Add additional heuristics if necessary
        // Example: Block opponent's potential winning move
        if (CheckBlockingMove(markedFields, player))
            score -= 100; // Deduct points for defensive moves

        return score;

    }

    int MiniMax(int[,] markedFields, int depth, bool isMaximizing, int alpha, int beta)
    {

        int result = CheckWinner();
        if (result != 0)
        {
            return result == 10 ? 10 - depth : -10 + depth;
        }

        int score = AdjustedEvaluateBoard(markedFields);

        //if(score == 10)
        //{
        //    return 10 - depth;
        //}

        //if (score == -10) 
        //{
        //    return -10 + depth;
        //}

        if (score == 10 || score == -10)
        {
            return score;
        }

        if (isMaximizing)
        {
            int bestScore = int.MinValue;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (markedFields[i, j] == -1)
                    {
                        markedFields[i, j] = 1; // AI move
                        int max = MiniMax(markedFields, depth + 1, !isMaximizing, alpha, beta);

                        if(max > bestScore) { 
                            bestScore = max;
                     
                        }

                        if(bestScore > alpha) alpha = bestScore;

                        markedFields[i, j] = -1;
                        if (beta <= alpha) break;
                    }
                }
            }

            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (markedFields[i, j] == -1)
                    {
                        markedFields[i, j] = 0; // Player move
                        int min = MiniMax(markedFields, depth + 1, isMaximizing, alpha, bestScore);

                        if(min <= bestScore) bestScore = min;

                        if(bestScore < beta) beta = bestScore;

                        markedFields[i, j] = -1;
                        if (beta <= alpha) break;
                    }
                }
            }

            return bestScore;
        }
    }

    Vector2Int? GetBlockingPatternMove()
    {
        foreach (var sequence in playerWinningSequences)
        {
            foreach (var move in sequence)
            {
                if (markedFields[move.x, move.y] == -1) // Spot is available
                {
                    return move; // Block this spot
                }
            }
        }
        return null;
    }

    void RecordPlayerMove(Vector2Int move)
    {
        if (moveFrequency.ContainsKey(move))
        {
            moveFrequency[move]++;
        }
        else
        {
            moveFrequency[move] = 1;
        }
    }


    bool CheckPlayerWinningMove(int[,] board, int player)
    {
        int[,] winPatterns = new int[,] {
            { 0, 1, 2 }, // Top row
            { 3, 4, 5 }, // Middle row
            { 6, 7, 8 }, // Bottom row
            { 0, 3, 6 }, // Left column
            { 1, 4, 7 }, // Center column
            { 2, 5, 8 }, // Right column
            { 0, 4, 8 }, // Diagonal \
            { 2, 4, 6 }  // Diagonal /
        };


        for (int i = 0; i < winPatterns.GetLength(0); i++)
        {
            int a = winPatterns[i, 0];
            int b = winPatterns[i, 1];
            int c = winPatterns[i, 2];

            int aRow = a / 3;
            int aCol = a % 3;
            int bRow = b / 3;
            int bCol = b % 3;
            int cRow = c / 3;
            int cCol = c % 3;

            if (board[aRow, aCol] == player &&
                board[bRow, bCol] == player &&
                board[cRow, cCol] == player)
            {
                return true;
            }


        }
        return false;
    }

    bool CheckBlockingMove(int[,] markedFields, int player)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (markedFields[i, j] == -1)
                {
                    markedFields[i, j] = player;
                    bool block = CheckPlayerWinningMove(markedFields, player);
                    markedFields[i, j] = -1;

                    if (block) return true;
                }
            }
        }

        return false;
    }

    int CheckWinner()
    {
        int[,] winningCombinations = new int[,] {
        { 0, 1, 2 }, // Top row
        { 3, 4, 5 }, // Middle row
        { 6, 7, 8 }, // Bottom row
        { 0, 3, 6 }, // Left column
        { 1, 4, 7 }, // Center column
        { 2, 5, 8 }, // Right column
        { 0, 4, 8 }, // Diagonal \
        { 2, 4, 6 }  // Diagonal /
    };

        for (int i = 0; i < winningCombinations.GetLength(0); i++)
        {
            int a = winningCombinations[i, 0];
            int b = winningCombinations[i, 1];
            int c = winningCombinations[i, 2];

            int aRow = a / 3;
            int aCol = a % 3;
            int bRow = b / 3;
            int bCol = b % 3;
            int cRow = c / 3;
            int cCol = c % 3;

            if (markedFields[aRow, aCol] == markedFields[bRow, bCol] &&
                markedFields[bRow, bCol] == markedFields[cRow, cCol] &&
                markedFields[aRow, aCol] != -1)
            {
                if (markedFields[aRow, aCol] == 0) // Player wins
                {
                    return -10;
                }
                else if (markedFields[aRow, aCol] == 1) // AI wins
                {
                    return 10;
                }
            }
        }

        // Check for a draw
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (markedFields[i, j] == -1)
                {
                    return 0; // Game not over
                }
            }
        }

        return 0; // Draw
    }


    bool CheckWin()
    {
        int winner = CheckWinner();
        return winner == 10 || winner == -10;
    }

    void EndGame()
    {
        foreach (Button btn in tictactoeSpaces)
        {
            btn.interactable = false;
        }

        if (whoseTurn == 0)
        {
            winSound.Play();
            resultText.text = "X wins!";
            currXWinCount++;
            XWinsCount.text = currXWinCount.ToString();

            StorePlayerWinningMoves();
        }
        else
        {
            loseSound.Play();
            resultText.text = "O wins!";
            currOWinsCounter++;
            OWinsCounter.text = currOWinsCounter.ToString();
        }

        currentPlayerTime = playerTimeLimit;
        isTimerRunning = false;
        rematchButton.gameObject.SetActive(true);
        resultText.gameObject.SetActive(true);
    }

    void StorePlayerWinningMoves()
    {
        List<Vector2Int> playerWinningMoves = new List<Vector2Int>();

        for(int i = 0; i < 3; i++) 
        {
            for (int j = 0; j < 3; j++)
            {
                if (markedFields[i, j] == 0)
                {
                    playerWinningMoves.Add(new Vector2Int(i, j));
                }
            }
        }

        playerWinningSequences.Add(playerWinningMoves);
    }

    void DrawGame()
    {
        tiesCount++;
        TiesCounter.text = tiesCount.ToString();
        resultText.text = "It's a draw!";
        resultText.gameObject.SetActive(true);
        rematchButton.gameObject.SetActive(true);
    }

    public void Rematch()
    {
        if (winSound.isPlaying)
        {
            winSound.Stop();
        }
        else if (loseSound.isPlaying)
        {
            loseSound.Stop();
        }

        moveFrequency.Clear();
        playerWinningSequences.Clear();

        GameInitialize();
    }
}
