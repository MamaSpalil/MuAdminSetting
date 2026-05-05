using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MuAdmin.Core;
using MuAdmin.Core.Catalogs;
using MuAdmin.Core.Models;

namespace MuAdmin.Controls
{
    /// <summary>
    /// Modal dialog used by <see cref="QuestSystemEditor"/> to compose a new
    /// quest row for <c>Quests\QuestSystem.ini</c> from drop-downs and
    /// numeric inputs. Auto-generates <c>Message1</c> /<c>Message2</c> from
    /// the selected location and monster, exposes a level-aware item picker
    /// when <c>PrizeType = 0 - Items</c>, and produces the final 17-cell
    /// quest line via <see cref="BuildQuestLine"/>.
    /// </summary>
    public partial class QuickAddQuestDialog : Form
    {
        private readonly ServerProject _project;

        private static readonly string[] PrizeTypeLabels =
        {
            "0 - Items",
            "1 - Zen",
            "2 - Normal Points",
            "3 - Credits",
            "4 - WCoin"
        };

        // Data sources for the location / monster / item drop-downs. Stored as
        // plain wrappers so we can resolve them back to numeric ids.
        private sealed class ComboItem
        {
            public int Id;
            public string Display;
            public override string ToString() { return Display; }
        }

        private sealed class ItemComboItem
        {
            public int Type;
            public int Index;
            public string BaseName;
            public override string ToString()
            {
                return string.Format("{0}/{1} — {2}", Type, Index, BaseName ?? string.Empty);
            }
        }

        private bool _suppress;

        public QuickAddQuestDialog(ServerProject project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            _project = project;

            InitializeComponent();
            PopulateLocations();
            PopulateMonsters();
            PopulatePrizeTypes();
            PopulateItems();
            WireEvents();

            // Initial state: prize type = Items, item group enabled.
            _cbPrizeType.SelectedIndex = 0;
            UpdateItemGroupVisibility();
            RefreshItemLevels();
            RegenerateMessages();
            RefreshPreview();
        }

        // ---------- public results ----------

        /// <summary>The 17-cell quest row built from the dialog's current state.</summary>
        public List<string> BuildQuestCells()
        {
            int locId = SelectedLocationId();
            int monId = SelectedMonsterId();
            int count = (int)_numCount.Value;
            int percent = _chkAutoPercent.Checked ? 100 : (int)_numPercent.Value;
            int prizeType = _cbPrizeType.SelectedIndex < 0 ? 0 : _cbPrizeType.SelectedIndex;
            int prizeValue = (int)_numPrizeValue.Value;
            string msg1 = Quote(_txtMessage1.Text);
            string msg2 = Quote(_txtMessage2.Text);
            int dropCnt = (int)_numDropCnt.Value;

            int itemType, itemIndex, lvlMin, lvlMax, skill, luck, opt, exc;
            if (prizeType == 0)
            {
                var item = SelectedItem();
                itemType = item != null ? item.Type : 0;
                itemIndex = item != null ? item.Index : 0;
                lvlMin = (int)_numLvlMin.Value;
                lvlMax = (int)_numLvlMax.Value;
                skill = (int)_numSkill.Value;
                luck = (int)_numLuck.Value;
                opt = (int)_numOpt.Value;
                exc = (int)_numExc.Value;
            }
            else
            {
                // Non-Items prizes still need 17 cells; mirror the conventions
                // used by existing quest rows in QuestSystem.ini: zeros across
                // the item descriptor block.
                itemType = 0;
                itemIndex = 0;
                lvlMin = 0;
                lvlMax = 0;
                skill = 0;
                luck = 0;
                opt = 0;
                exc = 0;
            }

            return new List<string>
            {
                locId.ToString(),
                monId.ToString(),
                count.ToString(),
                percent.ToString(),
                prizeType.ToString(),
                prizeValue.ToString(),
                msg1,
                msg2,
                dropCnt.ToString(),
                itemType.ToString(),
                itemIndex.ToString(),
                lvlMin.ToString(),
                lvlMax.ToString(),
                skill.ToString(),
                luck.ToString(),
                opt.ToString(),
                exc.ToString()
            };
        }

