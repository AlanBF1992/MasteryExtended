using HarmonyLib;
using MasteryExtended.Menu.Pages;
using StardewModdingAPI;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.VPP.Patches
{
    internal static class ModEntryPatcher
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> OnButtonPressedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                FieldInfo ccName = AccessTools.Field(typeof(StardewValley.Menus.ClickableComponent), nameof(StardewValley.Menus.ClickableComponent.name));
                ConstructorInfo indexAndProfessionConstructor = typeof(List<(int, string)>).GetConstructor(Type.EmptyTypes)!;
                MethodInfo joinSkillLvlVPPInfo = AccessTools.Method(typeof(ModEntryPatcher), nameof(joinSkillLvlVPP));
                MethodInfo indexAndProfessionChangerInfo = AccessTools.Method(typeof(ModEntryPatcher), nameof(indexAndProfessionChanger));

                // Get IndexAndProfessions
                var IndexAndProfessions = matcher.MatchStartForward(new CodeMatch(OpCodes.Newobj, indexAndProfessionConstructor)).Advance(1).Operand;

                // from: nothing
                // to:   readThisThing(IndexAndProfessions)
                matcher.
                    End()
                    .MatchStartBackwards(new CodeMatch(OpCodes.Ldstr, "-1"))
                    .MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_0))
                    .ThrowIfNotMatch("ModEntryPatcher.OnButtonPressedTranspiler: IL code 0 not found")
                ;

                matcher
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_S, IndexAndProfessions),
                        new CodeInstruction(OpCodes.Call, indexAndProfessionChangerInfo),
                        new CodeInstruction(OpCodes.Stloc_S, IndexAndProfessions)
                    )
                ;

                // from: DisplayHandler.MyCustomSkillBars.Value[IndexAndProfessions[index].Item1].name = IndexAndProfessions[index].Item2;
                // to:   DisplayHandler.MyCustomSkillBars.Value[IndexAndProfessions[index].Item1].name = renamer2000(IndexAndProfessions[index].Item2);
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Stfld, ccName),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldloc_S)
                    )
                    .ThrowIfNotMatch("ModEntryPatcher.OnButtonPressedTranspiler: IL code 1 not found")
                    .Insert(
                        new CodeInstruction(OpCodes.Call, joinSkillLvlVPPInfo)
                    );

                // from: description
                // to:   "MasteryExtended"
                matcher
                    .Advance(2)
                    .MatchStartForward(new CodeMatch(OpCodes.Stfld))
                    .ThrowIfNotMatch("ModEntryPatcher.OnButtonPressedTranspiler: IL code 2 not found")
                    .Insert(
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldstr, "MasteryExtended")
                    );

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(OnButtonPressedTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        private static List<(int, string)> indexAndProfessionChanger(List<(int,string)> IndexAndProfessions)
        {
            List<(int, string)> newIP = [];
            
            foreach (var item in IndexAndProfessions)
            {
                var profVPP = int.Parse(item.Item2);
                var lvlToCheck = item.Item1 < 5 ? 15 : 20;
                var skill = MasterySkillsPage.skills.First(s => s.containsProfession(profVPP));
                var prof = skill.Professions.First(p => p.LevelRequired == lvlToCheck && p.IsProfessionUnlocked()); // Da lo mismo en realidad
                newIP.Add((item.Item1, prof.Id.ToString()));
            }

            return newIP;
        }

        internal static string joinSkillLvlVPP(string name)
        {
            if (int.TryParse(name, out int value))
            {
                var skill = MasterySkillsPage.skills.First(s => s.containsProfession(value));
                var levelRequired = skill.Professions.Find(p => p.Id == value)!.LevelRequired;
                return skill.Id + "," + levelRequired;
            }

            return name;
        }
    }
}
