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
    }
}