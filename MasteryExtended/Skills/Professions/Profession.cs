using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Skills.Professions
{
    public partial class Profession
    {
        public int Id { get; set; }

        public Func<string> GetName = () => "";
        public int LevelRequired { get; set; } = 0;

        public Func<string> GetDescription = () => "";

        public Profession? RequiredProfessions { get; set;} = null;

        public Texture2D TextureSource { get; set; } = Game1.mouseCursors;

        public Rectangle TextureBounds { get; set; } = new Rectangle(0,0,16,16);

        /**************
        ** Public methods
         **************/
        public Profession() { }

        public Profession(int id, Func<string>? name, int levelRequired, Func<string>? description = null, Texture2D? textureSource = null, Rectangle? textureBounds = null, Profession? requiredProfession = null)
        {
            Id = id;
            if (name != null)
            {
                GetName = name;
            }
            else if (0 <= id && id <= 29)
            {
                GetName = () => LevelUpMenu.getProfessionTitleFromNumber(id);
            } else {
                GetName = () => "?";
            }

            LevelRequired = levelRequired;

            if (description != null) { GetDescription = description; }
            else if (0 <= id && id <= 29)
            {
                GetDescription = () =>
                {
                    List<string> profDesc = LevelUpMenu.getProfessionDescription(id);
                    profDesc.RemoveAt(0);
                    return String.Join("\n", profDesc);
                };
            }
            else {
                GetDescription = () => "";
            }

            RequiredProfessions = requiredProfession;
            if (textureSource != null) TextureSource = textureSource;
            if (textureBounds != null) {
                TextureBounds = (Rectangle)textureBounds;
            } else if (0 <= id && id <= 29)
            {
                TextureBounds = new Rectangle(16 * (id % 6), 16 * (39 + id / 6), 16, 16);
            }
        }

        /**************
         * Methods
         **************/

        public void AddProfessionToPlayer()
        {
            // Modify for custom skills later
            Game1.player.professions.Add(Id);
        }

        public void RemoveProfessionFromPlayer()
        {
            // Modify for custom skills later
            Game1.player.professions.Remove(Id);
        }

        public bool IsProfessionUnlocked()
        {
            // Modify for custom skills later
            return Game1.player.professions.Contains(Id);
        }
    }
}
