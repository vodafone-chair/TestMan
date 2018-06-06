namespace DrehtischGUI
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
            this.label2 = new System.Windows.Forms.Label();
            this.drehtisch_port_list = new System.Windows.Forms.ComboBox();
            this.statusbox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.winkel = new System.Windows.Forms.NumericUpDown();
            this.start_stop = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.serialport = new System.IO.Ports.SerialPort(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.winkel)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "COM-Port:";
            // 
            // drehtisch_port_list
            // 
            this.drehtisch_port_list.FormattingEnabled = true;
            this.drehtisch_port_list.Location = new System.Drawing.Point(74, 12);
            this.drehtisch_port_list.Name = "drehtisch_port_list";
            this.drehtisch_port_list.Size = new System.Drawing.Size(198, 21);
            this.drehtisch_port_list.TabIndex = 18;
            // 
            // statusbox
            // 
            this.statusbox.FormattingEnabled = true;
            this.statusbox.HorizontalScrollbar = true;
            this.statusbox.Location = new System.Drawing.Point(13, 102);
            this.statusbox.Name = "statusbox";
            this.statusbox.ScrollAlwaysVisible = true;
            this.statusbox.Size = new System.Drawing.Size(259, 147);
            this.statusbox.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Angle:";
            // 
            // winkel
            // 
            this.winkel.Location = new System.Drawing.Point(74, 40);
            this.winkel.Maximum = new decimal(new int[] {
            340,
            0,
            0,
            0});
            this.winkel.Name = "winkel";
            this.winkel.Size = new System.Drawing.Size(197, 20);
            this.winkel.TabIndex = 22;
            this.winkel.ValueChanged += new System.EventHandler(this.winkel_ValueChanged);
            // 
            // start_stop
            // 
            this.start_stop.Location = new System.Drawing.Point(12, 67);
            this.start_stop.Name = "start_stop";
            this.start_stop.Size = new System.Drawing.Size(123, 23);
            this.start_stop.TabIndex = 23;
            this.start_stop.Text = "Connect";
            this.start_stop.UseVisualStyleBackColor = true;
            this.start_stop.Click += new System.EventHandler(this.start_stop_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(152, 66);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(120, 23);
            this.button2.TabIndex = 24;
            this.button2.Text = "Move";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // serialport
            // 
            this.serialport.ReadTimeout = 500;
            this.serialport.ReceivedBytesThreshold = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.start_stop);
            this.Controls.Add(this.winkel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.drehtisch_port_list);
            this.Name = "Form1";
            this.Text = "DrehtischGUI";
            ((System.ComponentModel.ISupportInitialize)(this.winkel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox drehtisch_port_list;
        private System.Windows.Forms.ListBox statusbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown winkel;
        private System.Windows.Forms.Button start_stop;
        private System.Windows.Forms.Button button2;
        private System.IO.Ports.SerialPort serialport;
    }
}

