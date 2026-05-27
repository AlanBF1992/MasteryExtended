using MasteryExtended.Compatibility.VPP.Menu.Pages;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Compatibility.VPP.Patches
{
    internal static class MasteryProfessionsPagePatch
    {
        /***********
         * PATCHES *
         ***********/
        internal static void ctorPostfix(MasteryProfessionsPage __instance, Skill innerSkill)
        {
            if (innerSkill.Id is > 4 or < 0) return;

            var xPosition = __instance.PreviousPageButton!.bounds.X;
            var yPosition = __instance.PreviousPageButton!.bounds.Y;

            __instance.NextPageButton = new ClickableTextureComponent(new Rectangle(xPosition + 84 + 8, yPosition, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
            {
                visible = true,
                myID = 998
            };
            __instance.PreviousPageButton!.bounds.X -= 84 + 8;

            __instance.snapComponents();
        }

        internal static bool receiveLeftClickPrefix(MasteryProfessionsPage __instance, int x, int y)
        {
            if (__instance.NextPageButton?.bounds.Contains(x, y) == true && __instance.NextPageButton.visible)
            {
                Game1.playSound("cowboy_monsterhit");
                __instance.PressedButtonTimer = 100f;
                __instance.NextPageButton.region = 1;
                return false;
            }
            return true;
        }

        internal static bool updatePrefix(MasteryProfessionsPage __instance, GameTime time)
        {
            if (__instance.NextPageButton?.region == 0 || __instance.NextPageButton == null) return true;

            if (__instance.DestroyTimer > 0f)
            {
                __instance.DestroyTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;

                if (__instance.DestroyTimer <= 0f)
                {
                    Game1.activeClickableMenu = new MasteryProfessions15Page(__instance.InnerSkill);
                    return false;
                }
            }

            if (__instance.PressedButtonTimer > 0f)
            {
                __instance.PressedButtonTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
                __instance.NextPageButton!.sourceRect.X = 84;

                if (__instance.PressedButtonTimer <= 0f)
                {
                    __instance.DestroyTimer = 100f;
                }
            }

            return false;
        }
    }
}
