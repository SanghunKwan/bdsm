using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDBms
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DBManager dbms = new DBManager();
            dbms.Run();
        }
    }
}
