using System.IO;
using System.Text;

namespace MuAdmin.Core
{
    /// <summary>
    /// Auto-detection of the original text encoding of a server configuration
    /// file. MU Online files are typically Windows-1251 (Russian comments) or
    /// UTF-8 (with or without BOM). We honour the original encoding when saving
    /// so that the game server keeps reading the file correctly.
    /// </summary>
    public static class EncodingHelper
    {
        public sealed class DetectionResult
        {
            public Encoding Encoding { get; }
            public bool HasBom { get; }
            public string NewLine { get; }

            public DetectionResult(Encoding encoding, bool hasBom, string newLine)
            {
                Encoding = encoding;
                HasBom = hasBom;
                NewLine = newLine;
            }
        }

        public static DetectionResult Detect(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return Detect(bytes);
        }

        public static DetectionResult Detect(byte[] bytes)
        {
            // BOM checks
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return new DetectionResult(new UTF8Encoding(true), true, DetectNewLine(bytes));
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return new DetectionResult(Encoding.Unicode, true, DetectNewLine(bytes));
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return new DetectionResult(Encoding.BigEndianUnicode, true, DetectNewLine(bytes));

            // Heuristic: try to decode as UTF-8 strictly. If it fails on any
            // non-ASCII byte, fall back to Windows-1251 which is the historical
            // default for IGCN/MuEMU server text files with Russian comments.
            if (LooksLikeValidUtf8(bytes))
                return new DetectionResult(new UTF8Encoding(false), false, DetectNewLine(bytes));

            Encoding cp1251;
            try { cp1251 = Encoding.GetEncoding(1251); }
            catch { cp1251 = Encoding.Default; }
            return new DetectionResult(cp1251, false, DetectNewLine(bytes));
        }

        public static string ReadAllText(string path, out DetectionResult detection)
        {
            detection = Detect(path);
            byte[] bytes = File.ReadAllBytes(path);
            int offset = 0;
            if (detection.HasBom)
            {
                if (detection.Encoding is UTF8Encoding) offset = 3;
                else offset = 2;
            }
            return detection.Encoding.GetString(bytes, offset, bytes.Length - offset);
        }

        public static void WriteAllText(string path, string text, DetectionResult detection)
        {
            byte[] body = detection.Encoding.GetBytes(text);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                if (detection.HasBom)
                {
                    byte[] preamble = detection.Encoding.GetPreamble();
                    if (preamble != null && preamble.Length > 0)
                        fs.Write(preamble, 0, preamble.Length);
                }
                fs.Write(body, 0, body.Length);
            }
        }

        private static string DetectNewLine(byte[] bytes)
        {
            int max = bytes.Length < 4096 ? bytes.Length : 4096;
            for (int i = 0; i < max; i++)
            {
                if (bytes[i] == 0x0A)
                    return (i > 0 && bytes[i - 1] == 0x0D) ? "\r\n" : "\n";
            }
            return "\r\n";
        }

        private static bool LooksLikeValidUtf8(byte[] bytes)
        {
            int i = 0;
            bool hasMultibyte = false;
            while (i < bytes.Length)
            {
                byte b = bytes[i];
                if (b < 0x80) { i++; continue; }
                hasMultibyte = true;
                int extra;
                if ((b & 0xE0) == 0xC0) extra = 1;
                else if ((b & 0xF0) == 0xE0) extra = 2;
                else if ((b & 0xF8) == 0xF0) extra = 3;
                else return false;
                if (i + extra >= bytes.Length) return false;
                for (int j = 1; j <= extra; j++)
                    if ((bytes[i + j] & 0xC0) != 0x80) return false;
                i += extra + 1;
            }
            // Pure ASCII files: treat as UTF-8 (round-trip is identical).
            return hasMultibyte || bytes.Length > 0;
        }
    }
}
