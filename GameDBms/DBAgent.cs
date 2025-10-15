using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDBms
{
    public enum QueryType
    {
        Insert,
        Update,
        SelectID,
        SelectPw,
        Delete
    }


    internal class DBAgent
    {
        string _dbName;
        string _userID;
        string _userPW;

        MySqlConnection _connection;

        public string _selectDBName => _dbName;

        public string ConnectCB(string dbName, string id, string pw, string admin, int port)
        {
            _dbName = dbName;
            _userID = id;
            _userPW = pw;

            string connectMsg = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4};"
                                            , admin, port, _dbName, _userID, _userPW);

            try
            {
                _connection = new MySqlConnection(connectMsg);
                _connection.Open();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return string.Empty;

        }

        public string MakeQuery(string tableName, QueryType qType, params string[] param)
        {
            string query = string.Empty;
            switch (qType)
            {
                case QueryType.Insert:
                    query = string.Format("INSERT INTO `gamedb`.`{0}` (`UUID`, `AccountID`, `AccountPW`, `UserName`, `ClearStage`, `IngameGold`) VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}');", tableName, param[0], param[1], param[2], param[3], param[4], param[5]);
                    Console.WriteLine(query);
                    break;
                case QueryType.Update:
                    break;
                case QueryType.SelectID:
                    query = string.Format("SELECT * FROM `gamedb`.`{0}` WHERE `AccountID` = '{1}' LIMIT 1", tableName, param[0]);
                    break;
                case QueryType.SelectPw:
                    query = string.Format("SELECT * FROM `gamedb`.`{0}` WHERE (`AccountID`, `AccountPW`) = ('{1}','{2}') LIMIT 1", tableName, param[0], param[1]);
                    break;
                case QueryType.Delete:
                    //"DELETE FROM `gamedb`.`userinfodata` WHERE (`UUID` = '00000000000000000001') and (`AccountID` = '2');"
                    break;
            }

            return query;
        }

        public object SendQueryExcuteScalar(in string query)
        {
            MySqlCommand command = new MySqlCommand(query, _connection);

            return command.ExecuteScalar();
        }
        public int SendQueryExcuteNoQuery(in string query)
        {
            MySqlCommand command = new MySqlCommand(query, _connection);

            return command.ExecuteNonQuery();
        }
        public object SendQueryExcuteRead(in string query)
        {
            MySqlCommand command = new MySqlCommand(query, _connection);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                    return reader["AccountPW"];
                else
                    return null;
            }
        }

    }
}
