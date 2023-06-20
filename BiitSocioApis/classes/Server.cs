using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace BiitSocioApis.classes
{
    public class Serverr
    {
        public string buzzerPressedBy = "-1";
        public List<CLientss> players = new List<CLientss>();


        private Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local IP address and port
        private IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 5000);

        private void bindd()
        {
            try
            {
                listener.Bind(localEndPoint);
            }
            catch (Exception ex) { }
        }
        string getConnectedList()
        {
            string data = "";
            try
            {
                foreach (var item in players)
                {
                    data += item.id + "~";
                }
            }
            catch (Exception ex) { }
            return data;
        }
        public void StartListening()
        {
            try
            {
                bindd();
                listener.Listen(2);
                players.Clear();
                buzzerPressedBy = "-1";
                bool teamsDone = true;
                for (int i = 0; true; i++)
                {
                    // Start listening for incoming connections

                    // Accept an incoming connection
                    Socket client = listener.Accept();

                    new Thread(() => { handleClient(client); }).Start();

                }
                //MessageBox.Show("ended");
            }
            catch (Exception ex) { }
        }
        void handleClient(Socket socket)
        {
            try
            {
                CLientss c = new CLientss();
                c.socket = socket;
                byte[] data = new byte[1024];
                int bytesReceived = socket.Receive(data);
                string message = Encoding.ASCII.GetString(data, 0, bytesReceived);
                string[] dataa = message.Split(':');
                c.id = dataa[0];
                c.name = dataa[1];
                players.Add(c);
                Console.WriteLine(message + " is conncted ,Total are " + players.Count);
                new Thread(() =>
                {
                    new Thread(() => propagateMessage(getConnectedList(), c.socket, "-1")).Start();
                    startListening(players[players.Count - 1].socket, players.Count - 1);
                }).Start();

            }
            catch (Exception ex)
            {
            }
        }
        /* public static void setInitial(string tosend)
         {
             try
             {

                 //players[1].Send(Encoding.ASCII.GetBytes(tosend));
                 //players[0].Send(Encoding.ASCII.GetBytes(tosend));
                 if (players.Count == 2)
                 {

                     Thread t = new Thread(() => { players[0].Send(Encoding.ASCII.GetBytes(tosend)); });
                     Thread t1 = new Thread(() => { players[1].Send(Encoding.ASCII.GetBytes(tosend)); });
                     t.Start(); t1.Start();
                     t.Join(); t1.Join();
                     if (tosend == "Completed")
                     {
                         if (players.Count == 2)
                         {
                             players[0].Close();
                             players[1].Close();
                             players.Clear();
                         }
                     }
                 }
             }
             catch (Exception ex) { }
         }*/

        void propagateMessage(string message, Socket to, string from)
        {
            try
            {
                if (from != "-1" && from != "-2" && to != null)
                {
                    message = "-0~" + from + "~" + message;
                    Console.WriteLine(message);
                    new Thread(() => to.Send(Encoding.ASCII.GetBytes(message))).Start();
                    return;
                }
                else
                {
                    lock (players)
                    {
                        message = from + "~" + message;
                        var data = Encoding.ASCII.GetBytes(message);
                        foreach (var item in players)
                        {
                            try
                            {
                                new Thread(() => { item.socket.Send(data); }).Start();
                            }
                            catch (Exception ex) { }
                        }
                    }

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }
        void startListening(Socket client, int index)
        {
            try
            {
                while (client.Connected)
                {

                    byte[] data = new byte[1024];
                    int bytesReceived = client.Receive(data);
                    string message = Encoding.ASCII.GetString(data, 0, bytesReceived);
                    // Console.WriteLine(message);
                    try
                    {
                        string[] data2 = message.Split('~');

                        //byte[] data1 = Encoding.ASCII.GetBytes(data2[2]);
                        string id = data2[1];
                        new Thread(() => { propagateMessage(data2[2], players.Where(s => s.id == id).Select(s => s.socket).FirstOrDefault(), data2[0]); }).Start();
                        //BuzzerRoundCompleted= true;

                    }
                    catch (Exception ex) { }


                }
                string idd = players[index].id;


                players.RemoveAt(index);
                Console.WriteLine(idd + " is Disconneted ,Remaining are " + players.Count);

                propagateMessage(idd, null, "-2");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                try
                {
                    string id = players[index].id;


                    players.RemoveAt(index);
                    Console.WriteLine(id + " is Disconneted ,Remaining are " + players.Count);
                    propagateMessage(id, null, "-2");
                }
                catch (Exception ecx)
                {
                    Console.WriteLine(ecx.Message);
                }
            }
        }
        public void getIP()
        {
            try
            {
                var wifiInterface = NetworkInterface.GetAllNetworkInterfaces()
         .Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && i.Name.StartsWith("Wi"))
         .FirstOrDefault();

                // Get the IPv4 address of the WiFi interface
                IPAddress ipAddress = wifiInterface.GetIPProperties().UnicastAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address)
                    .FirstOrDefault();
                Console.WriteLine("Listening at: " + ipAddress.ToString());

            }
            catch
            {
                Console.WriteLine("Try Again!");

            }

        }
    }
    public class CLientss
    {
        public string id { get; set; }
        public string name { get; set; }
        public Socket socket { get; set; }
    }
}