using StardewValley;
using StardewValley.Delegates;

namespace MasteryExtended
{
    internal static class ModQueries
    {
        internal static void addGameStateQueries()
        {
            GameStateQuery.Register($"{ModEntry.ModManifest.UniqueID}_BookQuantity", bookQuantityQuery);
        }
        private static bool bookQuantityQuery(string[] query, GameStateQueryContext context)
        {
            if (ArgUtility.Get(query, 1) is not string enumCheck
                || !Enum.TryParse(enumCheck, out BooksQuantityOption result))
            {
                return false;
            }

            return result == ModEntry.Config.BooksQuantity;
        }
    }
}
