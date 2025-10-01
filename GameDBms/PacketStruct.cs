using System;
using System.Runtime.InteropServices;

namespace GameDBms
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Packet
    {
        [MarshalAs(UnmanagedType.U4)]
        public int _protocol;
        [MarshalAs(UnmanagedType.U4)]
        public int _totalSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1016)]
        public byte[] _data;
    }
}
