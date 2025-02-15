using HarmonyLib;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Patches;
using MasteryExtended.Skills;
using StardewModdingAPI;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.SpaceCore.Patches
{
    internal static class SCNewSkillsPagePatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> ctorTranspiler (IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                MethodInfo joinSkillLvlInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(SkillsPagePatch.joinSkillLvl));
                MethodInfo joinCustomSkillLvlInfo = AccessTools.Method(typeof(SCNewSkillsPagePatch), nameof(joinCustomSkillLvl));

                CodeMatcher matcher = new(instructions);

                // VANILLA SKILLS
                // from: string.Concat(whichProfession)
                // to:   "{skill},{lvl}", pa parsearlo luego y weá
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Box),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("SkillsPage.ctorTranspiler: IL code 1 not found")
                    .RemoveInstructions(3)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_S, 7),  // skill
                        new CodeInstruction(OpCodes.Ldloc_S, 9),  // lvl
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Call, joinSkillLvlInfo)
                    )
                ;

                // from: professionBlurb
                // to:   "MasteryExtended"
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Ldnull),
                        new CodeMatch(OpCodes.Ldloc_S)
                    )
                    .ThrowIfNotMatch("SkillsPage.ctorTranspiler: IL code 2 not found")
                    .Set(OpCodes.Ldstr, "MasteryExtended")
                ;

                // CUSTOM SKILLS
                // from: if (drawRed && (professionLevel + 1) % 5 == 0 && profession != null)
                // to:   if (drawRed && (professionLevel + 1) % 5 == 0)
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Brfalse),
                        new CodeMatch(OpCodes.Ldarg_0)
                    )
                    .ThrowIfNotMatch("SkillsPage.ctorTranspiler: IL code 3 not found")
                    .RemoveInstructions(2)
                ;

                // from: NewSkillsPage.CustomSkillPrefix + profession.Id
                // to:   "{skill},{lvl}", pa parsearlo luego y weá
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldstr),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("SkillsPage.ctorTranspiler: IL code 4 not found")
                    .RemoveInstructions(4)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_S, 18), // j: skill
                        new CodeInstruction(OpCodes.Ldloc_S, 17),  // i: lvl
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Call, joinCustomSkillLvlInfo)
                    )
                ;

                // from: professionBlurb
                // to:   "MasteryExtended"
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Ldnull),
                        new CodeMatch(OpCodes.Ldloc_S)
                    )
                    .ThrowIfNotMatch("SkillsPage.ctorTranspiler: IL code 5 not found")
                    .Set(OpCodes.Ldstr, "MasteryExtended")
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(ctorTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static IEnumerable<CodeInstruction> drawTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo widthMultiplierInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(SkillsPagePatch.widthMultiplier));
                MethodInfo MaxMasteryLevelsInfo = AccessTools.PropertyGetter(typeof(ModEntry), nameof(ModEntry.MaxMasteryLevels));
                MethodInfo drawNumbersInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(SkillsPagePatch.drawNumbers));
                MethodInfo stringCompareInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(SkillsPagePatch.stringCompare));
                MethodInfo drawAllProfessionsInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(SkillsPagePatch.drawAllProfessions));

                // add: width = width * widthMultiplier()
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_R4, 0.1f)
                    )
                    .ThrowIfNotMatch("NewSkillsPage.drawTranspiler: IL code 1 not found")
                    .Advance(3)
                    .Set(OpCodes.Ldloc_S, 63)
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, widthMultiplierInfo),
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Stloc_S, 63),
                        new CodeInstruction(OpCodes.Ldarg_1)
                    );

                // from: masteryLevel >= 5
                // to:   masteryLevel >= ModEntry.MaxMasteryLevels
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_5)
                    )
                    .ThrowIfNotMatch("NewSkillsPage.drawTranspiler: IL code 2 not found")
                    .Set(OpCodes.Call, MaxMasteryLevelsInfo)
                ;

                // delete: NumberSprite (2 times)
                // add:    drawNumbers(this, b)
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Ldloc_S)
                    )
                    .ThrowIfNotMatch("NewSkillsPage.drawTranspiler: IL code 3 not found")
                    .RemoveInstructions(66)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, drawNumbersInfo)
                    )
                ;

                // from: if (this.hoverText.Length > 0)
                // to:   if (this.hoverText.Length > 0 && !this.hoverText.Equals("MasteryExtended"))
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ldc_I4_0),
                        new CodeMatch(OpCodes.Ble_S)
                    )
                    .ThrowIfNotMatch("NewSkillsPage.drawTranspiler: IL code 4 not found")
                    .Advance(1)
                ;

                CodeInstruction hoverInstruction = matcher.Instruction;

                matcher.Advance(3);

                Label lbl = (Label)matcher.Operand;

                matcher
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        hoverInstruction,
                        new CodeInstruction(OpCodes.Ldstr, "MasteryExtended"),
                        new CodeInstruction(OpCodes.Call, stringCompareInfo),
                        new CodeInstruction(OpCodes.Brtrue_S, lbl)
                    )
                ;

                // from: code repeated for some reason
                // to:   oblivion
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ldc_I4_0),
                        new CodeMatch(OpCodes.Ble_S)
                    )
                    .ThrowIfNotMatch("NewSkillsPage.drawTranspiler: IL code 5 not found. Hopefully deleted by SpaceCore")
                ;

                List<Label> repeatedCodeLbl = matcher.Labels;

                matcher.RemoveInstructions(46);

                // add: draw all professions in the side
                matcher
                    .End()
                    .AddLabels(repeatedCodeLbl)
                    .SetOpcodeAndAdvance(OpCodes.Ldarg_0) //this
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_1), // b
                        new CodeInstruction(OpCodes.Call, drawAllProfessionsInfo),
                        new CodeInstruction(OpCodes.Ret)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(drawTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static IEnumerable<CodeInstruction> performHoverActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                // from: this.hoverTitle = blabla
                // to:   this.hoverTitle = skillBar.name
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldloc_1),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldstr),
                        new CodeMatch(OpCodes.Callvirt)
                    )
                    .ThrowIfNotMatch("NewSkillsPage.performHoverActionTranspiler: IL code 2 not found")
                    .Advance(3)
                    .RemoveInstructions(12)
                ;

                // from: this.professionImage = blabla
                // to:   this.professionImage = -1
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldloc_1),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldstr),
                        new CodeMatch(OpCodes.Callvirt)
                    )
                    .ThrowIfNotMatch("NewSkillsPage.performHoverActionTranspiler: IL code 3 not found")
                    .Advance(1)
                    .RemoveInstructions(10)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldc_I4_M1)
                    )
                ;

                // from: skillBar.scale = 0f;
                // to:   skillBar.scale = 4.2f;
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_1),
                        new CodeMatch(OpCodes.Ldc_R4),
                        new CodeMatch(OpCodes.Stfld)
                    )
                    .ThrowIfNotMatch("NewSkillsPage.performHoverActionTranspiler: IL code 4 not found")
                    .Advance(1)
                    .SetOperandAndAdvance(4.2f)
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(drawTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        private static string joinCustomSkillLvl(object skill, int lvl)
        {
            var metodoSC = AccessTools.Method("SpaceCore.Skills+Skill:GetName");
            string name = (string)metodoSC.Invoke(skill, null)!;
            Skill sk = MasterySkillsPage.skills.Find(s => s.GetName() == name)!;
            return sk.Id + "," + lvl;
        }
    }
}
