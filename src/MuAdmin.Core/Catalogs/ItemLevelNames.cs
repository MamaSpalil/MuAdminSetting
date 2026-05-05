using System.Collections.Generic;

namespace MuAdmin.Core.Catalogs
{
    /// <summary>
    /// Helper for items whose in-game display name varies by item level.
    /// <para>
    /// In MU Online, a few special "Misc" items keep the same
    /// <c>Type/Index</c> pair across multiple levels but render under
    /// different names — most notably <c>Box of Luck</c> (14, 11), which
    /// becomes <c>Box of Kundun +1..+5</c> at higher levels, and the
    /// scroll-of-emperor / chocolate boxes that follow the same pattern.
    /// <see cref="ItemCatalog"/> only stores the base name from
    /// <c>Item.txt</c>, so this small lookup gives the UI a way to show
    /// the correct level-aware name in pickers (e.g. the quick-add quest
    /// dialog's drop-down for <c>PrizeType = Items</c>).
    /// </para>
    /// </summary>
    public static class ItemLevelNames
    {
        // Per-level names keyed by ItemCatalog.Key(type, index). The list
        // is indexed by item level (0 = base). Only items with a known
        // canonical naming sequence are listed here; everything else falls
        // back to "<BaseName> +N" via DisplayName().
        private static readonly Dictionary<long, string[]> _builtIn = new Dictionary<long, string[]>
        {
            // 14/11 — Box of Luck → Box of Kundun +1..+5 (vanilla MU).
            { ItemCatalog.Key(14, 11), new[]
                {
                    "Box of Luck",
                    "Box of Kundun +1",
                    "Box of Kundun +2",
                    "Box of Kundun +3",
                    "Box of Kundun +4",
                    "Box of Kundun +5"
                }
            },
            // 14/12 — Heart of Love at higher levels.
            { ItemCatalog.Key(14, 12), new[]
                {
                    "Heart",
                    "Heart of Love"
                }
            },
            // 14/29 — Symbol of Kundun pieces 1..5.
            { ItemCatalog.Key(14, 29), new[]
                {
                    "Symbol of Kundun",
                    "Symbol of Kundun (1/5)",
                    "Symbol of Kundun (2/5)",
                    "Symbol of Kundun (3/5)",
                    "Symbol of Kundun (4/5)",
                    "Symbol of Kundun (5/5)"
                }
            },
            // 14/52 — Cherry Blossom Box (Spring event boxes).
            { ItemCatalog.Key(14, 52), new[]
                {
                    "Cherry Blossom Playing Box",
                    "Cherry Blossom Box +1",
                    "Cherry Blossom Box +2"
                }
            },
            // 13/15 — Invisibility Cloak / Cape of Lord upgrade tiers.
            { ItemCatalog.Key(13, 30), new[]
                {
                    "Box of Heaven",
                    "Box of Heaven +1",
                    "Box of Heaven +2"
                }
            }
        };

        // Names sourced from a project-loaded Item_level.txt. Replaces the
        // built-in entries for any item that appears in the file so the
        // server's customised naming wins over the vanilla defaults.
        private static Dictionary<long, string[]> _projectNames = new Dictionary<long, string[]>();

        /// <summary>
        /// Replaces the project-level overrides with names from the given
        /// <see cref="ItemLevelCatalog"/>. Pass <c>null</c> or an empty
        /// catalog to clear them and fall back to the built-in table only.
        /// </summary>
        public static void Apply(ItemLevelCatalog catalog)
        {
            var fresh = new Dictionary<long, string[]>();
            if (catalog != null && catalog.HasData)
            {
                // Group entries by (type,index) and build a dense
                // 0..maxLevel array, leaving gaps as null so the
                // DisplayName fallback ("BaseName +N") still applies.
                var byItem = new Dictionary<long, List<ItemLevelCatalog.Entry>>();
                foreach (var e in catalog.Entries)
                {
                    long k = ItemCatalog.Key(e.Type, e.Index);
                    List<ItemLevelCatalog.Entry> list;
                    if (!byItem.TryGetValue(k, out list))
                    {
                        list = new List<ItemLevelCatalog.Entry>();
                        byItem[k] = list;
                    }
                    list.Add(e);
                }
                foreach (var kv in byItem)
                {
                    int max = 0;
                    foreach (var e in kv.Value) if (e.Level > max) max = e.Level;
                    var arr = new string[max + 1];
                    foreach (var e in kv.Value) arr[e.Level] = e.Name;
                    fresh[kv.Key] = arr;
                }
            }
            _projectNames = fresh;
        }

        /// <summary>
        /// Returns a level-aware display name for the given item. When a
        /// canonical level-named entry exists for <paramref name="type"/>/
        /// <paramref name="index"/>/<paramref name="level"/>, that exact
        /// name is returned; otherwise the helper falls back to
        /// <c>"BaseName +Level"</c> (or just <c>BaseName</c> for level 0).
        /// </summary>
        public static string DisplayName(string baseName, int type, int index, int level)
        {
            if (string.IsNullOrEmpty(baseName)) baseName = "Item " + type + "/" + index;
            if (level < 0) return baseName;

            string named = LookupName(type, index, level);
            if (named != null) return named;
            if (level == 0) return baseName;
            return baseName + " +" + level;
        }

        /// <summary>
        /// Returns <c>true</c> when this item has any extra level-specific
        /// display names beyond level 0 (e.g. <c>Box of Luck</c>). Used by
        /// the UI to decide whether to surface a level picker.
        /// </summary>
        public static bool HasLevelVariants(int type, int index)
        {
            long k = ItemCatalog.Key(type, index);
            string[] table;
            if (_projectNames.TryGetValue(k, out table) && CountNonEmpty(table) > 1) return true;
            if (_builtIn.TryGetValue(k, out table) && table.Length > 1) return true;
            return false;
        }

        /// <summary>Returns the highest known level for an item, or <c>0</c> when only the base name is known.</summary>
        public static int MaxKnownLevel(int type, int index)
        {
            long k = ItemCatalog.Key(type, index);
            int max = 0;
            string[] table;
            if (_projectNames.TryGetValue(k, out table) && table.Length - 1 > max) max = table.Length - 1;
            if (_builtIn.TryGetValue(k, out table) && table.Length - 1 > max) max = table.Length - 1;
            return max;
        }

        private static string LookupName(int type, int index, int level)
        {
            long k = ItemCatalog.Key(type, index);
            string[] table;
            if (_projectNames.TryGetValue(k, out table)
                && level < table.Length
                && !string.IsNullOrEmpty(table[level]))
                return table[level];
            if (_builtIn.TryGetValue(k, out table)
                && level < table.Length
                && !string.IsNullOrEmpty(table[level]))
                return table[level];
            return null;
        }

        private static int CountNonEmpty(string[] arr)
        {
            int n = 0;
            for (int i = 0; i < arr.Length; i++) if (!string.IsNullOrEmpty(arr[i])) n++;
            return n;
        }
    }
}
