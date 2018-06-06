using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using UDP_Communications;

using System.Threading;

using System.IO;

namespace UDP_TestSuite
{
    public partial class Form1 : Form
    {
        //Listen zum automatisiertes Auslesen aller eingebenen Informationen für ein paket
        List<TextBox> MessageTextboxName = new List<TextBox>();
        List<TextBox> MessageTextboxVariable = new List<TextBox>();
        
        //Abstand der Textfelder untereinander
        int abstand = 10;

        //Kommunikationsklasse
        UDP_Server Server;

        //Testfunktion
        Boolean ButtonPressed = false;

        //Für empfangene Binärdaten
        byte[] ByteMessage = null;

        public Form1()
        {
            InitializeComponent();

            //Hinzufügen der 2 Textfelder zur Liste --> Automatisiertes Auslesen aller eingebenen Informationen für ein paket
            MessageTextboxName.Add(NameBox0);
            MessageTextboxVariable.Add(ValueBox0);
        }

        private void EmpfangeDaten(object sender, DatenEventArgs e)
        {
            try
            {
                //Überprüfung ob richtiges Paket oder nur eine Mitteilung von der Serverklasse erhalten
                if ((e.Sender_Typ != 0) && (e.Sender_ID != 0))
                {
                    if (e.Inhalt == "data")
                    {
                        //Daten über einen Stream erhalten
                        ByteMessage = Server.getStreamData(e.Sender_Typ, e.Sender_ID);
                    }
                    else
                    {
                        //Paket über UDP empfangen
                        if (e.Paket.isStatus) evaluateStatusMessage(e.Paket); //Statusnachricht
                        else evaluateCommand(e.Paket);    //Befehl
                    }
                }
                else
                {
                    //Mitteilung vom Server erhalten, z.B. Fehler, Statusbericht
                    if (e.Variablen_name == "error") error(null, "UDP-Server-Error: " + e.Inhalt);
                    else if (e.Variablen_name == "hint") statusBox1.print("Hint from UDP-Server: " + e.Inhalt);
                    else statusBox1.print("Warning from UDP-Server: " + e.Inhalt);
                }
            }
            catch (Exception ex)
            {
                statusBox1.print("Error @ receive packets: " + ex.Message + ":" + ex.ToString());
            }
        }

        private void evaluateStatusMessage(Packet Paket)
        {
            statusBox1.print("Status: " + Paket.toString(";", true));
        }

        private void evaluateCommand(Packet Paket)
        {            
            string timestamp = Paket.Timestamp;
            Server.send_answer(true, "10000", timestamp);

            //Befehl ausführen
            statusBox1.print("Command: " + Paket.toString(";", true));

            Server.send_answer(false, null, timestamp);
        }

        public void Append(string sFilename, string sLines)
        {
            ///<summary>
            /// Fügt den übergebenen Text an das Ende einer Textdatei an.
            ///</summary>
            ///<param name="sFilename">Pfad zur Datei</param>
            ///<param name="sLines">anzufügender Text</param>
            ///
            StreamWriter myFile = new StreamWriter(sFilename, true);
            myFile.Write(sLines);
            myFile.Close();
        }
        public void error(Exception ex, string msg)
        {
            //Zeigt Fehlermeldungen an und speichert sie im Logfile
            statusBox1.print(msg);
            DateTime currentDate = DateTime.Now;

            if (ex != null)
            {
                statusBox1.print(ex.Message);
                Append("error.log", currentDate.ToString() + " " + msg + "\n\r" + ex.ToString() + "\n\r");
            }
            else
            {
                Append("error.log", currentDate.ToString() + " " + msg + "\n\r");
            }
        }

        //public void EmpfangeDaten(object sender, DatenEventArgs e)
        //{
        //    //Temporäre Variablen zum Auslesen von Daten aus einem Paket
        //    string timestamp = null;

        //    if (e.Paket == null)
        //    {
        //        if (e.Variablen_name == null)
        //        {
        //            //Um Applikation nicht einfrieren zu lassen
        //            Application.DoEvents();
        //            return;
        //        }
        //        else
        //        {
        //            //Mitteilung vom Server erhalten, z.B. Fehler, Statusbericht
        //            if (e.Variablen_name.Equals("error", StringComparison.InvariantCultureIgnoreCase) == true)
        //                hf.error(null, "UDP-Server-Error: " + e.Inhalt);
        //            else if (e.Variablen_name.Equals("hint", StringComparison.InvariantCultureIgnoreCase) == true)
        //                hf.display("Hint from UDP-Server: " + e.Inhalt);
        //        }
        //    }
        //    else
        //    {
        //        //Paket empfangen
        //        //Unterscheidung ob Befehl oder normales Paket
        //        if (e.Inhalt == null)
        //        {
        //            //Statusmessage
        //            hf.display("Received status message from type " + e.Sender_Typ.ToString() + " with ID " + e.Sender_ID.ToString());
        //            hf.display(Server.DictionaryToString(e.Paket));
        //        }
        //        else
        //        {
        //            //Befehl
        //            hf.display("Received command from type " + e.Sender_Typ.ToString() + " with ID " + e.Sender_ID.ToString());
        //            hf.display(Server.DictionaryToString(e.Paket));

