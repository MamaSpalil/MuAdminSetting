using System;
using System.IO;
using MuAdmin.Core.Assets;
using MuAdmin.Core.Catalogs;

namespace MuAdmin.Core
{
    /// <summary>
    /// Represents an opened MU server folder. Holds the root path and the
    /// shared catalogs (maps / monsters / items) that are reused across all
    /// editor tabs to render numeric IDs as readable names.
    /// </summary>
    public sealed class ServerProject : IDisposable
    {
        public string RootPath { get; }
        public ItemCatalog Items { get; private set; }
        /// <summary>
        /// Per-level item names loaded from <c>Item_level.txt</c> at the
        /// server root. Used by the quest editors so prizes whose Type/
        /// Index repeats across multiple levels (e.g. 14/11) can be shown
        /// and picked by their proper variant name (Box of Luck / Heart
        /// of Love / Bok of Kundun+1 / ...).
        /// </summary>
        public ItemLevelCatalog ItemLevels { get; private set; }
        public MonsterCatalog Monsters { get; private set; }
        /// <summary>Поставщик визуальных ассетов (карты/монстры) для редакторов.</summary>
        public AssetsManager Assets { get; private set; }

        public ServerProject(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
                throw new ArgumentException("Root path is required.", nameof(rootPath));
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException(rootPath);

            RootPath = Path.GetFullPath(rootPath);
            Assets = new AssetsManager(RootPath);
            ReloadCatalogs();
        }

        public void ReloadCatalogs()
        {
            Items = ItemCatalog.Load(Path.Combine(RootPath, "Items", "Item.txt"));
            ItemLevels = ItemLevelCatalog.Load(Path.Combine(RootPath, "Item_level.txt"));
            Monsters = MonsterCatalog.Load(Path.Combine(RootPath, "Monsters", "Monster.txt"));
            // Make per-level names from Item_level.txt available to the
            // static helper used across the UI (quest editor, quick-add
            // dialog, ...). Hard-coded fallbacks remain for items not
            // present in the file.
            ItemLevelNames.Apply(ItemLevels);
        }

        public string Resolve(params string[] parts)
        {
            string p = RootPath;
            foreach (var part in parts) p = Path.Combine(p, part);
            return p;
        }

        public bool Exists(params string[] parts) { return File.Exists(Resolve(parts)); }
        public bool DirExists(params string[] parts) { return Directory.Exists(Resolve(parts)); }

        /// <summary>
        /// Heuristic check that a folder looks like a real MU server install.
        /// </summary>
        public static bool LooksLikeServerRoot(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;
            int hits = 0;
            string[] markers =
            {
                "CommonServer.cfg", "Items", "Monsters", "Shops", "Quests",
                "Move", "Events", "EventItemBags", "Custom", "Common"
            };
            foreach (var m in markers)
            {
                string p = Path.Combine(path, m);
                if (File.Exists(p) || Directory.Exists(p)) hits++;
            }
            return hits >= 4;
        }

        public void Dispose()
        {
            if (Assets != null) { Assets.Dispose(); Assets = null; }
        }
    }
}
