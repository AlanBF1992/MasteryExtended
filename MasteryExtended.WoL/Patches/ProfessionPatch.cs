using DaLion.Professions;
using HarmonyLib;
using Sickhead.Engine.Util;
using StardewModdingAPI;
using System.Reflection;
using Profession = MasteryExtended.Skills.Professions.Profession;

namespace MasteryExtended.WoL.Patches
{
    internal static class ProfessionPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static void AddProfessionToPlayerPostfix(Profession __instance)
        {
            var State = ModEntry.ModHelper.Reflection.GetMethod(typeof(ProfessionsMod), "get_State").Invoke<object>();
            var OrderedProfessions = (List<int>)State.GetInstanceField("_orderedProfessions")!;

            OrderedProfessions.Add(__instance.Id);
            State.SetInstanceField("_orderedProfessions", OrderedProfessions);
        }
    }
}
