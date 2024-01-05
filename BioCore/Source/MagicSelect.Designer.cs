namespace BioCore
{
    partial class MagicSelect
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MagicSelect));
            this.label1 = new System.Windows.Forms.Label();
            this.thBox = new System.Windows.Forms.ComboBox();
            this.numBox = new System.Windows.Forms.NumericUpDown();
            this.numericBox = new System.Windows.Forms.CheckBox();
            this.minBox = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.maxBox = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(14, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Threshold";
            // 
            // thBox
            // 
            this.thBox.FormattingEnabled = true;
            this.thBox.Items.AddRange(new object[] {
            "Min",
            "Median",
            "Median - Min"});
            this.thBox.Location = new System.Drawing.Point(111, 7);
            this.thBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.thBox.Name = "thBox";
            this.thBox.Size = new System.Drawing.Size(140, 23);
            this.thBox.TabIndex = 1;
            // 
            // numBox
            // 
            this.numBox.Location = new System.Drawing.Point(111, 38);
            this.numBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numBox.Name = "numBox";
            this.numBox.Size = new System.Drawing.Size(140, 23);
            this.numBox.TabIndex = 2;
            // 
            // numericBox
            // 
            this.numericBox.AutoSize = true;
            this.numericBox.ForeColor = System.Drawing.Color.White;
            this.numericBox.Location = new System.Drawing.Point(14, 39);
            this.numericBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericBox.Name = "numericBox";
            this.numericBox.Size = new System.Drawing.Size(72, 19);
            this.numericBox.TabIndex = 3;
            this.numericBox.Text = "Numeric";
            this.numericBox.UseVisualStyleBackColor = true;
            this.numericBox.CheckedChanged += new System.EventHandler(this.numericBox_CheckedChanged);
            // 
            // minBox
            // 
            this.minBox.Location = new System.Drawing.Point(111, 68);
            this.minBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.minBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.minBox.Name = "minBox";
            this.minBox.Size = new System.Drawing.Size(140, 23);
            this.minBox.TabIndex = 4;
            this.minBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(14, 70);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "Min";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(14, 100);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Max";
            // 
            // maxBox
            // 
            this.maxBox.Location = new System.Drawing.Point(111, 98);
            this.maxBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.maxBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.maxBox.Name = "maxBox";
            this.maxBox.Size = new System.Drawing.Size(140, 23);
            this.maxBox.TabIndex = 6;
            this.maxBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // MagicSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(122)))), ((int)(((byte)(156)))));
            this.ClientSize = new System.Drawing.Size(261, 127);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.maxBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.minBox);
            this.Controls.Add(this.numericBox);
            this.Controls.Add(this.numBox);
            this.Controls.Add(this.thBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MagicSelect";
            this.ShowInTaskbar = false;
            this.Text = "Magic Select";
            ((System.ComponentModel.ISupportInitialize)(this.numBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox thBox;
        private System.Windows.Forms.NumericUpDown numBox;
        private System.Windows.Forms.CheckBox numericBox;
        private System.Windows.Forms.NumericUpDown minBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown maxBox;
    }
}