using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.Models
{
    public class Attachment
    {
        public int Id_Att { get; set; }
        public int Id_Chat { get; set; }
        public int Id_Mess { get; set; }
        public string Attach { get; set; }
    }
}
