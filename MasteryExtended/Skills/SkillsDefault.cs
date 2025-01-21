using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MasteryExtended.Skills
{
    public partial class Skill
    {
        public static Skill Farming { get; } = new(null, 0, null, new Rectangle(22 * 16, 18 * 16, 16, 16),
            [Profession.Rancher, Profession.Tiller, Profession.Butcher, Profession.Shepherd, Profession.Artisan, Profession.Agriculturist]
        )
        {
            getLevel = () => Game1.player.GetUnmodifiedSkillLevel(0)
        };

        public static Skill Mining { get; } = new(null, 3, null, new Rectangle(2 * 16, 3 * 16, 16, 16),
            [Profession.Miner, Profession.Geologist, Profession.Blacksmith, Profession.Burrower, Profession.Excavator, Profession.Gemologist]
        )
        {
            getLevel = () => Game1.player.GetUnmodifiedSkillLevel(3)
        };
        public static Skill Foraging { get; } = new(null, 2, null, new Rectangle(13 * 16, 29 * 16, 16, 16),
            [Profession.Forester, Profession.Gatherer, Profession.Lumberjack, Profession.Tapper, Profession.Botanist, Profession.Tracker]
        )
        {
            getLevel = () => Game1.player.GetUnmodifiedSkillLevel(2)
        };
        public static Skill Fishing { get; } = new(null, 1, null, new Rectangle(19 * 16, 6 * 16, 16, 16),
            [Profession.Fisher, Profession.Trapper, Profession.Angler, Profession.Pirate, Profession.Baitmaster, Profession.Mariner]
        )
        {
            getLevel = () => Game1.player.GetUnmodifiedSkillLevel(1)
        };
        public static Skill Combat { get; } = new(null, 4, Game1.content.Load<Texture2D>("TileSheets\\weapons"), new Rectangle(6 * 16, 7 * 16, 16, 16),
            [Profession.Fighter, Profession.Scout, Profession.Brute, Profession.Defender, Profession.Acrobat, Profession.Desperado]
        )
        {
            getLevel = () => Game1.player.GetUnmodifiedSkillLevel(4)
        };
    }
}
