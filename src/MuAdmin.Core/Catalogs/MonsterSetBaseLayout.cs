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
    ///   3                                 // ← заголовок: номер карты
    ///   43  3  30  10  10  240  240 -1 3 // ← спавн (9 столбцов)
    ///   200 6  6   63  160 1             // ← фикс. NPC (6 столбцов)
    ///   end                              // ← терминатор
    /// </code>
    /// Координаты MU — целые в диапазоне <c>0..255</c> по обеим осям.
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

            int currentMap = -1;
            for (int i = 0; i < file.Lines.Count; i++)
            {
                var l = file.Lines[i];
                if (l.Kind == LineKind.BlockMarker)
                {
                    // "end" — закрытие сектора, следующий заголовок
                    // вновь установит currentMap.
                    currentMap = -1;
                    continue;
                }
                if (l.Kind != LineKind.Data) continue;

                // Заголовок сектора: ровно одна ячейка-целое.
                if (l.Cells.Count == 1)
                {
                    int m;
                    if (int.TryParse(l.Cells[0], out m))
                    {
                        currentMap = m;
                        continue;
                    }
                }

                if (currentMap < 0) continue; // данные вне сектора — пропускаем
                var s = TryParseSpawn(l, i, currentMap);
                if (s == null) continue;
                layout.Spawns.Add(s);
                layout.ByLineIndex[i] = s;
            }
            return layout;
        }

        private static Spawn TryParseSpawn(TabularLine line, int lineIndex, int mapNumber)
        {
            if (line.Cells.Count < 5) return null;
            int idx;
            if (!int.TryParse(line.Cells[0], out idx)) return null;

            // Стандартный 9-столбцовый формат: Index, _, _, sx, sy, ex, ey, element, count.
            if (line.Cells.Count >= 7
                && int.TryParse(line.Cells[3], out int sx)
                && int.TryParse(line.Cells[4], out int sy)
                && int.TryParse(line.Cells[5], out int ex)
                && int.TryParse(line.Cells[6], out int ey))
            {
                return new Spawn
                {
                    LineIndex = lineIndex,
                    MapNumber = mapNumber,
                    MonsterIndex = idx,
                    StartX = sx, StartY = sy, EndX = ex, EndY = ey,
                    IsPoint = false
                };
            }

            // 6-столбцовый формат фикс. NPC: Index, _, _, x, y, count.
            if (line.Cells.Count >= 5
                && int.TryParse(line.Cells[3], out int px)
                && int.TryParse(line.Cells[4], out int py))
            {
                return new Spawn
                {
                    LineIndex = lineIndex,
                    MapNumber = mapNumber,
                    MonsterIndex = idx,
                    StartX = px, StartY = py, EndX = px, EndY = py,
                    IsPoint = true
                };
            }
            return null;
        }
    }
}
