using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MuAdmin.Core.Catalogs;

namespace MuAdmin.Core.Assets
{
    /// <summary>
    /// Поставщик визуальных ассетов для редакторов: картинок карт
    /// (<c>Maps/&lt;Name&gt;.jpg</c>), картинок монстров и информации
    /// о наличии 3D-моделей (<c>Monster/Monster&lt;NN&gt;.bmd</c>).
    ///
    /// Для карт исходник — JPG, лежащий рядом с конфигами сервера в
    /// папке <c>Maps/</c> (имя файла = название карты из
    /// <see cref="MapNames"/>). Для монстров используется индекс в
    /// имени файла <c>Monster&lt;NN&gt;.bmd</c>; сами BMD-файлы — это
    /// 3D-меши клиента MU и не могут быть отрисованы стандартными
    /// средствами System.Drawing, поэтому при их наличии возвращается
    /// плейсхолдер с подписью <c>"M:N"</c>, а наличие модели можно
    /// проверить через <see cref="HasMonsterModel"/>.
    ///
    /// Менеджер потокобезопасен на чтение, держит LRU-кеш картинок
    /// (по умолчанию 256 элементов) и корректно освобождает их при
    /// <see cref="Dispose"/>.
    /// </summary>
    public sealed class AssetsManager : IDisposable
    {
        private const int DefaultCapacity = 256;

        private readonly string _serverRoot;
        private readonly int _capacity;
        private readonly object _gate = new object();

        // LRU: словарь key→node + двусвязный список (голова = самый свежий).
        private readonly Dictionary<string, LinkedListNode<Entry>> _cache =
            new Dictionary<string, LinkedListNode<Entry>>(StringComparer.Ordinal);
        private readonly LinkedList<Entry> _order = new LinkedList<Entry>();

        private bool _disposed;

        private sealed class Entry
        {
            public string Key;
            public Bitmap Image;
        }

        public AssetsManager(string serverRoot) : this(serverRoot, DefaultCapacity) { }

        public AssetsManager(string serverRoot, int capacity)
        {
            _serverRoot = serverRoot;
            _capacity = capacity > 0 ? capacity : DefaultCapacity;
        }

        /// <summary>Корневая папка сервера, относительно которой ищутся ассеты.</summary>
        public string ServerRoot { get { return _serverRoot; } }

        // ---- Карты ---------------------------------------------------------

        /// <summary>
        /// Возвращает картинку карты по числовому идентификатору
        /// (см. <see cref="MapNames"/>). Если файла нет — рисуется
        /// плейсхолдер с названием карты. Caller'у возвращается
        /// инстанс из кеша, его не нужно (и нельзя) диспозить.
        /// </summary>
        public Image GetMap(int mapNumber)
        {
            string name = MapNames.Get(mapNumber);
            string key = "map:" + mapNumber;
            return GetOrAdd(key, () => LoadMapImage(name) ?? PlaceholderRenderer.Render(key, name, 256));
        }

        /// <summary>Возвращает абсолютный путь к JPG/PNG карты, если он существует.</summary>
        public string FindMapFile(int mapNumber)
        {
            if (string.IsNullOrEmpty(_serverRoot)) return null;
            string name = MapNames.Get(mapNumber);
            string dir = Path.Combine(_serverRoot, "Maps");
            if (!Directory.Exists(dir)) return null;
            string[] exts = { ".jpg", ".jpeg", ".png", ".bmp" };
            foreach (var ext in exts)
            {
                string p = Path.Combine(dir, name + ext);
                if (File.Exists(p)) return p;
            }
            return null;
        }

        private Bitmap LoadMapImage(string name)
        {
            if (string.IsNullOrEmpty(_serverRoot) || string.IsNullOrEmpty(name)) return null;
            string dir = Path.Combine(_serverRoot, "Maps");
            if (!Directory.Exists(dir)) return null;
            string[] exts = { ".jpg", ".jpeg", ".png", ".bmp" };
            foreach (var ext in exts)
            {
                string p = Path.Combine(dir, name + ext);
                if (File.Exists(p))
                {
                    try { return LoadBitmapDetached(p); }
                    catch { return null; }
                }
            }
            return null;
        }

        // ---- Предметы ------------------------------------------------------

        /// <summary>
        /// Returns an icon for the item identified by <paramref name="type"/>
        /// (section in <c>Item.txt</c>) and <paramref name="index"/>. The
        /// manager probes <c>&lt;root&gt;/Items/&lt;type&gt;_&lt;index&gt;.{png,jpg,bmp}</c>
        /// (matching the convention proposed in <c>IMPROVEMENTS_PROMPT.md</c>);
        /// when no file exists a deterministic placeholder labelled
        /// <c>"T:I"</c> is returned. The result is cached and owned by the
        /// manager — callers must not dispose it.
        /// </summary>
        public Image GetItem(int type, int index)
        {
            string key = "item:" + type + "/" + index;
            return GetOrAdd(key, () => LoadItemImage(type, index)
                                       ?? PlaceholderRenderer.Render(key, type + ":" + index, 48));
        }

        /// <summary>True if a raster icon (not just a placeholder) is available for the item.</summary>
        public bool HasItemIcon(int type, int index)
        {
            return FindItemIcon(type, index) != null;
        }

