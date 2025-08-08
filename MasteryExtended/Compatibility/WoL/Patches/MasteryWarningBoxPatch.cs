using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class MasteryWarningBoxPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> ctorTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo masteryOldWarningInfo = AccessTools.Method("DaLion.Professions.I18n:Prestige_Mastery_Warning");
                MethodInfo masteryNewWarningInfo = AccessTools.Method(typeof(MasteryWarningBoxPatch), nameof(newWarning));
                MethodInfo newWidthInfo = AccessTools.Method(typeof(MasteryWarningBoxPatch), nameof(newWidth));
                FieldInfo widthFieldInfo = AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.width));
                CodeInstruction setWidthFieldInstruction = new(OpCodes.Stfld, widthFieldInfo);

                // from: I18n.Prestige_Mastery_Lock()
                // to:   newWarning()
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Call, masteryOldWarningInfo)
                    )
                    .ThrowIfNotMatch("WoL ctorTranspiler: IL Code 1 not found")
                    .Operand = masteryNewWarningInfo
                ;

                // from: 1200
                // to:   newWidth()

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4, 1200)
                    )
                    .ThrowIfNotMatch("WoL ctorTranspiler: IL Code 2 not found")
                ;

                matcher.Opcode = OpCodes.Call;
                matcher.Operand = newWidthInfo;

                // delete everything after: location.afterQuestion = this.AfterQuestionBehavior;
                // and before:              this.exitFunction = () => State.WarningBox = null;
                matcher
                    .MatchStartForward(new CodeMatch(OpCodes.Newobj))
                    .ThrowIfNotMatch("WoL ctorTranspiler: IL Code 3 not found")
                    .Advance(2)
                ;

                while (matcher.Opcode != setWidthFieldInstruction.opcode || matcher.Operand != setWidthFieldInstruction.operand)
                {
                    matcher.RemoveInstruction();
                }
                matcher.RemoveInstruction();

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(ctorTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static IEnumerable<CodeInstruction> drawTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo difHeightInfo = AccessTools.Method(typeof(MasteryWarningBoxPatch), nameof(difHeight));
                //MethodInfo spaceTwoInfo = AccessTools.Method(typeof(MasteryWarningBoxPatch), nameof(spaceTwo));

                // from: this.y - (this.heightForQuestions - this.height)
                // to:   this.y - (this.heightForQuestions - this.height) + difHeight(this)
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Sub),
                        new CodeMatch(OpCodes.Sub)
                    )
                    .ThrowIfNotMatch("WoL drawTranspiler: IL Code 1 not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, difHeightInfo),
                        new CodeInstruction(OpCodes.Add)
                    )
                ;

                // from: this.heightForQuestions + 8
                // to:   this.heightForQuestions + 8 - difHeight() + 8
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Ldc_I4_8),
                        new CodeMatch(OpCodes.Add)
                    )
                    .ThrowIfNotMatch("WoL drawTranspiler: IL Code 2 not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, difHeightInfo),
                        new CodeInstruction(OpCodes.Sub),
                        new CodeInstruction(OpCodes.Ldc_I4_8),
                        new CodeInstruction(OpCodes.Add)
                    )
                ;

                // from: this.y - (this.heightForQuestions - this.height) (another one)
                // to:   this.y - (this.heightForQuestions - this.height) + difHeight()
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Sub),
                        new CodeMatch(OpCodes.Sub)
                    )
                    .ThrowIfNotMatch("WoL drawTranspiler: IL Code 3 not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, difHeightInfo),
                        new CodeInstruction(OpCodes.Add)
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

        internal static string newWarning()
        {
            return Game1.content.LoadString("Strings\\UI:MasteryExtended_WoLMasteryWarning");
        }

        internal static int newWidth()
        {
            var text = Game1.parseText(newWarning(), Game1.dialogueFont, 1000);
            var textWidth = (int)Game1.dialogueFont.MeasureString(text).X;
            return textWidth + 32;
        }

        internal static int difHeight()
        {
            var text = Game1.parseText(newWarning(), Game1.dialogueFont, 1000);
            var baseHeight = SpriteText.getHeightOfString(newWarning(), newWidth() - 16);
            var realHeight = Game1.dialogueFont.MeasureString(text).Y;
            return baseHeight - (int)realHeight;
        }
    }
}