        /// <summary>
        /// Builds a fully formatted quest line (tab-separated, with leading
        /// tab and a trailing <c>// Quest #N</c> comment) ready to be
        /// inserted into <see cref="QuestSystemFile"/>.
        /// </summary>
        public QuestSystemLine BuildQuestLine(int questNumberHint)
        {
            var cells = BuildQuestCells();
            string trailing = questNumberHint > 0 ? "//Quest #" + questNumberHint : string.Empty;

            var line = new QuestSystemLine
            {
                Kind = QuestSystemLineKind.Quest,
                LeadingWhitespace = "\t",
                Cells = cells,
                TrailingComment = trailing,
                Modified = true
            };
            line.OriginalText = SerializePreview(cells, trailing);
            return line;
        }

        // ---------- populate helpers ----------

        private void PopulateLocations()
        {
            var list = MapNames.All
                .OrderBy(kv => kv.Key)
                .Select(kv => new ComboItem { Id = kv.Key, Display = kv.Value + "  (" + kv.Key + ")" })
                .ToList();
            _cbLocation.DisplayMember = "Display";
            _cbLocation.DataSource = list;
            if (list.Count > 0) _cbLocation.SelectedIndex = 0;
        }

        private void PopulateMonsters()
        {
            var list = new List<ComboItem>();
            if (_project.Monsters != null)
            {
                foreach (var kv in _project.Monsters.AllStats.OrderBy(kv => kv.Key))
                {
                    string nm = kv.Value != null ? kv.Value.Name : null;
                    if (string.IsNullOrEmpty(nm)) nm = "Monster " + kv.Key;
                    list.Add(new ComboItem { Id = kv.Key, Display = nm + "  (" + kv.Key + ")" });
                }
            }
            _cbMonster.DisplayMember = "Display";
            _cbMonster.DataSource = list;
            if (list.Count > 0) _cbMonster.SelectedIndex = 0;
        }

        private void PopulatePrizeTypes()
        {
            _cbPrizeType.Items.Clear();
            foreach (var l in PrizeTypeLabels) _cbPrizeType.Items.Add(l);
        }

        private void PopulateItems()
        {
            var list = new List<ItemComboItem>();
            if (_project.Items != null)
            {
                foreach (var e in _project.Items.Entries)
                {
                    list.Add(new ItemComboItem
                    {
                        Type = e.Type,
                        Index = e.Index,
                        BaseName = e.Name
                    });
                }
            }
            _cbItem.DataSource = list;
            if (list.Count > 0) _cbItem.SelectedIndex = 0;
        }

        private void WireEvents()
        {
            _cbLocation.SelectedIndexChanged += (s, e) => { RegenerateMessages(); RefreshPreview(); };
            _cbMonster.SelectedIndexChanged  += (s, e) => { RegenerateMessages(); RefreshPreview(); };
            _cbMonster.TextChanged           += (s, e) => { if (!_suppress) RefreshPreview(); };
            _cbPrizeType.SelectedIndexChanged += (s, e) => { UpdateItemGroupVisibility(); RefreshPreview(); };
            _numCount.ValueChanged    += (s, e) => RefreshPreview();
            _numPercent.ValueChanged  += (s, e) => RefreshPreview();
            _chkAutoPercent.CheckedChanged += (s, e) =>
            {
                _numPercent.Enabled = !_chkAutoPercent.Checked;
                if (_chkAutoPercent.Checked) _numPercent.Value = 100;
                RefreshPreview();
            };
            _numPrizeValue.ValueChanged += (s, e) => RefreshPreview();
            _txtMessage1.TextChanged    += (s, e) => RefreshPreview();
            _txtMessage2.TextChanged    += (s, e) => RefreshPreview();
            _numDropCnt.ValueChanged    += (s, e) => RefreshPreview();
            _cbItem.SelectedIndexChanged += (s, e) => { RefreshItemLevels(); RefreshPreview(); };
            _cbItemLevel.SelectedIndexChanged += (s, e) =>
            {
                if (_suppress) return;
                int lvl = SelectedItemLevel();
                // Convenience: when the user picks a named level variant we
                // keep LvlMin/LvlMax in sync so the produced row reflects it.
                _numLvlMin.Value = Math.Min(_numLvlMin.Maximum, lvl);
                _numLvlMax.Value = Math.Max(_numLvlMax.Value, _numLvlMin.Value);
                RefreshPreview();
            };
            _numLvlMin.ValueChanged += (s, e) =>
            {
                if (_numLvlMax.Value < _numLvlMin.Value) _numLvlMax.Value = _numLvlMin.Value;
                RefreshPreview();
            };
            _numLvlMax.ValueChanged += (s, e) =>
            {
                if (_numLvlMin.Value > _numLvlMax.Value) _numLvlMin.Value = _numLvlMax.Value;
                RefreshPreview();
            };
            _numSkill.ValueChanged += (s, e) => RefreshPreview();
            _numLuck.ValueChanged  += (s, e) => RefreshPreview();
            _numOpt.ValueChanged   += (s, e) => RefreshPreview();
            _numExc.ValueChanged   += (s, e) => RefreshPreview();

            _btnOk.Click += OnOkClick;
        }

