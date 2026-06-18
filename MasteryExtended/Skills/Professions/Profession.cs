using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Skills.Professions
{
    public partial class Profession
    {
        public int Id { get; set; }

        public Func<string> GetName { get; set; } = null!;

        public Func<string> GetDescription { get; set; } = null!;

        public int LevelRequired { get; set; }

        public Profession? RequiredProfessions { get; set; }

        public Func<Texture2D> TextureSource { get; set; } = () => Game1.mouseCursors;

        public Rectangle TextureBounds { get; set; } = new Rectangle(0, 0, 16, 16);

        public Action GetImmediateProfessionPerk { get; set; } = () => { };

        public Action RemoveImmediateProfessionPerk { get; set; } = () => { };

        /*****************
        * Public methods *
        ******************/
        public Profession() { }

        // Vanilla Professions
        public Profession(int id, int levelRequired, Profession? requiredProfession = null)
        {
            Id = id;
            GetName = () => LevelUpMenu.getProfessionTitleFromNumber(id);
            LevelRequired = levelRequired;
            GetDescription = () =>
            {
                List<string> profDesc = LevelUpMenu.getProfessionDescription(id);
                profDesc.RemoveAt(0);
                return string.Join("\n", profDesc);
            };
            RequiredProfessions = requiredProfession;
            TextureBounds = new Rectangle(16 * (id % 6), 16 * (39 + id / 6), 16, 16);
        }

        // SpaceCore Professions
        public Profession(int id, Func<string> name, Func<string> description, int levelRequired, Profession? requiredProfession, Func<Texture2D>? textureSource, Rectangle? textureBounds = null, Action? getImmediateProfessionPerk = null, Action? removeImmediateProfessionPerk = null)
        {
            Id = id;
            GetName = name;
            GetDescription = description;
            LevelRequired = levelRequired;

            RequiredProfessions = requiredProfession;
            if (textureSource is null)
            {
                TextureBounds = new Rectangle(320, 496, 16, 16);
            }
            else
            {
                TextureSource = textureSource;
            }
            if (textureBounds is not null)
            {
                TextureBounds = textureBounds.Value;
            }


            if (getImmediateProfessionPerk is not null)
            {
                GetImmediateProfessionPerk = getImmediateProfessionPerk;
            }

            if (removeImmediateProfessionPerk is not null)
            {
                RemoveImmediateProfessionPerk = removeImmediateProfessionPerk;
            }
        }

        /***********
         * Methods *
         ***********/

        public void AddProfessionToPlayer()
        {
            Game1.player.professions.Add(Id);
            GetImmediateProfessionPerk();
        }

        public void RemoveProfessionFromPlayer()
        {
            Game1.player.professions.Remove(Id);
            RemoveImmediateProfessionPerk();
        }

        public bool IsProfessionUnlocked()
        {
            return Game1.player.professions.Contains(Id);
        }

        public bool IsRequiredUnlocked()
        {
            return RequiredProfessions?.IsProfessionUnlocked() != false;
        }
    }
}
