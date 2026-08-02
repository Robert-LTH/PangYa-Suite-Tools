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

    public class exception : Exception
    {
        protected string m_message_error = "";
        protected string m_message_error_full = "";
        protected uint m_code_error = 0;
        public exception(string message) : base(message)
        {
            if (!string.IsNullOrEmpty(message))
                HandleException(this);
        }

        public exception(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public exception(string message_error, uint code_error) : this(UtilityMessages.Format("ErrorCode", message_error, code_error))
        {
            m_message_error = message_error;
            m_code_error = code_error;

            m_message_error_full = UtilityMessages.Format("ErrorCode", m_message_error, code_error);

            Debug.WriteLine(m_message_error_full);

            if (!string.IsNullOrEmpty(message_error))
                HandleException(this);
        }

        public exception(string message_error, STDA_ERROR_TYPE code_error) : this(UtilityMessages.Format("ErrorCode", message_error, code_error))
        {
            m_message_error = message_error;
            m_code_error = (uint)code_error;

            m_message_error_full = UtilityMessages.Format("ErrorCode", m_message_error, code_error);

            Debug.WriteLine(m_message_error_full);

        }

        public Exception GetException()
        {
            return this;
        }
        public string getMessageError()
        {
            return m_message_error;
        }

        public uint getCodeError()
        {
            return m_code_error;
        }

        public string getFullMessageError()
        {
            return UtilityMessages.Format("StackTrace", m_message_error_full, getStackTrace());
        }

        public string getStackTrace()
        {
            return this.StackTrace;
        }
        private void HandleException(exception ex, [CallerMemberName] string source = "")
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string dumpPath = $"crash_{timestamp}.dmp";
            string logPath = $"crash_{timestamp}.log";

            try
            {
                if (!File.Exists(dumpPath))
                    DumpGenerator.CreateDump(dumpPath, DumpGenerator.MINIDUMP_TYPE.MiniDumpNormal); // Cria dump

                // Cria log de erro
                File.WriteAllText(logPath,
                    UtilityMessages.Format("CrashLog", source, DateTime.Now, ex?.Message, ex?.StackTrace));

                Console.WriteLine(UtilityMessages.Format("DumpCreated", dumpPath, logPath));
            }
            catch (Exception innerEx)
            {
                Console.WriteLine(UtilityMessages.Format("DumpFailed", innerEx.Message));
            }

        }

        private void HandleException(Exception ex, [CallerMemberName] string source = "")
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string dumpPath = $"crash_{timestamp}.dmp";
            string logPath = $"crash_{timestamp}.log";

            try
            {
                // Cria dump
                DumpGenerator.CreateDump(dumpPath, DumpGenerator.MINIDUMP_TYPE.MiniDumpNormal);

                // Cria log de erro
                File.WriteAllText(logPath,
                    UtilityMessages.Format("CrashLog", source, DateTime.Now, ex?.Message, ex?.StackTrace));

                Console.WriteLine(UtilityMessages.Format("DumpCreated", dumpPath, logPath));
            }
            catch (Exception innerEx)
            {
                Console.WriteLine(UtilityMessages.Format("DumpFailed", innerEx.Message));
            }

        }
    }
}
