using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.Models
{
    public class News
    {
        public int Id_News { get; set; }
        public int Id_Author { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public int Interested { get; set; }
        public string Picture { get; set; }
        public string Title { get; set; }
    }
}
