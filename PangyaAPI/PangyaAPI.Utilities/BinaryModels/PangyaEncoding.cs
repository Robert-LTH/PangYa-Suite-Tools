#nullable disable
using System;
using System.Text;
using System.Threading;

namespace PangyaAPI.Utilities.BinaryModels
{
    public static class PangyaEncoding
    {
        public const string DefaultName = "shift_jis";

        private static Encoding _current = System.Text.Encoding.GetEncoding(DefaultName);

        public static Encoding Current => Volatile.Read(ref _current);

        public static void Configure(string encodingName)
        {
            var name = string.IsNullOrWhiteSpace(encodingName) ? DefaultName : encodingName;
            var encoding = System.Text.Encoding.GetEncoding(name);
            Interlocked.Exchange(ref _current, encoding);
        }
    }
}
