#nullable disable
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using PangyaAPI.Utilities.Resources;

namespace PangyaAPI.Utilities
{

    class DumpGenerator
    {
        [Flags]
        public enum MINIDUMP_TYPE : uint
        {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpWithThreadInfo = 0x00001000,
            // Você pode combinar com '|' os flags desejados
        }

        [DllImport("dbghelp.dll", SetLastError = true)]
        static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            uint processId,
            SafeHandle hFile,
            MINIDUMP_TYPE dumpType,
            IntPtr exceptionParam,
            IntPtr userStreamParam,
            IntPtr callbackParam);

        public static void CreateDump(string dumpFilePath, MINIDUMP_TYPE dumpType = MINIDUMP_TYPE.MiniDumpWithThreadInfo)
        {
            using (var fs = new FileStream(dumpFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var process = Process.GetCurrentProcess();

                bool result = MiniDumpWriteDump(
                    process.Handle,
                    (uint)process.Id,
                    fs.SafeFileHandle,
                    dumpType,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero);

                if (!result)
                    throw new InvalidOperationException(UtilityMessages.Format("DumpGenerationError", Marshal.GetLastWin32Error()));
            }
        }
    }
}
