using MasteryExtended.Patches;

namespace MasteryExtended
{
    internal readonly record struct BookInfo
    {
        public string Id { get; }
        public string DisplayNamePath { get; }
        public string BookDescriptionPath { get; }
        public int SpriteIndex { get; }
        public string ShopCondition { get; }
        public IReadOnlyList<string> ContextTags { get; }

        private BookInfo(string id, string displayNamePath, string bookDescriptionPath, int spriteIndex, string shopCondition, IReadOnlyList<string> contextTags)
        {
            Id = id;
            DisplayNamePath = displayNamePath;
            BookDescriptionPath = bookDescriptionPath;
            SpriteIndex = spriteIndex;
            ShopCondition = shopCondition;
            ContextTags = contextTags;
        }

        private static readonly BookInfo[] shortList = [
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillFarming_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillFarming_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillFarming_BookDescription",
                    24,
                    $"PLAYER_BASE_FARMING_LEVEL Current 10, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillFarming_ID 0 0",
                    ["book_item", "color_gold"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillFishing_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillFishing_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillFishing_BookDescription",
                    25,
                    $"PLAYER_BASE_FISHING_LEVEL Current 10, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillFishing_ID 0 0",
                    ["book_item", "color_blue"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillForaging_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillForaging_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillForaging_BookDescription",
                    26,
                    $"PLAYER_BASE_FORAGING_LEVEL Current 10, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillForaging_ID 0 0",
                    ["book_item", "color_green"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillMining_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillMining_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillMining_BookDescription",
                    27,
                    $"PLAYER_BASE_MINING_LEVEL Current 10, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillMining_ID 0 0",
                    ["book_item", "color_brown"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillCombat_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillCombat_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillCombat_BookDescription",
                    28,
                    $"PLAYER_BASE_COMBAT_LEVEL Current 10, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillCombat_ID 0 0",
                    ["book_item", "color_red"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_Unlock_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_Unlock_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_Unlock_BookDescription",
                    11,
                    $"PLAYER_VISITED_LOCATION Current MasteryCave, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Unlock_ID 0 0",
                    ["book_item", "color_iridium"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_Complete_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_Complete_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_Complete_BookDescription",
                    5,
                    $"PLAYER_VISITED_LOCATION Current MasteryCave, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Unlock_ID 1, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Complete_ID 0 0",
                    ["book_item", "color_iridium"]
                ),
            ];
        private static readonly BookInfo[] fullList = [
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionCoopmaster_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionCoopmaster_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionFarming_BookDescription",
                    0,
                    $"PLAYER_HAS_PROFESSION Current 2, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionCoopmaster_ID 0 0",
                    ["book_item", "color_gold"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionShepherd_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionShepherd_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionFarming_BookDescription",
                    6,
                    $"PLAYER_HAS_PROFESSION Current 3, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionShepherd_ID 0 0",
                    ["book_item", "color_gold"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionArtisan_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionArtisan_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionFarming_BookDescription",
                    12,
                    $"PLAYER_HAS_PROFESSION Current 4, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionArtisan_ID 0 0",
                    ["book_item", "color_gold"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAgriculturist_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionAgriculturist_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionFarming_BookDescription",
                    18,
                    $"PLAYER_HAS_PROFESSION Current 5, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAgriculturist_ID 0 0",
                    ["book_item", "color_gold"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAngler_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionAngler_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionFishing_BookDescription",
                    1,
                    $"PLAYER_HAS_PROFESSION Current 8, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAngler_ID 0 0",
                    ["book_item", "color_blue"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionPirate_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionPirate_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionFishing_BookDescription",
                    7,
                    $"PLAYER_HAS_PROFESSION Current 9, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionPirate_ID 0 0",
                    ["book_item", "color_blue"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionMariner_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionMariner_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionFishing_BookDescription",
                    13,
                    $"PLAYER_HAS_PROFESSION Current 10, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionMariner_ID 0 0",
                    ["book_item", "color_blue"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionLuremaster_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionLuremaster_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionFishing_BookDescription",
                    19,
                    $"PLAYER_HAS_PROFESSION Current 11, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionLuremaster_ID 0 0",
                    ["book_item", "color_blue"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionLumberjack_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionLumberjack_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionForaging_BookDescription",
                    2,
                    $"PLAYER_HAS_PROFESSION Current 14, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionLumberjack_ID 0 0",
                    ["book_item", "color_green"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionTapper_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionTapper_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionForaging_BookDescription",
                    8,
                    $"PLAYER_HAS_PROFESSION Current 15, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionTapper_ID 0 0",
                    ["book_item", "color_green"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBotanist_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionBotanist_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionForaging_BookDescription",
                    14,
                    $"PLAYER_HAS_PROFESSION Current 16, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBotanist_ID 0 0",
                    ["book_item", "color_green"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionTracker_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionTracker_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionForaging_BookDescription",
                    20,
                    $"PLAYER_HAS_PROFESSION Current 17, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionTracker_ID 0 0",
                    ["book_item", "color_green"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBlacksmith_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionBlacksmith_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionMining_BookDescription",
                    3,
                    $"PLAYER_HAS_PROFESSION Current 20, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBlacksmith_ID 0 0",
                    ["book_item", "color_brown"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionProspector_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionProspector_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionMining_BookDescription",
                    9,
                    $"PLAYER_HAS_PROFESSION Current 21, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionProspector_ID 0 0",
                    ["book_item", "color_brown"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionExcavator_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionExcavator_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionMining_BookDescription",
                    15,
                    $"PLAYER_HAS_PROFESSION Current 22, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionExcavator_ID 0 0",
                    ["book_item", "color_brown"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionGemologist_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionGemologist_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionMining_BookDescription",
                    21,
                    $"PLAYER_HAS_PROFESSION Current 23, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionGemologist_ID 0 0",
                    ["book_item", "color_brown"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBrute_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionBrute_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionCombat_BookDescription",
                    4,
                    $"PLAYER_HAS_PROFESSION Current 26, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBrute_ID 0 0",
                    ["book_item", "color_red"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionDefender_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionDefender_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionCombat_BookDescription",
                    10,
                    $"PLAYER_HAS_PROFESSION Current 27, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionDefender_ID 0 0",
                    ["book_item", "color_red"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAcrobat_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionAcrobat_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionCombat_BookDescription",
                    16,
                    $"PLAYER_HAS_PROFESSION Current 28, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAcrobat_ID 0 0",
                    ["book_item", "color_red"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionDesperado_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionDesperado_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_ProfessionCombat_BookDescription",
                    22,
                    $"PLAYER_HAS_PROFESSION Current 29, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionDesperado_ID 0 0",
                    ["book_item", "color_red"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_Unlock_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_Unlock_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_Unlock_BookDescription",
                    11,
                    $"PLAYER_VISITED_LOCATION Current MasteryCave, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Unlock_ID 0 0",
                    ["book_item", "color_iridium"]
                ),
                new BookInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_Complete_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_Complete_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_Complete_BookDescription",
                    5,
                    $"PLAYER_VISITED_LOCATION Current MasteryCave, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Unlock_ID 1, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Complete_ID 0 0",
                    ["book_item", "color_iridium"]
                ),
            ];

        internal static IReadOnlyList<BookInfo> BookPowerListShort => shortList;
        internal static IReadOnlyList<BookInfo> BookPowerListComplete => fullList;
    }

    internal readonly record struct PowerInfo
    {
        public string Id { get; }
        public string DisplayNamePath { get; }
        public string PowerDescriptionPath { get; }
        public string TexturePath { get; }
        public int SpriteIndex { get; }
        public string PowerUnlockCondition { get; }
        private IReadOnlyList<Func<string>>? DescriptionSubstitutions { get; }
        public bool IsDogStatueUnlock { get; }

        public readonly object[] GetSubstitutions()
        {
            return DescriptionSubstitutions?.Select(method => method()).ToArray() ?? Array.Empty<object>();
        }

        private PowerInfo(string id, string displayNamePath, string powerDescriptionPath, string texturePath, int spriteIndex, string powerUnlockCondition, Func<string>[]? subs = null, bool isDogStatueUnlock = false)
        {
            Id = id;
            DisplayNamePath = displayNamePath;
            PowerDescriptionPath = powerDescriptionPath;
            TexturePath = texturePath;
            SpriteIndex = spriteIndex;
            PowerUnlockCondition = powerUnlockCondition;
            DescriptionSubstitutions = subs;
            IsDogStatueUnlock = isDogStatueUnlock;
        }

        private static readonly PowerInfo[] powerList = [
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillFarming_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillFarming_BookName",
                    "Strings\\UI:MasteryExtended_BookPower_FarmingMastery_BookDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                    24,
                    $"ANY \"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Full, ANY " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionCoopmaster_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionShepherd_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionArtisan_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAgriculturist_ID 1\\\"\" " +
                        $"\"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Lite, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillFarming_ID 1\"",
                    [() => ModEntry.Config.BooksQuantity == BooksQuantityOption.Full? ((int)(FarmerPatch.ExtraMasteryExperienceMultiplier(0, false) * 100)).ToString(): "50"]
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillFishing_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillFishing_BookName",
                    "Strings\\UI:MasteryExtended_BookPower_FishingMastery_BookDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                    25,
                    $"ANY \"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Full, ANY " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAngler_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionPirate_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionMariner_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionLuremaster_ID 1\\\"\" " +
                        $"\"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Lite, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillFishing_ID 1\"",
                    [() => ModEntry.Config.BooksQuantity == BooksQuantityOption.Full? ((int)(FarmerPatch.ExtraMasteryExperienceMultiplier(1, false) * 100)).ToString(): "50"]
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillForaging_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillForaging_BookName",
                    "Strings\\UI:MasteryExtended_BookPower_ForagingMastery_BookDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                    26,
                    $"ANY \"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Full, ANY " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionLumberjack_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionTapper_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBotanist_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionTracker_ID 1\\\"\" " +
                        $"\"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Lite, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillForaging_ID 1\"",
                    [() => ModEntry.Config.BooksQuantity == BooksQuantityOption.Full? ((int)(FarmerPatch.ExtraMasteryExperienceMultiplier(2, false) * 100)).ToString(): "50"]
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillMining_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillMining_BookName",
                    "Strings\\UI:MasteryExtended_BookPower_MiningMastery_BookDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                    27,
                    $"ANY \"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Full, ANY " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBlacksmith_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionProspector_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionExcavator_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionGemologist_ID 1\\\"\" " +
                        $"\"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Lite, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillMining_ID 1\"",
                    [() => ModEntry.Config.BooksQuantity == BooksQuantityOption.Full? ((int)(FarmerPatch.ExtraMasteryExperienceMultiplier(3, false) * 100)).ToString(): "50"]
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_SkillCombat_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_SkillCombat_BookName",
                    "Strings\\UI:MasteryExtended_BookPower_CombatMastery_BookDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                    28,
                    $"ANY \"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Full, ANY " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionBrute_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionDefender_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionAcrobat_ID 1\\\" " +
                        $"\\\"PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_ProfessionDesperado_ID 1\\\"\" " +
                        $"\"{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Lite, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_SkillCombat_ID 1\"",
                    [() => ModEntry.Config.BooksQuantity == BooksQuantityOption.Full? ((int)(FarmerPatch.ExtraMasteryExperienceMultiplier(4, false) * 100)).ToString(): "50"]
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_Unlock_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_Unlock_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_Unlock_BookDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                    11,
                    $"!{ModEntry.ModManifest.UniqueID}_BookQuantityConfig None, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Unlock_ID 1",
                    [() => "50"]
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_BookMastery_Complete_ID",
                    "Strings\\UI:MasteryExtended_BookMastery_Complete_BookName",
                    "Strings\\UI:MasteryExtended_BookMastery_Complete_BookDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                    5,
                    $"!{ModEntry.ModManifest.UniqueID}_BookQuantityConfig Lite, PLAYER_STAT Current {ModEntry.ModManifest.UniqueID}_BookMastery_Complete_ID 1",
                    [() => "50"]
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_Reaper",
                    "Strings\\UI:MasteryExtended_ReaperName",
                    "Strings\\UI:MasteryExtended_ReaperDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/DogPowers",
                    0,
                    $"PLAYER_MOD_DATA Current {ModEntry.ModManifest.UniqueID}/ExtraMastery/Reaper true"
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_Mason",
                    "Strings\\UI:MasteryExtended_MasonName",
                    "Strings\\UI:MasteryExtended_MasonDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/DogPowers",
                    3,
                    $"PLAYER_MOD_DATA Current {ModEntry.ModManifest.UniqueID}/ExtraMastery/Mason true"
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_Woodlander",
                    "Strings\\UI:MasteryExtended_WoodlanderName",
                    "Strings\\UI:MasteryExtended_WoodlanderDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/DogPowers",
                    2,
                    $"PLAYER_MOD_DATA Current {ModEntry.ModManifest.UniqueID}/ExtraMastery/Woodlander true"
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_Baitbinder",
                    "Strings\\UI:MasteryExtended_BaitbinderName",
                    "Strings\\UI:MasteryExtended_BaitbinderDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/DogPowers",
                    1,
                    $"PLAYER_MOD_DATA Current {ModEntry.ModManifest.UniqueID}/ExtraMastery/Baitbinder true"
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_Runesmith",
                    "Strings\\UI:MasteryExtended_RunesmithName",
                    "Strings\\UI:MasteryExtended_RunesmithDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/DogPowers",
                    4,
                    $"PLAYER_MOD_DATA Current {ModEntry.ModManifest.UniqueID}/ExtraMastery/Runesmith true"
                ),
                new PowerInfo(
                    $"{ModEntry.ModManifest.UniqueID}_Attractive",
                    "Strings\\UI:MasteryExtended_AttractiveName",
                    "Strings\\UI:MasteryExtended_AttractiveDescription",
                    $"Tilesheets/{ModEntry.ModManifest.UniqueID}/DogPowers",
                    5,
                    $"PLAYER_MOD_DATA Current {ModEntry.ModManifest.UniqueID}/ExtraMastery/Attractive true"
                ),
            ];
        internal static IReadOnlyList<PowerInfo> PowerList => powerList;
    }
}
