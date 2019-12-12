using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Battleship
{
    class Connection
    {
        public Connection(Game game)
        {
            _game = game;
            receiveBuffer = new byte[8];
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
        char Id;

        public void SendShot(Field f)
        {
            byte[] sendBuffer = MakeMessage("1",Convert.ToChar('0' + f.wier).ToString() + Convert.ToChar('0' + f.kol).ToString());
            int sended = 0;
            while (sendBuffer.Length != sended)
            {
                sended += clientSocket.SendTo(sendBuffer, 0 + sended, sendBuffer.Length - sended, SocketFlags.None, ipEndpoint);
            }
            GetAnswer(f);
        }

        public void GetAnswer(Field f)
        {
            receiveBuffer = new byte[8];
            int received = 0;
            while (received < 7)
            {
                received += clientSocket.ReceiveFrom(receiveBuffer, 0 + received, receiveBuffer.Length - received, SocketFlags.None, ref ipFeedBack);
            }
            string Message = Encoding.ASCII.GetString(receiveBuffer);
            if (Message.Substring(2,3) == "999")
            {
                _game.ChangeGameStage(20);
                receiveBig();
            }
            if (Message[0] == '1' || Id != Message[1])
                return;
            _game.ShotResult(f, Message[4]);
            _game.ChangeGameStage(11);
            startReceiving();
        }
        public void GetAnswer()
        {
            receiveBuffer = new byte[8];
            int received = 0;
            while (received < 7)
            {
                received += clientSocket.ReceiveFrom(receiveBuffer, 0 + received, receiveBuffer.Length - received, SocketFlags.None, ref ipFeedBack);
            }
            string Message = Encoding.ASCII.GetString(receiveBuffer);
            if (Message.Substring(2, 3) == "999")
            {
                _game.ChangeGameStage(20);
                receiveBig();
            }
        }
        public void receiveBig()
        {
 
            string namefile = "GotExplosion.jpg";
            byte[] net_buf = new byte[10240];
            byte[] size_buf = new byte[sizeof(Int32)];
            int len = net_buf.Length;
            int received;
            int received_all = 0;
            received = clientSocket.ReceiveFrom(size_buf, 0, sizeof(Int32), SocketFlags.None, ref ipFeedBack);
            int size = BitConverter.ToInt32(size_buf, 0);
            _game.ChangeText(_game.info, size.ToString());
            if (File.Exists(namefile))
                File.Delete(namefile);
            FileStream fs = File.Create(namefile);
            while (true)
            {
                // receive 
                Array.Clear(net_buf, 0, len);
                received = this.clientSocket.ReceiveFrom(net_buf, 0, len, SocketFlags.None, ref ipFeedBack);
                received_all += received;
                if (received == 0 || received == -1)
                    break;
                fs.Write(net_buf, 0, received);
                if (received_all == size) break;
                // process 
            }
            _game.ChangeText(_game.info, received_all.ToString());
            fs.Close();
        }
        public void ReceiveGuestShot()
        {
            while (true)
            {
                clientSocket.ReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref ipFeedBack);
                if (!GuestHandle(Encoding.ASCII.GetString(receiveBuffer))) break;
            }
            return;
        }
        
        private bool GuestHandle(string Message)
        {
            //Socket clientSocket = (Socket)asyncSend.AsyncState;
            if (Message == "0099900") return false;
            if (Message[0] == '1' || Message[1] != Id)
                return true;
            if (Message[2] == '1')
            {
                _game.ShotResult(_game.GetEnemyField(Convert.ToInt16(Message[5] - '0'), Convert.ToInt16(Message[6] - '0')), Message[4]);
            }
            else if (Message[2] == '0')
                _game.ShotResult(_game.GetField(Convert.ToInt16(Message[5] - '0'), Convert.ToInt16(Message[6] - '0')), Message[4]);
            return true;

        }
        public bool ConnectToServer(string IPstring, int PortAdress, bool isPlayer )
        {
            ipEndpoint = null;
            connectionError = false;
            connectionFinished = false;
            try
            {
                foreach (IPAddress ipAddress in Dns.GetHostEntry(IPstring).AddressList)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        this.ipEndpoint =
                            new IPEndPoint(ipAddress, PortAdress);
                        break;
                    }
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
            }


            if (ipEndpoint == null) return false;
            ipFeedBack = new IPEndPoint(ipEndpoint.Address,ipEndpoint.Port);
            clientSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);
            byte[] sendBuffer = Encoding.ASCII.GetBytes("0010000");
            if (isPlayer)
                sendBuffer = Encoding.ASCII.GetBytes("0000000");

            IAsyncResult AskForID = clientSocket.BeginSendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, ipEndpoint, new AsyncCallback(IdAsked), clientSocket);
            return true;
        }

        private void IdAsked(IAsyncResult ar)
        {
            //Socket clientSocket = (Socket)ar.AsyncState;
            int bytessent = clientSocket.EndSendTo(ar);
            if (!(bytessent == 8)) connectionError = true;
            // pierwsze trzy cyfry to rodzaj prosby, czwarta to fakt czy chcesz zostac graczem czy gosciem
            try
            {
                IAsyncResult IDRec = clientSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref ipFeedBack, new AsyncCallback(IdReturned), clientSocket);
            }
            catch (System.Net.Sockets.SocketException)
            {
                _game.ChangeText(_game.info, "Nie polaczyles sie z serwerem, sprobuj ponownie.");
                return;
            }
        }

        private void IdReturned(IAsyncResult asyncSend)
        {
            //Socket clientSocket = (Socket)asyncSend.AsyncState;
            int bytesSent = clientSocket.EndReceiveFrom(asyncSend, ref ipFeedBack);
            if (!(bytesSent == 8)) connectionError = true;
            else
            {
                string Message = Encoding.ASCII.GetString(receiveBuffer);
                if (Message[0] == '1')
                    return;
                haveId = true;
                connectionFinished = true;
                Id = Message[2];
                if (Id < '2') _game.ChangeGameStage(1);
                else
                {
                    _game.ChangeGameStage(-1);
                    ReceiveGuestShot();
                    _game.ChangeGameStage(20);
                }

            }


        }
        public void GameBegin()
        {
            if (Id == '0')
            {
                _game.ChangeGameStage(10);
            }
            else if (Id == '1')
            {
                _game.ChangeGameStage(11);
                startReceiving();
            }
        }
        public void startReceiving()
        {
            connectionError = false;
            haveShot = false;
            IAsyncResult shotRec = clientSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref ipFeedBack, new AsyncCallback(shotReceived), clientSocket);
        }
        public void sendGameEnd()
        {
            byte[] sendBuffer;
            sendBuffer = Encoding.ASCII.GetBytes("0099900");
            int sended = 0;
            while (sendBuffer.Length != sended)
            {
                sended += clientSocket.SendTo(sendBuffer, sended + 0, sendBuffer.Length - sended, SocketFlags.None, ipEndpoint);
            }
            GetAnswer();
        }
        private void shotReceived(IAsyncResult asyncSend)
        {
            //Socket clientSocket = (Socket)asyncSend.AsyncState;
            //01 23 45 67 89 10/11 12/13 14/15 // 0 1 234 56
            int bytesSent = clientSocket.EndReceiveFrom(asyncSend, ref ipFeedBack);
            byte[] sendBuffer;
            if (!(bytesSent == 8)) connectionError = true;
            else
            {
                haveShot = true;
                var b = Convert.ToInt16('0');
                char response=_game.ReceiveShot(Convert.ToInt16(receiveBuffer[5] - '0'), Convert.ToInt16(receiveBuffer[6]-'0'));
                if (response=='9')
                {
                    sendGameEnd();
                    return;
                }
                else
                {
                    sendBuffer = MakeMessage("2", response.ToString() + (receiveBuffer[5] - '0').ToString() + (receiveBuffer[6] - '0').ToString());
                }
                
                int sended = 0;
                while (sendBuffer.Length != sended)
                {
                    sended+=clientSocket.SendTo(sendBuffer, sended + 0, sendBuffer.Length - sended, SocketFlags.None, ipEndpoint);
                }
                _game.ChangeGameStage(10);
            }
        }
        private byte[] MakeMessage(string type, string arg)
        {
            string MId = "0" + Id; // [0,1]
            string Mtype = type.PadLeft(2, '0'); // [2,3]
            string Marg = arg.PadLeft(3, '0'); // [4,5,6]
            string result = MId + Mtype + Marg;
            return Encoding.ASCII.GetBytes(result);
        }
        

    }
}
