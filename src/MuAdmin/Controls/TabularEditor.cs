using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using MuAdmin.Core;
using MuAdmin.Core.Models;
using MuAdmin.Core.Parsers;
using MuAdmin.Tabs;

namespace MuAdmin.Controls
{
    /// <summary>
    /// Editor for whitespace-separated MU server text files. Renders all
    /// data rows in a <see cref="DataGridView"/> with one column per cell;
    /// comment and blank lines remain in the model and are preserved on save.
    /// </summary>
    public partial class TabularEditor : UserControl, IFileEditor
    {
        private TabularFile _file;
        private DataTable _table;
        private bool _suppressEvents;

        public TabularEditor() { InitializeComponent(); }

        public Control AsControl { get { return this; } }
        public string FilePath { get { return _file != null ? _file.Path : null; } }
        public bool IsDirty { get; private set; }

        public new void Load(string path, ServerProject project)
        {
            _file = TabularFileParser.Parse(path);
            BuildTable();
            IsDirty = false;
            UpdateStatus();
        }

        public void Save(ServerProject project)
        {
            if (_file == null) return;
            CommitTable();
            if (project != null)
                BackupManager.CreateBackup(project.RootPath, _file.Path);
            TabularFileParser.Save(_file);
            IsDirty = false;
            UpdateStatus();
        }

        private void BuildTable()
        {
            _suppressEvents = true;
            try
            {
                int maxCols = 1;
                foreach (var l in _file.Lines)
                    if (l.Kind == LineKind.Data && l.Cells.Count > maxCols)
                        maxCols = l.Cells.Count;

                _table = new DataTable();
                _table.Columns.Add("#", typeof(int));
                _table.Columns.Add("Comment / End", typeof(string));
                for (int i = 0; i < maxCols; i++)
                    _table.Columns.Add("c" + (i + 1), typeof(string));
                _table.Columns.Add("// trailing", typeof(string));

                int lineIdx = 0;
                foreach (var l in _file.Lines)
                {
                    var row = _table.NewRow();
                    row[0] = lineIdx++;
                    if (l.Kind == LineKind.Data)
                    {
                        row[1] = string.Empty;
                        for (int i = 0; i < l.Cells.Count; i++) row[2 + i] = l.Cells[i];
                        row[_table.Columns.Count - 1] = l.TrailingComment;
                    }
                    else
                    {
                        row[1] = l.OriginalText ?? string.Empty;
                    }
                    _table.Rows.Add(row);
                }
                _grid.DataSource = _table;
                if (_grid.Columns.Count > 0) _grid.Columns[0].ReadOnly = true;
                if (_grid.Columns.Count > 1) _grid.Columns[1].ReadOnly = true; // comment column read-only
            }
            finally { _suppressEvents = false; }
        }

        private void CommitTable()
        {
            for (int r = 0; r < _table.Rows.Count; r++)
            {
                if (r >= _file.Lines.Count) break;
                var l = _file.Lines[r];
                if (l.Kind != LineKind.Data) continue;

                bool changed = false;
                int dataColCount = _table.Columns.Count - 3; // minus #, Comment, trailing
                for (int i = 0; i < dataColCount; i++)
                {
                    string newVal = _table.Rows[r][2 + i] as string ?? string.Empty;
                    string oldVal = i < l.Cells.Count ? l.Cells[i] : string.Empty;
                    if (newVal != oldVal) changed = true;
                }
                string newTrail = _table.Rows[r][_table.Columns.Count - 1] as string ?? string.Empty;
                if (newTrail != (l.TrailingComment ?? string.Empty)) changed = true;
                if (!changed) continue;

                l.Cells.Clear();
                for (int i = 0; i < dataColCount; i++)
                {
                    string v = _table.Rows[r][2 + i] as string ?? string.Empty;
                    if (i >= 1 && string.IsNullOrEmpty(v) && AllRemainingEmpty(r, i + 1, dataColCount))
                        break;
                    l.Cells.Add(v);
                }
                l.TrailingComment = newTrail;
                l.Modified = true;
            }
        }

        private bool AllRemainingEmpty(int row, int startCol, int dataColCount)
        {
            for (int i = startCol; i < dataColCount; i++)
                if (!string.IsNullOrEmpty(_table.Rows[row][2 + i] as string)) return false;
            return true;
        }

        private void OnCellChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressEvents) return;
            IsDirty = true;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            _statusLabel.Text = (_file == null ? "" : Path.GetFileName(_file.Path))
                + (IsDirty ? "  •  изменён" : "  •  без изменений");
        }
    }
}
