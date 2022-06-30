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

        public Chromosome(DataSource dataSource)
        {
            _dataSource = dataSource;
            Init();
        }

        public Chromosome(DataSource dataSource, List<FurnitureItem> representation)
        {
            Representation = representation;
            _dataSource = dataSource;
        }

        public Chromosome Copy()
        {
            return new Chromosome(_dataSource, Representation.ToList());
        }

        // initial random furniture alignment
        private void Init()
        {
            Representation = new List<FurnitureItem>();
            var table = _dataSource.Tables[Random.Range(0, _dataSource.Tables.Count)];
            var seat1 = _dataSource.Seats[Random.Range(0, _dataSource.Seats.Count)];
            var seat2 = _dataSource.Seats[Random.Range(0, _dataSource.Seats.Count)];
            var bed = _dataSource.Beds[Random.Range(0, _dataSource.Beds.Count)];
            var cabinet1 = _dataSource.Cabinets[Random.Range(0, _dataSource.Cabinets.Count)];
            var cabinet2 = _dataSource.Cabinets[Random.Range(0, _dataSource.Cabinets.Count)];
            var randomModels = new List<int>() {table, seat1, seat2, bed, cabinet1, cabinet2};
            for (var i = 6; i < Settings.NumberOfFurnitureItems; i++)
                randomModels.Add(_dataSource.Other[Random.Range(0, _dataSource.Other.Count)]);

            for (var i = 0; i < Settings.NumberOfFurnitureItems; i++)
            {
                var furnitureItem = new FurnitureItem(_dataSource.FurnitureModels[randomModels[i]]);
                for (var tries = 0; tries < 100; tries++)
                {
                    furnitureItem.Randomize(i > 0 ? Representation[0].Position : default);
                    if (!DoesOverlap(furnitureItem)) break;
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
            var pos = (int) (Settings.NumberOfFurnitureItems * (0.4f * Random.value + 0.3f)) + 1;
            var oldRepresentation = Representation.ToList();

            Representation = Representation.GetRange(0, pos);
            for (int i = pos; i < Settings.NumberOfFurnitureItems; i++)
                Representation.Add(!DoesOverlap(otherChromosome.Representation[i])
                    ? otherChromosome.Representation[i].Copy()
                    : oldRepresentation[i].Copy());

            return Copy();
        }

        // chance to make a random change to a furniture alignment
        public void Mutation()
        {
            if (Random.value > Settings.MutationChance)
                return;

            var pos = (int) (Settings.NumberOfFurnitureItems / (Math.Exp(-5.4f * (Random.value - 0.3f)) + 1));
            var furnitureItem = Representation[pos].Copy();
            Representation.RemoveAt(pos);

            // chance to change position
            if (Random.value < 0.8)
            {
                for (var tries = 0; tries < 100; tries++)
                {
                    furnitureItem.Randomize(Representation[0].Position);
                    if (!DoesOverlap(furnitureItem)) break;
                }
            }
            // chance to align
            else
                for (var tries = 0; tries < 100; tries++)
                {
                    furnitureItem.AlignToClosestWall();
                    if (!DoesOverlap(furnitureItem)) break;
                }

            Representation.Insert(pos, furnitureItem);

            if (pos == 0)
                for (pos = 1; pos <= 2; pos++)
                {
                    var f = Representation[pos].Copy();
                    Representation.RemoveAt(pos);
                    for (var tries = 0; tries < 100; tries++)
                    {
                        f.Randomize(furnitureItem.Position);
                        if (!DoesOverlap(f)) break;
                    }

                    Representation.Insert(pos, f);
                }
        }

        // calculates overall fitness of furniture layout
        public double Fitness()
        {
            return ClearanceCost() * Settings.ClearanceWeight +
                   CirculationCost() * Settings.CirculationWeight +
                   PairwiseCost() * Settings.PairwiseWeight +
                   ConversationCost() * Settings.ConversationWeight +
                   AnglesCost() * Settings.AnglesWeight +
                   BalanceCost() * Settings.BalanceWeight +
                   FurnitureAlignmentCost() * Settings.FurnitureAlignmentWeight +
                   WallAlignmentCost() * Settings.WallAlignmentWeight;
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
            for (var i = 0; i < Settings.NumberOfFurnitureItems - 1; i++)
            {
                for (var j = i + 1; j < Settings.NumberOfFurnitureItems; j++)
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
            for (var i = 0; i < Settings.NumberOfFurnitureItems - 1; i++)
            {
                for (var j = i + 1; j < Settings.NumberOfFurnitureItems; j++)
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
            for (var i = 0; i < Settings.NumberOfFurnitureItems - 1; i++)
            {
                for (var j = i + 1; j < Settings.NumberOfFurnitureItems; j++)
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