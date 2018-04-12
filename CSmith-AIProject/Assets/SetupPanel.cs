using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetupPanel : MonoBehaviour {

    [SerializeField]
    Text alpha;
    [SerializeField]
    Text lambda;
    [SerializeField]
    Text maxIterations;
    [SerializeField]
    Text nnFileName;

    private void Start()
    {
        if (GameManager.GetActive().training == false)
            gameObject.SetActive(false);
    }

    public void BeginGame()
    {
        double alphaVal;
        double lambdaVal;
        int maxItersVal;
 
        if (double.TryParse(alpha.text,out alphaVal) &&
            double.TryParse(lambda.text,out lambdaVal) &&
            int.TryParse(maxIterations.text,out maxItersVal))
        {
            GameManager.GetActive().InitTrainingGame(alphaVal, lambdaVal, maxItersVal, nnFileName.text);
            gameObject.SetActive(false);
        }



        
    }


}
