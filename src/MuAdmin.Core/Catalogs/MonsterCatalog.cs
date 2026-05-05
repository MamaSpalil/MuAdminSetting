using System.Collections.Generic;
using System.IO;
using MuAdmin.Core.Models;
using MuAdmin.Core.Parsers;

namespace MuAdmin.Core.Catalogs
{
    /// <summary>
    /// Loads <c>Monsters\Monster.txt</c> into an in-memory map of
    /// <c>MonsterIndex → (Name + key stats)</c> for use by the spot
    /// editor (<c>MonsterSetBase*.txt</c>) and by quest / drop editors.
    /// Stats are sourced from the documented column order:
    /// <c>Numb, Rate, Name, Level, Heal, Mana, MinDmg, MaxDmg, Def, ...</c>
    /// </summary>
    public sealed class MonsterCatalog
    {
        /// <summary>Key stats extracted from a <c>Monster.txt</c> row.</summary>
        public sealed class Stats
        {
            public string Name;
            public int Level;
            /// <summary>HP — column "Heal" in <c>Monster.txt</c>.</summary>
            public int Hp;
            public int Mana;
            public int MinDmg;
            public int MaxDmg;
            public int Def;
        }

        private readonly Dictionary<int, Stats> _stats = new Dictionary<int, Stats>();

        public string GetName(int index)
        {
            Stats s;
            return _stats.TryGetValue(index, out s) ? s.Name : "Monster " + index;
        }

        /// <summary>Returns extracted stats for the monster, or <c>null</c> if unknown.</summary>
        public Stats GetStats(int index)
        {
            Stats s;
            return _stats.TryGetValue(index, out s) ? s : null;
        }

        public IEnumerable<KeyValuePair<int, Stats>> AllStats { get { return _stats; } }

        /// <summary>
        /// Backwards-compatible name-only view for callers that don't need
        /// the full <see cref="Stats"/> record.
        /// </summary>
        public IReadOnlyDictionary<int, string> All
        {
            get
            {
                var d = new Dictionary<int, string>(_stats.Count);
                foreach (var kv in _stats) d[kv.Key] = kv.Value.Name;
                return d;
            }
        }

        public static MonsterCatalog Load(string monsterTxtPath)
        {
            var cat = new MonsterCatalog();
            if (string.IsNullOrEmpty(monsterTxtPath) || !File.Exists(monsterTxtPath))
                return cat;

            TabularFile tf;
            try { tf = TabularFileParser.Parse(monsterTxtPath); }
            catch { return cat; }

            foreach (var l in tf.Lines)
            {
                if (l.Kind != LineKind.Data || l.Cells.Count < 3) continue;
                int idx;
                if (!int.TryParse(l.Cells[0], out idx)) continue;
                // Column 2 is "Name" (Numb, Rate, Name, ...).
                string raw = l.Cells[2];
                if (raw.Length >= 2 && raw[0] == '"' && raw[raw.Length - 1] == '"')
                    raw = raw.Substring(1, raw.Length - 2);

                var s = new Stats { Name = raw };
                ParseInt(l.Cells, 3, out s.Level);
                ParseInt(l.Cells, 4, out s.Hp);
                ParseInt(l.Cells, 5, out s.Mana);
                ParseInt(l.Cells, 6, out s.MinDmg);
                ParseInt(l.Cells, 7, out s.MaxDmg);
                ParseInt(l.Cells, 8, out s.Def);
                cat._stats[idx] = s;
            }
            return cat;
        }

        private static void ParseInt(IList<string> cells, int col, out int value)
        {
            value = 0;
            if (col < cells.Count) int.TryParse(cells[col], out value);
        }
    }
}
