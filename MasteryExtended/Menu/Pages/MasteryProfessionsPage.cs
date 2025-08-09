using MasteryExtended.Skills;
using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace MasteryExtended.Menu.Pages
{
    /// <summary>
    /// Menu that shows the Professions
    /// </summary>
    public class MasteryProfessionsPage: MasteryPage
    {
        public Skill innerSkill;
        public List<Profession> LeftProfessionTree = [];
        public List<Profession> RightProfessionTree = [];

        public MasteryProfessionsPage(Skill innerSkill)
            : base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 160).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 160).Y, 1280, 160, showUpperRightCloseButton: true)
        {
            closeSound = "stone_button";

            this.innerSkill = innerSkill;

            // Every Skill has 2 main profession trees with a main Profession
            List<Profession> TreeHead = innerSkill.Professions.Where(p => p.LevelRequired == 5).ToList();

            LeftProfessionTree.Add(TreeHead[0]);
            RightProfessionTree.Add(TreeHead[1]);

            LeftProfessionTree.AddRange(innerSkill.Professions.Where(p => p.RequiredProfessions == LeftProfessionTree[0]).ToList());
            RightProfessionTree.AddRange(innerSkill.Professions.Where(p => p.RequiredProfessions == RightProfessionTree[0]).ToList());

            // Título
            MenuTitle = Game1.content.LoadString("Strings\\UI:MasteryExtended_MenuTitleProfession", innerSkill.GetName());

            // Calcular tamaños y posiciones de los componentes
            const int xPadding = 64;
            const int xSpaceBetweenProfessions = 8;
            const int xSpaceBetweenTrees = 32;

            const int yTopPadding = 80 + 64;
            const int ySpaceBetweenProfessions = 8;

            int professionWidth = (width - 2 * xPadding - 2 * xSpaceBetweenProfessions - xSpaceBetweenTrees) / 4;
            const int professionHeight = 152;

            List<int> xPosition =
            [
                xPositionOnScreen + xPadding + professionWidth /2 + xSpaceBetweenProfessions/2,
                xPositionOnScreen + xPadding,
                xPositionOnScreen + xPadding + professionWidth + xSpaceBetweenProfessions
            ];

            List<int> yPosition =
            [
                yPositionOnScreen + yTopPadding,
                yPositionOnScreen + yTopPadding + professionHeight + ySpaceBetweenProfessions,
                yPositionOnScreen + yTopPadding + professionHeight + ySpaceBetweenProfessions
            ];

            int spacingSecondTree = -xPadding + width / 2 + xSpaceBetweenTrees / 2;

            // Agregar los componentes y su posición
            // myAlternateID:
            // 0: No se puede activar // Falta el anterior
            // 1: Se puede activar
            // 2: Está activo y se puede hacer prestige
            // 3: Está prestiged
            for (int i = 0; i < 3; i++)
            {
                Profession pI = LeftProfessionTree[i];
                Profession pR = RightProfessionTree[i];

                pageTextureComponents.Add(new ClickableTextureComponent(new Rectangle(xPosition[i], yPosition[i], professionWidth, professionHeight), pI.TextureSource(), pI.TextureBounds, 1f, drawShadow: true)
                {
                    name = pI.GetName(),
                    hoverText = pI.GetDescription(),
                    myID = pI.Id,
                    myAlternateID = pI.IsProfessionUnlocked() ? 2 : pI.IsRequiredUnlocked() ? 1 : 0
                });

                pageTextureComponents.Add(new ClickableTextureComponent(new Rectangle(xPosition[i] + spacingSecondTree, yPosition[i], professionWidth, professionHeight), pR.TextureSource(), pR.TextureBounds, 1f, drawShadow: true)
                {
                    name = pR.GetName(),
                    hoverText = pR.GetDescription(),
                    myID = pR.Id,
                    myAlternateID = pR.IsProfessionUnlocked() ? 2 : pR.IsRequiredUnlocked() ? 1 : 0
                });
            }

            height += 2 * professionHeight + ySpaceBetweenProfessions + yTopPadding - 64;

            // Hacer espacio para los botones
            height += 48;

            // Offset so the menu is properly centered
            int num = base.yPositionOnScreen;
            base.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(800, base.height).Y;
            int offset = num - base.yPositionOnScreen;
            foreach (ClickableTextureComponent c in this.pageTextureComponents)
            {
                c.bounds.Y -= offset;
            }
            base.upperRightCloseButton.bounds.Y -= offset;

            // Agregar botón atrás para ir al menú de skills
            previousPageButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 - 84, yPositionOnScreen + height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
            {
                visible = true,
                myID = 999
            };

            snapComponents();
        }

        internal void snapComponents()
        {
            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();

                //First row
                pageTextureComponents[0].upNeighborID = upperRightCloseButton.myID;
                pageTextureComponents[1].upNeighborID = upperRightCloseButton.myID;
                pageTextureComponents[0].downNeighborID = pageTextureComponents[2].myID;
                pageTextureComponents[1].downNeighborID = pageTextureComponents[3].myID;

                pageTextureComponents[0].rightNeighborID = pageTextureComponents[1].myID;
                pageTextureComponents[1].leftNeighborID = pageTextureComponents[0].myID;

                //Second Row
                pageTextureComponents[2].rightNeighborID = pageTextureComponents[4].myID;
                pageTextureComponents[4].leftNeighborID = pageTextureComponents[2].myID;

                pageTextureComponents[4].rightNeighborID = pageTextureComponents[3].myID;
                pageTextureComponents[3].leftNeighborID = pageTextureComponents[4].myID;

                pageTextureComponents[3].rightNeighborID = pageTextureComponents[5].myID;

                pageTextureComponents[2].upNeighborID = pageTextureComponents[0].myID;
                pageTextureComponents[4].upNeighborID = pageTextureComponents[0].myID;

                pageTextureComponents[3].upNeighborID = pageTextureComponents[1].myID;
                pageTextureComponents[5].upNeighborID = pageTextureComponents[1].myID;

                pageTextureComponents[2].downNeighborID = previousPageButton!.myID;
                pageTextureComponents[4].downNeighborID = previousPageButton.myID;
                pageTextureComponents[3].downNeighborID = previousPageButton.myID;
                pageTextureComponents[5].downNeighborID = previousPageButton.myID;

                // Buttons
                previousPageButton.upNeighborID = pageTextureComponents[4].myID;
                if (nextPageButton != null)
                {
                    pageTextureComponents[3].downNeighborID = nextPageButton.myID;
                    pageTextureComponents[5].downNeighborID = nextPageButton.myID;

                    nextPageButton.leftNeighborID = previousPageButton.myID;
                    previousPageButton.rightNeighborID = nextPageButton.myID;
                    nextPageButton.upNeighborID = pageTextureComponents[3].myID;
                }

                currentlySnappedComponent = previousPageButton;

                base.snapCursorToCurrentSnappedComponent();
            }
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

                // Agregar una capa verde a los adquiridos (IsProfessionUnlocked == true) o no adquiribles (IsProfessionUnlocked == false)
                if (c.myAlternateID != 1)
                {
                    Color coverColor = innerSkill.Professions.Find(p => p.Id == c.myID)!.IsProfessionUnlocked() ? Color.Green * 0.3f : Color.Black * (c.region == 0 ? 0.75f : 0.6f);
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), c.bounds.X, c.bounds.Y, c.bounds.Width, c.bounds.Height,
                        coverColor,
                        3f, false);
                }

                // Dibuja el icono
                const float iconScale = 3f;
                Utility.drawWithShadow(b, c.texture,
                    c.getVector2() + new Vector2(c.bounds.Width / 2, 24) - new Vector2(16 * iconScale / 2, 0),
                    c.sourceRect, Color.White, 0f, Vector2.Zero, iconScale, shadowIntensity: 0.25f);

                // Dibuja el nombre
                var nameSize = Game1.dialogueFont.MeasureString(c.name);

                if (nameSize.X < c.bounds.Width)
                {
                    Utility.drawTextWithColoredShadow(b, c.name, Game1.dialogueFont,
                    c.getVector2() - new Vector2(nameSize.X / 2, nameSize.Y + 16) + new Vector2(c.bounds.Width / 2, c.bounds.Height),
                    Color.Black, Color.Black * 0.15f);
                }
                else
                {
                    string[] parsedName = Game1.parseText(c.name, Game1.smallFont, c.bounds.Width - 16).Split(Environment.NewLine);
                    for (int i = 0; i < parsedName.Length; i++)
                    {
                        nameSize = Game1.smallFont.MeasureString(parsedName[i].Trim());
                        Utility.drawTextWithColoredShadow(b, parsedName[i], Game1.smallFont,
                            c.getVector2() - new Vector2(nameSize.X / 2, nameSize.Y + 16 + (20 - 30 * i)* (parsedName.Length > 1 ? 1 : 0)) + new Vector2(c.bounds.Width / 2, c.bounds.Height),
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
                        (float)(previousPageButton.bounds.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f,
                        29 - (int)Math.Ceiling(Game1.dialogueFont.MeasureString(s).Y / 2) + (float)((previousPageButton.sourceRect.X == 84) ? 8 : 0)),
                    Color.Black * 1f, Color.Black * 0.2f, 1f, 0.9f);
            }

            if (nextPageButton != null)
            {
                nextPageButton.draw(b, Color.White, 0.88f);
                string s = Game1.content.LoadString("Strings\\UI:MasteryExtended_NextButton");
                Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont,
                    nextPageButton.getVector2() + new Vector2(
                        (float)(nextPageButton.bounds.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f,
                        6f + (float)((nextPageButton.sourceRect.X == 84) ? 8 : 0)),
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
                        string unlocked = hoveredProfession.IsProfessionUnlocked() ? Game1.content.LoadString("Strings\\UI:MasteryExtended_AlreadyUnlocked") +"\n" : "";
                        string requires = "";
                        if (hoveredProfession.RequiredProfessions?.IsProfessionUnlocked() == false)
                        {
                            requires += Game1.content.LoadString("Strings\\UI:MasteryExtended_RequirementsProfession", hoveredProfession.RequiredProfessions.GetName()) + "\n";
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
                if (c.bounds.Contains(x, y) && c.myAlternateID == 1 && levelsNotSpent > 0)
                {
                    // Add the profession and spend the mastery
                    var professionToAdd = innerSkill.Professions.Find(p => p.Id == c.myID)!;
                    professionToAdd.AddProfessionToPlayer();
                    Game1.stats.Increment("masteryLevelsSpent");
                    c.myAlternateID++;

                    // Update the map
                    Game1.currentLocation.MakeMapModifications(true);

                    // Update IDs
                    recalculateState(c.myID);

                    // Show which one was added
                    if (ModEntry.Config.ConfirmProfession)
                    {
                        performHoverAction(0, 0);
                        Game1.afterDialogues = () => SetChildMenu(null);
                        SetChildMenu(new DialogueBox(Game1.content.LoadString("Strings\\UI:MasteryExtended_AddedProfession", innerSkill.GetName(), professionToAdd.GetName())));
                    }
                }
            }

            if (previousPageButton?.bounds.Contains(x, y) == true && previousPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                pressedButtonTimer = 100f;
                previousPageButton.region = 1;
            }

            if (nextPageButton?.bounds.Contains(x, y) == true && nextPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                pressedButtonTimer = 100f;
                nextPageButton.region = 1;
            }
            base.receiveLeftClick(x, y, playSound);
        }

        public void recalculateState(int clickedProfession)
        {
            var idToCheck = innerSkill.Professions.Where(p => p.RequiredProfessions?.Id == clickedProfession).Select(p => p.Id);
            foreach (ClickableTextureComponent c in pageTextureComponents.Where(c => idToCheck.Contains(c.myID)))
            {
                c.myAlternateID++;
            }
        }

        public override void update(GameTime time)
        {
            if (destroyTimer > 0f)
            {
                destroyTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;

                if (destroyTimer <= 0f)
                {
                    Game1.activeClickableMenu = new MasterySkillsPage(1);
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
