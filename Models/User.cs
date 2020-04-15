using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public byte[] Salt { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
    }
}
