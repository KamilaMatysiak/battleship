using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

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
        public bool haveId;
        public bool haveShot;
        char Id;
        //Strzelanie, dostawanie wynikow strzalu
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
            string Message = ReadMessage(receiveBuffer, received);
            if (connectionError)
            {
                return;
            }

            _game.ShotResult(f, Message[4]);
            _game.ChangeGameStage(11);
            startReceiving();
        }
        public void GetAnswer()
        {
            try
            {
                receiveBuffer = new byte[8];
                int received = 0;
                while (received < 7)
                {
                    received += clientSocket.ReceiveFrom(receiveBuffer, 0 + received, receiveBuffer.Length - received, SocketFlags.None, ref ipFeedBack);
                }
                string Message = ReadMessage(receiveBuffer, received);
                if (connectionError)
                {
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Koniec gry :)");
            }
        }

        //Funkcja ktora czeka za asynchronicznie czeka za strzalem
        public void startReceiving()
        {
            connectionError = false;
            haveShot = false;
            IAsyncResult shotRec = clientSocket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref ipFeedBack, new AsyncCallback(shotReceived), clientSocket);
        }
        //Asynchroniczne odebranie strzalu
        private void shotReceived(IAsyncResult asyncSend)
        {
            int received = clientSocket.EndReceiveFrom(asyncSend, ref ipFeedBack);
            string Message = ReadMessage(receiveBuffer, received);
            if (connectionError)
            {
                return;
            }
            byte[] sendBuffer;
            haveShot = true;
            char response = _game.ReceiveShot(Convert.ToInt16(receiveBuffer[5] - '0'), Convert.ToInt16(receiveBuffer[6] - '0'));
            if (response == '9')
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
                sended += clientSocket.SendTo(sendBuffer, sended + 0, sendBuffer.Length - sended, SocketFlags.None, ipEndpoint);
            }
            _game.ChangeGameStage(10);
        }
        //Guest, czyli tylko i wylacznie odbieranie wynikow strzalow
        public void ReceiveGuestShot()
        {
            while (true)
                if (!GuestHandle(clientSocket.ReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ref ipFeedBack))) break;
            return;
        }

        private bool GuestHandle(int received)
        {
            string Message = ReadMessage(receiveBuffer, received);
            if (connectionError)
            {
                return false;
            }
            if (Message[2] == '1')
            {
                _game.ShotResult(_game.GetEnemyField(Convert.ToInt16(Message[5] - '0'), Convert.ToInt16(Message[6] - '0')), Message[4]);
            }
            else if (Message[2] == '0')
                _game.ShotResult(_game.GetField(Convert.ToInt16(Message[5] - '0'), Convert.ToInt16(Message[6] - '0')), Message[4]);
            return true;
        }

        //Otrzymywanie obrazka
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
            while (File.Exists(namefile))
            {
                namefile = "re" + namefile;
            }
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
            fs.Close();
            _game.ChangeText(_game.info, received_all.ToString());
            _game.exp = Image.FromFile(namefile);
            _game.ChangeGameStage(21);
        }

        //Funkcja ktora probuje sie polaczyc z serwerem oraz asynchronicznie pyta o nadanie id
        public bool ConnectToServer(string IPstring, int PortAdress, bool isPlayer )
        {
            if (haveId)
            {
                if (Id < '2') _game.ChangeGameStage(1);
                else
                {
                    _game.ChangeGameStage(-1);
                    ReceiveGuestShot();
                }
                return true;
            }


            ipEndpoint = null;
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
                _game.ChangeText(_game.info, "Nie polaczyles sie z serwerem, sprobuj ponownie.");
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
            int received = clientSocket.EndReceiveFrom(asyncSend, ref ipFeedBack);
            string Message = ReadMessage(receiveBuffer, received);
            if (connectionError)
            {
                return;
            }
            haveId = true;
            Id = Message[2];
            if (Id < '2') _game.ChangeGameStage(1);
            else
            {
                _game.ChangeGameStage(-1);
                ReceiveGuestShot();
            }
        }


        //Funkcja ktora rozpoczyna gre sieciowa, 
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
        //Cheat funkcja, sluzy do testowania
        public void sendGameEnd()
        {
            byte[] sendBuffer;
            sendBuffer = Encoding.ASCII.GetBytes("0099900");
            int sended = 0;
            while (sendBuffer.Length != sended)
                sended += clientSocket.SendTo(sendBuffer, sended + 0, sendBuffer.Length - sended, SocketFlags.None, ipEndpoint);
            GetAnswer();
        }
        private byte[] MakeMessage(string type, string arg)
        {
            string MId = "0" + Id; // [0,1]
            string Mtype = type.PadLeft(2, '0'); // [2,3]
            string Marg = arg.PadLeft(3, '0'); // [4,5,6]
            string result = MId + Mtype + Marg;
            return Encoding.ASCII.GetBytes(result);
        }
        private string ReadMessage(byte[] buffer, int bytes = 8)
        {
            connectionError = false;
            string message = "";
            if (bytes != 8)
            {
                connectionError = true;
                return message;
            }
            message = Encoding.ASCII.GetString(receiveBuffer);
            if (message[0] == '1')
            {
                connectionError = true;
                _game.ChangeGameStage(0);
                return message;
            }
            if (message.Substring(2, 3) == "999")
            {
                connectionError = true;
                _game.ChangeGameStage(20);
                receiveBig();
                return message;
            }
            if (haveId && Id!=message[1])
            {
                connectionError = true;
                return message;
            }
            return message;
        }

    }
}