        // ---------- selection helpers ----------

        private int SelectedLocationId()
        {
            var ci = _cbLocation.SelectedItem as ComboItem;
            return ci != null ? ci.Id : 0;
        }

        private int SelectedMonsterId()
        {
            var ci = _cbMonster.SelectedItem as ComboItem;
            if (ci != null) return ci.Id;
            // Fall back to any leading integer typed by the user.
            int v;
            if (!string.IsNullOrEmpty(_cbMonster.Text)
                && int.TryParse(_cbMonster.Text.Split(' ')[0], out v))
                return v;
            return 0;
        }

        private string SelectedMonsterName()
        {
            var ci = _cbMonster.SelectedItem as ComboItem;
            if (ci != null) return _project.Monsters != null
                ? _project.Monsters.GetName(ci.Id)
                : ci.Display;
            return _cbMonster.Text ?? string.Empty;
        }

        private string SelectedLocationName()
        {
            var ci = _cbLocation.SelectedItem as ComboItem;
            return ci != null ? MapNames.Get(ci.Id) : string.Empty;
        }

        private ItemComboItem SelectedItem()
        {
            return _cbItem.SelectedItem as ItemComboItem;
        }

        /// <summary>
        /// Returns the actual item level represented by the currently
        /// selected <c>_cbItemLevel</c> entry. Items are formatted as
        /// <c>"+N — Name"</c>, so the level is parsed from the leading
        /// <c>+N</c> marker rather than relying on the dropdown index
        /// (which no longer maps 1:1 to level once the level list is
        /// sourced from <c>Item_level.txt</c>).
        /// </summary>
        private int SelectedItemLevel()
        {
            var s = _cbItemLevel.SelectedItem as string;
            if (string.IsNullOrEmpty(s)) return 0;
            int start = 0;
            if (s.Length > 0 && s[0] == '+') start = 1;
            int end = start;
            while (end < s.Length && char.IsDigit(s[end])) end++;
            int v;
            return end > start && int.TryParse(s.Substring(start, end - start), out v) ? v : 0;
        }

        // ---------- behaviour ----------

        private void UpdateItemGroupVisibility()
        {
            bool isItems = _cbPrizeType.SelectedIndex == 0;
            _grpItem.Enabled = isItems;
            _grpItem.Visible = true; // keep visible (just disabled) so layout doesn't jump
        }

        private void RegenerateMessages()
        {
            if (_suppress) return;
            string loc = SelectedLocationName();
            string mon = SelectedMonsterName();
            // Normalize the monster name (drop the "(id)" suffix introduced by
            // the combobox display string when the user typed free-form text).
            mon = StripIdSuffix(mon);
            _suppress = true;
            try
            {
                _txtMessage1.Text = "Kill the " + mon + " in " + loc;
                _txtMessage2.Text = mon + ":";
            }
            finally { _suppress = false; }
        }

