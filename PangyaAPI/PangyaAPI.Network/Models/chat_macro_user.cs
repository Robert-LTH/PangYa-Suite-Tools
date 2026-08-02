using System.Runtime.InteropServices;
using PangyaAPI.Utilities.BinaryModels;
namespace PangyaAPI.Network.Models
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class chat_macro_user
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public chat_macro[] macro;

        public chat_macro_user()
        {
            macro = new chat_macro[9];
            clear();
        }

        public void setMacro(int index, string macros)
        {
            macro[index].text = macros;
        }

        public void clear()
        {
            for (int i = 0; i < macro.Length; i++)
            {
                macro[i] = new chat_macro();
            }
        }
        public byte[] ToArray()
        {
            using (var p = new PangyaBinaryWriter())
            {
                for (int i = 0; i < 9; i++)
                {
                    p.WriteString(macro[i].text, 64);
                }
                return p.GetBytes;
            }
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class chat_macro
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string text;
        }
    }

}
