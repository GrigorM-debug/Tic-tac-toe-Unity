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

        for(int i = 0; i < tictactoeSpaces.Length; i++)
        {
            tictactoeSpaces[i].interactable = true;
            tictactoeSpaces[i].GetComponent<Image>().sprite = null;
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

        if (whoseTurn == 0)
        {
            whoseTurn = 1;
            turnIcons[0].SetActive(false);
            turnIcons[1].SetActive(true);
        }
        else
        {
            whoseTurn = 0;
            turnIcons[0].SetActive(true);
            turnIcons[1].SetActive(false);
        }
    }
}
