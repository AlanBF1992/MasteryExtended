namespace MasteryExtended
{
    internal class ModConfig
    {
        /// <summary>Experience each extra mastery level costs.</summary>
        public int MasteryExpPerLevel { get; set; } = 30000;
        /// <summary>Mastery required to access the Mastery Room.</summary>
        public int MasteryRequiredForCave { get; set; } = 5;
        /// <summary>Require 3 professions for the pillars</summary>
        public bool ExtraRequiredProfession { get; set; } = true;
    }
}