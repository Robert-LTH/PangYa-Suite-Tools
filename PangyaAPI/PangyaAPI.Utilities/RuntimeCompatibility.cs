#nullable disable
using System.Runtime.CompilerServices;
using System.Text;

namespace PangyaAPI.Utilities
{
    public static class RuntimeCompatibility
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
