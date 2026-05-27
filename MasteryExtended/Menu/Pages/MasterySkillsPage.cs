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
            MenuTitle = Game1.content.LoadString("Strings\\UI:MasteryExtended_MenuTitleSkills");

            List<Skill> shownSkills = skills.FindAll(s => s.isVisible());

            CurrentPage = page;
            TotalPages = (int)Math.Ceiling(shownSkills.Count / 5f);

            int skillsToShow = Math.Min(5, shownSkills.Count - (CurrentPage - 1) * 5);

            foreach (Skill s in shownSkills.GetRange((CurrentPage - 1) * 5, skillsToShow))
            {
                PageTextureComponents.Add(new ClickableTextureComponent(Rectangle.Empty, s.TextureSource(), s.TextureBounds, 4f, drawShadow: true)
                {
                    name = s.GetName(),
                    hoverText = Game1.content.LoadString("Strings\\UI:MasteryExtended_HoverSkill", s.GetName()),
                    myID = s.Id, // Skill of the button
                    region = 0, // For the highlight
                    myAlternateID = s.getLevel() >= 10 && s.unlockedProfessionsCount() >= 2 ? 1 : 0 //Visible o no
                });
            }

            // Define the bounds of the items (Empty for now) and the size of things, including the menu
            float yHeight = 80f;
            const int xPadding = 64;
            for (int i = 0; i < PageTextureComponents.Count; i++)
            {
                PageTextureComponents[i].bounds = new Rectangle(xPositionOnScreen + xPadding, yPositionOnScreen + 64 + (int)yHeight, width - 2 * xPadding, 64);
                PageTextureComponents[i].label = Game1.parseText(PageTextureComponents[i].label, Game1.smallFont, width - 200);
                yHeight += Game1.smallFont.MeasureString(PageTextureComponents[i].label).Y;
                if (i < PageTextureComponents.Count - 1)
                {
                    yHeight += PageTextureComponents[i].sourceRect.Height > 16 ? 132 : 80;
                }
            }

            height += (int)yHeight;
            height -= 48;

            if (CurrentPage < TotalPages)
            {
                NextPageButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 + 8, yPositionOnScreen + height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
                {
                    visible = true,
                    myID = 998,
                    myAlternateID = CurrentPage + 1,
                    region = 0
                };
            }
            if (CurrentPage > 1)
            {
                PreviousPageButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 - 168 - 8, yPositionOnScreen + height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
                {
                    visible = true,
                    myID = 999,
                    myAlternateID = CurrentPage - 1,
                    region = 0
                };
            }

            // If there are no buttons
            if (NextPageButton == null && PreviousPageButton == null)
            {
                height -= 48;
            }

            // Add offset to items and menu
            int num = yPositionOnScreen;
            yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(800, height).Y;
            int offset = num - yPositionOnScreen;
            // If there is only a next

            foreach (ClickableTextureComponent c in PageTextureComponents)
            {
                c.bounds.Y -= offset;
            }
            upperRightCloseButton.bounds.Y -= offset;

            PreviousPageButton?.bounds.Y -= offset;
            NextPageButton?.bounds.Y -= offset;

            // Gamepad
            snapComponents();
        }

        internal void snapComponents()
        {
            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();

                ClickableComponent.ChainNeighborsUpDown(allClickableComponents);

                currentlySnappedComponent = PageTextureComponents[0];

                PageTextureComponents[0].upNeighborID = upperRightCloseButton.myID;
                upperRightCloseButton.downNeighborID = PageTextureComponents[0].myID;

                if (PreviousPageButton != null)
                {
                    PreviousPageButton.upNeighborID = PageTextureComponents[^1].myID;
                    PageTextureComponents[^1].downNeighborID = PreviousPageButton.myID;
                }
                if (NextPageButton != null)
                {
                    NextPageButton.upNeighborID = PageTextureComponents[^1].myID;
                    PageTextureComponents[^1].downNeighborID = NextPageButton.myID;
                }
                if (NextPageButton != null && PreviousPageButton != null)
                {
                    NextPageButton.leftNeighborID = PreviousPageButton.myID;
                    PreviousPageButton.rightNeighborID = NextPageButton.myID;
                }

                base.snapCursorToCurrentSnappedComponent();
            }
        }

        public override void draw(SpriteBatch b)
        {
            backgroundDraw(b);

            // Menu title
            SpriteText.drawStringHorizontallyCenteredAt(b, MenuTitle, xPositionOnScreen + width / 2, yPositionOnScreen + 48, 9999, -1, 9999, 1f, 0.88f, junimoText: false, Color.Black);

            // Buttons
            if (PreviousPageButton != null)
            {
                const float sScale = 1f;
                PreviousPageButton.draw(b, Color.White, 0.88f);
                string sBack = Game1.content.LoadString("Strings\\UI:MasteryExtended_BackButton");
                Vector2 sSize = sScale * Game1.dialogueFont.MeasureString(sBack);
                Utility.drawTextWithColoredShadow(b, sBack, Game1.dialogueFont, PreviousPageButton.getVector2() + new Vector2((float)(PreviousPageButton.bounds.Width / 2) - sSize.X / 2f, 32f - sSize.Y / 2f + (float)((PreviousPageButton.sourceRect.X == 84) ? 8 : 0)), Color.Black * 1f, Color.Black * 0.2f, sScale * 1f, 0.9f);
            }
            if (NextPageButton != null)
            {
                const float sScale = 1f;
                NextPageButton.draw(b, Color.White, 0.88f);
                string sNext = Game1.content.LoadString("Strings\\UI:MasteryExtended_NextButton");
                Vector2 sSize = sScale * Game1.dialogueFont.MeasureString(sNext);
                Utility.drawTextWithColoredShadow(b, sNext, Game1.dialogueFont, NextPageButton.getVector2() + new Vector2((float)(NextPageButton.bounds.Width / 2) - sSize.X / 2f, 32f - sSize.Y / 2f + (float)((NextPageButton.sourceRect.X == 84) ? 8 : 0)), Color.Black * 1f, Color.Black * 0.2f, sScale * 1f, 0.9f);
            }

            foreach (ClickableTextureComponent c in PageTextureComponents)
            {
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, c.region == 0 ? BackItemColor : BackItemColorHover, 3f, false);

                // Cover it if it shouldn't be visible
                if (c.myAlternateID == 0)
                {
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, Color.Black * (c.region == 0 ? 0.75f : 0.6f), 3f, false);
                }
                // If completed, make it yellow
                if (skills.Find(s => s.Id == c.myID)!.unlockedProfessionsCount() >= skills.Find(s => s.Id == c.myID)!.Professions.Count)
                {
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, Color.Green * 0.3f, 3f, false);
                }

                // Draw the icon
                int iconScale = 16 / Math.Max(c.sourceRect.Height, c.sourceRect.Width);
                Utility.drawWithShadow(b, c.texture, c.getVector2() + new Vector2(12f, 6f), c.sourceRect, Color.White, 0f, Vector2.Zero, 3f * iconScale, shadowIntensity: 0.25f);
                // Draw the name
                Utility.drawTextWithColoredShadow(b, c.name, Game1.dialogueFont,
                    c.getVector2() + new Vector2(72f, c.bounds.Height / 2) - new Vector2(0, (int)Math.Ceiling(Game1.dialogueFont.MeasureString(c.name).Y / 2) - 3),
                    Color.Black, Color.Black * 0.15f);
            }

            if (MasteryTrackerMenu.getCurrentMasteryLevel() <= (int)Game1.stats.Get("masteryLevelsSpent"))
            {
                drawHoverText(b, Game1.content.LoadString("Strings\\UI:MasteryExtended_LookOnly"), Game1.smallFont, overrideX: 0, overrideY: 0,
                    boxTexture: Game1.mouseCursors_1_6, boxSourceRect: new Rectangle(1, 85, 21, 21), boxShadowColor: Color.Black,
                    textColor: Color.Black, textShadowColor: Color.Black * 0.2f, boxScale: 2f);
            }

            drawHoverText(b, HoverText, Game1.smallFont,
                boxTexture: Game1.mouseCursors_1_6,
                boxSourceRect: new Rectangle(1, 85, 21, 21),
                boxShadowColor: Color.Black,
                textColor: Color.Black, textShadowColor: Color.Black * 0.2f, boxScale: 2f);

            base.draw(b);
            drawMouse(b); // Adds the mouse
        }

        public override void performHoverAction(int x, int y)
        {
            HoverText = "";

            foreach (ClickableTextureComponent c in PageTextureComponents)
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
                        HoverText = Game1.content.LoadString("Strings\\UI:MasteryExtended_CantAccessSkill", c.name);
                    }
                    else
                    {
                        // Set hover text
                        if (skills.Find(s => s.Id == c.myID)!.unlockedProfessionsCount() >= skills.Find(s => s.Id == c.myID)!.Professions.Count)
                        {
                            HoverText = Game1.content.LoadString("Strings\\UI:MasteryExtended_EveryProfessionUnlocked", c.name);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(c.hoverText))
                            {
                                HoverText = c.hoverText;
                            }
                        }
                    }
                }
            }
            base.performHoverAction(x, y);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (ClickableTextureComponent c in PageTextureComponents)
            {
                if (c.bounds.Contains(x, y) && c.myAlternateID == 1 && c.myID != -1)
                {
                    Game1.playSound("cowboy_monsterhit");
                    Game1.activeClickableMenu = new MasteryProfessionsPage(skills.Find(s => s.Id == c.myID)!);
                    break;
                }
            }

            if (PreviousPageButton?.bounds.Contains(x, y) == true && PreviousPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                PreviousPageButton.region = 1;
                PressedButtonTimer = 100f;
            }

            if (NextPageButton?.bounds.Contains(x, y) == true && NextPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                NextPageButton.region = 1;
                PressedButtonTimer = 100f;
            }

            base.receiveLeftClick(x, y, playSound);
        }

        public override void update(GameTime time)
        {
            if (DestroyTimer > 0f)
            {
                DestroyTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;

                if (DestroyTimer <= 0f)
                {
                    if (NextPageButton?.region == 1)
                    {
                        Game1.activeClickableMenu = new MasterySkillsPage(NextPageButton.myAlternateID);
                    }
                    else if (PreviousPageButton?.region == 1)
                    {
                        Game1.activeClickableMenu = new MasterySkillsPage(PreviousPageButton.myAlternateID);
                    }
                }
            }

            if (PressedButtonTimer > 0f)
            {
                PressedButtonTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;

                if (NextPageButton?.region == 1)
                {
                    NextPageButton.sourceRect.X = 84;
                }
                else if (PreviousPageButton?.region == 1)
                {
                    PreviousPageButton.sourceRect.X = 84;
                }
                if (PressedButtonTimer <= 0f)
                {
                    DestroyTimer = 100f;
                }
            }
            base.update(time);
        }
    }
}
