using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using StardewValley.Menus;
using MasteryExtended.Menu.Pages;

namespace MasteryExtended.Patches
{
    internal static class GameLocationPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        // Ganar maestría antes de maxear todo, solo con 1 listo.
        internal static bool performActionPrefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (__instance.ShouldIgnoreAction(action, who, tileLocation))
                {
                    __result = false;
                    return false;
                }
                if (!ArgUtility.TryGet(action, 0, out var actionType, out var error))
                {
                    __result = false;
                    return false;
                }
                if (who.IsLocalPlayer)
                {
                    if (actionType == "MasteryRoom")
                    {
                        int masteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
                        int masteryRequired = ModEntry.Config.MasteryRequiredForCave;
                        int totalSkills = Game1.player.farmingLevel.Value / 10 + Game1.player.fishingLevel.Value / 10 + Game1.player.foragingLevel.Value / 10 + Game1.player.miningLevel.Value / 10 + Game1.player.combatLevel.Value / 10;
                        if (masteryLevel >= masteryRequired || totalSkills >= 5)
                        {
                            Game1.playSound("doorClose");
                            Game1.warpFarmer("MasteryCave", 7, 11, 0);
                        }
                        else
                        {
                            Game1.drawObjectDialogue(new List<string> {
                                Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", totalSkills).Replace(".",""),
                                $"or one that transcended mortal knowledge. ({masteryLevel}/{masteryRequired})"
                            });
                        }
                        __result = true;
                        return false;
                    }
                    if (actionType == "MasteryCave_Pedestal")
                    {
                        Game1.activeClickableMenu = new MasteryTrackerMenu();
                        __result = true;
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(performActionPrefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }

        // Modificar mapa
        internal static void MakeMapModificationsPostfix(GameLocation __instance)
        {
            if (__instance.Name == "MasteryCave")
            {
                for (int which = 0; which < 5; which++)
                {
                    bool enoughProfessions = MasterySkillsPage.skills.Find(s => s.Id == which)!.unlockedProfessions() >= 3;

                    Game1.stats.Get("MasteryExp");
                    bool freeLevel = MasteryTrackerMenu.getCurrentMasteryLevel() > (int)Game1.stats.Get("masteryLevelsSpent");

                    if (!enoughProfessions || !freeLevel)
                    {
                        Game1.currentLocation.removeTemporarySpritesWithID(8765 + which);
                    }
                }
            }
        }
    }
}
