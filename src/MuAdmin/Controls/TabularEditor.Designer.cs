namespace MuAdmin.Controls
{
    partial class TabularEditor
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView _grid;
        private System.Windows.Forms.StatusStrip _status;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._grid = new System.Windows.Forms.DataGridView();
            this._status = new System.Windows.Forms.StatusStrip();
            this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this._grid)).BeginInit();
            this._status.SuspendLayout();
            this.SuspendLayout();

            this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grid.AllowUserToAddRows = false;
            this._grid.AllowUserToDeleteRows = false;
            this._grid.AllowUserToOrderColumns = false;
            this._grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this._grid.RowHeadersVisible = false;
            this._grid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnCellChanged);

            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Text = "(нет файла)";
            this._status.Items.Add(this._statusLabel);
            this._status.Dock = System.Windows.Forms.DockStyle.Bottom;

            this.Controls.Add(this._grid);
            this.Controls.Add(this._status);
            this.Name = "TabularEditor";
            this.Size = new System.Drawing.Size(800, 500);
            ((System.ComponentModel.ISupportInitialize)(this._grid)).EndInit();
            this._status.ResumeLayout(false);
            this._status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
