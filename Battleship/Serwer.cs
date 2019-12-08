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
        public Serwer()
        {
            receiveBuffer = new byte[8];
            connectionError = false;
            haveId = false;
        }


        IPEndPoint ipEndpoint;
        EndPoint ipFeedBack;
        Socket clientSocket;
        private byte[] receiveBuffer;
        public bool connectionError;
        public bool connectionFinished;
        public bool haveId;
        string Id;

        public void SendShot(Field f)
        {
            f.Button.BackColor = Color.Pink;
            
        }

        public void ReceiveShot()
        {
            
        }

        public bool ConnectToServer(string IpAdress, int PortAdress)
        {
            return true;
            ipEndpoint = null;
            connectionError = false;
            connectionFinished = false;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(IpAdress).AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.ipEndpoint =
                        new IPEndPoint(ipAddress, PortAdress);
                    break;
                }
            }
            if (ipEndpoint == null) return false;
            clientSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Udp);

            byte[] sendBuffer = Encoding.ASCII.GetBytes("00000011");

            IAsyncResult AskForID = clientSocket.BeginSendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ipEndpoint, new AsyncCallback(IdAsked),clientSocket);
        }

        private void IdAsked(IAsyncResult ar)
        {
            //Socket clientSocket = (Socket)ar.AsyncState;
            int bytessent = clientSocket.EndSendTo(ar);
            if (!(bytessent == 8)) connectionError = true;
            // pierwsze trzy cyfry to rodzaj prosby, czwarta to fakt czy chcesz zostac graczem czy gosciem

            IAsyncResult IDReturned = clientSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None,ref ipFeedBack, new AsyncCallback(IdReturned), clientSocket);
        }

        private void IdReturned(IAsyncResult asyncSend)
        {
            //Socket clientSocket = (Socket)asyncSend.AsyncState;
            int bytesSent = clientSocket.EndReceiveFrom(asyncSend,ref ipFeedBack);
            if (!(bytesSent == Encoding.ASCII.GetBytes("00000011").Length)) connectionError = true;
            else
            {
                haveId = true;
                connectionFinished = false;
                Id =Encoding.ASCII.GetString(receiveBuffer).Substring(0, 3);

            }

            
        }
    }
}
