using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class ProfessionPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> RemoveProfessionFromPlayerTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo removePrestigedinfo = AccessTools.Method(typeof(ProfessionPatch), nameof(removePrestiged));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Callvirt)
                    )
                    .Insert(
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Call, removePrestigedinfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(RemoveProfessionFromPlayerTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static void removePrestiged(int id)
        {
            if (id is >= 0 and < 30)
                Game1.player.professions.Remove(id + 100);
        }
    }
}
