using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UDP_Communications;

using System.IO.Ports;
using System.IO;

using System.Threading;

namespace XYGUI
{
    public partial class Form1 : Form
    {
        UDP_Server Server;

        Boolean debug_mode = true;

        //Beinhaltet alle Statusmitteilungen
        List<string> status_msg = new List<string>();

        //Listen die die COM-Ports beinhalten
        List<string> com_ports = new List<string>();

        //Größe des XY-Tisches
        int max_x = 390;
        int max_y = 450;
        int command_index = 256;
        int antwort = 0;

        public Form1()
        {
            InitializeComponent();
            //Verknüpfe unabhängige Serverklasse mit GUI, durch Events, die erzeugt werden, wenn Daten empfangen wurden
            Server = new UDP_Server(this.EmpfangeDaten);
            Server.init_udp("xyconfig.xml");
        }
        /// <summary>
        /// Sobald der UDP-Server Daten empfängt, wird über Events diese Methode aufgerufen
        /// </summary>
        /// <param name="sender">Objekt der UDP-Server-Klasse, dass die Daten empfangen hat</param>
        /// <param name="e">Übermittelte Informationen</param>
        public void EmpfangeDaten(object sender, DatenEventArgs e)
        {
            Application.DoEvents();
            if ((e.Variablen_name == null) && (e.Paket == null)) return;

            //Wenn Funktion aus anderem Thread aufgegerufen wird:

                if (e.Paket == null)
                {
                    //Mitteilung vom Server erhalten, z.B. Fehler, Statusbericht                    
                    if (e.Variablen_name.Equals("error", StringComparison.InvariantCultureIgnoreCase) == true)
                        error(null, "UDP-Server-Error: " + e.Inhalt);
                    else if (e.Variablen_name.Equals("hint", StringComparison.InvariantCultureIgnoreCase) == true)
                        display("Hint from UDP-Server: " + e.Inhalt);
                }
                else
                {


                    //Paket empfangen
                    //Unterscheidung ob Befehl oder normales Paket
                    if (e.Inhalt == null)
                    {
                        //Normales Paket
                    }
                    else
                    {
                        //Befehl erhalten
                        switch (e.Inhalt)
                        {
                            case "move":
                                //Empfangsbestätigung, wenn angefordert
                                if (e.Paket.Timestamp != null) Server.send_answer(true, "100", e.Paket.Timestamp);

                                //Befehl ausführen
                                string x = "";
                                string y = "";
                                sbyte status = 0;
                                if (e.Paket.Content.TryGetValue("x", out x) && e.Paket.Content.TryGetValue("y", out y)) status = xy_tisch_goto(Convert.ToInt32(x), Convert.ToInt32(y));
                                else status = xy_tisch_init();

                                Thread.Sleep(500);
                                Application.DoEvents();

                                display(status_code_to_string(status));

                                //Ausführungsbestätigung
                                if ((e.Paket.Timestamp != null) && ((status == 100) || (status == 10) || (status == 6))) Server.send_answer(false, null, e.Paket.Timestamp);
                                break;
                            default:
                                display("Unknown Command received!");
                                break;
                        }
                    }
                }

            //};
            //Invoke(LabelUpdate);
        }

        private void find_xy()
        {

            if (xytisch.IsOpen == false)
            {
                int cnt = 0;
                sbyte status = 0;
                // Get a list of serial port names.
                foreach (string port in com_ports)
                {
                    try
                    {
                        if (xytisch.IsOpen == true) xytisch.Close();
                            xytisch.PortName = port;
                            display(port);
                            xytisch.Open();
                            //Sende Testbefehl
                            status = xy_tisch_send_command(12, 0, 0, 0, 0);
                            if (status == 100)
                            {
                                display(status_code_to_string(xy_tisch_init()));
                                display("XY-Tisch gefunden");
                                serialportlist.SelectedIndex = cnt;
                                break;
                            }
                            else
                            {
                                display(status_code_to_string(status));
                            }
                            cnt++;                            
                    }                 
                    catch (Exception ex)
                    {
                        display(ex.Message);
                    }
                }
                if (cnt >= com_ports.Count)
                {
                    display("XY-Tisch nicht gefunden");
                }
            }
        }

