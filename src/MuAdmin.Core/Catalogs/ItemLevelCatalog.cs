using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MuAdmin.Core.Catalogs
{
    /// <summary>
    /// Loads <c>Item_level.txt</c> from the server root into an in-memory
    /// map of <c>(ItemType, ItemIndex, ItemLevel) → Name</c>.
    /// <para>
    /// Some MU items (e.g. <c>14/11</c>) keep the same Type/Index across
    /// several levels but render under different in-game names — for
    /// instance <c>14/11/0</c> is "Box of Luck" while <c>14/11/3</c> is
    /// "Heart of Love" and <c>14/11/8</c> is "Bok of Kundun+1". The base
    /// <c>Item.txt</c> only lists one name per Type/Index, so this extra
    /// file is the source of truth used by the quest editors to display
    /// (and pick) the correct prize variant.
    /// </para>
    /// </summary>
    public sealed class ItemLevelCatalog
    {
        /// <summary>Single per-level item entry.</summary>
        public sealed class Entry
        {
            public int Type;
            public int Index;
            public int Level;
            public string Name;
        }

        // Composite key combining ItemCatalog.Key(type, index) with the
        // level. Levels in MU never exceed a single byte in practice; we
        // shift by 16 to leave room while staying within long range.
        private readonly Dictionary<long, string> _byKey = new Dictionary<long, string>();
        // Per (type,index) sorted list of available levels. Exposed so that
        // UI level pickers can enumerate exactly the variants present in
        // the file (and not e.g. show "+4" when only 0/3/5/8 exist).
        private readonly Dictionary<long, List<int>> _levelsByItem = new Dictionary<long, List<int>>();
        private readonly List<Entry> _entries = new List<Entry>();

        /// <summary>All loaded entries in file order.</summary>
        public IReadOnlyList<Entry> Entries { get { return _entries; } }

        /// <summary>True when at least one row was successfully loaded.</summary>
        public bool HasData { get { return _byKey.Count > 0; } }

        private static long Key(int type, int index, int level)
        {
            return (ItemCatalog.Key(type, index) << 16) | (uint)(level & 0xFFFF);
        }

        /// <summary>
        /// Returns the named variant for <paramref name="type"/>/
        /// <paramref name="index"/>/<paramref name="level"/> when present
        /// in the file; otherwise <c>null</c>.
        /// </summary>
        public string TryGetName(int type, int index, int level)
        {
            string name;
            return _byKey.TryGetValue(Key(type, index, level), out name) ? name : null;
        }

        /// <summary>
        /// Returns the sorted list of known levels for the given
        /// type/index, or an empty list when the item is not in the file.
        /// </summary>
        public IReadOnlyList<int> GetKnownLevels(int type, int index)
        {
            List<int> list;
            if (_levelsByItem.TryGetValue(ItemCatalog.Key(type, index), out list))
                return list;
            return System.Array.Empty<int>();
        }

        /// <summary>
        /// Loads <c>Item_level.txt</c> at the given path. Missing or
        /// malformed files yield an empty catalog rather than throwing.
        /// The file format is one row per line, tab- or whitespace-
        /// separated: <c>ItemType  ItemIndex  ItemLevel  Name</c>.
        /// Lines beginning with <c>//</c> or that are blank are ignored.
        /// </summary>
        public static ItemLevelCatalog Load(string path)
        {
            var cat = new ItemLevelCatalog();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return cat;

            string[] lines;
            try { lines = File.ReadAllLines(path); }
            catch { return cat; }

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                string line = raw.TrimStart();
                if (line.StartsWith("//")) continue;

                // Split on the first 3 tab/whitespace runs so that the
                // name (which can contain spaces) is preserved verbatim.
                int p1, p2, p3;
                if (!FindNextSeparator(line, 0, out p1)) continue;
                if (!FindNextNonSeparator(line, p1, out int s2)) continue;
                if (!FindNextSeparator(line, s2, out p2)) continue;
                if (!FindNextNonSeparator(line, p2, out int s3)) continue;
                if (!FindNextSeparator(line, s3, out p3)) continue;
                if (!FindNextNonSeparator(line, p3, out int s4)) continue;

                int type, index, level;
                if (!int.TryParse(line.Substring(0, p1), out type)) continue;
                if (!int.TryParse(line.Substring(s2, p2 - s2), out index)) continue;
                if (!int.TryParse(line.Substring(s3, p3 - s3), out level)) continue;
                string name = line.Substring(s4).TrimEnd();
                if (string.IsNullOrEmpty(name)) continue;

                cat._byKey[Key(type, index, level)] = name;
                cat._entries.Add(new Entry { Type = type, Index = index, Level = level, Name = name });

                long itemKey = ItemCatalog.Key(type, index);
                List<int> levels;
                if (!cat._levelsByItem.TryGetValue(itemKey, out levels))
                {
                    levels = new List<int>();
                    cat._levelsByItem[itemKey] = levels;
                }
                if (!levels.Contains(level)) levels.Add(level);
            }

            // Keep level lists sorted so UI pickers display them in order.
            foreach (var kv in cat._levelsByItem)
                kv.Value.Sort();

            return cat;
        }

        private static bool FindNextSeparator(string s, int from, out int pos)
        {
            for (int i = from; i < s.Length; i++)
            {
                if (s[i] == '\t' || s[i] == ' ') { pos = i; return true; }
            }
            pos = -1;
            return false;
        }

        private static bool FindNextNonSeparator(string s, int from, out int pos)
        {
            for (int i = from; i < s.Length; i++)
            {
                if (s[i] != '\t' && s[i] != ' ') { pos = i; return true; }
            }
            pos = -1;
            return false;
        }
    }
}
