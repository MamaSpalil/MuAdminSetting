using System.Collections.Generic;
using MuAdmin.Core.Models;

namespace MuAdmin.Core.Catalogs
{
    /// <summary>
    /// Логический разбор файла спотов <c>MonsterSetBase*.txt</c> поверх
    /// уже распарсенного <see cref="TabularFile"/>. В отличие от
    /// плоского <see cref="TabularFile"/>, эта структура группирует
    /// строки в блоки <c>map → список спавнов</c> и для каждого спавна
    /// выделяет координаты прямоугольника зоны (<c>StartX, StartY,
    /// EndX, EndY</c>) и индекс монстра.
    ///
    /// Формат сектора (см. <c>Monsters/MonsterSetBase.txt</c>):
    /// <code>
    ///   3                                 // ← заголовок секции (НЕ номер карты!)
    ///   43  3  30  10  10  240  240 -1 3 // ← спавн (9 столбцов): MonsterIdx, Map, Dist, sx,sy,ex,ey, Element, Count
    ///   200 6  6   63  160 1             // ← фикс. NPC (6 столбцов): MonsterIdx, Map, Dist, X, Y, Count
    ///   end                              // ← терминатор
    /// </code>
    /// Кроме того, поддерживается формат <c>KanturuMonsterSetBase.txt</c>
    /// (7 колонок с ведущим порядковым номером):
    /// <code>
    ///   0  364  39  0  188  110  -1      // ← Seq, MonsterIdx, Map, Dist, X, Y, Count
    /// </code>
    /// Номер карты всегда читается из строки спавна — заголовок секции
    /// (одна целочисленная ячейка) — это просто идентификатор группы и
    /// игнорируется. Координаты MU — целые в диапазоне <c>0..255</c>
    /// по обеим осям.
    /// </summary>
    public sealed class MonsterSetBaseLayout
    {
        /// <summary>Описание одной строки спавна.</summary>
        public sealed class Spawn
        {
            /// <summary>Индекс соответствующей <see cref="TabularLine"/> в исходном файле.</summary>
            public int LineIndex;
            /// <summary>Номер карты, к которой относится спавн (см. <see cref="MapNames"/>).</summary>
            public int MapNumber;
            /// <summary>Индекс монстра (первый столбец).</summary>
            public int MonsterIndex;
            /// <summary>Координаты прямоугольника зоны спавна (0..255).</summary>
            public int StartX, StartY, EndX, EndY;
            /// <summary>True для фиксированной точки (6-столбцовый формат NPC).</summary>
            public bool IsPoint;
        }

        /// <summary>Все спавны файла в исходном порядке.</summary>
        public List<Spawn> Spawns { get; } = new List<Spawn>();

        /// <summary>Карта <c>LineIndex → Spawn</c> для быстрого поиска по выделенной строке.</summary>
        public Dictionary<int, Spawn> ByLineIndex { get; } = new Dictionary<int, Spawn>();

        /// <summary>Строит layout из распарсенного <see cref="TabularFile"/>.</summary>
        public static MonsterSetBaseLayout Build(TabularFile file)
        {
            var layout = new MonsterSetBaseLayout();
            if (file == null) return layout;

            for (int i = 0; i < file.Lines.Count; i++)
            {
                var l = file.Lines[i];
                if (l.Kind != LineKind.Data) continue;

                // Заголовок секции: ровно одна целочисленная ячейка.
                // Раньше мы интерпретировали её как номер карты, но в реальном
                // формате MU это просто id группы (Golden Invasion, Arena, ...) —
                // номер карты всегда берётся из самой строки спавна.
                if (l.Cells.Count == 1 && int.TryParse(l.Cells[0], out _))
                    continue;

                var s = TryParseSpawn(l, i);
                if (s == null) continue;
                layout.Spawns.Add(s);
                layout.ByLineIndex[i] = s;
            }
            return layout;
        }

        private static Spawn TryParseSpawn(TabularLine line, int lineIndex)
        {
            int n = line.Cells.Count;
            if (n < 6) return null;

            // Стандартный 9-столбцовый формат (зоны спавна):
            // MonsterIdx, Map, Dist, sx, sy, ex, ey, Element, Count.
            if (n >= 9
                && int.TryParse(line.Cells[0], out int idx9)
                && int.TryParse(line.Cells[1], out int map9)
                && int.TryParse(line.Cells[3], out int sx)
                && int.TryParse(line.Cells[4], out int sy)
                && int.TryParse(line.Cells[5], out int ex)
                && int.TryParse(line.Cells[6], out int ey)
                && InCoordRange(sx) && InCoordRange(sy)
                && InCoordRange(ex) && InCoordRange(ey))
            {
                return new Spawn
                {
                    LineIndex = lineIndex,
                    MapNumber = map9,
                    MonsterIndex = idx9,
                    StartX = sx, StartY = sy, EndX = ex, EndY = ey,
                    IsPoint = false
                };
            }

            // 7-столбцовый Kanturu-формат с ведущим порядковым номером:
            // Seq, MonsterIdx, Map, Dist, X, Y, Count.
            if (n == 7
                && int.TryParse(line.Cells[1], out int idxK)
                && int.TryParse(line.Cells[2], out int mapK)
                && int.TryParse(line.Cells[4], out int kx)
                && int.TryParse(line.Cells[5], out int ky)
                && InCoordRange(kx) && InCoordRange(ky))
            {
                return new Spawn
                {
                    LineIndex = lineIndex,
                    MapNumber = mapK,
                    MonsterIndex = idxK,
                    StartX = kx, StartY = ky, EndX = kx, EndY = ky,
                    IsPoint = true
                };
            }

            // 6-столбцовый формат фикс. NPC: MonsterIdx, Map, Dist, X, Y, Count.
            if (n >= 6
                && int.TryParse(line.Cells[0], out int idx6)
                && int.TryParse(line.Cells[1], out int map6)
                && int.TryParse(line.Cells[3], out int px)
                && int.TryParse(line.Cells[4], out int py)
                && InCoordRange(px) && InCoordRange(py))
            {
                return new Spawn
                {
                    LineIndex = lineIndex,
                    MapNumber = map6,
                    MonsterIndex = idx6,
                    StartX = px, StartY = py, EndX = px, EndY = py,
                    IsPoint = true
                };
            }
            return null;
        }

        private static bool InCoordRange(int v) { return v >= 0 && v <= 255; }
    }
}
