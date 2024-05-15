using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Patches
{
    internal static class SkillsPagePatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static void drawPrefix(SkillsPage __instance, out string __state)
        {
            __state = $"{(string)__instance.GetInstanceField("hoverTitle")!}-:-{(string)__instance.GetInstanceField("hoverText")!}";
            __instance.SetInstanceField("hoverTitle", "");
            __instance.SetInstanceField("hoverText", "");
        }

        internal static void drawPostfix(SkillsPage __instance, SpriteBatch b, string __state)
        {
            string[] parsedText = __state.Split("-:-");
            __instance.SetInstanceField("hoverTitle", parsedText[0]);
            __instance.SetInstanceField("hoverText", parsedText[1]);

            // Modificar lo que se muestra compare
            if (Game1.stats.Get("MasteryExp") == 0) return;

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
            float widthRatio = masterySpent >= 10? 0.725f: (masteryLevel >= 10? 0.7875f : 0.85f) ; //.725f cuando máximo, 0.7875f, .85f cuando mínimo
            float newWidth = widthRatio * width;

            // Parchar la parte de números
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
            NumberSprite.draw(masteryLevel, b, new Vector2(xOffset + __instance.xPositionOnScreen + 412 + (int)(584f * width), yOffset + __instance.yPositionOnScreen), (masteryLevel >= ModEntry.MaxMasteryPoints ? new(70, 210, 90) : (masterySpent == masteryLevel ? Color.OrangeRed : Color.SandyBrown)) * ((masteryLevel == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0);

            xOffset += masteryLevel < 10 ? 28: 0;

            // Luego el separador
            b.Draw(Game1.mouseCursors,
                new Vector2(xOffset + __instance.xPositionOnScreen + 352 + (int)(584f * width), yOffset + __instance.yPositionOnScreen + 4),
                new Rectangle(544, 136, 8, 8),
                Color.Black * 0.35f, 0f, new Vector2(4f, 4f), 4f * 1f, SpriteEffects.None, 0.85f);
            b.Draw(Game1.mouseCursors,
                new Vector2(xOffset + __instance.xPositionOnScreen + 356 + (int)(584f * width), yOffset + __instance.yPositionOnScreen),
                new Rectangle(544, 136, 8, 8),
                (masteryLevel >= ModEntry.MaxMasteryPoints ? new(70, 210, 90) : (masterySpent == masteryLevel ? Color.OrangeRed : Color.SandyBrown)) * 1f,
                0f, new Vector2(4f, 4f), 4f * 1f, SpriteEffects.None, 0.85f);

            // Luego el del lado izquierdo
            NumberSprite.draw(masterySpent, b, new Vector2(xOffset + __instance.xPositionOnScreen + 329 + (int)(584f * width), yOffset + __instance.yPositionOnScreen + 4), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
            NumberSprite.draw(masterySpent, b, new Vector2(xOffset + __instance.xPositionOnScreen + 333 + (int)(584f * width), yOffset + __instance.yPositionOnScreen),
                (masterySpent >= ModEntry.MaxMasteryPoints ? new(70, 210, 90) : (masterySpent == masteryLevel? Color.OrangeRed: Color.SandyBrown)) * ((masteryLevel == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0);

            if (((string)__instance.GetInstanceField("hoverText")!).Length > 0)
            {
                IClickableMenu.drawHoverText(b,
                    (string)__instance.GetInstanceField("hoverText")!,
                    Game1.smallFont, 0, 0, -1,
                    (((string)__instance.GetInstanceField("hoverTitle")!).Length > 0) ? (string)__instance.GetInstanceField("hoverTitle")! : null);
            }
        }
    }
}
