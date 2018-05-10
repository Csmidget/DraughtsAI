using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDataPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
        EventManager.RegisterToEvent("gameReset", CheckStatus);
	}

    void CheckStatus()
    {
        if (GameManager.GetActive().IsTraining())
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
}
