using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PosServer
{
    public class Message
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Msg { get; set; }
        public string Stamp { get; set; }

        public override string ToString()
        {
            return $"From: {From}\nTo: {To}\n{Msg}\nStamp: {Stamp}";
        }
    }

    public class Server
    {
        public static int PORT = 14300;
        public static int TAM = 1024;

        public static Dictionary<string, List<Message>> repo = new Dictionary<string, List<Message>>();

        public static IPAddress GetLocalIpAddress()
        {
            List<IPAddress> ipAddressList = new List<IPAddress>();
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            int t = ipHostInfo.AddressList.Length;
            string ip;
            for (int i = 0; i < t; i++)
            {
                ip = ipHostInfo.AddressList[i].ToString();
                if (ip.Contains(".") && !ip.Equals("127.0.0.1")) ipAddressList.Add(ipHostInfo.AddressList[i]);
            }
            if (ipAddressList.Count == 1)
            {
                return ipAddressList[0];
            }
            else
            {
                int i = 0;
                foreach (IPAddress ipa in ipAddressList)
                {
                    Console.WriteLine($"[{i++}]: {ipa}");
                }
                System.Console.Write($"Opción [0-{t - ipAddressList.Count}]: ");
                string s = Console.ReadLine();
                if (Int32.TryParse(s, out int j))
                {
                    if ((j >= 0) && (j <= t))
                    {
                        return ipAddressList[j];
                    }
                }
                return null;
            }
        }

        public static void StartListening()
        {
            byte[] bytes = new Byte[TAM];

            IPAddress ipAddress = GetLocalIpAddress();
            if (ipAddress == null) return;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);

            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting for a connection at {0}:{1} ...", ipAddress, PORT);
                    Socket handler = listener.Accept();

                    Message request = Receive(handler);

                    Console.WriteLine(request);//Print it

                    Message response = Process(request);

                    Send(handler, response);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void Send(Socket socket, Message message)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Message));
            Stream stream = new MemoryStream();
            serializer.Serialize(stream, message);
            byte[] byteData = ((MemoryStream)stream).ToArray();
            // string xml = Encoding.ASCII.GetString(byteData, 0, byteData.Length);
            // Console.WriteLine(xml);//Imprime el texto enviado
            int bytesSent = socket.Send(byteData);
        }

        public static Message Receive(Socket socket)
        {
            byte[] bytes = new byte[TAM];
            int bytesRec = socket.Receive(bytes);
            string xml = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            // Console.WriteLine(xml);//Imprime el texto recibido
            byte[] byteArray = Encoding.ASCII.GetBytes(xml);
            MemoryStream stream = new MemoryStream(byteArray);
            Message response = (Message)new XmlSerializer(typeof(Message)).Deserialize(stream);
            return response;
        }

        public static void AddMessage(Message message)
        {
            String clave = message.To;
            if (repo.ContainsKey(clave)) {
                repo[clave].Add(message);
            } 
            else {
                List<Message> lista = new List<Message>();
                lista.Add(message);
                repo.Add(clave, lista);
            }
            //TODO: Add Message
        }

        public static Message ListMessages(string toClient)
        {
            var i = 0;
            StringBuilder sb = new StringBuilder();

            //TODO: List Messages
            if (repo.ContainsKey(toClient)) {
                List<Message> lista = repo[toClient];
                while (i < lista.Count) {
                    sb.Append("[" + i + "] From: " + lista[i].From + "\n");
                    i++;
                }
                
            }

            return new Message { From = "0", To = toClient, Msg = sb.ToString(), Stamp = "Server" };
        }

        public static Message RetrMessage(string toClient, int index)
        {
            Message msg = new Message { From = "0", To = toClient, Msg = "NOT FOUND", Stamp = "Server" };

            //TODO: Retr Message
            if (repo.ContainsKey(toClient) && repo[toClient].Count > index) {
                List<Message> lista = repo[toClient];
                msg = lista[index];
                lista.Remove(msg);
                repo[toClient] = lista;
            }

            

            return msg;
        }

        public static Message Process(Message request)
        {
            Message response = new Message { From = "0", To = request.From, Msg = "ERROR", Stamp = "Server" };

            //TODO: Process
            String[] msg = request.Msg.Split();
            int n;

            if (request.Msg == "LIST") {
                response = ListMessages(request.From);
            }
            else if (msg[0] == "RETR" && int.TryParse(msg[1], out n)) {
                if (n >= 0)
                {
                    response = RetrMessage(request.From, n);
                }
            } 
            else if (int.Parse(request.To) > 0 && int.Parse(request.From) > 0) {
                AddMessage(request);
                response = new Message { From = "0", To = request.From, Msg = "OK", Stamp = "Server" };
            }

            return response;
        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}