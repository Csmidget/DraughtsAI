using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardRatingText : MonoBehaviour {

    [SerializeField]
    Text p1RatingText;
    [SerializeField]
    Text p2RatingText;

	// Use this for initialization
	void Start () {
        EventManager.RegisterToEvent("gameReset", UpdateFields);
        EventManager.RegisterToEvent("gameOver" , UpdateFields);
    }
	

	void UpdateFields () {
        p1RatingText.text = "P1: " + GameManager.GetActive().GetP1LastAvgRatingDiff().ToString("#.####");
        p2RatingText.text = "P2: " + GameManager.GetActive().GetP2LastAvgRatingDiff().ToString("#.####"); ;
    }
}
