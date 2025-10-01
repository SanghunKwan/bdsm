using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDBms
{
    internal class DBAgent
    {
        string _dbName;
        string _userID;
        string _userPW;

        MySqlConnection _connection;

        public string _selectDBName =>_dbName;

        public string ConnectCB(string dbName, string id, string pw, string admin, int port)
        {
            _dbName = dbName;
            _userID = id;
            _userPW = pw;

            string connectMsg = string.Format("Server={0},Port={1},Database={2};Uid={3};Pwd{4};"
                                            , admin, port, _dbName, _userID, _userPW);

            try
            {
                _connection = new MySqlConnection(connectMsg);
            }
            catch (Exception ex)
            {
                return ex.ToString()    ;
            }
            return string.Empty;

        }
    }
}
