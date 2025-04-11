using MasteryExtended.Menu;
using MasteryExtended.Skills;
using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace MasteryExtended.Compatibility.VPP.Menu.Pages
{
    internal class MasteryProfessions20Page : MasteryPage
    {
        public Skill innerSkill;
        public List<Profession> Professions = [];

        public MasteryProfessions20Page(Skill innerSkill)
            : base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 160).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 160).Y, 1280, 160, showUpperRightCloseButton: true)
        {
            closeSound = "stone_button";

            this.innerSkill = innerSkill;

            Professions.AddRange(innerSkill.Professions.Where(p => p.LevelRequired == 20));

            // Título
            MenuTitle = Game1.content.LoadString("Strings\\UI:MasteryExtended_MenuTitleProfession", innerSkill.GetName());

            // Calcular tamaños y posiciones de los componentes
            const int xPadding = 64;
            const int xSpaceBetweenTrees = 32;

            const int yTopPadding = 80 + 64;

            int professionWidth = (width - 2 * xPadding - xSpaceBetweenTrees) / 2;
            const int professionHeight = 312;

            int xPosition = xPositionOnScreen + xPadding;
            int yPosition = yPositionOnScreen + yTopPadding;

            int xSpacingRightHalf = -xPadding + width / 2 + xSpaceBetweenTrees / 2;

            bool innerSkillCheck = innerSkill.unlockedProfessionsCount(15) >= ModEntry.Config.Lvl15ProfessionsRequired && innerSkill.unlockedProfessionsCount(20) >= 1;
            static bool professionCheck(Profession prof) { return !prof.IsProfessionUnlocked() && prof.RequiredProfessions?.IsProfessionUnlocked() != false; }

            Profession pL = Professions[0];
            Profession pR = Professions[1];

            pageTextureComponents.Add(new ClickableTextureComponent(new Rectangle(xPosition, yPosition, professionWidth, professionHeight), pL.TextureSource(), pL.TextureBounds, 1f, drawShadow: true)
            {
                name = pL.GetName(),
                hoverText = pL.GetDescription(),
                myID = pL.Id,
                myAlternateID = professionCheck(pL) && innerSkillCheck ? 0 : 1
            });

            pageTextureComponents.Add(new ClickableTextureComponent(new Rectangle(xPosition + xSpacingRightHalf, yPosition, professionWidth, professionHeight), pR.TextureSource(), pR.TextureBounds, 1f, drawShadow: true)
            {
                name = pR.GetName(),
                hoverText = pR.GetDescription(),
                myID = pR.Id,
                myAlternateID = professionCheck(pR) && innerSkillCheck ? 0 : 1
            });

            height += professionHeight + yTopPadding - 64;

            // Hacer espacio para los botones
            height += 48;

            // Offset so the menu is properly centered
            int num = yPositionOnScreen;
            yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(800, height).Y;
            int offset = num - yPositionOnScreen;
            foreach (ClickableTextureComponent c in pageTextureComponents)
            {
                c.bounds.Y -= offset;
            }
            upperRightCloseButton.bounds.Y -= offset;

            // Agregar botón atrás para ir al menú de profesiones 1
            previousPageButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 - 84, yPositionOnScreen + height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
            {
                visible = true,
                myID = 999
            };

            // TODO
            // Agregar luego los botones de gamepad
        }

        // Agregar cada profesión de forma bonita.
        public override void draw(SpriteBatch b)
        {
            backgroundDraw(b);
            // Título, adaptar al tipo
            SpriteText.drawStringHorizontallyCenteredAt(b, MenuTitle, xPositionOnScreen + width / 2, yPositionOnScreen + 48, 9999, -1, 9999, 1f, 0.88f, junimoText: false, Color.Black);

            // AHORA SI LOS BOTONES Y WEÁ
            foreach (ClickableTextureComponent c in pageTextureComponents)
            {
                // TEXTURA DE BOTÓN
                drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height, c.region == 0 ? backItemColor : backItemColorHover, 3f, false);

                // Agregar una capa de color a los adquiridos o no adquiribles
                if (c.myAlternateID >= 1)
                {
                    Color coverColor = innerSkill.Professions.Find(p => p.Id == c.myID)!.IsProfessionUnlocked() ? Color.Green * 0.3f : Color.Black * (c.region == 0 ? 0.75f : 0.6f);
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height,
                        coverColor,
                        3f, false);
                }

                // Dibuja el icono
                const float iconScale = 7f;
                Utility.drawWithShadow(b, c.texture,
                    c.getVector2() + new Vector2(c.bounds.Width / 2, 48) - new Vector2(16 * iconScale / 2, 0),
                    c.sourceRect, Color.White, 0f, Vector2.Zero, iconScale, shadowIntensity: 0.25f);

                // Dibuja el nombre
                var nameSize = Game1.dialogueFont.MeasureString(c.name);

                if (nameSize.X < c.bounds.Width)
                {
                    Utility.drawTextWithColoredShadow(b, c.name, Game1.dialogueFont,
                    c.getVector2() - new Vector2(nameSize.X / 2, nameSize.Y + 48) + new Vector2(c.bounds.Width / 2, c.bounds.Height),
                    Color.Black, Color.Black * 0.15f);
                }
                else
                {
                    string[] parsedName = Game1.parseText(c.name, Game1.smallFont, c.bounds.Width - 16).Split(Environment.NewLine);
                    for (int i = 0; i < parsedName.Length; i++)
                    {
                        nameSize = Game1.smallFont.MeasureString(parsedName[i].Trim());
                        Utility.drawTextWithColoredShadow(b, parsedName[i], Game1.smallFont,
                            c.getVector2() - new Vector2(nameSize.X / 2, nameSize.Y + 32 + (20 - 30 * i) * (parsedName.Length > 1 ? 1 : 0)) + new Vector2(c.bounds.Width / 2, c.bounds.Height),
                            Color.Black, Color.Black * 0.15f);
                    }
                }
            }

            if (previousPageButton != null)
            {
                previousPageButton.draw(b, Color.White, 0.88f);
                string s = Game1.content.LoadString("Strings\\UI:MasteryExtended_BackButton");
                Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont,
                    previousPageButton.getVector2() + new Vector2(
                        previousPageButton.bounds.Width / 2 - Game1.dialogueFont.MeasureString(s).X / 2f,
                        29 - (int)Math.Ceiling(Game1.dialogueFont.MeasureString(s).Y / 2) + (float)(previousPageButton.sourceRect.X == 84 ? 8 : 0)),
                    Color.Black * 1f, Color.Black * 0.2f, 1f, 0.9f);
            }

            if (nextPageButton != null)
            {
                nextPageButton.draw(b, Color.White, 0.88f);
                string s = Game1.content.LoadString("Strings\\UI:MasteryExtended_NextButton");
                Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont,
                    nextPageButton.getVector2() + new Vector2(
                        nextPageButton.bounds.Width / 2 - Game1.dialogueFont.MeasureString(s).X / 2f,
                        6f + (nextPageButton.sourceRect.X == 84 ? 8 : 0)),
                    Color.Black * 1f, Color.Black * 0.2f, 1f, 0.9f);
            }

            if (MasteryTrackerMenu.getCurrentMasteryLevel() <= (int)Game1.stats.Get("masteryLevelsSpent"))
            {
                drawHoverText(b, Game1.content.LoadString("Strings\\UI:MasteryExtended_LookOnly"), Game1.smallFont, overrideX: 0, overrideY: 0,
                    boxTexture: Game1.mouseCursors_1_6, boxSourceRect: new Rectangle(1, 85, 21, 21), boxShadowColor: Color.Black,
                    textColor: Color.Black, textShadowColor: Color.Black * 0.2f, boxScale: 2f);
            }
            drawHoverText(b, Game1.parseText(hoverText, Game1.smallFont, 500), Game1.smallFont,
                boxTexture: Game1.mouseCursors_1_6,
                boxSourceRect: new Rectangle(1, 85, 21, 21),
                boxShadowColor: innerSkill.getLevel() <= 10 ? Color.Black : null,
                textColor: Color.Black, textShadowColor: Color.Black * 0.2f, boxScale: 2f);

            base.draw(b);
            drawMouse(b); // Adds the mouse
        }

        public override void performHoverAction(int x, int y)
        {
            hoverText = "";

            // Profesiones
            foreach (ClickableTextureComponent c in pageTextureComponents)
            {
                c.region = 0;
                if (c.bounds.Contains(x, y))
                {
                    Game1.SetFreeCursorDrag();

                    // Set highlight background
                    c.region = 1;

                    if (!string.IsNullOrEmpty(c.hoverText))
                    {
                        Profession hoveredProfession = innerSkill.Professions.Find(p => p.Id == c.myID)!;
                        string unlocked = hoveredProfession.IsProfessionUnlocked() ? Game1.content.LoadString("Strings\\UI:MasteryExtended_AlreadyUnlocked") + "\n\n" : "";
                        string requires = "";
                        if (unlocked.Length == 0)
                        {
                            int numRequired = Utility.Clamp(1, ModEntry.Config.Lvl15ProfessionsRequired, 8);
                            requires += innerSkill.unlockedProfessionsCount(15) < numRequired ? Game1.content.LoadString("Strings\\UI:MasteryExtended_RequirementsLvl15", numRequired) + "\n" : "";
                            requires += innerSkill.unlockedProfessionsCount(20) == 0 ? Game1.content.LoadString("Strings\\UI:MasteryExtended_RequirementsLvl20") + "\n" : "";
                        }

                        if (requires != "") requires = Game1.content.LoadString("Strings\\UI:MasteryExtended_RequirementsTitle") + "\n" + requires + "\n";

                        hoverText = unlocked + requires + c.hoverText;
                    }
                }
            }

            base.performHoverAction(x, y); // Do the hover for the close button
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            Game1.stats.Get("MasteryExp");
            int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
            int levelsNotSpent = levelsAchieved - (int)Game1.stats.Get("masteryLevelsSpent");

            foreach (ClickableTextureComponent c in pageTextureComponents)
            {
                if (c.bounds.Contains(x, y) && c.myAlternateID == 0 && levelsNotSpent > 0)
                {
                    // Add the profession and spend the mastery
                    var professionToAdd = innerSkill.Professions.Find(p => p.Id == c.myID)!;
                    professionToAdd.AddProfessionToPlayer();
                    Game1.stats.Increment("masteryLevelsSpent");

                    // Show which one was added
                    Game1.drawObjectDialogue(
                        Game1.content.LoadString("Strings\\UI:MasteryExtended_AddedProfession", innerSkill.GetName(), professionToAdd.GetName())
                    );

                    // Update the map
                    Game1.currentLocation.MakeMapModifications(true);
                }
            }

            if (previousPageButton?.bounds.Contains(x, y) == true && previousPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                pressedButtonTimer = 200f;
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
                    Game1.activeClickableMenu = new MasteryProfessions15Page(innerSkill);
                }
            }

            if (pressedButtonTimer > 0f)
            {
                pressedButtonTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
                previousPageButton!.sourceRect.X = 84;

                if (pressedButtonTimer <= 0f)
                {
                    destroyTimer = 100f;
                }
            }
            base.update(time);
        }
    }
}
