namespace Screenshot
{
    partial class ScreenCapture
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
            this.components = new System.ComponentModel.Container();
            this.statusBox = new ownControls.statusBox();
            this.label1 = new System.Windows.Forms.Label();
            this.filename = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.folder = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.SavePicsRepeated = new System.Windows.Forms.Button();
            this.seconds = new System.Windows.Forms.NumericUpDown();
            this.TakeSnapshot = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.seconds)).BeginInit();
            this.SuspendLayout();
            // 
            // statusBox
            // 
            this.statusBox.Location = new System.Drawing.Point(12, 100);
            this.statusBox.Name = "statusBox";
            this.statusBox.Size = new System.Drawing.Size(303, 150);
            this.statusBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Filename:";
            // 
            // filename
            // 
            this.filename.Location = new System.Drawing.Point(70, 6);
            this.filename.Name = "filename";
            this.filename.Size = new System.Drawing.Size(245, 20);
            this.filename.TabIndex = 2;
            this.filename.Text = "DATE_TIME_NUMBER.png";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(308, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Possible include pattern: DATE, TIME, NUMBER, TIMESTAMP";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(273, 71);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(42, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // folder
            // 
            this.folder.Location = new System.Drawing.Point(70, 47);
            this.folder.Name = "folder";
            this.folder.Size = new System.Drawing.Size(245, 20);
            this.folder.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Folder:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 71);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Browse";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyDocuments;
            // 
            // SavePicsRepeated
            // 
            this.SavePicsRepeated.Location = new System.Drawing.Point(93, 71);
            this.SavePicsRepeated.Name = "SavePicsRepeated";
            this.SavePicsRepeated.Size = new System.Drawing.Size(111, 23);
            this.SavePicsRepeated.TabIndex = 8;
            this.SavePicsRepeated.Text = "Save Pic every [ms]";
            this.SavePicsRepeated.UseVisualStyleBackColor = true;
            this.SavePicsRepeated.Click += new System.EventHandler(this.button3_Click);
            // 
            // seconds
            // 
            this.seconds.Location = new System.Drawing.Point(211, 74);
            this.seconds.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.seconds.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.seconds.Name = "seconds";
            this.seconds.Size = new System.Drawing.Size(56, 20);
            this.seconds.TabIndex = 9;
            this.seconds.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            // 
            // TakeSnapshot
            // 
            this.TakeSnapshot.Interval = 250;
            this.TakeSnapshot.Tick += new System.EventHandler(this.TakeSnapshot_Tick);
            // 
            // ScreenCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 262);
            this.Controls.Add(this.seconds);
            this.Controls.Add(this.SavePicsRepeated);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.folder);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.filename);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusBox);
            this.Name = "ScreenCapture";
            this.Text = "ScreenCapture";
            ((System.ComponentModel.ISupportInitialize)(this.seconds)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ownControls.statusBox statusBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox filename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox folder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button SavePicsRepeated;
        private System.Windows.Forms.NumericUpDown seconds;
        private System.Windows.Forms.Timer TakeSnapshot;
    }
}

