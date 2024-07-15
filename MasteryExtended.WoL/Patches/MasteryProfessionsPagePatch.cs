using HarmonyLib;
using MasteryExtended.Menu.Pages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace MasteryExtended.WoL.Patches
{
    internal static class MasteryProfessionsPagePatch
    {
        // Constructor, prestiged son myAlternateId = 2
        internal static void MasteryProfessionsPagePatchPostfix(MasteryProfessionsPage __instance)
        {
            foreach (var c in __instance.pageTextureComponents)
            {
                if (Game1.player.professions.Contains(c.myID + 100))
                {
                    c.myAlternateID = 2;
                }
            }
        }

        //Adds the stars
        internal static void drawPostfix(MasteryProfessionsPage __instance, SpriteBatch b)
        {
            if (__instance.innerSkill.getLevel() <= 10) return;
            if (__instance.innerSkill.Id < 0 || 4 < __instance.innerSkill.Id) return;

            foreach (ClickableTextureComponent c in __instance.pageTextureComponents)
            {
                if (c.myAlternateID == 0) continue;

                var reqProf = __instance.innerSkill.Professions.Find(p => p.Id == c.myID)!.RequiredProfessions;

                bool prestiged = Game1.player.professions.Contains(c.myID + 100);
                bool canPrestige = prestiged? true: (reqProf == null? true: Game1.player.professions.Contains(reqProf.Id + 100));
                if (!canPrestige) continue;
                b.Draw(Game1.mouseCursors_1_6, new Vector2(c.bounds.Right - 40, c.bounds.Top + 10), new Rectangle(prestiged? 33: 23, 89, 10, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.88f);
            }

            Utilities.newDrawHoverText(b, Game1.parseText(__instance.hoverText, Game1.smallFont, 500), Game1.smallFont,
                boxTexture: Game1.mouseCursors_1_6,
                boxSourceRect: new Rectangle(1, 85, 21, 21), boxShadowColor: Color.Black,
                textColor: Color.Black, textShadowColor: Color.Black * 0.2f, boxScale: 2f);

            __instance.drawMouse(b); // Adds the mouse
        }

        internal static void receiveLeftClickPostfix(MasteryProfessionsPage __instance, int x, int y)
        {
            if (__instance.innerSkill.Professions.Count(p => Game1.player.professions.Contains(p.Id + 100)) < 2) return;

            int levelsAchieved = MasteryTrackerMenu.getCurrentMasteryLevel();
            int levelsNotSpent = levelsAchieved - (int)Game1.stats.Get("masteryLevelsSpent");

            foreach (ClickableTextureComponent c in __instance.pageTextureComponents)
            {
                if (c.bounds.Contains(x, y) && c.myAlternateID == 1 && levelsNotSpent > 0)
                {
                    var reqProf = __instance.innerSkill.Professions.Find(p => p.Id == c.myID)!.RequiredProfessions;

                    bool canPrestige = (reqProf == null || Game1.player.professions.Contains(reqProf.Id + 100));
                    if (!canPrestige) continue;

                    Game1.player.professions.Add(c.myID + 100);
                    Game1.stats.Increment("masteryLevelsSpent");

                    var professionToAdd = __instance.innerSkill.Professions.Find(p => p.Id == c.myID)!;
                    // Show which one was added
                    Game1.drawObjectDialogue(
                        ModEntry.ModHelper.Translation.Get("prestiged-profession", new { skill = __instance.innerSkill.GetName(), prof = professionToAdd.GetName() })
                    );
                }
            }
        }

        internal static void performHoverActionPostfix(MasteryProfessionsPage __instance, int x, int y)
        {
            if (__instance.innerSkill.Id < 0 || 4 < __instance.innerSkill.Id) return;

            foreach (ClickableTextureComponent c in __instance.pageTextureComponents)
            {
                if (c.bounds.Contains(x, y))
                {
                    if (c.myAlternateID == 0) return;

                    Game1.SetFreeCursorDrag();
                    var dalionProf = DaLion.Professions.Framework.VanillaProfession.FromValue(c.myID);

                    if (c.myAlternateID == 1)
                    {
                        __instance.hoverText += "\n\nPrestiged: " + dalionProf.GetTitle(true) + "\n" + dalionProf.GetDescription(true);
                    }
                    if (c.myAlternateID == 2)
                    {
                        __instance.hoverText = "= Already prestiged =\n"+ dalionProf.GetDescription(true);
                    }
                }
            }
        }
    }
}
