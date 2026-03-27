using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Constants;

namespace MasteryExtended
{
    internal static class Utilities
    {
        internal static int CountClaimedPillars()
        {
            return (int)Game1.player.stats.Get("mastery_total_pillars");
        }

        internal static void SetMasteryPillarsClaimed()
        {
            uint count = 0;
            for (int i = 0; i < 5; i++)
            {
                count += Game1.player.stats.Get(StatKeys.Mastery(i));
            }
            Game1.player.stats.Set("mastery_total_pillars", count);
        }

        internal static int countDigits(this int n) =>
            n == 0 ? 1 : (n > 0 ? 1 : 2) + (int)Math.Log10(Math.Abs((double)n));

        internal static List<Vector2> getListOfTileLocationsForTileRectangle(Rectangle area)
        {
            int leftTile = area.Left / 64;
            int rightTile = area.Right / 64;
            int topTile = area.Top / 64;
            int bottomTile = area.Bottom / 64;

            List<Vector2> result = new((rightTile - leftTile + 1) * (bottomTile - topTile + 1));

            for (int x = leftTile; x <= rightTile; x++)
            {
                for (int y = topTile; y <= bottomTile; y++)
                {
                    result.Add(new Vector2(x, y));
                }
            }

            return result;
        }
    }
}