using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Skills.Professions
{
    public partial class Profession
    {
        /**************
         * Farming
         **************/
        public static Profession Rancher { get; } = new Profession(0, 5);
        public static Profession Tiller { get; } = new Profession(1, 5);
        public static Profession Butcher { get; } = new Profession(2, 10, Rancher);
        public static Profession Shepherd { get; } = new Profession(3, 10, Rancher);
        public static Profession Artisan { get; } = new Profession(4, 10, Tiller);
        public static Profession Agriculturist { get; } = new Profession(5, 10, Tiller);

        /**************
         * Fishing
         **************/
        public static Profession Fisher { get; } = new Profession(6, 5);
        public static Profession Trapper { get; } = new Profession(7, 5);
        public static Profession Angler { get; } = new Profession(8, 10, Fisher);
        public static Profession Pirate { get; } = new Profession(9, 10, Fisher);
        public static Profession Baitmaster { get; } = new Profession(10, 10, Trapper);
        public static Profession Mariner { get; } = new Profession(11, 10, Trapper);

        /**************
         * Foraging
         **************/
        public static Profession Forester { get; } = new Profession(12, 5);
        public static Profession Gatherer { get; } = new Profession(13, 5);
        public static Profession Lumberjack { get; } = new Profession(14, 10, Forester);
        public static Profession Tapper { get; } = new Profession(15, 10, Forester);
        public static Profession Botanist { get; } = new Profession(16, 10, Gatherer);
        public static Profession Tracker { get; } = new Profession(17, 10, Gatherer);

        /**************
         * Mining
         **************/
        public static Profession Miner { get; } = new Profession(18, 5);
        public static Profession Geologist { get; } = new Profession(19, 5);
        public static Profession Blacksmith { get; } = new Profession(20, 10, Miner);
        public static Profession Burrower { get; } = new Profession(21, 10, Miner);
        public static Profession Excavator { get; } = new Profession(22, 10, Geologist);
        public static Profession Gemologist { get; } = new Profession(23, 10, Geologist);

        /**************
         * Combat
         **************/
        public static Profession Fighter { get; } = new Profession(24, 5);
        public static Profession Scout { get; } = new Profession(25, 5);
        public static Profession Brute { get; } = new Profession(26, 10, Fighter);
        public static Profession Defender { get; } = new Profession(27, 10, Fighter);
        public static Profession Acrobat { get; } = new Profession(28, 10, Scout);
        public static Profession Desperado { get; } = new Profession(29, 10, Scout);
    }
}
