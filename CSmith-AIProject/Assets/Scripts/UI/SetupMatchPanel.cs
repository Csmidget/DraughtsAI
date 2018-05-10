using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SetupMatchPanel : MonoBehaviour {

    [SerializeField]
    GameObject setupPanel;

    [SerializeField]
    Text p1Type;
    [SerializeField]
    Text p2Type;

    [SerializeField]
    Text p1SearchDepth;
    [SerializeField]
    Text p2SearchDepth;

    [SerializeField]
    Text p1nnFileName;
    [SerializeField]
    Text p2nnFileName;

    [SerializeField]
    Text gameCount;

    public void Start()
    {
        EventManager.RegisterToEvent("tournamentComplete", Reactivate);
    }

    public void Reactivate()
    {
        gameObject.SetActive(true);
    }

    public void BeginGame()
    {
        PlayerType p1TypeVal;
        PlayerType p2TypeVal;

        int p1SearchDepthVal = 0;
        int p2SearchDepthVal = 0;
        int gameCountVal;

        switch(p1Type.text)
        {
            case "AI":
                p1TypeVal = PlayerType.AI;
                break;
            case "ADRNG AI":
                p1TypeVal = PlayerType.ADRNG;
                break;
            case "DROSAS AI":
                p1TypeVal = PlayerType.DROSAS;
                break;
            case "ADRAS AI":
                p1TypeVal = PlayerType.ADRAS;
                break;
            default:
                p1TypeVal = PlayerType.Human;
                break;
        }
        switch (p2Type.text)
        {
            case "AI":
                p2TypeVal = PlayerType.AI;
                break;
            case "ADRNG AI":
                p2TypeVal = PlayerType.ADRNG;
                break;
            case "DROSAS AI":
                p2TypeVal = PlayerType.DROSAS;
                break;
            case "ADRAS AI":
                p2TypeVal = PlayerType.ADRAS;
                break;
            default:
                p2TypeVal = PlayerType.Human;
                break;
        }    

        if (p1TypeVal != PlayerType.Human)
        {
            if (!int.TryParse(p1SearchDepth.text, out p1SearchDepthVal))
            {
                return;
            }
        }

        if (p2TypeVal != PlayerType.Human)
        {
            if (!int.TryParse(p2SearchDepth.text, out p2SearchDepthVal))
            {
                return;
            }
        }

        if (!int.TryParse(gameCount.text, out gameCountVal))
        {
            gameCountVal = 1;
        }

        if (!File.Exists("NetWeights\\" + p1nnFileName.text) && p1nnFileName.text != "")
            return;

        if (!File.Exists("NetWeights\\" + p2nnFileName.text) && p2nnFileName.text != "")
            return;

        GameManager.GetActive().InitTournament(p1TypeVal, p2TypeVal, p1SearchDepthVal, p2SearchDepthVal, p1nnFileName.text, p2nnFileName.text,gameCountVal);
        gameObject.SetActive(false);
    }

    public void Back()
    {
        setupPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
