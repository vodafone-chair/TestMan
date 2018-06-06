namespace UDP_TestSuite
{
    partial class Form1
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
            this.ipBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.setupTab = new System.Windows.Forms.TabPage();
            this.label14 = new System.Windows.Forms.Label();
            this.repeatUpDown = new System.Windows.Forms.NumericUpDown();
            this.filter = new System.Windows.Forms.CheckBox();
            this.idBox = new System.Windows.Forms.NumericUpDown();
            this.typeBox = new System.Windows.Forms.NumericUpDown();
            this.ttlBox = new System.Windows.Forms.NumericUpDown();
            this.portBox = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.messageTab = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.ValueBox0 = new System.Windows.Forms.TextBox();
            this.NameBox0 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.NumberOfVariables = new System.Windows.Forms.NumericUpDown();
            this.commandTab = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.automaticExecuteAck = new System.Windows.Forms.CheckBox();
            this.automaticReceiveAck = new System.Windows.Forms.CheckBox();
            this.timeoutBox = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.executeAck = new System.Windows.Forms.Button();
            this.timestampLabel = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.receiveACK = new System.Windows.Forms.Button();
            this.streamTab = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.sendFile = new System.Windows.Forms.Button();
            this.streamStart = new System.Windows.Forms.Button();
            this.idSelector = new System.Windows.Forms.NumericUpDown();
            this.typeSelector = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.initToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendMessageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBox1 = new ownControls.statusBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.PacketListCnt = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.setupTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.repeatUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.idBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.typeBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ttlBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.portBox)).BeginInit();
            this.messageTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumberOfVariables)).BeginInit();
            this.commandTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeoutBox)).BeginInit();
            this.streamTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.idSelector)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.typeSelector)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ipBox
            // 
            this.ipBox.Location = new System.Drawing.Point(73, 11);
            this.ipBox.Name = "ipBox";
            this.ipBox.Size = new System.Drawing.Size(100, 20);
            this.ipBox.TabIndex = 0;
            this.ipBox.Text = "224.5.6.7";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP-Adresse:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.setupTab);
            this.tabControl1.Controls.Add(this.messageTab);
            this.tabControl1.Controls.Add(this.commandTab);
            this.tabControl1.Controls.Add(this.streamTab);
            this.tabControl1.Location = new System.Drawing.Point(17, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(652, 244);
            this.tabControl1.TabIndex = 2;
            // 
            // setupTab
            // 
            this.setupTab.Controls.Add(this.label14);
            this.setupTab.Controls.Add(this.repeatUpDown);
            this.setupTab.Controls.Add(this.filter);
            this.setupTab.Controls.Add(this.idBox);
            this.setupTab.Controls.Add(this.typeBox);
            this.setupTab.Controls.Add(this.ttlBox);
            this.setupTab.Controls.Add(this.portBox);
            this.setupTab.Controls.Add(this.label5);
            this.setupTab.Controls.Add(this.label4);
            this.setupTab.Controls.Add(this.label3);
            this.setupTab.Controls.Add(this.label2);
            this.setupTab.Controls.Add(this.label1);
            this.setupTab.Controls.Add(this.ipBox);
            this.setupTab.Location = new System.Drawing.Point(4, 22);
            this.setupTab.Name = "setupTab";
            this.setupTab.Padding = new System.Windows.Forms.Padding(3);
            this.setupTab.Size = new System.Drawing.Size(644, 218);
            this.setupTab.TabIndex = 0;
            this.setupTab.Text = "Setup";
            this.setupTab.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 170);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(45, 13);
            this.label14.TabIndex = 14;
            this.label14.Text = "Repeat:";
            // 
            // repeatUpDown
            // 
            this.repeatUpDown.Location = new System.Drawing.Point(73, 168);
            this.repeatUpDown.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.repeatUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.repeatUpDown.Name = "repeatUpDown";
            this.repeatUpDown.Size = new System.Drawing.Size(100, 20);
            this.repeatUpDown.TabIndex = 13;
            this.repeatUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // filter
            // 
            this.filter.AutoSize = true;
            this.filter.Checked = true;
            this.filter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.filter.Location = new System.Drawing.Point(9, 144);
            this.filter.Name = "filter";
            this.filter.Size = new System.Drawing.Size(88, 17);
            this.filter.TabIndex = 12;
            this.filter.Text = "Adapter-Filter";
            this.filter.UseVisualStyleBackColor = true;
            this.filter.CheckedChanged += new System.EventHandler(this.filter_CheckedChanged);
            // 
            // idBox
            // 
            this.idBox.Location = new System.Drawing.Point(73, 118);
            this.idBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.idBox.Name = "idBox";
            this.idBox.Size = new System.Drawing.Size(100, 20);
            this.idBox.TabIndex = 11;
            this.idBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // typeBox
            // 
            this.typeBox.Location = new System.Drawing.Point(73, 92);
            this.typeBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.typeBox.Name = "typeBox";
            this.typeBox.Size = new System.Drawing.Size(100, 20);
            this.typeBox.TabIndex = 10;
            this.typeBox.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            // 
            // ttlBox
            // 
            this.ttlBox.Location = new System.Drawing.Point(73, 66);
            this.ttlBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ttlBox.Name = "ttlBox";
            this.ttlBox.Size = new System.Drawing.Size(100, 20);
            this.ttlBox.TabIndex = 9;
            this.ttlBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // portBox
            // 
            this.portBox.Location = new System.Drawing.Point(73, 40);
            this.portBox.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(100, 20);
            this.portBox.TabIndex = 8;
            this.portBox.Value = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "ID:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Type:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "TTL:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port:";
            // 
            // messageTab
            // 
            this.messageTab.AutoScroll = true;
            this.messageTab.Controls.Add(this.label8);
            this.messageTab.Controls.Add(this.label7);
            this.messageTab.Controls.Add(this.ValueBox0);
            this.messageTab.Controls.Add(this.NameBox0);
            this.messageTab.Controls.Add(this.label6);
            this.messageTab.Controls.Add(this.NumberOfVariables);
            this.messageTab.Location = new System.Drawing.Point(4, 22);
            this.messageTab.Name = "messageTab";
            this.messageTab.Padding = new System.Windows.Forms.Padding(3);
            this.messageTab.Size = new System.Drawing.Size(644, 218);
            this.messageTab.TabIndex = 1;
            this.messageTab.Text = "Message";
            this.messageTab.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(378, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(82, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Variable values:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(78, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Variable name:";
            // 
            // ValueBox0
            // 
            this.ValueBox0.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ValueBox0.Location = new System.Drawing.Point(238, 51);
            this.ValueBox0.Name = "ValueBox0";
            this.ValueBox0.Size = new System.Drawing.Size(382, 20);
            this.ValueBox0.TabIndex = 3;
            // 
            // NameBox0
            // 
            this.NameBox0.Location = new System.Drawing.Point(9, 51);
            this.NameBox0.Name = "NameBox0";
            this.NameBox0.Size = new System.Drawing.Size(223, 20);
            this.NameBox0.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 14);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Number of variables:";
            // 
            // NumberOfVariables
            // 
            this.NumberOfVariables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumberOfVariables.Location = new System.Drawing.Point(116, 12);
            this.NumberOfVariables.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumberOfVariables.Name = "NumberOfVariables";
            this.NumberOfVariables.Size = new System.Drawing.Size(504, 20);
            this.NumberOfVariables.TabIndex = 0;
            this.NumberOfVariables.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumberOfVariables.ValueChanged += new System.EventHandler(this.NumberOfVariables_ValueChanged);
            // 
            // commandTab
            // 
            this.commandTab.Controls.Add(this.label11);
            this.commandTab.Controls.Add(this.automaticExecuteAck);
            this.commandTab.Controls.Add(this.automaticReceiveAck);
            this.commandTab.Controls.Add(this.timeoutBox);
            this.commandTab.Controls.Add(this.label10);
            this.commandTab.Controls.Add(this.executeAck);
            this.commandTab.Controls.Add(this.timestampLabel);
            this.commandTab.Controls.Add(this.label9);
            this.commandTab.Controls.Add(this.receiveACK);
            this.commandTab.Location = new System.Drawing.Point(4, 22);
            this.commandTab.Name = "commandTab";
            this.commandTab.Size = new System.Drawing.Size(644, 218);
            this.commandTab.TabIndex = 2;
            this.commandTab.Text = "Receive Command";
            this.commandTab.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(233, 155);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 13);
            this.label11.TabIndex = 10;
            this.label11.Text = "not working!";
            // 
            // automaticExecuteAck
            // 
            this.automaticExecuteAck.AutoSize = true;
            this.automaticExecuteAck.Checked = true;
            this.automaticExecuteAck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.automaticExecuteAck.Location = new System.Drawing.Point(234, 93);
            this.automaticExecuteAck.Name = "automaticExecuteAck";
            this.automaticExecuteAck.Size = new System.Drawing.Size(146, 17);
            this.automaticExecuteAck.TabIndex = 9;
            this.automaticExecuteAck.Text = "Send automatic response";
            this.automaticExecuteAck.UseVisualStyleBackColor = true;
            this.automaticExecuteAck.CheckedChanged += new System.EventHandler(this.automaticExecuteAck_CheckedChanged);
            // 
            // automaticReceiveAck
            // 
            this.automaticReceiveAck.AutoSize = true;
            this.automaticReceiveAck.Checked = true;
            this.automaticReceiveAck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.automaticReceiveAck.Location = new System.Drawing.Point(234, 65);
            this.automaticReceiveAck.Name = "automaticReceiveAck";
            this.automaticReceiveAck.Size = new System.Drawing.Size(146, 17);
            this.automaticReceiveAck.TabIndex = 8;
            this.automaticReceiveAck.Text = "Send automatic response";
            this.automaticReceiveAck.UseVisualStyleBackColor = true;
            this.automaticReceiveAck.CheckedChanged += new System.EventHandler(this.automaticReceiveAck_CheckedChanged);
            // 
            // timeoutBox
            // 
            this.timeoutBox.Location = new System.Drawing.Point(123, 31);
            this.timeoutBox.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.timeoutBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.timeoutBox.Name = "timeoutBox";
            this.timeoutBox.Size = new System.Drawing.Size(104, 20);
            this.timeoutBox.TabIndex = 7;
            this.timeoutBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(17, 34);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(106, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "Timeout (x * 100 ms):";
            // 
            // executeAck
            // 
            this.executeAck.Enabled = false;
            this.executeAck.Location = new System.Drawing.Point(18, 89);
            this.executeAck.Name = "executeAck";
            this.executeAck.Size = new System.Drawing.Size(209, 23);
            this.executeAck.TabIndex = 4;
            this.executeAck.Text = "Execution ACK";
            this.executeAck.UseVisualStyleBackColor = true;
            this.executeAck.Click += new System.EventHandler(this.executeAck_Click);
            // 
            // timestampLabel
            // 
            this.timestampLabel.AutoSize = true;
            this.timestampLabel.Location = new System.Drawing.Point(86, 15);
            this.timestampLabel.Name = "timestampLabel";
            this.timestampLabel.Size = new System.Drawing.Size(85, 13);
            this.timestampLabel.TabIndex = 2;
            this.timestampLabel.Text = "0000000000000";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 15);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(61, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "Timestamp:";
            // 
            // receiveACK
            // 
            this.receiveACK.Enabled = false;
            this.receiveACK.Location = new System.Drawing.Point(18, 60);
            this.receiveACK.Name = "receiveACK";
            this.receiveACK.Size = new System.Drawing.Size(209, 23);
            this.receiveACK.TabIndex = 0;
            this.receiveACK.Text = "Receive ACK";
            this.receiveACK.UseVisualStyleBackColor = true;
            this.receiveACK.Click += new System.EventHandler(this.receiveACK_Click);
            // 
            // streamTab
            // 
            this.streamTab.Controls.Add(this.button1);
            this.streamTab.Controls.Add(this.sendFile);
            this.streamTab.Controls.Add(this.streamStart);
            this.streamTab.Controls.Add(this.idSelector);
            this.streamTab.Controls.Add(this.typeSelector);
            this.streamTab.Controls.Add(this.label13);
            this.streamTab.Controls.Add(this.label12);
            this.streamTab.Location = new System.Drawing.Point(4, 22);
            this.streamTab.Name = "streamTab";
            this.streamTab.Size = new System.Drawing.Size(644, 218);
            this.streamTab.TabIndex = 3;
            this.streamTab.Text = "Streaming";
            this.streamTab.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(269, 73);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Stop stream";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // sendFile
            // 
            this.sendFile.Location = new System.Drawing.Point(269, 43);
            this.sendFile.Name = "sendFile";
            this.sendFile.Size = new System.Drawing.Size(75, 23);
            this.sendFile.TabIndex = 5;
            this.sendFile.Text = "Send File";
            this.sendFile.UseVisualStyleBackColor = true;
            this.sendFile.Click += new System.EventHandler(this.sendFile_Click);
            // 
            // streamStart
            // 
            this.streamStart.Location = new System.Drawing.Point(269, 14);
            this.streamStart.Name = "streamStart";
            this.streamStart.Size = new System.Drawing.Size(75, 23);
            this.streamStart.TabIndex = 4;
            this.streamStart.Text = "Start stream";
            this.streamStart.UseVisualStyleBackColor = true;
            this.streamStart.Click += new System.EventHandler(this.streamStart_Click);
            // 
            // idSelector
            // 
            this.idSelector.Location = new System.Drawing.Point(172, 17);
            this.idSelector.Maximum = new decimal(new int[] {
            254,
            0,
            0,
            0});
            this.idSelector.Name = "idSelector";
            this.idSelector.Size = new System.Drawing.Size(80, 20);
            this.idSelector.TabIndex = 3;
            // 
            // typeSelector
            // 
            this.typeSelector.Location = new System.Drawing.Point(59, 17);
            this.typeSelector.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.typeSelector.Name = "typeSelector";
            this.typeSelector.Size = new System.Drawing.Size(80, 20);
            this.typeSelector.TabIndex = 2;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(145, 19);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(21, 13);
            this.label13.TabIndex = 1;
            this.label13.Text = "ID:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(18, 19);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(34, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Type:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.initToolStripMenuItem,
            this.sendMessageToolStripMenuItem,
            this.sendCommandToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(686, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // initToolStripMenuItem
            // 
            this.initToolStripMenuItem.Name = "initToolStripMenuItem";
            this.initToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.initToolStripMenuItem.Text = "Connect";
            this.initToolStripMenuItem.Click += new System.EventHandler(this.initToolStripMenuItem_Click);
            // 
            // sendMessageToolStripMenuItem
            // 
            this.sendMessageToolStripMenuItem.Name = "sendMessageToolStripMenuItem";
            this.sendMessageToolStripMenuItem.Size = new System.Drawing.Size(94, 20);
            this.sendMessageToolStripMenuItem.Text = "Send message";
            this.sendMessageToolStripMenuItem.Click += new System.EventHandler(this.sendMessageToolStripMenuItem_Click);
            // 
            // sendCommandToolStripMenuItem
            // 
            this.sendCommandToolStripMenuItem.Name = "sendCommandToolStripMenuItem";
            this.sendCommandToolStripMenuItem.Size = new System.Drawing.Size(166, 20);
            this.sendCommandToolStripMenuItem.Text = "Send message as command";
            this.sendCommandToolStripMenuItem.Click += new System.EventHandler(this.sendCommandToolStripMenuItem_Click);
            // 
            // statusBox1
            // 
            this.statusBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusBox1.Location = new System.Drawing.Point(17, 277);
            this.statusBox1.Name = "statusBox1";
            this.statusBox1.Size = new System.Drawing.Size(652, 182);
            this.statusBox1.TabIndex = 5;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.PacketListCnt});
            this.statusStrip.Location = new System.Drawing.Point(0, 466);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(686, 22);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(84, 17);
            this.toolStripStatusLabel1.Text = "Packets in List:";
            // 
            // PacketListCnt
            // 
            this.PacketListCnt.Name = "PacketListCnt";
            this.PacketListCnt.Size = new System.Drawing.Size(13, 17);
            this.PacketListCnt.Text = "0";
            // 
            // StatusTimer
            // 
            this.StatusTimer.Enabled = true;
            this.StatusTimer.Tick += new System.EventHandler(this.StatusTimer_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 488);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.statusBox1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "UDP-Communications-Test-Suite";
            this.tabControl1.ResumeLayout(false);
            this.setupTab.ResumeLayout(false);
            this.setupTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.repeatUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.idBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.typeBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ttlBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.portBox)).EndInit();
            this.messageTab.ResumeLayout(false);
            this.messageTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumberOfVariables)).EndInit();
            this.commandTab.ResumeLayout(false);
            this.commandTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeoutBox)).EndInit();
            this.streamTab.ResumeLayout(false);
            this.streamTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.idSelector)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.typeSelector)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ipBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage setupTab;
        private System.Windows.Forms.TabPage messageTab;
        private System.Windows.Forms.TabPage commandTab;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown idBox;
        private System.Windows.Forms.NumericUpDown typeBox;
        private System.Windows.Forms.NumericUpDown ttlBox;
        private System.Windows.Forms.NumericUpDown portBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown NumberOfVariables;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem initToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendMessageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendCommandToolStripMenuItem;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox ValueBox0;
        private System.Windows.Forms.TextBox NameBox0;
        private System.Windows.Forms.Label timestampLabel;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button receiveACK;
        private System.Windows.Forms.Button executeAck;
        private System.Windows.Forms.NumericUpDown timeoutBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox automaticExecuteAck;
        private System.Windows.Forms.CheckBox automaticReceiveAck;
        private System.Windows.Forms.Label label11;
        private ownControls.statusBox statusBox1;
        private System.Windows.Forms.TabPage streamTab;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown idSelector;
        private System.Windows.Forms.NumericUpDown typeSelector;
        private System.Windows.Forms.Button streamStart;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel PacketListCnt;
        private System.Windows.Forms.Timer StatusTimer;
        private System.Windows.Forms.Button sendFile;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox filter;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown repeatUpDown;
    }
}

