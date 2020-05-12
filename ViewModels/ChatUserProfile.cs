using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class ChatUserProfile
    {
        public int IdUser { get; set; }
        public string Login { get; set; }
        public string Avatar { get; set; }
        public string GeoLocation { get; set; }
        public string ChatId { get; set; }
    }
}
