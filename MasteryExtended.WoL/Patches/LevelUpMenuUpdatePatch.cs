using StardewValley;
using DaLion.Professions.Framework;
using DaLion.Shared.Extensions;
using DaLion.Shared.Extensions.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using System.Reflection;
using DaLion.Shared.Extensions.Stardew;
using StardewModdingAPI;
using HarmonyLib;

namespace MasteryExtended.WoL.Patches
{
    internal static class LevelUpMenuUpdatePatch
    {
        internal static bool LevelUpMenuUpdatePrefix(
            LevelUpMenu __instance,
            List<int> ___professionsToChoose,
            TemporaryAnimatedSpriteList ___littleStars,
            List<CraftingRecipe> ___newCraftingRecipes,
            int ___currentLevel,
            int ___currentSkill,
            ref bool ___isProfessionChooser,
            ref bool ___hasUpdatedProfessions,
            ref int ___timerBeforeStart,
            ref string ___title,
            ref List<string> ___extraInfoForLevel,
            ref List<string> ___leftProfessionDescription,
            ref List<string> ___rightProfessionDescription,
            ref MouseState ___oldMouseState,
            ref Rectangle ___sourceRectForLevelIcon,
            GameTime time)
        {
            if (___currentSkill >= Farmer.luckSkill || !__instance.isProfessionChooser)
            {
                return true; // run original logic
            }

            if (__instance.hasUpdatedProfessions && ___professionsToChoose.Count == 2 &&
                ShouldSuppressClick(___professionsToChoose[0], ___currentLevel) &&
                ShouldSuppressClick(___professionsToChoose[1], ___currentLevel))
            {
                __instance.isActive = false;
                __instance.informationUp = false;
                __instance.isProfessionChooser = false;
                return true; // run original logic
            }

            if (!__instance.isActive || ___currentLevel <= 10)
            {
                return true; // run original logic
            }

            try
            {
                var player = Game1.player;
                if (!___hasUpdatedProfessions)
                {
                    ISkill skill = VanillaSkill.FromValue(___currentSkill);
                    switch (___currentLevel)
                    {
                        case 15:

                            ___professionsToChoose.AddRange(skill.TierOneProfessions.Where(p => p.GetBranchingProfessions.Any(x => player.professions.Contains(x.Id))).Select(x => x.Id));
                            break;
                        case 20:
                            //Debería haber un y solo un prestige en este punto
                            int rootId = skill.TierOneProfessions.Where(p => player.professions.Contains(p.Id + 100)).Select(x => x.Id).First();

                            IProfession root = VanillaProfession.FromValue(rootId);
                            ___professionsToChoose.AddRange(root.GetBranchingProfessions
                                .Select(p => p.Id)
                                .Where(player.professions.Contains));
                            break;
                    }

                    if (___professionsToChoose.Count == 0)
                    {
                        return true; // run original logic
                    }

                    ___leftProfessionDescription = LevelUpMenu.getProfessionDescription(___professionsToChoose[0]);
                    if (___professionsToChoose.Count > 1)
                    {
                        ___rightProfessionDescription = LevelUpMenu.getProfessionDescription(___professionsToChoose[1]);
                    }

                    ___hasUpdatedProfessions = true;
                }

                if (___professionsToChoose.Count != 1)
                {
                    return true; // run original logic
                }

                #region choose single profession

                for (var i = ___littleStars.Count - 1; i >= 0; i--)
                {
                    if (___littleStars[i].update(time))
                    {
                        ___littleStars.RemoveAt(i);
                    }
                }

                var xPositionOnScreen = __instance.xPositionOnScreen;
                var width = __instance.width;
                if (Game1.random.NextBool(0.03))
                {
                    var position =
                        new Vector2(
                            0f,
                            // ReSharper disable once PossibleLossOfFraction
                            (Game1.random.Next(__instance.yPositionOnScreen - 128, __instance.yPositionOnScreen - 4) / 20 *
                             4 *
                             5) + 32)
                        {
                            X = Game1.random.NextBool()
                                ? Game1.random.Next(
                                    xPositionOnScreen + (width / 2) - 228,
                                    xPositionOnScreen + (width / 2) - 132)
                                : Game1.random.Next(
                                    xPositionOnScreen + (width / 2) + 116,
                                    xPositionOnScreen + width - 160),
                        };

                    if (position.Y < __instance.yPositionOnScreen - 64 - 8)
                    {
                        position.X = Game1.random.Next(
                            xPositionOnScreen + (width / 2) - 116,
                            xPositionOnScreen + (width / 2) + 116);
                    }

                    position.X = position.X / 20f * 4f * 5f;
                    ___littleStars.Add(
                        new TemporaryAnimatedSprite(
                            "LooseSprites\\Cursors",
                            new Rectangle(364, 79, 5, 5),
                            80f,
                            7,
                            1,
                            position,
                            flicker: false,
                            flipped: false,
                            1f,
                            0f,
                            Color.White,
                            4f,
                            0f,
                            0f,
                            0f)
                        { local = true, });
                }

                if (___timerBeforeStart > 0)
                {
                    ___timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
                    if (___timerBeforeStart > 0 || !Game1.options.SnappyMenus)
                    {
                        return false;
                    }

                    __instance.populateClickableComponentList();
                    __instance.snapToDefaultClickableComponent();

                    return false;
                }

                __instance.height = 512;
                ___oldMouseState = Game1.input.GetMouseState();
                if (__instance is { isActive: true, informationUp: false, starIcon: not null })
                {
                    __instance.starIcon.sourceRect.X =
                        __instance.starIcon.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 294 : 310;
                }

                if (__instance is { isActive: true, starIcon: not null, informationUp: false } &&
                    (___oldMouseState.LeftButton == ButtonState.Pressed ||
                     (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) &&
                    __instance.starIcon.containsPoint(___oldMouseState.X, ___oldMouseState.Y))
                {
                    ___newCraftingRecipes.Clear();
                    ___extraInfoForLevel.Clear();
                    player.completelyStopAnimatingOrDoingAction();
                    Game1.playSound("bigSelect");
                    __instance.informationUp = true;
                    __instance.isProfessionChooser = false;
                    (___currentSkill, ___currentLevel) = player.newLevels[0];
                    ___title = Game1.content.LoadString(
                        "Strings\\UI:LevelUp_Title",
                        ___currentLevel,
                        Farmer.getSkillDisplayNameFromIndex(___currentSkill));
                    ___extraInfoForLevel = __instance.getExtraInfoForLevel(___currentSkill, ___currentLevel);
                    ___sourceRectForLevelIcon = ___currentSkill switch
                    {
                        0 => new Rectangle(0, 0, 16, 16),
                        1 => new Rectangle(16, 0, 16, 16),
                        3 => new Rectangle(32, 0, 16, 16),
                        2 => new Rectangle(80, 0, 16, 16),
                        4 => new Rectangle(128, 16, 16, 16),
                        5 => new Rectangle(64, 0, 16, 16),
                        _ => ___sourceRectForLevelIcon,
                    };

                    ___professionsToChoose.Clear();
                    ___isProfessionChooser = true;
                    ISkill skill = VanillaSkill.FromValue(___currentSkill);
                    switch (___currentLevel)
                    {
                        case 15:
                            ___professionsToChoose.AddRange(skill.TierOneProfessions.Where(p => p.GetBranchingProfessions.Any(x => player.professions.Contains(x.Id))).Select(x => x.Id));
                            break;
                        case 20:
                            //Debería haber un y solo un prestige en este punto
                            int rootId = skill.TierOneProfessions.Where(p => player.professions.Contains(p.Id + 100)).Select(x => x.Id).First();

                            IProfession root = VanillaProfession.FromValue(rootId);
                            ___professionsToChoose.AddRange(root.GetBranchingProfessions
                                .Select(p => p.Id)
                                .Where(player.professions.Contains));
                            break;
                    }

                    ___leftProfessionDescription = LevelUpMenu.getProfessionDescription(___professionsToChoose[0]);
                    if (___professionsToChoose.Count > 1)
                    {
                        ___rightProfessionDescription = LevelUpMenu.getProfessionDescription(___professionsToChoose[1]);
                    }

                    const int newHeight = 0;
                    __instance.height = newHeight + 256 + (___extraInfoForLevel.Count * 64 * 3 / 4);
                    player.freezePause = 100;
                }

                if (!__instance.isActive || !__instance.informationUp)
                {
                    return false;
                }

                player.completelyStopAnimatingOrDoingAction();
                if (__instance.okButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
                {
                    __instance.okButton.scale = Math.Min(1.1f, __instance.okButton.scale + 0.05f);
                    if ((___oldMouseState.LeftButton == ButtonState.Pressed ||
                         (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) &&
                        __instance.readyToClose())
                    {
                        __instance.okButtonClicked();

                        ISkill skill = VanillaSkill.FromValue(___currentSkill);

                        switch (___currentLevel)
                        {
                            case 15:
                                int rootId15 = skill.TierOneProfessions.Where(p => p.GetBranchingProfessions.Any(x => player.professions.Contains(x.Id))).Select(x => x.Id).First();
                                if (player.professions.AddOrReplace(rootId15 + 100))
                                {
                                    __instance.getImmediateProfessionPerk(rootId15 + 100);
                                }

                                break;
                            case 20:
                                //Debería haber un y solo un prestige en este punto
                                int rootId20 = skill.TierOneProfessions.Where(p => player.professions.Contains(p.Id + 100)).Select(x => x.Id).First();

                                var GetCurrentBranchingProfessionForRoot = AccessTools.Method("DaLion.Professions.Framework.Extensions.FarmerExtensions:GetCurrentBranchingProfessionForRoot");
                                var branch = (int)GetCurrentBranchingProfessionForRoot.Invoke(null,[player, VanillaProfession.FromValue(rootId20)])!;

                                ___professionsToChoose.Add(branch);
                                if (player.professions.AddOrReplace(branch + 100))
                                {
                                    __instance.getImmediateProfessionPerk(branch + 100);
                                }

                                break;
                        }

                        __instance.RemoveLevelFromLevelList();
                    }
                }
                else
                {
                    __instance.okButton.scale = Math.Max(1f, __instance.okButton.scale - 0.05f);
                }

                player.freezePause = 100;

                #endregion choose single profession

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                ModEntry.LogMonitor.Log($"Failed in {MethodBase.GetCurrentMethod()?.Name}:\n{ex}",LogLevel.Info);
                return true; // default to original logic
            }
        }

        private static bool ShouldSuppressClick(int hovered, int currentLevel)
        {
            var HasAllProfessionsBranchingFrom = AccessTools.Method("DaLion.Professions.Framework.Extensions.FarmerExtensions:HasAllProfessionsBranchingFrom");
            var HasProfession = AccessTools.Method("DaLion.Professions.Framework.Extensions.FarmerExtensions:HasProfession");

            return VanillaProfession.TryFromValue(hovered, out var profession) &&
                   ((currentLevel == 5 && (bool)HasAllProfessionsBranchingFrom.Invoke(null,[Game1.player, profession])!) ||
                    (currentLevel == 10 && (bool)HasProfession.Invoke(null,[Game1.player, profession, false])!));
        }
    }
}
