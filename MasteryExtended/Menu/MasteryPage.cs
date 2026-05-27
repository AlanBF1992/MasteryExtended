using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Menu
{
    public abstract class MasteryPage(int x, int y, int width, int height, bool showUpperRightCloseButton = false) : IClickableMenu(x, y, width, height, showUpperRightCloseButton)
    {
        public string MenuTitle { get; set; } = "";
        public int TotalPages { get; set; } = 1;
        public int CurrentPage { get; set; } = 1;
        public string HoverText { get; set; } = "";
        protected internal float PressedButtonTimer { get; set; }
        protected internal float DestroyTimer { get; set; }
        public ClickableTextureComponent? NextPageButton { get; set; }
        public ClickableTextureComponent? PreviousPageButton { get; set; }
        public List<ClickableTextureComponent> PageTextureComponents { get; set; } = [];
        public Color BackItemColor { get; set; } = new(132, 160, 255, 220);
        public Color BackItemColorHover { get; set; } = new(132, 160, 255, 150);

        public override void performHoverAction(int x, int y)
        {
            // Back button
            if (PreviousPageButton?.containsPoint(x, y) == true)
            {
                PreviousPageButton.sourceRect.X = 42;
            }
            else
            {
                PreviousPageButton?.sourceRect.X = 0;
            }

            // Next button
            if (NextPageButton?.containsPoint(x, y) == true)
            {
                NextPageButton.sourceRect.X = 42;
            }
            else
            {
                NextPageButton?.sourceRect.X = 0;
            }

            base.performHoverAction(x, y);
        }

        public void backgroundDraw(SpriteBatch b)
        {
            // Full background if chosen in options
            if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
            }
            // Menu background
            drawTextureBox(b, Game1.mouseCursors_1_6, new Rectangle(1, 85, 21, 21), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f);
            // Nice borders
            b.Draw(Game1.mouseCursors_1_6, Position + new Vector2(24f, 28f), new Rectangle(0, 144, 23, 23), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors_1_6, Position + new Vector2(24f, height - 24), new Rectangle(0, 144, 23, 23), Color.White, -MathF.PI / 2f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors_1_6, Position + new Vector2(width - 24, 28f), new Rectangle(0, 144, 23, 23), Color.White, -MathF.PI * 1.5f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Game1.mouseCursors_1_6, Position + new Vector2(width - 24, height - 24), new Rectangle(0, 144, 23, 23), Color.White, MathF.PI, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
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

            Vector2 bold_text_size = (boldTitleText != null) ? Game1.dialogueFont.MeasureString(boldTitleText) : Vector2.Zero;

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

            Vector2 textPos = new(x + 16, y + 16 + 4);

            // Box shadow
            if (drawBoxShadow)
            {
                drawTextureBox(b,
                    boxTexture,
                    boxSourceRect.Value,
                    x - 8, y + 8,
                    textWidth, textHeight, boxShadowColor.Value * 0.5f * alpha, boxScale, false);
            }
            // Box
            drawTextureBox(b,
                boxTexture,
                boxSourceRect.Value,
                x, y,
                textWidth, textHeight, boxColor.Value * alpha, boxScale, false);
            // Title
            if (boldTitleText != null)
            {
                // Title Box
                drawTextureBox(b,
                    boxTexture,
                    boxSourceRect.Value,
                    x, y + 4,
                    width,
                    (int)bold_text_size.Y + 32 - 4,
                    boxShadowColor.Value * 0.25f * alpha, boxScale, drawShadow: false);

                drawTextureBox(b,
                    boxTexture,
                    boxSourceRect.Value,
                    x, y,
                    width,
                    (int)bold_text_size.Y + 32 - 4,
                    boxColor.Value * alpha, boxScale, drawShadow: false);

                // Title Text Shadow
                if (drawTextShadow)
                {
                    b.DrawString(Game1.dialogueFont, boldTitleText, textPos + new Vector2(2f, 2f), textShadowColor.Value * alpha);
                    b.DrawString(Game1.dialogueFont, boldTitleText, textPos + new Vector2(0f, 2f), textShadowColor.Value * alpha);
                }
                // Title Text Shadow
                b.DrawString(Game1.dialogueFont, boldTitleText, textPos, textColor.Value * 0.9f * alpha);

                textPos.Y += (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + 16;
            }

            // Text shadow
            if (drawTextShadow)
            {
                b.DrawString(font, text, textPos + new Vector2(2f, 2f), textShadowColor.Value * alpha);
                b.DrawString(font, text, textPos + new Vector2(0f, 2f), textShadowColor.Value * alpha);
                b.DrawString(font, text, textPos + new Vector2(2f, 0f), textShadowColor.Value * alpha);
            }
            // Text
            b.DrawString(font, text, textPos, textColor.Value * 0.9f * alpha);
        }

        public override void receiveGamePadButton(Buttons button)
        {
            base.receiveGamePadButton(button);
            switch (button)
            {
                case Buttons.LeftTrigger:
                    if (PreviousPageButton?.visible == true)
                    {
                        Game1.playSound("cowboy_monsterhit");
                        PreviousPageButton.region = 1;
                        PressedButtonTimer = 100f;
                    }
                    break;
                case Buttons.RightTrigger:
                    if (NextPageButton?.visible == true)
                    {
                        Game1.playSound("cowboy_monsterhit");
                        NextPageButton.region = 1;
                        PressedButtonTimer = 100f;
                    }
                    break;
            }
        }
    }
}
