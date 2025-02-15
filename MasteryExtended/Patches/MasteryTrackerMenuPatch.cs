using HarmonyLib;
using MasteryExtended.Menu;
using MasteryExtended.Menu.Pages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class MasteryTrackerMenuPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static bool getMasteryExpNeededForLevelPrefix(int level, ref int __result)
        {
            if (level <= 5) { return true; }

            __result = MasteryTrackerMenu.getMasteryExpNeededForLevel(5) + (level - 5) * ModEntry.Config.MasteryExpPerLevel;

            return false;
        }

        /// <summary>Permite que el nivel de maestría sea mayor a 5.</summary>
        internal static void getCurrentMasteryLevelPostfix(ref int __result)
        {
            try
            {
                int masteryExp = (int)Game1.stats.Get("MasteryExp");

                int level = 0;

                for (int i = 1; i <= ModEntry.MaxMasteryLevels - 5; i++)
                {
                    if (masteryExp >= MasteryTrackerMenu.getMasteryExpNeededForLevel(5 + i))
                    {
                        level++;
                    }
                }

                __result += level;
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(getCurrentMasteryLevelPostfix)}:\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>Hace más pequeña la barra de maestría en el menú de habilidades y muestra maestrías mayores.</summary>
        internal static IEnumerable<CodeInstruction> drawBarTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo MaxMasteryLevelsInfo = AccessTools.PropertyGetter(typeof(ModEntry), nameof(ModEntry.MaxMasteryLevels));

                // from: 5
                // to:   ModEntry.MaxMasteryLevels
                matcher
                    .MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_5))
                    .Set(OpCodes.Call, MaxMasteryLevelsInfo)
                    .MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_5))
                    .Set(OpCodes.Call, MaxMasteryLevelsInfo)
                    .MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_5))
                    .Set(OpCodes.Call, MaxMasteryLevelsInfo)
                    .MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_5))
                    .Set(OpCodes.Call, MaxMasteryLevelsInfo)
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(drawBarTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /// <summary>Modifica el tamaño del menu pedestal y hace visible el botón</summary>
        internal static void MasteryTrackerMenuPostfix(MasteryTrackerMenu __instance, int whichSkill = -1)
        {
            int levelsNotSpent = MasteryTrackerMenu.getCurrentMasteryLevel() - (int)Game1.stats.Get("masteryLevelsSpent");

            if (whichSkill == -1)
            {
                // Add more height for the button
                __instance.height += 48;
                __instance.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(800, __instance.height).Y;
                __instance.upperRightCloseButton.bounds.Y = __instance.yPositionOnScreen - 8;

                // Crear el botón
                if (Game1.player.stats.Get(StatKeys.Mastery(-1)) == 0)
                {
                    __instance.mainButton = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + __instance.width / 2 - 168 / 2, __instance.yPositionOnScreen + __instance.height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
                    {
                        visible = true,
                        myID = 0,
                        myAlternateID = levelsNotSpent,
                        upNeighborID = __instance.upperRightCloseButton.myID
                    };
                }
                // Snap
                if (Game1.options.SnappyMenus)
                {
                    __instance.populateClickableComponentList();
                    __instance.currentlySnappedComponent = __instance.getComponentWithID(__instance.mainButton == null ? __instance.upperRightCloseButton.myID : 0);
                    __instance.snapCursorToCurrentSnappedComponent();
                }
            }
            else
            {
                int numUnlockedProfessions = MasterySkillsPage.skills.Find(s => s.Id == whichSkill)!.Professions.FindAll(p => p.IsProfessionUnlocked()).Count;
                bool claim = levelsNotSpent > 0 && numUnlockedProfessions>= (ModEntry.Config.ExtraRequiredProfession? 3: 2);

                __instance.SetInstanceField("canClaim", claim);
            }
        }

        internal static void drawPostfix(MasteryTrackerMenu __instance, SpriteBatch b)
        {
            if ((int)__instance.GetInstanceField("which")! != -1)
            {
                if (__instance.mainButton != null && !(bool)__instance.GetInstanceField("canClaim")!)
                {
                    //Parche sobre el botón
                    b.Draw(Game1.staminaRect, new Rectangle(__instance.mainButton.bounds.X, __instance.mainButton.bounds.Y, __instance.mainButton.bounds.Width, __instance.mainButton.bounds.Height + 5), new Color(137, 137, 137));
                    __instance.mainButton?.draw(b, Color.White * 0.5f, 0.88f);
                    string s = Game1.content.LoadString("Strings\\1_6_Strings:Claim");
                    Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont, __instance.mainButton!.getVector2() + new Vector2((float)(__instance.mainButton.bounds.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f, 6f + (float)((__instance.mainButton.sourceRect.X == 84) ? 8 : 0)), Color.Black * 0.5f, Color.Black * 0.2f, 1f, 0.9f);

                    __instance.drawMouse(b);
                }
            }
            else
            {
                Game1.stats.Get("MasteryExp");
                int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();

                //Stars
                for (int i = 0; i < 5; i++)
                {
                    b.Draw(Game1.mouseCursors_1_6,
                        new Vector2((float)(__instance.xPositionOnScreen + __instance.width / 2) - 110f + (float)(i * 11 * 4), __instance.yPositionOnScreen + 220),
                        new Rectangle((i >= Utilities.countClaimedPillars() && i < levelsAchieved) ?
                            (43 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600 / 100 * 10) :
                            ((Utilities.countClaimedPillars() > i) ? 33 : 23), //23 = vacío, 33 el dorado
                            89, 10, 11),
                        Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                }

                // Add the button
                if (__instance.mainButton != null)
                {
                    __instance.mainButton.draw(b, Color.White, 0.88f);
                    string s = Game1.content.LoadString("Strings\\UI:MasteryExtended_InvestButton");
                    Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont,
                        __instance.mainButton.getVector2() + new Vector2(
                            (float)(__instance.mainButton.bounds.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f,
                            29 - (int)Math.Ceiling(Game1.dialogueFont.MeasureString(s).Y / 2) + (float)((__instance.mainButton.sourceRect.X == 84) ? 8 : 0)),
                        Color.Black, Color.Black * 0.2f, 1f, 0.9f);
                }
            }

            if (__instance.mainButton != null)
            {
                MasteryPage.drawHoverText(b, __instance.mainButton.name, Game1.smallFont,
                    boxTexture: Game1.mouseCursors_1_6,
                    boxSourceRect: new Rectangle(1, 85, 21, 21),
                    textColor: Color.Black, textShadowColor: Color.Black * 0.2f, boxScale: 2f);
            }

            __instance.drawMouse(b);
        }

        /// <summary>
        /// Modified so it doesn't try to claim reward
        /// </summary>
        internal static bool receiveLeftClickPrefix(MasteryTrackerMenu __instance, int x, int y, bool playSound = true)
        {
            if ((int)__instance.GetInstanceField("which")! != -1)
            {
                if (((float)__instance.GetInstanceField("destroyTimer")! <= 0f) && __instance.mainButton?.containsPoint(x, y) == true && (float)__instance.GetInstanceField("pressedButtonTimer")! <= 0f && (bool)__instance.GetInstanceField("canClaim")!)
                {
                    ModEntry.Data.claimedRewards++;
                }
                return true;
            }
            if ((float)__instance.GetInstanceField("destroyTimer")! <= 0f)
            {
                // Botón de cerrado, porque no puedo llamar a la base.
                if (__instance.upperRightCloseButton != null && __instance.readyToClose() && __instance.upperRightCloseButton.containsPoint(x, y))
                {
                    if (playSound)
                    {
                        Game1.playSound((string)__instance.GetInstanceField("closeSound")!);
                    }
                    __instance.exitThisMenu();
                }

                // Botón de maestrías
                if (__instance.mainButton?.containsPoint(x, y) == true && (float)__instance.GetInstanceField("pressedButtonTimer")! <= 0f)
                {
                    Game1.playSound("cowboy_monsterhit");
                    DelayedAction.playSoundAfterDelay("cowboy_monsterhit", 100);
                    __instance.SetInstanceField("pressedButtonTimer", 100f);
                    // The function call is in the update, or else it doesn't push the button
                }
            }

            return false;
        }

        internal static bool updatePrefix(MasteryTrackerMenu __instance, GameTime time)
        {
            if ((int)__instance.GetInstanceField("which")! != -1)
            {
                    return true;
            }

            if ((float)__instance.GetInstanceField("destroyTimer")! > 0f)
            {
                __instance.SetInstanceField("destroyTimer", (float)__instance.GetInstanceField("destroyTimer")! - (int)time.ElapsedGameTime.TotalMilliseconds);
                if ((float)__instance.GetInstanceField("destroyTimer")! <= 0f)
                {
                    Game1.activeClickableMenu = new MasterySkillsPage(1);
                    Game1.playSound("discoverMineral");
                }
            }
            if ((float)__instance.GetInstanceField("pressedButtonTimer")! > 0f)
            {
                __instance.SetInstanceField("pressedButtonTimer", (float)__instance.GetInstanceField("pressedButtonTimer")! - (int)time.ElapsedGameTime.TotalMilliseconds);
                __instance.mainButton.sourceRect.X = 84;
                if ((float)__instance.GetInstanceField("pressedButtonTimer")! <= 0f)
                {
                    __instance.SetInstanceField("destroyTimer", 100f);
                }
            }
            return false;
        }

        internal static void performHoverActionPostfix(MasteryTrackerMenu __instance, int x, int y)
        {
            int which = (int)__instance.GetInstanceField("which")!;
            if (__instance.mainButton != null)
            {
                __instance.mainButton.name = "";
            }
            if (__instance.mainButton?.containsPoint(x, y) == true && !(bool)__instance.GetInstanceField("canClaim")!)
            {
                bool freeLevel = MasteryTrackerMenu.getCurrentMasteryLevel() > (int)Game1.stats.Get("masteryLevelsSpent");

                if (which != -1)
                {
                    bool enoughProfessions = MasterySkillsPage.skills.Find(s => s.Id == which)!.Professions.FindAll(p => p.IsProfessionUnlocked()).Count >= (ModEntry.Config.ExtraRequiredProfession? 3 : 2);

                    __instance.mainButton.name +=
                        (!enoughProfessions ?
                        Game1.content.LoadString("Strings\\UI:MasteryExtended_NeedMoreProfessions", ModEntry.Config.ExtraRequiredProfession ? 3 : 2) : "") +
                        (!enoughProfessions && !freeLevel ? "\n" : "") +
                        (!freeLevel ? Game1.content.LoadString("Strings\\UI:MasteryExtended_NeedMoreLevels") : "");
                }
                else {
                    __instance.mainButton.name +=
                        (!freeLevel ? Game1.content.LoadString("Strings\\UI:MasteryExtended_NeedMoreLevels") : "") +
                        (!freeLevel ? $"\n{Game1.content.LoadString("Strings\\UI:MasteryExtended_CantSpend")}" : "");
                }
            }
        }
    }
}