using System.Collections.Generic;
using System.IO;
using MuAdmin.Core.Models;
using MuAdmin.Core.Parsers;

namespace MuAdmin.Core.Catalogs
{
    /// <summary>
    /// Loads <c>Items\Item.txt</c> into an in-memory map of
    /// <c>(Type, Index) → ItemName</c> so that other UI views (Shops,
    /// EventItemBags, drop tables) can show readable names instead of raw
    /// numeric IDs.
    /// </summary>
    public sealed class ItemCatalog
    {
        private readonly Dictionary<long, string> _byKey = new Dictionary<long, string>();

        public string GetName(int type, int index)
        {
            string name;
            return _byKey.TryGetValue(Key(type, index), out name)
                ? name
                : "Item " + type + "/" + index;
        }

        public bool TryGetName(int type, int index, out string name)
        {
            return _byKey.TryGetValue(Key(type, index), out name);
        }

        public IEnumerable<KeyValuePair<long, string>> All { get { return _byKey; } }

        public static long Key(int type, int index) { return ((long)type << 32) | (uint)index; }

        public static ItemCatalog Load(string itemTxtPath)
        {
            var cat = new ItemCatalog();
            if (string.IsNullOrEmpty(itemTxtPath) || !File.Exists(itemTxtPath))
                return cat;

            TabularFile tf;
            try { tf = TabularFileParser.Parse(itemTxtPath); }
            catch { return cat; }

            int currentType = -1;
            foreach (var l in tf.Lines)
            {
                if (l.Kind != LineKind.Data) continue;
                // A type-section header is a single integer on its own line.
                if (l.Cells.Count == 1)
                {
                    int t;
                    if (int.TryParse(l.Cells[0], out t)) { currentType = t; continue; }
                }
                if (currentType < 0 || l.Cells.Count < 9) continue;
                int idx;
                if (!int.TryParse(l.Cells[0], out idx)) continue;
                // Column 8 (0-based) is ItemName according to the Item.txt
                // header in this repository.
                string raw = l.Cells[8];
                cat._byKey[Key(currentType, idx)] = Unquote(raw);
            }
            return cat;
        }

        private static string Unquote(string s)
        {
            if (s == null) return string.Empty;
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
                return s.Substring(1, s.Length - 2);
            return s;
        }
    }
}
