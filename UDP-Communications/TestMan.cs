using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

//Zur Kommunikation über UDP
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
//Um einzelne Threads anhalten zu können
using System.Threading;

//Zum Einlesen von XML-Konfigurationsdateien
using System.Xml;
//Stopwatch für Zeitaufnahme
using System.Diagnostics;

//Sollte entfernt werden, wenn Klasse fertig
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

//Zum Filtern von Strings
using System.Text.RegularExpressions;

namespace UDP_Communications
{
    public class UDP_Server
    {
        //////////////////////////////////////////////////////////////////////////////////////
        //Die Klasse kann genutzt werden, um Nachrichten zwischen Programmen auf demselben oder mehreren senden und empfangen zu können.
        //Funktioniert mit C# und in Labview
        //Doku zum Aufbau der Pakete vorhanden
        //Nutzt UDP-Multicast zur serverlosen Kommunikation
        //Per default sind Netzwerkadapter mit x.x.4.x blockiert, um nicht das Lehrstuhlnetz zu fluten
        //
        //Reminder:
        //---------
        //Maximale Paketgröße 64KB  
        //Bei Paketen mit mehreren Variablen darf jede Variable nur einmal vorhanden sein
        //"command" als Variablenname ist ein reserviertes Schlüsselwort!
        //Erneutes Initialisieren funktioniert nicht!
        //
        //////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////
        //Events ans Hauptprogramm nur am Schluss übergeben, sonst kann der Thread hängen bleiben.
        //////////////////////////////////////////////////////////////////////////////////////

        //Gibt Kommentare auf der Console aus.
        public bool debug = true;

        //Wurde die Funktion ohne Konstruktor aufegrufen?
        private bool noConstructor = false;

        //UDP-Client
        private Boolean udp_client_aktiv = false;
        private Boolean udp_client_stopped = false;

        //Create a socket list with all interfaces     
        private List<Socket> udp_send_list = new List<Socket>();
        private List<Socket> udp_receive_list = new List<Socket>();

        //enthält Typ des Servers
        private byte server_type = 255;

        //enthält ID des Servers
        private byte server_id = 255;

        //Aktuell verwendete Version der Klasse
        public readonly string Version = "";

        //Speicher für den manager Thread
        //--------------------------------
        private Dictionary<string, object> ManagerVariables = new Dictionary<string, object>();
        private byte server_id_wish_old = 0;        
        private byte id_wish_counter = 0;
        private byte server_type_wish = 0;
        private byte server_id_wish = 0;
        private byte server_id_inital_wish = 0;

        private byte manager_timer = 0;
        public int packet_limit = 0;
        //-----------------------------------------


        //Maximale Anzahl von Wiederholungen, wie z.B. Kommandos
        private int max_retry = 3;
        //Tiemout für Acks
        public int AckTimeout = 500; //ms

        //Stoppt im Fehlerfall das Relaying
        public bool stopRelay = true;

        //Für das TCP Streaming zwischen Applikationen
        private List<TCPStream> StreamListe = new List<TCPStream>();
        private string TCPClientAddress = null;
        private string TCPServerAddress = null;
        private string tcp_server_endpoint = null;

        //Um Sender gezielt auszublenden
        private List<string> whiteList = new List<string>();
        private List<string> blackList = new List<string>();
        private List<string> received_packetnumbers = new List<string>();
        private bool AdapterFilter = true;

        //Liste mit empfangenen Paketen
        private List<Packet> packet_list = new List<Packet>();

        //Zur Übermittlung der empfangenen Daten als Event
        public delegate void SenderHandler(object sender, DatenEventArgs e);
        public event SenderHandler _meinEvent;

