using Model;
using Repository;
using Service;
using UnityEngine;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using static Utils.Utils;

public class LoadFurniture : MonoBehaviour
{
    public int numberOfFurnitureItems = 5;
    public int numberOfIterations = 100;
    public int populationSize = 50;
    public double mutationChance = 0.5;
    public int numberOfSelectedIndividuals = 3;
    public double clearanceWeight = 1;
    public double circulationWeight = 1;
    public double pairwiseWeight = 1;
    public double conversationWeight = 1;
    public double anglesWeight = 1;
    public double balanceWeight = 1;
    public double furnitureAlignmentWeight = 1;
    public double wallAlignmentWeight = 1;

    void Start()
    {
        // load materials and models
        var repository = new DataSource();

        // generate solution with genetic algorithm
        var parameters = new Parameters(numberOfFurnitureItems, numberOfIterations, populationSize, mutationChance,
            numberOfSelectedIndividuals, clearanceWeight, circulationWeight, pairwiseWeight, conversationWeight,
            anglesWeight, balanceWeight, furnitureAlignmentWeight, wallAlignmentWeight);
        var solution = new GeneticAlgorithm(repository, parameters).GenerateSolution();
        var representation = solution.Representation;
        Debug.Log(solution.Fitness());

        // instantiate solution
        foreach (var furnitureItem in representation)
        {
            var instance = furnitureItem.Instantiate();
            foreach (Transform child in instance)
                child.GetComponent<Renderer>().material =
                    repository.Materials[Random.Range(0, repository.Materials.Count)];
        }
    }
}