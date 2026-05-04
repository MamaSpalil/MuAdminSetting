using System.Collections.Generic;
using System.IO;
using System.Text;
using MuAdmin.Core.Models;

namespace MuAdmin.Core.Parsers
{
    /// <summary>
    /// Reader/writer for <c>Quests\QuestSystem.ini</c>. Combines the
    /// INI-style key/value parsing used by <see cref="IniFileParser"/> for the
    /// top <c>[Common]</c> section with the quoted-aware tab-separated row
    /// parsing used by <see cref="TabularFileParser"/> for the quest table.
    /// </summary>
    /// <remarks>
    /// Order-preserving: every source line ends up as a
    /// <see cref="QuestSystemLine"/>; if a line is not edited it is emitted
    /// verbatim from <see cref="QuestSystemLine.OriginalText"/> on save.
    /// </remarks>
    public static class QuestSystemFileParser
    {
        public static QuestSystemFile Parse(string path)
        {
            EncodingHelper.DetectionResult enc;
            string text = EncodingHelper.ReadAllText(path, out enc);
            var file = new QuestSystemFile { Path = path, Encoding = enc };

            // Track whether we are still inside the top-of-file INI section.
            // The first non-INI data line (single integer block marker, or a
            // multi-cell quest row) flips us into "table" mode and we stop
            // interpreting subsequent "key = value"-shaped lines as INI.
            bool inHeader = true;

            using (var reader = new StringReader(text))
            {
                string raw;
                while ((raw = reader.ReadLine()) != null)
                {
                    var line = ParseLine(raw, ref inHeader);
                    file.Lines.Add(line);
                }
            }
            return file;
        }

        public static void Save(QuestSystemFile file, string path = null)
        {
            string target = path ?? file.Path;
            var sb = new StringBuilder(file.Lines.Count * 48);
            for (int i = 0; i < file.Lines.Count; i++)
            {
                sb.Append(SerializeLine(file.Lines[i]));
                if (i < file.Lines.Count - 1)
                    sb.Append(file.Encoding.NewLine);
            }
            sb.Append(file.Encoding.NewLine);
            EncodingHelper.WriteAllText(target, sb.ToString(), file.Encoding);
        }

        private static QuestSystemLine ParseLine(string raw, ref bool inHeader)
        {
            var line = new QuestSystemLine { OriginalText = raw };

            if (string.IsNullOrWhiteSpace(raw))
            {
                line.Kind = QuestSystemLineKind.Blank;
                return line;
            }

            // Capture leading whitespace, used when a Quest row needs to be
            // re-emitted (preserves source indentation).
            int firstNonWs = 0;
            while (firstNonWs < raw.Length && (raw[firstNonWs] == ' ' || raw[firstNonWs] == '\t'))
                firstNonWs++;
            line.LeadingWhitespace = raw.Substring(0, firstNonWs);
            string trimmed = raw.Substring(firstNonWs);

            if (trimmed.StartsWith("//") || trimmed.StartsWith(";"))
            {
                line.Kind = QuestSystemLineKind.Comment;
                return line;
            }

            if (trimmed.StartsWith("["))
            {
                int close = trimmed.IndexOf(']');
                if (close > 0)
                {
                    line.Kind = QuestSystemLineKind.Section;
                    line.SectionName = trimmed.Substring(1, close - 1).Trim();
                    inHeader = true;
                    return line;
                }
            }

            // "end" terminator (case-insensitive) closes the table.
            string trimmedFull = trimmed.TrimEnd();
            if (string.Equals(trimmedFull, "end", System.StringComparison.OrdinalIgnoreCase))
            {
                line.Kind = QuestSystemLineKind.BlockMarker;
                return line;
            }

            // Split out a trailing "// ..." comment, respecting quoted strings.
            int commentStart = FindTrailingCommentStart(trimmed);
            string contentPart = commentStart < 0 ? trimmed : trimmed.Substring(0, commentStart).TrimEnd();
            string commentPart = commentStart < 0 ? string.Empty : trimmed.Substring(commentStart).TrimEnd();

            if (inHeader)
            {
                // Try INI key = value first.
                int eq = contentPart.IndexOf('=');
                if (eq > 0)
                {
                    int keyEnd = eq;
                    while (keyEnd > 0 && (contentPart[keyEnd - 1] == ' ' || contentPart[keyEnd - 1] == '\t')) keyEnd--;
                    string key = contentPart.Substring(0, keyEnd).TrimStart();
                    string sepLeft = contentPart.Substring(keyEnd, eq - keyEnd);
                    int valStart = eq + 1;
                    while (valStart < contentPart.Length && (contentPart[valStart] == ' ' || contentPart[valStart] == '\t')) valStart++;
                    string sepRight = contentPart.Substring(eq + 1, valStart - (eq + 1));
                    string value = contentPart.Substring(valStart);

                    line.Kind = QuestSystemLineKind.KeyValue;
                    line.Key = key;
                    line.Value = value;
                    line.Separator = sepLeft + "=" + sepRight;
                    line.TrailingComment = commentPart;
                    return line;
                }
            }

            // Tokenize as a tabular row (quoted-aware).
            var cells = SplitCells(contentPart);
            line.Cells = cells;
            line.TrailingComment = commentPart;

            // A standalone integer is a block-count marker (e.g. the "1" that
            // precedes the quest table). Switch out of header mode.
            if (cells.Count == 1)
            {
                int dummy;
                if (int.TryParse(cells[0], out dummy))
                {
                    line.Kind = QuestSystemLineKind.BlockMarker;
                    inHeader = false;
                    return line;
                }
            }

            if (cells.Count >= 2)
            {
                line.Kind = QuestSystemLineKind.Quest;
                inHeader = false;
                return line;
            }

            line.Kind = QuestSystemLineKind.Other;
            return line;
        }

        private static string SerializeLine(QuestSystemLine l)
        {
            if (!l.Modified) return l.OriginalText ?? string.Empty;

            switch (l.Kind)
            {
                case QuestSystemLineKind.Section:
                    return "[" + (l.SectionName ?? string.Empty) + "]";

                case QuestSystemLineKind.KeyValue:
                {
                    var sb = new StringBuilder();
                    sb.Append(l.LeadingWhitespace ?? string.Empty);
                    sb.Append(l.Key ?? string.Empty);
                    sb.Append(l.Separator ?? " = ");
                    sb.Append(l.Value ?? string.Empty);
                    if (!string.IsNullOrEmpty(l.TrailingComment))
                    {
                        sb.Append(' ');
                        sb.Append(l.TrailingComment);
                    }
                    return sb.ToString();
                }

                case QuestSystemLineKind.Quest:
                {
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
                }

                default:
                    return l.OriginalText ?? string.Empty;
            }
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
