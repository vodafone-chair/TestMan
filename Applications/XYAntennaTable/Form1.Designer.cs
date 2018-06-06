namespace XYGUI
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Move_xy = new System.Windows.Forms.Button();
            this.statusbox = new System.Windows.Forms.ListBox();
            this.y_pos = new System.Windows.Forms.NumericUpDown();
            this.x_pos = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.serialportlist = new System.Windows.Forms.ComboBox();
            this.Connect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.xytisch = new System.IO.Ports.SerialPort(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.status_label = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.y_pos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.x_pos)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Move_xy
            // 
            this.Move_xy.Location = new System.Drawing.Point(187, 69);
            this.Move_xy.Name = "Move_xy";
            this.Move_xy.Size = new System.Drawing.Size(139, 23);
            this.Move_xy.TabIndex = 1;
            this.Move_xy.Text = "Move";
            this.Move_xy.UseVisualStyleBackColor = true;
            this.Move_xy.Click += new System.EventHandler(this.button1_Click);
            // 
            // statusbox
            // 
            this.statusbox.FormattingEnabled = true;
            this.statusbox.HorizontalScrollbar = true;
            this.statusbox.Location = new System.Drawing.Point(12, 98);
            this.statusbox.Name = "statusbox";
            this.statusbox.ScrollAlwaysVisible = true;
            this.statusbox.Size = new System.Drawing.Size(314, 121);
            this.statusbox.TabIndex = 2;
            // 
            // y_pos
            // 
            this.y_pos.Location = new System.Drawing.Point(122, 72);
            this.y_pos.Maximum = new decimal(new int[] {
            450,
            0,
            0,
            0});
            this.y_pos.Name = "y_pos";
            this.y_pos.Size = new System.Drawing.Size(59, 20);
            this.y_pos.TabIndex = 3;
            // 
            // x_pos
            // 
            this.x_pos.Location = new System.Drawing.Point(34, 72);
            this.x_pos.Maximum = new decimal(new int[] {
            390,
            0,
            0,
            0});
            this.x_pos.Name = "x_pos";
            this.x_pos.Size = new System.Drawing.Size(59, 20);
            this.x_pos.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "X:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(99, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Y:";
            // 
            // serialportlist
            // 
            this.serialportlist.FormattingEnabled = true;
            this.serialportlist.Location = new System.Drawing.Point(102, 12);
            this.serialportlist.Name = "serialportlist";
            this.serialportlist.Size = new System.Drawing.Size(225, 21);
            this.serialportlist.TabIndex = 7;
            // 
            // Connect
            // 
            this.Connect.Location = new System.Drawing.Point(12, 40);
            this.Connect.Name = "Connect";
            this.Connect.Size = new System.Drawing.Size(314, 23);
            this.Connect.TabIndex = 8;
            this.Connect.Text = "Connect";
            this.Connect.UseVisualStyleBackColor = true;
            this.Connect.Click += new System.EventHandler(this.Connect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Serial Port:";
            // 
            // xytisch
            // 
            this.xytisch.BaudRate = 38400;
            this.xytisch.ReadTimeout = 100;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_label});
            this.statusStrip1.Location = new System.Drawing.Point(0, 231);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(339, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // status_label
            // 
            this.status_label.Name = "status_label";
            this.status_label.Size = new System.Drawing.Size(31, 17);
            this.status_label.Text = "        ";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 253);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Connect);
            this.Controls.Add(this.serialportlist);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.x_pos);
            this.Controls.Add(this.y_pos);
            this.Controls.Add(this.statusbox);
            this.Controls.Add(this.Move_xy);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "XY-GUI";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.y_pos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.x_pos)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Move_xy;
        private System.Windows.Forms.ListBox statusbox;
        private System.Windows.Forms.NumericUpDown y_pos;
        private System.Windows.Forms.NumericUpDown x_pos;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox serialportlist;
        private System.Windows.Forms.Button Connect;
        private System.Windows.Forms.Label label3;
        private System.IO.Ports.SerialPort xytisch;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel status_label;
    }
}

