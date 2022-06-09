using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Repository;
using UnityEngine;
using static Utils.Utils;
using Random = UnityEngine.Random;

namespace Service
{
    public class Chromosome
    {
        public List<FurnitureItem> Representation;
        private readonly DataSource _dataSource;
        private readonly Parameters _parameters;

        public Chromosome(DataSource dataSource, Parameters parameters)
        {
            _dataSource = dataSource;
            _parameters = parameters;
            Init();
        }

        public Chromosome(DataSource dataSource, Parameters parameters, List<FurnitureItem> representation)
        {
            Representation = representation;
            _dataSource = dataSource;
            _parameters = parameters;
        }

        public Chromosome Copy()
        {
            return new Chromosome(_dataSource, _parameters, Representation.ToList());
        }

        // initial random furniture alignment
        private void Init()
        {
            Representation = new List<FurnitureItem>();
            var bed = _dataSource.Beds[Random.Range(0, _dataSource.Beds.Count)];
            var seat1 = _dataSource.Seats[Random.Range(0, _dataSource.Seats.Count)];
            var seat2 = _dataSource.Seats[Random.Range(0, _dataSource.Seats.Count)];
            var table = _dataSource.Tables[Random.Range(0, _dataSource.Tables.Count)];
            var cabinet = _dataSource.Cabinets[Random.Range(0, _dataSource.Cabinets.Count)];
            var randomModels = new List<int>() {bed, seat1, seat2, table, cabinet};
            for (var i = 5; i < _parameters.NumberOfFurnitureItems; i++)
                randomModels.Add(Random.Range(0, _dataSource.FurnitureModels.Count));

            for (var i = 0; i < _parameters.NumberOfFurnitureItems; i++)
            {
                var furnitureItem = new FurnitureItem(_dataSource.FurnitureModels[randomModels[i]]);
                furnitureItem.Randomize();
                var tries = 0;
                while (DoesOverlap(furnitureItem) && tries < 100)
                {
                    furnitureItem.Randomize();
                    tries++;
                }

                Representation.Add(furnitureItem);
            }
        }

        // checks if furniture item overlaps any other existing items
        private bool DoesOverlap(FurnitureItem furnitureItem)
        {
            foreach (var f in Representation)
                if (furnitureItem.GetOverlapArea(f) > 0)
                    return true;

            return false;
        }

        //  crossover between 2 chromosomes overlapping at a random position
        public Chromosome Crossover(Chromosome otherChromosome)
        {
            var pos = (int) (_parameters.NumberOfFurnitureItems * (0.4f * Random.value + 0.3f));
            var oldRepresentation = Representation.ToList();

            Representation = Representation.GetRange(0, pos);
            for (int i = pos; i < _parameters.NumberOfFurnitureItems; i++)
                Representation.Add(!DoesOverlap(otherChromosome.Representation[i])
                    ? otherChromosome.Representation[i].Copy()
                    : oldRepresentation[i].Copy());

            return new Chromosome(_dataSource, _parameters, Representation.ToList());
        }

        // chance to make a random change to a furniture alignment
        public void Mutation()
        {
            if (Random.value > _parameters.MutationChance)
                return;

            var pos = Random.Range(0, _parameters.NumberOfFurnitureItems);
            var furnitureItem = Representation[pos].Copy();
            Representation.RemoveAt(pos);

            var probability = Random.value;
            if (probability < 0.2)
            {
                furnitureItem.Randomize();
                var tries = 0;
                while (DoesOverlap(furnitureItem) && tries < 100)
                {
                    furnitureItem.Randomize();
                    tries++;
                }
            }
            else
            {
                if (furnitureItem.Model.Type.Equals(FurnitureType.Bed) ||
                    furnitureItem.Model.Type.Equals(FurnitureType.Cabinet))
                    furnitureItem.SnapToClosestWall();
                else
                    furnitureItem.AlignToClosestWall();
                var tries = 0;
                while (DoesOverlap(furnitureItem) && tries < 100)
                {
                    if (furnitureItem.Model.Type.Equals(FurnitureType.Bed) ||
                        furnitureItem.Model.Type.Equals(FurnitureType.Cabinet))
                        furnitureItem.SnapToClosestWall();
                    else
                        furnitureItem.AlignToClosestWall();
                    tries++;
                }
            }

            Representation.Insert(pos, furnitureItem);
        }

