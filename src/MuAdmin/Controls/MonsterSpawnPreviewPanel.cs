using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MuAdmin.Core;
using MuAdmin.Core.Catalogs;

namespace MuAdmin.Controls
{
    /// <summary>
    /// Боковая панель предпросмотра спотов для редактора
    /// <c>MonsterSetBase*.txt</c>. Показывает картинку текущей карты
    /// (из <c>Maps/&lt;Name&gt;.jpg</c>), сверху рисует прямоугольники
    /// зон спавна по координатам <c>StartX..EndX × StartY..EndY</c> и
    /// подсвечивает выделенный в гриде ряд. Под картой выводит имя
    /// карты, название монстра и ключевую инфу о спавне.
    /// Координаты MU интерпретируются как 0..255 по обеим осям.
    /// </summary>
    public sealed class MonsterSpawnPreviewPanel : UserControl
    {
        private const int MapCoordMax = 255;

        private readonly Panel _imagePanel;
        private readonly Label _captionLabel;
        private readonly Label _infoLabel;
        private readonly PictureBox _monsterIcon;

        private ServerProject _project;
        private MonsterSetBaseLayout _layout;
        private MonsterSetBaseLayout.Spawn _selected;

        public MonsterSpawnPreviewPanel()
        {
            Width = 280;
            Dock = DockStyle.Right;

            _captionLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Padding = new Padding(6, 4, 6, 0),
                Font = new Font(FontFamily.GenericSansSerif, 9f, FontStyle.Bold),
                Text = "Карта: —"
            };

            _imagePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 256,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            _imagePanel.Paint += OnPaintImage;
            _imagePanel.Resize += (s, e) => _imagePanel.Invalidate();

            _monsterIcon = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 64,
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.FromArgb(250, 250, 250)
            };