        /// <summary>Returns the absolute path to the item icon file, or <c>null</c>.</summary>
        public string FindItemIcon(int type, int index)
        {
            if (string.IsNullOrEmpty(_serverRoot)) return null;
            string dir = Path.Combine(_serverRoot, "Items");
            if (!Directory.Exists(dir)) return null;
            string stem = type.ToString() + "_" + index.ToString();
            string[] exts = { ".png", ".jpg", ".jpeg", ".bmp" };
            foreach (var e in exts)
            {
                string p = Path.Combine(dir, stem + e);
                if (File.Exists(p)) return p;
            }
            return null;
        }

        private Bitmap LoadItemImage(int type, int index)
        {
            string p = FindItemIcon(type, index);
            if (p == null) return null;
            try { return LoadBitmapDetached(p); }
            catch { return null; }
        }

        // ---- Монстры -------------------------------------------------------

        /// <summary>
        /// Возвращает картинку монстра по индексу. Сейчас сервер
        /// поставляет лишь BMD-модели (не растровые), поэтому
        /// возвращается плейсхолдер с подписью <c>"M:N"</c>; если в
        /// будущем рядом с BMD появится одноимённый PNG/JPG —
        /// загрузится он. Caller не владеет результатом.
        /// </summary>
        public Image GetMonster(int monsterIndex)
        {
            string key = "monster:" + monsterIndex;
            return GetOrAdd(key, () => LoadMonsterImage(monsterIndex)
                                       ?? PlaceholderRenderer.Render(key, "M:" + monsterIndex, 64));
        }

        /// <summary>True, если для индекса есть BMD-модель в <c>Monster/</c>.</summary>
        public bool HasMonsterModel(int monsterIndex)
        {
            return FindMonsterModel(monsterIndex) != null;
        }

        /// <summary>Возвращает абсолютный путь к BMD-модели монстра, если она есть.</summary>
        public string FindMonsterModel(int monsterIndex)
        {
            if (string.IsNullOrEmpty(_serverRoot)) return null;
            string dir = Path.Combine(_serverRoot, "Monster");
            if (!Directory.Exists(dir)) return null;
            // Имена в реальной поставке: Monster01.bmd, Monster25.bmd, Monster123.bmd.
            // Чтобы не зависеть от ширины числа, перебираем 1, 2 и 3-цифровые варианты.
            string[] names =
            {
                "Monster" + monsterIndex.ToString("D2") + ".bmd",
                "Monster" + monsterIndex.ToString() + ".bmd",
                "Monster" + monsterIndex.ToString("D3") + ".bmd"
            };
            foreach (var n in names)
            {
                string p = Path.Combine(dir, n);
                if (File.Exists(p)) return p;
            }
            return null;
        }

        private Bitmap LoadMonsterImage(int monsterIndex)
        {
            if (string.IsNullOrEmpty(_serverRoot)) return null;
            string dir = Path.Combine(_serverRoot, "Monster");
            if (!Directory.Exists(dir)) return null;
            string[] exts = { ".png", ".jpg", ".jpeg", ".bmp" };
            string[] stems =
            {
                "Monster" + monsterIndex.ToString("D2"),
                "Monster" + monsterIndex.ToString(),
                "Monster" + monsterIndex.ToString("D3")
            };
            foreach (var s in stems)
                foreach (var e in exts)
                {
                    string p = Path.Combine(dir, s + e);
                    if (File.Exists(p))
                    {
                        try { return LoadBitmapDetached(p); }
                        catch { return null; }
                    }
                }
            return null;
        }

        // ---- LRU -----------------------------------------------------------

        private Image GetOrAdd(string key, Func<Bitmap> factory)
        {
            lock (_gate)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(AssetsManager));
                LinkedListNode<Entry> node;
                if (_cache.TryGetValue(key, out node))
                {
                    _order.Remove(node);
                    _order.AddFirst(node);
                    return node.Value.Image;
                }
            }

            // Создаём вне lock — загрузка с диска может быть долгой.
            Bitmap created = factory();
            if (created == null)
                created = PlaceholderRenderer.Render(key, "?", 64);

            lock (_gate)
            {
                if (_disposed)
                {
                    created.Dispose();
                    throw new ObjectDisposedException(nameof(AssetsManager));
                }
                LinkedListNode<Entry> existing;
                if (_cache.TryGetValue(key, out existing))
                {
                    // Кто-то успел добавить — выбрасываем своё, возвращаем уже закешированное.
                    created.Dispose();
                    _order.Remove(existing);
                    _order.AddFirst(existing);
                    return existing.Value.Image;
                }
                var entry = new Entry { Key = key, Image = created };
                var added = _order.AddFirst(entry);
                _cache[key] = added;
                EvictIfNeeded();
                return created;
            }
        }

        private void EvictIfNeeded()
        {
            while (_cache.Count > _capacity && _order.Last != null)
            {
                var last = _order.Last;
                _order.RemoveLast();
                _cache.Remove(last.Value.Key);
                if (last.Value.Image != null)
                {
                    try { last.Value.Image.Dispose(); } catch { /* ignore */ }
                }
            }
        }

        /// <summary>
        /// Загружает Bitmap так, чтобы файл на диске не оставался
        /// заблокированным (стандартный <c>Image.FromFile</c> держит
        /// поток открытым до Dispose).
        /// </summary>
        private static Bitmap LoadBitmapDetached(string path)
        {
            using (var src = Image.FromFile(path))
                return new Bitmap(src);
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed) return;
                _disposed = true;
                foreach (var n in _cache.Values)
                {
                    if (n.Value.Image != null)
                    {
                        try { n.Value.Image.Dispose(); } catch { /* ignore */ }
                    }
                }
                _cache.Clear();
                _order.Clear();
            }
        }
    }
}
