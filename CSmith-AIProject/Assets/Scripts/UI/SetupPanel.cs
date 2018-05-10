using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupPanel : MonoBehaviour {

    public GameObject setupNormalMatch;

    [SerializeField]
    Text alpha;
    [SerializeField]
    Text lambda;
    [SerializeField]
    Text maxIterations;
    [SerializeField]
    Text searchDepth;
    [SerializeField]
    Text nnFileName;

    private void Start()
    {
        EventManager.RegisterToEvent("trainingComplete", Reactivate);
    }

    private void Reactivate()
    {
        gameObject.SetActive(true);
    }

    public void BeginGame()
    {
        double alphaVal;
        double lambdaVal;
        int maxItersVal;
        int searchDepthVal;
        if (double.TryParse(alpha.text,out alphaVal) &&
            double.TryParse(lambda.text,out lambdaVal) &&
            int.TryParse(maxIterations.text,out maxItersVal) && 
            int.TryParse(searchDepth.text,out searchDepthVal))
        {
            GameManager.GetActive().InitTrainingGame(alphaVal, lambdaVal, maxItersVal, nnFileName.text, searchDepthVal);
            gameObject.SetActive(false);
        }       
    }

    public void NonTrainingMatch()
    {
        setupNormalMatch.SetActive(true);
        gameObject.SetActive(false);
    }


}
