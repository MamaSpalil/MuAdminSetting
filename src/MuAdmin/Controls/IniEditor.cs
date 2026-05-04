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
    /// Editor for INI/CFG files. Shows every line of the source file in a
    /// grid, grouped by section, with a "Value" column that the user can
    /// edit. Comment, blank and embedded tabular lines are shown read-only so
    /// that the user can see the surrounding context but cannot accidentally
    /// destroy formatting (these lines are still preserved verbatim on save).
    /// </summary>
    public partial class IniEditor : UserControl, IFileEditor
    {
        private IniFile _file;
        private DataTable _table;
        private bool _suppressEvents;

        public IniEditor() { InitializeComponent(); }

        public Control AsControl { get { return this; } }
        public string FilePath { get { return _file != null ? _file.Path : null; } }
        public bool IsDirty { get; private set; }

        public new void Load(string path, ServerProject project)
        {
            _file = IniFileParser.Parse(path);
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
            IniFileParser.Save(_file);
            IsDirty = false;
            UpdateStatus();
        }

        private void BuildTable()
        {
            _suppressEvents = true;
            try
            {
                _table = new DataTable();
                _table.Columns.Add("#", typeof(int));
                _table.Columns.Add("Section", typeof(string));
                _table.Columns.Add("Key", typeof(string));
                _table.Columns.Add("Value", typeof(string));
                _table.Columns.Add("Comment", typeof(string));

                string section = string.Empty;
                int idx = 0;
                foreach (var l in _file.Lines)
                {
                    var row = _table.NewRow();
                    row[0] = idx++;
                    switch (l.Kind)
                    {
                        case IniLineKind.Section:
                            section = l.SectionName ?? string.Empty;
                            row[1] = "[" + section + "]";
                            break;
                        case IniLineKind.KeyValue:
                            row[1] = section;
                            row[2] = l.Key;
                            row[3] = l.Value;
                            row[4] = l.TrailingComment;
                            break;
                        case IniLineKind.Comment:
                        case IniLineKind.Blank:
                        case IniLineKind.Other:
                            row[4] = l.OriginalText ?? string.Empty;
                            break;
                    }
                    _table.Rows.Add(row);
                }
                _grid.DataSource = _table;
                if (_grid.Columns.Count > 0) _grid.Columns[0].ReadOnly = true;
                if (_grid.Columns.Count > 1) _grid.Columns[1].ReadOnly = true;
                if (_grid.Columns.Count > 2) _grid.Columns[2].ReadOnly = true;
                if (_grid.Columns.Count > 4) _grid.Columns[4].ReadOnly = true;
            }
            finally { _suppressEvents = false; }
        }

        private void CommitTable()
        {
            for (int r = 0; r < _table.Rows.Count && r < _file.Lines.Count; r++)
            {
                var l = _file.Lines[r];
                if (l.Kind != IniLineKind.KeyValue) continue;

                string newVal = _table.Rows[r][3] as string ?? string.Empty;
                if (newVal != (l.Value ?? string.Empty))
                {
                    l.Value = newVal;
                    l.Modified = true;
                }
            }
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
