using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TounamentData : MonoBehaviour {

    [SerializeField]
    Text valuesText;
    [SerializeField]
    Text sideText;

    PlayerType p1Type;
    PlayerType p2Type;

    string p1String;
    string p2String;
    string p1SideString;
    string p2SideString;

    int p1Side;

    int p1Wins;
    int p2Wins;

    GameManager activeManager;

    // Use this for initialization
    void Start () {
        activeManager = GameManager.GetActive();
        EventManager.RegisterToEvent("gameReset", RunUpdate);
        EventManager.RegisterToEvent("gameOver", RunUpdate);
    }
	
	// Update is called once per frame
	void RunUpdate () {
        if (p1Type != activeManager.GetP1Type() ||
            p2Type != activeManager.GetP2Type() ||
            p1Wins != activeManager.GetP1Wins() ||
            p2Wins != activeManager.GetP2Wins() ||
            p1Side != activeManager.GetP1Side())
            UpdateValues();
	}

    void UpdateValues()
    {

        p1Type = activeManager.GetP1Type();
        p2Type = activeManager.GetP2Type();
        p1Wins = activeManager.GetP1Wins();
        p2Wins = activeManager.GetP2Wins();

        if (p1Type == PlayerType.AI)
            p1String = "AI";
        else if (p1Type == PlayerType.Human)
            p1String = "Human";

        if (p2Type == PlayerType.AI)
            p2String = "AI";
        else if (p2Type == PlayerType.Human)
            p2String = "Human";

        p1Side = activeManager.GetP1Side();

        if (p1Side == 1)
        {
            p1SideString = "Black";
            p2SideString = "White";
        }
        else
        {
            p1SideString = "White";
            p2SideString = "Black";
        }


        valuesText.text = p1String + "\n" + p2String + "\n\n" + p1Wins + "\n" + p2Wins;
        sideText.text = p1SideString + "\n" + p2SideString;
    }
}
