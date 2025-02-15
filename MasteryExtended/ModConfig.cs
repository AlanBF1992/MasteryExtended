namespace MasteryExtended
{
    /// <summary>The mod configuration class from the player.</summary>
    public sealed class ModConfig
    {
        /// <summary>Experience each extra mastery level costs.</summary>
        public int MasteryExpPerLevel { get; set; } = 30000;
        /// <summary>Mastery required to access the Mastery Room.</summary>
        public int MasteryRequiredForCave { get; set; } = 5;
        /// <summary>Require 3 professions for the pillars</summary>
        public bool ExtraRequiredProfession { get; set; } = true;
        /// <summary>Percentage of Exp that goes to Mastery between level 11 and 20</summary>
        public int MasteryPercentage { get; set; } = 20;
        /// <summary>VPP: Level 10 Professions required to get a Level 15.</summary>
        public int Lvl10ProfessionsRequired { get; set; } = 2;
        /// <summary>VPP: Level 15 Professions required to get an extra combo.</summary>
        public int Lvl15ProfessionsRequired { get; set; } = 4;
    }
}