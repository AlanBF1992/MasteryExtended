using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StardewValley.Constants;
using MasteryExtended.Skills;
using System.Security.Claims;
using MasteryExtended.Menu.Pages;
using System.Collections.Generic;
using System;

namespace MasteryExtended.Patches
{
    internal static class MasteryTrackerMenuPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

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

                for (int i = 1; i <= ModEntry.MaxMasteryPoints - 5; i++)
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
        internal static bool drawBarPrefix(SpriteBatch b, Vector2 topLeftSpot, float widthScale = 1f)
        {
            try
            {
                float fullWidthScale = widthScale;

                int masteryExp = (int)Game1.stats.Get("MasteryExp");
                int masteryLevelAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
                int masterySpent = (int)Game1.stats.Get("masteryLevelsSpent");

                float currentProgressXP = masteryExp - MasteryTrackerMenu.getMasteryExpNeededForLevel(masteryLevelAchieved);

                float expNeededToReachNextLevel = MasteryTrackerMenu.getMasteryExpNeededForLevel(masteryLevelAchieved + 1) - MasteryTrackerMenu.getMasteryExpNeededForLevel(masteryLevelAchieved);

                // El largo de la barra en el menú se debe ajustar porque a niveles mayores el número la cubre
                if (fullWidthScale != 1f) {
                    // A little shorter
                    widthScale *= masterySpent >= 10 ? 0.725f : (masteryLevelAchieved >= 10 ? 0.7875f : 0.85f);
                    // The first one in each section removes the original
                    // Shadow
                    b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 103, (int)topLeftSpot.Y + 144, (int)(584f * fullWidthScale) + 4, 40), new Color(242, 177, 107));
                    b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 103, (int)topLeftSpot.Y + 144, (int)(584f * widthScale) + 4, 40), Color.Black * 0.35f);

                    // Border
                    b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 108, (int)topLeftSpot.Y + 140, (int)(146 * 4 * fullWidthScale) + 4, 40), new Color(242, 177, 107));
                    b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 108, (int)topLeftSpot.Y + 140, (int)((float)(((masteryLevelAchieved >= ModEntry.MaxMasteryPoints) ? 144 : 146) * 4) * widthScale) + 4, 40), new Color(60, 60, 25));

                    // Background
                    b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 144, (int)(576f * widthScale), 32), new Color(173, 129, 79));
                }

                int barWidth = (int)(576f * currentProgressXP / expNeededToReachNextLevel * widthScale);

                // Experience Bar
                if (masteryLevelAchieved >= ModEntry.MaxMasteryPoints)
                {
                    barWidth = (int)(576f * widthScale);
                }
                if (masteryLevelAchieved >= ModEntry.MaxMasteryPoints || barWidth > 0)
                {
                    Color light = new(60, 180, 80);
                    Color med = new(0, 113, 62);
                    Color medDark = new(0, 80, 50);
                    Color dark = new(0, 60, 30);
                    if (masteryLevelAchieved >= ModEntry.MaxMasteryPoints && fullWidthScale == 1f)
                    {
                        light = new(220, 220, 220);
                        med = new(140, 140, 140);
                        medDark = new(80, 80, 80);
                        dark = med;
                    }
                    if (fullWidthScale != 1f)
                    {
                        dark = medDark;
                    }
                    b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 144, barWidth, 32), med);
                    b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 148, 4, 28), medDark);
                    if (barWidth > 8)
                    {
                        b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 112, (int)topLeftSpot.Y + 172, barWidth - 8, 4), medDark);
                        b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 116, (int)topLeftSpot.Y + 144, barWidth - 4, 4), light);
                        b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 104 + barWidth, (int)topLeftSpot.Y + 144, 4, 28), light);
                        b.Draw(Game1.staminaRect, new Rectangle((int)topLeftSpot.X + 108 + barWidth, (int)topLeftSpot.Y + 144, 4, 32), dark);
                    }
                }
                if (masteryLevelAchieved < ModEntry.MaxMasteryPoints)
                {
                    string s = currentProgressXP + "/" + expNeededToReachNextLevel;
                    b.DrawString(Game1.smallFont, s, new Vector2((float)((int)topLeftSpot.X + 112) + 288f * widthScale - Game1.smallFont.MeasureString(s).X / 2f, (float)(int)topLeftSpot.Y + 146f), Color.White * 0.75f);
                }

                return false;
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(drawBarPrefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }

        /// <summary>Modifica el tamaño del menu pedestal y hace visible el botón</summary>
        internal static void MasteryTrackerMenuPostfix(MasteryTrackerMenu __instance, int whichSkill = -1)
        {
            if (whichSkill == -1)
            {
                // Add more height for the button
                __instance.height += 48;
                __instance.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(800, __instance.height).Y;
                __instance.upperRightCloseButton.bounds.Y = __instance.yPositionOnScreen - 8;

                // Crear el botón
                if (Game1.player.stats.Get(StatKeys.Mastery(-1)) == 0)
                {
                    __instance.mainButton = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + __instance.width / 2 - 168/2, __instance.yPositionOnScreen + __instance.height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
                    {
                        visible = true,
                        myID = 0
                    };
                }
            } else
            {
                int levelsNotSpent = MasteryTrackerMenu.getCurrentMasteryLevel() - (int)Game1.stats.Get("masteryLevelsSpent");
                int numUnlockedProfessions = MasterySkillsPage.skills.Find(s => s.Id == whichSkill)!.Professions.FindAll(p => p.IsProfessionUnlocked()).Count;
                bool claim = levelsNotSpent > 0 && numUnlockedProfessions >= 3;

                __instance.SetInstanceField("canClaim", claim);
            }
        }

        internal static void drawPostfix(MasteryTrackerMenu __instance, SpriteBatch b)
        {
            if ((int)__instance.GetInstanceField("which")! != -1) {
                if (__instance.mainButton != null && !(bool)__instance.GetInstanceField("canClaim")!)
                {
                    //Parche sobre el botón
                    b.Draw(Game1.staminaRect, new Rectangle(__instance.mainButton.bounds.X, __instance.mainButton.bounds.Y, __instance.mainButton.bounds.Width, __instance.mainButton.bounds.Height + 5), new Color(137, 137, 137));
                    __instance.mainButton?.draw(b, Color.White * 0.5f, 0.88f);
                    string s = Game1.content.LoadString("Strings\\1_6_Strings:Claim");
                    Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont, __instance.mainButton!.getVector2() + new Vector2((float)(__instance.mainButton.bounds.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f, 6f + (float)((__instance.mainButton.sourceRect.X == 84) ? 8 : 0)), Color.Black * 0.5f, Color.Black * 0.2f, 1f, 0.9f);

                    __instance.drawMouse(b);
                }
            } else
            {
                Game1.stats.Get("MasteryExp");
                int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
                int levelsNotSpent = levelsAchieved - (int)Game1.stats.Get("masteryLevelsSpent");

                for (int i = 0; i < 5; i++)
                {
                    b.Draw(Game1.mouseCursors_1_6,
                        new Vector2((float)(__instance.xPositionOnScreen + __instance.width / 2) - 110f + (float)(i * 11 * 4), __instance.yPositionOnScreen + 220),
                        new Rectangle((i >= ModEntry.Data.claimedRewards && i < levelsAchieved) ?
                            (43 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600 / 100 * 10) :
                            ((ModEntry.Data.claimedRewards > i) ? 33 : 23), //23 = vacío, 33 el dorado
                            89, 10, 11),
                        Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
                }

                // Add the button
                if (__instance.mainButton != null)
                {
                    __instance.mainButton.draw(b, (levelsNotSpent > 0) ? Color.White : (Color.White * 0.5f), 0.88f);
                    string s = ModEntry.ModHelper.Translation.Get("invest-button");
                    Utility.drawTextWithColoredShadow(b, s, Game1.dialogueFont,
                        __instance.mainButton.getVector2() + new Vector2(
                            (float)(__instance.mainButton.bounds.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f,
                            29 - (int)Math.Ceiling(Game1.dialogueFont.MeasureString(s).Y / 2) + (float)((__instance.mainButton.sourceRect.X == 84) ? 8 : 0)),
                        Color.Black * ((levelsNotSpent > 0) ? 1f : 0.5f), Color.Black * 0.2f, 1f, 0.9f);
                }
            }

            if (__instance.mainButton != null)
            {
                IClickableMenu.drawHoverText(b,
                    __instance.mainButton.name,
                    Game1.smallFont,
                    boxTexture: Game1.mouseCursors_1_6, boxSourceRect: new Rectangle(1, 85, 21, 21),
                    textShadowColor: Color.Black * 0.2f, boxScale: 1f);
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
                if (!((float)__instance.GetInstanceField("destroyTimer")! > 0f) && __instance.mainButton?.containsPoint(x, y) == true && (float)__instance.GetInstanceField("pressedButtonTimer")! <= 0f && (bool)__instance.GetInstanceField("canClaim")!)
                {
                    ModEntry.Data.claimedRewards++;
                }
                return true;
            }
            if (!((float)__instance.GetInstanceField("destroyTimer")! > 0f))
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
                Game1.stats.Get("MasteryExp");
                int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
                int levelsNotSpent = levelsAchieved - (int)Game1.stats.Get("masteryLevelsSpent");

                if (__instance.mainButton != null && levelsNotSpent > 0 && __instance.mainButton.containsPoint(x, y) && (float)__instance.GetInstanceField("pressedButtonTimer")! <= 0f)
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
                    bool enoughProfessions = MasterySkillsPage.skills.Find(s => s.Id == which)!.Professions.FindAll(p => p.IsProfessionUnlocked()).Count >= 3;

                    __instance.mainButton.name +=
                        (!enoughProfessions ? "You need 3 professions in this Skill." : "") +
                        (!enoughProfessions && !freeLevel ? "\n" : "");
                }
                __instance.mainButton.name += (!freeLevel ? "You need an extra Mastery Level." : "");
            }
        }
    }
}