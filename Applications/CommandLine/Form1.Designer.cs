namespace CTRLGUI
{
    partial class CtrlGUI
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.position_label = new System.Windows.Forms.ToolStripStatusLabel();
            this.host_name = new System.Windows.Forms.ToolStripStatusLabel();
            this.lip = new System.Windows.Forms.ToolStripStatusLabel();
            this.pip = new System.Windows.Forms.ToolStripStatusLabel();
            this.Zeitgeber = new System.Windows.Forms.Timer(this.components);
            this.statusbox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.update_intervall = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.exec_Button = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.update_intervall)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.position_label,
            this.host_name,
            this.lip,
            this.pip});
            this.statusStrip1.Location = new System.Drawing.Point(0, 334);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(379, 24);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // position_label
            // 
            this.position_label.Name = "position_label";
            this.position_label.Size = new System.Drawing.Size(0, 19);
            // 
            // host_name
            // 
            this.host_name.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.host_name.Name = "host_name";
            this.host_name.Size = new System.Drawing.Size(59, 19);
            this.host_name.Text = "                ";
            // 
            // lip
            // 
            this.lip.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lip.Name = "lip";
            this.lip.Size = new System.Drawing.Size(68, 19);
            this.lip.Text = "                   ";
            // 
            // pip
            // 
            this.pip.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.pip.Name = "pip";
            this.pip.Size = new System.Drawing.Size(56, 19);
            this.pip.Text = "               ";
            // 
            // Zeitgeber
            // 
            this.Zeitgeber.Enabled = true;
            this.Zeitgeber.Interval = 60000;
            this.Zeitgeber.Tick += new System.EventHandler(this.Zeitgeber_Tick);
            // 
            // statusbox
            // 
            this.statusbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusbox.FormattingEnabled = true;
            this.statusbox.HorizontalExtent = 3000;
            this.statusbox.HorizontalScrollbar = true;
            this.statusbox.Location = new System.Drawing.Point(12, 39);
            this.statusbox.Name = "statusbox";
            this.statusbox.ScrollAlwaysVisible = true;
            this.statusbox.Size = new System.Drawing.Size(357, 251);
            this.statusbox.TabIndex = 25;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 311);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Update-Interval [s]:";
            // 
            // update_intervall
            // 
            this.update_intervall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.update_intervall.Location = new System.Drawing.Point(116, 309);
            this.update_intervall.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.update_intervall.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.update_intervall.Name = "update_intervall";
            this.update_intervall.Size = new System.Drawing.Size(253, 20);
            this.update_intervall.TabIndex = 27;
            this.update_intervall.Value = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Command:";
            // 
            // commandTextBox
            // 
            this.commandTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commandTextBox.Location = new System.Drawing.Point(72, 9);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(208, 20);
            this.commandTextBox.TabIndex = 29;
            // 
            // exec_Button
            // 
            this.exec_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exec_Button.Location = new System.Drawing.Point(286, 7);
            this.exec_Button.Name = "exec_Button";
            this.exec_Button.Size = new System.Drawing.Size(83, 23);
            this.exec_Button.TabIndex = 30;
            this.exec_Button.Text = "Execute";
            this.exec_Button.UseVisualStyleBackColor = true;
            this.exec_Button.Click += new System.EventHandler(this.exec_Button_Click);
            // 
            // CtrlGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 358);
            this.Controls.Add(this.exec_Button);
            this.Controls.Add(this.commandTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.update_intervall);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusbox);
            this.Controls.Add(this.statusStrip1);
            this.MaximizeBox = false;
            this.Name = "CtrlGUI";
            this.Text = "CtrlGUI";
            this.Load += new System.EventHandler(this.GPSGUI_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.update_intervall)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel position_label;
        private System.Windows.Forms.Timer Zeitgeber;
        private System.Windows.Forms.ListBox statusbox;
        private System.Windows.Forms.ToolStripStatusLabel host_name;
        private System.Windows.Forms.ToolStripStatusLabel lip;
        private System.Windows.Forms.ToolStripStatusLabel pip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown update_intervall;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.Button exec_Button;
    }
}

