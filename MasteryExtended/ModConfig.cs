using StardewModdingAPI;

namespace MasteryExtended
{
    internal class ModConfig
    {
        /// <summary>Experience each extra mastery level costs.</summary>
        public int MasteryExpPerLevel { get; set; } = 30000;
        /// <summary>Mastery required to access the Mastery Room.</summary>
        public int MasteryRequiredForCave { get; set; } = 5;
    }
}