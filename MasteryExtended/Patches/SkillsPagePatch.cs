using HarmonyLib;
using MasteryExtended.Menu.Pages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class SkillsPagePatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> ctorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                MethodInfo joinSkillLvlInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(joinSkillLvl));

                CodeMatcher matcher = new(instructions);

                // from: whichProfession.ToString() ?? ""
                // to:   "{skill},{lvl}", pa parsearlo luego y weá
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Dup)
                    )
                    .ThrowIfNotMatch("SkillsPage.ctorTranspiler: IL code 1 not found")
                    .Advance(2)
                    .RemoveInstructions(6)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_S, 8), // j: skill
                        new CodeInstruction(OpCodes.Ldloc_S, 7),  // i: lvl
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

                MethodInfo widthMultiplierInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(widthMultiplier));
                MethodInfo MaxMasteryLevelsInfo = AccessTools.PropertyGetter(typeof(ModEntry), nameof(ModEntry.MaxMasteryLevels));
                MethodInfo drawNumbersInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(drawNumbers));
                MethodInfo stringCompareInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(stringCompare));
                MethodInfo drawAllProfessionsInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(drawAllProfessions));

                // add: num12 = num12 * widthMultiplier()
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_R4, 0.1f)
                    )
                    .ThrowIfNotMatch("SkillsPage.drawTranspiler: IL code 1 not found")
                    .Advance(3)
                    .Set(OpCodes.Ldloc_S, 27)
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, widthMultiplierInfo),
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Stloc_S, 27),
                        new CodeInstruction(OpCodes.Ldarg_1)
                    );

                // from: masteryLevel >= 5
                // to:   masteryLevel >= ModEntry.MaxMasteryLevels
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_5)
                    )
                    .ThrowIfNotMatch("SkillsPage.drawTranspiler: IL code 2 not found")
                    .Set(OpCodes.Call, MaxMasteryLevelsInfo)
                ;

                // from: NumberSprite (2 times)
                // add:  drawNumbers(this, b)
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Ldloc_S)
                    )
                    .ThrowIfNotMatch("SkillsPage.drawTranspiler: IL code 3 not found")
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
                    .ThrowIfNotMatch("SkillsPage.drawTranspiler: IL code 4 not found")
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

                // add: draw all professions in the side
                matcher
                    .End()
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
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldloc_3),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("SkillsPage.performHoverActionTranspiler: IL code 2 not found")
                    .RemoveInstructions(2)
                ;

                // from: this.professionImage = blabla
                // to:   this.professionImage = -1
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldloc_3),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Call)
                    )
                    .ThrowIfNotMatch("SkillsPage.performHoverActionTranspiler: IL code 3 not found")
                    .Advance(1)
                    .RemoveInstructions(3)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldc_I4_M1)
                    )
                ;

                // from: skillBar.scale = 0f;
                // to:   skillBar.scale = 4.2f;
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_3),
                        new CodeMatch(OpCodes.Ldc_R4),
                        new CodeMatch(OpCodes.Stfld)
                    )
                    .ThrowIfNotMatch("SkillsPage.performHoverActionTranspiler: IL code 4 not found")
                    .Advance(1)
                    .SetOperandAndAdvance(4.2f)
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(performHoverActionTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static bool stringCompare(string str1, string str2)
        {
            return str1.Trim().Equals(str2.Trim());
        }

        internal static string joinSkillLvl(int skill, int lvl)
        {
            var skillId = skill switch {
                0 => 0,
                1 => 3,
                2 => 2,
                3 => 1,
                4 => 4,
                _ => 0
            };
            return skillId + "," + lvl;
        }

        internal static float widthMultiplier()
        {
            int currentMasteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
            int masterySpent = (int)Game1.stats.Get("masteryLevelsSpent");

            int digitsToShow = currentMasteryLevel.countDigits() + masterySpent.countDigits();

            return 0.85f - (digitsToShow - 2) * 0.0625f;
        }

        internal static void drawNumbers(IClickableMenu page, SpriteBatch b)
        {
            int masteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
            float masteryStringWidth = Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\1_6_Strings:Mastery").TrimEnd(':')).X;
            int masterySpent = (int)Game1.stats.Get("masteryLevelsSpent");

            int xOffset = (int)masteryStringWidth - 64;
            const int yOffset = 508;

            float width = 0.64f;
            width -= (masteryStringWidth - 100f) / 800f;
            if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru)
            {
                width += 0.1f;
            }

            // Dibujar los números (1ro sombra, 2do texto)
            // Primero el del lado derecho
            NumberSprite.draw(masteryLevel, b,
                              new Vector2(xOffset + page.xPositionOnScreen + 408 + (int)(584f * width), yOffset + page.yPositionOnScreen + 4),
                              Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
            NumberSprite.draw(masteryLevel, b,
                              new Vector2(xOffset + page.xPositionOnScreen + 412 + (int)(584f * width), yOffset + page.yPositionOnScreen),
                              (masteryLevel >= ModEntry.MaxMasteryLevels ? new(70, 210, 90) : (masterySpent == masteryLevel ? Color.OrangeRed : Color.SandyBrown)) * ((masteryLevel == 0) ? 0.75f : 1f),
                              1f, 0.87f, 1f, 0);

            xOffset += masteryLevel < 10 ? 28 : (masteryLevel < 100? 0 : -24);

            // Luego el separador
            b.Draw(Game1.mouseCursors,
                new Vector2(xOffset + page.xPositionOnScreen + 352 + (int)(584f * width), yOffset + page.yPositionOnScreen + 4),
                new Rectangle(544, 136, 8, 8),
                Color.Black * 0.35f, 0f, new Vector2(4f, 4f), 4f * 1f, SpriteEffects.None, 0.85f);
            b.Draw(Game1.mouseCursors,
                new Vector2(xOffset + page.xPositionOnScreen + 356 + (int)(584f * width), yOffset + page.yPositionOnScreen),
                new Rectangle(544, 136, 8, 8),
                (masteryLevel >= ModEntry.MaxMasteryLevels ? new(70, 210, 90) : (masterySpent == masteryLevel ? Color.OrangeRed : Color.SandyBrown)) * 1f,
                0f, new Vector2(4f, 4f), 4f * 1f, SpriteEffects.None, 0.85f);

            // Luego el del lado izquierdo
            NumberSprite.draw(masterySpent, b, new Vector2(xOffset + page.xPositionOnScreen + 329 + (int)(584f * width), yOffset + page.yPositionOnScreen + 4), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
            NumberSprite.draw(masterySpent, b, new Vector2(xOffset + page.xPositionOnScreen + 333 + (int)(584f * width), yOffset + page.yPositionOnScreen),
                (masterySpent >= ModEntry.MaxMasteryLevels ? new(70, 210, 90) : (masterySpent == masteryLevel ? Color.OrangeRed : Color.SandyBrown)) * ((masteryLevel == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0);
        }

        internal static void drawAllProfessions(object instance, SpriteBatch b)
        {
            string hoverTitle = (string)instance.GetInstanceField("hoverTitle")!;
            string hoverText = ((string)instance.GetInstanceField("hoverText")!);

            if (!hoverText.Trim().Equals("MasteryExtended")) return;

            var parsed = hoverTitle.Split(",").Select(int.Parse).ToList();
            var skillId = parsed[0];
            var lvlRequired = parsed[1];

            var skill = MasterySkillsPage.skills.Find(s => s.Id == skillId)!;

            const int xSpacing = 8;
            const int ySpacing = 8;

            var yPosition = 0;
            var xPosition = 0;

            if(ModEntry.Config.SkillNameOnMenuHover)
            {
                IClickableMenu.drawHoverText(b, skill.GetName(), Game1.smallFont, overrideX: xSpacing, overrideY: ySpacing, boxScale: 1f);
                yPosition += 72;
            }

            var unlockedProfessions = skill.unlockedProfessions().Where(p => p.LevelRequired == lvlRequired).OrderByDescending(p => p.GetDescription().Length).ToArray();

            for (int i = unlockedProfessions.Length - 1; i >= 0; i--)
            {
                var prof = unlockedProfessions[i];

                // Box and Icon
                IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPosition + xSpacing, yPosition + ySpacing, 66, 66, Color.White, 0.75f);
                b.Draw(prof.TextureSource(), new Rectangle(xPosition + xSpacing + 12, yPosition + ySpacing + 12, 66 - 24, 66 - 24), prof.TextureBounds, Color.White);

                // Description
                var descText = Game1.parseText(prof.GetDescription(), Game1.smallFont, 460);
                var descSize = Game1.smallFont.MeasureString(descText);

                IClickableMenu.drawHoverText(b, descText, Game1.smallFont, overrideX: xPosition + xSpacing + 72, overrideY: yPosition + ySpacing, boxScale: 0.75f);

                yPosition += (int)descSize.Y + 32 + 8;
                var nextHeight = i == 0 ? 0: Game1.smallFont.MeasureString(Game1.parseText(unlockedProfessions[i - 1].GetDescription(), Game1.smallFont, 460)).Y + 32;

                if (yPosition + nextHeight + 8 >= Game1.uiViewport.Height)
                {
                    xPosition += 600;

                    foreach(var prof2 in unlockedProfessions[0..i])
                    {
                        var descSize2 = (int)Game1.smallFont.MeasureString(Game1.parseText(prof2.GetDescription(), Game1.smallFont, 460)).Y;

                        yPosition -= descSize2 + 32 + 8;
                    }
                }
            }
        }
    }
}
