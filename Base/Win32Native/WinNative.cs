using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalcLang.Interpreter;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;

namespace Base
{
    /*
     * Some Methods to try to clone part of the .Net Framework.
     * This should just be prove of concept
     */
    public static class WinNative
    {
        private static class NativeMethods
        {
            #region Dll imports
            [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "SetFilePointer")]
            public static extern unsafe int SetFilePointer(SafeFileHandle handle, int lo, int* hi, int origin);

            [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "SetThreadErrorMode")]
            public static extern bool SetErrorMode(int newMode, out int oldMode);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
            public static extern SafeFileHandle CreateFile(String lpFileName,
                        int dwDesiredAccess, System.IO.FileShare dwShareMode,
                        SECURITY_ATTRIBUTES securityAttrs, System.IO.FileMode dwCreationDisposition,
                        int dwFlagsAndAttributes, IntPtr hTemplateFile);

            [DllImport("kernel32.dll")]
            public static extern int GetFileType(SafeFileHandle handle);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern unsafe int WriteFile(SafeFileHandle handle, byte* bytes, int numBytesToWrite, out int bytesWritten, NativeOverlapped* mustBeZero);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern unsafe int ReadFile(SafeFileHandle handle, byte* bytes, int numBytesToRead, out int numBytesRead, NativeOverlapped* mustBeZero);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern uint GetConsoleOutputCP();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern uint GetConsoleCP();
            #endregion
        }

        [CalcCallableMethod("Win32Native_SetErrorMode", 1)]
        public static object SetErrorMode(ScriptThread thread, object instance, object[] parameters)
        {
            object ret = null;

            int newMode = Convert.ToInt32(parameters[0]);
            int oldMode;

            NativeMethods.SetErrorMode(newMode, out oldMode);

            ret = (Decimal)oldMode;

            return ret;
        }

        [CalcCallableMethod("Win32Native_GetLastWin32Error", 0)]
        public static object GetLastWin32Error(ScriptThread thread, object instance, object[] parameters) => Marshal.GetLastWin32Error();

        [CalcCallableMethod("Win32Native_CreateFile", 7, "fileName,dwDesiredAccess,dwShareMode,securityAttrs,dwCreationDisposition,dwFlagsAndAttributes,hTemplateFile")]
        public static object CreateFile(ScriptThread thread, object instance, object[] parameters)
        {
            //Create a native fileHandle and return an array with the pointer under Handle
            //Like in GetStdHandle()

            string fileName = (string)parameters[0];
            int dwDesiredAccess = unchecked((int)Convert.ToUInt32(parameters[1]));
            System.IO.FileShare dwShareMode = (System.IO.FileShare)Convert.ToInt32(parameters[2]);
            SECURITY_ATTRIBUTES securityAttrs = null;
            System.IO.FileMode dwCreationDisposition = (System.IO.FileMode)Convert.ToInt32(parameters[4]);
            int dwFlagsAndAttributes = Convert.ToInt32(parameters[5]);
            IntPtr hTemplateFile = IntPtr.Zero;

            SafeFileHandle handle =
                NativeMethods.CreateFile(fileName, dwDesiredAccess, dwShareMode,
                securityAttrs, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

            DataTable ret = new DataTable();
            ret.SetString(thread, "Handle", handle.DangerousGetHandle());
            ret.SetString(thread, "NativeHandle", handle);
            ret.SetString(thread, "IsInvalid", handle.IsInvalid);

            return ret;
        }

        [CalcCallableMethod("Win32Native_GetFileType", 1)]
        public static object GetFileType(ScriptThread thread, object instance, object[] parameters)
        {
            object ret = null;
            IntPtr preHandle = (IntPtr)parameters[0];
            SafeFileHandle handle = new SafeFileHandle(preHandle, false);
            ret = (decimal)NativeMethods.GetFileType(handle);
            return ret;
        }

        [CalcCallableMethod("Win32Native_DisposeNativeHandle", 1, "nativeHandle")]
        public static object DisposeNativeHandle(ScriptThread thread, object instance, object[] parameters)
        {
            DataTable handleHolder = (DataTable)parameters[0];
            SafeFileHandle handle = (SafeFileHandle)handleHolder.GetString(thread, "NativeHandle");
            handle.Dispose();

            return null;
        }

        [CalcCallableMethod("Win32Native_SetFilePointer", 4)]
        public static unsafe object SetFilePointer(ScriptThread thread, object instance, object[] parameters)
        {
            IntPtr preHandle = (IntPtr)parameters[0];
            SafeFileHandle handle = new SafeFileHandle(preHandle, false);
            int lo = Convert.ToInt32(parameters[1]);
            int hi = Convert.ToInt32(parameters[2]);
            int origin = Convert.ToInt32(parameters[3]);

            object ret = null;

            lo = NativeMethods.SetFilePointer(handle, lo, &hi, origin);

            if (lo == -1)
                hi = 0;

            ret = (decimal)((((ulong)((uint)hi)) << 32) | ((uint)lo));

            return ret;
        }

        [CalcCallableMethod("Win32Native_GetStdHandle", 1)]
        public static object GetStdHandle(ScriptThread thread, object instance, object[] parameters)
        {
            int nStdHandle = Convert.ToInt32(parameters[0]);
            return NativeMethods.GetStdHandle(nStdHandle);
        }

        [CalcCallableMethod("Win32Native_WriteFile", 4)]
        public unsafe static object WriteFile(ScriptThread thread, object instance, object[] parameters)
        {
            IntPtr preHandle = (IntPtr)parameters[0];
            SafeFileHandle handle = new SafeFileHandle(preHandle, false);
            var bytesDT = ((DataTable)parameters[1]).GetIntIndexedDict();
            int offset = Convert.ToInt32(parameters[2]);
            int numBytesToWrite = Convert.ToInt32(parameters[3]);
            int numBytesWritten = 0;

            byte[] bytes = new byte[bytesDT.Count];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(bytesDT[i]);
            }

            fixed (byte* p = bytes)
            {
                NativeMethods.WriteFile(handle, p + offset, numBytesToWrite, out numBytesWritten, (NativeOverlapped*)IntPtr.Zero);
            }

            return numBytesWritten;
        }

        [CalcCallableMethod("Win32Native_ReadFile", 4)]
        public unsafe static object ReadFile(ScriptThread thread, object instance, object[] parameters)
        {
            IntPtr preHandle = (IntPtr)parameters[0];
            SafeFileHandle handle = new SafeFileHandle(preHandle, false);
            var bytesDT = (DataTable)parameters[1];
            int offset = Convert.ToInt32(parameters[2]);
            int numBytesToRead = Convert.ToInt32(parameters[3]);
            int numBytesRead = 0;

            byte[] bytes = new byte[numBytesToRead];

            fixed (byte* p = bytes)
            {
                NativeMethods.ReadFile(handle, p + offset, numBytesToRead, out numBytesRead, (NativeOverlapped*)IntPtr.Zero);
            }

            for (int i = 0; i < numBytesToRead; i++)
                bytesDT.SetInt(thread, i, bytes[i]);

            return numBytesRead;
        }

        [CalcCallableMethod("Win32Native_WaitForAvailableConsoleInput", 1)]
        public static object WaitForAvailableConsoleInput(ScriptThread thread, object instance, object[] parameters)
        {
            IntPtr preHandle = (IntPtr)parameters[0];
            SafeFileHandle handle = new SafeFileHandle(preHandle, false);

            //while (!Console.KeyAvailable) { }

            return null;
        }

        [CalcCallableMethod("Win32Native_GetConsoleOutputCP", 0)]
        public static object GetConsoleOutputCP(ScriptThread thread, object instance, object[] parameters) => (decimal)NativeMethods.GetConsoleOutputCP();

        [CalcCallableMethod("Win32Native_GetConsoleCP", 0)]
        public static object GetConsoleCP(ScriptThread thread, object instance, object[] parameters) => (decimal)NativeMethods.GetConsoleCP();

        [CalcCallableMethod("Win32Native_GetEncodingBytes", 4)]
        public static object GetEncodingBytes(ScriptThread thread, object instance, object[] parameters)
        {
            int cp = Convert.ToInt32(parameters[0]);
            int start = Convert.ToInt32(parameters[2]);
            int count = Convert.ToInt32(parameters[3]);

            var charsDT = ((DataTable)parameters[1]).GetIntIndexedDict();

            char[] chars = new char[charsDT.Count];

            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = Convert.ToString(charsDT[i])[0];
            }

            var encoding = Encoding.GetEncoding(cp);
            byte[] resultBytes = encoding.GetBytes(chars, start, count);

            var bytesDT = new DataTable(resultBytes.Length, 0);

            for (int i = 0; i < resultBytes.Length; i++)
                bytesDT.SetInt(thread, i, (decimal)resultBytes[i]);

            return bytesDT;
        }

        [CalcCallableMethod("Win32Native_GetEncodingMaxCharCount", 2)]
        public static object GetEncodingMaxCharCount(ScriptThread thread, object instance, object[] parameters)
        {
            int cp = Convert.ToInt32(parameters[0]);
            int bufferSize = Convert.ToInt32(parameters[1]);

            var encoding = Encoding.GetEncoding(cp);
            int ret = encoding.GetMaxCharCount(bufferSize);

            return (decimal)ret;
        }

        [CalcCallableMethod("Win32Native_GetEncodingPreamble", 1)]
        public static object GetEncodingPreamble(ScriptThread thread, object instance, object[] parameters)
        {
            int cp = Convert.ToInt32(parameters[0]);

            var enc = Encoding.GetEncoding(cp);
            byte[] preamble = enc.GetPreamble();

            DataTable ret = new DataTable(preamble.Length, 0);

            for (int i = 0; i < preamble.Length; i++)
                ret.SetInt(thread, i, (decimal)preamble[i]);

            return ret;
        }

        [CalcCallableMethod("Win32Native_GetEncodingChars", 6)]
        public static object GetEncodingChars(ScriptThread thread, object instance, object[] parameters)
        {
            int cp = Convert.ToInt32(parameters[0]);
            var bytesDT = ((DataTable)parameters[1]).GetIntIndexedDict();
            var byteIndex = Convert.ToInt32(parameters[2]);
            var byteCount = Convert.ToInt32(parameters[3]);
            var charsDT = (DataTable)parameters[4];
            var charIndex = Convert.ToInt32(parameters[5]);

            var enc = Encoding.GetEncoding(cp);
            byte[] bytes = new byte[bytesDT.Count];

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(bytesDT[i]);

            char[] charBuff = new char[bytes.Length];

            int chars = enc.GetDecoder().GetChars(bytes, byteIndex, byteCount, charBuff, 0);

            for (int i = 0; i < chars; i++)
            {
                charsDT.SetInt(thread, i + charIndex, charBuff[i].ToString());
            }

            return chars;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class SECURITY_ATTRIBUTES
        {
            internal int nLength = 0;
            internal unsafe byte* pSecurityDiscriptor = null;
            internal int bInheritHandle = 0;
        }
    }
}