        //Für Registrierung in einer c# App
        public UDP_Server(SenderHandler EmpfangeDaten)
        {
            //Verknüpfe Methode des Hauptprogramm mit dieser Klasse um Daten zu übermitteln
            _meinEvent += new SenderHandler(EmpfangeDaten);

            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Version = string.Format("{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);

            noConstructor = false;
        }

        //Für Labview eigener Konstruktor ohne Übergabe von einer Methode
        public UDP_Server()
        {
            //Verknüpfe mit interner Methode um Daten zu übermitteln (durch getPacket)
            _meinEvent += new SenderHandler(AddPacketsToList);

            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Version = string.Format("{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);

            noConstructor = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //Zum Senden von Paketen und Befehlen
        //////////////////////////////////////////////////////////////////////////////////////

        private bool PacketSyntaxCheck(Dictionary<string, string> Liste, bool checkReservedKeywords = false)
        {
            //TODO: Syntax check, no reserved keywords used, length okay, etc...
            return true;
        }

        public bool send_command(string[] StringArray, out string return_message)
        {
            return SendCommand(StringArrayToDictionary(StringArray), out return_message);
        }

        public bool send_command(string[] StringArray)
        {
            return SendCommand(StringArrayToDictionary(StringArray));
        }

        public bool SendCommand(string[] StringArray)
        {
            return SendCommand(StringArrayToDictionary(StringArray));
        }

        public bool send_command(Dictionary<string, string> Liste)
        {
            return SendCommand(Liste);
        }

        public bool SendCommand(Dictionary<string, string> Liste)
        {
            string return_message = null;
            return SendCommand(Liste, out return_message);
        }

        public bool SendCommand(Dictionary<string, string> Liste, out string return_message)
        {
            return_message = null;
            //Checke Syntax des Befehls
            if (!PacketSyntaxCheck(Liste, true))
            {
                return false;
            }

            //Erzeugt einen Zeitstempel, damit genau auf diese Paket reagiert wird.
            try
            {
                Liste.Add("timestamp", GetTimestamp(DateTime.Now));
            }
            catch
            {
                //Zeitstempel vielleicht schon enthalten?
                Liste["timestamp"] = GetTimestamp(DateTime.Now);
            }

            //Erzeuge ein Paketobjekt daraus
            Packet newPacket = new Packet(Liste, server_type, server_id);
            //Ist ein gültiges Paket entstanden
            if (newPacket.isInvalid == true)
            {
                //Nein, dann raus hier?
                return false;
            }
            //Makiere als ausgehendes Paket
            newPacket.Outgoing = true;

            //Füge Paket zur allgemeinen Liste hinzu(, damit die receiveAcks eingetragen werden können)
            packet_list.Add(newPacket);

            try {
                //Sendet Befehl ab
                SendData(Liste);

                //Warte auf Antwort vom Empfänger
                Stopwatch zeit = new Stopwatch();
                zeit.Start();

                while (newPacket.CommandIsReceived == false)
                {
                    //Verhindert zufrieren des Programmes
                    //Thread.Yield();
                    Thread.Sleep(10);

                    if (!PacketValid(newPacket)) break;

                    //Zählt die abgelaufene Zeit
                    if (zeit.ElapsedMilliseconds > AckTimeout)
                    {
                        zeit.Stop();
                        zeit.Start();
                        //Nach 500 ms Wartezeit wird die Bestätigung vom Empfänger zum Sender wiederholt
                        //Erhöhe den Versuchszähler
                        newPacket.incrementRetryCount();

                        if (newPacket.getRetryCount() > max_retry)
                        {
                            //Abbrechen, wenn >3 mal wiederholt wurde.
                            createEvent("error", "Could not send Command! Receive-Timeout!", 0, 0, newPacket);
                            newPacket.isInvalid = true;
                            return false;
                        }
                        else
                        {
                            //Wiederholt Befehl
                            SendData(Liste);
                        }
                    }
                    if (newPacket.CommandIsExecuted == true)
                    {
                        //Bestätigung der Ausführung erhalten
                        break;
                    }
                }

                //Warte auf Ausführungsbestätigung des Befehls vom Empfänger
                //Der Empfänger kann eine eigene Wartezeit übermitteln, bevor ein Timeout gemeldet wird
                zeit.Stop();
                zeit.Start();
                while (newPacket.CommandIsExecuted == false)
                {
                    //Verhindert das Zufrieren des Programmes
                    //Thread.Yield();
                    Thread.Sleep(10);

                    if (!PacketValid(newPacket)) break;

                    //Zählt die abgelaufene Zeit
                    if (zeit.ElapsedMilliseconds > newPacket.Timeout)
                    {
                        //Nach xx (default: 5) sek Wartezeit abbrechen
                        createEvent("error", "Could not execute Command! Timeout(" + newPacket.Timeout.ToString() + "ms)!", 0, 0, newPacket);
                        newPacket.isInvalid = true;
                        return false;
                    }
                }
                return_message = newPacket.return_message;
                newPacket.isInvalid = true;
                return true;
            } catch (Exception ex) {
                createEvent("error", "General failure at sending a command! " +ex.Message+ "\n" + ex.StackTrace, 0, 0, newPacket);
                newPacket.isInvalid = true;
                return false;
            }             
        }

        //Senden einer einzelnen Variable mit Inhalt
        public bool send_data(string variable, string message)
        {
            return SendData(variable, message);
        }

        public bool SendData(string variable, string message)
        {
            Dictionary<string, string> Liste = new Dictionary<string, string>();
            Liste.Add(variable, message);
            return SendData(Liste);
        }

        //Versucht eine Nachricht an die Multicastgruppe zu versenden

        public bool send_data(string[] StringArray)
        {
            return SendData(StringArray);
        }

        public bool SendData(string[] StringArray)
        {
            return SendData(StringArrayToDictionary(StringArray));
        }

        public bool send_data(Dictionary<string, string> Liste) {
            return SendData(Liste);
        }

        public bool SendData(Dictionary<string, string> Liste)
        {
            try
            {
                string temp = "";
                //Füge eindeutige Packetnummer hinzu, wenn noch nicht vorhanden, um identische Pakete unterscheiden zu können. Pakete können doppelt am Empfänger ankommen, wenn z.B. mehrere Netzwerkadapter an einem Rechner angeschlossen sind
                if (!Liste.TryGetValue("packetnumber", out temp)) Liste.Add("packetnumber", GetTimestamp(DateTime.Now));

                byte[] packet = new byte[64000];

                //Anfang eines Paketes
                packet[0] = 0x60;
                //Typ des Programms
                packet[1] = server_type;
                //ID des Programms
                packet[2] = server_id;
                //Zeiger auf aktuelle Stelle im Paket
                int cnt = 3;
                //Sende alle Strings in dem Verzeichnis Liste
                foreach (KeyValuePair<string, string> pair in Liste)
                {
                    //Konvertiere Variablen String to Byte
                    byte[] byte_variable = StringToByteArray(pair.Key);
                    //Länge des Variablennamens
                    packet[cnt] = Convert.ToByte(byte_variable.Length);
                    cnt++;
                    //Kopiere Name der Variable in Paket                
                    Array.Copy(byte_variable, 0, packet, cnt, byte_variable.Length);
                    //Startwert für nächsten Byte-weisen Kopiervorgang
                    cnt = cnt + byte_variable.Length + 4;
                    //Konvertiere Message String to Byte
                    byte[] byte_message = StringToByteArray(pair.Value);
                    //Länge des MessageStrings schreiben
                    Array.Copy(System.BitConverter.GetBytes(byte_message.Length), 0, packet, cnt - 4, 4);
                    //Message string in Paket kopieren
                    Array.Copy(byte_message, 0, packet, cnt, byte_message.Length);
                    cnt = cnt + byte_message.Length + 1;
                }
                //Loop über alle vorhanden Netzwerkadapter
                foreach (Socket udp_send in udp_send_list)
                {
                    //Senden des Paketes
                    udp_send.Send(packet, cnt, SocketFlags.None);
                }               
            }
            catch (Exception ex)
            {
                createEvent("error", "Could not send Message! " + ex.ToString(), 0, 0);
                return false;
            }
            return true;
        }

        //Empfangsbestätigung eines Befehls vom Empfänger zurück zum Sender
        public void send_answer(bool received, string timeout, string timestamp, string message = null)
        {
            //Funktion muss in einen neuen Thread, um den alten nicht zu blockieren.
            Thread myThread = new System.Threading.Thread(delegate()
            {

            //Klassifizierung der Antwort
            string answer = "executed";
            if (received) answer = "received";

            //Erstelle ein Antwortpaket
            Dictionary<string, string> antwortpaket = new Dictionary<string, string>();
            //... mit der Antwort selber
            antwortpaket.Add("command", answer);
            //... dem Timeout für die Empfangsbestätigung = Zeit um einen Befehl am Empfänger abarbeiten zu können
            if (timeout != null) antwortpaket.Add("timeout", timeout);
            //... und einer eindeutigen Nummer, damit der Sender weiß, zu welchem Befehl der ACK gehört
            antwortpaket.Add("timestamp", timestamp);
            if (message != null)
            {
                antwortpaket.Add("message", message);
            }
            SendData(antwortpaket);

            Packet newPacket = new Packet(antwortpaket, server_type, server_id);
            newPacket.Outgoing = true;

            packet_list.Add(newPacket);

            //Ein 4-way Handshake ist gewünscht, d.h. wir warten auf die Empfangsbestätigung vom Sender

            Stopwatch zeit = new Stopwatch();
            zeit.Start();

            //Wait for ACK from Commander
            while (newPacket.isAck == false)
            {
                //Thread.Yield();
                Thread.Sleep(10);

                if (!PacketValid(newPacket)) break;

                if (zeit.ElapsedMilliseconds > AckTimeout)
                {
                    zeit.Stop();
                    zeit.Start();
                    //Nach 500 ms Wartezeit wird die Bestätigung vom Empfänger zum Sender wiederholt
                    //Erhöhe den Versuchszähler
                    newPacket.incrementRetryCount();
                        
                    if (newPacket.getRetryCount() > max_retry)
                    {
                        //Abbrechen, wenn 3 mal wiederholt wurde.
                        createEvent("error", "Could not get " + answer + " ACK! " + timestamp, 0, 0);
                        break;
                    }
                    else
                    {
                        //Wiederhole ACK
                        SendData(antwortpaket);
                    }
                }
            }

            //Paket löschen
            newPacket.isInvalid = true;

            });
            myThread.Start();
        }

        public bool start_stream(byte Type, byte ID, bool autoConnect = true)
        {
            return startStream(Type, ID);
        }

        public bool startStream(byte Type, byte ID, bool autoConnect = true)
        {
            for (int i = 0; i < StreamListe.Count; i++)
            {
                if (StreamListe[i].check(Type, ID))
                {
                    createEvent("hint", "Already connected.", 0, 0);
                    return true;
                }
            }

            tcp_server_endpoint = null;

            //Warte auf Antwort vom Empfänger
            Stopwatch zeit = new Stopwatch();
            zeit.Start();
            TCPClientAddress = null;

            //Bekomme IP adresse von der gegenseite
            Dictionary<string, string> Liste = new Dictionary<string, string>();
            Liste.Add("command", "getlocalip");
            Liste.Add("type", Type.ToString());
            Liste.Add("id", ID.ToString());
            bool successfull = SendCommand(Liste);

            print("getlocalip:" + successfull.ToString());

            //Warte das die Gegenseite mit einer validen Adresse antwortet
            while (TCPClientAddress == null)
            {
                //Verhindert zufrieren des Programmes
                //Thread.Yield();
                Thread.Sleep(10);

                //Zählt die abgelaufene Zeit
                if (zeit.ElapsedMilliseconds > 3000)
                {
                    //Nach 3 Sekunden warten abbrechen
                    createEvent("error", "Could not get IP-Address from the client.", 0, 0);
                    return false;
                }
            }

            //////////////////////////////////////////////////////
            //Server vorbereiten
            //IP-Adresse erhalten
            string[] Adresse = TCPClientAddress.Split(':');
            TCPStream newStream = new TCPStream(this);

            newStream.type = Type;
            newStream.id = ID;

            successfull = false;
            /////////////////////////////////////
            //Start Server
            if (newStream.start_server(TCPServerAddress, getNextFreePort(TCPServerAddress)))
            {
                int port = newStream.Port;
                /////////////////////////////////////
                //Wait for Clients
                //Funktion muss in einen neuen Thread, um den alten nicht zu blockieren.
                Thread myThread = new System.Threading.Thread(delegate()
                {
                    //Server starten
                    newStream.wait_for_client();
                });
                myThread.Start();

                //////////////////////////////////
                //Client darüber informieren
                Liste.Clear();
                Liste.Add("command", "startTCPClient");
                Liste.Add("type", Type.ToString());
                Liste.Add("id", ID.ToString());
                Liste.Add("ip", TCPServerAddress);
                Liste.Add("port", port.ToString());
                successfull = SendCommand(Liste);

                print("startTCPClient:" + successfull.ToString());
            }

            if ((successfull == false) && (autoConnect))
            {
                try
                {
                    newStream.stop();
                }
                catch
                {
                }
                newStream = null;

                //Nicht erfolgreich!
                //Okay, dann soll der Client probieren, Server zu sein.
                print("hint: Client starts Server.");
                Liste.Clear();
                Liste.Add("command", "startTCPServer");
                Liste.Add("type", Type.ToString());
                Liste.Add("id", ID.ToString());
                successfull = SendCommand(Liste);
            }
            else if (successfull)
            {
                //Okay stream is ready to use! --> Add it to the list...
                if (StreamListe.Contains(newStream))
                {
                    createEvent("error", "There is already a similar stream in the List!", 0, 0);
                    return false;
                }
                else
                {
                    StreamListe.Add(newStream);
                }
            }

            return successfull;
        }

        public bool stop_stream(byte Type = 0, byte ID = 0)
        {
            return stopStream(Type, ID);
        }

        public bool stopStream(byte Type = 0, byte ID = 0)
        {
            bool all = false;
            try
            {
                if ((Type == 0) && (ID == 0)) all = true;

                //foreach (TCPStream strom in StreamListe)
                for (int i = 0; i < StreamListe.Count; i++)
                {
                    if (all)
                    {
                        if (StreamListe[i].stop() == false)
                        {
                            //TODO: something to close it
                        }
                        StreamListe.Remove(StreamListe[i]);
                    }
                    else
                    {
                        if (StreamListe[i].check(Type, ID))
                        {
                            if (StreamListe[i].stop() == false)
                            {
                                //TODO: something to close it
                            }
                            StreamListe.Remove(StreamListe[i]);
                            return true;
                        }
                    }
                }
            }
            catch(Exception err)
            {
                createEvent("error", "Could not remove stream from List! " + err.Message + "\n List will be reseted and all remaning stream removed.", 0, 0);
                StreamListe = null;
                StreamListe = new List<TCPStream>();

            }
            return false;
        }

        public bool write(double[] data, byte Type = 0, byte ID = 0)
        {
            try
            {
                byte[] buffer = GetBytes(data);
                return (write(buffer,Type,ID));
            }
            catch (Exception ex)
            {
                createEvent("error", "Could not write to TCP-Stream! " + ex.ToString(), 0, 0);
                return false;
            }
        }

        public bool write(string data, byte Type = 0, byte ID = 0)
        {
            try {
                byte[] buffer = StringToByteArray(data);
                return (write(buffer, Type, ID));
            }
            catch (Exception ex)
            {
                createEvent("error", "Could not write to TCP-Stream! " + ex.ToString(), 0, 0);
                return false;
            }
        }

        public bool write(byte[] data, byte Type = 0, byte ID = 0)
        {
            try
            {
                bool returnValue = false;

                //No data --> abort transmission
                if (data == null)
                {
                    createEvent("error", "No content!", 0,0);
                    return false;
                }

                //Data is going to be send to several Streams?
                if ((Type == 0) && (ID == 0))
                {
                    if (StreamListe.Count == 0)
                    {
                        createEvent("error", "No stream exist!", 0, 0);
                        return false;
                    }

                    //Going through all currently opend streams
                    for (int i = 0; i < StreamListe.Count; i++)
                    {
                        print("Write to " + StreamListe[i].Type + "," + StreamListe[i].ID);
                        if (StreamListe[i].is_ready_to_send) returnValue = StreamListe[i].Write(data);
                        else
                        {
                            createEvent("warn", "Stream busy", 0, 0);
                        }
                    }
                }
                else
                {
                    TCPStream stream = get_stream(Type, ID);
                    if (stream == null)
                    {
                        createEvent("error", "Stream doesnt exist!", 0, 0);
                        return false;
                    }
                    print("Write to " + stream.Type + "," + stream.ID);
                    if (stream.is_ready_to_send) returnValue = stream.Write(data);
                    else
                    {
                        createEvent("warn", "Stream busy", 0, 0);
                    }
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                createEvent("error", "Could not write to TCP-Stream! " + ex.ToString(), 0, 0);
                return false;
            }
        }

        public bool stream_ready_to_send(byte Type = 0, byte ID = 0)
        {
            if ((Type == 0) && (ID == 0))
            {
                if (StreamListe.Count == 0)
                {
                    createEvent("error", "No stream exist!", 0, 0);
                    return false;
                }

                //Going through all currently opend streams
                for (int i = 0; i < StreamListe.Count; i++)
                {
                    if (StreamListe[i].is_ready_to_send) return true;
                }
                return false;
            }
            else
            {
                TCPStream stream = get_stream(Type, ID);
                if (stream == null)
                {
                    createEvent("error", "Stream doesnt exist!", 0, 0);
                    return false;
                }
                return stream.is_ready_to_send;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //Thread zum Empfangen der UDP Datagramme
        //////////////////////////////////////////////////////////////////////////////////////

        //Der Manager ist gedacht, um alls zu überwachen. Er wird vom untätigen receive Thread aufgerufen.
        private void manager(object source, System.Timers.ElapsedEventArgs e)
        {
            //Hat es ein anderer Thread nötiger aufgerufen zu werden???
            //Thread.Yield();

            if ((manager_timer % 10 == 0) && (server_type == 255) && (server_id == 255))
            {
                //Führe diesen Block jede Skeunde aus
                //Während der Init: Es läuft alles, aber wir sind noch nicht fertig, die ID fehlt

                if (server_id_wish == 255) server_id_wish = 1;

                print("M: " + id_wish_counter.ToString() + ":" + server_type.ToString() + "," + server_id.ToString() + ": " + server_id_wish.ToString());

                if (server_id_wish_old != server_id_wish) id_wish_counter = 0; //Neue ID probieren --> daher cnt zurücksetzen

                if (id_wish_counter > max_retry)
                {
                    //Anscheinend gibt es keinen anderen mit der selben ID
                    server_id = server_id_wish;
                    server_type = server_type_wish;
                    //--> fertig
                    print("M: " + " Nehme folgenden Typ und ID an:" + server_type.ToString() + "," + server_id.ToString());
                    createEvent("warn", "Type, ID: " + server_type.ToString() + "," + server_id.ToString(), 0, 0);
                    return;
                }

                //Alle IDs ausprobiert --> Fehler;
                if (server_id_wish + 1 == server_id_inital_wish)
                {
                    createEvent("error", "ID already assigned! No free ID available! Disconnect from Multicast-Group!", 0, 0);
                    //Funktion muss in einen neuen Thread, um den alten nicht zu blockieren.
                    Thread myThread = new System.Threading.Thread(delegate()
                    {
                        stop_udp();
                    });
                    myThread.Start();

                    print("M: " + "Not Found:" + server_type.ToString() + "," + server_id.ToString());
                }

                //Sende neuen ID-Vorschlag
                Dictionary<string, string> test_id = new Dictionary<string, string>() { { "type", server_type_wish.ToString() }, { "id", server_id_wish.ToString() } };
                SendData(test_id);
                print("M: " + "Sende neue anfrage!");
                //Alte ID ablegen
                server_id_wish_old = server_id_wish;

                id_wish_counter++;
            }
            
            if (initComplete())
            {
                //Lösche nicht mehr verwendete Pakete
                lock (packet_list)
                {                    
                    for (int i = 0; i < packet_list.Count; i++)
                    {
                        //Aktuell beachtetes Paket
                        Packet paket = packet_list[i];
                        //Wie oft wurde das Paket vom Manager gelesen?
                        uint count = paket.ReadFromManager();

                        //Relaying?
                        if (stopRelay == false)
                        {
                            //Checken, ob das Paket nur über einen von mehreren Netzwerkadaptern empfangen wurde, es soll dann weitergeleitet werden
                            if ((udp_receive_list.Count > 1) && (count == 15))
                            {
                                //Iteration über alle vorhandenen Netzwerkadapter
                                for (int AdapterCount = 0; AdapterCount < udp_send_list.Count; AdapterCount++)
                                {
                                    if (!paket.Adapter.Contains(AdapterCount))
                                    {
                                        try
                                        {
                                            //Das Paket wurde nicht von diesem Netzwerkadapter empfangen, d.h. es muss hier weitergeleitet werden
                                            udp_send_list[AdapterCount].Send(paket.ByteArray, paket.ByteArray.Length, SocketFlags.None);
                                            print("Relay packet " + paket.Packetnumber.ToString() + " to adapter " + AdapterCount.ToString());
                                        }
                                        catch (Exception ex)
                                        {
                                            createEvent("error", "Relaying failed! " + ex.Message, 0, 0);
                                            stopRelay = true;
                                        }
                                    }
                                }
                            }
                        }

                        //Wurde das Paket schon mehr als 3 mal vom Manager gelesen und ist zudem schon abgearbeitet?
                        if ((count == 0) && (paket.isInvalid == true))
                        {
                            //dann wird es gelöscht.
                            packet_list.Remove(paket);
                        }

                        //Wenn gewünscht, verwerfe alte Packete...
                        if ((packet_limit != 0) && (packet_list.Count > packet_limit))
                        {
                            packet_list.RemoveAt(0);
                        }
                    }
                }
            }

            //Jede sek aufrufen.
            if (manager_timer % 10 == 0)
            {
                //Nach unbenutzen Streams guggn
                for (int i = 0; i < StreamListe.Count; i++)
                {
                    if (StreamListe[i].is_running() == false)
                    {
                        StreamListe.Remove(StreamListe[i]);
                    }
                }
            }

            manager_timer++;
        }

        private void ReceiveData()
        {            
            //Zum Starten und Beenden des Threads von außen
            udp_client_stopped = false;
            udp_client_aktiv = true;

            //Manager kümmert sich um die Commandos, etc.
            System.Timers.Timer ManagerTimer = new System.Timers.Timer(100);
            ManagerTimer.Elapsed += new System.Timers.ElapsedEventHandler(manager);
            ManagerTimer.Start();

            //Schleife arbeitet solange bis (von außen) der Empfang der UDP-Pakete abgebrochen wird
            while (udp_client_aktiv == true)
            {                
                //Iteration über alle vorhandenen Netzwerkadapter
                for (int AdapterCount = 0; AdapterCount < udp_receive_list.Count; AdapterCount++)
                {
                    Socket udp_receive = udp_receive_list[AdapterCount];
                    //Falls mal ein Paket nicht richtig empfangen wurde
                    try
                    {
                        //Daten überhaupt verfügbar??
                        if (udp_receive.Available > 0)
                        {
                            //Enthält alle empfangenen Daten
                            byte[] packet = new byte[64000];

                            // Bytes empfangen.
                            udp_receive.Receive(packet);

                            //Dekodierung
                            Packet newPacket = decode(packet, AdapterCount);      //Leere, eigene, black list und doppelt empfangene Pakete werden hier schon gefiltert

                            //Nichts empfangen?
                            if (newPacket != null)
                            {
                                //Funktion muss in einen neuen Thread, um den alten nicht zu blockieren.
                                Thread myThread = new System.Threading.Thread(delegate()
                                {

                                    //Analyse und Filterung, wenn das Paket nicht schon vorher aussortiert wurde
                                    if (!newPacket.isInvalid) newPacket = evaluate(newPacket);

                                    //Nicht rausgekickt?, dann ist es ein Paket dass man an das Hauptprogramm weiter geben sollte.
                                    if (!newPacket.isInvalid)
                                    {
                                        createEvent("new", "packet", newPacket.SenderType, newPacket.SenderID, newPacket);
                                        //Wenn Klasse von Labview/Simple Chat aufgerufen wird, dann wird das Paket hier noch nicht als invalid gekennzeichnet, sondern erst von der getPacket Funktion
                                        if (noConstructor == false) newPacket.isInvalid = true;
                                    }

                                    //Jedes Paket wird zur Pakteliste hinzugefügt
                                    lock (packet_list)
                                    {
                                        packet_list.Add(newPacket);
                                    }

                                });
                                myThread.Start();
                            }
                        }
                        else
                        {
                            //Keine Daten, dann einfach warten
                            //Thread.Yield();
                            Thread.Sleep(10);
                        }

                    }
                    //Empfang der Traces fehlgeschlagen
                    catch (System.Net.Sockets.SocketException ex)       //Netzwerkfehler
                    {
                        if ((ex.ErrorCode == 10060) || (ex.ErrorCode == 10035))
                        {
                            //Einfach nichts innerhalb der Zeitspanne empfangen! 10035--> Fehlercode unter Linux --> kurze Pause
                            //Thread.Yield();
                            Thread.Sleep(10);
                        }
                        else
                            createEvent("error", "UDP-Thread receive network error: " + ex.ErrorCode.ToString(), 0, 0);
                    }
                    catch (Exception ex)                            //anderweitiger Fehler
                    {
                        createEvent("error", "UDP-Thread receive error: " + ex.ToString(), 0, 0);
                    }
                }
            }

            //Stop den Manager
            ManagerTimer.Stop();
            ManagerTimer.Close();

            //Traces werden nicht mehr empfangen
            udp_client_stopped = true;
        }

        //Dekodiere Paket
        //d.h. ordne die Variablen zu etc.
        //Sortiere doppelte und nicht an uns gerichtete pakete aus
        private Packet decode(byte[] packet, int AdapterCount)
        {
            //Type und ID des Senders von dem wir das Paket erhalten haben
            byte Sender_Type = 0;
            byte Sender_ID = 0;

            //Enthält das dekodierte Paket getrennt nach Variablen, Inhalten als strings
            Dictionary<string, string> packet_content = new Dictionary<string,string>();

            //Durchsuche komplettes Paket
            for (int i = 0; i < packet.Length; i++)
            {
                //Makiert den Beginn eines Paketes
                if (packet[i] == 0x60)
                {
                    //Informationen über den Sender herausschreiben
                    Sender_Type = packet[i + 1];
                    Sender_ID = packet[i + 2];

                    //Eigene Mitteilungen herausfiltern
                    if ((Sender_ID == server_id) && (Sender_Type == server_type)) return null;
                    //White und Black list checken
                    if (!checkId(Sender_Type, Sender_ID)) return null;

                    //Jetzt alle übermittelten Variablen dekodieren
                    for (i = i + 3; i < packet.Length; i++)
                    {
                        byte length_of_var = packet[i];
                        if (length_of_var == 0) break;
                        string variable = System.Text.Encoding.ASCII.GetString(packet, i + 1, length_of_var);
                        i = i + length_of_var + 1;
                        int length_of_msg = System.BitConverter.ToInt32(packet, i);
                        string message = System.Text.Encoding.ASCII.GetString(packet, i + 4, length_of_msg);
                        i = i + length_of_msg + 4;
                                 
                        //das komplette Paket wird als Dictionary übergeben
                        packet_content.Add(variable, message);                                    
                    }
                }
            }

            //Darf das Paket empfangen werden?
            //d.h. es hat Inhalt??
            if ((packet_content == null) || (packet_content.Count == 0))
            {
                return null;
            }

            Packet returnPacket = new Packet(packet_content, Sender_Type, Sender_ID);
            //Lege auch das empfangene Byte Array ab
            returnPacket.ByteArray = packet;
            //Hinterlegen über welchen Adapter das Paket empfangen wurde
            returnPacket.Adapter.Add(AdapterCount);

            //temporärer String
            string value = null;
            //Paket mehrmals empfangen?
            if (packet_content.TryGetValue("packetnumber", out value))
            {
                if (received_packetnumbers.Contains(value))
                {
                    returnPacket.isInvalid = true;
                    //Damit Liste nicht übervoll wird
                    if (received_packetnumbers.Count > 500)
                    {
                        received_packetnumbers.RemoveAt(0);
                    }
                    return null;
                }

                Packet PaketAusDerListe = getPacket(UInt64.Parse(value));
                if (PaketAusDerListe != null)
                {
                    //dann filtern, in dem es als invalid gekennzeichnet wird
                    returnPacket.isInvalid = true;
                    //Das gefundene Paket wurde auch von einem anderen Netzwerkadapter gefunden
                    PaketAusDerListe.Adapter.Add(AdapterCount);
                    return null;
                }
                received_packetnumbers.Add(value);
            }
            
            //Erzeuge ein Packet Objekt
            return returnPacket;
        }

        //Evaluiere Paket
        //werte den inhalt aus --> z.B. ist eine Antwort nötig.
        private Packet evaluate(Packet Paket)
        {
            print("E: " + DictionaryToString(Paket.OriginalContent));

            //temporär
            string value = null;

            //------------------------------------------------------------------------------------------------------------------
            //ID-Zuordnung und versions unterschiede finden
            //------------------------------------------------------------------------------------------------------------------

            //Ein anderer oder unserer Server ist auf ID suche.
            if (Paket.isIDSearch)
            {
                //Wir sind auf ID suche??
                if ((server_id == 255) && (server_type == 255))
                {
                    print("E: ID schon an jemand anderen vergeben. " + server_type_wish.ToString() + "=" + Paket.SenderType.ToString() + ", " + server_id_wish.ToString() + "=" + Paket.SenderID.ToString());

                    //... und haben eine Rückmeldung bekommen???
                    if ((Paket.SenderID == server_id_wish) && (Paket.SenderType == server_type_wish))
                    {                        
                        //Unsere ID ist schon vergeben
                        //daher nächster Vorschlag
                        server_id_wish++;
                        print("E: Nächster Vorschlag " + server_id_wish.ToString());
                        id_wish_counter = 0;

                        createEvent("warn", "ID " + Paket.SenderID.ToString() + " not available!", 0, 0);
                    }
                    else
                    {
                        //Wasn hier los? Diese Ablehnung nicht akzeptiert
                        print("E: ID-Ablehnung nicht akzeptiert: " + DictionaryToString(Paket.OriginalContent));
                    }
                }
                else if (Paket.isCommand)
                {
                    //Ein anderer Server hat schon geantwortet

                }
                else if ((Paket.receiveType == server_type) && (Paket.receiveID == server_id)) //Anderer Server auf ID suche
                {
                    //Die ID ist schon an uns vergeben --> Mitteilung senden
                    print("E: ID schon an uns vergeben. " + Paket.receiveType.ToString() + ", " + Paket.receiveID.ToString());
                    SendData(new Dictionary<string, string>() { { "command", "ID not available" }, { "version", Version } });
                }
                else
                {
                    //Hhmh geht an wen anders, einfach versions nummer senden.
                    //Sende Versionsnummer der UDP-Klasse
                    SendData("version", Version);
                }
                Paket.isInvalid = true;
            }

            //Nach Versionsunterschieden suchen
            if (Paket.Content.TryGetValue("version", out value))
            {
                if (value != Version)
                {
                    print("E: Versionsunterschied! " + Version + " != " + value + " used by " + Paket.SenderType + ", " + Paket.SenderID);                    

                    //Versionsunterschiede der verwendeten UDP Klasse festgestellt
                    createEvent("hint", "Our server runs with a different version " + Version + " than " + value + " used by " + Paket.SenderType + ", " + Paket.SenderID, 0, 0);
                }

                Paket.isInvalid = true;
            }
            //------------------------------------------------------------------------------------------------------------------


            //Paket nur eine Statusnachricht??
            if (!Paket.isStatus)
            {
                //Nein! Es ist ein ein Ack bzw. Command

                //aber gilt der auch für uns?
                //Unpassende Befehle herausfiltern
                if ((Paket.receiveType != 0) && (Paket.receiveType != server_type)) Paket.isInvalid = true;
                if ((Paket.receiveID != 0) && (Paket.receiveID != server_id)) Paket.isInvalid = true;

                if (!Paket.isInvalid)
                {

                    if (!Paket.isCommand)
                    {
                        //Paket ist ein ack

                        lock (packet_list)
                        {
                            //Durchsuchen aller Pakete
                            for (int i = 0; i < packet_list.Count; i++)
                            {
                                //Können wir ein receiveAck einem gesendetem Command zu ordnen
                                if (packet_list[i].Timestamp == Paket.Timestamp)
                                {
                                    Packet found = packet_list[i];

                                    if ((found != null) && (found.Outgoing))
                                    {
                                        print("E: ack received for " + Paket.Timestamp);
                                        if (Paket.Content.TryGetValue("message", out value)) found.return_message = value;

                                        if ((Paket.isReceiveAck) && (found.isCommand))
                                        {
                                            //Hier Antwort in Form eines Acks senden
                                            //Erstelle ein Antwortpaket
                                            Dictionary<string, string> antwortpaket = new Dictionary<string, string>();
                                            //... mit der Antwort selber
                                            antwortpaket.Add("command", "ack");
                                            //... und einer eindeutigen Nummer, damit der Sender weiß, zu welchem Befehl der ACK gehört
                                            antwortpaket.Add("timestamp", found.Timestamp);
                                            //antwort ist direkt an den Sender gerichtet
                                            antwortpaket.Add("id", Paket.SenderID.ToString());
                                            antwortpaket.Add("type", Paket.SenderType.ToString());
                                            SendData(antwortpaket);

                                            //Makierung für den Befehl setzen, dass er erhalten wurde
                                            found.CommandIsReceived = true;
                                            found.Timeout = Paket.Timeout;
                                            
                                            print("E: receiveAck");
                                        }
                                        else if ((Paket.isExecuteAck) && (found.isCommand))
                                        {
                                            //Hier Antwort in Form eines Acks senden
                                            //Erstelle ein Antwortpaket
                                            Dictionary<string, string> antwortpaket = new Dictionary<string, string>();
                                            //... mit der Antwort selber
                                            antwortpaket.Add("command", "ack");
                                            //... und einer eindeutigen Nummer, damit der Sender weiß, zu welchem Befehl der ACK gehört
                                            antwortpaket.Add("timestamp", found.Timestamp);
                                            //antwort ist direkt an den Sender gerichtet
                                            antwortpaket.Add("id", Paket.SenderID.ToString());
                                            antwortpaket.Add("type", Paket.SenderType.ToString());
                                            SendData(antwortpaket);

                                            //Makierung für den Befehl setzen, dass er erhalten wurde
                                            found.CommandIsExecuted = true;
                                            //Falls auf mehrere Bestätigungen gewartet wird
                                            found.CommandExecutedCounter++;

                                            print("E: executeAck");
                                        }
                                        else if (Paket.isAck)
                                        {
                                            found.isAck = true;
                                            //Antwort auf ein receiveAck erhalten
                                            print("E: ack");
                                        }
                                        else if (Paket.Timeout != 5000) //Standard wartezeit in der Paketklasse
                                        {
                                            //Neue Informationen über eine längere Wartezeit erhalten
                                            found.Timeout = Paket.Timeout;

                                            print("E: timeout update");
                                        }
                                        else
                                        {
                                            //Ganz putziger Fall --> reporten?     
                                            print("E: Unknown state.");
                                        }
                                    }
                                }
                            }
                        }
                        //Damit es nicht bis zum Hauptprogramm durchdringt und gelöscht wird
                        Paket.isInvalid = true;
                    }
                    else
                    {
                        //Paket könnte ein command sein, der vielleicht nur an die UDP-Klasse gerichtet ist.                        
                        switch (Paket.Command.ToLower())
                        {
                            case "getlocalip":
                                send_answer(true, "1000", Paket.Timestamp);
                                //Wird die IP Adresse dieser Application angefordert??
                                print("E: getlocalip");
                                //Bekomme alle Netzwerkadapter und dessen IP-Adressen
                                Dictionary<int, List<IPAddress>> liste = getInterfaceIndex();
                                int ip_cnt = 0;
                                Dictionary<string, string> antwort = new Dictionary<string, string>();
                                antwort.Add("command", "response");
                                antwort.Add("answer", "getlocalip");
                                antwort.Add("id", Paket.SenderID.ToString());
                                antwort.Add("type", Paket.SenderType.ToString());
                                foreach (var pair in liste)
                                {
                                    foreach (IPAddress addr in pair.Value)
                                    {
                                        string port = getNextFreePort(addr.ToString()).ToString();
                                        if (port == "0") break;
                                        antwort.Add(ip_cnt.ToString(), addr.ToString() + ":" + port);
                                        print(ip_cnt.ToString() + ", " + addr.ToString() + ":" + port);
                                        ip_cnt++;
                                    }
                                }
                                SendCommand(antwort);
                                send_answer(false, null, Paket.Timestamp);

                                //Damit es nicht bis zum Hauptprogramm durchdringt und gelöscht wird
                                Paket.isInvalid = true;
                            break;

                            case "starttcpclient":

                                send_answer(true, "10000", Paket.Timestamp);
                                Paket.isInvalid = true;
                                print("E: Trying to connect to TCP-Server:" + Paket.toString());

                                createEvent("hint", "Init stream with " + Paket.SenderType.ToString() + ", " + Paket.SenderID.ToString(), 0, 0);

                                TCPStream newStream = new TCPStream(this);
                                if (Paket.Content.TryGetValue("ip", out value))
                                {
                                    newStream.ip = value;
                                }
                                else break;
                                if (Paket.Content.TryGetValue("port", out value))
                                {
                                    newStream.port = Int32.Parse(value);
                                }
                                else break;

                                if (newStream.connect_to_server(newStream.ip, newStream.port))
                                {

                                    newStream.type = Paket.SenderType;
                                    newStream.id = Paket.SenderID;

                                    StreamListe.Add(newStream);

                                    send_answer(false, null, Paket.Timestamp);
                                    print("E: Done!"); 
                                }
                                else
                                {
                                    try
                                    {
                                        newStream.stop();
                                    }
                                    catch
                                    {
                                    }
                                    newStream = null;

                                    createEvent("error", "Couldn't connect to " + Paket.SenderType.ToString() + ", " + Paket.SenderID.ToString(), 0, 0);
                                }

                            break;

                            case "starttcpserver":
                                send_answer(true, "15000", Paket.Timestamp);
                                Paket.isInvalid = true;
                                print("E: Trying to start TCP-Server:" + Paket.toString());

                                createEvent("hint", "Init stream with " + Paket.SenderType.ToString() + ", " + Paket.SenderID.ToString(), 0, 0);

                                if (start_stream(Paket.SenderType, Paket.SenderID, false))
                                {
                                    send_answer(false, null, Paket.Timestamp);
                                    print("E: Done!");
                                }
                            break;

                            case "tcpreconnect":
                                //Instand feedback
                                send_answer(true, "500", Paket.Timestamp);                               

                                TCPStream stream = get_stream(Paket.SenderType, Paket.SenderID);
                                
                                if ((stream != null) && (!stream.is_server()))
                                {
                                    if (Paket.Content.TryGetValue("ip", out value))
                                    {
                                        stream.ip = value;
                                    }
                                    if (Paket.Content.TryGetValue("port", out value))
                                    {
                                        stream.port = Int32.Parse(value);
                                    }
                                    send_answer(false, null, Paket.Timestamp);
                                    stream.try_to_reconnect(true);
                                }
                                
                            break;

                            case "response":
                                //Antwort auf einen Befehl erhalten
                                if (Paket.Content.TryGetValue("answer", out value))
                                {
                                    send_answer(true, "500", Paket.Timestamp);

                                    switch (value)
                                    {
                                        case "getlocalip":                                            
                                            bool ready = false;

                                            //IP Adressen von Gegenseite empfangen
                                            Dictionary<int, List<IPAddress>> ipadressen = getInterfaceIndex();
                                            foreach (var pair in ipadressen)
                                            {
                                                foreach (IPAddress addr in pair.Value)
                                                {
                                                    //Server
                                                    string[] addrArray = addr.ToString().Split('.');
                                                    string searchAddr = addrArray[0] + "." + addrArray[1] + "." + addrArray[2];

                                                    foreach (var receivedAddr in Paket.Content)
                                                    {
                                                        //Client
                                                        string[] c_addrArray = receivedAddr.Value.ToString().Split(':');
                                                        print("E: is " + searchAddr + " matching to client " + c_addrArray[0]);

                                                        int pos = receivedAddr.Value.IndexOf(searchAddr);
                                                        if (pos != -1)
                                                        {
                                                            print("E: Found " + c_addrArray[0] + " as Client and " + addr.ToString() + " as Server pair");
                                                            TCPClientAddress = receivedAddr.Value;
                                                            TCPServerAddress = addr.ToString();
                                                            //Perfektes Match gefunden!
                                                            if ((c_addrArray[0] != addr.ToString()))
                                                            {
                                                                print("E: Found perfect match!");
                                                                ready = true;
                                                                break;                                                                
                                                            }
                                                        }
                                                    }
                                                    if (ready == true) break;
                                                }
                                                if (ready == true) break;
                                            }
                                            if (ready == false)
                                            {
                                                createEvent("hint", "Did not found a match between remote ip and our! Our:\n" + DictionaryToString(ipadressen) + "\n Remote:" + DictionaryToString(Paket.Content), 0, 0);
                                            }
                                            break;

                                        default:

                                            break;
                                    }

                                    send_answer(false, null, Paket.Timestamp);

                                    Paket.isInvalid = true;

                                }  
                            break;

                            default:
                                //Wurde der Befehl schon mal empfangen??
                                lock (packet_list)
                                {
                                    //Durchsuchen aller Pakete
                                    for (int i = 0; i < packet_list.Count; i++)
                                    {
                                        //Können wir ein receiveAck einem gesendetem Command zu ordnen
                                        if ((packet_list[i].Timestamp == Paket.Timestamp) && (packet_list[i].isCommand))
                                        {
                                            print("E: command received twice!");
                                            Paket.isInvalid = true;
                                            break;
                                        }
                                    }
                                }

                                break;
                        }                                                 
                    }
                }
            }
            else
            {
                //Statusmitteilung
                //tcp_server_endpoint
                if (Paket.Content.TryGetValue("tcp_server_endpoint", out value))
                {
                    tcp_server_endpoint = value;
                }
            }

            print("------------------------------------------------------------");

            return Paket;
        }
        
        //(Für Labview) eigene interner Empfänger, der alle Informationen der Reihenfolge nach (FIFO-Prinzip) speichert.
        private void AddPacketsToList(object sender, DatenEventArgs e)
        {
               
            //Temporäre Variable
            Packet paket = new Packet();            

            //Mitteilung vom Server erhalten, z.B. Fehler, Statusbericht
            if ((e.Variablen_name.Equals("warn", StringComparison.InvariantCultureIgnoreCase) == true) || (e.Variablen_name.Equals("error", StringComparison.InvariantCultureIgnoreCase) == true) || (e.Variablen_name.Equals("hint", StringComparison.InvariantCultureIgnoreCase) == true))
            {
                paket.Type = e.Variablen_name;
                paket.Content = new Dictionary<string, string>() { {e.Variablen_name, e.Inhalt} };
                paket.isInvalid = false;
            }

            if ((e.Variablen_name == "new") && (e.Inhalt == "packet"))
            {
                //Paket steht schon in der Queue
            }
            else
            {
                lock (packet_list)
                {
                    packet_list.Add(paket);
                }
            }
        }

        //(Für Labview) Empfangene Pakete vorhanden??
        public Boolean data_available()
        {
            return DataAvailable();
        }

        public Boolean DataAvailable()
        {
            if (getPacket(true) != null) return true;
            else return false;
        }

        //(Für Labview) Bekomme Paket als String-Array
        public string[] getPacketAsStringArray(bool completePacket = false)
        {
            Packet var = getPacket();
            if (var != null) return var.toStringArray(completePacket);
            return null;
        }

        public string getPacketAsString(string seperationString = "|", bool completePacket = false)
        {
            Packet var = getPacket();
            if (var != null) return var.toString(seperationString, completePacket);
            return null;
        }

        public Packet getPacket(bool doNotMarkAsInvalid = false)
        {
            lock (packet_list)
            {
                for (int i = 0; i < packet_list.Count; i++)
                {
                    Packet paket = packet_list[i];
                    if (paket.isInvalid)
                    {
                        //Schon bearbeitete Pakete
                    }
                    else if (paket.Outgoing)
                    {
                        //Ausgesendete Pakete
                    }
                    else
                    {
                        //Zu lesende Pakete!
                        if (!doNotMarkAsInvalid) paket.isInvalid = true;
                        return paket;
                    }
                }
            }
            return null;
        }

        public Packet getPacket(string timestamp)
        {
            lock (packet_list)
            {
                for (int i = 0; i < packet_list.Count; i++)
                {
                    if (packet_list[i].Timestamp == timestamp) return packet_list[i];
                }
            }
            return null;
        }

        public Packet getPacket(UInt64 packetnumber)
        {
            lock (packet_list)
            {
                for (int i = 0; i < packet_list.Count; i++)
                {
                    if (packet_list[i].Packetnumber == packetnumber) return packet_list[i];
                }
            }
            return null;
        }

        private bool PacketValid(Packet paket)
        {           
            lock (packet_list)
            {
                if (packet_list.Contains(paket) == false) return false;
                for (int i = 0; i < packet_list.Count; i++)
                {
                    if ((packet_list[i].Equals(paket) == true) && (packet_list[i].isInvalid == true)) return false;
                }
                return true;
            }            
        }

        public List<Packet> getPacketList()
        {
            return packet_list;
        }

        public bool stream_data_available(out byte RemoteType, out byte RemoteID)
        {
            return StreamDataAvailable(out RemoteType, out RemoteID);
        }

        public bool StreamDataAvailable(out byte RemoteType, out byte RemoteID)
        {
            RemoteType = 0;
            RemoteID = 0;

            foreach (TCPStream strom in StreamListe)
            {
                if (strom.DataAvailable() == true)
                {
                    RemoteType = strom.Type;
                    RemoteID = strom.ID;
                    return true;
                }
            }
            return false;
        }

        public byte[] getStreamData(byte RemoteType, byte RemoteID)
        {
            foreach (TCPStream strom in StreamListe)
            {
                if (strom.check(RemoteType, RemoteID))
                {
                    if (strom.DataAvailable() == true)
                    {
                        return strom.getData();
                    }
                }
            }
            return null;
        }

        public string getUpdatedTcpEndpoint()
        {
            return tcp_server_endpoint;
        }

        public int getNumbersInPacketList()
        {
            return this.getPacketList().Count;
        }

        public void clear_packet_list()
        {
            packet_list.Clear();
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //Initialisierung
        //////////////////////////////////////////////////////////////////////////////////////

        //Ermittle die Nummer und IP-Adressen eines jeden Netzwerkadapters im Rechner
        private Dictionary<int, List<IPAddress>> getInterfaceIndex(string filter = "")
        {
            //Iterate through al adapters
            Dictionary<int, List<IPAddress>> liste = new Dictionary<int, List<IPAddress>>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    //Filtere unerwünschte Adapter anhand ihrer IP adresse aus
                    List<IPAddress> ipadressen = new List<IPAddress>();
                    Boolean addni = true;
                    print(ni.Name);
                    print(ni.GetIPProperties().GetIPv4Properties().Index.ToString());
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            string ipaddr = ip.Address.ToString();
                            print(ip.Address.ToString());

                            if (filterOut(filter, ipaddr))
                            {
                                addni = false;
                                print("Not Allowed");
                            }
                            else
                            {
                                print("Allowed");
                            }

                            ipadressen.Add(ip.Address);
                        }
                    }
                    if (addni) liste.Add(ni.GetIPProperties().GetIPv4Properties().Index, ipadressen);
                }
            }
            return liste;
        }

        private bool filterOut(string filter_string, string ipaddress)
        {
            //Wenn gewünscht wird der Filter deaktiviert
            if (AdapterFilter == false) return false;

            //Füge Lehrstuhlnetz als Standardfilter ein
            filter_string = ".4.;.90.;" + filter_string;
            string[] filters = filter_string.Split(';');

            foreach (string filter in filters)
            {

                if ((filter != null) && (filter != ""))
                {
                    int pos = ipaddress.IndexOf(filter);
                    if (pos != -1) return true;
                    //Nur lokale Netze!
                    pos = ipaddress.IndexOf("192.168.");
                    if (pos == -1) return true;

                }
            }
            return false;
        }

        //(Manuelles) Initialisieren der Netzwerkkommunikation
        public bool init_udp(string ipadress, int port, int ttl, byte type, byte id, bool wait = false, string filter = "")
        {

            //Ist schon ein Thread aktiv?, dann keine Initialisierung
            if (udp_client_aktiv == false)
            {
                createEvent("hint", "UDP Version " + Version, 0, 0);

                uint cnt = 0;

                //Init des Empfangs
                //We now need to join a multicast group. Multicast IP addresses are within the Class D range of 224.0.0.0-239.255.255.255
                IPAddress ip = IPAddress.Parse(ipadress);

                //Bekomme alle Netzwerkadapter und dessen IP-Adressen
                Dictionary<int, List<IPAddress>> liste = getInterfaceIndex(filter);
                //Liste mit allen Sockets auf denen gelauscht wird --> Löschen
                udp_receive_list.Clear();
                //Jeden Netzwerkadapter durchgehen
                foreach (var pair in liste)
                {
                    //Für Debugzwecke, Netzwerkadpater nummer + alle IP-Adressen in einen String schreiben
                    string eth_info = "eth" + pair.Key.ToString() + ": ";
                    foreach (IPAddress addr in pair.Value)
                    {
                        eth_info += addr + " ";
                    }

                    //Empfangs-Thread starten
                    try
                    {                        
                        ////////////////Vorbereiten für Senden
                        //We first create a socket as if we were creating a normal unicast UDP socket.
                        Socket udp_send = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                        // Bind the socket to default IP address and port. 0 means the system itself is looking for a free port.
                        udp_send.Bind(new IPEndPoint(pair.Value[0], 0));

                        //Other applications can use that socket
                        udp_send.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

                        // Add membership to the group.
                        MulticastOption mcastOpt = new MulticastOption(ip, pair.Value[0]);
                        udp_send.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOpt);

                        // Set the required interface for outgoing multicast packets.
                        udp_send.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, (int)IPAddress.HostToNetworkOrder(pair.Key));

                        //This sets the time to live for the socket - this is very important in defining scope for the multicast data. 
                        //Setting a value of 1 will mean the multicast data will not leave the local network, setting it to anything 
                        //above this will allow the multicast data to pass through several routers, with each router decrementing the TTL by 1. 
                        //Getting the TTL value right is important for bandwidth considerations.
                        udp_send.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);

                        //timeout in ms
                        udp_send.SendTimeout = 100;

                        //This creates the end point that allows us to send multicast data, we connect the socket to this end point. 
                        //We are now a fully fledged member of the multicast group and can send data to it 
                        udp_send.Connect(new IPEndPoint(ip, port));

                        //Füge Empfangs- und Sende-sockets zur klasseninternen Liste hinzu, die ein späteres iterieren über alle Adapter emöglicht
                        udp_send_list.Add(udp_send);


                        ////////////////Vorbereiten für Empfangen
                        Socket udp_receive = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        //Damit mehrere Programme den selben Port nutzen können
                        udp_receive.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                        //We create an IP endpoint for the incoming data to any IP address on port [port] and bind that to the socket.
                        IPEndPoint ipep2 = new IPEndPoint(IPAddress.Any, port);
                        udp_receive.Bind(ipep2);

                        udp_receive.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, pair.Key));

                        udp_receive.ReceiveTimeout = 100;

                        //Füge Empfangs- und Sende-sockets zur klasseninternen Liste hinzu, die ein späteres iterieren über alle Adapter emöglicht
                        udp_receive_list.Add(udp_receive);
                        


                        //Hinweis zurück an das Hauptprogramm
                        createEvent("hint", "Listen on " + cnt.ToString() +  ": "+ eth_info + ": " + ipadress + ", " + port.ToString() + ", TTL " + ttl.ToString(), 0, 0);
                        cnt++;
                    }
                    catch (Exception ex)
                    {
                        createEvent("error", "UDP-Communication on Interface" + eth_info + " could not initialised! " + ex.ToString(), 0, 0);
                    }
                }

