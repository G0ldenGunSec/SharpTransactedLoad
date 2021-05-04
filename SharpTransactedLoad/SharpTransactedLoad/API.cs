using System;
using System.Runtime.InteropServices;

namespace STL
{
    class API
    {
        // Native API's
        //-----------
        [DllImport("KernelBase.dll")]
        public static extern IntPtr CreateFileW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr lpSecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("KernelBase.dll")]
        public static extern uint GetFileAttributesW(IntPtr lpFileName);

        [DllImport("KernelBase.dll")]
        public static extern bool GetFileAttributesExW(IntPtr lpFileName,
            uint fInfoLevelId,
            IntPtr lpFileInformation);

        [DllImport("KernelBase.dll", SetLastError = true)]
        public static extern bool GetFileInformationByHandle(IntPtr hFile,
            IntPtr lpFileInformation);

        [DllImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteFile(IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        [System.Runtime.InteropServices.DllImport("ktmw32.dll", SetLastError = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        public extern static System.IntPtr CreateTransaction(
            IntPtr lpTransactionAttributes,
            IntPtr UOW,
            int CreateOptions,
            int IsolationLevel,
            int IsolationFlags,
            int Timeout,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] System.Text.StringBuilder Description);


        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFileTransactedW([MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr lpSecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile,
            IntPtr hTransaction,
            ref ushort pusMiniVersion,
            IntPtr nullValue);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);


        // Delegates
        //-----------
        public struct DELEGATES
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate IntPtr CreateFileW(
                [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
                UInt32 dwDesiredAccess,
                UInt32 dwShareMode,
                IntPtr lpSecurityAttributes,
                UInt32 dwCreationDisposition,
                UInt32 dwFlagsAndAttributes,
                IntPtr hTemplateFile);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate uint GetFileAttributesW(IntPtr lpFileName);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool GetFileAttributesExW(IntPtr lpFileName,
                uint fInfoLevelId,
                IntPtr lpFileInformation);

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate bool GetFileInformationByHandle(IntPtr hFile,
                IntPtr lpFileInformation);
        }
    }
}
