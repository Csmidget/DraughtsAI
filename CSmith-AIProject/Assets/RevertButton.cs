using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevertButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
        EventManager.RegisterToEvent("turnOver",VisibilityCheck);
        EventManager.RegisterToEvent("gameReset",VisibilityCheck);
    }
	
    void VisibilityCheck()
    {
        if (GameManager.GetActive().GetActivePlayerType() == PlayerType.Human && gameObject.activeSelf == false)
            gameObject.SetActive(true);
        else if (gameObject.activeSelf == true)
            gameObject.SetActive(false);

    }

    public void OnPress()
    {
        GameManager.GetActive().RevertTurn();
    }

	// Update is called once per frame
	void Update () {
		
	}
}