                //Empfangsthread im Hintergrund laufen lassen
                Thread receiveThread = new Thread(new ThreadStart(ReceiveData));
                receiveThread.IsBackground = true;
                receiveThread.Start();

                //Setzen der ID und Typ
                server_id_wish = id;
                server_type_wish = type;
                //--> Manager kümmert sich drum

                // Create new stopwatch to build a timeout
                Stopwatch stopwatch = new Stopwatch();

                // Begin timing
                stopwatch.Start();                

                if (wait)
                {
                    while (!initComplete())
                    {
                        //Thread.Yield();
                        Thread.Sleep(10);
                        if (stopwatch.Elapsed > TimeSpan.FromMinutes(1)) return false;
                    }
                }

            }
            else
            {
                createEvent("error", "UDP-Thread is already running!", 0, 0);
                return false;
            }

            return true;
        }        

        //Zum Initialisiern mit Hilfe einer Konfigdatei
        public Dictionary<string, string> init_udp(string path)
        {

            string udp_port = null;
            string udp_ttl = null;
            string udp_ip = null;
            string udp_id = null;
            string udp_type = null;
            Dictionary<string, string> settings = new Dictionary<string, string>();

            if (path != null)
            {
                settings = readConfigFile(path);
                settings.TryGetValue("Port", out udp_port);
                settings.TryGetValue("TTL", out udp_ttl);
                settings.TryGetValue("IP", out udp_ip);
                settings.TryGetValue("ID", out udp_id);
                settings.TryGetValue("Type", out udp_type);
            }

            //Defaulteinstellungen
            if (udp_port == null) udp_port = "50000";
            if (udp_ttl == null) udp_ttl = "3";
            if (udp_ip == null) udp_ip = "224.5.6.7";
            if (udp_id == null) udp_id = "1";
            if (udp_type == null) udp_type = "255";

            if (init_udp(udp_ip, Convert.ToInt32(udp_port), Convert.ToInt32(udp_ttl), Convert.ToByte(udp_type), Convert.ToByte(udp_id), true) == false)
            {
                return null;
            }
            return settings;
        }

        //Zum Stoppen des Empfangs von außen
        public void stop_udp()  
        {
            //Stoppe alle Streams
            stopStream();

            int cnt = 0;
            //Trace Thread stoppen
            udp_client_aktiv = false;
            //Wartet bis der Thread geschlossen ist.
            while (udp_client_stopped == false)
            {
                //Kein Zufrieren der GUI - hoffentlich :)
                Thread.Sleep(100);
                cnt++;
                //Länger als 3 Sekunden gewartet?
                if (cnt > 30) break;
            }

            //TODO: bessere Möglichkeit zum Schließen der Sockets???
            foreach (Socket udp_receive in udp_receive_list)
            {
                //udp_receive.Disconnect(true);
                udp_receive.Close();
            }
            foreach (Socket udp_send in udp_send_list)
            {
                //udp_send.Disconnect(true);
                udp_send.Close();
            }
        }

        //Server aktiv??
        public Boolean udp_active()
        {
            return udp_client_aktiv;
        }

        public bool init_complete()
        {
            return initComplete();
        }

        public bool initComplete()
        {
            if (!((server_id == 255) && (server_type == 255))) return true;
            return false;
        }

        private int getNextFreePort(string ipaddr = "127.0.0.1")
        {
            int port = 0;
            try
            {                
                //Öffne Dummy mäßig einen TCP Stream mit der Portnummer 0
                //Der TCP Stack sucht dann automatischen einen freien Port heraus
                //den lesen wir dann heraus
                TcpListener dummyListener = new TcpListener(new IPEndPoint(IPAddress.Parse(ipaddr), 0));
                dummyListener.Start();
                IPEndPoint endpoint = (IPEndPoint)dummyListener.LocalEndpoint;
                port = endpoint.Port;
                dummyListener.Stop();
                dummyListener = null;
            }
            catch(Exception ex)
            {
                print("Couldn't find a free port! " + ex.Message);
            }
            return port;
        }

        public void adapter_filter(bool activate = true) {
            AdapterFilter = activate;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //Kontrollfunktionen wie Black und Whitelist von Programmen, die man (nicht) empfangen möchte
        //Nie wirklich getestet! :)
        //////////////////////////////////////////////////////////////////////////////////////
        private Boolean checkId(string client)
        {
            //Darf der Server Nachrichten von diesem Server annehmen?
            if (whiteList.Contains(client)) return true;
            if (blackList.Contains(client)) return false;
            if (blackList.Count > 0) return true;
            if (whiteList.Count == 0) return true;
            return false;
        }

        private Boolean checkId(byte client_type, byte client_id)
        {
            return checkId(client_type.ToString() + "-" + client_id.ToString());
        }

        public void AddToWhiteList(byte client_type, byte client_id)
        {
            AddToWhiteList(client_type.ToString() + "-" + client_id.ToString());
        }

        public void AddToWhiteList(string client)
        {
            whiteList.Add(client);
        }

        public void AddToBlackList(byte client_type, byte client_id)
        {
            AddToBlackList(client_type.ToString() + "-" + client_id.ToString());
        }

        public void AddToBlackList(string client)
        {
            blackList.Add(client);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        //Sonstige Funktionen
        //////////////////////////////////////////////////////////////////////////////////////

        //Lese Konfigdatei aus und schreibt alles in ein Dictionary
        public Dictionary<string, string> readConfigFile(string path)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();

            XmlDocument myXmlDocument = new XmlDocument();
            myXmlDocument.Load(path);
            settings = crawlXML(myXmlDocument.DocumentElement);

            return settings;
        }

        //Geht durch alle XML-Knoten durch --> Kann rekursiv verwendet werden, um auch Unterknoten auszuwerten
        public Dictionary<string, string> crawlXML(XmlNode node)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Dictionary<string, string> dic2 = new Dictionary<string, string>();
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
                    //Komische Eigenschaft von c# die ich nicht verstanden hab
                    if (childnode.Name == "#text") dic.Add(childnode.ParentNode.Name, childnode.InnerText);
                    else dic.Add(childnode.Name, childnode.InnerText);
                }
            }

            return dic;
        }

        //Zwei Methoden mit denen man einen String in ein Byte Array und ein Bytearray in einen String wandeln kann.
        public byte[] StringToByteArray(string str)
        {
            if (str != null)
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                return enc.GetBytes(str);
            }
            else
            {
                return null;
            }
        }

        public string ByteArrayToString(byte[] arr)
        {
            if (arr != null)
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                return enc.GetString(arr);
            }
            else
            {
                return null;
            }
        }

        public byte getServerID()
        {
            return server_id;
        }

        public byte getServerType()
        {
            return server_type;
        }

        public void createEvent(string name, string content, byte type = 0, byte id = 0)
        {
            //Funktion muss in einen neuen Thread, um den alten nicht zu blockieren.
            Thread myThread = new System.Threading.Thread(delegate()
            {
                _meinEvent(this, new DatenEventArgs(name, content, type, id));
            });
            myThread.Start();
        }

        public void createEvent(string name, string content, byte type = 0, byte id = 0, Packet paket = null)
        {
            //Funktion muss in einen neuen Thread, um den alten nicht zu blockieren.
            Thread myThread = new System.Threading.Thread(delegate()
            {
                _meinEvent(this, new DatenEventArgs(name, content, type, id, paket));
            });
            myThread.Start();
        }

        //Für Labview trennung von Status und Inhalt
        //public string[] PacketArrayToString(string[] packet)
        //{
        //    string[] result = new string[2];
        //    for (int i = 0; i < 4; i++)
        //    {
        //        result[0] += packet[i] + ";";
        //    }
        //    for (int i = 4; i < packet.GetLength(0); i++)
        //    {
        //        result[1] += packet[i] + ";";
        //    }
        //    return result;
        //}

        //Wandelt ein Dictionary in ein String Array um, ggf. mit Werten am Anfang des Arrays (preString)
        //public string[] DictionaryToStringArray(Dictionary<string, string> Liste, string[] preString)
        //{
        //    string[] str = new string[preString.GetLength(0) + Liste.Count * 2];
        //    int cnt = 0;
        //    foreach (string word in preString)
        //    {
        //        str[cnt++] = word;
        //    }
        //    string[] temp = DictionaryToStringArray(Liste);
        //    foreach (string word in temp)
        //    {
        //        str[cnt++] = word;
        //    }
        //    return str;
        //}

        public Dictionary<string, string> StringArrayToDictionary(string[] stringArray)
        {
            Dictionary<string, string> Liste = new Dictionary<string, string>();
            try
            {
                for (int i = 0; i < stringArray.GetLength(0); i += 2)
                {
                    Liste.Add(stringArray[i], stringArray[i + 1]);
                }
            }
            catch (Exception ex)
            {
                createEvent("error", "Problems at adding strings from the array to the dictionary! " + ex.ToString(), 0, 0);
            }
            return Liste;
        }

        //Bekomme einen Zeitstempel im Stringformat nicht zu verwechseln mit einem UNIX timestamp!
        private String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        private object getValue(string name)
        {
            object value = null;
            if (ManagerVariables.TryGetValue(name, out value)) return value;
            return null;
        }

        private void setValue(string name, object value)
        {
            object test = null;
            if (ManagerVariables.TryGetValue(name, out test))
            {
                ManagerVariables[name] = value;
            }
            else
            {
                ManagerVariables.Add(name, value);
            }
        }

        ///////////////////////////////////////////////////////
        //Für Debugzwecke
        ///////////////////////////////////////////////////////

        private void print(string text)
        {
            if (debug)
            {
                Console.WriteLine(text);
            }
        }
        
        public string DictionaryToString(Dictionary<string, string> Liste)
        {
            string str = "";
            foreach (var pair in Liste)
            {
                str = str + "|" + pair.Key + ":" + pair.Value + "|";
            }
            return str;
        }

        public string DictionaryToString(Dictionary<int, List<IPAddress>> Liste)
        {
            string str = "";
            foreach (var pair in Liste)
            {
                str += "|" + pair.Key.ToString() + ":";
                foreach (IPAddress add in pair.Value)
                {
                    str += add.ToString() + ";";
                }             
            }
            return str;
        }

        public string[] DictionaryToStringArray(Dictionary<string, string> Liste)
        {
            string[] str = new string[Liste.Count * 2];
            int cnt = 0;
            foreach (var pair in Liste)
            {
                str[cnt++] = pair.Key;
                str[cnt++] = pair.Value;
            }
            return str;
        }

        public byte[] GetBytes(double[] values)
        {
            var result = new byte[values.Length * sizeof(double)];
            Buffer.BlockCopy(values, 0, result, 0, result.Length);
            return result;
        }

        public double[] GetDoubles(byte[] bytes)
        {
            var result = new double[bytes.Length / sizeof(double)];
            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);
            return result;
        }

        private TCPStream get_stream(byte Type, byte ID)
        {
            //Going through all currently opend streams
            for (int i = 0; i < StreamListe.Count; i++)
            {
                if ((StreamListe[i].Type == Type) && (StreamListe[i].ID == ID))
                {
                    return StreamListe[i];
                }
            }
            return null;
        }

    }

    //////////////////////////////////////////////////////////////////////////////////////
    ////Zur Übermittlung von Daten bei Auslösen eines Events
    ////d.h. von UDP-Klasse zurück zum Hauptprogramm
    //////////////////////////////////////////////////////////////////////////////////////

    public class DatenEventArgs : EventArgs
    {
        public readonly string Variablen_name;
        public readonly string Inhalt;
        public readonly byte Sender_Typ;
        public readonly byte Sender_ID;
        public readonly Packet Paket;

        public DatenEventArgs(string variablen_name, string inhalt, byte sender_Typ, byte sender_ID)
        {
            //Used to give a message form the class to the main application
            Variablen_name = variablen_name;
            Inhalt = inhalt;
            Paket = null;
            Sender_Typ = sender_Typ;
            Sender_ID = sender_ID;
        }

        public DatenEventArgs(string variablen_name, string inhalt, byte sender_Typ, byte sender_ID, Packet paket)
        {
            //Used to inform about a wrong or received Packet
            Variablen_name = variablen_name;
            Inhalt = inhalt;
            Paket = paket;
            Sender_Typ = sender_Typ;
            Sender_ID = sender_ID;
        }
    }

    public class TCPStream
    {
        private UDP_Server serverinstance = null;

        //Informationen über Ziel
        public byte type = 0;
        public byte id = 0;
        public int port = 0;
        public string ip = null;

        //Show messages at local commandline
        private bool debug = true;

        //Für Server
        private TcpListener myListener = null;

        //Für Client
        private TcpClient myClient = null;

        //This is the socket were everything goes through, for server as well as client
        private Socket socket = null;

        //Zum Abholen der Daten
        private byte[] data = null;
        private bool dataavailable = false;

        //Controlthread
        private Thread receiveThread = null;
        private bool sleeping = false;
        private bool thread_is_receiving = false;
        private bool stop_thread = false;
        private bool reconnect_running = false;
        private bool stream_active = true;

        //Timeouts festlegen
        private long MAX_WARTEZEIT = 5000; //ms
        private int SHORT_WARTEZEIT = 1000; //ms
        //xx K große Kontrollpakete
        private int PACKETSIZE = 4096;

        //Widerholung der Packete im Fehlerfall
        private int MAX_RETRY_WRITING = 3;

        public TCPStream(UDP_Server ServerInstance = null)
        {
            serverinstance = ServerInstance;
        }

        ///////////////////////////////////////////////////
        //Start and stop Stream
        ///////////////////////////////////////////////////

        public bool start_server(string ServerIP = "", int ServerPort = 0)
        {
            try
            {
                //Clean up old session!
                stop(false, false);

                if (ServerIP == "")
                {
                    ServerIP = get_local_ip();
                }

                //Remember given parameters
                ip = ServerIP;
                port = ServerPort;

                //Starte TCP-Connection                        
                IPAddress serverIP = IPAddress.Parse(ServerIP);
                try
                {
                    //Zuerst den Server der auf Daten lauscht
                    /* Initializes the Listener */
                    myListener = new TcpListener(serverIP, port);
                    /* Start Listening at the specified port */
                    myListener.Start();
                }
                catch
                {
                    print("warn: Specified port maybe not available! Try again with another Port!");
                    try
                    {
                        myListener.Stop();
                        myListener = null;
                        port = getNextFreePort(serverIP.ToString());
                        myListener = new TcpListener(serverIP, port);
                    }
                    catch
                    {
                        print("Could not set up server!");
                        myListener = null;
                        return false;
                    }
                }

                //Warte das ein Client sich mit unserem Server verbindet
                print("Server: The local End point is :" + myListener.LocalEndpoint);
                return true;
            }
            catch (Exception ex)
            {
                error(ex, "Could not setup server!");
                return false;
            }
        }

        public bool wait_for_client()
        {
            try
            {
                //Create timer to realize timeout error
                Stopwatch timeout = new Stopwatch();
                timeout.Start();
                //Server waits for clients
                print("Server: Waiting for a connection.....");
                while (myListener.Pending() == false)
                {
                    Thread.Sleep(SHORT_WARTEZEIT);
                    //xx Sekunden warten
                    if (timeout.ElapsedMilliseconds > MAX_WARTEZEIT)
                    {
                        error(null, "Server: Timeout! No client connected.");
                        return false;
                    }

                }
                start_socket(myListener.AcceptSocket());

                print("Server: Connection accepted from " + socket.RemoteEndPoint + " and TCP-Connection established.");

                return true;
            }
            catch (Exception ex)
            {
                error(ex, "Waiting for client failed.");
                return false;
            }
        }

        public bool connect_to_server(string ServerIP = "", int ServerPort = 0)
        {
            try
            {
                //Clean up old session!
                stop(false, false);

                //If no IP address is given use local ip address
                if (ServerIP == "")
                {
                    ServerIP = get_local_ip();
                }

                //Remember given parameters
                ip = ServerIP;
                port = ServerPort;

                //Most likly to report the timeout message
                string error_msg = null;

                //Create new TCP Client
                myClient = new TcpClient();
                //Create the IP-Address out of the string
                IPAddress clientIP = IPAddress.Parse(ip);
                print("Client: Trying to connect to other server:" + ip + ":" + port.ToString());
                //Start a Timer to realize a timeout
                Stopwatch timeout = new Stopwatch();
                timeout.Start();
                while (myClient.Connected == false)
                {
                    //Es kann vorkommen, dass der andere Server noch nicht läuft, daher mehrmals probieren
                    try
                    {
                        //Try to connect
                        myClient.Connect(clientIP, port);
                    }
                    catch (Exception ex)
                    {
                        //most likly a timeout error
                        Thread.Sleep(SHORT_WARTEZEIT);
                        //prepare the error message
                        error_msg = "Error: Client:" + ex.Message + "\n" + ex.StackTrace;
                    }

                    //Maximal 10 Sekunden warten
                    if (timeout.ElapsedMilliseconds > MAX_WARTEZEIT)
                    {
                        //Ok use the previous created error message
                        error(null, "Client could not connect to remote server: Timeout! " + error_msg);
                        return false;
                    }
                }

                //Everything fine, than start controlthread
                start_socket(myClient.Client);

                print("Client is connected to remote server and TCP-Connection established.");
            }
            catch (Exception ex)
            {
                error(ex, "Cannot connect to server");
                return false;
            }

            return true;
        }

        private void start_socket(Socket sock)
        {
            stop_thread = false;

            socket = sock;

            socket.ReceiveTimeout = 2 * SHORT_WARTEZEIT;
            socket.SendTimeout = SHORT_WARTEZEIT;
            socket.ReceiveBufferSize = PACKETSIZE;

            //Empfangsthread im Hintergrund laufen lassen
            receiveThread = new Thread(new ThreadStart(control_thread));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        public bool stop(bool withNewThread = false, bool sendStopMessage = true)
        {
            if (withNewThread)
            {
                //Funktion soll in einen neuen Thread, um den alten nicht zu blockieren.
                Thread myThread = new System.Threading.Thread(delegate()
                {
                    stop();
                });
                myThread.Start();
                return true;
            }
            else
            {
                print("TCP Connection closing...");
                try
                {
                    if ((receiveThread != null) && (receiveThread.IsAlive))
                    {
                        //Stop receive thread!
                        stop_thread = true;
                        int cnt = 0;
                        while (thread_is_receiving)
                        {
                            Thread.Sleep(10);
                            cnt += 10;
                            if (cnt > SHORT_WARTEZEIT)
                            {
                                error(null, "Our recieving thread blocks! Kill it...");
                                //Kill the control thread
                                try
                                {
                                    receiveThread.Abort();
                                }
                                catch (Exception ex)
                                {
                                    error(ex, "Recieve Thread already closed.");
                                }
                                break;
                            }
                        }
                    }

                    //Inform other side about Closing!
                    try
                    {
                        if (sendStopMessage) write_message("stop");
                    }
                    catch (Exception e)
                    {
                        print("error: Cannot send stop message " + e.Message + "\n" + e.StackTrace);
                    }

                    //Close NetworkStreams...
                    try
                    {
                        if (myListener != null)
                        {
                            myListener.Stop();
                        }
                        else if (myClient != null)
                        {
                            myClient.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        error(ex, "Couldnt close Server or Client.");
                    }

                    //Close underlying Network Socket
                    try
                    {
                        if (socket != null) socket.Close();
                    }
                    catch (Exception ex)
                    {
                        error(ex, "Couldnt close Socket.");
                    }

                    //Reset all common functions
                    receiveThread = null;
                    myListener = null;
                    myClient = null;
                    socket = null;
                }
                catch (Exception ex)
                {
                    error(ex, "Error while stopping.");
                    return false;
                }
                print("Stream stopped " + type.ToString() + "," + id.ToString());
                return true;
            }
        }

        private bool is_connected()
        {
            if (receiveThread.IsAlive == false) return false;
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException ex) {
                error(ex, "Check: Connection is not working! " + ex.SocketErrorCode);
                return false; 
            }
        }

        private void control_thread()
        {
            //Loop which only ends, when bool variable controlthreadruns changed
            while (stop_thread == false)
            {
                //Falls mal ein Paket nicht richtig empfangen wurde
                try
                {
                    //Writing process is going on and reading thread should not interfere!
                    thread_is_receiving = false;
                    if (sleeping)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    //Ok start listening on socket for status messages
                    thread_is_receiving = true;

                    string message = read_message();

                    if (message != "")
                    {
                        print(message);
                        //Evaluate status message
                        switch (message)
                        {
                            case "new_data":
                                if (read())
                                {
                                    //Hauptprogramm mitteilen, dass Daten da sind.
                                    serverinstance.createEvent("new", "data", type, id);
                                    dataavailable = true;                                    
                                }

                                break;
                            case "stop":                                
                                reconnect_running = true; //Prevent any unintendet reconnect, which can be occure because of the stopping procedure
                                print("Stop message received!");
                                stop(true);
                                stream_active = false;
                                goto Finish;  

                            case "check":
                                //Echo zurück sozusagen als Bestätigung
                                write_message(message);
                                break;
                        }
                    }
                    else if (message == null)
                    {
                        //Serious problem occured --> stop everything.
                        goto Error;  
                    }
                    else
                    {
                        //Nichts empfangen, do some clean up...

                        //Beim letzten mal empfangene Daten löschen
                        if ((dataavailable == false) && (data != null)) data = null;

                        //Connection still running
                        if (is_connected() == false)
                        {
                            //If not, than ...
                            goto Error;  
                        }
                    }
                }
                catch (Exception ex)  //anderweitiger Fehler
                {
                    print("error: TCP-Thread receive error:\n" + ex.Message + "\n" + ex.StackTrace);
                    //Serious problem occured --> stop everything.
                    goto Error;                 
                }

            }
            goto Finish;
            Error:                
                try_to_reconnect(true);   
            Finish:
            stop_thread = false;
            thread_is_receiving = false;
        }

        public bool try_to_reconnect(bool withNewThread = false)
        {
            if (reconnect_running == true)
            {
                error(null, "Reconnect already ongoing!");
                return false;
            }

            if (withNewThread)
            {
                //Funktion soll in einen neuen Thread, um den alten nicht zu blockieren.
                Thread myThread = new System.Threading.Thread(delegate()
                {
                    try_to_reconnect();
                });
                myThread.Start();
                return true;
            }
            else
            {
                reconnect_running = true;

                //Server running? --> Inform other side
                if (serverinstance != null)
                {
                    Dictionary<string, string> Liste = new Dictionary<string, string>();
                    Liste.Add("command", "TCPreconnect");
                    Liste.Add("type", type.ToString());
                    Liste.Add("id", id.ToString());
                    Liste.Add("ip", ip);
                    Liste.Add("port", port.ToString());
                    serverinstance.SendCommand(Liste);
                }

                bool successfull = false;

                if (!is_server())
                {
                    //We are client.
                    successfull = connect_to_server(ip, port);
                }
                else
                {
                    //We are Server.
                    if (start_server(ip, port) != false)
                    {
                        successfull = wait_for_client();
                    }
                }

                if (successfull == false)
                {
                    stop();
                    stream_active = false;
                }

                reconnect_running = false;

                return successfull;
            }
        }

        ///////////////////////////////////////////////////
        //Write and Read stuff
        ///////////////////////////////////////////////////

        private bool read()
        {
            //Reads the binary data

            write_message("start");
            //Get the length of the data
            string str = read_message();
            //Now we need to convert to a length to know what is coming
            int length = 0;
            if (Int32.TryParse(str, out length))
            {
                print("Receiving " + length.ToString() + " Bytes...");
                //Gives an ACK back, so that the send can transmit data
                write_message("ok");
                //Create stream out of the socket
                try
                {
                    //Contains all received data
                    data = new byte[length];
                    //Enthält temporär alle empfangenen Daten
                    byte[] buffer = new byte[PACKETSIZE];
                    //Counter to determine the position within the streamed data
                    int read_pointer = 0;
                    int bytesToRead = PACKETSIZE;
                    //Short packet?
                    if (bytesToRead > length) bytesToRead = length;
                    //Read until finished
                    while (length > 0)
                    {
                        //print("Read: " + length.ToString() + ", " + read_pointer.ToString() + ", " + bytesToRead.ToString());
                        bytesToRead = socket.Receive(buffer, bytesToRead, SocketFlags.None);
                        Array.Copy(buffer, 0, data, read_pointer, bytesToRead);
                        //Calculate next pointer position
                        read_pointer += bytesToRead;
                        //Calculate the remaining length of the data
                        length -= bytesToRead;
                        //Are we near the end of the data block?
                        if (PACKETSIZE > length)
                        {
                            bytesToRead = length;
                        }
                        else
                        {
                            //Sometimes it receive less data...
                            bytesToRead = PACKETSIZE;
                        }
                    }

                    print(read_pointer.ToString() + " Bytes received.");
                    if (read_pointer != data.Length)
                    {
                        print("warn: data maybe not complete.");
                        write_message("false");
                    }
                    else
                    {
                        write_message("true");
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    error(ex, "Streaming of data failed!");
                }
                //Did we read the complete stuff?
                if ((data != null) && (data.Length == length)) return true;
            }
            else
            {
                error(null, "Did not get length of binary data!");
            }
            return false;
        }

        private string read_message()
        {
            //Reads a status message
            try
            {
                //Wait a little bit until enough data is available
                int cnt = 0;
                while (socket.Available < PACKETSIZE)
                {
                    Thread.Sleep(10);
                    cnt += 10;
                    if (cnt > SHORT_WARTEZEIT) return ""; //No data available
                }

                byte[] buffer = new byte[PACKETSIZE];
                //Receiving data
                if (socket.Receive(buffer, PACKETSIZE, SocketFlags.None) > 0)
                {
                    //Strip unused characters
                    return getString(buffer).ToLower();
                }
            }
            catch (TimeoutException ex)
            {
                error(ex, "error: Timeout! Cannot read status message.");
                return "";
            }
            catch (Exception ex)
            {
                error(ex, "General connection failure! Cannot read status message.");
            }
            return null;
        }

        public byte[] getData()
        {
            //This function gives the received data back to the main program
            dataavailable = false;
            return data;
        }

        public bool DataAvailable()
        {
            return dataavailable;
        }

        private bool write_message(string message)
        {
            //Writes a status message
            try
            {
                byte[] msg = string_to_byte_array(message);
                byte[] buffer = new byte[PACKETSIZE];
                //Create the message
                Array.Copy(msg, 0, buffer, 0, msg.Length);
                //Send it
                socket.Send(buffer, 0, PACKETSIZE, SocketFlags.None);
                return true;
            }
            catch (Exception ex)
            {
                error(ex, "Cannot send data! " + message);
            }
            return false;
        }

        public bool Write(byte[] binary_data, int attempt = 0)
        {
            if ((!is_ready_to_send) && (attempt == 0))
            {
                error(null, "Other transmission going on!");
                return false;
            }

            attempt++;
            if (attempt > MAX_RETRY_WRITING)
            {
                error(null, "Cannot send data! After " + attempt.ToString() + " I give up.");
                return false;
            }

            //Länge der Daten
            int length = binary_data.Length;

            //Set receive thread to sleep!
            sleeping = true;
            int cnt = 0;
            while (thread_is_receiving)
            {
                Thread.Sleep(10);
                cnt += 10;
                if (cnt > MAX_WARTEZEIT)
                {
                    error(null, "Our recieving thread blocks!");
                    sleeping = false;
                    print("I try it again...");
                    return Write(binary_data, attempt);
                }
            }

            //Inform remote side to preapre to receive data
            write_message("new_data");
            //Other side ready?
            if (read_message() == "start")
            {
                //Yes, write length of data
                write_message(length.ToString());
                //Did other side understand?
                if (read_message() == "ok")
                {
                    //Counter to determine the position within the streamed data
                    int write_pointer = 0;
                    int bytesToWrite = PACKETSIZE;
                    //Short packet?
                    if (bytesToWrite > length) bytesToWrite = length;
                    //Send the real data until finished
                    while (length > 0)
                    {
                        //print("Write: " + length.ToString() + ", " + write_pointer.ToString() + ", " + bytesToWrite.ToString());
                        try
                        {
                            //Write junk of the data
                            if (socket.Send(binary_data, write_pointer, bytesToWrite, SocketFlags.None) < bytesToWrite)
                            {
                                //Error occured
                                print("Error occured! Abort...");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            error(ex, "Couldnt send data! Abort...");
                            break;
                        }

                        //Calculate next pointer position
                        write_pointer += bytesToWrite;
                        //Calculate the remaining length of the data
                        length -= bytesToWrite;
                        //Are we near the end of the data block?
                        if (bytesToWrite > length)
                        {
                            bytesToWrite = length;
                        }
                    }

                    if ((length == 0) && (read_message() == "true"))
                    {
                        sleeping = false;
                        return true;
                    }
                    print("warn: Some data is missing!");
                }
                else
                {
                    error(null, "Remote side did not understand length of data!");
                }
            }
            else
            {
                error(null, "Did not get ACK for start of transmission!");
            }
            sleeping = false;
            print("I try it again...");
            return Write(binary_data, attempt);
        }

        ///////////////////////////////////////////////////
        //Debug und sonstige Funktionen
        ///////////////////////////////////////////////////

        private int getNextFreePort(string ipaddr = "127.0.0.1")
        {
            int port = 0;
            try
            {
                //Öffne Dummy mäßig einen TCP Stream mit der Portnummer 0
                //Der TCP Stack sucht dann automatischen einen freien Port heraus
                //den lesen wir dann heraus
                TcpListener dummyListener = new TcpListener(new IPEndPoint(IPAddress.Parse(ipaddr), 0));
                dummyListener.Start();
                IPEndPoint endpoint = (IPEndPoint)dummyListener.LocalEndpoint;
                port = endpoint.Port;
                dummyListener.Stop();
                dummyListener = null;
            }
            catch (Exception ex)
            {
                error(ex, "Couldn't find a free port! " + ex.Message);
            }
            return port;
        }

        private void print(string text)
        {
            if (debug)
            {
                Console.WriteLine(text);
            }
            if (serverinstance != null)
            {
                if (text.IndexOf("error") != -1)
                {
                    serverinstance.createEvent("error", text, 0, 0);
                }
                else
                {
                    serverinstance.createEvent("hint", text, 0, 0);
                }
            }       
        }

        private byte[] string_to_byte_array(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }

        private string byte_array_to_string(byte[] arr)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(arr);
        }

        private string get_local_ip()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        private string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings. 
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "");                
            }
            // If we timeout when replacing invalid characters,  
            // we should return Empty. 
            catch
            {
                return String.Empty;
            }
        }

        private string getString(byte[] input)
        {
            return CleanInput(byte_array_to_string(input));
        }

        private void Append(string sFilename, string sLines)
        {
            ///<summary>
            /// Fügt den übergebenen Text an das Ende einer Textdatei an.
            ///</summary>
            ///<param name="sFilename">Pfad zur Datei</param>
            ///<param name="sLines">anzufügender Text</param>
            ///
            Stopwatch zeit = new Stopwatch();
            zeit.Start();
            StreamWriter myFile = null;
            while (myFile == null)
            {
                try
                {
                    myFile = new StreamWriter(sFilename, true);
                }
                catch (IOException)
                {
                    Thread.Sleep(1);
                }
                if (zeit.ElapsedMilliseconds > MAX_WARTEZEIT)
                {
                    print("Cannot open logfile " + sFilename);
                    return;
                }
            }
            myFile.Write(sLines);
            myFile.Close();
        }
        private void error(Exception ex, string msg)
        {
            try
            {
                //Zeigt Fehlermeldungen an und speichert sie im Logfile
                print(msg);
                DateTime currentDate = DateTime.Now;

                if (ex != null)
                {
                    print(ex.Message);
                    Append("error.log", currentDate.ToString() + " " + msg + "\n\r" + ex.Message + "\n" + ex.StackTrace + "\n\r");
                }
                else
                {
                    Append("error.log", currentDate.ToString() + " " + msg + "\n\r");
                }
            }
            catch (Exception e)
            {
                print("Cannot report error!" + "\n\r" + e.Message + "\n" + e.StackTrace + "\n\r");
            }
        }

        public bool check(byte Type = 0, byte ID = 0)
        {
            //Check if argument is supplied
            if ((type > 0) && (id > 0))
            {
                //Yes, then check if the stream is connected to the right remote side
                if (type != Type) return false;
                if (id != ID) return false;
            }
            //Return if stream is still working
            return is_connected();
        }

        public byte Type
        {
            get { return type; }
            set { type = value; }
        }

        public byte ID
        {
            get { return id; }
            set { id = value; }
        }

        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public bool is_server()
        {
            if (myListener == null)
            {
                return false;
            }
            return true;
        }

        public bool is_running()
        {
            return stream_active;
        }

        public bool is_ready_to_send
        {
            get
            {
                if (sleeping) return false;
                if (!thread_is_receiving) return false;
                return true;
            }
        }
    }

    public class Packet
    {
        //TODO: edit public avialable functions to the needs! I think some should be readonly


        //What kind of packet
        private string type = "status";
        //Information about the sender of the packet
        private byte sender_id = 0;
        private byte sender_type = 0;
        //Packetnumber is intended to filter out if a packet is received twice
        private UInt64 packetnumber = 0;
        //Rest of the content
        private Dictionary<string, string> content = new Dictionary<string, string>();
        private Dictionary<string, string> original_content = new Dictionary<string, string>();
        //Byte Array enthält alle original empfangenen Daten
        private byte[] bytearray = null;
        //Über welchen Netzwerkadapter wurde das Paket empfangen
        private List<int> adapter = new List<int>();
        //If a command is send or received...
        private string command = "";
        //Timestamp to order commands
        private string timestamp = "";
        //First Response to a command
        private bool isreceiveack = false;
        //Ack that the receive ack is received
        private bool receiveackack = false;
        //Command succesfully received
        private bool commandreceived = false;
        //Execution ack to a command
        private bool isexecuteack = false;
        //ack that the execution ack is received
        private bool executeackack = false;
        //Command successfully executed
        private bool commandexecuted = false;
        //Command executed by several apps
        private int commandexecutedcounter = 0;
        //Ack that the both of the above acks a received.
        private bool isack = false;
        //information about the timeout for a command
        private int timeout = 5000; //ms
        //A command is send to a specific or a group of clients
        private byte receiveid = 0;
        private byte receivetype = 0;
        //Outgoing packet which is send from us!
        private bool outgoing = false;
        //Is Packet okay, otherwise it will be deleted by the manager
        private bool invalid = true;
        //A Application looks for a id and type
        private bool isidsearch = false;
        //Number of attempts to send command
        private uint retry = 0;
        //Für den Packetmanager, damit er weiß wie oft das Paket von ihm schon gelesen wurde
        private uint counter = 20;
        //Zurückmeldung zu einem Befehl
        private string returnMessage = null;
        
        //dummy konstruktor
        public Packet()
        {

        }

        //richtiger Konstruktor
        public Packet(Dictionary<string, string> recieved_content, byte Sender_Type, byte Sender_ID)
        {
            //Wo kommt das Paket her??
            sender_id = Sender_ID;
            sender_type = Sender_Type;
            //Jemand sucht eine neue ID
            if ((sender_id == 255) && (sender_type == 255)) isidsearch = true;

            //Store original content for debug cases etc...
            Dictionary<string, string> content_copy = CloneDictionary<string, string>(recieved_content);
            original_content = CloneDictionary<string, string>(recieved_content);

            //Temporäre Variable zum Auslesen von strings aus dem Dictionary
            string value = null;

            //Zum späteren Herausfiltern doppelter Pakete
            if (content_copy.TryGetValue("packetnumber", out value))
            {
                content_copy.Remove("packetnumber");
                packetnumber = UInt64.Parse(value);
            }

            //Type und ID herausfinden
            if (content_copy.TryGetValue("id", out value))
            {
                content_copy.Remove("id");
                receiveid = Convert.ToByte(value);
            }
            else
            {
                receiveid = 0;
            }

            if (content_copy.TryGetValue("type", out value))
            {
                content_copy.Remove("type");
                receivetype = Convert.ToByte(value);
            }
            else
            {
                receivetype = 0;
            }

            //Timestamp to order commands
            if (content_copy.TryGetValue("timestamp", out value))
            {
                content_copy.Remove("timestamp");
                timestamp = value;
            }

            //Ein Sender wartet xx ms bevor er einen Fehler bei der Ausführung meldet.
            if (content_copy.TryGetValue("timeout", out value))
            {
                content_copy.Remove("timeout");
                timeout = Convert.ToInt32(value);
            }
                               

            //Ist es ein Befehl?
            if (content_copy.TryGetValue("command", out value))
            {
                command = value;
                content_copy.Remove("command");
                type = "command";

                //Paket was die ID suche betrifft, mit Schreibfehlern in unterschiedlichen Versionen der UDP Klasse
                if ((command.ToLower() == "id not available") || (command.ToLower() == "id not avialable")) isidsearch = true;

                //Is Response to command (ACK)?
                if ((command == "received") || (command == "executed") || (command == "ack"))
                {

                    //Art von Antwortpaket
                    switch (command)
                    {
                        case "received":
                            isReceiveAck = true;
                            break;
                        case "executed":
                            isExecuteAck = true;
                            break;
                        default:
                            isAck = true;
                            break;
                    }
                }
            }

            //Store all variables and the content 
            content = content_copy;

            invalid = false;
        }

        public uint ReadFromManager()
        {
            if (counter > 0) counter--;            
            return counter;
        }

        public string Type
        {
            get { return type; }
            set { type = value.ToLower(); }
        }

        public string return_message
        {
            get { return returnMessage; }
            set { returnMessage = value; }
        }

        public string Command
        {
            get { return command; }
            set { command = value; }
        }

        public string[] StringArray
        {
            get { return DictionaryToStringArray(content); }
        }

        public byte[] ByteArray
        {
            get { return bytearray; }
            set { bytearray = value; }
        }

        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        public List<int> Adapter
        {
            get { return adapter; }
            set { adapter = value; }
        }

        public Dictionary<string, string> Content
        {
            get { return content; }
            set { content = value; }
        }

        public Dictionary<string, string> OriginalContent
        {
            get { return original_content; }
            set { original_content = value; }
        }

        public UInt64 Packetnumber
        {
            get { return packetnumber; }
            set { packetnumber = value; }
        }

        public string Timestamp
        {
            get { return timestamp; }
            set { timestamp = value.ToLower(); }
        }

        public bool isIDSearch
        {
            get { return isidsearch; }
            set { isidsearch = value; }
        }

        public bool Outgoing
        {
            get { return outgoing; }
            set { outgoing = value; }
        }

        public bool CommandIsExecuted
        {
            get { return commandexecuted; }
            set { commandexecuted = value; }
        }

        public int CommandExecutedCounter
        {
            get { return commandexecutedcounter; }
            set { commandexecutedcounter = value; }
        }

        public bool CommandIsReceived
        {
            get { return commandreceived; }
            set { commandreceived = value; }
        }

        public bool isReceiveAck
        {
            get { return isreceiveack; }
            set { isreceiveack = value; }
        }

        public bool isExecuteAck
        {
            get { return isexecuteack; }
            set { isexecuteack = value; }
        }

        public bool ReceiveAckAck
        {
            get { return receiveackack; }
            set { receiveackack = value; }
        }

        public bool ExecuteAckAck
        {
            get { return executeackack; }
            set { executeackack = value; }
        }

        public bool isAck
        {
            get { return isack; }
            set { isack = value; }
        }

        public byte SenderID
        {
            get { return sender_id; }
            set { sender_id = value; }
        }

        public byte SenderType
        {
            get { return sender_type; }
            set { sender_type = value; }
        }

        public byte receiveID
        {
            get { return receiveid; }
            set { receiveid = value; }
        }

        public byte receiveType
        {
            get { return receivetype; }
            set { receivetype = value; }
        }

        public bool isStatus
        {
            get 
            { 
                if (type == "status") return true;
                return false;
            }
        }

        public bool isCommand
        {
            get
            {
                //Because ack uses also the command keyword
                if (isReceiveAck) return false;
                if (isExecuteAck) return false;
                if (isAck) return false;

                if (type == "command") return true;
                return false;
            }
        }

        public bool isServerMessage
        {
            get
            {
                if ((type == "warn") || (type == "hint") || (type == "error")) return true;
                return false;
            }
        }

        public bool isHint
        {
            get
            {
            if (type == "hint") return true;
            return false;
            }
        }

        public bool isError()
        {
            if (type == "error") return true;
            return false;
        }

        public bool isInvalid
        {
            get { return invalid; }
            set { invalid = value; }
        }

        public string[] toStringArray(bool completePacket = false)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(content);
            if (isCommand)
            {
                result.Add("command", command);
                if (timestamp != null)
                {
                    result.Add("timestamp", timestamp);
                }
            }

            if (completePacket)
            {
                //TODO: write all information into the dictionary
            }

            string[] str = new string[result.Count * 2];
            int cnt = 0;
            foreach (var pair in result)
            {
                str[cnt++] = pair.Key;
                str[cnt++] = pair.Value;
            }
            return str;
        }

        public string toString(string seperationString = "|", bool completePacket = false)
        {
            Dictionary<string, string> result = content;
            if (isCommand)
            {
                result.Add("command", command);
                if (timestamp != null)
                {
                    result.Add("timestamp", timestamp);
                }
            }

            if (completePacket)
            {
                //TODO: write all information into the dictionary
                //--> make it better
            }

            string str = "";
            foreach (var pair in result)
            {
                str = str + pair.Key + seperationString + pair.Value + seperationString;
            }

            if (completePacket)
            {
                str = sender_type.ToString() + seperationString + sender_id.ToString() + seperationString + str;
            }

            return str;
        }

        public Dictionary<K, V> CloneDictionary<K, V>(Dictionary<K, V> dict)
        {
            Dictionary<K, V> newDict = null;

            // The clone method is immune to the source dictionary being null.
            if (dict != null)
            {
                // If the key and value are value types, clone without serialization.
                if (((typeof(K).IsValueType || typeof(K) == typeof(string)) &&
                     (typeof(V).IsValueType) || typeof(V) == typeof(string)))
                {
                    newDict = new Dictionary<K, V>();
                    // Clone by copying the value types.
                    foreach (KeyValuePair<K, V> kvp in dict)
                    {
                        newDict[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    // Clone by serializing to a memory stream, then deserializing.
                    // Don't use this method if you've got a large objects, as the
                    // BinaryFormatter produces bloat, bloat, and more bloat.
                    BinaryFormatter bf = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream();
                    bf.Serialize(ms, dict);
                    ms.Position = 0;
                    newDict = (Dictionary<K, V>)bf.Deserialize(ms);
                }
            }

            return newDict;
        }

        public uint getRetryCount()
        {
            return retry;
        }

        public void incrementRetryCount()
        {
            retry++;
        }

        private string[] DictionaryToStringArray(Dictionary<string, string> Liste)
        {
            string[] str = new string[Liste.Count * 2];
            int cnt = 0;
            foreach (var pair in Liste)
            {
                str[cnt++] = pair.Key;
                str[cnt++] = pair.Value;
            }
            return str;
        }

    }

}