        private static string StripIdSuffix(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            int lp = s.LastIndexOf('(');
            if (lp > 0) return s.Substring(0, lp).TrimEnd();
            return s.Trim();
        }

        private void RefreshItemLevels()
        {
            _suppress = true;
            try
            {
                _cbItemLevel.Items.Clear();
                var item = SelectedItem();
                if (item == null)
                {
                    _cbItemLevel.Items.Add("+0 — (нет вещи)");
                    _cbItemLevel.SelectedIndex = 0;
                    return;
                }

                // Prefer the explicit level list from Item_level.txt when
                // the project has loaded one — that file is the source of
                // truth for which level variants actually exist for an
                // item (e.g. 14/11 has 0/1/2/3/5/6/7/8/9/10/11/12/13/14/15
                // but not 4). Fall back to a dense 0..max range when the
                // file is absent or the item isn't listed in it.
                var knownLevels = _project.ItemLevels != null
                    ? _project.ItemLevels.GetKnownLevels(item.Type, item.Index)
                    : null;

                int maxLvl = Math.Max(0, ItemLevelNames.MaxKnownLevel(item.Type, item.Index));
                int upper = Math.Max(maxLvl, 7);

                int selectedIdx = -1;
                int desiredLvl = (int)_numLvlMin.Value;

                if (knownLevels != null && knownLevels.Count > 0)
                {
                    for (int i = 0; i < knownLevels.Count; i++)
                    {
                        int lvl = knownLevels[i];
                        string display = ItemLevelNames.DisplayName(item.BaseName, item.Type, item.Index, lvl);
                        _cbItemLevel.Items.Add("+" + lvl + " — " + display);
                        if (lvl == desiredLvl) selectedIdx = i;
                    }
                }
                else
                {
                    for (int lvl = 0; lvl <= upper; lvl++)
                    {
                        string display = ItemLevelNames.DisplayName(item.BaseName, item.Type, item.Index, lvl);
                        _cbItemLevel.Items.Add("+" + lvl + " — " + display);
                        if (lvl == desiredLvl) selectedIdx = lvl;
                    }
                }

                if (_cbItemLevel.Items.Count > 0)
                    _cbItemLevel.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;
            }
            finally { _suppress = false; }
        }

        private void RefreshPreview()
        {
            if (_suppress) return;
            try
            {
                var cells = BuildQuestCells();
                _txtPreview.Text = SerializePreview(cells, "//Quest #N");
            }
            catch (Exception ex)
            {
                _txtPreview.Text = "(ошибка предпросмотра: " + ex.Message + ")";
            }
        }

        private static string SerializePreview(IList<string> cells, string trailingComment)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append('\t');
            for (int i = 0; i < cells.Count; i++)
            {
                if (i > 0) sb.Append('\t');
                sb.Append(cells[i] ?? string.Empty);
            }
            if (!string.IsNullOrEmpty(trailingComment))
            {
                sb.Append('\t');
                sb.Append(trailingComment);
            }
            return sb.ToString();
        }

        private static string Quote(string s)
        {
            if (s == null) return "\"\"";
            // Strip embedded double quotes — the file format uses them as cell
            // delimiters and won't survive a round-trip otherwise.
            s = s.Replace("\"", string.Empty);
            return "\"" + s + "\"";
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            if (_cbLocation.SelectedItem == null)
            {
                MessageBox.Show(this, "Выберите локацию.", "Быстрое добавление квеста",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            if (SelectedMonsterId() <= 0 && _cbMonster.SelectedItem == null)
            {
                MessageBox.Show(this, "Выберите монстра.", "Быстрое добавление квеста",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
            if (_cbPrizeType.SelectedIndex == 0 && SelectedItem() == null)
            {
                MessageBox.Show(this, "Для типа приза «0 - Items» необходимо выбрать вещь.",
                    "Быстрое добавление квеста",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }
        }
    }
}
