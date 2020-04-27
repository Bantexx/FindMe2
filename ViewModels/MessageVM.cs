using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class MessageVM
    {
        public List<Attachment> chat_attachments { get; set; }
        public List<Message> messages { get; set; }
        public User myprofile { get; set; }

    }
}
