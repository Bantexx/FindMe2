using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class MainNewsVM
    {
        public NewsInLent News { get; set; }
        public List<string> user_tags { get; set; }
        public List<string> popular_tags { get; set; }
        public string content { get; set; }
        public User current_user { get; set; }
        public List<FavoritesNews> fav_news { get; set; }
        public List<Tag> All_tags { get; set; }
    }
}
