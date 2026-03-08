using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Tools;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class MeleeWeaponPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/
        internal static IEnumerable<CodeInstruction> DoDamageTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo getListOfTileLocationsForBordersOfNonTileRectangleInfo = AccessTools.Method(typeof(Utility), nameof(Utility.getListOfTileLocationsForBordersOfNonTileRectangle));
                MethodInfo pickListOfTilesInfo = AccessTools.Method(typeof(MeleeWeaponPatch), nameof(pickListOfTiles));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Call, getListOfTileLocationsForBordersOfNonTileRectangleInfo)
                    )
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0)
                    )
                    .SetOperandAndAdvance(pickListOfTilesInfo)
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(DoDamageTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * METHODS *
         ***********/
        private static List<Vector2> pickListOfTiles(Rectangle area, Tool tool)
        {
            if (!tool.isScythe()
                || !tool.lastUser.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Reaper", out string value)
                || !bool.Parse(value))
            {
                return Utility.getListOfTileLocationsForBordersOfNonTileRectangle(area);
            }

            return Utilities.getListOfTileLocationsForNonTileRectangle(area);
        }

        internal static void getAreaOfEffectPostfix(MeleeWeapon __instance, int x, int y, int facingDirection, ref Vector2 tileLocation1, ref Vector2 tileLocation2, Rectangle wielderBoundingBox, int indexInCurrentAnimation, ref Rectangle __result)
        {
            if (!__instance.isScythe()
                || !__instance.lastUser.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Reaper", out string value)
                || !bool.Parse(value))
            {
                return;
            }

            Rectangle areaOfEffect = Rectangle.Empty;

            const int width = 192;
            const int height = 192;

            switch (facingDirection)
            {
                case 0:
                    areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - 12, width, height + 16);
                    tileLocation1 = new Vector2(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64, areaOfEffect.Top / 64);
                    tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Top / 64);
                    switch (indexInCurrentAnimation)
                    {
                        case 0:
                            areaOfEffect.Offset(-24 - width / 2, -12);
                            break;
                        case 1:
                            areaOfEffect.Offset(-16 - width / 2, -38);
                            areaOfEffect.Height += 32;
                            break;
                        case 2:
                            areaOfEffect.Offset(-12, -52);
                            areaOfEffect.Height += 48;
                            break;
                        case 3:
                            areaOfEffect.Offset(12, -51);
                            areaOfEffect.Height += 48;
                            break;
                        case 4:
                            areaOfEffect.Offset(16 + width / 2, -35);
                            areaOfEffect.Height += 32;
                            break;
                        case 5:
                            areaOfEffect.Offset(24 + width / 2, -8);
                            break;
                    }
                    break;
                case 2:
                    areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, height + 64);
                    tileLocation1 = new Vector2(Game1.random.Choose(areaOfEffect.Left, areaOfEffect.Right) / 64, areaOfEffect.Center.Y / 64);
                    tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
                    switch (indexInCurrentAnimation)
                    {
                        case 0:
                            areaOfEffect.Offset(24 + width / 2, -41);
                            break;
                        case 1:
                            areaOfEffect.Offset(16 + width / 2, -29);
                            areaOfEffect.Height += 32;
                            break;
                        case 2:
                            areaOfEffect.Offset(12, -30);
                            areaOfEffect.Height += 48;
                            break;
                        case 3:
                            areaOfEffect.Offset(-12, -31);
                            areaOfEffect.Height += 48;
                            break;
                        case 4:
                            areaOfEffect.Offset(-16 - width / 2, -31);
                            areaOfEffect.Height += 32;
                            break;
                        case 5:
                            areaOfEffect.Offset(-24 - width / 2, -44);
                            break;
                    }
                    break;
                case 1:
                    areaOfEffect = new Rectangle(wielderBoundingBox.Right + 4, y - height / 2 - 16, height + 32, width);
                    tileLocation1 = new Vector2(areaOfEffect.Center.X / 64, Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64);
                    tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
                    switch (indexInCurrentAnimation)
                    {
                        case 0:
                            areaOfEffect.Offset(-24, -24 - width / 2);
                            break;
                        case 1:
                            areaOfEffect.Offset(-12, -16 - width / 2);
                            areaOfEffect.Width += 32;
                            break;
                        case 2:
                            areaOfEffect.Offset(-13, -12);
                            areaOfEffect.Width += 48;
                            break;
                        case 3:
                            areaOfEffect.Offset(-14, 12);
                            areaOfEffect.Width += 48;
                            break;
                        case 4:
                            areaOfEffect.Offset(-14, 16 + width / 2);
                            areaOfEffect.Width += 32;
                            break;
                        case 5:
                            areaOfEffect.Offset(-27, 24 + width / 2);
                            break;
                    }
                    break;
                case 3:
                    areaOfEffect = new Rectangle(wielderBoundingBox.Left - height - 16, y - height / 2 - 16, height + 32, width);
                    tileLocation1 = new Vector2(areaOfEffect.Left / 64, Game1.random.Choose(areaOfEffect.Top, areaOfEffect.Bottom) / 64);
                    tileLocation2 = new Vector2(areaOfEffect.Left / 64, areaOfEffect.Center.Y / 64);
                    switch (indexInCurrentAnimation)
                    {
                        case 0:
                            areaOfEffect.Offset(4, -24 - width / 2);
                            break;
                        case 1:
                            areaOfEffect.Offset(-40, -16 - width / 2);
                            areaOfEffect.Width += 32;
                            break;
                        case 2:
                            areaOfEffect.Offset(-54, -12);
                            areaOfEffect.Width += 48;
                            break;
                        case 3:
                            areaOfEffect.Offset(-53, 12);
                            areaOfEffect.Width += 48;
                            break;
                        case 4:
                            areaOfEffect.Offset(-37, 16 + width / 2);
                            areaOfEffect.Width += 32;
                            break;
                        case 5:
                            areaOfEffect.Offset(8, 24 + width / 2);
                            break;
                    }
                    break;
            }

            areaOfEffect.Inflate(__instance.addedAreaOfEffect.Value, __instance.addedAreaOfEffect.Value);
            __result = areaOfEffect;
        }
    }
}
