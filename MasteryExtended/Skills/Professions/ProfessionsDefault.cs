using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Skills.Professions
{
    public partial class Profession
    {
        /**************
         * Farming
         **************/
        public static Profession Rancher { get; } = new Profession(0, null, 5);
        public static Profession Tiller { get; } = new Profession(1, null, 5);
        public static Profession Butcher { get; } = new Profession(2, () => LevelUpMenu.getProfessionTitleFromNumber(2), 10, null, null, null, Rancher);
        public static Profession Shepherd { get; } = new Profession(3, () => LevelUpMenu.getProfessionTitleFromNumber(3), 10, null, null, null, Rancher);
        public static Profession Artisan { get; } = new Profession(4, () => LevelUpMenu.getProfessionTitleFromNumber(4), 10, null, null, null, Tiller);
        public static Profession Agriculturist { get; } = new Profession(5, () => LevelUpMenu.getProfessionTitleFromNumber(5), 10, null, null, null, Tiller);

        /**************
         * Fishing
         **************/
        public static Profession Fisher { get; } = new Profession(6, () => LevelUpMenu.getProfessionTitleFromNumber(6), 5);
        public static Profession Trapper { get; } = new Profession(7, () => LevelUpMenu.getProfessionTitleFromNumber(7), 5);
        public static Profession Angler { get; } = new Profession(8, () => LevelUpMenu.getProfessionTitleFromNumber(8), 10, null, null, null, Fisher);
        public static Profession Pirate { get; } = new Profession(9, () => LevelUpMenu.getProfessionTitleFromNumber(9), 10, null, null, null, Fisher);
        public static Profession Baitmaster { get; } = new Profession(10, () => LevelUpMenu.getProfessionTitleFromNumber(10), 10, null, null, null, Trapper);
        public static Profession Mariner { get; } = new Profession(11, () => LevelUpMenu.getProfessionTitleFromNumber(11), 10, null, null, null, Trapper);

        /**************
         * Foraging
         **************/
        public static Profession Forester { get; } = new Profession(12, () => LevelUpMenu.getProfessionTitleFromNumber(12), 5);
        public static Profession Gatherer { get; } = new Profession(13, () => LevelUpMenu.getProfessionTitleFromNumber(13), 5);
        public static Profession Lumberjack { get; } = new Profession(14, () => LevelUpMenu.getProfessionTitleFromNumber(14), 10, null, null, null, Forester);
        public static Profession Tapper { get; } = new Profession(15, () => LevelUpMenu.getProfessionTitleFromNumber(15), 10, null, null, null, Forester);
        public static Profession Botanist { get; } = new Profession(16, () => LevelUpMenu.getProfessionTitleFromNumber(16), 10, null, null, null, Gatherer);
        public static Profession Tracker { get; } = new Profession(17, () => LevelUpMenu.getProfessionTitleFromNumber(17), 10, null, null, null, Gatherer);

        /**************
         * Foraging
         **************/
        public static Profession Miner { get; } = new Profession(18, () => LevelUpMenu.getProfessionTitleFromNumber(18), 5);
        public static Profession Geologist { get; } = new Profession(19, () => LevelUpMenu.getProfessionTitleFromNumber(19), 5);
        public static Profession Blacksmith { get; } = new Profession(20, () => LevelUpMenu.getProfessionTitleFromNumber(20), 10, null, null, null, Miner);
        public static Profession Burrower { get; } = new Profession(21, () => LevelUpMenu.getProfessionTitleFromNumber(21), 10, null, null, null, Miner);
        public static Profession Excavator { get; } = new Profession(22, () => LevelUpMenu.getProfessionTitleFromNumber(22), 10, null, null, null, Geologist);
        public static Profession Gemologist { get; } = new Profession(23, () => LevelUpMenu.getProfessionTitleFromNumber(23), 10, null, null, null, Geologist);

        /**************
         * Combat
         **************/
        public static Profession Fighter { get; } = new Profession(24, () => LevelUpMenu.getProfessionTitleFromNumber(24), 5);
        public static Profession Scout { get; } = new Profession(25, () => LevelUpMenu.getProfessionTitleFromNumber(25), 5);
        public static Profession Brute { get; } = new Profession(26, () => LevelUpMenu.getProfessionTitleFromNumber(26), 10, null, null, null, Fighter);
        public static Profession Defender { get; } = new Profession(27, () => LevelUpMenu.getProfessionTitleFromNumber(27), 10, null, null, null, Fighter);
        public static Profession Acrobat { get; } = new Profession(28, () => LevelUpMenu.getProfessionTitleFromNumber(28), 10, null, null, null, Scout);
        public static Profession Desperado { get; } = new Profession(29, () => LevelUpMenu.getProfessionTitleFromNumber(29), 10, null, null, null, Scout);
    }
}
