using MasteryExtended.Skills;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace MasteryExtended.Menu.Pages
{
    /// <summary>
    /// Main Menu that shows the Skills
    /// </summary>
    public class MasterySkillsPage : MasteryPage
    {
        public static List<Skill> skills { get; } = [Skill.Farming, Skill.Mining, Skill.Foraging, Skill.Fishing, Skill.Combat];

        public MasterySkillsPage(int page)
            : base((int)Utility.getTopLeftPositionForCenteringOnScreen(720, 320).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(720, 320).Y, 720, 320, showUpperRightCloseButton: true)
        {
            closeSound = "stone_button";
            MenuTitle = ModEntry.ModHelper.Translation.Get("menu-title-skills");

            List <Skill> shownSkills = skills.FindAll(s => s.showSkill());

            actualPage = page;
            totalPages = (int)Math.Ceiling(shownSkills.Count/5f);

            int skillsToShow = Math.Min(5, shownSkills.Count - (actualPage - 1) * 5);

            foreach (Skill s in shownSkills.GetRange((actualPage - 1) * 5, skillsToShow))
            {
                pageTextureComponents.Add(new ClickableTextureComponent(Rectangle.Empty, s.TextureSource, s.TextureBounds, 4f, drawShadow: true)
                {
                    name = s.GetName(),
                    hoverText = ModEntry.ModHelper.Translation.Get("hover-skill", new { skill = s.GetName() }),
                    myID = s.Id, // Skill of the button
                    region = 0, // For the highlight
                    myAlternateID = s.getLevel() >= 10 && s.unlockedProfessions() >= 2? 1: 0 //Visible o no
                });
            }

            // Define the bounds of the items (Empty for now) and the size of things, including the menu
            float yHeight = 80f;
            const int xPadding = 64;
            for (int i = 0; i < pageTextureComponents.Count; i++)
            {
                pageTextureComponents[i].bounds = new Rectangle(xPositionOnScreen + xPadding, yPositionOnScreen + 64 + (int)yHeight, width - 2 * xPadding, 64);
                pageTextureComponents[i].label = Game1.parseText(pageTextureComponents[i].label, Game1.smallFont, width - 200);
                yHeight += Game1.smallFont.MeasureString(pageTextureComponents[i].label).Y;
                if (i < pageTextureComponents.Count - 1)
                {
                    yHeight += pageTextureComponents[i].sourceRect.Height > 16 ? 132 : 80;
                }
            }

            height += (int)yHeight;
            height -= 48;

            if (actualPage < totalPages)
            {
                nextPageButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 + 8, yPositionOnScreen + height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
                {
                    visible = true,
                    myID = 998,
                    myAlternateID = actualPage + 1,
                    region = 0
                };
            }
            if (actualPage > 1)
            {
                previousPageButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 - 168 - 8, yPositionOnScreen + height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
                {
                    visible = true,
                    myID = 999,
                    myAlternateID = actualPage - 1,
                    region = 0
                };
            }

            // If there are no buttons
            if (nextPageButton == null && previousPageButton == null)
            {
                height -= 48;
            }

            // Add offset to items and menu
            int num = yPositionOnScreen;
            yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(800, height).Y;
            int offset = num - yPositionOnScreen;
            // If there is only a next

            foreach (ClickableTextureComponent c in pageTextureComponents)
            {
                c.bounds.Y -= offset;
            }
            upperRightCloseButton.bounds.Y -= offset;

            if (previousPageButton != null)
            {
                previousPageButton.bounds.Y -= offset;
            }
            if (nextPageButton != null)
            {
                nextPageButton.bounds.Y -= offset;
            }

            // Para controles o teclado
            if (Game1.options.SnappyMenus)
            {
                populateClickableComponentList();

                allClickableComponents.Reverse();
                ClickableComponent.ChainNeighborsUpDown(allClickableComponents);

                if (nextPageButton == null && previousPageButton == null)
                {
                    currentlySnappedComponent = getComponentWithID(4);
                }
                else if (nextPageButton != null)
                {
                    currentlySnappedComponent = getComponentWithID(998);
                }
                else
                {
                    currentlySnappedComponent = getComponentWithID(999);
                }

                if (nextPageButton != null && previousPageButton != null)
                {
                    nextPageButton.upNeighborID = previousPageButton.upNeighborID;
                    previousPageButton.rightNeighborID = nextPageButton.myID;
                    nextPageButton.leftNeighborID = previousPageButton.myID;
                    previousPageButton.downNeighborID = nextPageButton.downNeighborID;
                }

                snapCursorToCurrentSnappedComponent();
            }
        }

        public override void draw(SpriteBatch b)
        {
            backgroundDraw(b);

            // Menu title
            SpriteText.drawStringHorizontallyCenteredAt(b, MenuTitle, xPositionOnScreen + width / 2, yPositionOnScreen + 48, 9999, -1, 9999, 1f, 0.88f, junimoText: false, Color.Black);
            // TODO
            // Texto en botones que caiga o se hace fuente más pequeña
            //Buttons
            if (previousPageButton != null)
            {
                const float sScale = 1f;
                previousPageButton.draw(b, Color.White, 0.88f);
                string sBack = ModEntry.ModHelper.Translation.Get("back-button");
                Vector2 sSize = sScale*Game1.dialogueFont.MeasureString(sBack);
                Utility.drawTextWithColoredShadow(b, sBack, Game1.dialogueFont, previousPageButton.getVector2() + new Vector2((float)(previousPageButton.bounds.Width / 2) - sSize.X / 2f, 32f - sSize.Y / 2f + (float)((previousPageButton.sourceRect.X == 84) ? 8 : 0)), Color.Black * 1f, Color.Black * 0.2f, sScale * 1f, 0.9f);
            }
            if (nextPageButton != null)
            {
                const float sScale = 1f;
                nextPageButton.draw(b, Color.White, 0.88f);
                string sNext = ModEntry.ModHelper.Translation.Get("next-button");
                Vector2 sSize = sScale * Game1.dialogueFont.MeasureString(sNext);
                Utility.drawTextWithColoredShadow(b, sNext, Game1.dialogueFont, nextPageButton.getVector2() + new Vector2((float)(nextPageButton.bounds.Width / 2) - sSize.X / 2f, 32f - sSize.Y / 2f + (float)((nextPageButton.sourceRect.X == 84) ? 8 : 0)), Color.Black * 1f, Color.Black * 0.2f, sScale * 1f, 0.9f);
            }
            // The rest
            foreach (ClickableTextureComponent c in pageTextureComponents)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, c.region == 0 ? backItemColor : backItemColorHover, 3f, false);

                // Cubrirlo si no debiese ser visible
                if (c.myAlternateID == 0)
                {
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, Color.Black * (c.region == 0 ? 0.75f : 0.6f), 3f, false);
                }
                // Si está completo, hacerlo amarillo
                if (skills.Find(s => s.Id == c.myID)!.unlockedProfessions() >= 6)
                {
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, Color.Green * 0.3f, 3f, false);
                }

                // Dibuja el icono
                float iconScale = 16 / Math.Max(c.sourceRect.Height, c.sourceRect.Width);
                Utility.drawWithShadow(b, c.texture, c.getVector2() + new Vector2(12f, 6f), c.sourceRect, Color.White, 0f, Vector2.Zero, 3f * iconScale, shadowIntensity: 0.25f);
                // Dibuja el nombre
                Utility.drawTextWithColoredShadow(b, c.name, Game1.dialogueFont,
                    c.getVector2() + new Vector2(72f, c.bounds.Height/2) - new Vector2(0, (int)Math.Ceiling(Game1.dialogueFont.MeasureString(c.name).Y / 2) - 3),
                    Color.Black, Color.Black * 0.15f);
            }

            if(MasteryTrackerMenu.getCurrentMasteryLevel() <= (int)Game1.stats.Get("masteryLevelsSpent"))
            {
                Utilities.newDrawHoverText(b, ModEntry.ModHelper.Translation.Get("look-only"), Game1.smallFont, overrideX: 0, overrideY: 0,
                    boxTexture: Game1.mouseCursors_1_6, boxSourceRect: new Rectangle(1, 85, 21, 21), boxShadowColor: Color.Black,
                    textColor: Color.Black, textShadowColor: Color.Black * 0.2f, boxScale: 2f);
            }

            Utilities.newDrawHoverText(b, hoverText, Game1.smallFont,
                boxTexture: Game1.mouseCursors_1_6,
                boxSourceRect: new Rectangle(1, 85, 21, 21),
                boxShadowColor: Color.Black,
                textColor: Color.Black, textShadowColor: Color.Black * 0.2f, boxScale: 2f);

            base.draw(b);
            drawMouse(b); // Adds the mouse
        }

        public override void performHoverAction(int x, int y)
        {
            hoverText = "";

            foreach (ClickableTextureComponent c in pageTextureComponents)
            {
                c.region = 0;
                if (c.bounds.Contains(x, y))
                {
                    Game1.SetFreeCursorDrag();

                    // For the hover color
                    c.region = 1;

                    // Set hover text
                    if (c.myAlternateID == 0)
                    {
                        hoverText = ModEntry.ModHelper.Translation.Get("cant-access-skill", new { skill = c.name});
                    } else
                    {
                        // Set hover text
                        if (skills.Find(s => s.Id == c.myID)!.unlockedProfessions() >= 6)
                        {
                            hoverText = ModEntry.ModHelper.Translation.Get("every-profession-unlocked", new { skill = c.name });
                        } else
                        {
                            if (!string.IsNullOrEmpty(c.hoverText))
                            {
                                hoverText = c.hoverText;
                            }
                        }
                    }
                }
            }
            base.performHoverAction(x, y);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (ClickableTextureComponent c in pageTextureComponents)
            {
                if (c.bounds.Contains(x, y) && c.myAlternateID == 1)
                {
                    if (c.myID != -1)
                    {
                        Game1.playSound("cowboy_monsterhit");
                        Game1.activeClickableMenu = new MasteryProfessionsPage(skills.Find(s => s.Id == c.myID)!);
                        break;
                    }
                }
            }

            if (previousPageButton?.bounds.Contains(x, y) == true && previousPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                previousPageButton.region = 1;
                pressedButtonTimer = 100f;
            }

            if (nextPageButton?.bounds.Contains(x, y) == true && nextPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                nextPageButton.region = 1;
                pressedButtonTimer = 100f;
            }

            base.receiveLeftClick(x, y, playSound);
        }

        public override void update(GameTime time)
        {
            if (destroyTimer > 0f)
            {
                destroyTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;

                if (destroyTimer <= 0f)
                {
                    if (nextPageButton?.region == 1)
                    {
                        Game1.activeClickableMenu = new MasterySkillsPage(nextPageButton.myAlternateID);
                    }
                    else if (previousPageButton?.region == 1)
                    {
                        Game1.activeClickableMenu = new MasterySkillsPage(previousPageButton.myAlternateID);
                    }
                }
            }

            if (pressedButtonTimer > 0f)
            {
                pressedButtonTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;

                if (nextPageButton?.region == 1)
                {
                    nextPageButton.sourceRect.X = 84;
                }
                else if (previousPageButton?.region == 1)
                {
                    previousPageButton.sourceRect.X = 84;
                }
                if (pressedButtonTimer <= 0f)
                {
                    destroyTimer = 100f;
                }
            }
            base.update(time);
        }
    }
}
