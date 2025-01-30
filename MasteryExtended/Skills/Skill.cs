using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MasteryExtended.Skills
{
    public partial class Skill
    {
        public Func<string> GetName = () => "";
        public int Id { get; set; }
        public List<Profession> Professions { get; set; } = [];
        public Texture2D TextureSource { get; set; } = Game1.content.Load<Texture2D>("Maps\\springobjects");
        public Rectangle TextureBounds { get; set; } = new Rectangle(0, 0, 16, 16);

        public Func<int> getLevel { get; set; } = () => 0;
        public Func<bool> showSkill { get; set; } = () => true;

        /**************
        ** Public methods
         **************/
        public Skill() { }

        public Skill(Func<string>? name, int id, Texture2D? textureSource = null, Rectangle? textureBounds = null, List<Profession>? professions = null)
        {
            Id = id;
            GetName = name ?? id switch
            {
                0 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604"),
                1 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607"),
                2 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606"),
                3 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605"),
                4 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608"),
                _ => () => "?",
            };
            if (textureSource != null) { TextureSource = textureSource; }
            if (textureBounds != null) { TextureBounds = (Rectangle)textureBounds; }
            if (professions != null) Professions = professions;
        }

        public int unlockedProfessions()
        {
            return Professions.Count(prof => prof.IsProfessionUnlocked());
        }

        public int unlockedProfessions(int lvl)
        {
            return Professions.Count(prof => prof.IsProfessionUnlocked() && prof.LevelRequired == lvl);
        }
    }
}
