using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.Models
{
    public class News_Attachment
    {
        public int Id { get; set; }
        public int Id_News { get; set; }
        public string Url_attach { get; set; }
        public string Type { get; set; }
    }
}
