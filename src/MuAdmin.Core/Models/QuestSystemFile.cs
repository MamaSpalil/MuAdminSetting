using System.Collections.Generic;

namespace MuAdmin.Core.Models
{
    /// <summary>
    /// In-memory representation of <c>Quests\QuestSystem.ini</c>. The file is a
    /// hybrid: a small INI <c>[Common]</c> block at the top, then a single
    /// integer "block-count" header (e.g. <c>1</c>), then a list of
    /// whitespace/tab-separated quest rows that contain quoted message strings,
    /// finally an <c>end</c> marker.
    /// </summary>
    /// <remarks>
    /// Every physical source line is preserved as a <see cref="QuestSystemLine"/>
    /// with its <see cref="QuestSystemLine.OriginalText"/>; on save unmodified
    /// lines are emitted verbatim, exactly like
    /// <see cref="TabularFile"/>/<see cref="IniFile"/>, so a round-trip of an
    /// untouched file is byte-for-byte identical.
    /// </remarks>
    public sealed class QuestSystemFile
    {
        public string Path { get; set; }
        public EncodingHelper.DetectionResult Encoding { get; set; }
        public List<QuestSystemLine> Lines { get; } = new List<QuestSystemLine>();
    }

    public enum QuestSystemLineKind
    {
        Blank,
        Comment,
        Section,
        KeyValue,
        BlockMarker,    // standalone integer (block count) or "end"
        Quest,
        Other
    }

    /// <summary>
    /// A single physical line. <see cref="OriginalText"/> is preserved verbatim
    /// and re-emitted as-is unless <see cref="Modified"/> is <c>true</c>.
    /// </summary>
    public sealed class QuestSystemLine
    {
        public QuestSystemLineKind Kind { get; set; }
        public string OriginalText { get; set; }
        public bool Modified { get; set; }

        // INI Section / KeyValue
        public string SectionName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Separator { get; set; } = " = ";
        public string TrailingComment { get; set; } = string.Empty;

        // Quest / Data
        public string LeadingWhitespace { get; set; } = string.Empty;
        /// <summary>
        /// Raw cells split from the row (preserves quoted message cells with
        /// their surrounding double quotes). For an unparseable row this is the
        /// only source of truth; the typed <see cref="QuestRow"/> mirrors the
        /// well-known 17-column schema described in the project README.
        /// </summary>
        public List<string> Cells { get; set; } = new List<string>();
    }

    /// <summary>Prize type for a <see cref="QuestSystemLine"/> quest row.</summary>
    public enum QuestPrizeType
    {
        Items = 0,
        Zen = 1,
        NormalPoints = 2,
        Credits = 3,
        WCoin = 4
    }

    /// <summary>
    /// Strongly-typed view of a quest row's cells. The 17 columns are:
    /// LocationId, MonsterId, MonsterCount, Percent, PrizeType, PrizeValue,
    /// Message1, Message2, DropItemCount, ItemType, ItemIndex, LvlMin, LvlMax,
    /// Skill, Luck, Opt, Exc.
    /// </summary>
    public static class QuestRowSchema
    {
        public const int ColumnCount = 17;

        public const int Location      = 0;
        public const int Monster       = 1;
        public const int MonsterCount  = 2;
        public const int Percent       = 3;
        public const int PrizeType     = 4;
        public const int PrizeValue    = 5;
        public const int Message1      = 6;
        public const int Message2      = 7;
        public const int DropItemCount = 8;
        public const int ItemType      = 9;
        public const int ItemIndex     = 10;
        public const int LvlMin        = 11;
        public const int LvlMax        = 12;
        public const int Skill         = 13;
        public const int Luck          = 14;
        public const int Opt           = 15;
        public const int Exc           = 16;

        public static readonly string[] ColumnTitles =
        {
            "Location", "Monster", "Count", "%", "PrizeType", "PrizeValue",
            "Message1", "Message2",
            "DropCnt", "ItemType", "ItemIndex", "LvlMin", "LvlMax",
            "Skill", "Luck", "Opt", "Exc"
        };
    }
}
