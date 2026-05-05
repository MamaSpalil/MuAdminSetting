using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
    /// Specialised editor for <c>Quests\QuestSystem.ini</c>. The top grid
    /// exposes the <c>[Common]</c> INI keys (Enable / NPC / NameQuest); the
    /// bottom grid renders one row per <see cref="QuestSystemLineKind.Quest"/>
    /// line with semantic columns (Location, Monster, Count, Percent,
    /// PrizeType combo, PrizeValue, Message1/2, DropItem, ItemType/Index,
    /// LvlMin/Max, Skill, Luck, Opt, Exc) and resolves Location / Monster /
    /// Item ids to readable names via the project catalogs. Comment, blank
    /// and block-marker lines appear in the lower grid as read-only banner
    /// rows so that contextual headers like <c>////Lorencia Quest</c> stay
    /// in place exactly like <see cref="TabularEditor"/> does for tab-separated
    /// files.
    /// </summary>
    public partial class QuestSystemEditor : UserControl, IFileEditor
    {
        private QuestSystemFile _file;
        private ServerProject _project;
        private DataTable _headerTable;
        private DataTable _questTable;
        // Maps quest grid row index -> source line index in _file.Lines.
        private List<int> _rowToLine;
        // Same for header grid.
        private List<int> _headerRowToLine;
        private bool _suppressEvents;

        // Column indexes inside _questTable (offset by 2: # + Comment columns).
        private const int ColComment = 1;
        private const int FirstDataCol = 2;                 // Location
        private const int ColLocationName = FirstDataCol + QuestRowSchema.ColumnCount;
        private const int ColMonsterName = ColLocationName + 1;
        private const int ColItemName = ColMonsterName + 1;
        private const int ColItemIcon = ColItemName + 1;
        private const int ColTrailing = ColItemIcon + 1;

        // Prize type display strings shown in the combobox column.
        private static readonly string[] PrizeTypeLabels =
        {
            "0 - Items",
            "1 - Zen",
            "2 - Normal Points",
            "3 - Credits",
            "4 - WCoin"
        };

        public QuestSystemEditor() { InitializeComponent(); }

        public Control AsControl { get { return this; } }
        public string FilePath { get { return _file != null ? _file.Path : null; } }
        public bool IsDirty { get; private set; }

        public new void Load(string path, ServerProject project)
        {
            _project = project;
            _file = QuestSystemFileParser.Parse(path);
            BuildHeaderTable();
            BuildQuestTable();
            IsDirty = false;
            UpdateStatus();
        }

        public void Save(ServerProject project)
        {
            if (_file == null) return;
            CommitHeaderTable();
            CommitQuestTable();
            if (project != null)
                BackupManager.CreateBackup(project.RootPath, _file.Path);
            QuestSystemFileParser.Save(_file);
            IsDirty = false;
            UpdateStatus();
        }

        // ---------------- Header (top) grid ----------------

        private void BuildHeaderTable()
        {
            _suppressEvents = true;
            try
            {
                _headerTable = new DataTable();
                _headerTable.Columns.Add("#", typeof(int));
                _headerTable.Columns.Add("Section", typeof(string));
                _headerTable.Columns.Add("Key", typeof(string));
                _headerTable.Columns.Add("Value", typeof(string));
                _headerTable.Columns.Add("Comment", typeof(string));

                _headerRowToLine = new List<int>();
                string section = string.Empty;
                for (int i = 0; i < _file.Lines.Count; i++)
                {
                    var l = _file.Lines[i];
                    if (l.Kind == QuestSystemLineKind.Section)
                    {
                        section = l.SectionName ?? string.Empty;
                        AddHeaderRow(i, "[" + section + "]", null, null, null);
                    }
                    else if (l.Kind == QuestSystemLineKind.KeyValue)
                    {
                        AddHeaderRow(i, section, l.Key, l.Value, l.TrailingComment);
                    }
                }

                _headerGrid.DataSource = _headerTable;
                if (_headerGrid.Columns.Count > 0) _headerGrid.Columns[0].ReadOnly = true;
                if (_headerGrid.Columns.Count > 1) _headerGrid.Columns[1].ReadOnly = true;
                if (_headerGrid.Columns.Count > 2) _headerGrid.Columns[2].ReadOnly = true;
                if (_headerGrid.Columns.Count > 4) _headerGrid.Columns[4].ReadOnly = true;
            }
            finally { _suppressEvents = false; }
        }

        private void AddHeaderRow(int sourceLine, string section, string key, string value, string comment)
        {
            var row = _headerTable.NewRow();
            row[0] = _headerRowToLine.Count;
            row[1] = section ?? string.Empty;
            row[2] = (object)key ?? DBNull.Value;
            row[3] = (object)value ?? DBNull.Value;
            row[4] = (object)comment ?? DBNull.Value;
            _headerTable.Rows.Add(row);
            _headerRowToLine.Add(sourceLine);
        }

        private void CommitHeaderTable()
        {
            for (int r = 0; r < _headerTable.Rows.Count; r++)
            {
                int li = _headerRowToLine[r];
                var l = _file.Lines[li];
                if (l.Kind != QuestSystemLineKind.KeyValue) continue;

                string newVal = _headerTable.Rows[r][3] as string ?? string.Empty;
                if (newVal != (l.Value ?? string.Empty))
                {
                    l.Value = newVal;
                    l.Modified = true;
                }
            }
        }

        // ---------------- Quest (bottom) grid ----------------

        private void BuildQuestTable()
        {
            _suppressEvents = true;
            try
            {
                _questTable = new DataTable();
                _questTable.Columns.Add("#", typeof(int));
                _questTable.Columns.Add("Comment / End", typeof(string));
                foreach (var title in QuestRowSchema.ColumnTitles)
                    _questTable.Columns.Add(title, typeof(string));
                _questTable.Columns.Add("Location name", typeof(string));
                _questTable.Columns.Add("Monster name", typeof(string));
                _questTable.Columns.Add("Item name", typeof(string));
                _questTable.Columns.Add("Item", typeof(System.Drawing.Image));
                _questTable.Columns.Add("// trailing", typeof(string));

                _rowToLine = new List<int>();
                int idx = 0;
                for (int i = 0; i < _file.Lines.Count; i++)
                {
                    var l = _file.Lines[i];
                    // Skip the lines we already render in the header grid so
                    // they aren't duplicated (and cannot be edited from two
                    // places at once).
                    if (l.Kind == QuestSystemLineKind.Section) continue;
                    if (l.Kind == QuestSystemLineKind.KeyValue) continue;

                    var row = _questTable.NewRow();
                    row[0] = idx++;
                    if (l.Kind == QuestSystemLineKind.Quest)
                    {
                        row[ColComment] = string.Empty;
                        for (int c = 0; c < QuestRowSchema.ColumnCount; c++)
                        {
                            string cell = c < l.Cells.Count ? l.Cells[c] : string.Empty;
                            row[FirstDataCol + c] = c == QuestRowSchema.PrizeType
                                ? PrizeTypeLabel(cell)
                                : (c == QuestRowSchema.Message1 || c == QuestRowSchema.Message2
                                    ? Unquote(cell)
                                    : cell);
                        }
                        row[ColLocationName] = ResolveLocation(l, QuestRowSchema.Location);
                        row[ColMonsterName] = ResolveMonster(l);
                        row[ColItemName] = ResolveItem(l);
                        row[ColItemIcon] = ResolveItemIcon(l);
                        row[ColTrailing] = l.TrailingComment;
                    }
                    else
                    {
                        // Banner row: show original text in the comment column,
                        // leave data columns blank/read-only.
                        row[ColComment] = l.OriginalText ?? string.Empty;
                    }
                    _questTable.Rows.Add(row);
                    _rowToLine.Add(i);
                }

                _grid.DataSource = _questTable;

                // Column setup: all read-only, then reopen the editable ones.
                if (_grid.Columns.Count > 0) _grid.Columns[0].ReadOnly = true;          // #
                if (_grid.Columns.Count > ColComment) _grid.Columns[ColComment].ReadOnly = true;
                if (_grid.Columns.Count > ColLocationName) _grid.Columns[ColLocationName].ReadOnly = true;
                if (_grid.Columns.Count > ColMonsterName) _grid.Columns[ColMonsterName].ReadOnly = true;
                if (_grid.Columns.Count > ColItemName) _grid.Columns[ColItemName].ReadOnly = true;
                if (_grid.Columns.Count > ColItemIcon)
                {
                    _grid.Columns[ColItemIcon].ReadOnly = true;
                    var iconCol = _grid.Columns[ColItemIcon] as DataGridViewImageColumn;
                    if (iconCol != null)
                    {
                        iconCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                        iconCol.Width = 36;
                        iconCol.HeaderText = string.Empty;
                    }
                }
                if (_grid.Columns.Count > ColTrailing) _grid.Columns[ColTrailing].ReadOnly = true;

                // Replace the PrizeType column with a combobox that has the
                // five labelled options. Keep the source DataTable column
                // (string) so DataSource binding still works; the combobox
                // edits the same underlying cell.
                int prizeColIdx = FirstDataCol + QuestRowSchema.PrizeType;
                if (_grid.Columns.Count > prizeColIdx)
                {
                    var oldCol = _grid.Columns[prizeColIdx];
                    string colName = oldCol.Name;
                    string colHeader = oldCol.HeaderText;
                    string dataMember = oldCol.DataPropertyName;
                    _grid.Columns.RemoveAt(prizeColIdx);
                    var combo = new DataGridViewComboBoxColumn
                    {
                        Name = colName,
                        HeaderText = colHeader,
                        DataPropertyName = dataMember,
                        FlatStyle = FlatStyle.Flat
                    };
                    foreach (var lbl in PrizeTypeLabels) combo.Items.Add(lbl);
                    _grid.Columns.Insert(prizeColIdx, combo);
                }

                ApplyBannerRowStyles();
            }
            finally { _suppressEvents = false; }
        }

        private void ApplyBannerRowStyles()
        {
            for (int r = 0; r < _grid.Rows.Count && r < _rowToLine.Count; r++)
            {
                var l = _file.Lines[_rowToLine[r]];
                if (l.Kind == QuestSystemLineKind.Quest) continue;
                var gr = _grid.Rows[r];
                gr.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                gr.DefaultCellStyle.ForeColor = Color.DimGray;
                gr.ReadOnly = true;
            }
        }

        private void CommitQuestTable()
        {
            for (int r = 0; r < _questTable.Rows.Count; r++)
            {
                int li = _rowToLine[r];
                var l = _file.Lines[li];
                if (l.Kind != QuestSystemLineKind.Quest) continue;

                bool changed = false;
                var newCells = new List<string>(QuestRowSchema.ColumnCount);
                for (int c = 0; c < QuestRowSchema.ColumnCount; c++)
                {
                    string raw = _questTable.Rows[r][FirstDataCol + c] as string ?? string.Empty;
                    string normalized;
                    if (c == QuestRowSchema.PrizeType)
                        normalized = PrizeTypeValueFromLabel(raw);
                    else if (c == QuestRowSchema.Message1 || c == QuestRowSchema.Message2)
                        normalized = Quote(raw);
                    else
                        normalized = raw.Trim();

                    string old = c < l.Cells.Count ? l.Cells[c] : string.Empty;
                    if (normalized != old) changed = true;
                    newCells.Add(normalized);
                }
                string newTrail = _questTable.Rows[r][ColTrailing] as string ?? string.Empty;
                if (newTrail != (l.TrailingComment ?? string.Empty)) changed = true;

                if (!changed) continue;

                l.Cells = newCells;
                l.TrailingComment = newTrail;
                l.Modified = true;
            }
        }

        // ---------------- Helpers ----------------

        private static string Unquote(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
                return s.Substring(1, s.Length - 2);
            return s;
        }

        private static string Quote(string s)
        {
            if (s == null) return "\"\"";
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"') return s; // already quoted
            return "\"" + s + "\"";
        }

        private static string PrizeTypeLabel(string cell)
        {
            int v;
            if (int.TryParse(cell, out v) && v >= 0 && v < PrizeTypeLabels.Length)
                return PrizeTypeLabels[v];
            return cell ?? string.Empty;
        }

        private static string PrizeTypeValueFromLabel(string label)
        {
            if (string.IsNullOrEmpty(label)) return "0";
            for (int i = 0; i < PrizeTypeLabels.Length; i++)
                if (label == PrizeTypeLabels[i]) return i.ToString();
            // Fall back to a leading integer if the user typed one.
            int sep = label.IndexOf(' ');
            string head = sep > 0 ? label.Substring(0, sep) : label;
            int v;
            return int.TryParse(head, out v) ? v.ToString() : label.Trim();
        }

        private string ResolveLocation(QuestSystemLine l, int col)
        {
            int v;
            if (col < l.Cells.Count && int.TryParse(l.Cells[col], out v))
                return MapNames.Get(v);
            return string.Empty;
        }

        private string ResolveMonster(QuestSystemLine l)
        {
            if (_project == null || _project.Monsters == null) return string.Empty;
            int v;
            if (QuestRowSchema.Monster < l.Cells.Count && int.TryParse(l.Cells[QuestRowSchema.Monster], out v))
                return _project.Monsters.GetName(v);
            return string.Empty;
        }

        private string ResolveItem(QuestSystemLine l)
        {
            if (_project == null || _project.Items == null) return string.Empty;
            int t, idx;
            if (QuestRowSchema.ItemType < l.Cells.Count
                && QuestRowSchema.ItemIndex < l.Cells.Count
                && int.TryParse(l.Cells[QuestRowSchema.ItemType], out t)
                && int.TryParse(l.Cells[QuestRowSchema.ItemIndex], out idx))
                return _project.Items.GetName(t, idx);
            return string.Empty;
        }

        private object ResolveItemIcon(QuestSystemLine l)
        {
            if (_project?.Assets == null) return DBNull.Value;
            int t, idx;
            if (QuestRowSchema.ItemType < l.Cells.Count
                && QuestRowSchema.ItemIndex < l.Cells.Count
                && int.TryParse(l.Cells[QuestRowSchema.ItemType], out t)
                && int.TryParse(l.Cells[QuestRowSchema.ItemIndex], out idx))
                return (object)_project.Assets.GetItem(t, idx);
            return DBNull.Value;
        }

        private void OnHeaderCellChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressEvents) return;
            IsDirty = true;
            UpdateStatus();
        }

        private void OnQuestCellChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressEvents) return;
            IsDirty = true;

            // Refresh the three resolved-name columns when the underlying ids
            // change so the user sees the new name without reloading.
            if (e.RowIndex < 0 || e.RowIndex >= _rowToLine.Count) { UpdateStatus(); return; }
            int li = _rowToLine[e.RowIndex];
            var l = _file.Lines[li];
            if (l.Kind != QuestSystemLineKind.Quest) { UpdateStatus(); return; }

            // Build a synthetic line view from the current row to resolve names.
            var synth = new QuestSystemLine { Cells = new List<string>(QuestRowSchema.ColumnCount) };
            for (int c = 0; c < QuestRowSchema.ColumnCount; c++)
            {
                string raw = _questTable.Rows[e.RowIndex][FirstDataCol + c] as string ?? string.Empty;
                if (c == QuestRowSchema.PrizeType) raw = PrizeTypeValueFromLabel(raw);
                synth.Cells.Add(raw);
            }
            _suppressEvents = true;
            try
            {
                _questTable.Rows[e.RowIndex][ColLocationName] = ResolveLocation(synth, QuestRowSchema.Location);
                _questTable.Rows[e.RowIndex][ColMonsterName] = ResolveMonster(synth);
                _questTable.Rows[e.RowIndex][ColItemName] = ResolveItem(synth);
                _questTable.Rows[e.RowIndex][ColItemIcon] = ResolveItemIcon(synth);
            }
            finally { _suppressEvents = false; }

            UpdateStatus();
        }

        private void OnGridDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Suppress combobox "value is not in the list" errors that can
            // happen when an existing row had an out-of-range PrizeType. The
            // raw value is still preserved in the underlying DataTable.
            e.ThrowException = false;
        }

        private void UpdateStatus()
        {
            _statusLabel.Text = (_file == null ? string.Empty : Path.GetFileName(_file.Path))
                + (IsDirty ? "  •  изменён" : "  •  без изменений");
        }

        // ---------------- Quick add quest ----------------

        private void OnAddQuestClick(object sender, EventArgs e)
        {
            if (_file == null || _project == null)
            {
                MessageBox.Show(this,
                    "Файл QuestSystem.ini не загружен.",
                    "Быстрое добавление квеста",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new QuickAddQuestDialog(_project))
            {
                if (dlg.ShowDialog(this.FindForm() ?? (Control)this) != DialogResult.OK)
                    return;

                int questNumber = CountExistingQuestRows() + 1;
                var newLine = dlg.BuildQuestLine(questNumber);
                int insertAt = FindInsertionIndex();
                _file.Lines.Insert(insertAt, newLine);

                // Rebuild the bottom grid so the new row shows up immediately
                // with its resolved Location/Monster/Item names. The header
                // grid is unaffected.
                BuildQuestTable();
                IsDirty = true;
                UpdateStatus();

                // Scroll to and select the freshly added row.
                int newGridRow = _rowToLine != null ? _rowToLine.IndexOf(insertAt) : -1;
                if (newGridRow >= 0 && newGridRow < _grid.Rows.Count)
                {
                    _grid.ClearSelection();
                    _grid.Rows[newGridRow].Selected = true;
                    _grid.FirstDisplayedScrollingRowIndex = newGridRow;
                }
            }
        }

        private int CountExistingQuestRows()
        {
            int n = 0;
            for (int i = 0; i < _file.Lines.Count; i++)
                if (_file.Lines[i].Kind == QuestSystemLineKind.Quest) n++;
            return n;
        }

        // Inserts immediately before the trailing "end" terminator, or at the
        // end of the file if no such marker is present. Skips trailing blank
        // lines so the new row sits flush with the last quest entry.
        private int FindInsertionIndex()
        {
            for (int i = _file.Lines.Count - 1; i >= 0; i--)
            {
                var l = _file.Lines[i];
                if (l.Kind == QuestSystemLineKind.BlockMarker
                    && string.Equals((l.OriginalText ?? string.Empty).Trim(), "end",
                                     StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            // Fall back: insert before any trailing blank lines.
            int idx = _file.Lines.Count;
            while (idx > 0 && _file.Lines[idx - 1].Kind == QuestSystemLineKind.Blank) idx--;
            return idx;
        }
    }
}
