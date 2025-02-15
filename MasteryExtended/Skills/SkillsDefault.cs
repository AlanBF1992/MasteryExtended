using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MasteryExtended.Skills
{
    public partial class Skill
    {
        public static Skill Farming { get; } = new(0, null, new Rectangle(22 * 16, 18 * 16, 16, 16),
            [Profession.Rancher, Profession.Tiller, Profession.Butcher, Profession.Shepherd, Profession.Artisan, Profession.Agriculturist]
        );

        public static Skill Mining { get; } = new(3, null, new Rectangle(2 * 16, 3 * 16, 16, 16),
            [Profession.Miner, Profession.Geologist, Profession.Blacksmith, Profession.Burrower, Profession.Excavator, Profession.Gemologist]
        );
        public static Skill Foraging { get; } = new(2, null, new Rectangle(13 * 16, 29 * 16, 16, 16),
            [Profession.Forester, Profession.Gatherer, Profession.Lumberjack, Profession.Tapper, Profession.Botanist, Profession.Tracker]
        );
        public static Skill Fishing { get; } = new(1, null, new Rectangle(19 * 16, 6 * 16, 16, 16),
            [Profession.Fisher, Profession.Trapper, Profession.Angler, Profession.Pirate, Profession.Baitmaster, Profession.Mariner]
        );
        public static Skill Combat { get; } = new(4, Game1.content.Load<Texture2D>("TileSheets\\weapons"), new Rectangle(6 * 16, 7 * 16, 16, 16),
            [Profession.Fighter, Profession.Scout, Profession.Brute, Profession.Defender, Profession.Acrobat, Profession.Desperado]
        );
    }
}
