using Mysqlx;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
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
        const string _userTable = "userinfodata";
        //const string _gameTable = "gameTableData";
        DBAgent _agent;

        Socket _waitSocket;
        Socket _connectServer;
        bool _isQuit = false;
        Thread _sendThread;
        Thread _receiveThread;

        Queue<Packet_uuid> _sendQueue;
        Queue<Packet_uuid> _receiveQueue;

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

            _sendQueue = new Queue<Packet_uuid>();
            _receiveQueue = new Queue<Packet_uuid>();

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
                //서버에 접속이 되었다고 알려야 함.
                Console.WriteLine("서버 연결");
                Packet_uuid pack;
                pack._protocol = (uint)DBProtocol.Send.DBConnect_Success;
                pack._totalSize = 0;
                pack._data = new byte[1008];
                pack._uuid = 10000000000000000;
                SendQueueIn(pack);
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
                        Packet_uuid receive = (Packet_uuid)ConverterPack.ByteArrayToStructure(buffer, typeof(Packet_uuid), receiveLength);
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
                    Packet_uuid send = _sendQueue.Dequeue();
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
                    Packet_uuid pack = _receiveQueue.Dequeue();

                    switch ((DBProtocol.Receive)pack._protocol)
                    {
                        case DBProtocol.Receive.Join_User:
                            ReceiveJoin(pack);
                            break;

                        case DBProtocol.Receive.Login_User:
                            ReceiveLogin(pack);
                            break;

                        case DBProtocol.Receive.CheckId_User:
                            ReceiveCheckId(pack);
                            break;
                    }
                    //receive protocol에 따라 처리.
                }
            }
        }
        public void SendQueueIn(Packet_uuid pack)
        {
            if (pack._protocol < (uint)DBProtocol.Send.End)
                _sendQueue.Enqueue(pack);
            else
                Console.WriteLine("알 수 없는 프로토콜이 감지되었습니다.(번호[{0}])", pack._protocol);
        }
        public bool CheckJoin(string table, string userID, out uint error)
        {
            //1. id 유효성 검사
            if (!IsIdValid(userID, out error))
                return false;

            if(HasID(table, userID))
            {
                error = 3;
                return false;
            }

            return true;
        }
        public bool HasID(in string table, in string userID)
        {
            string queryDlg = _agent.MakeQuery(table, QueryType.SelectID, userID);

            return _agent.SendQueryExcuteScalar(queryDlg) != null;
        }
        public bool HasID(in string table, in string userID, in string userPwd)
        {
            string queryDlg = _agent.MakeQuery(table, QueryType.SelectPw, userID, userPwd);

            return _agent.SendQueryExcuteScalar(queryDlg) != null;
        }
        public bool IsIdValid(in string userID, out uint error)
        {
            if (userID.Length <= 0 || userID.Length > 10)
            {
                error = 1;
                return false;
            }

            for (int i = 0; i < userID.Length; i++)
            {
                if (!char.IsLetterOrDigit(userID[i]))
                {
                    error = 2;
                    return false;
                }
            }

            error = 0;
            return true;
        }

        public bool CheckLogin(in string table, in string userID, in string userPwd, out uint error)
        {
            if(!IsIdValid(userID, out error))
                return false;

            if(!HasID(table, userID, userPwd))
            {
                error = 3;
                return false;
            }

            return true;
        }


        #region [리시브 처리 함수]
        void ReceiveJoin(Packet_uuid receive)
        {
            Console.WriteLine(" Receive.Join_User 신호가 들어왔습니다.");
            Packet_Join packJoin = (Packet_Join)ConverterPack.ByteArrayToStructure(receive._data, typeof(Packet_Join), (int)receive._totalSize);
            Packet_uuid send;
            send._data = new byte[1008];
            if (CheckJoin(_userTable, packJoin._id, out uint error))
            {
                //검사 성공. 고유 id 생성 및 db에 저장.
                string query = _agent.MakeQuery(_userTable, QueryType.Insert, "1000000", packJoin._id, packJoin._pw, packJoin._name, packJoin._clearStage.ToString(), packJoin._gold.ToString());
                _agent.SendQueryExcuteNoQuery(query);
                send._protocol = (uint)DBProtocol.Send.Join_Success;
                send._totalSize = 0;
                send._uuid = 10000000000000000;
            }
            else
            {
                //검사 실패. failed. (1. 글자수 2. 쓸 수 없는 문자. 3. 중복)
                Packet_Std_Failed failed;
                failed._errorCord = error;
                byte[] datas = ConverterPack.StructureToByteArray(failed);
                send._protocol = (uint)DBProtocol.Send.Join_Failed;
                send._totalSize = (uint)datas.Length;
                Array.Copy(datas, send._data, datas.Length);
                send._uuid = 10000000000000000;
            }
            //string queryDlg = _agent.
            //서버로 회답.
            SendQueueIn(send);
        }
        void ReceiveLogin(Packet_uuid receive)
        {
            Console.WriteLine("Receive.Login_User 신호가 들어왔습니다.");
            Packet_Login packLogin = (Packet_Login)ConverterPack.ByteArrayToStructure(receive._data, typeof(Packet_Login), (int)receive._totalSize);
            Console.WriteLine("id:{0}, pw:{1}", packLogin._id, packLogin._pw);
            Packet_uuid send;
            send._data = new byte[1008];
            if (CheckLogin(_userTable, packLogin._id, packLogin._pw, out uint error))
            {

                Packet_LoginData data;
                data._name = "asdf";
                data._clearStage = 1;
                data._gold = 1000;
                byte[] bytes = ConverterPack.StructureToByteArray(data);

                send._protocol = (uint)DBProtocol.Send.Login_Success;
                send._totalSize = (uint)bytes.Length;
                Array.Copy(bytes, send._data, bytes.Length);
                send._uuid = 10000000000000000;
            }
            else
            {
                Packet_Std_Failed failed;
                failed._errorCord = error;
                byte[] datas = ConverterPack.StructureToByteArray(failed);

                send._protocol = (uint)DBProtocol.Send.Login_Failed;
                send._totalSize = (uint)datas.Length;
                Array.Copy(datas, send._data, datas.Length);
                send._uuid = 10000000000000000;
            }

            
            SendQueueIn(send);
        }
        void ReceiveCheckId(Packet_uuid receive)
        {
            Console.WriteLine("Receive.CheckId_User 신호가 들어왔습니다.");
            Packet_DuplicationId packLogin = (Packet_DuplicationId)ConverterPack.ByteArrayToStructure(receive._data, typeof(Packet_DuplicationId), (int)receive._totalSize);
            Console.WriteLine("id:{0}", packLogin._id);
            Packet_uuid send;
            send._data = new byte[1008];
            if (HasID(_userTable, packLogin._id))
            {
                send._protocol = (uint)DBProtocol.Send.CheckId_Success;
                send._totalSize = 0;
                send._uuid = 10000000000000000;
            }
            else
            {
                send._protocol = (uint)DBProtocol.Send.CheckId_Failed;
                send._totalSize = 0;
                send._uuid = 10000000000000000;
            }

            SendQueueIn(send);
        }

        #endregion [리시브 처리 함수]
    }
}
