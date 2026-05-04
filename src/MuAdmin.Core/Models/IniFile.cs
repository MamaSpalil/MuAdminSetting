using System.Collections.Generic;

namespace MuAdmin.Core.Models
{
    /// <summary>
    /// Order-preserving INI file model. Each entry corresponds to a single
    /// physical line (section header, key/value pair, comment or blank). On
    /// save the parser re-emits unmodified lines verbatim so the diff is
    /// limited strictly to user edits.
    /// </summary>
    public sealed class IniFile
    {
        public string Path { get; set; }
        public EncodingHelper.DetectionResult Encoding { get; set; }
        public List<IniLine> Lines { get; } = new List<IniLine>();

        public IEnumerable<IniLine> Sections
        {
            get
            {
                foreach (var l in Lines)
                    if (l.Kind == IniLineKind.Section) yield return l;
            }
        }

        public IniLine FindKey(string section, string key)
        {
            string current = string.Empty;
            foreach (var l in Lines)
            {
                if (l.Kind == IniLineKind.Section) current = l.SectionName;
                else if (l.Kind == IniLineKind.KeyValue
                         && string.Equals(current, section, System.StringComparison.OrdinalIgnoreCase)
                         && string.Equals(l.Key, key, System.StringComparison.OrdinalIgnoreCase))
                    return l;
            }
            return null;
        }
    }

    public enum IniLineKind
    {
        Blank,
        Comment,
        Section,
        KeyValue,
        Other
    }

    public sealed class IniLine
    {
        public IniLineKind Kind { get; set; }
        public string OriginalText { get; set; }
        public bool Modified { get; set; }

        public string SectionName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        /// <summary>Trailing inline comment, preserved verbatim on save.</summary>
        public string TrailingComment { get; set; } = string.Empty;
        /// <summary>Whitespace separator between key, '=' and value.</summary>
        public string Separator { get; set; } = " = ";
    }
}
