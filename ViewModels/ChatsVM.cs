using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class ChatsVM
    {
        public List<ChatUserProfile> chats { get; set;}
        public MessageVM messageVm { get; set; }
        public string idChat { get; set; }
        public ChatUserProfile currentChat { get; set; }
        public string search_str { get; set; }
    }
}
