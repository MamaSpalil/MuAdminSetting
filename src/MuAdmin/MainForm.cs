using System;
using System.IO;
using System.Windows.Forms;
using MuAdmin.Core;
using MuAdmin.Tabs;

namespace MuAdmin
{
    /// <summary>
    /// Application shell. Top-level menu lets the user open a server folder,
    /// save changes and create backups; the main client area hosts a
    /// <see cref="TabControl"/> with one <see cref="FileCategoryTab"/> per
    /// configuration category (Quests, Monsters, Shops, Events, EventItemBags,
    /// Custom, Move, Items, CommonServer).
    /// </summary>
    public partial class MainForm : Form
    {
        private ServerProject _project;
        private readonly FileCategoryTab[] _tabs;

        public MainForm()
        {
            InitializeComponent();
            _tabs = new[]
            {
                NewTab("Quests",       "Квесты",           new[] { "Quests" }),
                NewTab("Monsters",     "Монстры / Споты",  new[] { "Monsters" }),
                NewTab("Shops",        "Магазины",         new[] { "Shops" }),
                NewTab("Events",       "События",          new[] { "Events" }),
                NewTab("EventItemBags","Event Item Bags",  new[] { "EventItemBags" }),
                NewTab("Custom",       "Custom",           new[] { "Custom" }),
                NewTab("Move",         "Телепортация",     new[] { "Move" }),
                NewTab("Items",        "Предметы",         new[] { "Items" }),
                NewTab("CommonServer", "CommonServer",     new[] { "Common", "CommonServer.cfg" })
            };
            foreach (var t in _tabs)
            {
                var page = new TabPage(t.Title) { Tag = t };
                t.Dock = DockStyle.Fill;
                page.Controls.Add(t);
                _mainTabs.TabPages.Add(page);
            }
            UpdateTitle();
        }

        private FileCategoryTab NewTab(string id, string title, string[] roots)
        {
            return new FileCategoryTab
            {
                CategoryId = id,
                Title = title,
                RelativeRoots = roots
            };
        }

        private void UpdateTitle()
        {
            Text = _project == null
                ? "MuAdmin — редактор серверных конфигов"
                : "MuAdmin — " + _project.RootPath;
            _statusLabel.Text = _project == null
                ? "Откройте папку сервера через меню Файл → Открыть папку…"
                : "Папка сервера: " + _project.RootPath;
        }

        private void OnOpenFolder(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Выберите корневую папку сервера MU Online";
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                LoadProject(dlg.SelectedPath);
            }
        }

        private void LoadProject(string path)
        {
            try
            {
                if (!ServerProject.LooksLikeServerRoot(path))
                {
                    var r = MessageBox.Show(this,
                        "Выбранная папка не похожа на корневую папку сервера MU. Открыть всё равно?",
                        "MuAdmin",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r != DialogResult.Yes) return;
                }
                var old = _project;
                _project = new ServerProject(path);
                foreach (var t in _tabs) t.AttachProject(_project);
                if (old != null) old.Dispose();
                UpdateTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не удалось открыть папку:\r\n" + ex.Message,
                    "MuAdmin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSaveCurrent(object sender, EventArgs e)
        {
            var t = ActiveTab();
            if (t != null) t.SaveActive();
        }

        private void OnSaveAll(object sender, EventArgs e)
        {
            int saved = 0;
            foreach (var t in _tabs) saved += t.SaveAllDirty();
            _statusLabel.Text = "Сохранено файлов: " + saved;
        }

        private void OnReload(object sender, EventArgs e)
        {
            var t = ActiveTab();
            if (t != null) t.ReloadActive();
        }

        private void OnExit(object sender, EventArgs e) { Close(); }

        private void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "MuAdmin 1.0\r\n" +
                "Графический редактор серверных конфигов MU Online.\r\n" +
                "Сборка: Visual Studio Enterprise 2019 (16.11.34), .NET Framework 4.8, x86 / x64.",
                "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private FileCategoryTab ActiveTab()
        {
            if (_mainTabs.SelectedTab == null) return null;
            return _mainTabs.SelectedTab.Tag as FileCategoryTab;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            int dirty = 0;
            foreach (var t in _tabs) dirty += t.DirtyCount;
            if (dirty > 0)
            {
                var r = MessageBox.Show(this,
                    "Есть несохранённые изменения (" + dirty + " файл(ов)). Сохранить перед выходом?",
                    "MuAdmin", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel) { e.Cancel = true; return; }
                if (r == DialogResult.Yes) OnSaveAll(this, EventArgs.Empty);
            }
            base.OnFormClosing(e);
        }
    }
}
