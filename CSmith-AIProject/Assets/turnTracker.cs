using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class turnTracker : MonoBehaviour {

    private Text text;
    private GameManager gameManager;

    private void Start()
    {
        text = GetComponent<Text>();
        gameManager = GameManager.GetActive();
        EventManager.RegisterToEvent("gameReset", SetTracker);
        EventManager.RegisterToEvent("turnOver", SetTracker);
    }

	void SetTracker () {

		if (gameManager.GetActiveSide() == 1)
        {
            text.text = "Black's Turn";
        }
        else
        {
            text.text = "White's Turn";
        }
	}
}