        private sbyte xy_tisch_send_command(Byte command, Byte value_1, Byte value_2, Byte value_3, Byte value_4)
        {
            //Sendet einen Befehl an den XY-Tisch

            Byte[] packet = new Byte[5] { command, value_1, value_2, value_3, value_4 };
            Byte[] read_packet = new Byte[4] { 0,0,0,0};

            if (xytisch.IsOpen == true)
            {
                //alles gelesene verwerfen
                xytisch.DiscardInBuffer();
                //Paket an Tisch senden
                xytisch.Write(packet, 0, 5);
                //Anzeigen
                if (debug_mode) status_msg.Add("Send: " + paket_to_string(packet));                
                //Bestätigung abwarten
                int timeout_counter = 0;

                while (true)
                {
                    if (command == 0) return 100;
                    //Eigentlich nur für Debug-zwecke, alles null setzen
                    read_packet = new Byte[4] { 0, 0, 0, 0};
                    try
                    {
                        //Lesen des Pakets
                        xytisch.Read(read_packet, 0, 4);
                    }
                    catch
                    {
                        //Höchstwahrscheinlich Timeout!
                        timeout_counter++;
                        status_label.Text = timeout_counter.ToString();
                        Application.DoEvents();
                        //eventuelle Nachrichten anzeigen
                        display();
                        if (timeout_counter > 100) return 9;//über 10 sek gewartet
                        continue;                        
                    }

                    //if ((read_packet[0] == 0) && (read_packet[1] == 0) && (read_packet[2] == 0) && (read_packet[3] == 0)) { display(); continue; }
                   
                    //Anzeigen des empfangenen Befehls
                    if (debug_mode) status_msg.Add(paket_to_string(read_packet));

                    command_index = read_packet[1];

                    //Dekodierung des Paketes
                    switch(read_packet[0]) {
                        case 1:
                            //Befehl bestätigt -> weiterhin warten
                            if (debug_mode) status_msg.Add("ack: " + command_index.ToString());
                            //Um das suchen nach dem Tishc abzukürzen...
                            if (command == 12) return 100;
                            break;
                        case 2:
                            //Befehlspuffer voll
                            return 10;
                        case 3:
                            //Ausführung des Befehls startet
                            if (debug_mode) status_msg.Add("start: " + command_index.ToString());
                            break;
                        case 4:
                            //Antwort erhalten
                            antwort = read_packet[2] << 8 + read_packet[3];
                            if (debug_mode) status_msg.Add("answer for " + command_index.ToString() + ": " + antwort.ToString());
                            break;
                        case 5:
                            //Befehl ausgeführt
                            if (debug_mode) status_msg.Add("finished: " + command_index.ToString());

                            switch (read_packet[2])
                            {
                                case 0:
                                    return 100;
                                case 1:
                                    return 6;
                                case 2:
                                    display(status_code_to_string(11));
                                    return 100;
                            }
                            
                            break;

                    }
                }                
            }
            else
            {
                return 7;
            }
        }

        private string paket_to_string(Byte[] packet)
        {
            string str = "";
            foreach (byte b in packet)
            {
                str = str + " " + b.ToString();
            }
            return str;
        }

        private sbyte xy_measure_table()
        {
            //Misst die maximale Größe des XY-Tisches
            //funktioniert nicht!
            Byte[] packet = new byte[4];

            sbyte status = xy_tisch_send_command(10, 0, 0, 0, 0);

            //x highbyte auslesen
            max_x = packet[2] << 8;
            //x lowbyte auslesen
            max_x |= packet[3];
            max_x = Convert.ToInt32(Convert.ToDouble(max_x) * Math.PI * 9.56 / 400);

            //y highbyte auslesen
            max_y = packet[2] << 8;
            //y lowbyte auslesen
            max_y |= packet[3];
            max_y = Convert.ToInt32(Convert.ToDouble(max_y) * Math.PI * 19.1 / 400);

            return status;
        }

        private sbyte xy_tisch_goto(int x, int y)
        {
            if (xytisch.IsOpen == true)
            {
                if ((x <= 390) && (y <= 450))
                {
                    //X, Y auf 16 bit erweitern sozusagen so hoch auflösen wie möglich
                    //und in integer umwandeln damit der uC damit rechnen kann
                    x = x * 64;
                    y = y * 64;
                    return xy_tisch_send_command(1, Convert.ToByte(x >> 8), Convert.ToByte(x & 255), Convert.ToByte(y >> 8), Convert.ToByte(y & 255));                     
                }
                else
                {
                    return 4;
                }
            }
            else
            {
                return 7;
            }
        }

