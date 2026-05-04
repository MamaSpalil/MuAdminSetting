using System.Collections.Generic;

namespace MuAdmin.Core.Models
{
    /// <summary>
    /// In-memory representation of a tab/space separated MU server text file
    /// with <c>//</c> line and trailing comments.
    /// </summary>
    /// <remarks>
    /// The format used across <c>Monsters\Monster.txt</c>, <c>Shops\*.txt</c>,
    /// <c>EventItemBags\*.txt</c>, <c>Quests\Quest.txt</c>, <c>Items\Item.txt</c>,
    /// <c>Move\Gate.txt</c> and many others. The model preserves every single
    /// source line so a round-trip save of an unmodified file produces
    /// byte-for-byte identical output.
    /// </remarks>
    public sealed class TabularFile
    {
        public string Path { get; set; }
        public EncodingHelper.DetectionResult Encoding { get; set; }
        public List<TabularLine> Lines { get; } = new List<TabularLine>();

        /// <summary>All data rows in source order (read-only view).</summary>
        public IEnumerable<TabularLine> DataRows
        {
            get
            {
                foreach (var l in Lines)
                    if (l.Kind == LineKind.Data) yield return l;
            }
        }
    }

    public enum LineKind
    {
        Blank,
        Comment,
        Data,
        BlockMarker  // "end" or single integer block-count headers
    }

    /// <summary>
    /// A single physical line of a tabular file. <see cref="OriginalText"/> is
    /// always preserved verbatim and is used when the row has not been edited;
    /// edits replace it with a re-emitted version that joins
    /// <see cref="Cells"/> with tabs and appends <see cref="TrailingComment"/>.
    /// </summary>
    public sealed class TabularLine
    {
        public LineKind Kind { get; set; }
        public string OriginalText { get; set; }
        public bool Modified { get; set; }

        /// <summary>Whitespace prefix on the line (preserved on save).</summary>
        public string LeadingWhitespace { get; set; } = string.Empty;
        public List<string> Cells { get; set; } = new List<string>();
        /// <summary>Inline trailing comment including the leading <c>//</c>.</summary>
        public string TrailingComment { get; set; } = string.Empty;

        public override string ToString()
        {
            return Kind == LineKind.Data ? string.Join("\t", Cells) : OriginalText;
        }
    }
}
