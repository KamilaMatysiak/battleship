using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Battleship
{
    class Serwer
    {
        IPEndPoint ipEndpoint;

        public void SendShot(Field f)
        {
            f.Button.BackColor = Color.Pink;
            
        }

        public void ReceiveShot()
        {
            
        }

        public void ConnectToServer(string IpAdress, int PortAdress)
        {
            ipEndpoint = null;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(IpAdress).AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
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
        }

        private void firstConnect(IAsyncResult ar)
        {
           
        }

    }
}
