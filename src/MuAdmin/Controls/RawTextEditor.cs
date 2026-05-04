using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MuAdmin.Core;
using MuAdmin.Tabs;

namespace MuAdmin.Controls
{
    /// <summary>
    /// Fallback editor used for files without a dedicated structured view
    /// (notably <c>.dat</c> binaries and unknown formats). Text-looking files
    /// are shown in a <see cref="TextBox"/>; binary files are shown as a
    /// read-only hex dump.
    /// </summary>
    public partial class RawTextEditor : UserControl, IFileEditor
    {
        private string _path;
        private EncodingHelper.DetectionResult _enc;
        private byte[] _bytes;
        private bool _binary;

        public RawTextEditor() { InitializeComponent(); }

        public Control AsControl { get { return this; } }
        public string FilePath { get { return _path; } }
        public bool IsDirty { get; private set; }

        public new void Load(string path, ServerProject project)
        {
            _path = path;
            _bytes = File.ReadAllBytes(path);
            _binary = LooksBinary(_bytes);
            if (_binary)
            {
                _enc = null;
                _text.ReadOnly = true;
                _text.Text = ToHex(_bytes);
                _statusLabel.Text = Path.GetFileName(path) + "  •  бинарный (read-only hex)";
            }
            else
            {
                _enc = EncodingHelper.Detect(_bytes);
                int offset = _enc.HasBom ? _enc.Encoding.GetPreamble().Length : 0;
                _text.ReadOnly = false;
                _text.Text = _enc.Encoding.GetString(_bytes, offset, _bytes.Length - offset);
                _statusLabel.Text = Path.GetFileName(path) + "  •  без изменений";
            }
            IsDirty = false;
        }

        public void Save(ServerProject project)
        {
            if (_binary || _path == null) return;
            if (project != null) BackupManager.CreateBackup(project.RootPath, _path);
            EncodingHelper.WriteAllText(_path, _text.Text, _enc);
            IsDirty = false;
            _statusLabel.Text = Path.GetFileName(_path) + "  •  сохранён";
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (_binary) return;
            IsDirty = true;
            _statusLabel.Text = Path.GetFileName(_path) + "  •  изменён";
        }

        private static bool LooksBinary(byte[] b)
        {
            if (b == null || b.Length == 0) return false;
            int max = b.Length < 4096 ? b.Length : 4096;
            int nonText = 0;
            for (int i = 0; i < max; i++)
            {
                byte v = b[i];
                if (v == 0) return true;
                if (v < 0x09 || (v > 0x0D && v < 0x20)) nonText++;
            }
            return nonText * 10 > max;
        }

        private static string ToHex(byte[] b)
        {
            var sb = new StringBuilder(b.Length * 4);
            for (int i = 0; i < b.Length; i += 16)
            {
                sb.AppendFormat("{0:X8}: ", i);
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < b.Length) sb.AppendFormat("{0:X2} ", b[i + j]);
                    else sb.Append("   ");
                }
                sb.Append(' ');
                for (int j = 0; j < 16 && i + j < b.Length; j++)
                {
                    byte v = b[i + j];
                    sb.Append(v >= 0x20 && v < 0x7F ? (char)v : '.');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
