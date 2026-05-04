namespace MuAdmin
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip _menu;
        private System.Windows.Forms.ToolStripMenuItem _menuFile;
        private System.Windows.Forms.ToolStripMenuItem _menuFileOpen;
        private System.Windows.Forms.ToolStripMenuItem _menuFileSave;
        private System.Windows.Forms.ToolStripMenuItem _menuFileSaveAll;
        private System.Windows.Forms.ToolStripMenuItem _menuFileReload;
        private System.Windows.Forms.ToolStripSeparator _menuFileSep;
        private System.Windows.Forms.ToolStripMenuItem _menuFileExit;
        private System.Windows.Forms.ToolStripMenuItem _menuHelp;
        private System.Windows.Forms.ToolStripMenuItem _menuHelpAbout;
        private System.Windows.Forms.TabControl _mainTabs;
        private System.Windows.Forms.StatusStrip _status;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._menu = new System.Windows.Forms.MenuStrip();
            this._menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this._menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this._menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this._menuFileSaveAll = new System.Windows.Forms.ToolStripMenuItem();
            this._menuFileReload = new System.Windows.Forms.ToolStripMenuItem();
            this._menuFileSep = new System.Windows.Forms.ToolStripSeparator();
            this._menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this._menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this._menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this._mainTabs = new System.Windows.Forms.TabControl();
            this._status = new System.Windows.Forms.StatusStrip();
            this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();

            this._menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this._menuFile, this._menuHelp });
            this._menu.Location = new System.Drawing.Point(0, 0);
            this._menu.Name = "_menu";
            this._menu.Size = new System.Drawing.Size(1200, 24);

            this._menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
            {
                this._menuFileOpen, this._menuFileSave, this._menuFileSaveAll,
                this._menuFileReload, this._menuFileSep, this._menuFileExit
            });
            this._menuFile.Text = "Файл";

            this._menuFileOpen.Text = "Открыть папку…";
            this._menuFileOpen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            this._menuFileOpen.Click += new System.EventHandler(this.OnOpenFolder);

            this._menuFileSave.Text = "Сохранить";
            this._menuFileSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            this._menuFileSave.Click += new System.EventHandler(this.OnSaveCurrent);

            this._menuFileSaveAll.Text = "Сохранить всё";
            this._menuFileSaveAll.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            this._menuFileSaveAll.Click += new System.EventHandler(this.OnSaveAll);

            this._menuFileReload.Text = "Перечитать активный файл";
            this._menuFileReload.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this._menuFileReload.Click += new System.EventHandler(this.OnReload);

            this._menuFileExit.Text = "Выход";
            this._menuFileExit.Click += new System.EventHandler(this.OnExit);

            this._menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this._menuHelpAbout });
            this._menuHelp.Text = "Справка";
            this._menuHelpAbout.Text = "О программе";
            this._menuHelpAbout.Click += new System.EventHandler(this.OnAbout);

            this._mainTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainTabs.Location = new System.Drawing.Point(0, 24);
            this._mainTabs.Name = "_mainTabs";
            this._mainTabs.Size = new System.Drawing.Size(1200, 654);
            this._mainTabs.TabIndex = 1;

            this._status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this._statusLabel });
            this._status.Location = new System.Drawing.Point(0, 678);
            this._status.Name = "_status";
            this._status.Size = new System.Drawing.Size(1200, 22);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Text = "Готово";

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this._mainTabs);
            this.Controls.Add(this._status);
            this.Controls.Add(this._menu);
            this.MainMenuStrip = this._menu;
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MuAdmin";
        }
    }
}