        // calculates overall fitness of furniture layout
        public double Fitness()
        {
            return ClearanceCost() * _parameters.ClearanceWeight +
                   CirculationCost() * _parameters.CirculationWeight +
                   PairwiseCost() * _parameters.PairwiseWeight +
                   ConversationCost() * _parameters.ConversationWeight +
                   AnglesCost() * _parameters.AnglesWeight +
                   BalanceCost() * _parameters.BalanceWeight +
                   FurnitureAlignmentCost() * _parameters.FurnitureAlignmentWeight +
                   WallAlignmentCost() * _parameters.WallAlignmentWeight;
        }

        // calculates clearance cost for layout
        private double ClearanceCost()
        {
            return Representation.Sum(furnitureItemA =>
                Representation.Sum(furnitureItemB =>
                    furnitureItemA.GetOverlapArea(furnitureItemB, shouldIncludeClearance: true)));
        }

        // calculates circulation cost for layout
        private int CirculationCost()
        {
            return GetCirculationComponents(Representation);
        }

        // calculates pairwise cost for layout
        private float PairwiseCost()
        {
            var sum = 0f;
            for (var i = 0; i < _parameters.NumberOfFurnitureItems - 1; i++)
            {
                for (var j = i + 1; j < _parameters.NumberOfFurnitureItems; j++)
                {
                    var recommendedDistances = Representation[i].GetPairwiseDistance(Representation[j]);
                    if (recommendedDistances == null)
                        continue;
                    recommendedDistances.Deconstruct(out var min, out var max);
                    var distance = Vector3.Distance(Representation[i].Position, Representation[j].Position);
                    sum += PlateauFunction(distance, min, max);
                }
            }

            return -sum;
        }

        // calculates conversation cost for layout
        private float ConversationCost()
        {
            var sum = 0f;
            for (var i = 0; i < _parameters.NumberOfFurnitureItems - 1; i++)
            {
                for (var j = i + 1; j < _parameters.NumberOfFurnitureItems; j++)
                {
                    var recommendedDistances = Representation[i].GetConversationDistance(Representation[j]);
                    if (recommendedDistances == null)
                        continue;
                    recommendedDistances.Deconstruct(out var min, out var max);
                    var distance = Vector3.Distance(Representation[i].Position, Representation[j].Position);
                    sum += PlateauFunction(distance, min, max);
                }
            }

            return -sum;
        }

        // calculates angle cost for layout
        private double AnglesCost()
        {
            double sum = 0;
            for (var i = 0; i < _parameters.NumberOfFurnitureItems - 1; i++)
            {
                for (var j = i + 1; j < _parameters.NumberOfFurnitureItems; j++)
                {
                    var recommendedConversationDistances = Representation[i].GetConversationDistance(Representation[j]);
                    var recommendedPairwiseDistances = Representation[i].GetPairwiseDistance(Representation[j]);
                    if (recommendedConversationDistances == null && recommendedPairwiseDistances == null)
                        continue;
                    sum += (Math.Cos(DegToRad(Representation[i].Angle - Representation[j].Angle)) + 1) *
                           (Math.Cos(DegToRad(Representation[j].Angle - Representation[i].Angle)) + 1);
                }
            }

            return -sum;
        }

        // calculates balance cost for layout
        private float BalanceCost()
        {
            var sum = Vector3.zero;
            foreach (var furnitureItem in Representation)
                sum += furnitureItem.Bounds.size.x * furnitureItem.Bounds.size.z * furnitureItem.Position;
            return (sum / Representation.Sum(item => item.Bounds.size.x * item.Bounds.size.z)).magnitude;
        }

        // calculates furniture alignment cost for layout
        private double FurnitureAlignmentCost()
        {
            return Representation.Sum(furnitureItemA =>
                Representation.Sum(furnitureItemB =>
                    Math.Cos(DegToRad(4 * (furnitureItemA.Angle - furnitureItemB.Angle)))));
        }

        // calculates wall alignment cost for layout
        private double WallAlignmentCost()
        {
            return Representation.Sum(furnitureItemA =>
                Math.Cos(DegToRad(4 * furnitureItemA.Angle)));
        }
    }
}