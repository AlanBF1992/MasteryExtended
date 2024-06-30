using DaLion.Professions;
using DaLion.Shared.Data;
using StardewValley;

namespace MasteryExtended.WoL.Patches
{
    internal static class ProfessionSavingEventPatch
    {
        internal static void OnSavingImplPostfix()
        {
            Farmer player = Game1.player;
            ModDataManager Data = ModEntry.ModHelper.Reflection.GetMethod(typeof(ProfessionsMod), "get_Data").Invoke<ModDataManager>();
            List<int> storedProfessions = Data.Read(player, "OrderedProfessions").Split(',').Select(int.Parse).ToList();
            storedProfessions.AddRange(ModEntry.ProfessionsToSave);
            storedProfessions.Sort();
            Data.Write(player, "OrderedProfessions", string.Join(',', storedProfessions));
        }
    }
}
