using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameController : MonoBehaviour
{
    public int whoseTurn; // 0 = x and 1 = o
    public int turnCount;
    public GameObject[] turnIcons;
    public Sprite[] playerIcons;
    public Button[] tictactoeSpaces; //playable space for out game
    public int[] markedFields;
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


    // Start is called before the first frame update
    void Start()
    {
        GameInitialize();

        //moveSound.Play();

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
        markedFields = new int[9];
        resultText.gameObject.SetActive(false);
        isTimerRunning = true;
        currentPlayerTime = playerTimeLimit;
        rematchButton.gameObject.SetActive(false);
        hasPlayedEndingSound = false;
        //Game history
        //XWinsCount.text = "0";
        //OWinsCounter.text = "0";
        //TiesCounter.text = "0";

        XWinsCount.text = currXWinCount.ToString();
        OWinsCounter.text = currOWinsCounter.ToString();
        TiesCounter.text = tiesCount.ToString();

        for (int i = 0; i < tictactoeSpaces.Length; i++)
        {
            tictactoeSpaces[i].interactable = true;
            tictactoeSpaces[i].GetComponent<Image>().sprite = null;
        }

        for (int i = 0; i < markedFields.Length; i++) 
        {
            markedFields[i] = -1;
        }
    }

    // Update is called once per frame
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

        rematchButton.gameObject.SetActive(true );

        tiesCount++;
        TiesCounter.text = tiesCount.ToString();
    }

    //Paramater variable is showing which button in the grid is clicked
    public void TicTacToePlayableButtons(int whichButton)
    {
        if (timeIsEndingSound.isPlaying)
        {
            timeIsEndingSound.Stop();
        }

        tictactoeSpaces[whichButton].image.sprite = playerIcons[whoseTurn]; // displaying the icon of the player who clicked the button (X for X player, O for O player

        tictactoeSpaces[whichButton].interactable = false; //Making sure that the button can't be clicked more than once. After clicking you can't interact with it.

        turnCount++;
        markedFields[whichButton] = whoseTurn;

        moveSound.Play();

        if(CheckWin())
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
        //int move = -1;

        // Alpha and beta should be initialized to int.MinValue and int.MaxValue respectively
        int alpha = int.MinValue;
        int beta = int.MaxValue;

        List<int> bestMoves = new List<int>();

        for (int i = 0; i < markedFields.Length; i++)
        {
            if (markedFields[i] == -1)
            {
                markedFields[i] = 1;
                int score = MiniMax(markedFields, 0, false, alpha, beta);
                markedFields[i] = -1;

                if (score > bestScore)
                {
                    bestScore = score;
                    //move = i;
                    bestMoves.Clear();
                    bestMoves.Add(i); // score the best move
                }
                else if (score == bestScore) 
                {
                    bestMoves.Add(i);
                }
            }
        }

        //if (move != -1)
        //{
        //    TicTacToePlayableButtons(move);
        //}

        if (bestMoves.Count > 0) 
        {
            int move = bestMoves[Random.Range(0, bestMoves.Count)];
            TicTacToePlayableButtons(move);
        }

        currentPlayerTime = playerTimeLimit;
    }

    //Added Alpha–beta pruning optimisation
    int MiniMax(int[] markedFields, int depth, bool isMaximizing, int alpha, int beta)
    {
        int result = CheckWinner();
        if (result != 0)
        {
            //return result;
            return result == 1 ? 10 : result == -1 ? -10 : 0;
        }

        //Checking if Computer is on turn
        if (isMaximizing)
        {
            int bestScore = int.MinValue;

            for (int i = 0; i < markedFields.Length; i++)
            {
                if (markedFields[i] == -1)
                {
                    markedFields[i] = 1; // AI move

                    //Calling the function but this time is Player move - isMaximazing = false
                    int score = MiniMax(markedFields, depth + 1, false, alpha, beta);
                    markedFields[i] = -1;
                    bestScore = Mathf.Max(score, bestScore);
                    alpha = Mathf.Max(alpha, bestScore);

                    if(alpha >= beta)
                    {
                        break;
                    }
                }
            }

            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;

            for (int i = 0; i < markedFields.Length; i++)
            {
                if (markedFields[i] == -1)
                {
                    markedFields[i] = 0; // Player's move

                    //Calling the function but this time is Computer move - is Maximizing = true
                    int score = MiniMax(markedFields, depth + 1, true, alpha, beta);
                    markedFields[i] = -1;
                    bestScore = Mathf.Min(score, bestScore);
                }
            }

            return bestScore;
        }
    }

    int CheckWinner()
    {
        int[,] winningCombinations = new int[,] {
            { 0, 1, 2 },
            { 3, 4, 5 },
            { 6, 7, 8 },
            { 0, 3, 6 },
            { 1, 4, 7 },
            { 2, 5, 8 },
            { 0, 4, 8 },
            { 2, 4, 6 }
        };

        for (int i = 0; i < winningCombinations.GetLength(0); i++)
        {
            int a = winningCombinations[i, 0];
            int b = winningCombinations[i, 1];
            int c = winningCombinations[i, 2];

            if (markedFields[a] == markedFields[b] && markedFields[b] == markedFields[c] && markedFields[a] != -1)
            {
                if (markedFields[a] == 0) // Player wins
                {
                    return -10;
                }
                else if (markedFields[a] == 1) // AI wins
                {
                    return 10;
                }
            }
        }

        // Check for a draw
        for (int i = 0; i < markedFields.Length; i++)
        {
            if (markedFields[i] == -1)
            {
                return 0; // Game not over
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

        if(whoseTurn == 0)
        {
            winSound.Play();
            resultText.text = "X wins!";

            currXWinCount = int.Parse(XWinsCount.text);
            currXWinCount++;
            XWinsCount.text = currXWinCount.ToString();
        }
        else
        {
            loseSound.Play();
            resultText.text = "O wins!";
            
            currOWinsCounter = int.Parse(OWinsCounter.text);
            currOWinsCounter++;
            OWinsCounter.text = currOWinsCounter.ToString();
        }

        currentPlayerTime = playerTimeLimit;
        isTimerRunning = false;
        rematchButton.gameObject.SetActive(true);
        //string result = (whoseTurn == 0) ? "X wins!" : "O wins!";

        //resultText.text = result;
        resultText.gameObject.SetActive(true);
        
        //Debug.Log(result);
    }


    void DrawGame()
    {
        //Debug.Log("It's a draw!");
        tiesCount = int.Parse(TiesCounter.text);
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
