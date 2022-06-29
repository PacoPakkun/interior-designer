using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartApp : MonoBehaviour
{
    public InputField width;
    public InputField depth;
    public Button button;

    public int numberOfIterations = 100;
    public int populationSize = 40;
    public double mutationChance = 0.7;
    public int numberOfSelectedIndividuals = 5;
    public double clearanceWeight = 1;
    public double circulationWeight = 1;
    public double pairwiseWeight = 1;
    public double conversationWeight = 1;
    public double anglesWeight = 1;
    public double balanceWeight = 1;
    public double furnitureAlignmentWeight = 1;
    public double wallAlignmentWeight = 1;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(ButtonHandler);
        width.text = Settings.Width.ToString();
        depth.text = Settings.Depth.ToString();

        Settings.NumberOfIterations = numberOfIterations;
        Settings.PopulationSize = populationSize;
        Settings.MutationChance = mutationChance;
        Settings.NumberOfSelectedIndividuals = numberOfSelectedIndividuals;
        Settings.ClearanceWeight = clearanceWeight;
        Settings.CirculationWeight = circulationWeight;
        Settings.PairwiseWeight = pairwiseWeight;
        Settings.ConversationWeight = conversationWeight;
        Settings.AnglesWeight = anglesWeight;
        Settings.BalanceWeight = balanceWeight;
        Settings.FurnitureAlignmentWeight = furnitureAlignmentWeight;
        Settings.WallAlignmentWeight = wallAlignmentWeight;
    }

    private void ButtonHandler()
    {
        Settings.Width = float.Parse(width.text);
        Settings.Depth = float.Parse(depth.text);
        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Single);
    }
}