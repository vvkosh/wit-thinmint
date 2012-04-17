using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace ThinMint_PCApp
{
    static class WinApi
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct DCB
        {
            public int DCBlength;
            public int BaudRate;
            public System.Collections.Specialized.BitVector32 bits1;
            public short wReserved;
            public short XonLim;
            public short XoffLim;
            public byte ByteSize;
            public byte Parity;
            public byte StopBits;
            public byte XonChar;
            public byte XoffChar;
            public byte ErrorChar;
            public byte EofChar;
            public byte EvtChar;
            public short wReserved1;
        }

        internal struct COMMTIMEOUTS
        {
            public uint ReadIntervalTimeout;
            public uint ReadTotalTimeoutMultiplier;
            public uint ReadTotalTimeoutConstant;
            public uint WriteTotalTimeoutMultiplier;
            public uint WriteTotalTimeoutConstant;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDP_CAPS
        {
            public short Usage;
            public short UsagePage;
            public short InputReportByteLength;
            public short OutputReportByteLength;
            public short FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public short[] Reserved;
            public short NumberLinkCollectionNodes;
            public short NumberInputButtonCaps;
            public short NumberInputValueCaps;
            public short NumberInputDataIndices;
            public short NumberOutputButtonCaps;
            public short NumberOutputValueCaps;
            public short NumberOutputDataIndices;
            public short NumberFeatureButtonCaps;
            public short NumberFeatureValueCaps;
            public short NumberFeatureDataIndices;
        }

        [DllImport("kernel32.dll")]
        internal static extern int CancelIo(
            SafeFileHandle hObject);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr CreateEvent(
            IntPtr lpEventAttributes,
            bool bManualReset,
            bool bInitialState,
            string lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetCommState(
            IntPtr hFile,
            out DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetOverlappedResult(
            SafeFileHandle hFile,
            IntPtr lpOverlapped,
            ref int lpNumberOfBytesTransferred,
            bool bWait);

        [DllImport("kernel32.dll")]
        internal static extern bool SetCommState(
            IntPtr hFile,
            ref DCB lpDCB);
        [DllImport("hid.dll")]
        internal static extern bool HidD_FlushQueue(
            SafeFileHandle HidDeviceObject);

        [DllImport("hid.dll", SetLastError = false)]
        internal static extern bool HidD_FreePreparsedData(
            IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = false)]
        internal static extern bool HidD_GetFeature(
            SafeFileHandle HidDeviceObject,
            byte[] ReportBuffer,
            int ReportBufferLength);

        [DllImport("hid.dll", SetLastError = false)]
        internal static extern bool HidD_GetPreparsedData(
            SafeFileHandle HidDeviceObject,
            out IntPtr PreparsedData);

        [DllImport("hid.dll", SetLastError = false)]
        internal static extern bool HidD_SetFeature(
            SafeFileHandle HidDeviceObject,
            byte[] ReportBuffer,
            int ReportBufferLength);

        [System.Runtime.InteropServices.DllImport("hid.dll", SetLastError = false)]
        internal static extern bool HidP_GetCaps(
            IntPtr PreparsedData,
            ref HIDP_CAPS Capabilities);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int ReadFile(
            SafeFileHandle hFile,
            byte[] lpBuffer,
            int nNumberOfBytesToRead,
            ref int lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int ReadFile(
            SafeFileHandle hFile,
            IntPtr lpBuffer,
            int nNumberOfBytesToRead,
            ref int lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        internal static extern int SetCommMask(
            IntPtr hFile,
            int dwEvtMask);

        [DllImport("kernel32.dll")]
        internal static extern int SetCommTimeouts(
            IntPtr hFile,
            ref COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll")]
        internal static extern int SetupComm(
            IntPtr hFile,
            int dwInQueue,
            int dwOutQueue);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool WaitCommEvent(
            IntPtr hFile,
            IntPtr lpEvtMask,
            System.Threading.NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll")]
        internal static extern int WaitForSingleObject(
            IntPtr hHandle,
            Int32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int WriteFile(
            SafeFileHandle hFile,
            byte[] lpBuffer,
            int nNumberOfBytesToWrite,
            ref int lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        internal const int FILE_FLAG_NORMAL = 0x80;
        internal const int FILE_FLAG_OVERLAPPED = 0x40000000;

        internal const int INVALID_HANDLE_VALUE = -1;

        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;

        internal const int FILE_SHARE_READ = 1;
        internal const int FILE_SHARE_WRITE = 2;

        internal const int OPEN_EXISTING = 3;

        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_ACCESS_DENIED = 5;
        internal const int ERROR_INVALID_HANDLE = 6;
        internal const int ERROR_IO_PENDING = 997;

        internal const int EV_RXCHAR = 0x0001;

        internal const int WAIT_OBJECT_0 = 0x0;
        internal const int WAIT_TIMEOUT = 0X102;
    }
}