        private SByte xy_tisch_init()
        {
            //Nullposition einstellen
            return xy_tisch_send_command(11, 0, 0, 0, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            display(status_code_to_string(xy_tisch_goto(Convert.ToInt32(x_pos.Value),Convert.ToInt32(y_pos.Value))));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Zum Anzeigen, Loggen von Statusmitteilung
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Append(string sFilename, string sLines)
        {
            ///<summary>
            /// Fügt den übergebenen Text an das Ende einer Textdatei an.
            ///</summary>
            ///<param name="sFilename">Pfad zur Datei</param>
            ///<param name="sLines">anzufügender Text</param>
            ///
            StreamWriter myFile = new StreamWriter(sFilename, true);
            myFile.WriteLine(sLines);
            myFile.Close();
        }

        public void error(Exception ex, string msg)
        {
            //Zeigt Fehlermeldungen an und speichert sie im Logfile
            display(msg);
            DateTime currentDate = DateTime.Now;

            if (ex != null)
            {
                display(ex.Message);
                Append("error.log", currentDate.ToString() + " " + msg + "\n\r" + ex.ToString() + "\n\r");
            }
            else
            {
                Append("error.log", currentDate.ToString() + " " + msg + "\n\r");
            }
        }

        public void display(string msg)
        {
            //Zeigt Nachricht in der Statusliste an
            try
            {
                if (status_msg.Count > 1000) status_msg.Clear();
                status_msg.Add(msg);
                try
                {
                    //Funktion wird aus gleichem Thread aufgerufen
                    statusbox.DataSource = null;
                    statusbox.DataSource = status_msg;
                    statusbox.ClearSelected();
                    statusbox.SetSelected(status_msg.Count - 1, true);
                }
                catch
                {
                    //Wenn Funktion aus anderem Thread aufgegerufen wird:
                    MethodInvoker LabelUpdate = delegate
                    {
                        try
                        {
                            statusbox.DataSource = null;
                            statusbox.DataSource = status_msg;
                            statusbox.ClearSelected();
                            statusbox.SetSelected(status_msg.Count - 1, true);
                        }
                        catch (Exception err)
                        {
                            //Mitteilung kann nicht angezeigt werden -> speichern in error-Logfile
                            //MessageBox.Show("Kann Statusliste nicht updaten! Siehe error.log!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            DateTime currentDate = DateTime.Now;
                            Append("error.log", currentDate.ToString() + " Kann Statusliste nicht mit der Nachicht '" + msg + "' updaten! \n\r" + err.ToString() + "\n\r");
                        }
                    };
                    Invoke(LabelUpdate);
                }
            }
            catch (Exception err)
            {
                //Mitteilung kann nicht angezeigt werden -> speichern in error-Logfile
                //MessageBox.Show("Kann Statusliste nicht updaten! Siehe error.log!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                DateTime currentDate = DateTime.Now;
                Append("error.log", currentDate.ToString() + " Kann Statusliste nicht mit der Nachicht '" + msg + "' updaten! \n\r" + err.ToString() + "\n\r");
            }
        }

        private string dictionary_to_string(Dictionary<string, string> dic)
        {
            string str = "";
            // Use var keyword to enumerate dictionary
            foreach (var pair in dic)
            {
                str = str + "|" + pair.Key + ":" + pair.Value;
            }

            return str;
        }

        public void display()
        {
            try
            {
                //Funktion wird aus gleichem Thread aufgerufen
                statusbox.DataSource = null;
                statusbox.DataSource = status_msg;
                statusbox.ClearSelected();
                statusbox.SetSelected(status_msg.Count - 1, true);
            }
            catch
            {
                //Wenn Funktion aus anderem Thread aufgegerufen wird:
                MethodInvoker LabelUpdate = delegate
                {
                    try
                    {
                        statusbox.DataSource = null;
                        statusbox.DataSource = status_msg;
                        statusbox.ClearSelected();
                        statusbox.SetSelected(status_msg.Count - 1, true);
                    }
                    catch (Exception err)
                    {
                        //Mitteilung kann nicht angezeigt werden -> speichern in error-Logfile
                        DateTime currentDate = DateTime.Now;
                        Append("error.log", currentDate.ToString() + " Kann Statusliste nicht  updaten! \n\r" + err.ToString() + "\n\r");
                    }
                };
                Invoke(LabelUpdate);
            }
        }

        private void init_program()
        {
            //Liest alle verfügbaren Schnittstellen ein!
            com_ports.Clear();
            serialportlist.DataSource = null;
            serialportlist.SelectedIndex = -1;
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                com_ports.Add(port);
            }
            serialportlist.DataSource = com_ports;
        }

        private string status_code_to_string(sbyte code)
        {
            switch (code)
            {

                case 100:
                    return "Successfully executed, no error ";

                case 101:
                    return "Command loaded into TMCL™ program EEPROM";

                case 10:
                    return "Buffer full. Command not accepted.";

                case 11:
                    return "Warning: Emergency stop"; 

                case 1:
                    return "Wrong checksum ";

                case 2:
                    return "Invalid command ";

                case 3:
                    return "Wrong type ";

                case 4:
                    return "Invalid value ";

                case 5:
                    return "Configuration EEPROM locked ";
                case 6:
                    return "Command not available ";
                //Wenn Zugriff auf Modul nicht möglich bzw. gestört:
                case 7:
                    return "Serieller Port nicht geöffnet";

                case 8:
                    return "Schreiben nicht möglich";

                case 9:
                    return "Zeitüberschreitung, beim Empfang des Antwortpaketes";

                default:
                    if (code < 0)
                    {
                        return "Checksumme der Antwort ist falsch, gelesener Statuscode des Moduls wird aber zurückgegeben! " + status_code_to_string(Math.Abs(code));
                    }
                    else
                    {
                        return "Undefinierter Fehler";
                    }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.DoEvents();
            this.Show();
            Connect_Click(null, null);
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            if (Connect.Text == "Connect")
            {
                init_program();
                find_xy();
                Connect.Text = "Disconnect";
            }
            else
            {
                xy_tisch_send_command(0, 0, 0, 0, 0);
                xytisch.Close();
                Connect.Text = "Connect";
            }
        }       
    }
}
