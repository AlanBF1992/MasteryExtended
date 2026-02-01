using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
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
                resetProfessions);

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
                int maxBaseProfLvl = s.Professions.Max(p => p.LevelRequired) / 5;
                spentLevelsInProfessions += Math.Max(s.unlockedProfessionsCount() - Math.Min((int)Math.Floor(s.getLevel() / 5f), maxBaseProfLvl), 0);
            }

            Utilities.SetMasteryPillarsClaimed();

            int totalSpentLevels = (int)Game1.player.stats.Get("mastery_total_pillars") + spentLevelsInProfessions;

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
                int maxBaseProfLvl =  s.Professions.Max(p => p.LevelRequired)/5;
                spentLevelsInProfessions += Math.Max(s.unlockedProfessionsCount() - Math.Min((int)Math.Floor(s.getLevel() / 5f), maxBaseProfLvl), 0);
            }

            int totalSpentLevels = (int)Game1.player.stats.Get("mastery_total_pillars") + spentLevelsInProfessions;

            int expToSet = MasteryTrackerMenu.getMasteryExpNeededForLevel(totalSpentLevels);

            Game1.stats.Set("MasteryExp", expToSet);
        }

        internal static void resetProfessions(string _, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }
            if (args.Length > 1)
            {
                LogMonitor.Log("Incorrect number of arguments provided.", LogLevel.Error);
                return;
            }

            List<Skill> targetSkills = MasterySkillsPage.skills;
            if (args.Length > 0 && args[0].Length > 0)
            {
                targetSkills = targetSkills.Where(s => s.GetName().EqualsIgnoreCase(args[0])).ToList();
                if (targetSkills.Count == 0)
                {
                    LogMonitor.Log($"Skill '{args[0]}' not found.", LogLevel.Error);
                    return;
                }
            }

            targetSkills.ForEach(s => s.Professions.ForEach(p => p.RemoveProfessionFromPlayer()));

            foreach (Skill s in targetSkills)
            {
                for (int i = 1; i <= s.getLevel(); i++)
                {
                    if (s.ProfessionChooserLevels.Contains(i))
                    {
                        s.addNewLevel(i);
                    }
                }

                LogMonitor.Log($"Skill '{s.GetName()}' reset.", LogLevel.Info);
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
