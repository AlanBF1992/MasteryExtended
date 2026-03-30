
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Tools;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class ToolPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/
        internal static IEnumerable<CodeInstruction> CanForgeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo itemLevelInfo = AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.getItemLevel));
                MethodInfo canBeForgedInfo = AccessTools.Method(typeof(ToolPatch), nameof(canBeForged));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Callvirt, itemLevelInfo)
                    )
                    .ThrowIfNotMatch("ToolPatch.CanForgeTranspiler: IL code not found")
                    .Advance(-1)
                    .RemoveInstructions(8)
                    .Opcode = OpCodes.Brfalse_S
                ;

                matcher
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, canBeForgedInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(CanForgeTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * METHODS *
         ***********/
        internal static bool canBeForged(MeleeWeapon weapon)
        {
            if (MeleeWeaponPatch.isFarmerRunesmith(weapon.lastUser))
            {
                return true;
            }
            return weapon.getItemLevel() < 15 && !weapon.Name.Contains("Galaxy");
        }
    }
}
