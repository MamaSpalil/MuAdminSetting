namespace MuAdmin.Controls
{
    partial class QuickAddQuestDialog
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TableLayoutPanel _layout;
        private System.Windows.Forms.Label _lblLocation;
        private System.Windows.Forms.ComboBox _cbLocation;
        private System.Windows.Forms.Label _lblMonster;
        private System.Windows.Forms.ComboBox _cbMonster;
        private System.Windows.Forms.Label _lblCount;
        private System.Windows.Forms.NumericUpDown _numCount;
        private System.Windows.Forms.Label _lblPercent;
        private System.Windows.Forms.NumericUpDown _numPercent;
        private System.Windows.Forms.CheckBox _chkAutoPercent;
        private System.Windows.Forms.Label _lblPrizeType;
        private System.Windows.Forms.ComboBox _cbPrizeType;
        private System.Windows.Forms.Label _lblPrizeValue;
        private System.Windows.Forms.NumericUpDown _numPrizeValue;
        private System.Windows.Forms.Label _lblMessage1;
        private System.Windows.Forms.TextBox _txtMessage1;
        private System.Windows.Forms.Label _lblMessage2;
        private System.Windows.Forms.TextBox _txtMessage2;
        private System.Windows.Forms.Label _lblDropCnt;
        private System.Windows.Forms.NumericUpDown _numDropCnt;

        private System.Windows.Forms.GroupBox _grpItem;
        private System.Windows.Forms.TableLayoutPanel _itemLayout;
        private System.Windows.Forms.Label _lblItem;
        private System.Windows.Forms.ComboBox _cbItem;
        private System.Windows.Forms.Label _lblItemLevel;
        private System.Windows.Forms.ComboBox _cbItemLevel;
        private System.Windows.Forms.Label _lblLvlMin;
        private System.Windows.Forms.NumericUpDown _numLvlMin;
        private System.Windows.Forms.Label _lblLvlMax;
        private System.Windows.Forms.NumericUpDown _numLvlMax;
        private System.Windows.Forms.Label _lblSkill;
        private System.Windows.Forms.NumericUpDown _numSkill;
        private System.Windows.Forms.Label _lblLuck;
        private System.Windows.Forms.NumericUpDown _numLuck;
        private System.Windows.Forms.Label _lblOpt;
        private System.Windows.Forms.NumericUpDown _numOpt;
        private System.Windows.Forms.Label _lblExc;
        private System.Windows.Forms.NumericUpDown _numExc;

        private System.Windows.Forms.Label _lblPreview;
        private System.Windows.Forms.TextBox _txtPreview;

        private System.Windows.Forms.FlowLayoutPanel _buttonRow;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Button _btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._layout = new System.Windows.Forms.TableLayoutPanel();
            this._lblLocation = new System.Windows.Forms.Label();
            this._cbLocation = new System.Windows.Forms.ComboBox();
            this._lblMonster = new System.Windows.Forms.Label();
            this._cbMonster = new System.Windows.Forms.ComboBox();
            this._lblCount = new System.Windows.Forms.Label();
            this._numCount = new System.Windows.Forms.NumericUpDown();
            this._lblPercent = new System.Windows.Forms.Label();
            this._numPercent = new System.Windows.Forms.NumericUpDown();
            this._chkAutoPercent = new System.Windows.Forms.CheckBox();
            this._lblPrizeType = new System.Windows.Forms.Label();
            this._cbPrizeType = new System.Windows.Forms.ComboBox();
            this._lblPrizeValue = new System.Windows.Forms.Label();
            this._numPrizeValue = new System.Windows.Forms.NumericUpDown();
            this._lblMessage1 = new System.Windows.Forms.Label();
            this._txtMessage1 = new System.Windows.Forms.TextBox();
            this._lblMessage2 = new System.Windows.Forms.Label();
            this._txtMessage2 = new System.Windows.Forms.TextBox();
            this._lblDropCnt = new System.Windows.Forms.Label();
            this._numDropCnt = new System.Windows.Forms.NumericUpDown();

            this._grpItem = new System.Windows.Forms.GroupBox();
            this._itemLayout = new System.Windows.Forms.TableLayoutPanel();
            this._lblItem = new System.Windows.Forms.Label();
            this._cbItem = new System.Windows.Forms.ComboBox();
            this._lblItemLevel = new System.Windows.Forms.Label();
            this._cbItemLevel = new System.Windows.Forms.ComboBox();
            this._lblLvlMin = new System.Windows.Forms.Label();
            this._numLvlMin = new System.Windows.Forms.NumericUpDown();
            this._lblLvlMax = new System.Windows.Forms.Label();
            this._numLvlMax = new System.Windows.Forms.NumericUpDown();
            this._lblSkill = new System.Windows.Forms.Label();
            this._numSkill = new System.Windows.Forms.NumericUpDown();
            this._lblLuck = new System.Windows.Forms.Label();
            this._numLuck = new System.Windows.Forms.NumericUpDown();
            this._lblOpt = new System.Windows.Forms.Label();
            this._numOpt = new System.Windows.Forms.NumericUpDown();
            this._lblExc = new System.Windows.Forms.Label();
            this._numExc = new System.Windows.Forms.NumericUpDown();

            this._lblPreview = new System.Windows.Forms.Label();
            this._txtPreview = new System.Windows.Forms.TextBox();

            this._buttonRow = new System.Windows.Forms.FlowLayoutPanel();
            this._btnOk = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this._numCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numPercent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numPrizeValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numDropCnt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLvlMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLvlMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numSkill)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLuck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numOpt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numExc)).BeginInit();
            this._layout.SuspendLayout();
            this._grpItem.SuspendLayout();
            this._itemLayout.SuspendLayout();
            this._buttonRow.SuspendLayout();
            this.SuspendLayout();

            // ---------- main 4-column layout ----------
            this._layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layout.ColumnCount = 4;
            this._layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this._layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this._layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._layout.RowCount = 11;
            for (int i = 0; i < 10; i++)
                this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._layout.Padding = new System.Windows.Forms.Padding(8);
            this._layout.AutoSize = false;

            // Row 0: Location | Monster
            this._lblLocation.Text = "Локация:";
            this._lblLocation.AutoSize = true;
            this._lblLocation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._cbLocation.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cbLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cbLocation.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            this._lblMonster.Text = "Монстр:";
            this._lblMonster.AutoSize = true;
            this._lblMonster.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._cbMonster.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cbMonster.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this._cbMonster.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._cbMonster.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._layout.Controls.Add(this._lblLocation, 0, 0);
            this._layout.Controls.Add(this._cbLocation, 1, 0);
            this._layout.Controls.Add(this._lblMonster, 2, 0);
            this._layout.Controls.Add(this._cbMonster, 3, 0);

            // Row 1: Count | Percent (with Auto checkbox)
            this._lblCount.Text = "Количество:";
            this._lblCount.AutoSize = true;
            this._lblCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._numCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numCount.Maximum = 100000;
            this._numCount.Minimum = 1;
            this._numCount.Value = 30;
            this._lblPercent.Text = "Процент (0-100):";
            this._lblPercent.AutoSize = true;
            this._lblPercent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._numPercent.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numPercent.Maximum = 100;
            this._numPercent.Value = 100;
            this._chkAutoPercent.Text = "Авто (100%)";
            this._chkAutoPercent.AutoSize = true;
            this._chkAutoPercent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._chkAutoPercent.Checked = false;
            // Place auto checkbox under the percent value.
            this._layout.Controls.Add(this._lblCount, 0, 1);
            this._layout.Controls.Add(this._numCount, 1, 1);
            this._layout.Controls.Add(this._lblPercent, 2, 1);
            this._layout.Controls.Add(this._numPercent, 3, 1);

            // Row 2: (empty) | (empty) | (empty) | Auto checkbox
            this._layout.Controls.Add(this._chkAutoPercent, 3, 2);

            // Row 3: PrizeType | PrizeValue
            this._lblPrizeType.Text = "Тип приза:";
            this._lblPrizeType.AutoSize = true;
            this._lblPrizeType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._cbPrizeType.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cbPrizeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._lblPrizeValue.Text = "PrizeValue:";
            this._lblPrizeValue.AutoSize = true;
            this._lblPrizeValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._numPrizeValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numPrizeValue.Maximum = 2000000000;
            this._numPrizeValue.Minimum = 0;
            this._numPrizeValue.Value = 0;
            this._layout.Controls.Add(this._lblPrizeType, 0, 3);
            this._layout.Controls.Add(this._cbPrizeType, 1, 3);
            this._layout.Controls.Add(this._lblPrizeValue, 2, 3);
            this._layout.Controls.Add(this._numPrizeValue, 3, 3);

            // Row 4: Message1 (spans 3 columns)
            this._lblMessage1.Text = "Message1:";
            this._lblMessage1.AutoSize = true;
            this._lblMessage1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._txtMessage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layout.Controls.Add(this._lblMessage1, 0, 4);
            this._layout.Controls.Add(this._txtMessage1, 1, 4);
            this._layout.SetColumnSpan(this._txtMessage1, 3);

            // Row 5: Message2 (spans 3 columns)
            this._lblMessage2.Text = "Message2:";
            this._lblMessage2.AutoSize = true;
            this._lblMessage2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._txtMessage2.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layout.Controls.Add(this._lblMessage2, 0, 5);
            this._layout.Controls.Add(this._txtMessage2, 1, 5);
            this._layout.SetColumnSpan(this._txtMessage2, 3);

            // Row 6: DropCnt
            this._lblDropCnt.Text = "DropCnt:";
            this._lblDropCnt.AutoSize = true;
            this._lblDropCnt.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._numDropCnt.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numDropCnt.Maximum = 1000;
            this._numDropCnt.Minimum = 0;
            this._numDropCnt.Value = 1;
            this._layout.Controls.Add(this._lblDropCnt, 0, 6);
            this._layout.Controls.Add(this._numDropCnt, 1, 6);

            // Row 7: Item group box (spans all 4 columns)
            this._grpItem.Text = "Приз — вещь (PrizeType = 0)";
            this._grpItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grpItem.AutoSize = true;
            this._grpItem.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._layout.Controls.Add(this._grpItem, 0, 7);
            this._layout.SetColumnSpan(this._grpItem, 4);

            this._itemLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._itemLayout.ColumnCount = 4;
            this._itemLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this._itemLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._itemLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this._itemLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._itemLayout.RowCount = 4;
            for (int i = 0; i < 4; i++)
                this._itemLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._itemLayout.AutoSize = true;
            this._itemLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._itemLayout.Padding = new System.Windows.Forms.Padding(4);
            this._grpItem.Controls.Add(this._itemLayout);

            // Item row 0: Item picker (spans 3 cols) and Item level
            this._lblItem.Text = "Вещь:";
            this._lblItem.AutoSize = true;
            this._lblItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._cbItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cbItem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this._cbItem.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this._cbItem.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this._cbItem.DropDownHeight = 320;
            this._itemLayout.Controls.Add(this._lblItem, 0, 0);
            this._itemLayout.Controls.Add(this._cbItem, 1, 0);
            this._itemLayout.SetColumnSpan(this._cbItem, 3);

            // Item row 1: ItemLevel picker
            this._lblItemLevel.Text = "Уровень имени:";
            this._lblItemLevel.AutoSize = true;
            this._lblItemLevel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._cbItemLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cbItemLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._itemLayout.Controls.Add(this._lblItemLevel, 0, 1);
            this._itemLayout.Controls.Add(this._cbItemLevel, 1, 1);
            this._itemLayout.SetColumnSpan(this._cbItemLevel, 3);

            // Item row 2: LvlMin / LvlMax
            this._lblLvlMin.Text = "LvlMin:";
            this._lblLvlMin.AutoSize = true;
            this._lblLvlMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._numLvlMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numLvlMin.Maximum = 15;
            this._numLvlMin.Minimum = 0;
            this._lblLvlMax.Text = "LvlMax:";
            this._lblLvlMax.AutoSize = true;
            this._lblLvlMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._numLvlMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this._numLvlMax.Maximum = 15;
            this._numLvlMax.Minimum = 0;
            this._itemLayout.Controls.Add(this._lblLvlMin, 0, 2);
            this._itemLayout.Controls.Add(this._numLvlMin, 1, 2);
            this._itemLayout.Controls.Add(this._lblLvlMax, 2, 2);
            this._itemLayout.Controls.Add(this._numLvlMax, 3, 2);

            // Item row 3: Skill / Luck / Opt / Exc — each is label+up/down in a flow.
            var skillLuckLayout = new System.Windows.Forms.TableLayoutPanel();
            skillLuckLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            skillLuckLayout.AutoSize = true;
            skillLuckLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            skillLuckLayout.ColumnCount = 8;
            for (int i = 0; i < 8; i++)
                skillLuckLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(
                    i % 2 == 0 ? System.Windows.Forms.SizeType.AutoSize : System.Windows.Forms.SizeType.Percent, 25F));

            this._lblSkill.Text = "Skill:";  this._lblSkill.AutoSize = true; this._lblSkill.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblLuck.Text  = "Luck:";   this._lblLuck.AutoSize = true;  this._lblLuck.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblOpt.Text   = "Opt:";    this._lblOpt.AutoSize = true;   this._lblOpt.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblExc.Text   = "Exc:";    this._lblExc.AutoSize = true;   this._lblExc.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._numSkill.Dock = System.Windows.Forms.DockStyle.Fill; this._numSkill.Maximum = 1;   this._numSkill.Minimum = 0;
            this._numLuck.Dock  = System.Windows.Forms.DockStyle.Fill; this._numLuck.Maximum  = 1;   this._numLuck.Minimum  = 0;
            this._numOpt.Dock   = System.Windows.Forms.DockStyle.Fill; this._numOpt.Maximum   = 7;   this._numOpt.Minimum   = 0;
            this._numExc.Dock   = System.Windows.Forms.DockStyle.Fill; this._numExc.Maximum   = 63;  this._numExc.Minimum   = 0;
            skillLuckLayout.Controls.Add(this._lblSkill, 0, 0); skillLuckLayout.Controls.Add(this._numSkill, 1, 0);
            skillLuckLayout.Controls.Add(this._lblLuck,  2, 0); skillLuckLayout.Controls.Add(this._numLuck,  3, 0);
            skillLuckLayout.Controls.Add(this._lblOpt,   4, 0); skillLuckLayout.Controls.Add(this._numOpt,   5, 0);
            skillLuckLayout.Controls.Add(this._lblExc,   6, 0); skillLuckLayout.Controls.Add(this._numExc,   7, 0);

            this._itemLayout.Controls.Add(skillLuckLayout, 0, 3);
            this._itemLayout.SetColumnSpan(skillLuckLayout, 4);

            // Row 8: Preview label
            this._lblPreview.Text = "Предпросмотр строки:";
            this._lblPreview.AutoSize = true;
            this._lblPreview.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._layout.Controls.Add(this._lblPreview, 0, 8);
            this._layout.SetColumnSpan(this._lblPreview, 4);

            // Row 9: Preview textbox
            this._txtPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtPreview.Multiline = true;
            this._txtPreview.ReadOnly = true;
            this._txtPreview.WordWrap = false;
            this._txtPreview.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this._txtPreview.Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericMonospace, 8.5F);
            this._txtPreview.Height = 56;
            this._layout.Controls.Add(this._txtPreview, 0, 9);
            this._layout.SetColumnSpan(this._txtPreview, 4);

            // Row 10: button row (right-aligned)
            this._buttonRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._buttonRow.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this._buttonRow.AutoSize = true;
            this._btnOk.Text = "Добавить";
            this._btnOk.AutoSize = true;
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnCancel.Text = "Отмена";
            this._btnCancel.AutoSize = true;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonRow.Controls.Add(this._btnOk);
            this._buttonRow.Controls.Add(this._btnCancel);
            this._layout.Controls.Add(this._buttonRow, 0, 10);
            this._layout.SetColumnSpan(this._buttonRow, 4);

            // ---------- form ----------
            this.AcceptButton = this._btnOk;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(720, 600);
            this.MinimumSize = new System.Drawing.Size(640, 540);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Быстрое добавление квеста";
            this.Controls.Add(this._layout);

            ((System.ComponentModel.ISupportInitialize)(this._numCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numPercent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numPrizeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numDropCnt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLvlMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLvlMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numSkill)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLuck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numOpt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numExc)).EndInit();
            this._layout.ResumeLayout(false);
            this._layout.PerformLayout();
            this._grpItem.ResumeLayout(false);
            this._grpItem.PerformLayout();
            this._itemLayout.ResumeLayout(false);
            this._itemLayout.PerformLayout();
            this._buttonRow.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
