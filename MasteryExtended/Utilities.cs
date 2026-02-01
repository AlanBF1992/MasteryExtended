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

        public static int countDigits(this int n) =>
            n == 0 ? 1 : (n > 0 ? 1 : 2) + (int)Math.Log10(Math.Abs((double)n));
    }
}