        //            //Dekodierung
        //            switch (e.Inhalt.ToLower())
        //            {
        //                case "ping":
        //                    //Empfangsbestätigung, wenn angefordert
        //                    if (e.Paket.TryGetValue("timestamp", out timestamp)) Server.send_answer("received", "10", timestamp);

        //                    //Befehl ausführen
        //                    hf.display("Ping");

        //                    //Ausführungsbestätigung
        //                    if (e.Paket.TryGetValue("timestamp", out timestamp)) Server.send_answer("executed", null, timestamp);

        //                    break;

        //                case "test":

        //                    //Test command empfangen --> wechsle zu entsprechendem Tab
        //                    if (tabControl1.InvokeRequired) //Cross Thread Operation?
        //                    {   //Ja! Dann Befehl mit Invoke in ursprünglichen Thread ausführen
        //                        tabControl1.Invoke(new Action(delegate() { tabControl1.SelectedIndex = 2; }));
        //                    }
        //                    else
        //                    {   //Nein, alles läuft im selben Thread
        //                        tabControl1.SelectedIndex = 2;
        //                    }
                            
        //                    //Empfangsbestätigung, wenn angefordert
        //                    if (e.Paket.TryGetValue("timestamp", out timestamp))
        //                    {

        //                        if (timestampLabel.InvokeRequired) timestampLabel.Invoke(new Action(delegate() { timestampLabel.Text = timestamp; }));
        //                        else timestampLabel.Text = timestamp;


        //                        hf.display("Sending receive ACK...Waiting for 2 way handshake ACK");
        //                        if (automaticReceiveAck.Checked) {                                    
        //                            Server.send_answer("received", timeoutBox.Value.ToString(), timestamp);
        //                        }
        //                        else
        //                        {
        //                            waitForButtonPress(timestamp, receiveACK);
        //                            Server.send_answer("received", timeoutBox.Value.ToString(), timestamp);
        //                        }
        //                    }

        //                    //Befehl ausführen
        //                    //.....

        //                    //Ausführungsbestätigung
        //                    if (e.Paket.TryGetValue("timestamp", out timestamp))
        //                    {
        //                        hf.display("Sending execute ACK...Waiting for 2 way handshake ACK");
        //                        if (automaticExecuteAck.Checked)
        //                        {
        //                            Server.send_answer("executed", null, timestamp);
        //                        }
        //                        else
        //                        {
        //                            waitForButtonPress(timestamp, executeAck);
        //                            Server.send_answer("executed", null, timestamp);
        //                        }
        //                    }
                            
        //                    break;

        //                default:
        //                    //Unknown command
        //                    break;
        //            }
        //        }
        //    }
        //}

        private void waitForButtonPress(string timestamp, Button button)
        {
            //Funktion wird aus anderem Thread aufgegerufen
            MethodInvoker LabelUpdate = delegate
            {
                Color original = button.BackColor;
                ButtonPressed = false;
                while (ButtonPressed == false)
                {
                    //Button flackern lassen
                    if (button.BackColor == original)
                    {
                        button.BackColor = Color.Red;
                    }
                    else
                    {
                        button.BackColor = original;
                    }
                    //Damit GUI nicht zufriert
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
                button.BackColor = original;

            };
            Invoke(LabelUpdate);
            
        }

        private void NumberOfVariables_ValueChanged(object sender, EventArgs e)
        {
            //Fügt mehr Felder hinzu, wenn gewünscht
            //erlaubt Eingabe mehrer Variablen + Inhalt
            if (NumberOfVariables.Value > MessageTextboxName.Count)
            {
                //Variablen name
                TextBox NameBox = new TextBox();
                TextBox PreviousNameBox = MessageTextboxName[MessageTextboxName.Count - 1];
                NameBox.Location = new Point(PreviousNameBox.Location.X, PreviousNameBox.Location.Y + PreviousNameBox.Height + abstand);
                NameBox.Size = PreviousNameBox.Size;
                messageTab.Controls.Add(NameBox);
                MessageTextboxName.Add(NameBox);

                //Variablen inhalt
                TextBox ValueBox = new TextBox();
                TextBox PrevoiusValueBox = MessageTextboxVariable[MessageTextboxVariable.Count-1];
                ValueBox.Location = new Point(PrevoiusValueBox.Location.X, PrevoiusValueBox.Location.Y + PrevoiusValueBox.Height + abstand);
                ValueBox.Size = PrevoiusValueBox.Size;
                ValueBox.Anchor = PrevoiusValueBox.Anchor;
                messageTab.Controls.Add(ValueBox);
                MessageTextboxVariable.Add(ValueBox);
                
            } else if (NumberOfVariables.Value < MessageTextboxName.Count) {
                //Eigentlich nicht benötigt...
            }
        }

        private void initToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (initToolStripMenuItem.Text != "Connect")
            {
                Server.stop_udp();
                Server = null;
                initToolStripMenuItem.Text = "Connect";
            }
            else
            {
                //Initialisierung der Kommunikationsklasse
                Server = new UDP_Server(this.EmpfangeDaten);

                Server.adapter_filter(filter.Checked);

                statusBox1.print("Please wait...");
                //Initialisiere Server auf gegebene Multicast Adresse
                //Erfolgreich oder nicht??                
                if (Server.init_udp(ipBox.Text, (int)portBox.Value, (int)ttlBox.Value, (byte)typeBox.Value, (byte)idBox.Value, true) == false) initToolStripMenuItem.Text = "Connect"; //Nein
                else
                {
                    //Ja!
                    typeBox.Value = Server.getServerType();
                    idBox.Value = Server.getServerID();
                    initToolStripMenuItem.Text = "Disconnect";
                }
            }
        }

