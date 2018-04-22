using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


	public void BeginGame()
    {
        PlayerType p1TypeVal;
        PlayerType p2TypeVal;

        int p1SearchDepthVal = 0;
        int p2SearchDepthVal = 0;

        if (p1Type.text == "AI") p1TypeVal = PlayerType.AI;
        else p1TypeVal = PlayerType.Human;

        if (p2Type.text == "AI") p2TypeVal = PlayerType.AI;
        else p2TypeVal = PlayerType.Human;

        if (p1TypeVal == PlayerType.AI)
        {
            if (!int.TryParse(p1SearchDepth.text, out p1SearchDepthVal))
            {
                return;
            }
        }

        if (p2TypeVal == PlayerType.AI)
        {
            if (!int.TryParse(p2SearchDepth.text, out p2SearchDepthVal))
            {
                return;
            }
        }

        GameManager.GetActive().InitNormalGame(p1TypeVal, p2TypeVal, p1SearchDepthVal, p2SearchDepthVal, p1nnFileName.text, p2nnFileName.text);
        gameObject.SetActive(false);
    }

    public void Back()
    {
        setupPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
