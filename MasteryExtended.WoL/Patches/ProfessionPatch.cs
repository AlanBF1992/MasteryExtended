using MasteryExtended.Menu.Pages;
using StardewModdingAPI;
using StardewValley;
using System;
using DaLion.Shared.Data;
using DaLion.Professions;
using StardewValley.Menus;
using MasteryExtended.Skills.Professions;
using DaLion.Shared.Extensions;
using System.Collections.Generic;

namespace MasteryExtended.WoL.Patches
{
    internal static class ProfessionPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static void AddProfessionToPlayerPostfix(Profession __instance)
        {
            ModEntry.ProfessionsToSave.Add(__instance.Id);
        }
    }
}
