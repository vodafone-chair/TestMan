using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HelperFunctions
{
    public class helperFunctions
    {
        //Beinhaltet alle Statusmitteilungen
        List<string> status_msg = new List<string>();

        //Einstellungen für das Programm
        public Dictionary<string, Control> options = new Dictionary<string, Control>();

        Form WindowsFenster = null;
        //ListBox statusbox = null;
        //Label label = null;
        //ToolStripStatusLabel ToolStripLabel = null;

        //Wartefenster
        System.Threading.Timer AnimationTimer;
        Form waitWindow = null;

        //////////////////////////////////////////////////////////////////////////////////////////
        //Konstruktor
        //////////////////////////////////////////////////////////////////////////////////////////

        public helperFunctions(Form aktuelleForm)
        {
            WindowsFenster = aktuelleForm;
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //Read in Options from a XML to enable configure the program not only via xml
        //////////////////////////////////////////////////////////////////////////////////////////
        public void readOptions(string filename, Panel panel, int start_x, int start_y, int rand = 20, int height = 15)
        {

            Dictionary<string, string[]> settings = new Dictionary<string, string[]>();

            if (filename == null)
            {
                error(null, "Wrong config file!");
            }
            else
            {
                settings = readConfigFile(filename);

                //Alle Optionen einlesen
                options = new Dictionary<string, Control>();

                //Anzahl Steuerlemente/Optionen
                int zaehler = 0;

                //Position der Steuerelemente                       
                int y_pos = start_y;
                int x_pos = start_x;

                int width = panel.Width - 2 * rand;

                foreach (var pair in settings)
                {

                    if (zaehler != 0) y_pos += 2 * height;

                    string key = pair.Key.ToLower();

                    //Inhalt der Steuerelemente
                    string value = pair.Value[0];
                    value = value.Replace(" ", "");

                    long zahl = 0;
                    zaehler++;

                    //name der Variable
                    Label variablenName = new Label();
                    variablenName.Location = new Point(x_pos, y_pos);
                    //variablenName.Size = new Size(width, height);
                    variablenName.Text = pair.Key;
                    panel.Controls.Add(variablenName);
                    variablenName.AutoSize = true;
                    //variablenName.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);                    

                    y_pos = variablenName.Location.Y + variablenName.Height;

                    //Bedienelement erstellen                                                
                    if ((value.ToLower() == "true") || (value.ToLower() == "false"))
                    {
                        CheckBox element = new CheckBox();
                        element.Location = new Point(x_pos, y_pos);
                        element.Size = new Size(width, height);
                        //element.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                        if (pair.Value.Length > 1) element.Tag = pair.Value[1];
                        else element.Tag = null;

                        //Inhakt des Elementes bestimmen
                        if (value.ToLower() == "true") element.Checked = true;
                        else element.Checked = false;

                        //Hinzufügen zur Form und zu einem Dictionary
                        options.Add(key, element);
                        panel.Controls.Add(element);
                    }
                    else if (Int64.TryParse(value, out zahl))
                    {
                        NumericUpDown element = new NumericUpDown();
                        element.Location = new Point(x_pos, y_pos);
                        element.Size = new Size(width, height);
                        //element.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                        if (pair.Value.Length > 1) element.Tag = pair.Value[1];
                        else element.Tag = null;

                        element.Maximum = Int64.MaxValue;
                        element.Minimum = Int64.MinValue;

                        element.Value = zahl;

                        //Hinzufügen zur Form und zu einem Dictionary
                        options.Add(key, element);
                        panel.Controls.Add(element);
                    }
                    else
                    {
                        TextBox element = new TextBox();
                        element.Location = new Point(x_pos, y_pos);
                        element.Size = new Size(width, height);
                        //element.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                        if (pair.Value.Length > 1) element.Tag = pair.Value[1];
                        else element.Tag = null;

                        element.Text = pair.Value[0];

                        //Hinzufügen zur Form und zu einem Dictionary
                        options.Add(key, element);
                        panel.Controls.Add(element);
                    }
                }
            }
        }
        //To Access the options
        public object getValue(string name)
        {
            var objekt = options[name.ToLower()];
            if (objekt is TextBox) return (String)objekt.Text;
            if (objekt is CheckBox) return (Boolean)((CheckBox)objekt).Checked;
            if (objekt is NumericUpDown) return (long)((NumericUpDown)objekt).Value;
            return null;
        }
        public string getTag(string name)
        {
            var objekt = options[name.ToLower()];
            return (string)objekt.Tag;
            //if (objekt is TextBox) return (string)objekt.Tag;
            //if (objekt is CheckBox) return (string)((CheckBox)objekt).Tag;
            //if (objekt is NumericUpDown) return (string)((NumericUpDown)objekt).Tag;
            //return null;
        }
        public void setValue(string name, object value)
        {           
            var objekt = options[name.ToLower()];
            if (objekt.InvokeRequired)
            {
                objekt.Invoke(new Action(() => this.setValue(name, value)));
            }
            else
            {
                if (objekt is TextBox) objekt.Text = (string)value;
                if (objekt is CheckBox) ((CheckBox)objekt).Checked = (Boolean)value;
                if (objekt is NumericUpDown) ((NumericUpDown)objekt).Value = (long)value;
                options[name.ToLower()] = objekt;
            }
        }

        private Dictionary<string, string[]> readConfigFile(string path)
        {
            Dictionary<string, string[]> settings = new Dictionary<string, string[]>();

            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(path);
            settings = crawlXML(myXmlDocument.DocumentElement);

            return settings;
        }

        private Dictionary<string, string[]> crawlXML(XmlNode node)
        {
            Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
            Dictionary<string, string[]> dic2 = new Dictionary<string, string[]>();

            string[] temp = null;

            //Durchläuft alle Elemente in einem Knoten
            foreach (XmlNode childnode in node.ChildNodes)
            {
                //Weitere Unterknoten vorhanden -> rekursiver Aufruf
                if (childnode.HasChildNodes)
                {
                    dic2 = crawlXML(childnode);
                    //Alle Informationen eintragen, bei doppelten Knoten Zähler anhängen
                    foreach (var pair in dic2)
                    {
                        string info = pair.Key;
                        int cnt = 0;
                        string cnt_str = "";
                        while (cnt != -1)
                        {
                            if (dic.ContainsKey(info + cnt_str) == false)
                            {
                                //Key noch nicht vorhanden
                                dic.Add(info + cnt_str, pair.Value);
                                break;
                            }
                            cnt++;
                            cnt_str = cnt.ToString();
                        }
                    }
                }
                else
                {
                    int anzahl = 0;

                    if (node.Attributes != null) anzahl = node.Attributes.Count;

                    if (anzahl > 0)
                    {
                        temp = new string[anzahl + 1];
                        temp[0] = childnode.InnerText;
                        int count = 0;
                        foreach (XmlAttribute att in node.Attributes)
                        {
                            // attribute stuff
                            //count++;
                            //temp[count] = att.Name;
                            count++;
                            temp[count] = att.Value;
                        }
                    }
                    else
                    {
                        temp = new string[1];
                        temp[0] = childnode.InnerText;
                    }
                    //Komische Eigenschaft von c# die ich nicht verstanden hab
                    if (childnode.Name == "#text") dic.Add(childnode.ParentNode.Name, temp);
                    else dic.Add(childnode.Name, temp);
                }
            }

            return dic;
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //To log errors and show informations
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
        public string error(Exception ex, string msg)
        {
            //Zeigt Fehlermeldungen an und speichert sie im Logfile
            DateTime currentDate = DateTime.Now;

            string lines = "";

            if (ex != null)
            {
                lines = currentDate.ToString() + " " + msg + "\n\r" + ex.ToString() + "\n\r";
            }
            else
            {
                lines = currentDate.ToString() + " " + msg + "\n\r";
            }

            Append("error.log", lines);
            return lines;
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //Conversion
        //////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Wandelt in String in Double um
        /// </summary>
        /// <param name="In"></param>
        /// <returns></returns>
        public static double ToDouble(string In)
        {
            In = In.Replace(",", ".");

            return double.Parse(In, System.Globalization.CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Wandelt String in Double um mit Default falls es nicht möglich ist
        /// </summary>
        /// <param name="In"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static double ToDouble(string In, double Default)
        {
            double dblOut;

            In = In.Replace(",", ".");

            try
            {
                dblOut = double.Parse(In, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                dblOut = Default;
            }

            return dblOut;
        }
        public double[] StringToDouble(string str, char split)
        {
            string[] strArray = str.Split(split);
            double[] array = new double[strArray.Length];
            for (int cnt = 0; cnt < strArray.Length; cnt++)
            {
                string value = strArray[cnt];
                if (value.Length > 0) double.TryParse(value, out array[cnt]);
            }
            return array;
        }
        public double[] StringToDouble(string str)
        {
            return StringToDouble(str, ';');
        }

        public double MagTodB(double variable)
        {
            double result = ((10 * Math.Log10(variable)));
            return result;
        }
        public double[] MagTodB(double[] variable)
        {
            double[] result = new double[variable.Length];
            for (int i = 0; i < variable.Length; i++)
            {
                result[i] = ((10 * Math.Log10(variable[i])));
            }
            return result;
        }

        public double dBtoMag(double variable)
        {
            double result;

            double temp = Math.Pow(10, Math.Abs(variable) / 10);
            if (variable >= 0)
            {
                result = temp;
            }
            else
            {
                result = (1 / temp);
            }
            return result;
        }
        public double[] dBtoMag(double[] variable)
        {
            double[] result = new double[variable.Length];

            for (int i = 0; i < variable.Length; i++)
            {
                double temp = Math.Pow(10, Math.Abs(variable[i]) / 10);
                if (variable[i] >= 0)
                {
                    result[i] = temp;
                }
                else
                {
                    result[i] = (1 / temp);
                }
            }
            return result;
        }

        public double toDouble(string message, double default_value = Double.NaN)
        {
            if (double.TryParse(message, out default_value)) return default_value;
            return default_value;
        }

        public int toInt(string message, int default_value = int.MinValue)
        {
            if (int.TryParse(message, out default_value)) return default_value;
            return default_value;
        }

        public double[] TeilArray(double[] data, int index, int length)
        {
            int ende = index + length;
            if (ende > data.Length)
            {
                length = data.Length - index;
            }
            double[] result = new double[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //Progress bar in a seperate window
        //////////////////////////////////////////////////////////////////////////////////////////
        public void closeWaitingWindow()
        {
            AnimationTimer.Dispose();
            AnimationTimer = null;
            waitWindow.Close();
            waitWindow.Dispose();
            WindowsFenster.Enabled = true;
        }

        public void waitingWindow(string message = "")
        {
            waitWindow = new Form();
            waitWindow.FormBorderStyle = FormBorderStyle.None;

            int x_pos = 10;
            int y_pos = 10;

            int breite = 0;

            Label pleaseWait = new Label();
            pleaseWait.Text = "Please wait";
            pleaseWait.Location = new Point(x_pos, y_pos);
            if (pleaseWait.Width > breite) breite = pleaseWait.Width;
            y_pos += pleaseWait.Location.X + pleaseWait.Height;

            waitWindow.Controls.Add(pleaseWait);

            if (message != "")
            {
                Label messageLabel = new Label();
                messageLabel.Text = message;
                messageLabel.Location = new Point(x_pos, y_pos);
                y_pos += messageLabel.Location.X + messageLabel.Height;
                if (messageLabel.Width > breite) breite = messageLabel.Width;
                waitWindow.Controls.Add(messageLabel);

            }

            ProgressBar Bar = new ProgressBar();
            Bar.Location = new Point(x_pos, y_pos);
            Bar.Size = new Size(breite + x_pos / 2, pleaseWait.Height);
            Bar.Name = "Bar";
            y_pos += Bar.Location.X + Bar.Height;
            waitWindow.Controls.Add(Bar);

            waitWindow.Location = new Point(WindowsFenster.Location.X + WindowsFenster.Width / 2, WindowsFenster.Location.Y + WindowsFenster.Height / 2);
            waitWindow.Height = y_pos;
            waitWindow.Width = breite + x_pos;
            waitWindow.Show();

            // Make this form the active form and make it TopMost
            waitWindow.ShowInTaskbar = false;
            waitWindow.TopMost = true;
            waitWindow.Focus();
            waitWindow.BringToFront();

            WindowsFenster.Enabled = false;

            Application.DoEvents();

            AnimationTimer = new System.Threading.Timer(new TimerCallback(WaitingWindowAnimation), null, 0, 1000);
        }

        private void WaitingWindowAnimation(object obj)
        {
            try
            {
                ProgressBar Bar = (ProgressBar)waitWindow.Controls["Bar"];
                Application.DoEvents();
                if (Bar == null) return;
                int progress = Bar.Value;
                progress += 10;
                if (progress > 100) progress = 0;
                Bar.Invoke(new Action(() => Bar.Value = progress));
                Application.DoEvents();
            }
            catch
            {
                //Disposes itself, in case of error
                if (AnimationTimer != null) AnimationTimer.Dispose();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //Additional functions
        //////////////////////////////////////////////////////////////////////////////////////////

        public String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        public string DialogBox(string message = "Please type a string:", int width = 500, int margin = 10)
        {
            //Abstand rechts, links, oben , untereinander
            int rahmen_links = margin;
            int rahmen_rechts = margin;
            int rahmen_oben = margin;
            int breite = width;
            int hoehe = 110;
            int abstand = margin;

            string result = null;

            Form testDialog = new Form();
            testDialog.Location = new Point(WindowsFenster.Location.X + WindowsFenster.Width / 2, WindowsFenster.Location.Y + WindowsFenster.Height / 2);
            testDialog.Width = rahmen_links + breite + rahmen_rechts;
            testDialog.Height = hoehe;
            testDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            testDialog.MaximizeBox = false;
            testDialog.MinimizeBox = false;
            testDialog.Text = message;

            TextBox textBox = new TextBox();
            textBox.Location = new Point(rahmen_links, rahmen_oben);
            textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBox.Width = breite - rahmen_links - rahmen_rechts;
            testDialog.Controls.Add(textBox);

            Button ok = new Button();
            ok.Text = "OK";
            ok.Location = new Point(textBox.Width + textBox.Location.X - ok.Width, textBox.Location.Y + textBox.Height + abstand);
            //ok.Width = breite / 3;
            ok.Anchor = AnchorStyles.Right;
            testDialog.AcceptButton = ok;
            testDialog.Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "Cancel";
            cancel.Location = new Point(rahmen_links, textBox.Location.Y + textBox.Height + abstand);
            //cancel.Width = breite / 3;
            cancel.Anchor = AnchorStyles.Left;
            testDialog.CancelButton = cancel;
            testDialog.Controls.Add(cancel);

            ok.DialogResult = DialogResult.OK;
            cancel.DialogResult = DialogResult.Cancel;

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (testDialog.ShowDialog(WindowsFenster) == DialogResult.OK)
            {
                //Read the contents of testDialog's TextBox.
                if (textBox.Text != "") result = textBox.Text;
            }
            testDialog.Dispose();

            return result;
        }
    }

    public class FadenPool //= engl. ThreadPool :)
    {
        List<Faden> ThreadListe = new List<Faden>();

        public void add(Faden faden)
        {
            ThreadListe.Add(faden);
        }

        public void remove(Faden faden)
        {
            ThreadListe.Remove(faden);
        }

        public void closeAllThreads()
        {
            foreach (Faden faden in ThreadListe)
            {
                faden.Stop();
            }
        }
    }

    //Zur Übergabe von Funktionen
    public delegate void MyDelegate();

    public class hfDatenEventArgs : EventArgs
    {
        public readonly string Variablen_name;
        public readonly string Inhalt;
        public readonly Dictionary<string, string> Paket;

        public hfDatenEventArgs(string variablen_name, string inhalt)
        {
            Variablen_name = variablen_name;
            Inhalt = inhalt;
            Paket = null;

        }

        public hfDatenEventArgs(Dictionary<string, string> paket, string inhalt)
        {
            Variablen_name = null;
            Inhalt = inhalt;
            Paket = paket;

        }
    }

    //Zur Übermittlung der empfangenen/bearbeiten Thread Daten
    public delegate void ReceiveHandler(object sender, hfDatenEventArgs e);

    //Kontrollfunktionen für einen einzelnen Thread
    public class Faden //= engl. Thread :)
    {
        //Thread an sich
        Thread thread = null;
        //Die Funktion die er bearbeiten soll
        MyDelegate funktion = null;
        //Wie lange soll auf den Stop gewartet werden
        public int stopTimeout = 100; // stopTimeout * 100ms
        //Wartezeit zwischen zwei Durchläufen
        public int waitTime = 100; //ms
        //Falls ein Fehler aufgetreten ist, steht hier die Fehlermeldung
        public Exception errorMessage = null;
        //Thread ist aktiv?
        public Boolean ThreadActive = false;
        //Thread ist beendet??
        Boolean ThreadStopped = false;
        //Thread namen
        public string name = null;

        private event ReceiveHandler _meinEvent;

        public Faden(MyDelegate Funktion, ReceiveHandler EventFunktion = null, string Name = "Thread", Boolean autoStart = true)
        {
            try
            {
                name = Name;
                funktion = Funktion;
                //Dump Evaluationsthread starten
                //Thread im Hintergrund laufen lassen
                thread = new Thread(new ThreadStart(Working));
                thread.IsBackground = true;
                if (autoStart == true) thread.Start();

                if (EventFunktion != null)
                {
                    //Verknüpfe Methode des Hauptprogramm mit dieser Klasse um Daten zu übermitteln
                    _meinEvent += new ReceiveHandler(EventFunktion);
                }
                else
                {
                    _meinEvent = null;
                }
            }
            catch (Exception ex)    //Thread konnte nicht eingerichtet werden
            {
                errorMessage = ex;
            }
        }

        public void start()
        {
            thread.Start();
        }

        private void Working()
        {
            //Zum Starten und Beenden des Threads von außen
            ThreadStopped = false;
            ThreadActive = true;

            //Schleife arbeitet solange bis (von außen) die Auswertung abgebrochen wird
            while (ThreadActive == true)
            {
                funktion();
                if (_meinEvent != null)
                {
                    _meinEvent(this, null);
                }
                //waitTime ms schlafen
                Thread.Sleep(waitTime);
            }
            ThreadStopped = true;
        }

        public void Stop()
        {
            if (ThreadActive == true)
            {
                //Thread stoppen, wenn geöffnet
                ThreadActive = false;
                //Wartet bis der Thread geschlossen ist.
                int cnt = 0;
                while (ThreadStopped == false)
                {
                    cnt++;
                    if (cnt > stopTimeout)
                    {
                        thread.Abort();
                        break;
                    }
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
            }
        }
    }
}
