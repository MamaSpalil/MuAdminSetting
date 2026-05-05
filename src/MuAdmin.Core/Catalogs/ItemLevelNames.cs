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
        private static readonly Dictionary<long, string[]> _names = new Dictionary<long, string[]>
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
            if (level <= 0) return baseName;

            string[] table;
            if (_names.TryGetValue(ItemCatalog.Key(type, index), out table)
                && level < table.Length)
                return table[level];

            return baseName + " +" + level;
        }

        /// <summary>
        /// Returns <c>true</c> when this item has any extra level-specific
        /// display names beyond level 0 (e.g. <c>Box of Luck</c>). Used by
        /// the UI to decide whether to surface a level picker.
        /// </summary>
        public static bool HasLevelVariants(int type, int index)
        {
            string[] table;
            return _names.TryGetValue(ItemCatalog.Key(type, index), out table)
                   && table.Length > 1;
        }

        /// <summary>Returns the highest known level for an item, or <c>0</c> when only the base name is known.</summary>
        public static int MaxKnownLevel(int type, int index)
        {
            string[] table;
            return _names.TryGetValue(ItemCatalog.Key(type, index), out table) ? table.Length - 1 : 0;
        }
    }
}
