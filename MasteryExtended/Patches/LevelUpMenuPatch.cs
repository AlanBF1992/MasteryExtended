using MasteryExtended.Menu.Pages;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MasteryExtended.Patches
{
    internal static class LevelUpMenuPatch
    {
        internal static bool AddMissedProfessionChoicesPrefix(Farmer farmer)
        {
            int[] skills = [0, 3, 2, 1, 4];

            foreach (int skill in skills)
            {
                if (farmer.GetUnmodifiedSkillLevel(skill) >= 5 && !farmer.newLevels.Contains(new Point(skill, 5)) && farmer.getProfessionForSkill(skill, 5) == -1)
                {
                    farmer.newLevels.Add(new Point(skill, 5));
                }
                if (farmer.GetUnmodifiedSkillLevel(skill) >= 10 && !farmer.newLevels.Contains(new Point(skill, 10)) && farmer.getProfessionForSkill(skill, 10) == -1 && MasterySkillsPage.skills.Find(s => s.Id == skill)!.unlockedProfessionsCount() <= 2)
                {
                    farmer.newLevels.Add(new Point(skill, 10));
                }
            }
            return false;
        }
    }
}
