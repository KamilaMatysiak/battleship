using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Battleship
{
    class Game
    {
        IPEndPoint ipEndpoint;
        public List<Field> playerFields;
        public List<Field> enemyFields;
        public int gameStage { get; set; }

        public string IpAdress
        {
            get { return ipAdress; }
            set { ipAdress = value; }
        }
        public string PortAdress
        {
            get { return portAdress; }
            set { portAdress = value; }
        }

        public Game()
        {
            gameStage = 0;
        }

        public void ConnectToServer(string IpAdress, int PortAdress)
        {
            ipEndpoint = null;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(IpAdress).AddressList)
            {
               if(ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.ipEndpoint =
                        new IPEndPoint(ipAddress, PortAdress);
                    break;
                }
            }
            if (ipEndpoint == null) return;

            Socket clientSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            IAsyncResult asyncConnect = clientSocket.BeginConnect(
              ipEndpoint, new AsyncCallback(firstConnect), clientSocket);

            Console.Write("Lacze sie.");
            /* if (writeDot(asyncConnect) == true)
             {
                 Thread.Sleep(30);
             }*/
        }

        private void firstConnect(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

    }
}