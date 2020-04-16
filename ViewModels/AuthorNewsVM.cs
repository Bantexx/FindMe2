using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class AuthorNewsVM
    {
        public int idNews { get; set; }
        public int IdAuthor { get; set; }
        public string Text { get; set; }
        public DateTime DateNews { get; set; }
        public int Interested { get; set; }
        public string Picture { get; set; }
        public string LoginAuthor { get; set; }
        public string Avatar { get; set; }

    }
}
