using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class SearchAllVM
    {
        public List<User> users { get; set; }
        public List<AuthorNewsVM> news { get; set; }
        //public List<Room> rooms { get; set; }
    }
}
