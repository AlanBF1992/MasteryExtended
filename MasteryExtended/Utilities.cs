using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended
{
    public static class Utilities
    {
        /// <summary>
        /// Replacement for IClickable.drawHoverText
        /// </summary>
        public static void newDrawHoverText(SpriteBatch b, string text, SpriteFont font, int xOffset = 0, int yOffset = 0, string? boldTitleText = null, int overrideX = -1, int overrideY = -1, float alpha = 1f, Texture2D? boxTexture = null, Rectangle? boxSourceRect = null, Color? boxColor = null, Color? boxShadowColor = null, Color? textColor = null, Color? textShadowColor = null, float boxScale = 1f, int boxWidthOverride = -1, int boxHeightOverride = -1, bool drawBoxShadow = true, bool drawTextShadow = true)
        {
            if (string.IsNullOrEmpty(text)) return;

            boxTexture ??= Game1.menuTexture;
            boxSourceRect ??= new Rectangle(0, 256, 60, 60);
            boxColor ??= Color.White;
            textColor ??= Game1.textColor;
            textShadowColor ??= Game1.textShadowColor;
            boxShadowColor ??= Color.Black;

            Vector2 bold_text_size = (boldTitleText != null)? Game1.dialogueFont.MeasureString(boldTitleText): new(0,0);

            int x = overrideX != -1 ? overrideX : Game1.getOldMouseX() + 32 + xOffset;
            int y = overrideY != -1 ? overrideY : Game1.getOldMouseY() + 32 + yOffset;

            int width = (int)Math.Max(bold_text_size.X, font.MeasureString(text).X + 32) + 4;
            int height = (int)((boldTitleText != null) ? (bold_text_size.Y + 16f) : 0f) + (int)font.MeasureString(text).Y + 32;

            int textWidth = boxWidthOverride != -1 ? boxWidthOverride : width;
            int textHeight = boxHeightOverride != -1 ? boxHeightOverride : height;

            // Push to borders if its outside
            if (x + textWidth > Utility.getSafeArea().Right)
            {
                x = Utility.getSafeArea().Right - textWidth;
                y += 16;
            }
            if (y + textHeight > Utility.getSafeArea().Bottom)
            {
                x += 16;
                if (x + textWidth > Utility.getSafeArea().Right)
                {
                    x = Utility.getSafeArea().Right - textWidth;
                }
                y = Utility.getSafeArea().Bottom - textHeight;
            }

            //Box Shadow
            if (drawBoxShadow)
            {
                IClickableMenu.drawTextureBox(b,
                    boxTexture,
                    boxSourceRect.Value,
                    x - 8, y + 8,
                    textWidth, textHeight, boxShadowColor.Value * 0.5f * alpha, boxScale, false);
            }
            //Box
            IClickableMenu.drawTextureBox(b,
                boxTexture,
                boxSourceRect.Value,
                x, y,
                textWidth, textHeight, boxColor.Value * alpha, boxScale, false);
            //Title
            if (boldTitleText != null)
            {
                // Title Box
                IClickableMenu.drawTextureBox(b,
                    boxTexture,
                    boxSourceRect.Value,
                    x, y + 4,
                    width,
                    (int)bold_text_size.Y + 32 - 4,
                    boxShadowColor.Value * 0.25f * alpha, boxScale, drawShadow: false);

                IClickableMenu.drawTextureBox(b,
                    boxTexture,
                    boxSourceRect.Value,
                    x, y,
                    width,
                    (int)bold_text_size.Y + 32 - 4,
                    boxColor.Value * alpha, boxScale, drawShadow: false);

                // Title Text Shadow
                if (drawTextShadow)
                {
                    b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 2f), textShadowColor.Value * alpha);
                    b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y + 16 + 4) + new Vector2(0f, 2f), textShadowColor.Value * alpha);
                }
                // Title Text Shadow
                b.DrawString(Game1.dialogueFont, boldTitleText, new Vector2(x + 16, y + 16 + 4), textColor.Value * 0.9f * alpha);

                y += (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 16;
            }

            //Text Shadow
            if (drawTextShadow)
            {
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 2f), textShadowColor.Value * alpha);
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(0f, 2f), textShadowColor.Value * alpha);
                b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 0f), textShadowColor.Value * alpha);
            }
            //Text
            b.DrawString(font, text, new Vector2(x + 16, y + 16 + 4), textColor.Value * 0.9f * alpha);
        }

        public static float EncloseNumber(float min, float number, float max)
        {
            if (number <= min) return min;
            if (number >= max) return max;
            return number;
        }
    }
}
