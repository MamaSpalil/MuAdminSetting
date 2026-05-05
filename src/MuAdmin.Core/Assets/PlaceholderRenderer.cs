using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace MuAdmin.Core.Assets
{
    /// <summary>
    /// Рисует аккуратные плейсхолдеры для отсутствующих ассетов
    /// (картинок монстров / карт / предметов). Цвет фона детерминирован
    /// по ключу — одинаковый объект получит одинаковый оттенок между
    /// запусками. Используется <see cref="AssetsManager"/> когда файла
    /// картинки нет или формат не поддерживается (например, BMD-модели).
    /// </summary>
    public static class PlaceholderRenderer
    {
        /// <summary>
        /// Создаёт квадратный плейсхолдер указанного размера с
        /// центрированной короткой подписью (например, "M:25" или
        /// "Lorencia"). Caller владеет возвращённым <see cref="Bitmap"/>.
        /// </summary>
        public static Bitmap Render(string key, string label, int size)
        {
            if (size < 8) size = 8;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                Color back = PastelFromKey(key ?? string.Empty);
                Color border = Darker(back, 0.2f);
                using (var brush = new SolidBrush(back))
                    g.FillRectangle(brush, 0, 0, size, size);
                using (var pen = new Pen(border))
                    g.DrawRectangle(pen, 0, 0, size - 1, size - 1);

                if (!string.IsNullOrEmpty(label))
                {
                    float fontSize = System.Math.Max(7f, size / 5f);
                    using (var font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var fg = new SolidBrush(Darker(back, 0.6f)))
                    {
                        g.DrawString(label, font, fg, new RectangleF(0, 0, size, size), fmt);
                    }
                }
            }
            return bmp;
        }

        /// <summary>
        /// Стабильный пастельный цвет из ключа (FNV-1a 32-bit).
        /// </summary>
        private static Color PastelFromKey(string key)
        {
            uint hash = 2166136261;
            for (int i = 0; i < key.Length; i++)
            {
                hash ^= key[i];
                hash *= 16777619;
            }
            // HSV with high value, medium-low saturation для пастельных тонов.
            float h = (hash % 360u);
            return FromHsv(h, 0.35f, 0.92f);
        }

        /// <summary>
        /// Затемняет цвет на долю <paramref name="amount"/> в [0..1].
        /// Аналог <c>ControlPaint.Dark</c>, но без зависимости от
        /// <c>System.Windows.Forms</c>, чтобы Core оставался UI-нейтральным.
        /// </summary>
        private static Color Darker(Color c, float amount)
        {
            if (amount < 0) amount = 0;
            if (amount > 1) amount = 1;
            float k = 1f - amount;
            int r = (int)System.Math.Round(c.R * k);
            int g = (int)System.Math.Round(c.G * k);
            int b = (int)System.Math.Round(c.B * k);
            return Color.FromArgb(c.A, r, g, b);
        }

        private static Color FromHsv(float h, float s, float v)
        {
            int hi = (int)(h / 60f) % 6;
            float f = h / 60f - (int)(h / 60f);
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
            float r = 0, g = 0, b = 0;
            switch (hi)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                case 5: r = v; g = p; b = q; break;
            }
            return Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }
}
