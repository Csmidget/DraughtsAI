using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentDataPanel : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        EventManager.RegisterToEvent("gameReset", CheckStatus);
    }

    void CheckStatus()
    {
        if (GameManager.GetActive().IsTournament())
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
}
