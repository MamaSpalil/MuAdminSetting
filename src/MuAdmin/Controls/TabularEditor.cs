using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using MuAdmin.Core;
using MuAdmin.Core.Catalogs;
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
        private ServerProject _project;
        // Спец-режим: показ колонки-картинки для каталога монстров.
        private bool _hasMonsterIconColumn;
        // Спец-режим: панель предпросмотра спотов справа.
        private MonsterSpawnPreviewPanel _spawnPreview;
        private MonsterSetBaseLayout _layout;
        // file.Lines index ↔ DataTable row index (равны, см. BuildTable).

        public TabularEditor() { InitializeComponent(); }

        public Control AsControl { get { return this; } }
        public string FilePath { get { return _file != null ? _file.Path : null; } }
        public bool IsDirty { get; private set; }

        public new void Load(string path, ServerProject project)
        {
            _project = project;
            _file = TabularFileParser.Parse(path);
            ConfigureSpecialModes(path);
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

        /// <summary>
        /// Включает доп. UI для специально распознаваемых файлов:
        /// каталога монстров (колонка-иконка) и файлов спотов
        /// (боковая панель предпросмотра карты).
        /// </summary>
        private void ConfigureSpecialModes(string path)
        {
            string fileName = (Path.GetFileName(path) ?? string.Empty);
            string parent = (Path.GetFileName(Path.GetDirectoryName(path) ?? string.Empty) ?? string.Empty);

            _hasMonsterIconColumn =
                string.Equals(parent, "Monsters", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(fileName, "Monster.txt", StringComparison.OrdinalIgnoreCase) &&
                _project?.Assets != null;

            bool isSpotFile =
                string.Equals(parent, "Monsters", StringComparison.OrdinalIgnoreCase) &&
                fileName.IndexOf("MonsterSetBase", StringComparison.OrdinalIgnoreCase) >= 0 &&
                fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) &&
                _project?.Assets != null;

            if (isSpotFile)
            {
                if (_spawnPreview == null)
                {
                    _spawnPreview = new MonsterSpawnPreviewPanel();
                    Controls.Add(_spawnPreview);
                    // Docked controls are laid out from largest z-index (back)
                    // to smallest. Send the preview to the back so its Right
                    // dock is honoured before the grid's Fill consumes the
                    // remaining client area.
                    _spawnPreview.SendToBack();
                    _grid.BringToFront();
                    _grid.RowEnter += OnGridRowEnter;
                }
                _layout = MonsterSetBaseLayout.Build(_file);
                _spawnPreview.Attach(_project, _layout);
                _spawnPreview.Visible = true;
            }
            else if (_spawnPreview != null)
            {
                _spawnPreview.Visible = false;
                _layout = null;
            }
        }

        private void OnGridRowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (_spawnPreview == null || _table == null) return;
            if (e.RowIndex < 0 || e.RowIndex >= _table.Rows.Count) return;
            // Колонка "#" хранит исходный индекс строки в файле (см. BuildTable).
            int lineIdx = (int)_table.Rows[e.RowIndex][0];
            _spawnPreview.SelectByLineIndex(lineIdx);
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
                if (_hasMonsterIconColumn)
                    _table.Columns.Add("Icon", typeof(System.Drawing.Image));
                _table.Columns.Add("#", typeof(int));
                _table.Columns.Add("Comment / End", typeof(string));
                for (int i = 0; i < maxCols; i++)
                    _table.Columns.Add("c" + (i + 1), typeof(string));
                _table.Columns.Add("// trailing", typeof(string));

                int iconOffset = _hasMonsterIconColumn ? 1 : 0;
                int lineIdx = 0;
                foreach (var l in _file.Lines)
                {
                    var row = _table.NewRow();
                    row[iconOffset + 0] = lineIdx++;
                    if (l.Kind == LineKind.Data)
                    {
                        row[iconOffset + 1] = string.Empty;
                        for (int i = 0; i < l.Cells.Count; i++) row[iconOffset + 2 + i] = l.Cells[i];
                        row[_table.Columns.Count - 1] = l.TrailingComment;
                        if (_hasMonsterIconColumn && l.Cells.Count >= 1
                            && int.TryParse(l.Cells[0], out int monIdx))
                        {
                            row[0] = _project.Assets.GetMonster(monIdx);
                        }
                    }
                    else
                    {
                        row[iconOffset + 1] = l.OriginalText ?? string.Empty;
                    }
                    _table.Rows.Add(row);
                }
                _grid.DataSource = _table;
                // Запрещаем сортировку — индекс строки в гриде должен
                // совпадать с индексом в file.Lines (см. CommitTable),
                // иначе сохранение запишет ячейки не в свою строку и
                // повредит файл (особенно заметно на спотах).
                foreach (DataGridViewColumn col in _grid.Columns)
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                int idColIdx = iconOffset;
                int commentColIdx = iconOffset + 1;
                if (_grid.Columns.Count > idColIdx) _grid.Columns[idColIdx].ReadOnly = true;
                if (_grid.Columns.Count > commentColIdx) _grid.Columns[commentColIdx].ReadOnly = true;
                if (_hasMonsterIconColumn && _grid.Columns.Count > 0)
                {
                    var iconCol = _grid.Columns[0] as DataGridViewImageColumn;
                    if (iconCol != null)
                    {
                        iconCol.ReadOnly = true;
                        iconCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                        iconCol.Width = 48;
                        iconCol.HeaderText = string.Empty;
                    }
                    _grid.RowTemplate.Height = 48;
                }
            }
            finally { _suppressEvents = false; }
        }

        private void CommitTable()
        {
            int iconOffset = _hasMonsterIconColumn ? 1 : 0;
            for (int r = 0; r < _table.Rows.Count; r++)
            {
                if (r >= _file.Lines.Count) break;
                var l = _file.Lines[r];
                if (l.Kind != LineKind.Data) continue;

                bool changed = false;
                int dataColCount = _table.Columns.Count - 3 - iconOffset; // minus #, Comment, trailing, [icon]
                for (int i = 0; i < dataColCount; i++)
                {
                    string newVal = _table.Rows[r][iconOffset + 2 + i] as string ?? string.Empty;
                    string oldVal = i < l.Cells.Count ? l.Cells[i] : string.Empty;
                    if (newVal != oldVal) changed = true;
                }
                string newTrail = _table.Rows[r][_table.Columns.Count - 1] as string ?? string.Empty;
                if (newTrail != (l.TrailingComment ?? string.Empty)) changed = true;
                if (!changed) continue;

                l.Cells.Clear();
                for (int i = 0; i < dataColCount; i++)
                {
                    string v = _table.Rows[r][iconOffset + 2 + i] as string ?? string.Empty;
                    if (i >= 1 && string.IsNullOrEmpty(v) && AllRemainingEmpty(r, i + 1, dataColCount, iconOffset))
                        break;
                    l.Cells.Add(v);
                }
                l.TrailingComment = newTrail;
                l.Modified = true;
            }
        }

        private bool AllRemainingEmpty(int row, int startCol, int dataColCount, int iconOffset)
        {
            for (int i = startCol; i < dataColCount; i++)
                if (!string.IsNullOrEmpty(_table.Rows[row][iconOffset + 2 + i] as string)) return false;
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
