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

        public Func<int> getLevel { get; set; } = null!;
        public Func<bool> showSkill { get; set; } = null!;
        public Action<int> addNewLevel { get; set; } = null!;
        public List<int> ProfessionChooserLevels { get; set; } = null!;
        /*****************
        * Public methods *
        ******************/
        public Skill() { }

        // Vanilla Skills
        public Skill(int id, Texture2D? textureSource, Rectangle? textureBounds, List<Profession>? professions)
        {
            Id = id;
            GetName = id switch
            {
                0 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604"),
                3 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605"),
                2 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606"),
                1 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607"),
                4 => () => Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608"),
                _ => () => "?",
            };

            if (textureSource != null) TextureSource = textureSource;
            if (textureBounds != null) TextureBounds = textureBounds.Value;
            if (professions != null) Professions = professions;

            getLevel = () => Game1.player.GetUnmodifiedSkillLevel(id);
            addNewLevel = (lvl) => Game1.player.newLevels.Add(new Point(id, lvl));
            showSkill = () => true;
            ProfessionChooserLevels = [5, 10];
        }

        // SpaceCore Skills
        public Skill(Func<string> name, int id, Texture2D? textureSource, List<Profession>? professions, Func<int> getLvl, Action<int> addLvl, Func<bool> showSk, List<int>? professionChooserLevels)
        {
            Id = id;
            GetName = name;
            if (textureSource != null) TextureSource = textureSource;
            if (professions != null) Professions = professions;
            getLevel = getLvl;
            addNewLevel = addLvl;
            showSkill = showSk;
            if (professionChooserLevels != null) ProfessionChooserLevels = professionChooserLevels;
        }

        public bool containsProfession(int id)
        {
            return Professions.Any(p => p.Id == id);
        }

        public List<Profession> unlockedProfessions()
        {
            return Professions.FindAll(prof => prof.IsProfessionUnlocked());
        }

        public int unlockedProfessionsCount()
        {
            return Professions.Count(prof => prof.IsProfessionUnlocked());
        }

        public int unlockedProfessionsCount(int lvl)
        {
            return Professions.Count(prof => prof.IsProfessionUnlocked() && prof.LevelRequired == lvl);
        }

        public int unlockedProfessionsCount(int minLvl, int maxLvl)
        {
            return Professions.Count(prof => prof.IsProfessionUnlocked() && prof.LevelRequired >= minLvl && prof.LevelRequired <= maxLvl);
        }
    }
}
