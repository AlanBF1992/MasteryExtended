namespace MasteryExtended
{
    public enum BooksQuantityOption
    {
        Full = 0,
        Lite = 1,
        None = 2
    }

    public enum SkillsVsMasteryPointsOption
    {
        SkillOrMastery  = 0,
        Skill           = 1,
        Mastery         = 2,
        SkillAndMastery = 3
    }

    public enum PillarsVsProfessionsOption
    {
        Professions = 0,
        Pillars = 1,
        None = 2
    }

    public enum MasonDropsOption
    {
        Everything = 0,
        StoneAndClay = 1
    }

    /// <summary>The mod configuration class from the player.</summary>
    public sealed class ModConfig
    {
        // Experience each extra mastery level costs.
        public int MasteryExpPerLevel { get; set; } = 30000;
        // Book Quantity Config
        public BooksQuantityOption BooksQuantity { get; set; } = BooksQuantityOption.Full;
        // Book Price
        public int BookPrice { get; set; } = 25000;
        // Enable extra Powers
        public bool EnableDogPowers { get; set; } = true;
        // What to drop with Mason
        public MasonDropsOption MasonDrops { get; set; } = MasonDropsOption.Everything;
        // See title of Skill on Menu Hover.
        public bool SkillNameOnMenuHover { get; set; } = true;
        // See title of Profession on Menu Hover.
        public bool ProfessionNameOnMenuHover { get; set; } = false;
        // How to unlock the Mastery Room
        public SkillsVsMasteryPointsOption SkillsVsMasteryPoints { get; set; } = SkillsVsMasteryPointsOption.SkillOrMastery;
        // Include custom Skills?
        public bool IncludeCustomSkills { get; set; } = true;
        // Amount of level 10 skills required to access the Mastery Room
        public int SkillsRequiredForMasteryRoom { get; set; } = 5;
        // Mastery required to access the Mastery Room.
        public int MasteryRequiredForCave { get; set; } = 5;
        // Order of the unlocking.
        public PillarsVsProfessionsOption PillarsVsProfessions { get; set; } = PillarsVsProfessionsOption.Professions;
        // Require n professions for the pillars, with 2<=n<=6.
        public int RequiredProfessionForPillars { get; set; } = 3;
        // Require n pillars for the pedestal, with 1<=n<=5.
        public int RequiredPilarsToThePedestal { get; set; } = 3;
        // Provide confirmation of profession added.
        public bool ConfirmProfession { get; set; } = true;
        // Percentage of Exp that goes to Mastery between level 11 and 20.
        public int MasteryPercentage { get; set; } = 20;
        // VPP: Level 10 Professions required to get a Level 15.
        public int Lvl10ProfessionsRequired { get; set; } = 2;
        // VPP: Level 15 Professions required to get an extra combo.
        public int Lvl15ProfessionsRequired { get; set; } = 4;
    }
}