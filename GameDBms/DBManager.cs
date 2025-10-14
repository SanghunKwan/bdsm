using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Threading;
using tGameServer.NetworkDefine;

namespace GameDBms
{

    public enum Result_Connect
    {
        Success = 0,
        Failed,
        Already
    }

    internal class DBManager
    {
        const string _adminName = "localHost";
        const int _portNumber = 3306;
        long _stdUUID = 10000000000000000;

        DBAgent _agentDB;
        CommandManager _cmdMng;
        NetManager _netMng;

        Thread _netMain;

        public DBManager()
        {
            _cmdMng = new CommandManager();
            _netMng = new NetManager();

            
        }


        public Result_Connect ConnectDB(string id, string pw, string dbName)
        {
            if (_agentDB == null)
            {
                _agentDB = new DBAgent();

                string result = _agentDB.ConnectCB(dbName, id, pw, _adminName, _portNumber);
                if (result.Length != 0)
                {
                    Console.WriteLine("오류 : {0}", result);
                    return Result_Connect.Failed;
                }

                _netMng.InitNetwork(_agentDB);
                _netMain = new Thread(_netMng.MainLoop);
            }
            else
            {
                Console.WriteLine("이미 같은 DB에 접속해 있습니다.");
                return Result_Connect.Already;
            }

            _netMain.Start();
            

            return Result_Connect.Success;
        }

        public void Run()
        {
            bool _isQuit = false;
            while (!_isQuit)
            {
                int number = _cmdMng.GetSelectNumber("1. DB연결\n2. 종료\n항목을 선택하세요 :", 2);
                if (number == 1)
                {
                    string account = _cmdMng.GetSelectText("계정 이름을 입력하세요 : ");
                    string pass = _cmdMng.GetSelectText("비밀 번호를 입력하세요 : ");
                    string dbName = _cmdMng.GetSelectText("DB 이름을 입력하세요 : ");

                    Result_Connect result = ConnectDB(account, pass, dbName);
                    if (result == Result_Connect.Failed || result == Result_Connect.Already)
                        _isQuit = true;
                    else if (result == Result_Connect.Success)
                    {
                        Console.Clear();
                        _netMng._IsEnd = _isQuit = ConnectLoop();
                    }
                }
                else
                {
                    //종료
                    _netMng._IsEnd = _isQuit = true;
                }
            }
        }
        bool ConnectLoop()
        {
            bool isResult = true;
            bool isExit = false;
            while (!isExit)
            {
                int number = _cmdMng.GetSelectNumber("1. 테이블 생성\n2. 속성 추가 및 변경\n3. 테이블 삭제\n4. 이전으로\n5. 처음으로\n항목을 선택하세요 :", 5);

                switch (number)
                {
                    case 1://생성
                        isExit = CreateTableLoop();
                        break;
                    case 2://추가 및 변경
                        isExit = AddNEditLoop();
                        break;
                    case 3://삭제
                        DestroyTableLoop();
                        break;
                    case 4:
                        isExit = true;
                        isResult = false;
                        break;
                    case 5:
                        isExit = true;
                        break;
                }
            }

            return isResult;
        }
        bool CreateTableLoop()
        {
            string name = _cmdMng.GetSelectText("생성할 테이블 이름을 입력하세요(처음으로 f) : ", "f");
            if (name.Length == 0)
            {
                Console.Clear();
                return true;
            }
            //테이블 생성

            return false;
        }
        bool AddNEditLoop()
        {
            string name = _cmdMng.GetSelectText("수정 또는 변경할 테이블 이름을 입력하세요(처음으로 f) : ", "f");
            if (name.Length == 0)
            {
                Console.Clear();
                return true;
            }

            bool isResult = false;
            //지정한 테이블이 있는지 확인.
            Console.WriteLine("{0}.{1} 테이블을 지정하셨습니다.", _agentDB._selectDBName, name);
            int select = _cmdMng.GetSelectNumber("1. 추가\n 2. 수정\n3. 처음으로\n진행할 작업을 선택하세요 : ", 3);
            switch (select)
            {
                case 1://추가
                    InsertRow(_agentDB._selectDBName, name);
                    break;
                case 2://수정
                    break;
                case 3:
                    Console.Clear();
                    isResult = true;
                    break;
            }
            return isResult;
        }
        bool DestroyTableLoop()
        {
            string name = _cmdMng.GetSelectText("삭제할 테이블 이름을 입력하세요(처음으로 f) : ", "f");
            if (name.Length == 0)
            {
                Console.Clear();
                return true;
            }

            //테이블 삭제


            return false;
        }

        void InsertRow(in string dbName, in string tableName)
        {
            //_agentDB._selectDBName
            //"INSERT INTO `gamedb`.`userinfodata` (`UUID`, `AccountID`, `AccountPW`, `UserName`, `ClearStage`, `IngameGold`) VALUES ('1', '2', '3', '4', '5', '6');"
        }
    }
}