        private void sendMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Lese alle Textfelder nacheinander aus und bastle ein Paket zusammen
            Dictionary<string, string> Paket = new Dictionary<string, string>();

            if (Server.udp_active() == true)
            {
                for (int i = 0; i < MessageTextboxName.Count; i++)
                {
                    TextBox NameBox = MessageTextboxName[i];
                    TextBox ValueBox = MessageTextboxVariable[i];

                    if (NameBox.Text != "")
                    {
                        Paket.Add(NameBox.Text, ValueBox.Text);                        
                    }
                }
                //Paket wird versendet
                if (Paket.Count > 0)
                {
                    for (int i = 0; i < repeatUpDown.Value; i++)
                    {
                        statusBox1.print(i.ToString());
                        Server.SendData(Paket);
                        Thread.Yield();
                        Thread.Sleep(1);
                        Application.DoEvents();
                            
                    }
                }
                else statusBox1.print("Packet not send, because no data was entered!");
            }
        }

        private void sendCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Lese alle Textfelder nacheinander aus und bastle ein Paket zusammen
            Dictionary<string, string> Paket = new Dictionary<string, string>();
            //Ist überhaupt das command key word eingegeben???
            Boolean commandKeyWord = false;

            if (Server.udp_active() == true)
            {
                for (int i = 0; i < MessageTextboxName.Count; i++)
                {
                    TextBox NameBox = MessageTextboxName[i];
                    TextBox ValueBox = MessageTextboxVariable[i];

                    if (NameBox.Text != "")
                    {
                        Paket.Add(NameBox.Text, ValueBox.Text);
                    }

                    if (NameBox.Text.ToLower() == "command") commandKeyWord = true;
                }

                //Paket wird versendet
                 if ((Paket.Count > 0) && (commandKeyWord == true))
                {
                    for (int i = 0; i < repeatUpDown.Value; i++)
                    {
                        statusBox1.print(i.ToString());
                        Server.SendCommand(Paket);
                        Thread.Yield();
                        Thread.Sleep(1);
                        Application.DoEvents();
                    }
                }
                else statusBox1.print("Command not send, because no data was entered or command key word is missing!");
            }
        }

        private void automaticReceiveAck_CheckedChanged(object sender, EventArgs e)
        {
            //Wenn Label ausgewählt ist, dann Button deaktivieren
            if (automaticReceiveAck.Checked) receiveACK.Enabled = false;
            else receiveACK.Enabled = true;
        }

        private void automaticExecuteAck_CheckedChanged(object sender, EventArgs e)
        {
            //Wenn Label ausgewählt ist, dann Button deaktivieren
            if (automaticExecuteAck.Checked) executeAck.Enabled = false;
            else executeAck.Enabled = true;
        }

        private void receiveACK_Click(object sender, EventArgs e)
        {
            ButtonPressed = true;
        }

        private void executeAck_Click(object sender, EventArgs e)
        {
            ButtonPressed = true;
        }

        private void streamStart_Click(object sender, EventArgs e)
        {
            Server.startStream((byte)typeSelector.Value, (byte)idSelector.Value);
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if ((Server != null) && Server.udp_active())
            {
                PacketListCnt.Text = Server.getPacketList().Count.ToString();

                if (ByteMessage != null)
                {
                    StatusTimer.Enabled = false;
                    SaveFileDialog FileDialog1 = new SaveFileDialog();

                    FileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    FileDialog1.FilterIndex = 2;
                    FileDialog1.RestoreDirectory = true;

                    if (FileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(FileDialog1.FileName, ByteMessage);
                    }
                    ByteMessage = null;
                    StatusTimer.Enabled = true;
                }
            }
        }

        private void sendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog FileDialog1 = new OpenFileDialog();

            FileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            FileDialog1.FilterIndex = 2;
            FileDialog1.RestoreDirectory = true;

            if (FileDialog1.ShowDialog() == DialogResult.OK)
            {
                byte[] array = File.ReadAllBytes(FileDialog1.FileName);
                Server.write(array, (byte)typeSelector.Value, (byte)idSelector.Value);
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Server.stopStream((byte)typeSelector.Value, (byte)idSelector.Value);
        }

        private void filter_CheckedChanged(object sender, EventArgs e)
        {
            if (Server != null)
            {
                Server.adapter_filter(filter.Checked);
            }
        }

    }

}
