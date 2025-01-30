using MasteryExtended.Menu.Pages;
using StardewValley.Menus;
using StardewValley;
using Microsoft.Xna.Framework;
using MasteryExtended.Skills;
using MasteryExtended.VPP.Menu.Pages;

namespace MasteryExtended.VPP.Patches
{
    internal static class MasteryProfessionsPagePatch
    {
        internal static void CtorPostfix(MasteryProfessionsPage __instance, Skill innerSkill)
        {
            if (innerSkill.Id is > 4 or < 0) return;

            var xPosition = __instance.previousPageButton!.bounds.X;
            var yPosition = __instance.previousPageButton!.bounds.Y;

            __instance.nextPageButton = new ClickableTextureComponent(new Rectangle(xPosition + 84 + 8, yPosition, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
            {
                visible = true,
                myID = 998
            };
            __instance.previousPageButton!.bounds.X -= 84 + 8;
        }

        internal static bool receiveLeftClickPrefix(MasteryProfessionsPage __instance, int x, int y)
        {
            if (__instance.nextPageButton?.bounds.Contains(x, y) == true && __instance.nextPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                __instance.pressedButtonTimer = 200f;
                __instance.nextPageButton.region = 1;
                return false;
            }
            return true;
        }

        internal static bool updatePrefix(MasteryProfessionsPage __instance, GameTime time)
        {
            if (__instance.nextPageButton?.region == 0 || __instance.nextPageButton == null) return true;

            if (__instance.destroyTimer > 0f)
            {
                __instance.destroyTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;

                if (__instance.destroyTimer <= 0f)
                {
                    Game1.activeClickableMenu = new MasteryProfessions15Page(__instance.innerSkill);
                    return false;
                }
            }

            if (__instance.pressedButtonTimer > 0f)
            {
                __instance.pressedButtonTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
                __instance.nextPageButton!.sourceRect.X = 84;

                if (__instance.pressedButtonTimer <= 0f)
                {
                    __instance.destroyTimer = 100f;
                }
            }

            return false;
        }
    }
}
