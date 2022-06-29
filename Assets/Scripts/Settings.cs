public static class Settings
{
    public static float Width { get; set; } = 7;
    public static float Depth { get; set; } = 5;
    public static int NumberOfFurnitureItems { get; set; } = (int) (Width * Depth / 5);
    public static int NumberOfIterations { get; set; }
    public static int PopulationSize { get; set; }
    public static double MutationChance { get; set; }
    public static int NumberOfSelectedIndividuals { get; set; }
    public static double ClearanceWeight { get; set; }
    public static double CirculationWeight { get; set; }
    public static double PairwiseWeight { get; set; }
    public static double ConversationWeight { get; set; }
    public static double AnglesWeight { get; set; }
    public static double BalanceWeight { get; set; }
    public static double FurnitureAlignmentWeight { get; set; }
    public static double WallAlignmentWeight { get; set; } = 1;
}