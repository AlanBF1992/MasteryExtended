using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended
{
    internal static class ModCommands
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /// <summary>Add commands to debug</summary>
        internal static void addConsoleCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add(
                "masteryExtended_RecountUsed",
                "Recounts the player's Mastery Level Used.",
                (_, _) => recountUsedMasteryLevels());

            helper.ConsoleCommands.Add(
                "masteryExtended_ResetExp",
                "Sets the Mastery Exp to the minimum possible.",
                (_, _) => resetMasteryExp());

            helper.ConsoleCommands.Add(
                "masteryExtended_ResetProfessions",
                "Reset Vanilla Professions when you sleep.",
                (_, _) => resetProfessions());

            helper.ConsoleCommands.Add(
                "masteryExtended_AddMasteryLevel",
                "Add Mastery Levels. <value> is an optional argument.",
                addMasteryLevel);
        }

        internal static void recountUsedMasteryLevels()
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            int spentLevelsInProfessions = 0;

            foreach (Skill s in MasterySkillsPage.skills)
            {
                spentLevelsInProfessions += Math.Max(s.unlockedProfessionsCount() - Math.Min((int)Math.Floor(s.getLevel() / 5f), 2), 0);
            }

            ModEntry.Data.claimedRewards = Utilities.countClaimedPillars();
            int totalSpentLevels = Utilities.countClaimedPillars() + spentLevelsInProfessions;

            Game1.stats.Set("masteryLevelsSpent", totalSpentLevels);
        }

        internal static void resetMasteryExp()
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }
            int spentLevelsInProfessions = 0;

            foreach (Skill s in MasterySkillsPage.skills)
            {
                spentLevelsInProfessions += Math.Max(s.unlockedProfessionsCount() - Math.Min((int)Math.Floor(s.getLevel() / 5f), 2), 0);
            }

            int totalSpentLevels = Utilities.countClaimedPillars() + spentLevelsInProfessions;

            int expToSet = MasteryTrackerMenu.getMasteryExpNeededForLevel(totalSpentLevels);

            Game1.stats.Set("MasteryExp", expToSet);
        }

        internal static void resetProfessions()
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            var allSkills = MasterySkillsPage.skills;//.FindAll(s => s.Id is >= 0 and <= 4);

            allSkills.ForEach(s => s.Professions.ForEach(p => p.RemoveProfessionFromPlayer()));

            foreach (Skill s in allSkills)
            {
                int points = s.getLevel() / 5;

                for (int i = 1; i <= points; i++)
                {
                    s.addNewLevel(5*i);
                }
            }
            recountUsedMasteryLevels();
        }

        internal static void addMasteryLevel(string _, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }
            int levels = 1;
            if (args.Length > 0 && !int.TryParse(args[0], out levels))
            {
                LogMonitor.Log("Parameter should be an integer", LogLevel.Error);
                return;
            }
            int currentLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
            int spentLevels = (int)Game1.stats.Get("masteryLevelsSpent");
            int newLevel = Utility.Clamp(spentLevels, currentLevel + levels, ModEntry.MaxMasteryLevels);
            int expToSet = MasteryTrackerMenu.getMasteryExpNeededForLevel(newLevel);
            Game1.stats.Set("MasteryExp", expToSet);
        }
    }
}
