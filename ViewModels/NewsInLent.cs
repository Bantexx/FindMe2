using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class NewsInLent
    {
        public List<AuthorNewsVM> lent_news;
        public List<News_tags> lent_tags;
        public List<News_Attachment> lent_attach;
    }
}
