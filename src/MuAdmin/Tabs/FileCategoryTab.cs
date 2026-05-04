using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MuAdmin.Controls;
using MuAdmin.Core;

namespace MuAdmin.Tabs
{
    /// <summary>
    /// One tab of the main window. Shows a tree of files under the configured
    /// <see cref="RelativeRoots"/> and hosts the appropriate editor
    /// (<see cref="TabularEditor"/>, <see cref="IniEditor"/> or
    /// <see cref="RawTextEditor"/>) on the right side.
    /// </summary>
    public partial class FileCategoryTab : UserControl
    {
        public string CategoryId { get; set; }
        public string Title { get; set; }
        /// <summary>One or more paths relative to the server root.</summary>
        public string[] RelativeRoots { get; set; } = new string[0];

        private ServerProject _project;
        private readonly Dictionary<string, IFileEditor> _openEditors =
            new Dictionary<string, IFileEditor>(StringComparer.OrdinalIgnoreCase);
        private IFileEditor _active;

        public FileCategoryTab() { InitializeComponent(); }

        public int DirtyCount
        {
            get
            {
                int n = 0;
                foreach (var e in _openEditors.Values) if (e.IsDirty) n++;
                return n;
            }
        }

        public void AttachProject(ServerProject project)
        {
            _project = project;
            _openEditors.Clear();
            _editorHost.Controls.Clear();
            _active = null;
            _tree.Nodes.Clear();
            if (_project == null) return;

            foreach (var rel in RelativeRoots)
            {
                string full = Path.Combine(_project.RootPath, rel);
                if (Directory.Exists(full))
                    _tree.Nodes.Add(BuildDirNode(full));
                else if (File.Exists(full))
                    _tree.Nodes.Add(MakeFileNode(full));
            }
            _tree.ExpandAll();
        }

        private TreeNode BuildDirNode(string dir)
        {
            var node = new TreeNode(Path.GetFileName(dir)) { Tag = null };
            try
            {
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    if (string.Equals(Path.GetFileName(sub), BackupManager.BackupFolderName,
                        StringComparison.OrdinalIgnoreCase)) continue;
                    node.Nodes.Add(BuildDirNode(sub));
                }
                foreach (var f in Directory.GetFiles(dir))
                    node.Nodes.Add(MakeFileNode(f));
            }
            catch { /* ignore unreadable folders */ }
            return node;
        }

        private TreeNode MakeFileNode(string file)
        {
            return new TreeNode(Path.GetFileName(file)) { Tag = file };
        }

        private void OnTreeSelect(object sender, TreeViewEventArgs e)
        {
            string path = e.Node.Tag as string;
            if (path == null) return;
            ShowEditor(path);
        }

        private void ShowEditor(string path)
        {
            IFileEditor ed;
            if (!_openEditors.TryGetValue(path, out ed))
            {
                ed = CreateEditorFor(path);
                if (ed == null) return;
                ed.AsControl.Dock = DockStyle.Fill;
                try { ed.Load(path, _project); }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Не удалось открыть файл:\r\n" + ex.Message,
                        "MuAdmin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _openEditors[path] = ed;
            }
            if (_active != null && _active != ed) _active.AsControl.Visible = false;
            if (!_editorHost.Controls.Contains(ed.AsControl))
                _editorHost.Controls.Add(ed.AsControl);
            ed.AsControl.Visible = true;
            ed.AsControl.BringToFront();
            _active = ed;
            _pathLabel.Text = path;
        }

        private static IFileEditor CreateEditorFor(string path)
        {
            string ext = (Path.GetExtension(path) ?? string.Empty).ToLowerInvariant();
            switch (ext)
            {
                case ".ini":
                case ".cfg":
                    return new IniEditor();
                case ".txt":
                    return new TabularEditor();
                case ".dat":
                    // Probe: small + ASCII-ish? show as text, otherwise hex.
                    return new RawTextEditor();
                default:
                    return new RawTextEditor();
            }
        }

        public void SaveActive()
        {
            if (_active == null) return;
            try { _active.Save(_project); }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Ошибка сохранения:\r\n" + ex.Message,
                    "MuAdmin", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public int SaveAllDirty()
        {
            int n = 0;
            foreach (var ed in _openEditors.Values)
            {
                if (!ed.IsDirty) continue;
                try { ed.Save(_project); n++; }
                catch (Exception ex)
                {
                    MessageBox.Show(this,
                        "Ошибка сохранения " + ed.FilePath + ":\r\n" + ex.Message,
                        "MuAdmin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return n;
        }

        public void ReloadActive()
        {
            if (_active == null) return;
            string p = _active.FilePath;
            _editorHost.Controls.Remove(_active.AsControl);
            _openEditors.Remove(p);
            _active = null;
            ShowEditor(p);
        }

        private void OnSaveClick(object sender, EventArgs e) { SaveActive(); }
        private void OnReloadClick(object sender, EventArgs e) { ReloadActive(); }
    }

    /// <summary>Common contract implemented by every concrete file editor.</summary>
    public interface IFileEditor
    {
        Control AsControl { get; }
        string FilePath { get; }
        bool IsDirty { get; }
        void Load(string path, ServerProject project);
        void Save(ServerProject project);
    }
}
