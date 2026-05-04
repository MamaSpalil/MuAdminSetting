namespace MuAdmin.Tabs
{
    partial class FileCategoryTab
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.SplitContainer _split;
        private System.Windows.Forms.TreeView _tree;
        private System.Windows.Forms.Panel _editorHost;
        private System.Windows.Forms.ToolStrip _toolbar;
        private System.Windows.Forms.ToolStripButton _btnSave;
        private System.Windows.Forms.ToolStripButton _btnReload;
        private System.Windows.Forms.ToolStripLabel _pathLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._split = new System.Windows.Forms.SplitContainer();
            this._tree = new System.Windows.Forms.TreeView();
            this._editorHost = new System.Windows.Forms.Panel();
            this._toolbar = new System.Windows.Forms.ToolStrip();
            this._btnSave = new System.Windows.Forms.ToolStripButton();
            this._btnReload = new System.Windows.Forms.ToolStripButton();
            this._pathLabel = new System.Windows.Forms.ToolStripLabel();

            ((System.ComponentModel.ISupportInitialize)(this._split)).BeginInit();
            this._split.Panel1.SuspendLayout();
            this._split.Panel2.SuspendLayout();
            this._split.SuspendLayout();
            this._toolbar.SuspendLayout();
            this.SuspendLayout();

            this._toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this._btnSave, this._btnReload, this._pathLabel
            });
            this._toolbar.Location = new System.Drawing.Point(0, 0);
            this._toolbar.Size = new System.Drawing.Size(900, 25);
            this._toolbar.Dock = System.Windows.Forms.DockStyle.Top;

            this._btnSave.Text = "Сохранить";
            this._btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnSave.Click += new System.EventHandler(this.OnSaveClick);

            this._btnReload.Text = "Перечитать";
            this._btnReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnReload.Click += new System.EventHandler(this.OnReloadClick);

            this._pathLabel.Text = "(файл не выбран)";

            this._split.Dock = System.Windows.Forms.DockStyle.Fill;
            this._split.Location = new System.Drawing.Point(0, 25);
            this._split.SplitterDistance = 280;
            this._split.Size = new System.Drawing.Size(900, 535);

            this._tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tree.HideSelection = false;
            this._tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeSelect);
            this._split.Panel1.Controls.Add(this._tree);

            this._editorHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this._split.Panel2.Controls.Add(this._editorHost);

            this.Controls.Add(this._split);
            this.Controls.Add(this._toolbar);
            this.Name = "FileCategoryTab";
            this.Size = new System.Drawing.Size(900, 560);

            this._split.Panel1.ResumeLayout(false);
            this._split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._split)).EndInit();
            this._split.ResumeLayout(false);
            this._toolbar.ResumeLayout(false);
            this._toolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
