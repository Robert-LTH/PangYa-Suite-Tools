using System;
namespace PangyaAPI.Network.Models
{

    public partial class TableMac
    {
        public string Mac_Adress { get; set; }
        public DateTime Date { get; set; }

        public TableMac(string adress, DateTime insert_time)
        {
            Mac_Adress = adress;
            Date = insert_time;
        }
    }

}
