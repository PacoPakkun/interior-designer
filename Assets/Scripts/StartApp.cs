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

    public int numberOfIterations;
    public int populationSize;
    public double mutationChance;
    public int numberOfSelectedIndividuals;
    public double clearanceWeight;
    public double circulationWeight;
    public double pairwiseWeight;
    public double conversationWeight;
    public double anglesWeight;
    public double balanceWeight;
    public double furnitureAlignmentWeight;
    public double wallAlignmentWeight;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(ButtonHandler);
        width.text = Settings.Width.ToString();
        depth.text = Settings.Depth.ToString();
    }

    private void ButtonHandler()
    {
        if (SceneManager.GetActiveScene().name.Equals("HomeScene"))
        {
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

        Settings.Width = float.Parse(width.text);
        Settings.Depth = float.Parse(depth.text);
        SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Single);
    }
}