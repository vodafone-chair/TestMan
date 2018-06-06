using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.IO.Ports;

using System.Threading;

using UDP_Communications;

namespace DrehtischGUI
{
    public partial class Form1 : Form
    {
        //Beinhaltet alle Statusmitteilungen
        List<string> status_msg = new List<string>();

        UDP_Server Server;

        //Listen die die COM-Ports beinhalten
        List<string> com_ports = new List<string>();

        //Statusmeldungen vom Drehtisch
        sbyte status = 0;
        //Rückgabewert vom Drehtisch
        int rueckgabe_wert;


        public Form1()
        {
            InitializeComponent();
                     
            //Damit Fenster gleich angezeigt wird
            Application.DoEvents();
            this.Show();
            Application.DoEvents();

            Server = new UDP_Server(this.EmpfangeDaten);
            init_program();
            //Server starten, Drehtischport öffnen und initialisieren
            start_stop_Click(null, null);

        }

        public void EmpfangeDaten(object sender, DatenEventArgs e)
        {

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

                }
                else
                {
                    //Wenn Funktion aus anderem Thread aufgegerufen wird:
                    MethodInvoker LabelUpdate = delegate
                    {
                    //Befehle dekodieren
                    switch (e.Inhalt)
                    {
                        case "move":

                            //Empfangsbestätigung, wenn angefordert
                            if (e.Paket.Timestamp != null) Server.send_answer(true, "450", e.Paket.Timestamp);

                            //Befehl ausführen
                            string angle = "";
                            if (e.Paket.Content.TryGetValue("angle", out angle)) bewege_drehtisch(Convert.ToInt32(angle));
                            else bewege_drehtisch(0);

                            //Ausführungsbestätigung
                            if (e.Paket.Timestamp != null) Server.send_answer(false, null, e.Paket.Timestamp);

                            break;
  
                        default:
                            display("Unknown Command! " + e.Inhalt);
                            break;
                    }
                };
                Invoke(LabelUpdate);

                }
            }

        }

        private void init_program()
        {
            //Liest alle verfügbaren Schnittstellen ein!
            com_ports.Clear();
            drehtisch_port_list.DataSource = null;
            drehtisch_port_list.SelectedIndex = -1;
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                com_ports.Add(port);
            }
            drehtisch_port_list.DataSource = com_ports;
        }

        //Sucht und öffnet Drehtisch
        public void  openDrehtisch() {

            int cnt = 0;

            //Übernimmt ausgewähltes Element aus der Liste
            //    cnt = drehtisch_port_list.SelectedIndex;

            // Get a list of serial port names.
            foreach (string port in com_ports)
            {
                try
                {

                    serialport.PortName = port;
                    display(port);
                    serialport.Open();
                    //Sende Initialisierungsbefehl und Text
                    status = send_tmcl_command(31, 0, 0);
                    if (status == 100)
                    {
                        display("Drehtisch found!");
                        drehtisch_port_list.SelectedIndex = cnt;
                        status = nullposition();
                        break;

                    }
                    else
                    {
                        display(status_code_to_string(status));
                        serialport.Close();
                    }

                }
                catch (Exception ex)
                {
                    display(ex.Message);
                }
                cnt++;
            }

            if (cnt >= com_ports.Count) display("Drehtisch not found!");

        }

        private void bewege_drehtisch(int angle)
        {
            if ((angle > 340) || (angle < 0))
            {
                error(null, "Can't move to that angle!");
                return;
            }

            //Berechne Motorposition
            int position = (12800 / 360) * angle;

            //Bewege Tisch zur angegebenen Position
            if (serialport.IsOpen == true)
            {
                display("Move to " + angle.ToString() + "°");

                this.Enabled = false;

                sbyte status;

                //rote LED wird eingeschaltet
                status = send_tmcl_command(14, 1, 0);

                //bewege zur Position
                if (angle == 0)
                {
                    status = nullposition(); //bei 0° kommt die Initalisierungsroutine zum Einsatz                    
                }
                else
                {
                    status = send_tmcl_command(4, 0, position);    //bei anderen Winkeln

                    //Warte, dass Position erreicht ist
                    rueckgabe_wert = 0;

                    //Warte bis Paket empfangen wurde, dass Drehtisch die Bewegung abgeschlossen hat
                    while (rueckgabe_wert == 0)
                    {
                        Thread.Sleep(100);
                        Application.DoEvents();
                        status = send_tmcl_command(6, 8, 0);
                    }
                }
                //rote LED wird ausgeschaltet, da Bewegung abgeschlossen, der
                //Drehtisch ist wieder bereit
                status = send_tmcl_command(14, 1, 1);

                this.Enabled = true;

                if (status == 100)
                {

                }
                else
                {
                    error(null, status_code_to_string(status));
                }
            }
            else
            {
                error(null, "Port is closed!");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //Funktionen die die Kommunikation mit dem Drehtisch sicherstellen
        //////////////////////////////////////////////////////////////////////////////////////////
        private sbyte send_tmcl_command(byte befehl, byte typ, int wert)
        {
            //Sendet einen Befehl an ein TMCL Modul
            //und gibt einen Statuscode bzw. einen 32-bit signed Wert zurück
            //
            //Statuscode:
            //Code   Meaning 
            //----------------------------------------------------------
            //100    Successfully executed, no error 
            //101    Command loaded into TMCL™ program EEPROM
            //1      Wrong checksum 
            //2      Invalid command 
            //3      Wrong type 
            //4      Invalid value 
            //5      Configuration EEPROM locked 
            //6      Command not available 
            //Wenn Zugriff auf Modul nicht möglich bzw. gestört:
            //7      Serieller Port nicht geöffnet
            //8      Schreiben nicht möglich
            //9      Zeitüberschreitung, beim Empfang des Antwortpaketes
            //<0     Checksumme der Antwort ist falsch, gelesener Statuscode des Moduls wird
            //       aber zurückgegeben
            //11     undefinierter Fehler
            int faktor = 1;
            byte motor = 0;     //Motor- oder Banknummer
            byte adresse = 1;   //Adresse des Moduls (im Busbetrieb)
            sbyte status = 0;

            //Zusendendes Paket
            byte[] paket = new Byte[9];
            //Antwortpaket
            byte[] antwort = new Byte[9];
            //Zählvariable und Checksumme um Korrektheit der Pakete zu überprüfen
            byte i, checksum = 0;

            //Ändern, wenn mehrere Motoren angesteuert werden sollen
            paket[0] = adresse;
            paket[3] = motor;

            //Siehe TMCL Datenblatt
            paket[1] = befehl;
            paket[2] = typ;


            //Teile Wert auf 4 Byte auf
            for (i = 0; i < 4; i++)
            {
                paket[7 - i] = (byte)(wert % 256);
                wert = wert / 256;
            }

            //Berechne Checksumme: 8 bit Addition über alle Bytes
            for (i = 0; i < 8; i++)
            {
                checksum += paket[i];
            }
            paket[8] = checksum;

            //show_paket(paket);

            try
            {
                //Sende Befehl
                serialport.Write(paket, 0, 9);
            }
            catch
            {
                return 8;
            }

            //Warte bis Paket empfangen wurde oder 1 Sekunde verstrichen ist
            DateTime StartZeit = DateTime.Now;
            TimeSpan TimeOut = new TimeSpan(0, 0, 0, 1, 0);
            TimeSpan GemesseneZeit = new TimeSpan(0, 0, 0, 0, 0);
            DateTime EndZeit = DateTime.Now;

            while (serialport.BytesToRead < 9)
            {
                EndZeit = DateTime.Now;
                GemesseneZeit = EndZeit - StartZeit;
                if (GemesseneZeit > TimeOut)
                {
                    return 9;
                }
            }

            try
            {
                //Lese Antwortpaket nach jedem Befehl
                serialport.Read(antwort, 0, 9);
            }
            catch
            {
                return 9;
            }

            //Prüfe Checksumme
            checksum = 0;
            for (i = 0; i < 8; i++)
            {
                checksum += antwort[i];
            }

            //Checksumme korrekt?
            if (checksum != antwort[8])
            {
                status = (sbyte)(-antwort[2]);
            }
            else
            {
                status = (sbyte)(antwort[2]);
            }

            //Füge Wert wieder zusammen
            rueckgabe_wert = 0;
            for (i = 7; i > 3; i--)
            {
                rueckgabe_wert = rueckgabe_wert + faktor * antwort[i];
                faktor = faktor * 256;
            }

            return status;

        }

        private sbyte nullposition()
        {
            if (serialport.IsOpen)
            {
                sbyte status = send_tmcl_command(131, 0, 0);
                //Suchen nach der Nullposition
                status = send_tmcl_command(129, 0, 0);
                rueckgabe_wert = 1;
                while (rueckgabe_wert != 0)
                {
                    Thread.Sleep(100);
                    Application.DoEvents();
                    status = send_tmcl_command(135, 0, 0);
                }
                return status;
            }
            else
            {
                return 7;
            }
        }

        private void show_paket(byte[] paket)
        {
            //Zeige (TMCL) Paket
            string result = "";
            foreach (byte element in paket)
            {
                result += element.ToString() + " ";
            }
            // Kosmetik
            result.TrimEnd(new char[] { ' ' });
            display(result);
        }
        private string status_code_to_string(sbyte code)
        {
            switch (code)
            {

                case 100:
                    return "Successfully executed, no error ";

                case 101:
                    return "Command loaded into TMCL™ program EEPROM";


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

        //////////////////////////////////////////////////////////////////////////////////////////
        //Sonstige
        //////////////////////////////////////////////////////////////////////////////////////////
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

        private void button2_Click(object sender, EventArgs e)
        {
            bewege_drehtisch(Convert.ToInt32(winkel.Value));
        }

        private void start_stop_Click(object sender, EventArgs e)
        {
            if (start_stop.Text != "Connect")
            {
                Server.stop_udp();
                serialport.Close();
                start_stop.Text = "Connect";
            }
            else
            {
                openDrehtisch();
                Server.init_udp("drehtischconfig.xml");
                if (Server.udp_active() == false) start_stop.Text = "Connect";
                else start_stop.Text = "Disconnect";
            }
        }

        private void winkel_ValueChanged(object sender, EventArgs e)
        {
            bewege_drehtisch(Convert.ToInt32(winkel.Value));
        }

    }
}
