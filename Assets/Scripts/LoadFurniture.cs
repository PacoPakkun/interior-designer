using Model;
using Repository;
using Service;
using UnityEngine;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public class LoadFurniture : MonoBehaviour
{
    private DataSource _repository;

    void Start()
    {
        // load materials and models
        _repository = new DataSource();

        // generate room floor and walls
        GenerateRoom();

        // generate solution with genetic algorithm
        var solution = new GeneticAlgorithm(_repository).GenerateSolution();
        var representation = solution.Representation;
        Debug.Log(solution.Fitness());

        // instantiate solution
        foreach (var furnitureItem in representation)
        {
            var instance = furnitureItem.Instantiate();
            foreach (Transform child in instance)
                child.GetComponent<Renderer>().material =
                    _repository.Materials[Random.Range(0, _repository.Materials.Count)];
        }
    }

    private void GenerateRoom()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.localPosition = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(Settings.Width, 0.01f, Settings.Depth);
        floor.GetComponent<Renderer>().material = _repository.Materials[7];

        GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall1.transform.localPosition = new Vector3(0, 0.75f, Settings.Depth / 2);
        wall1.transform.localScale = new Vector3(Settings.Width, 1.5f, 0.1f);
        wall1.GetComponent<Renderer>().material = _repository.Materials[9];

        GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall2.transform.localPosition = new Vector3(-Settings.Width / 2, 0.75f, 0);
        wall2.transform.localScale = new Vector3(0.1f, 1.5f, Settings.Depth);
        wall2.GetComponent<Renderer>().material = _repository.Materials[9];

        GameObject wall3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall3.transform.localPosition = new Vector3(Settings.Width / 2, 0.75f, 0);
        wall3.transform.localScale = new Vector3(0.1f, 1.5f, Settings.Depth);
        wall3.GetComponent<Renderer>().material = _repository.Materials[9];
    }
}