            _infoLabel = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(6),
                Font = new Font(FontFamily.GenericMonospace, 9f),
                Text = "Выберите строку спавна, чтобы увидеть\nкоординаты зоны на карте."
            };

            // Порядок добавления важен (Dock fill должен быть последним по визуалу,
            // но в WinForms порядок в Controls — обратный к z-order, поэтому
            // добавляем "fill" первым.)
            Controls.Add(_infoLabel);
            Controls.Add(_monsterIcon);
            Controls.Add(_imagePanel);
            Controls.Add(_captionLabel);
        }

        /// <summary>Привязывает менеджер ассетов / каталог монстров.</summary>
        public void Attach(ServerProject project, MonsterSetBaseLayout layout)
        {
            _project = project;
            _layout = layout;
            _selected = null;
            UpdateAll();
        }

        /// <summary>
        /// Обновляет панель под выделенную в гриде строку
        /// (индекс физической строки в <see cref="MuAdmin.Core.Models.TabularFile"/>).
        /// </summary>
        public void SelectByLineIndex(int lineIndex)
        {
            if (_layout == null) { _selected = null; UpdateAll(); return; }
            MonsterSetBaseLayout.Spawn s;
            _layout.ByLineIndex.TryGetValue(lineIndex, out s);
            if (!ReferenceEquals(s, _selected))
            {
                _selected = s;
                UpdateAll();
            }
        }

        private void UpdateAll()
        {
            if (_selected != null)
            {
                _captionLabel.Text = "Карта: " + MapNames.Get(_selected.MapNumber)
                                                + "  (#" + _selected.MapNumber + ")";
                if (_project?.Assets != null)
                    _monsterIcon.Image = _project.Assets.GetMonster(_selected.MonsterIndex);
                string mname = _project?.Monsters != null
                    ? _project.Monsters.GetName(_selected.MonsterIndex)
                    : ("Monster " + _selected.MonsterIndex);
                bool hasModel = _project?.Assets != null && _project.Assets.HasMonsterModel(_selected.MonsterIndex);
                _infoLabel.Text = string.Format(
                    "Монстр: {0}\n" +
                    "Индекс: {1}\n" +
                    "Зона:   ({2},{3}) – ({4},{5})\n" +
                    "Тип:    {6}\n" +
                    "Модель: {7}",
                    mname, _selected.MonsterIndex,
                    _selected.StartX, _selected.StartY, _selected.EndX, _selected.EndY,
                    _selected.IsPoint ? "фикс. точка" : "прямоугольник зоны",
                    hasModel ? "BMD есть" : "нет .bmd");
            }
            else
            {
                _captionLabel.Text = "Карта: —";
                _monsterIcon.Image = null;
                _infoLabel.Text = "Выберите строку спавна, чтобы увидеть\nкоординаты зоны на карте.";
            }
            _imagePanel.Invalidate();
        }

        private void OnPaintImage(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var bounds = _imagePanel.ClientRectangle;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_selected == null || _project?.Assets == null)
            {
                using (var b = new SolidBrush(_imagePanel.BackColor)) g.FillRectangle(b, bounds);
                TextRenderer.DrawText(g, "(карта не выбрана)", Font, bounds, Color.Gray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            var img = _project.Assets.GetMap(_selected.MapNumber);
            // Вписываем картинку в панель с сохранением пропорций.
            var dst = FitRect(img.Size, bounds);
            g.DrawImage(img, dst);

            // Все спавны этой карты — серым; выбранный — ярким.
            if (_layout != null)
            {
                using (var dimPen = new Pen(Color.FromArgb(140, 60, 90, 200), 1f))
                using (var dimFill = new SolidBrush(Color.FromArgb(40, 60, 90, 200)))
                using (var hotPen = new Pen(Color.FromArgb(220, 220, 40, 40), 2f))
                using (var hotFill = new SolidBrush(Color.FromArgb(80, 220, 40, 40)))
                {
                    foreach (var s in _layout.Spawns)
                    {
                        if (s.MapNumber != _selected.MapNumber) continue;
                        var r = SpawnRect(s, dst);
                        if (s == _selected) continue; // нарисуем поверх
                        if (s.IsPoint)
                        {
                            g.FillEllipse(dimFill, r);
                            g.DrawEllipse(dimPen, r);
                        }
                        else
                        {
                            g.FillRectangle(dimFill, r);
                            g.DrawRectangle(dimPen, r.X, r.Y, r.Width, r.Height);
                        }
                    }
                    var hr = SpawnRect(_selected, dst);
                    if (_selected.IsPoint)
                    {
                        g.FillEllipse(hotFill, hr);
                        g.DrawEllipse(hotPen, hr);
                    }
                    else
                    {
                        g.FillRectangle(hotFill, hr);
                        g.DrawRectangle(hotPen, hr.X, hr.Y, hr.Width, hr.Height);
                    }
                }
            }
        }

        private static Rectangle FitRect(Size src, Rectangle dst)
        {
            if (src.Width <= 0 || src.Height <= 0) return dst;
            float ar = (float)src.Width / src.Height;
            float dar = (float)dst.Width / dst.Height;
            int w, h;
            if (ar >= dar) { w = dst.Width; h = (int)(w / ar); }
            else { h = dst.Height; w = (int)(h * ar); }
            return new Rectangle(dst.X + (dst.Width - w) / 2, dst.Y + (dst.Height - h) / 2, w, h);
        }

        private static Rectangle SpawnRect(MonsterSetBaseLayout.Spawn s, Rectangle dst)
        {
            int x1 = System.Math.Min(s.StartX, s.EndX);
            int y1 = System.Math.Min(s.StartY, s.EndY);
            int x2 = System.Math.Max(s.StartX, s.EndX);
            int y2 = System.Math.Max(s.StartY, s.EndY);
            float fx = (float)dst.Width / (MapCoordMax + 1);
            float fy = (float)dst.Height / (MapCoordMax + 1);
            int rx = dst.X + (int)(x1 * fx);
            int ry = dst.Y + (int)(y1 * fy);
            int rw = System.Math.Max(4, (int)((x2 - x1 + 1) * fx));
            int rh = System.Math.Max(4, (int)((y2 - y1 + 1) * fy));
            return new Rectangle(rx, ry, rw, rh);
        }
    }
}
