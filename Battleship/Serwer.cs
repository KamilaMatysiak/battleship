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
    class Connection
    {
        public Connection(Game game)
        {
            _game = game;
            receiveBuffer = new byte[16];
            connectionError = false;
            haveId = false;
        }

        Game _game;
        IPEndPoint ipEndpoint;
        EndPoint ipFeedBack;
        Socket clientSocket;
        private byte[] receiveBuffer;
        public bool connectionError;
        public bool connectionFinished;
        public bool haveId;
        public bool haveShot;
        string Id;

        public void SendShot(Field f)
        {
            f.Button.BackColor = Color.Pink;

        }

        public void ReceiveShot(Field f)
        {

        }

        public bool ConnectToServer(string IPstring, int PortAdress)
        {
            ipEndpoint = null;
            connectionError = false;
            connectionFinished = false;
            foreach (IPAddress ipAddress in Dns.GetHostEntry(IPstring).AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.ipEndpoint =
                        new IPEndPoint(ipAddress, PortAdress);
                    break;
                }
            }
            if (ipEndpoint == null) return false;
            ipFeedBack = new IPEndPoint(ipEndpoint.Address,ipEndpoint.Port);
            clientSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);

            byte[] sendBuffer = Encoding.ASCII.GetBytes("00000000");

            IAsyncResult AskForID = clientSocket.BeginSendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ipEndpoint, new AsyncCallback(IdAsked), clientSocket);
            return true;
        }

        private void IdAsked(IAsyncResult ar)
        {
            //Socket clientSocket = (Socket)ar.AsyncState;
            int bytessent = clientSocket.EndSendTo(ar);
            if (!(bytessent == 8)) connectionError = true;
            // pierwsze trzy cyfry to rodzaj prosby, czwarta to fakt czy chcesz zostac graczem czy gosciem

            IAsyncResult IDRec = clientSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref ipFeedBack, new AsyncCallback(IdReturned), clientSocket);
        }

        private void IdReturned(IAsyncResult asyncSend)
        {
            //Socket clientSocket = (Socket)asyncSend.AsyncState;
            int bytesSent = clientSocket.EndReceiveFrom(asyncSend, ref ipFeedBack);
            if (!(bytesSent == Encoding.ASCII.GetBytes("00000000").Length)) connectionError = true;
            else
            {
                haveId = true;
                connectionFinished = false;
                Id = Encoding.ASCII.GetString(receiveBuffer).Substring(0, 3);

            }


        }
        public void startReceiving()
        {
            connectionError = false;
            haveShot = false;
            IAsyncResult shotRec = clientSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref ipFeedBack, new AsyncCallback(shotReceived), clientSocket);
        }

        private void shotReceived(IAsyncResult asyncSend)
        {
            //Socket clientSocket = (Socket)asyncSend.AsyncState;
            //01 23 45 67 89 10/11 12/13 14/15 
            int bytesSent = clientSocket.EndReceiveFrom(asyncSend, ref ipFeedBack);
            if (!(bytesSent == Encoding.ASCII.GetBytes("00000000").Length)) connectionError = true;
            else
            {
                haveShot = true;
                string response=_game.ReceiveShot(BitConverter.ToInt16(receiveBuffer, 12), BitConverter.ToInt16(receiveBuffer, 14));
                byte[] sendBuffer = MakeMessage("1",response);
                int sended = 0;
                while (sendBuffer.Length != sended)
                {
                    sended+=clientSocket.SendTo(sendBuffer, sended, sendBuffer.Length - sended, SocketFlags.None, ipEndpoint);
                }
            }
        }
        private byte[] MakeMessage(string type, string arg)
        {
            string MId = Id.PadLeft(3, '0');
            string Mtype = type.PadLeft(3, '0');
            string Marg = arg.PadLeft(2, '0');
            return Encoding.ASCII.GetBytes(MId + Mtype + Marg);
        }
    }
}
