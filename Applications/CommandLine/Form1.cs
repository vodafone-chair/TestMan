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

//Für Commandline
using System.Diagnostics;

//Für Spracheinstellungen
using System.Globalization;

//Für IP-Adresse
using System.Net;

//System-Informationen
using System.Management;

//UDP-Server
using UDP_Communications;
using HelperFunctions;

namespace CTRLGUI
{
    public partial class CtrlGUI : Form
    {              
        //CPU-Last etc.
        private System.Diagnostics.PerformanceCounter m_memoryCounter;
        private System.Diagnostics.PerformanceCounter m_CPUCounter;

        //Beinhaltet alle Statusmitteilungen
        List<string> status_msg = new List<string>();

        //UDP-Client
        string serverName;
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
                        string timestamp = "";
                        string befehl = null;
                        //Befehl erhalten
                        switch (e.Inhalt)
                        {

                            case "name":

                                if (e.Paket.Timestamp != null) Server.send_answer(true, "5", timestamp); //500ms Wartezeit
                                Server.SendData("name", serverName);
                                if (e.Paket.Timestamp != null) Server.send_answer(false, null, timestamp);
                            break;

                            case "shutdown":
                                if (e.Paket.Timestamp != null) Server.send_answer(true, "5", timestamp); //500ms Wartezeit
                                Shutdown("1");
                                if (e.Paket.Timestamp != null) Server.send_answer(false, null, timestamp);
                            break;

                            case "cmd":
                                if (e.Paket.Timestamp != null) Server.send_answer(true, "6000", timestamp); //600s Wartezeit

                                if (e.Paket.Content.TryGetValue("value", out befehl)) Server.SendData("response", execute_cmd(befehl));

                                if (e.Paket.Timestamp != null) Server.send_answer(false, null, timestamp);
                            break;

                            case "restart":
                            case "reboot":
                            if (e.Paket.Timestamp != null) Server.send_answer(true, "5", timestamp); //500ms Wartezeit
                            Shutdown("6");
                            if (e.Paket.Timestamp != null) Server.send_answer(false, null, timestamp);
                            break;

                            default:
                                display("Unknown Command received!");
                                Server.SendData("error", "Unknown Command received!");
                            break;
                        }
                    }
                }
        }

        public CtrlGUI()
        {
            InitializeComponent();
            Application.DoEvents();
        }

        public float GetAvailableMemory()
        {
            return m_memoryCounter.NextValue();
        }

        public float GetCPULoad()
        {
            return m_CPUCounter.NextValue();
        }

        public string GetPublicIP()
        {
            try
            {
                String direction = "";
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    direction = stream.ReadToEnd();
                }

                //Search for the ip in the html
                int first = direction.IndexOf("Address: ") + 9;
                int last = direction.LastIndexOf("</body>");
                direction = direction.Substring(first, last - first);

                return direction;
            }
            catch
            {
                display("Couldn't get public IP");
                return null;
            }
        }

        private string GetLocalIP()
        {

            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }

            return localIP;
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

        public void show_msg(string msg)
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

        public void display(string msg)
        {

            string[] split = msg.Split(new Char[] { '\n', '\r'});

            foreach (string s in split)
            {

                if (s.Trim() != "")
                    show_msg(s);
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
 
        }

        private void Zeitgeber_Tick(object sender, EventArgs e)
        {
            Zeitgeber.Interval = (int)update_intervall.Value * 1000;
            getData(null);
        }

        private void connection_button_Click(object sender, EventArgs e)
        {

        }

        private string execute_cmd(string befehl) {
            string output = null;
            if (befehl != null)
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + befehl);
                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.UseShellExecute = false;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;

                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                output = proc.StandardOutput.ReadToEnd();
            }
            // Display the command output.
            display(output);
            return output;
        }

        private void GPSGUI_Load(object sender, EventArgs e)
        {
            display("Arguments:");
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                Console.WriteLine(arg);
            }

            m_memoryCounter = new System.Diagnostics.PerformanceCounter();
            m_memoryCounter.CategoryName = "Memory";
            m_memoryCounter.CounterName = "Available MBytes";

            m_CPUCounter = new System.Diagnostics.PerformanceCounter();
            m_CPUCounter.CategoryName = "Processor";
            m_CPUCounter.CounterName = "% Processor Time";
            m_CPUCounter.InstanceName = "_Total";

            Server = new UDP_Server(this.EmpfangeDaten);

            //Server.init_udp("ctrlconfig.xml");

            //getData("Server start");
        }

        private void getData(string msg)
        {
            //Daten versenden
            serverName = System.Windows.Forms.SystemInformation.ComputerName;
            Dictionary<string, string> Daten = new Dictionary<string, string>()
	                        {
	                            {"IP", GetPublicIP()},
                                {"LIP", GetLocalIP()},
                                {"Host", serverName},
	                            {"CPULoad", GetCPULoad().ToString()},
	                            {"Speicher", GetAvailableMemory().ToString()},
	                        };
            
            if (msg != null) Daten.Add("msg", msg);

            //to UDP
            Server.SendData(Daten);            
            display("Host-Name:");
            display(Daten["Host"]);
            display("Public IP Address:");
            display(Daten["IP"]);
            display("Local IP Address:");
            display(Daten["LIP"]);
            display("System: CPU-Load: " + Daten["CPULoad"] + "% Avail. Memory (MB): " + Daten["Speicher"]);

            host_name.Text = serverName;
            pip.Text = Daten["IP"];
            lip.Text = Daten["LIP"];
            
        }

        /// <summary>
        /// Beendet oder startet Windows neu
        /// </summary>
        /// <param name="flags"></param>
        void Shutdown(string flags)
        {
            getData("Stop Server: " + flags);

            ManagementBaseObject mboShutdown = null;
            ManagementClass mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            ManagementBaseObject mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system. Use "2" to reboot.
            mboShutdownParams["Flags"] = flags;
            mboShutdownParams["Reserved"] = "0";
            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                mboShutdown = manObj.InvokeMethod("Win32Shutdown", mboShutdownParams, null);
            }
        }

        private void exec_Button_Click(object sender, EventArgs e)
        {
            execute_cmd(commandTextBox.Text);
        }
    }
}
