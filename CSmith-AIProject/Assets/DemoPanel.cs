using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoPanel : MonoBehaviour {

 //   [SerializeField]
 //   GameObject demoPanel;

    [SerializeField]
    Text gameCount;

    [SerializeField]
    Text p1Type;
    [SerializeField]
    Text p2Type;

    [SerializeField]
    GameObject p1DDATitle;
    [SerializeField]
    GameObject p2DDATitle;
    [SerializeField]
    GameObject p1DDADropDown;
    [SerializeField]
    GameObject p2DDADropDown;

    [SerializeField]
    Text p1DDA;
    [SerializeField]
    Text p2DDA;



    string p1NN;
    string p2NN;

    // Use this for initialization
    public void Start()
    {
        EventManager.RegisterToEvent("tournamentComplete", Reactivate);
        PlayerTypeUpdated();
    }

    public void BeginGame()
    {
        PlayerType p1TypeVal;
        PlayerType p2TypeVal;

        int p1SearchDepthVal = 0;
        int p2SearchDepthVal = 0;
        int gameCountVal;

        switch (p1Type.text)
        {
            case "Weak AI":
                p1TypeVal = PlayerType.AI;
                p1NN = "7";
                break;
            case "Strong AI":
                p1TypeVal = PlayerType.ADRNG;
                p1NN = "24";
                break;
            default:
                p1TypeVal = PlayerType.Human;
                break;
        }
        switch (p2Type.text)
        {
            case "Weak AI":
                p2TypeVal = PlayerType.AI;
                p2NN = "7";
                break;
            case "Strong AI":
                p2TypeVal = PlayerType.ADRNG;
                p2NN = "24";
                break;
            default:
                p2TypeVal = PlayerType.Human;
                break;
        }

        if(p1TypeVal == PlayerType.ADRNG)
        {
            switch (p1DDA.text)
            {
                case "None":
                    p1TypeVal = PlayerType.AI;
                    break;
                case "DROSAS":
                    p1TypeVal = PlayerType.DROSAS;
                    break;
                case "ADRAS":
                    p1TypeVal = PlayerType.ADRAS;
                    break;
            }
        }
        if (p2TypeVal == PlayerType.ADRNG)
        {
            switch (p2DDA.text)
            {
                case "None":
                    p2TypeVal = PlayerType.AI;
                    break;
                case "DROSAS":
                    p2TypeVal = PlayerType.DROSAS;
                    break;
                case "ADRAS":
                    p2TypeVal = PlayerType.ADRAS;
                    break;
            }
        }


        if (!int.TryParse(gameCount.text, out gameCountVal))
        {
            gameCountVal = 1;
        }



        GameManager.GetActive().InitTournament(p1TypeVal, p2TypeVal, 6, 6, p2NN, p2NN, gameCountVal);
        gameObject.SetActive(false);
    }

    public void PlayerTypeUpdated()
    {
        if (p1Type.text != "Strong AI")
        {
            p1DDADropDown.SetActive(false);
            p1DDATitle.SetActive(false);
        }
        else
        {
            p1DDADropDown.SetActive(true);
            p1DDATitle.SetActive(true);
        }

        if (p2Type.text != "Strong AI")
        {
            p2DDADropDown.SetActive(false);
            p2DDATitle.SetActive(false);
        }
        else
        {
            p2DDADropDown.SetActive(true);
            p2DDATitle.SetActive(true);
        }

    }

    public void Reactivate()
    {
        gameObject.SetActive(true);
    }
}
