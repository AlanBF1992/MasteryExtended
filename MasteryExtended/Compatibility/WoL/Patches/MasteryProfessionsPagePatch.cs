using HarmonyLib;
using MasteryExtended.Menu.Pages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class MasteryProfessionsPagePatch
    {
        // Constructor, prestiged son myAlternateId = 2
        internal static void MasteryProfessionsPagePatchPostfix(MasteryProfessionsPage __instance)
        {
            foreach (var c in __instance.pageTextureComponents.Where(c => Game1.player.professions.Contains(c.myID + 100)))
            {
                c.myAlternateID = 3;
            }
        }

        //Adds the stars
        internal static void drawPostfix(MasteryProfessionsPage __instance, SpriteBatch b)
        {
            if (__instance.innerSkill.getLevel() <= 10) return;
            if (__instance.innerSkill.Id is < 0 or > 4) return;

            foreach (ClickableTextureComponent c in __instance.pageTextureComponents.Where(c => c.myAlternateID != 0))
            {
                var reqProf = __instance.innerSkill.Professions.Find(p => p.Id == c.myID)!.RequiredProfessions;
                bool prestiged = Game1.player.professions.Contains(c.myID + 100);
                bool canPrestige = prestiged || reqProf == null || Game1.player.professions.Contains(reqProf.Id + 100);

                if (!canPrestige) continue;
                b.Draw(Game1.mouseCursors_1_6, new Vector2(c.bounds.Right - 40, c.bounds.Top + 10), new Rectangle(prestiged ? 33 : 23, 89, 10, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.88f);
            }

            MasteryProfessionsPage.drawHoverText(b, Game1.parseText(__instance.hoverText, Game1.smallFont, 500), Game1.smallFont,
                boxTexture: Game1.mouseCursors_1_6,
                boxSourceRect: new Rectangle(1, 85, 21, 21),
                boxShadowColor: Color.Black,
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

                    bool canPrestige = reqProf == null || Game1.player.professions.Contains(reqProf.Id + 100);
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
            if (__instance.innerSkill.Id is < 0 or > 4) return;

            foreach (ClickableTextureComponent c in __instance.pageTextureComponents.Where(c => c.bounds.Contains(x, y)))
            {
                Game1.SetFreeCursorDrag();
                var profMethod = AccessTools.Method("DaLion.Professions.Framework.VanillaProfession:FromValue", [typeof(int)]);
                dynamic dalionProf = profMethod.Invoke(null, [c.myID])!;

                switch (c.myAlternateID)
                {
                    case 0:
                    case 1:
                    case 2:
                        __instance.hoverText += "\n\nPrestiged: " + dalionProf.GetTitle(true) + "\n";
                        __instance.hoverText += dalionProf.GetDescription(true);
                        break;
                    case 3:
                        __instance.hoverText = "= Already prestiged =\n" + dalionProf.GetDescription(true);
                        break;
                }
            }
        }
    }
}
