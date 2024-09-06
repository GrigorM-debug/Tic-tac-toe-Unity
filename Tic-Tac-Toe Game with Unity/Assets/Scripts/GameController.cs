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

    void Start()
    {
        GameInitialize();

        moveSound = GetComponents<AudioSource>()[2];
        winSound = GetComponents<AudioSource>()[0];
        loseSound = GetComponents<AudioSource>()[1];
        timeIsEndingSound = GetComponents<AudioSource>()[4];
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
                hasPlayedEndingSound = true; // Ensure the sound only plays once when reaching 3 seconds
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
        tiesCount++;
        TiesCounter.text = tiesCount.ToString();
    }

    public void TicTacToePlayableButtons(int x, int y)
    {
        if (timeIsEndingSound.isPlaying)
        {
            timeIsEndingSound.Stop();
        }

        tictactoeSpaces[x * 3 + y].image.sprite = playerIcons[whoseTurn]; // Display the icon of the player who clicked the button
        tictactoeSpaces[x * 3 + y].interactable = false; // Disable the button after clicking

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

        List<Vector2Int> bestMoves = new List<Vector2Int>(); // Use Vector2Int to store 2D indices

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (markedFields[i, j] == -1)
                {
                    markedFields[i, j] = 1; // AI move
                    int score = MiniMax(markedFields, 0, false, alpha, beta);
                    markedFields[i, j] = -1;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMoves.Clear();
                        bestMoves.Add(new Vector2Int(i, j)); // Add the best move
                    }
                    else if (score == bestScore)
                    {
                        bestMoves.Add(new Vector2Int(i, j));
                    }

                    alpha = Mathf.Max(alpha, bestScore);
                    if (beta <= alpha)
                        break;
                }
            }
        }

        // If there are best moves available
        if (bestMoves.Count > 0)
        {
            Vector2Int move = bestMoves[0];
            Vector2Int leastFrequentMove = GetMostFrequentMove();
            if (bestMoves.Contains(leastFrequentMove))
            {
                move = leastFrequentMove;
            }
            else
            {
                move = bestMoves[Random.Range(0, bestMoves.Count)];
            }

            // Convert the 2D move to a 1D index
            int moveIndex = move.x * 3 + move.y;
            TicTacToePlayableButtons(move.x, move.y);
        }

        currentPlayerTime = playerTimeLimit;
    }

    int MiniMax(int[,] markedFields, int depth, bool isMaximizing, int alpha, int beta)
    {
        int result = CheckWinner();
        if (result != 0)
        {
            return result == 1 ? 10 - depth : result == -1 ? depth - 10 : 0;
        }

        if (depth >= 4)
        {
            return AdjustedEvaluateBoard(markedFields);
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
                        int score = MiniMax(markedFields, depth + 1, false, alpha, beta);
                        markedFields[i, j] = -1;
                        bestScore = Mathf.Max(score, bestScore);
                        alpha = Mathf.Max(alpha, bestScore);

                        if (beta <= alpha)
                            break;
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
                        int score = MiniMax(markedFields, depth + 1, true, alpha, beta);
                        markedFields[i, j] = -1;
                        bestScore = Mathf.Min(score, bestScore);
                        beta = Mathf.Min(beta, bestScore);

                        if (beta <= alpha)
                            break;
                    }
                }
            }

            return bestScore;
        }
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

    Vector2Int GetMostFrequentMove()
    {
        return moveFrequency.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
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
        if (markedFields[1, 1] == ai) score += 10;

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
            if (markedFields[corner[0], corner[1]] == ai) score += 5;
            if (markedFields[corner[0], corner[1]] == player) score -= 5;
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
            if (markedFields[side[0], side[1]] == ai) score += 2;
            if (markedFields[side[0], side[1]] == player) score -= 2;
        }

        // Add additional heuristics if necessary
        // Example: Block opponent's potential winning move
        if (CheckBlockingMove(markedFields, player))
            score -= 20; // Deduct points for defensive moves

        return score;
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

    void DrawGame()
    {
        tiesCount++;
        TiesCounter.text = tiesCount.ToString();
        resultText.text = "It's a draw!";
        resultText.gameObject.SetActive(true);
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

        GameInitialize();
    }
}
