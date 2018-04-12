using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingData : MonoBehaviour {

    [SerializeField]
    Text text;

    string currentTrainingSide;
    int gamesComplete;
    int totalTrainingWins;
    int totalControlWins;
    int trainingWins;
    int controlWins;
    int controlUpdates;

    GameManager activeManager;
    void Start()
    {
        activeManager = GameManager.GetActive();
    }

    // Update is called once per frame
    void Update () {
        if (activeManager.GetGamesComplete() != gamesComplete ||
            activeManager.GetTotalTrainingWins() != totalTrainingWins ||
            activeManager.GetTotalControlWins() != totalControlWins ||
            activeManager.GetTrainingWins() != trainingWins ||
            activeManager.GetControlWins() != controlWins ||
            activeManager.GetControlUpdates() != controlUpdates)
            UpdateValues();
	}

    private void UpdateValues()
    {
        if (activeManager.GetCurrentTrainingSide() == 1)
            currentTrainingSide = "Black";
        else
            currentTrainingSide = "White";

        gamesComplete = activeManager.GetGamesComplete();
        totalTrainingWins = activeManager.GetTotalTrainingWins();
        totalControlWins = activeManager.GetTotalControlWins();
        trainingWins = activeManager.GetTrainingWins();
        controlWins = activeManager.GetControlWins();
        controlUpdates = activeManager.GetControlUpdates();

        text.text = currentTrainingSide + "\n\n\n" + gamesComplete + "\n" + totalTrainingWins + "\n" + totalControlWins + "\n" + controlUpdates + "\n\n" + trainingWins + "\n" + controlWins;
    }
}
