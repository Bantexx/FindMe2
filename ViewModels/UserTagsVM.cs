using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class UserTagsVM
    {
        public User user { get; set; }
        public List<Tag> tags { get; set; }
    }
}
