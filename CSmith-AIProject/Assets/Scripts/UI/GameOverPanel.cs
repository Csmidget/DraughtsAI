using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour {

    [SerializeField]
    Text gameOverText;

    GameManager gameManager;

	// Use this for initialization
	void Start () {
        EventManager.RegisterToEvent("gameOver", DisplayPanel);
        EventManager.RegisterToEvent("gameReset", HidePanel);
        gameObject.SetActive(false);
        gameManager = GameManager.GetActive();
	}

    void HidePanel()
    {
        gameObject.SetActive(false);
    }
	
    void DisplayPanel()
    {
        int winner = gameManager.GetWinner();
        if (winner == 1)
            gameOverText.text = "Black Wins!";
        else if (winner == 2)
            gameOverText.text = "White Wins!";
        else
            return;

        gameObject.SetActive(true);

    }

}
