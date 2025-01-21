using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Menu
{
    public abstract class MasteryPage(int x, int y, int width, int height, bool showUpperRightCloseButton = false) : IClickableMenu(x, y, width, height, showUpperRightCloseButton)
    {
        public string MenuTitle { get; internal set; } = "";

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
    }
}
