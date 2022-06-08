namespace Model
{
    public class Parameters
    {
        public readonly int NumberOfFurnitureItems;
        public readonly int NumberOfIterations;
        public readonly int PopulationSize;
        public readonly double MutationChance = 50;
        public readonly int NumberOfSelectedIndividuals = 3;
        public readonly double ClearanceWeight;
        public readonly double CirculationWeight;
        public readonly double PairwiseWeight;
        public readonly double ConversationWeight;
        public readonly double AnglesWeight;
        public readonly double BalanceWeight;
        public readonly double FurnitureAlignmentWeight;
        public readonly double WallAlignmentWeight;

        public Parameters(int numberOfFurnitureItems, int numberOfIterations, int populationSize, double mutationChance,
            int numberOfSelectedIndividuals, double clearanceWeight, double circulationWeight, double pairwiseWeight,
            double conversationWeight, double anglesWeight, double balanceWeight, double furnitureAlignmentWeight,
            double wallAlignmentWeight)
        {
            this.NumberOfFurnitureItems = numberOfFurnitureItems;
            this.NumberOfIterations = numberOfIterations;
            this.PopulationSize = populationSize;
            this.MutationChance = mutationChance;
            this.NumberOfSelectedIndividuals = numberOfSelectedIndividuals;
            this.ClearanceWeight = clearanceWeight;
            this.CirculationWeight = circulationWeight;
            this.PairwiseWeight = pairwiseWeight;
            this.ConversationWeight = conversationWeight;
            this.AnglesWeight = anglesWeight;
            this.BalanceWeight = balanceWeight;
            this.FurnitureAlignmentWeight = furnitureAlignmentWeight;
            this.WallAlignmentWeight = wallAlignmentWeight;
        }
    }
}