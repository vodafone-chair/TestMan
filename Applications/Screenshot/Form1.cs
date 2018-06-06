using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

using UDP_Communications;

namespace Screenshot
{
    public partial class ScreenCapture : Form
    {
        int screenShotCounter = 0;
        //Kommunikationsklasse
        UDP_Server Server;

        public ScreenCapture()
        {
            InitializeComponent();

            folder.Text = AppDomain.CurrentDomain.BaseDirectory;

            Server = new UDP_Server(this.EmpfangeDaten);

            //Initialisiere Server auf gegebene Multicast Adresse
            //Erfolgreich oder nicht??                
            if (Server.init_udp("224.5.6.7", 50000, 1, 11, 1, true) == false) error(null, "Cannot initialize UDP!");
            else
            {
                //Ja!
                statusBox.print("UDP up with " + Server.getServerType().ToString() + ", " + Server.getServerID().ToString());
            }
        }

        private void CaptureScreen()
        {
            try
            {
                string datei = filename.Text;
                datei = datei.Replace(".", "/");
                datei = datei.Replace("TIMESTAMP", GetTimestamp(DateTime.Now));
                datei = datei.Replace("DATE", DateTime.Now.ToShortDateString());
                datei = datei.Replace(".", "_");
                datei = datei.Replace("TIME", DateTime.Now.ToShortTimeString());
                datei = datei.Replace(":", "_");
                datei = datei.Replace("NUMBER", screenShotCounter.ToString());
                datei = datei.Replace("/", ".");
                string file = Path.Combine(folder.Text, Path.GetFileName(datei));

                //Größe des Bildschirms
                Rectangle bounds = Screen.GetBounds(Point.Empty);

                //Erzeuge Screenshot als Graphic-Objekt und wandle in Bitmap-Objekt um (zur besseren Weiterverarbeitung png,gif,jpeg)
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }

                    //Stream png
                    bitmap.Save(file); //System.Drawing.Imaging.ImageFormat.Png
                }

                screenShotCounter++;

                statusBox.print("Screenshot #" + screenShotCounter.ToString() + " " + file + " created.");
            }
            catch (Exception ex)
            {
                error(ex, "Not able to send the Image!");
            }
        }

        private void CaptureAllScreen()
        {
            try
            {
                string datei = filename.Text;
                datei = datei.Replace(".", "/");
                datei = datei.Replace("TIMESTAMP", GetTimestamp(DateTime.Now));
                datei = datei.Replace("DATE", DateTime.Now.ToShortDateString());
                datei = datei.Replace(".", "_");
                datei = datei.Replace("TIME", DateTime.Now.ToShortTimeString());
                datei = datei.Replace(":", "_");
                datei = datei.Replace("NUMBER", screenShotCounter.ToString());
                datei = datei.Replace("/", ".");
                string file = Path.Combine(folder.Text, Path.GetFileName(datei));

                // Determine the size of the "virtual screen", which includes all monitors.
                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;

                //Erzeuge Screenshot als Graphic-Objekt und wandle in Bitmap-Objekt um (zur besseren Weiterverarbeitung png,gif,jpeg)
                using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
                    }

                    //Stream png
                    bitmap.Save(file); //System.Drawing.Imaging.ImageFormat.Png
                }

                screenShotCounter++;

                statusBox.print("Screenshot #" + screenShotCounter.ToString() + " " + file + " created.");
            }
            catch (Exception ex)
            {
                error(ex, "Not able to send the Image!");
            }
        }

        private String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
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
            statusBox.print(msg);
            DateTime currentDate = DateTime.Now;

            if (ex != null)
            {
                statusBox.print(ex.Message);
                Append("error.log", currentDate.ToString() + " " + msg + "\n\r" + ex.ToString() + "\n\r");
            }
            else
            {
                Append("error.log", currentDate.ToString() + " " + msg + "\n\r");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                folder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CaptureScreen();
        }

        //Empfängt Befehle von der UDP-Multicastgruppe
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
                        byte[] ByteMessage = Server.getStreamData(e.Sender_Typ, e.Sender_ID);
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
                    else if (e.Variablen_name == "hint") statusBox.print("Hint from UDP-Server: " + e.Inhalt);
                    else statusBox.print("Warning from UDP-Server: " + e.Inhalt);
                }
            }
            catch (Exception ex)
            {
                statusBox.print("Error @ receive packets: " + ex.Message + ":" + ex.ToString());
            }
        }

        private void evaluateStatusMessage(Packet Paket)
        {
            statusBox.print("Status: " + Paket.toString(";", true));
        }

        private void evaluateCommand(Packet Paket)
        {
            string timestamp = Paket.Timestamp;
            Server.send_answer(true, "5000", timestamp);

            string value_str = null;
            double value = double.NegativeInfinity;
            if (Paket.Content.TryGetValue("value", out value_str))
            {
                if (double.TryParse(value_str, out value))
                {

                }
                else
                {
                    //Umwandlung string to long fehlgeschlagen
                }
            }
            else
            {
                //Wert value nicht im Paket enthalten
            }

            string return_message = null;

            //Befehl ausführen
            switch (Paket.Command)
            {
                case "screenshot":

                    CaptureScreen();

                    break;
                case "screenshot_all":

                    CaptureAllScreen();

                    break;

                default:
                    statusBox.print("Unknown command: " + Paket.toString(";", true));
                    break;
            }

            Server.send_answer(false, null, timestamp, return_message);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TakeSnapshot.Interval = (int)seconds.Value;
            if (TakeSnapshot.Enabled) TakeSnapshot.Enabled = false;
            else TakeSnapshot.Enabled = true;
        }

        private void TakeSnapshot_Tick(object sender, EventArgs e)
        {
            CaptureAllScreen();
        }
    }
}
