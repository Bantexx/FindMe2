using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.Models
{
    public class Message
    {
        public int Id_mess { get; set; }
        public int Id_chat { get; set; }
        public int Id_sender { get; set; }
        public DateTime date_create { get; set; }
        public int isRead { get; set; }
        public string Text { get; set; }
    }
}
