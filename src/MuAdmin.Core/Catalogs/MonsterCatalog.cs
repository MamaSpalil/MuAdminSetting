using System.Collections.Generic;
using System.IO;
using MuAdmin.Core.Models;
using MuAdmin.Core.Parsers;

namespace MuAdmin.Core.Catalogs
{
    /// <summary>
    /// Loads <c>Monsters\Monster.txt</c> into an in-memory map of
    /// <c>MonsterIndex → Name</c> for use by the spot editor
    /// (<c>MonsterSetBase*.txt</c>) and by quest / drop editors.
    /// </summary>
    public sealed class MonsterCatalog
    {
        private readonly Dictionary<int, string> _names = new Dictionary<int, string>();

        public string GetName(int index)
        {
            string name;
            return _names.TryGetValue(index, out name) ? name : "Monster " + index;
        }

        public IReadOnlyDictionary<int, string> All { get { return _names; } }

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
                cat._names[idx] = raw;
            }
            return cat;
        }
    }
}
