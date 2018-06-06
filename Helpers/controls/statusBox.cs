using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

namespace ownControls
{
    public partial class statusBox: UserControl
    {
        private uint max_entry = 500;
        private List<string> liste = new List<string>();
        public delegate void Action();

        public statusBox()
        {
            InitializeComponent();
        }

        private void update()
        {
            listBox.DataSource = null;
            listBox.DataSource = liste;

            //Aktuell ausgewähltes Element nicht mehr selektieren
            listBox.ClearSelected();
            //Dafür das letzte, damit die Listbox immer automatisch nach unten scrollt
            try
            {
                if (liste.Count > 0) listBox.SetSelected(liste.Count - 1, true);
            }
            catch
            {

            }
        }

        public void print(string line) {

            try
            {
                //Erzeuge eine neue Zeile auf der ListBox

                //Wenn zuviele Einträge vorhanden, dann Liste löschen
                if (liste.Count > max_entry) liste.Remove(liste[0]);

                //Eintrag hinzufügen
                liste.Add(line);

                if (listBox.InvokeRequired)
                {
                    listBox.Invoke(new Action(update));
                }
                else
                {
                    //Funktion wird aus gleichem Thread aufgerufen
                    update();
                }
            }
            catch
            {

            }
        }

        public void print(string[] lines)
        {
            foreach (string line in lines)
            {
                print(line);
            }
        }

        public void print(Dictionary<string, string> dic, string separator = ":")
        {
            foreach (var pair in dic)
            {
                print(pair.Key + separator + pair.Value);
            }
        }

        public void print(List<string> list)
        {
            foreach (string line in list)
            {
                print(line);
            }
        }

        public void clear()
        {            
            liste.Clear();
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(update));
            }
            else
            {
                //Funktion wird aus gleichem Thread aufgerufen
                update();
            }
        }

        public void setNumberOfEntries(uint number)
        {
            if (number == 0) number = 500;
            max_entry = number;
        }

        public void setHorizontalExtend(uint number)
        {
            if (number == 0) number = 5000;
            listBox.HorizontalExtent = (int)number;
        }

        public void save()
        {
            //k.A. ob es benötigt wird, wenn keine DataSource vorhanden ist
            if (liste != null)
            {

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter myStream = new StreamWriter(saveFileDialog1.FileName, true);
                    foreach (string line in liste)
                    {
                        myStream.WriteLine(line);
                    }
                    // Code to write the stream goes here.
                    myStream.Close();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clear();
        }
    }
}
