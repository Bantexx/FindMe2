using FindMe2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class ProfileModel
    {
        public User current_user;
        public List<Tag> tags;

        public ProfileModel(User cur_user,List<Tag> _tags)
        {
            current_user = cur_user;
            tags = _tags;
        }
    }
}
