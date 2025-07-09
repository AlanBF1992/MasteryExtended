using StardewValley;

namespace MasteryExtended
{
    internal static class Utilities
    {
        internal static int countClaimedPillars()
        {
            List<string> checkRecipeList = ["Statue Of Blessings", "Heavy Furnace", "Challenge Bait", "Treasure Totem", "Anvil"];
            return checkRecipeList.Count(x => Game1.player.craftingRecipes.ContainsKey(x));
        }

        public static int countDigits(this int n) =>
            n == 0 ? 1 : (n > 0 ? 1 : 2) + (int)Math.Log10(Math.Abs((double)n));
    }
}