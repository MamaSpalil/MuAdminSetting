namespace MuAdmin.Controls
{
    partial class QuestSystemEditor
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.SplitContainer _split;
        private System.Windows.Forms.DataGridView _headerGrid;
        private System.Windows.Forms.DataGridView _grid;
        private System.Windows.Forms.StatusStrip _status;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
        private System.Windows.Forms.ToolStrip _toolbar;
        private System.Windows.Forms.ToolStripButton _btnAddQuest;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._split = new System.Windows.Forms.SplitContainer();
            this._headerGrid = new System.Windows.Forms.DataGridView();
            this._grid = new System.Windows.Forms.DataGridView();
            this._status = new System.Windows.Forms.StatusStrip();
            this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._toolbar = new System.Windows.Forms.ToolStrip();
            this._btnAddQuest = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this._split)).BeginInit();
            this._split.Panel1.SuspendLayout();
            this._split.Panel2.SuspendLayout();
            this._split.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._headerGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
            this._status.SuspendLayout();
            this.SuspendLayout();

            this._split.Dock = System.Windows.Forms.DockStyle.Fill;
            this._split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this._split.SplitterDistance = 120;
            this._split.Panel1.Controls.Add(this._headerGrid);
            this._split.Panel2.Controls.Add(this._grid);

            this._headerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._headerGrid.AllowUserToAddRows = false;
            this._headerGrid.AllowUserToDeleteRows = false;
            this._headerGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this._headerGrid.RowHeadersVisible = false;
            this._headerGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnHeaderCellChanged);

            this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grid.AllowUserToAddRows = false;
            this._grid.AllowUserToDeleteRows = false;
            this._grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this._grid.RowHeadersVisible = false;
            this._grid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnQuestCellChanged);
            this._grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.OnGridDataError);

            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Text = "(нет файла)";
            this._status.Items.Add(this._statusLabel);
            this._status.Dock = System.Windows.Forms.DockStyle.Bottom;

            this._toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this._toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._btnAddQuest.Text = "+ Добавить квест";
            this._btnAddQuest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnAddQuest.ToolTipText = "Быстрое добавление квеста (Ctrl+N)";
            this._btnAddQuest.Click += new System.EventHandler(this.OnAddQuestClick);
            this._toolbar.Items.Add(this._btnAddQuest);

            this.Controls.Add(this._split);
            this.Controls.Add(this._toolbar);
            this.Controls.Add(this._status);
            this.Name = "QuestSystemEditor";
            this.Size = new System.Drawing.Size(900, 600);

            this._split.Panel1.ResumeLayout(false);
            this._split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._split)).EndInit();
            this._split.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._headerGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
            this._status.ResumeLayout(false);
            this._status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
