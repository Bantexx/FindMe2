using FindMe2.DapRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.MainClasses
{
    [Authorize]
    public class ChatHub : Hub
    {
        IUserRepository repo;
        public ChatHub(IUserRepository rep)
        {
            repo = rep;
        }
        public async Task Send(string message,string to,string chat_id,string dt)
        { 
            int chatId = Convert.ToInt32(chat_id);
            if (repo.GetChat(Convert.ToInt32(Context.User.Identity.Name), Convert.ToInt32(to))!=0)
            {
                DateTime mess_time = Convert.ToDateTime(dt);
                await repo.AddMessage(Convert.ToInt32(Context.User.Identity.Name), message, mess_time, chatId);
                await Clients.User(to).SendAsync("ShowMess", message, Context.User.Identity.Name, mess_time);
            }
        }
    }
}
