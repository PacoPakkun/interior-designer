using System.Collections.Generic;
using System.Linq;
using Model;
using Repository;
using UnityEngine;

namespace Service
{
    public class GeneticAlgorithm
    {
        private List<Chromosome> _population;
        private readonly DataSource _dataSource;

        public GeneticAlgorithm(DataSource dataSource)
        {
            _dataSource = dataSource;
            Populate();
        }

        // initial random population of chromosomes
        private void Populate()
        {
            _population = new List<Chromosome>();
            for (int i = 0; i < Settings.PopulationSize; i++)
            {
                _population.Add(new Chromosome(_dataSource));
            }
        }

        // runs the genetic algorithm iterations
        public Chromosome GenerateSolution()
        {
            for (var i = 0; i < Settings.NumberOfIterations; i++)
            {
                var newPopulation = new List<Chromosome>();
                var newPopulationMutated = new List<Chromosome>();
                for (var j = 0; j < Settings.PopulationSize * 70 / 100; j++)
                {
                    var pos = Selection();
                    var candidate = _population[pos];
                    _population.RemoveAt(pos);

                    newPopulation.Add(candidate.Copy());
                    candidate.Mutation();
                    newPopulationMutated.Add(candidate);
                }

                for (var j = 0; j < Settings.PopulationSize * 30 / 100; j++)
                {
                    var posA = Random.Range(0, newPopulation.Count);
                    var posB = Random.Range(0, newPopulation.Count);
                    while (posA == posB)
                        posB = Random.Range(0, newPopulation.Count);

                    var offspring = newPopulation[posA].Crossover(newPopulation[posB]);
                    offspring.Mutation();
                    newPopulationMutated.Add(offspring.Copy());
                }

                _population = newPopulationMutated;
            }

            return _population.Aggregate((a, b) =>
                a.Fitness() < b.Fitness() ? a : b);
        }

        // tournament that selects the best chromosome of a random few
        private int Selection()
        {
            var candidates = new List<int>();
            for (var i = 0; i < Settings.NumberOfSelectedIndividuals; i++)
            {
                var rand = Random.Range(0, _population.Count);
                while (candidates.Contains(rand))
                    rand = Random.Range(0, _population.Count);
                candidates.Add(rand);
            }

            candidates.Sort((a, b) => _population[a].Fitness().CompareTo(_population[b].Fitness()));
            return candidates.First();
        }
    }
}