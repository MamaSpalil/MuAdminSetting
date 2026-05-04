namespace MuAdmin.Controls
{
    partial class RawTextEditor
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox _text;
        private System.Windows.Forms.StatusStrip _status;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._text = new System.Windows.Forms.TextBox();
            this._status = new System.Windows.Forms.StatusStrip();
            this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._status.SuspendLayout();
            this.SuspendLayout();

            this._text.Dock = System.Windows.Forms.DockStyle.Fill;
            this._text.Multiline = true;
            this._text.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._text.WordWrap = false;
            this._text.AcceptsTab = true;
            this._text.AcceptsReturn = true;
            this._text.Font = new System.Drawing.Font("Consolas", 9F);
            this._text.TextChanged += new System.EventHandler(this.OnTextChanged);

            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Text = "(нет файла)";
            this._status.Items.Add(this._statusLabel);
            this._status.Dock = System.Windows.Forms.DockStyle.Bottom;

            this.Controls.Add(this._text);
            this.Controls.Add(this._status);
            this.Name = "RawTextEditor";
            this.Size = new System.Drawing.Size(800, 500);
            this._status.ResumeLayout(false);
            this._status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
