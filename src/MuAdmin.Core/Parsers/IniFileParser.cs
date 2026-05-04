using System.IO;
using System.Text;
using MuAdmin.Core.Models;

namespace MuAdmin.Core.Parsers
{
    /// <summary>
    /// Order-preserving parser/serializer for INI/CFG files used by MU server
    /// modules (<c>Common\*.ini</c>, <c>Custom\*.ini</c>, <c>Items\*.ini</c>,
    /// <c>Quests\*.ini</c>, <c>Events\**\*.ini</c>, <c>CommonServer.cfg</c>,
    /// ...). Comment styles supported: <c>;</c> and <c>//</c>. Inline comments
    /// after a key/value are preserved verbatim. Embedded tabular blocks
    /// (e.g. inside <c>Custom\AlertSystem.ini</c>) are kept as opaque
    /// <see cref="IniLineKind.Other"/> lines and round-trip without changes.
    /// </summary>
    public static class IniFileParser
    {
        public static IniFile Parse(string path)
        {
            EncodingHelper.DetectionResult enc;
            string text = EncodingHelper.ReadAllText(path, out enc);
            var file = new IniFile { Path = path, Encoding = enc };

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    file.Lines.Add(ParseLine(line));
            }
            return file;
        }

        public static void Save(IniFile file, string path = null)
        {
            string target = path ?? file.Path;
            var sb = new StringBuilder(file.Lines.Count * 32);
            for (int i = 0; i < file.Lines.Count; i++)
            {
                sb.Append(SerializeLine(file.Lines[i]));
                if (i < file.Lines.Count - 1) sb.Append(file.Encoding.NewLine);
            }
            sb.Append(file.Encoding.NewLine);
            EncodingHelper.WriteAllText(target, sb.ToString(), file.Encoding);
        }

        private static string SerializeLine(IniLine l)
        {
            if (!l.Modified) return l.OriginalText ?? string.Empty;
            switch (l.Kind)
            {
                case IniLineKind.Section:
                    return "[" + (l.SectionName ?? string.Empty) + "]";
                case IniLineKind.KeyValue:
                    var sb = new StringBuilder();
                    sb.Append(l.Key ?? string.Empty);
                    sb.Append(l.Separator ?? " = ");
                    sb.Append(l.Value ?? string.Empty);
                    if (!string.IsNullOrEmpty(l.TrailingComment))
                    {
                        sb.Append(' ');
                        sb.Append(l.TrailingComment);
                    }
                    return sb.ToString();
                default:
                    return l.OriginalText ?? string.Empty;
            }
        }

        private static IniLine ParseLine(string raw)
        {
            var line = new IniLine { OriginalText = raw };

            if (string.IsNullOrWhiteSpace(raw)) { line.Kind = IniLineKind.Blank; return line; }

            string t = raw.TrimStart();
            if (t.StartsWith("//") || t.StartsWith(";"))
            {
                line.Kind = IniLineKind.Comment;
                return line;
            }
            if (t.StartsWith("["))
            {
                int close = t.IndexOf(']');
                if (close > 0)
                {
                    line.Kind = IniLineKind.Section;
                    line.SectionName = t.Substring(1, close - 1).Trim();
                    return line;
                }
            }

            int eq = raw.IndexOf('=');
            if (eq > 0)
            {
                string keyPart = raw.Substring(0, eq);
                string rest = raw.Substring(eq + 1);

                // Determine separator (preserves whitespace style around '=').
                int keyEnd = keyPart.Length;
                while (keyEnd > 0 && (keyPart[keyEnd - 1] == ' ' || keyPart[keyEnd - 1] == '\t')) keyEnd--;
                string key = keyPart.Substring(0, keyEnd).TrimStart();
                string sepLeft = keyPart.Substring(keyEnd);
                int valStart = 0;
                while (valStart < rest.Length && (rest[valStart] == ' ' || rest[valStart] == '\t')) valStart++;
                string sepRight = rest.Substring(0, valStart);
                string valuePart = rest.Substring(valStart);

                // Split inline trailing comment (// or ;).
                int cIdx = FindInlineCommentStart(valuePart);
                string value, comment;
                if (cIdx < 0) { value = valuePart.TrimEnd(); comment = string.Empty; }
                else
                {
                    value = valuePart.Substring(0, cIdx).TrimEnd();
                    comment = valuePart.Substring(cIdx).TrimEnd();
                }

                line.Kind = IniLineKind.KeyValue;
                line.Key = key;
                line.Value = value;
                line.TrailingComment = comment;
                line.Separator = sepLeft + "=" + sepRight;
                return line;
            }

            line.Kind = IniLineKind.Other;
            return line;
        }

        private static int FindInlineCommentStart(string s)
        {
            bool inQuote = false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"') inQuote = !inQuote;
                else if (!inQuote)
                {
                    if (c == ';') return i;
                    if (c == '/' && i + 1 < s.Length && s[i + 1] == '/') return i;
                }
            }
            return -1;
        }
    }
}
