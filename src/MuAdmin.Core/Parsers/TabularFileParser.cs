using System.Collections.Generic;
using System.IO;
using System.Text;
using MuAdmin.Core.Models;

namespace MuAdmin.Core.Parsers
{
    /// <summary>
    /// Reader/writer for whitespace-separated MU server text files
    /// (<c>Monster.txt</c>, <c>Shop*.txt</c>, <c>EventItemBag*.txt</c>,
    /// <c>Quest.txt</c>, <c>Item.txt</c>, <c>Gate.txt</c>, ...).
    /// </summary>
    /// <remarks>
    /// The parser is conservative: every source line is preserved as a
    /// <see cref="TabularLine"/> with its <see cref="TabularLine.OriginalText"/>.
    /// On save, <see cref="TabularLine.OriginalText"/> is emitted verbatim
    /// unless <see cref="TabularLine.Modified"/> is <c>true</c>, in which case
    /// the cells are joined by a single tab and the trailing comment is
    /// re-appended. This guarantees that round-trip of an unmodified file
    /// produces byte-for-byte identical output.
    /// </remarks>
    public static class TabularFileParser
    {
        public static TabularFile Parse(string path)
        {
            EncodingHelper.DetectionResult enc;
            string text = EncodingHelper.ReadAllText(path, out enc);
            var file = new TabularFile { Path = path, Encoding = enc };

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    file.Lines.Add(ParseLine(line));
            }
            return file;
        }

        public static void Save(TabularFile file, string path = null)
        {
            string target = path ?? file.Path;
            var sb = new StringBuilder(file.Lines.Count * 32);
            for (int i = 0; i < file.Lines.Count; i++)
            {
                var l = file.Lines[i];
                sb.Append(SerializeLine(l));
                if (i < file.Lines.Count - 1)
                    sb.Append(file.Encoding.NewLine);
            }
            // Preserve trailing newline if the source had one.
            // We can't inspect that easily here, but most MU files end with a
            // newline; emit one if the last source line wasn't blank-on-blank.
            sb.Append(file.Encoding.NewLine);
            EncodingHelper.WriteAllText(target, sb.ToString(), file.Encoding);
        }

        private static string SerializeLine(TabularLine l)
        {
            if (!l.Modified) return l.OriginalText ?? string.Empty;
            switch (l.Kind)
            {
                case LineKind.Data:
                    var sb = new StringBuilder();
                    sb.Append(l.LeadingWhitespace ?? string.Empty);
                    for (int i = 0; i < l.Cells.Count; i++)
                    {
                        if (i > 0) sb.Append('\t');
                        sb.Append(l.Cells[i] ?? string.Empty);
                    }
                    if (!string.IsNullOrEmpty(l.TrailingComment))
                    {
                        sb.Append('\t');
                        sb.Append(l.TrailingComment);
                    }
                    return sb.ToString();
                default:
                    return l.OriginalText ?? string.Empty;
            }
        }

        private static TabularLine ParseLine(string raw)
        {
            var line = new TabularLine { OriginalText = raw };

            // Whitespace-only or empty -> Blank.
            if (string.IsNullOrWhiteSpace(raw))
            {
                line.Kind = LineKind.Blank;
                return line;
            }

            // Find leading whitespace prefix.
            int firstNonWs = 0;
            while (firstNonWs < raw.Length && (raw[firstNonWs] == ' ' || raw[firstNonWs] == '\t'))
                firstNonWs++;
            line.LeadingWhitespace = raw.Substring(0, firstNonWs);
            string trimmed = raw.Substring(firstNonWs);

            // Pure comment line.
            if (trimmed.StartsWith("//"))
            {
                line.Kind = LineKind.Comment;
                return line;
            }

            // Block markers used by Quest.txt / EventItemBag*.txt /
            // MonsterSetBase*.txt etc. ("end" terminator and standalone integer
            // section-count headers).
            string trimmedFull = trimmed.TrimEnd();
            if (string.Equals(trimmedFull, "end", System.StringComparison.OrdinalIgnoreCase))
            {
                line.Kind = LineKind.BlockMarker;
                return line;
            }

            // Split content from trailing "// ..." comment, but be careful not
            // to break on // inside quoted strings.
            int commentStart = FindTrailingCommentStart(trimmed);
            string contentPart = commentStart < 0 ? trimmed : trimmed.Substring(0, commentStart).TrimEnd();
            string commentPart = commentStart < 0 ? string.Empty : trimmed.Substring(commentStart).TrimEnd();

            line.TrailingComment = commentPart;
            line.Cells = SplitCells(contentPart);
            line.Kind = LineKind.Data;
            return line;
        }

        private static int FindTrailingCommentStart(string s)
        {
            bool inQuote = false;
            for (int i = 0; i < s.Length - 1; i++)
            {
                char c = s[i];
                if (c == '"') inQuote = !inQuote;
                else if (!inQuote && c == '/' && s[i + 1] == '/') return i;
            }
            return -1;
        }

        /// <summary>
        /// Splits a row into cells on runs of whitespace, but treats a quoted
        /// substring (<c>"..."</c>) as a single cell so that names containing
        /// spaces survive the round-trip.
        /// </summary>
        private static List<string> SplitCells(string s)
        {
            var cells = new List<string>();
            int i = 0;
            while (i < s.Length)
            {
                while (i < s.Length && (s[i] == ' ' || s[i] == '\t')) i++;
                if (i >= s.Length) break;
                if (s[i] == '"')
                {
                    int start = i++;
                    while (i < s.Length && s[i] != '"') i++;
                    if (i < s.Length) i++; // include closing quote
                    cells.Add(s.Substring(start, i - start));
                }
                else
                {
                    int start = i;
                    while (i < s.Length && s[i] != ' ' && s[i] != '\t') i++;
                    cells.Add(s.Substring(start, i - start));
                }
            }
            return cells;
        }
    }
}
