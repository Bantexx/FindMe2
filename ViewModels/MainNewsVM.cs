using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class MainNewsVM
    {
        public List<AuthorNewsVM> author_news { get; set; }
        public List<string> user_tags { get; set; }
        public List<string> popular_tags { get; set; }
        public string content { get; set; }
    }
}
