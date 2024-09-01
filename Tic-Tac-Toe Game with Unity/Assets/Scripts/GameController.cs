using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public int whoseTurn; // 0 = x and 1 = o
    public int turnCount;
    public GameObject[] turnIcons;
    public Sprite[] playerIcons;
    public Button[] tictactoeSpaces; //playable space for out game
    public int[] markedFields;

    // Start is called before the first frame update
    void Start()
    {
        GameInitialize();
    }

    void GameInitialize()
    {
        whoseTurn = 0;
        turnCount = 0;
        turnIcons[0].SetActive(true);
        turnIcons[1].SetActive(false);
        markedFields = new int[9];

        for(int i = 0; i < tictactoeSpaces.Length; i++)
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
        
    }

    //Paramater variable is showing which button in the grid is clicked
    public void TicTacToePlayableButtons(int whichButton)
    {
        tictactoeSpaces[whichButton].image.sprite = playerIcons[whoseTurn]; // displaying the icon of the player who clicked the button (X for X player, O for O player

        tictactoeSpaces[whichButton].interactable = false; //Making sure that the button can't be clicked more than once. After clicking you can't interact with it.

        turnCount++;
        markedFields[whichButton] = whoseTurn;

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
            }
        }
    }

    void ComputerMove()
    {
        int bestScore = int.MinValue;
        int move = -1;

        for (int i = 0; i < markedFields.Length; i++)
        {
            if (markedFields[i] == -1)
            {
                markedFields[i] = 1;
                int score = MiniMax(markedFields, 0, false);
                markedFields[i] = -1;

                if(score > bestScore)
                {
                    bestScore = score;
                    move = i;
                }
            }
        }

        if (move != -1)
        {
            TicTacToePlayableButtons(move);
        }
    }

    int MiniMax(int[] markedFields, int depth, bool isMaximizing)
    {
        int result = CheckWinner();
        if (result != 0)
        {
            return result;
        }

        if (isMaximizing)
        {
            int bestScore = int.MinValue;

            for (int i = 0; i < markedFields.Length; i++)
            {
                if (markedFields[i] == -1)
                {
                    markedFields[i] = 1; // AI move
                    int score = MiniMax(markedFields, depth + 1, false);
                    markedFields[i] = -1;
                    bestScore = Mathf.Max(score, bestScore);
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
                    int score = MiniMax(markedFields, depth + 1, true);
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
        string result = (whoseTurn == 0) ? "X wins!" : "O wins!";
        Debug.Log(result);
    }


    void DrawGame()
    {
        Debug.Log("It's a draw!");
    }
}
