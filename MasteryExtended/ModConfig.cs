namespace MasteryExtended
{
    /// <summary>The mod configuration class from the player.</summary>
    public sealed class ModConfig
    {
        // Experience each extra mastery level costs.
        public int MasteryExpPerLevel { get; set; } = 30000;
        // If you can open the Mastery Cave with Mastery Levels.
        public bool MasteryCaveAlternateOpening { get; set; } = true;
        // Mastery required to access the Mastery Room.
        public int MasteryRequiredForCave { get; set; } = 5;
        // Provide confirmation of profession added.
        public bool ConfirmProfession { get; set; } = true;
        // See title of Skill on Menu Hover.
        public bool SkillNameOnMenuHover { get; set; } = true;
        // See title of Profession on Menu Hover.
        public bool ProfessionNameOnMenuHover { get; set; } = false;
        // Order of the unlocking.
        public string PillarsVsProfessions { get; set; } = "Professions required for Pillars";
        // Require n professions for the pillars, with 2<=n<=6.
        public int RequiredProfessionForPillars { get; set; } = 3;
        // Require n pillars for the pedestal, with 1<=n<=5.
        public int RequiredPilarsToThePedestal { get; set; } = 3;
        // Percentage of Exp that goes to Mastery between level 11 and 20.
        public int MasteryPercentage { get; set; } = 20;
        // VPP: Level 10 Professions required to get a Level 15.
        public int Lvl10ProfessionsRequired { get; set; } = 2;
        // VPP: Level 15 Professions required to get an extra combo.
        public int Lvl15ProfessionsRequired { get; set; } = 4;
    }
}