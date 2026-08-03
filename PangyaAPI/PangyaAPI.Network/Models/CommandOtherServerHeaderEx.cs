namespace PangyaAPI.Network.Models
{

    // Auth Server - Server Send command to Other Server Header Ex
    public class CommandOtherServerHeaderEx : CommandOtherServerHeader
    {
        public class StCommand
        {
            public byte[] buff { get; set; }
            public ushort size { get; set; }
            private bool state { get; set; }

            public StCommand(ushort size = 0)
            {
                buff = null;
                this.size = 0;
                state = false;

                init(size);
            }

            public void Destroy()
            {
                buff = null;
                state = false;
            }

            public void init(ushort size)
            {
                if (size > 0)
                {
                    this.size = size;

                    if (buff != null)
                        Destroy();

                    buff = new byte[size];
                    state = true;
                }
            }

            public bool is_good() => state;
        }

        public StCommand command { get; set; }

        public CommandOtherServerHeaderEx(uint ul = 0) : base(ul)
        {
            command = new StCommand(0);
            Clear();
        }

        ~CommandOtherServerHeaderEx()
        {
            Clear();
        }

        public new void Clear()
        {
            base.Clear();
            command.Destroy();
        }
    }

}
