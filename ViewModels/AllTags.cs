using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class AllTags
    {
        public List<Tag> user_tags { get; set; }
        public List<Tag> pop_tags { get; set; }
    }
}
