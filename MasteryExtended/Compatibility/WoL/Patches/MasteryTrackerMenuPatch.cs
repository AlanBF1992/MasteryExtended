using HarmonyLib;
using MasteryExtended.Skills;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class MasteryTrackerMenuPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /// <summary>Re adds the Claim button to the Combat Mastery Pillar</summary>
        internal static void ctorPostfix(MasteryTrackerMenu __instance, int whichSkill = -1)
        {
            if (whichSkill != Skill.Combat.Id) return;
            if (__instance.mainButton is null)
            {
                __instance.SetInstanceField("canClaim", false);
                __instance.mainButton = new ClickableTextureComponent(new Rectangle(__instance.xPositionOnScreen + __instance.width / 2 - 84, __instance.yPositionOnScreen + __instance.height - 112, 168, 80), Game1.mouseCursors_1_6, new Rectangle(0, 123, 42, 21), 4f)
                {
                    visible = true,
                    myID = 0,
                    myAlternateID = 99
                };
            }
        }

        internal static void drawPrefix(MasteryTrackerMenu __instance, ref ClickableTextureComponent __state)
        {
            __state = __instance.mainButton;
            if (__instance.mainButton?.myAlternateID == 99) __instance.mainButton = null;
        }

        internal static void drawPostfix(MasteryTrackerMenu __instance, SpriteBatch b, ClickableTextureComponent __state)
        {
            if ((int)__instance.GetInstanceField("which")! != Skill.Combat.Id) return;

            __instance.mainButton = __state;

            if (__instance.mainButton?.myAlternateID == 99)
            {
                __instance.mainButton.draw(b, Color.White, 0.88f);
                const string text = "Change";
                Utility.drawTextWithColoredShadow(b, text, Game1.dialogueFont, __instance.mainButton.getVector2() + new Vector2(__instance.mainButton.bounds.Width / 2 - Game1.dialogueFont.MeasureString(text).X / 2f, 6f + (__instance.mainButton.sourceRect.X == 84 ? 8 : 0)), Color.Black, Color.Black * 0.2f, 1f, 0.9f);
            }

            __instance.drawMouse(b);
        }

        internal static bool receiveLeftClickPrefix(MasteryTrackerMenu __instance, int x, int y)
        {
            if (__instance.mainButton == null || __instance.mainButton.myAlternateID != 99) return true;

            if (__instance.mainButton.containsPoint(x, y))
            {
                Type MasteryLimitSelectionPage = AccessTools.TypeByName("DaLion.Professions.Framework.UI.MasteryLimitSelectionPage");
                Game1.activeClickableMenu = Activator.CreateInstance(MasteryLimitSelectionPage) as IClickableMenu;
            }

            return false;
        }

        internal static void performHoverActionPostfix(MasteryTrackerMenu __instance, int x, int y)
        {
            if ((int)__instance.GetInstanceField("which")! != Skill.Combat.Id) return;
            if (__instance.mainButton == null || __instance.mainButton.myAlternateID != 99) return;

            if (__instance.mainButton.containsPoint(x, y))
            {
                __instance.mainButton.sourceRect.X = 42;
            }
            else
            {
                __instance.mainButton.sourceRect.X = 0;
            }
        }
    }
}
