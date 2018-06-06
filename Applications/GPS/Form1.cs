using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Threading;

//Für Spracheinstellungen
using System.Globalization;

//UDP-Server
using UDP_Communications;

namespace GPSGUI
{
    public partial class GPSGUI : Form
    {
        //Beinhaltet alle Statusmitteilungen
        List<string> status_msg = new List<string>();

        //Listen die die COM-Ports beinhalten
        List<string> com_ports = new List<string>();

        // Local variables used to hold the present
        // position as latitude and longitude
        public double Latitude;
        public double Longitude;
        public double lastLatitude;
        public double lastLongitude;
        public string GpsTime;
        public string GpsHeight;
        Boolean gps_available = false;

        //UDP-Client
        UDP_Server Server;

        /// <summary>
        /// Sobald der UDP-Server Daten empfängt, wird über Events diese Methode aufgerufen
        /// </summary>
        /// <param name="sender">Objekt der UDP-Server-Klasse, dass die Daten empfangen hat</param>
        /// <param name="e">Übermittelte Informationen</param>
        public void EmpfangeDaten(object sender, DatenEventArgs e)
        {
            Application.DoEvents();
            if ((e.Variablen_name == null) && (e.Paket == null)) return;

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
                            case "store":
                                
                                if (e.Paket.Timestamp != null) Server.send_answer(true, "500", e.Paket.Timestamp); //500ms Wartezeit

                                lastLatitude = Latitude;
                                lastLongitude = Longitude;

                                if (e.Paket.Timestamp != null) Server.send_answer(false, null, e.Paket.Timestamp);
                                break;

                            default:
                                display("Unknown Command received!");
                                break;
                        }
                    }
                }
        }


        public GPSGUI()
        {
            InitializeComponent();

            Application.DoEvents();

            Server = new UDP_Server(this.EmpfangeDaten);

            Server.init_udp("gpsconfig.xml");

            init_program();
            connect_to_gps();

        }

        private void connect_to_gps()
        {
            //Finde serielle Schnittstelle an der Fernbedienung angeschlossen ist!
            int a = 0;

            // Get a list of serial port names.
            //Gehe alle verfügbaren Schnittstellen durch
            foreach (string port in com_ports)
            {
                try
                {

                    //setze auf aktuellen Port
                    gps.PortName = port;
                    //Hinweis in der GUI
                    display(port);
                    //Öffne aktuellen Port
                    gps.Open();

                    //Zeichne empfangene Daten für eine gewisse Zeit auf
                    Thread.Sleep(500);
                    Application.DoEvents();
                    string data = gps.ReadExisting();

                    //Port wieder schließen
                    gps.Close();

                    //Wenn bestimmter String vorhanden ist, dann GPS-Gerät gefunden
                    if ((data.IndexOf("GPGGA") != -1) || (data.IndexOf("SDDBT") != -1) || (data.IndexOf("GPDTM") != -1) || (data.IndexOf("GPRMC") != -1))
                    {                        
                        display("GPS-Empfänger gefunden");
                        gps_port_list.SelectedIndex = a;
                        //Öffne GPS-Schnittstelle nach dem richtiger Port gefunden wurde
                        open_gps();
                        break;
                    }

                    //nächsten Port probieren

                }
                catch (Exception ex)
                {
                    //Zeige Fehler an der beim aktuellen Verbindungsversuch aufgetreten ist
                    display(ex.Message);
                }
                a++;
            }

            //Hinweis das kein GPS Modul gefunden wurde
            if (a >= com_ports.Count)
            {
                error(null, "GPS-Modul nicht gefunden");
            }

        }

        private void open_gps()
        {
            //Öffnen des Ports je nachdem was in der Liste eingestellt wird!

            //GPS-Maus
            if (gps.IsOpen == false)
            {
                // Try to open the serial port
                try
                {
                    //
                    int selectedIndex = gps_port_list.SelectedIndex;
                    gps.PortName = com_ports[selectedIndex];
                    gps.Open();

                    //Aktiviere Zeitgeber um GPS Informationen regelmäßig auszulesen.
                    Zeitgeber.Enabled = true;
                    display("Kommunikation mit GPS-Empfänger aufgebaut");
                }
                catch (Exception ex)
                {
                    error(ex, "Kommunikation mit GPS-Empfänger nicht aufgebaut!");
                    Zeitgeber.Enabled = false;
                }
            }
        }

        private void disconnect_from_gps()
        {
            //Schließen des GPS-Ports
            try
            {
                gps.Close();
                display("Kommunikation mit GPS-Modul abgebaut");
            }
            catch (Exception ex)
            {
                error(ex, "Beim Schließen der Geräte ist ein Fehler aufgetreten!");
            }

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
                if (status_msg.Count > 60000) status_msg.Clear();
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Beim Schließen der Anwendung (muss eventuell noch ein Verweis in der Load Funktion eingetragen werden)
            //Port wird auch ohne Aufruf dieser Methode beim Schließen des Programms geschlossen
            //eventuell weitere Funktionen...
            disconnect_from_gps();
        }

        private void init_program()
        {
            //Liest alle verfügbaren Schnittstellen ein!
            com_ports.Clear();
            gps_port_list.DataSource = null;
            gps_port_list.SelectedIndex = -1;
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                com_ports.Add(port);

            }

            gps_port_list.DataSource = com_ports;
        }

        ///////////////////////////////////////////////////////////////////////////////
        //GPS-Funktionen von StackOverflow
        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            //code for Distance in Kilo Meter
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Abs(Math.Round(rad2deg(Math.Acos(dist)) * 60 * 1.1515 * 1.609344 * 1000, 0));
            return (dist);
        }

        private double GetDirection(double lat1, double lon1, double lat2, double lon2)
        {
            //code for Direction in Degrees
            double dlat = deg2rad(lat1) - deg2rad(lat2);
            double dlon = deg2rad(lon1) - deg2rad(lon2);
            double y = Math.Sin(dlon) * Math.Cos(dlat);
            double x = Math.Cos(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) - Math.Sin(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(dlon);
            double direct = Math.Round(rad2deg(Math.Atan2(y, x)), 0);
            if (direct < 0)
                direct = direct + 360;
            return (direct);
        }

        private double GetSpeed(double lat1, double lon1, double lat2, double lon2, DateTime CurTime, DateTime PrevTime)
        {
            //code for speed in Kilo Meter/Hour
            TimeSpan TimeDifference = CurTime.Subtract(PrevTime);
            double TimeDifferenceInSeconds = Math.Round(TimeDifference.TotalSeconds, 0);
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = rad2deg(Math.Acos(dist)) * 60 * 1.1515 * 1.609344;
            double Speed = Math.Abs(Math.Round((dist / Math.Abs(TimeDifferenceInSeconds)) * 60 * 60, 0));
            return (Speed);
        }

        private void Zeitgeber_Tick(object sender, EventArgs e)
        {
            Dictionary<string, string> keinGPS = new Dictionary<string, string>()
	                        {
	                            {"Latitude", "-"},
	                            {"Longitude", "-"},
	                            {"Height", "-"},
                                {"Distance", "-1"}
	                        };


            //Erst nach 5 unlesbaren Paketen, wird GPS Position als nicht verfügbar gekennzeichnet
            byte failure_counter = 0;
            if (gps.IsOpen == true)
            {

                //Button verändern
                connection_button.Text = "Disconnect";

                string data = gps.ReadExisting();
                string[] strArr = data.Split('$');
                for (int i = 0; i < strArr.Length; i++)
                {
                    string strTemp = strArr[i];
                    string[] lineArr = strTemp.Split(',');
                    if ((lineArr[0] == "GPGGA"))
                    {
                        try
                        {

                            // Display string representations of numbers for en-us culture
                            CultureInfo ci = new CultureInfo("en-us");

                            //Latitude
                            double XX = Convert.ToDouble(lineArr[2].Substring(0, 2));     //str2num(c_array{5}(1:3));
                            double YY = Convert.ToDouble(lineArr[2].Substring(2, 2));            //c_array{5}(4:5);
                            double ZZZZ = Convert.ToDouble(lineArr[2].Substring(5, 4));          //c_array{5}(7:end);
                            //Umrechnung nötig --> siehe Wikipedia
                            Latitude = Math.Round(XX + (YY + ZZZZ / 10000) / 60, 6);
                            //Latitude = lat.ToString("F06", ci);

                            //Longitude
                            XX = Convert.ToDouble(lineArr[4].Substring(0, 3));     //str2num(c_array{5}(1:3));
                            YY = Convert.ToDouble(lineArr[4].Substring(3, 2));            //c_array{5}(4:5);
                            ZZZZ = Convert.ToDouble(lineArr[4].Substring(6, 4));          //c_array{5}(7:end);
                            //Umrechnung nötig --> siehe Wikipedia
                            Longitude = Math.Round(XX + (YY + ZZZZ / 10000) / 60, 6);
                            //Longitude = lon.ToString("F06", ci);

                            //Height
                            GpsHeight = lineArr[9];
                            gps_available = true;

                            //Anzeige der Entfernung vom letzten Messpunkt (bzw. ersten überhaupt empfangenen GPS-Punkt)
                            if (lastLatitude == 0) lastLatitude = Latitude;
                            if (lastLongitude == 0) lastLongitude = Longitude;

                            double s = GetDistance(lastLatitude, lastLongitude, Latitude, Longitude);

                            //Für die Anzeige optimieren
                            if (s >= 1000) abstand_letzte_messung.Text = (s/1000).ToString() + "k";
                            else abstand_letzte_messung.Text = s.ToString();

                            //Neu zeichnen der Karte
                            this.Refresh();

                            //Daten versenden
                            // Example Dictionary again
                            Dictionary<string, string> GPSDaten = new Dictionary<string, string>()
	                        {
	                            {"Latitude", Latitude.ToString()},
	                            {"Longitude", Longitude.ToString()},
	                            {"Height", GpsHeight.ToString()},
                                {"Distance", s.ToString()}
	                        };

                            Server.SendData(GPSDaten);
                            latitude_label.Text = Latitude.ToString();
                            longitude_label.Text = Longitude.ToString();
                            height_label.Text = GpsHeight.ToString();

                        }
                        catch
                        {
                            //Cannot Read GPS values
                            failure_counter++;
                            if (failure_counter > 5)
                            {
                                failure_counter = 0;
                                Latitude = double.NaN;
                                Longitude = double.NaN;
                                GpsHeight = "GPS Unavailable";
                                gps_available = false;
                                abstand_letzte_messung.Text = "---";
                                Server.SendData(keinGPS);
                                latitude_label.Text = "---";
                                longitude_label.Text = "---";
                                height_label.Text = "---";
                            }
                        }

                        try
                        {
                            //Zeit - Unabhängig von den Positionsdaten, da die GPS-Zeit auch bei nur einem Satelliten zur Vefügung steht
                            GpsTime = lineArr[1].Substring(0, 2) + ":" + lineArr[1].Substring(2, 2) + ":" + lineArr[1].Substring(4, 2);
                            Server.SendData("Time", GpsTime);
                        }
                        catch
                        {
                            GpsTime = "GPS Unavailable";
                            gps_available = false;
                            Server.SendData(keinGPS);
                        }

                        //Zeige Uhrzeit im Statuslabel an
                        time_label.Text = GpsTime;

                    }
                }
            }
            else
            {
                Latitude = double.NaN;
                Longitude = double.NaN;
                GpsHeight = "COM Port Closed";
                Zeitgeber.Enabled = false;
                abstand_letzte_messung.Text = "---";
                //Button verändern
                connection_button.Text = "Connect";
                Server.SendData("status", "GPS deactivated");
            }
        }

        private void connection_button_Click(object sender, EventArgs e)
        {
            if (connection_button.Text == "Disconnect") {
                gps.Close();
            } else {
                open_gps();
            }
        }

        private void GPSGUI_Load(object sender, EventArgs e)
        {

        }
    }
}
