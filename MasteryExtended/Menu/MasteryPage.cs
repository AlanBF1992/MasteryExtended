using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Menu
{
    public abstract class MasteryPage(int x, int y, int width, int height, bool showUpperRightCloseButton = false) : IClickableMenu(x, y, width, height, showUpperRightCloseButton)
    {
        public string MenuTitle = "";

        public string hoverText = "";

        public int totalPages = 1;

        public int actualPage = 1;

        public float pressedButtonTimer;

        public float destroyTimer;

        public ClickableTextureComponent? nextPageButton;

        public ClickableTextureComponent? previousPageButton;

        public List<ClickableTextureComponent> pageTextureComponents = [];

        public Color backItemColor = new(132, 160, 255, 220); // Same Color as BG: alpha = 246

        public Color backItemColorHover = new(132, 160, 255, 150);

        public override void performHoverAction(int x, int y)
        {
            // Botón atrás
            if (previousPageButton?.containsPoint(x, y) == true)
            {
                previousPageButton.sourceRect.X = 42;
            }
            else if (previousPageButton != null)
            {
                previousPageButton.sourceRect.X = 0;
            }

            // Botón adelante
            if (nextPageButton?.containsPoint(x, y) == true)
            {
                nextPageButton.sourceRect.X = 42;
            }
            else if (nextPageButton != null)
            {
                nextPageButton.sourceRect.X = 0;
            }

            base.performHoverAction(x, y);
        }

        public void backgroundDraw(SpriteBatch b)
        {
            // Full Background, si se elige en opciones
            if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
            }
            // Fondo del menu
            drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(1, 85, 21, 21), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f);
            // Bordes bonitos
            b.Draw(Game1.mouseCursors_1_6, Position + new Vector2(6f, 7f) * 4f, new Rectangle(0, 144, 23, 23), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors_1_6, Position + new Vector2(24f, height - 24), new Rectangle(0, 144, 23, 23), Color.White, -(float)Math.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors_1_6, Position + new Vector2(width - 24, 28f), new Rectangle(0, 144, 23, 23), Color.White, -4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors_1_6, Position + new Vector2(width - 24, height - 24), new Rectangle(0, 144, 23, 23), Color.White, (float)Math.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
        }

        public static void drawHoverText(SpriteBatch b, string text, SpriteFont font, int xOffset = 0, int yOffset = 0, string? boldTitleText = null, int overrideX = -1, int overrideY = -1, float alpha = 1f, Texture2D? boxTexture = null, Rectangle? boxSourceRect = null, Color? boxColor = null, Color? boxShadowColor = null, Color? textColor = null, Color? textShadowColor = null, float boxScale = 1f, int boxWidthOverride = -1, int boxHeightOverride = -1, bool drawBoxShadow = true, bool drawTextShadow = true)
        {
            if (string.IsNullOrEmpty(text)) return;

            boxTexture ??= Game1.menuTexture;
            boxSourceRect ??= new Rectangle(0, 256, 60, 60);
            boxColor ??= Color.White;
            textColor ??= Game1.textColor;
            textShadowColor ??= Game1.textShadowColor;
            boxShadowColor ??= Color.Transparent;

            Vector2 bold_text_size = (boldTitleText != null) ? Game1.dialogueFont.MeasureString(boldTitleText) : new(0, 0);

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

        public override void receiveGamePadButton(Buttons button)
        {
            base.receiveGamePadButton(button);
            switch (button)
            {
                case Buttons.LeftTrigger:
                    if (previousPageButton?.visible == true)
                    {
                        Game1.playSound("cowboy_monsterhit");
                        previousPageButton.region = 1;
                        pressedButtonTimer = 100f;
                    }
                    break;
                case Buttons.RightTrigger:
                    if (nextPageButton?.visible == true)
                    {
                        Game1.playSound("cowboy_monsterhit");
                        nextPageButton.region = 1;
                        pressedButtonTimer = 100f;
                    }
                    break;
            }
        }
    }
}
