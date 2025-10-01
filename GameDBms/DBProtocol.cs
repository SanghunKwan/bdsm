using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDBms
{
    internal class DBProtocol
    {
        public enum Send
        {
            Send_Connect_Success,
        }
        public enum Receive
        {
            Receive_Connect_Success,
        }
    }
}
