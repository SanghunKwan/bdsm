using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using tGameServer.NetworkDefine;

namespace GameDBms
{
    internal class NetManager
    {
        const short _port = 789;
        DBAgent _agent;

        Socket _waitSocket;
        Socket _connectServer;
        bool _isQuit = false;
        Thread _sendThread;
        Thread _receiveThread;

        Queue<Packet> _sendQueue;
        Queue<Packet> _receiveQueue;

        public bool _IsEnd
        {
            set => _isQuit = value;
        }

        public NetManager()
        {
            _waitSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _waitSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _waitSocket.Listen(10);

            _sendThread = new Thread(SendLoop);
            _receiveThread = new Thread(ReceiveLoop);
        }

        public void InitNetwork(DBAgent agent)
        {
            _agent = agent;

            _sendQueue = new Queue<Packet>();
            _receiveQueue = new Queue<Packet>();

            //쓰레드 가동.
            _sendThread.Start();
            _receiveThread.Start();
        }

        public void MainLoop()
        {
            while (true)
            {
                if (Process())
                    break;
            }
        }
        public bool Process()
        {
            if (_waitSocket.Poll(0, SelectMode.SelectRead))
            {
                _connectServer = _waitSocket.Accept();
                Packet send = new Packet();

                //서버에 접속이 되었다고 알려야 함.
                Packet pack = ConverterPack.CreatePack((uint)DBProtocol.Send.DBConnect_Success, 0, null);
                SendQueueIn(pack);

                _sendQueue.Enqueue(send);

            }

            if (_connectServer != null && _connectServer.Poll(0, SelectMode.SelectRead))
            {
                try
                {
                    //byte 배열에서 Packet으로 변환.
                    byte[] buffer = new byte[1024];
                    int receiveLength = _connectServer.Receive(buffer);
                    if (receiveLength > 0)
                    {
                        Packet receive = (Packet)ConverterPack.ByteArrayToStructure(buffer, typeof(Packet), receiveLength);
                        _receiveQueue.Enqueue(receive);
                    }
                    else
                    {

                    }
                    //_receiveQueue.Enqueue(receive);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("오류 : {0}", ex.ToString());
                }
            }

            return _isQuit;
        }

        void SendLoop()
        {
            while (!_isQuit)
            {
                if (_sendQueue.Count > 0)
                {
                    //send처리.
                    Packet send = _sendQueue.Dequeue();
                    //send를 byte[]로 변환.
                    byte[] pack = ConverterPack.StructureToByteArray(send);
                    _connectServer.Send(pack);
                }
            }
        }
        void ReceiveLoop()
        {
            while (!_isQuit)
            {
                if (_receiveQueue.Count > 0)
                {
                    //receive처리.
                    Packet pack = _receiveQueue.Dequeue();

                    switch ((DBProtocol.Receive)pack._protocol)
                    {
                        case DBProtocol.Receive.Join_User:
                            Console.WriteLine(" Receive.Join_User 신호가 들어왔습니다.");
                            Packet_Join packJoin = (Packet_Join)ConverterPack.ByteArrayToStructure(pack._data, typeof(Packet_Join), (int)pack._totalSize);
                            Console.WriteLine("id:{0}, pw:{1}, name:{2}, stage:{3}, gold:{4}", packJoin._id, packJoin._pw,  packJoin._name, packJoin._clearStage, packJoin._gold);
                            //서버로 회답.
                            SendQueueIn(ConverterPack.CreatePack((uint)DBProtocol.Send.Join_Success, 0, null));

                            break;

                        case DBProtocol.Receive.Login_User:
                            Console.WriteLine("Receive.Login_User 신호가 들어왔습니다.");
                            Packet_Login packLogin= (Packet_Login)ConverterPack.ByteArrayToStructure(pack._data, typeof(Packet_Login), (int)pack._totalSize);
                            Console.WriteLine("id:{0}, pw:{1}", packLogin._id, packLogin._pw);

                            Packet_LoginData data;
                            data._name = "asdf";
                            data._clearStage = 1;
                            data._gold = 1000;
                            byte[] bytes = ConverterPack.StructureToByteArray(data);
                            SendQueueIn(ConverterPack.CreatePack((uint)DBProtocol.Send.Login_Success, (uint)bytes.Length, bytes));
                            break;
                    }
                    //receive protocol에 따라 처리.
                }
            }
        }

        public void SendQueueIn(Packet pack)
        {
            if (pack._protocol < (uint)DBProtocol.Send.End)
                _sendQueue.Enqueue(pack);
            else
                Console.WriteLine("알 수 없는 프로토콜이 감지되었습니다.(번호[{0}])", pack._protocol);
        }
    }
}
