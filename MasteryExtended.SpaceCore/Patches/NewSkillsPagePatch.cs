using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using SpaceCore.Interface;

namespace MasteryExtended.SC.Patches
{
    internal static class NewSkillsPagePatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static void drawPrefix(NewSkillsPage __instance, out string __state)
        {
            const string newStart = NewSkillsPage.CustomSkillPrefix == "C" ? "Z" : "C";

            __state = $"{(string)__instance.GetInstanceField("hoverTitle")!}-:-{(string)__instance.GetInstanceField("hoverText")!}-:-{newStart}";

            __instance.SetInstanceField("hoverTitle", "");
            __instance.SetInstanceField("hoverText", "");

            foreach (var skillbar in __instance.skillBars)
            {
                if (skillbar.name.StartsWith(NewSkillsPage.CustomSkillPrefix))
                {
                    skillbar.name = String.Concat(newStart, skillbar.name);
                }
            }
        }

        internal static void drawPostfix(NewSkillsPage __instance, SpriteBatch b, string __state)
        {
            if (Game1.stats.Get("MasteryExp") > 0)
            {
                int masteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
                float masteryStringWidth = Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\1_6_Strings:Mastery").TrimEnd(':')).X;
                int masterySpent = (int)Game1.stats.Get("masteryLevelsSpent");

                int xOffset = (int)masteryStringWidth - 64;
                const int yOffset = 508;

                float width = 0.64f;
                width -= (masteryStringWidth - 100f) / 800f;
                if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru)
                {
                    width += 0.1f;
                }
                float widthRatio = masterySpent >= 10 ? 0.725f : (masteryLevel >= 10 ? 0.7875f : 0.85f); //.725f cuando máximo, 0.7875f, .85f cuando mínimo
                float newWidth = widthRatio * width;

                // Parchear la parte de números
                b.Draw(Game1.menuTexture, new Rectangle(
                    __instance.xPositionOnScreen + xOffset + 388 + (int)(584f * newWidth), //x
                    __instance.yPositionOnScreen + yOffset - 20, //y
                    __instance.width - xOffset - 428 - (int)(584f * newWidth),
                    44),
                    new Rectangle(120, 172, 8, 8),
                    Color.White);

                b.Draw(Game1.menuTexture, new Rectangle(
                    __instance.xPositionOnScreen + __instance.width - 40,
                    yOffset + __instance.yPositionOnScreen - 20,
                    8, 44),
                    new Rectangle(216, 172, 8, 8),
                    Color.White);

                // Dibujar los números (1ro sombra, 2do texto)
                // Primero el del lado derecho
                NumberSprite.draw(masteryLevel, b, new Vector2(xOffset + __instance.xPositionOnScreen + 408 + (int)(584f * width), yOffset + __instance.yPositionOnScreen + 4), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
                NumberSprite.draw(masteryLevel, b, new Vector2(xOffset + __instance.xPositionOnScreen + 412 + (int)(584f * width), yOffset + __instance.yPositionOnScreen), (masteryLevel >= MasteryExtended.ModEntry.MaxMasteryPoints ? new(70, 210, 90) : (masterySpent == masteryLevel ? Color.OrangeRed : Color.SandyBrown)) * ((masteryLevel == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0);

                xOffset += masteryLevel < 10 ? 28 : 0;

                // Luego el separador
                b.Draw(Game1.mouseCursors,
                    new Vector2(xOffset + __instance.xPositionOnScreen + 352 + (int)(584f * width), yOffset + __instance.yPositionOnScreen + 4),
                    new Rectangle(544, 136, 8, 8),
                    Color.Black * 0.35f, 0f, new Vector2(4f, 4f), 4f * 1f, SpriteEffects.None, 0.85f);
                b.Draw(Game1.mouseCursors,
                    new Vector2(xOffset + __instance.xPositionOnScreen + 356 + (int)(584f * width), yOffset + __instance.yPositionOnScreen),
                    new Rectangle(544, 136, 8, 8),
                    (masteryLevel >= MasteryExtended.ModEntry.MaxMasteryPoints ? new(70, 210, 90) : (masterySpent == masteryLevel ? Color.OrangeRed : Color.SandyBrown)) * 1f,
                    0f, new Vector2(4f, 4f), 4f * 1f, SpriteEffects.None, 0.85f);

                // Luego el del lado izquierdo
                NumberSprite.draw(masterySpent, b, new Vector2(xOffset + __instance.xPositionOnScreen + 329 + (int)(584f * width), yOffset + __instance.yPositionOnScreen + 4), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
                NumberSprite.draw(masterySpent, b, new Vector2(xOffset + __instance.xPositionOnScreen + 333 + (int)(584f * width), yOffset + __instance.yPositionOnScreen),
                    (masterySpent >= MasteryExtended.ModEntry.MaxMasteryPoints ? new(70, 210, 90) : (masterySpent == masteryLevel ? Color.OrangeRed : Color.SandyBrown)) * ((masteryLevel == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0);
            }

            /***************
             * HOVER THINGS
             ***************/

            // FIX THE TITLE AND TEXT
            string[] parsedText = __state.Split("-:-");
            __instance.SetInstanceField("hoverTitle", parsedText[0]);
            __instance.SetInstanceField("hoverText", parsedText[1]);
            string newStart = parsedText[2];
            foreach (var skillbar in __instance.skillBars)
            {
                if (!skillbar.name.StartsWith(newStart)) continue;
                skillbar.name = skillbar.name[1..];
            }

            // FIX THE IMAGE
            // Add Scroll Offset
            foreach (ClickableTextureComponent skillBar in __instance.skillBars)
                skillBar.bounds = new Rectangle(skillBar.bounds.Left, skillBar.bounds.Top - (int)__instance.GetInstanceField("skillScrollOffset")! * 56, skillBar.bounds.Width, skillBar.bounds.Height);

            // Configure the Image, Title and Text
            foreach (ClickableTextureComponent skillBar in __instance.skillBars)
            {
                if (!skillBar.name.StartsWith(NewSkillsPage.CustomSkillPrefix))
                    continue;
                if (skillBar.scale == 0.0)
                {
                    IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), skillBar.bounds.X - 16 - 8, skillBar.bounds.Y - 16 - 16, 96, 96, Color.White, 1f, false);

                    if (skillBar.name.StartsWith(NewSkillsPage.CustomSkillPrefix))
                    {
                        skillBar.scale = Game1.pixelZoom;
                        if (skillBar.containsPoint(Game1.getMouseX(), Game1.getMouseY()) && !skillBar.name.Equals("-1") && skillBar.hoverText.Length > 0)
                        {
                            var SkillsByNameField = ModEntry.ModHelper.Reflection.GetField<Dictionary<string, SpaceCore.Skills.Skill>>(typeof(SpaceCore.Skills), "SkillsByName");
                            var professions = SkillsByNameField.GetValue().SelectMany(s => s.Value.Professions).ToList();
                            var profession = professions.Find(p => NewSkillsPage.CustomSkillPrefix + p.Id == skillBar.name)!;

                            __instance.SetInstanceField("hoverTitle", profession.GetName());
                            __instance.SetInstanceField("hoverText", profession.GetDescription());

                            Texture2D actuallyAProfessionImage = profession.Icon ?? Game1.staminaRect;
                            skillBar.scale = 0.0f;
                            b.Draw(texture: actuallyAProfessionImage,
                                position: new Vector2(skillBar.bounds.X - (Game1.pixelZoom * 2), skillBar.bounds.Y - (Game1.tileSize / 2) + (Game1.tileSize / 4)),
                                sourceRectangle: new Rectangle(0, 0, 16, 16),
                                Color.White, rotation: 0.0f, origin: Vector2.Zero, scale: 4f, SpriteEffects.None, layerDepth: 1f);
                        }
                    }
                }
            }
            // Delete Scroll Offset
            foreach (ClickableTextureComponent skillBar in __instance.skillBars)
                skillBar.bounds = new Rectangle(skillBar.bounds.Left, skillBar.bounds.Top + (int)__instance.GetInstanceField("skillScrollOffset")! * 56, skillBar.bounds.Width, skillBar.bounds.Height);

            Utilities.newDrawHoverText(b,
                (string)__instance.GetInstanceField("hoverText")!,
                Game1.smallFont, 0, 0,
                (string)__instance.GetInstanceField("hoverTitle")!, boxShadowColor: Color.Black * 0.66f);
        }
    }
}
