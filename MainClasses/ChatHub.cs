using FindMe2.DapRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    }
}
