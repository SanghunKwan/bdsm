using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace GameDBms
{
    internal class NetManager
    {
        const short _port = 789;
        Socket _waitSocket;
        Socket _connectServer;
        bool _isQuit = false;
        Thread _sendThread;
        Thread _receiveThread;

        Queue<Packet> _sendQueue;
        Queue<Packet> _receiveQueue;

        public NetManager()
        {
            _waitSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _waitSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _waitSocket.Listen(10);

            _sendThread = new Thread(SendLoop);
            _receiveThread = new Thread(ReceiveLoop);
        }

        public void InitNetwork()
        {
            _sendQueue = new Queue<Packet>();
            _receiveQueue = new Queue<Packet>();

            //쓰레드 가동.
            _sendThread.Start();
            _receiveThread.Start();
        }

        public bool MainLoop()
        {
            if (_waitSocket.Poll(0, SelectMode.SelectRead))
            {
                _connectServer = _waitSocket.Accept();

                Packet send = new Packet();
                //내용 저장

                _sendQueue.Enqueue(send);

            }

            if (_connectServer != null && _connectServer.Poll(0, SelectMode.SelectRead))
            {
                try
                {
                    //byte 배열에서 Packet으로 변환.
                    Packet receive = new Packet();
                    _receiveQueue.Enqueue(receive);
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
                    byte[] data = new byte[1024];//임시 사이즈.

                    _connectServer.Send(data);
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

                    //receive protocol에 따라 처리.
                }
            }
        }
    }
}
