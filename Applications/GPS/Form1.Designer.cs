namespace GPSGUI
{
    partial class GPSGUI
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
            this.abstand_letzte_messung = new System.Windows.Forms.ToolStripStatusLabel();
            this.latitude_label = new System.Windows.Forms.ToolStripStatusLabel();
            this.longitude_label = new System.Windows.Forms.ToolStripStatusLabel();
            this.height_label = new System.Windows.Forms.ToolStripStatusLabel();
            this.time_label = new System.Windows.Forms.ToolStripStatusLabel();
            this.connection_button = new System.Windows.Forms.Button();
            this.gps_port_list = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gps = new System.IO.Ports.SerialPort(this.components);
            this.Zeitgeber = new System.Windows.Forms.Timer(this.components);
            this.statusbox = new System.Windows.Forms.ListBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.position_label,
            this.abstand_letzte_messung,
            this.latitude_label,
            this.longitude_label,
            this.height_label,
            this.time_label});
            this.statusStrip1.Location = new System.Drawing.Point(0, 243);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(272, 24);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // position_label
            // 
            this.position_label.Name = "position_label";
            this.position_label.Size = new System.Drawing.Size(0, 19);
            // 
            // abstand_letzte_messung
            // 
            this.abstand_letzte_messung.Name = "abstand_letzte_messung";
            this.abstand_letzte_messung.Size = new System.Drawing.Size(28, 19);
            this.abstand_letzte_messung.Text = "       ";
            // 
            // latitude_label
            // 
            this.latitude_label.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.latitude_label.Name = "latitude_label";
            this.latitude_label.Size = new System.Drawing.Size(59, 19);
            this.latitude_label.Text = "                ";
            // 
            // longitude_label
            // 
            this.longitude_label.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.longitude_label.Name = "longitude_label";
            this.longitude_label.Size = new System.Drawing.Size(68, 19);
            this.longitude_label.Text = "                   ";
            // 
            // height_label
            // 
            this.height_label.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.height_label.Name = "height_label";
            this.height_label.Size = new System.Drawing.Size(56, 19);
            this.height_label.Text = "               ";
            // 
            // time_label
            // 
            this.time_label.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.time_label.Name = "time_label";
            this.time_label.Size = new System.Drawing.Size(53, 19);
            this.time_label.Text = "              ";
            // 
            // connection_button
            // 
            this.connection_button.Location = new System.Drawing.Point(12, 205);
            this.connection_button.Name = "connection_button";
            this.connection_button.Size = new System.Drawing.Size(250, 27);
            this.connection_button.TabIndex = 15;
            this.connection_button.Text = "Connect";
            this.connection_button.UseVisualStyleBackColor = true;
            this.connection_button.Click += new System.EventHandler(this.connection_button_Click);
            // 
            // gps_port_list
            // 
            this.gps_port_list.FormattingEnabled = true;
            this.gps_port_list.Location = new System.Drawing.Point(114, 12);
            this.gps_port_list.Name = "gps_port_list";
            this.gps_port_list.Size = new System.Drawing.Size(148, 21);
            this.gps_port_list.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "COM-Port:";
            // 
            // gps
            // 
            this.gps.BaudRate = 4800;
            this.gps.DataBits = 7;
            // 
            // Zeitgeber
            // 
            this.Zeitgeber.Interval = 500;
            this.Zeitgeber.Tick += new System.EventHandler(this.Zeitgeber_Tick);
            // 
            // statusbox
            // 
            this.statusbox.FormattingEnabled = true;
            this.statusbox.Location = new System.Drawing.Point(12, 39);
            this.statusbox.Name = "statusbox";
            this.statusbox.Size = new System.Drawing.Size(250, 160);
            this.statusbox.TabIndex = 25;
            // 
            // GPSGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(272, 267);
            this.Controls.Add(this.statusbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.connection_button);
            this.Controls.Add(this.gps_port_list);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "GPSGUI";
            this.Text = "GPSGUI";
            this.Load += new System.EventHandler(this.GPSGUI_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button connection_button;
        private System.Windows.Forms.ComboBox gps_port_list;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripStatusLabel position_label;
        private System.Windows.Forms.ToolStripStatusLabel time_label;
        private System.IO.Ports.SerialPort gps;
        private System.Windows.Forms.Timer Zeitgeber;
        private System.Windows.Forms.ToolStripStatusLabel abstand_letzte_messung;
        private System.Windows.Forms.ListBox statusbox;
        private System.Windows.Forms.ToolStripStatusLabel latitude_label;
        private System.Windows.Forms.ToolStripStatusLabel longitude_label;
        private System.Windows.Forms.ToolStripStatusLabel height_label;
    }
}

