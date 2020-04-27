using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FindMe2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using FindMe2.DapRepo;
using FindMe2.ViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using FindMe2.MainClasses;
using IPGeolocation;

namespace FindMe2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        IUserRepository repo;
        User current_user;
        IWebHostEnvironment _appEnvironment;
        private int Current_idUser;
        public HomeController(IUserRepository rep, IWebHostEnvironment appEnvironment, IHttpContextAccessor contextAccessor)
        {
            _appEnvironment = appEnvironment;
            repo = rep;
            Current_idUser = Convert.ToInt32(contextAccessor.HttpContext.User.Identity.Name);
        }
        [HttpGet]
        public IActionResult Main(string content = null)
        {
            MainNewsVM main_news = new MainNewsVM();
            if (content == "popular")
            {
                List<Tag> pop_tags = repo.GetPopularTags(5);

                main_news.user_tags = repo.GetUserTags(Current_idUser).Select(x => x.Title).ToList();
                main_news.popular_tags = pop_tags.Select(x => x.Title).ToList();
                main_news.author_news = repo.GetPopularNews(4);
                main_news.content = content;

                return View(main_news);
            }
            else
            {               
                List<Tag> user_tags = repo.GetUserTags(Current_idUser);

                main_news.user_tags = user_tags.Select(x => x.Title).ToList();
                main_news.popular_tags = repo.GetPopularTags(5).Select(x => x.Title).ToList();
                main_news.author_news = repo.GetNewsByUserTags(Current_idUser);
                main_news.content = content;

                return View(main_news);
            }
        }
        public IActionResult Profile()
        {
            current_user = repo.GetUserById(User.Identity.Name);
            ProfileModel user_profile = new ProfileModel(current_user, repo.GetUserTags(current_user.Id));
            return View(user_profile);
        }
        [HttpGet]
        public IActionResult EditProfile()
        {
            current_user = repo.GetUserById(User.Identity.Name);
            EditProfileVM edit_user = new EditProfileVM();
            edit_user.Login = current_user.Login;
            edit_user.Email = current_user.Email;
            edit_user.Picture = current_user.Picture;
            edit_user.tags =  repo.GetUserTags(current_user.Id).Select(x=>x.Title).ToArray();
            return View(edit_user);
        }
        [HttpPost]
        public IActionResult EditProfile(EditProfileVM editprof)
        {
            current_user = repo.GetUserById(User.Identity.Name);
            if (ModelState.IsValid)
            {
                repo.UpdateInfoUser(editprof, current_user.Id);
                return RedirectToAction("Profile");              
            }
            else
            {
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }            
            return View(editprof);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult ChangeAva()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        [HttpPost]
        public async Task<IActionResult> ChangeAva(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                string path = "/img/" + User.Identity.Name + uploadedFile.FileName;

                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                repo.UpdateAvatar(path, User.Identity.Name);
            }

            return RedirectToAction("Profile");
        }
        [HttpGet]
        public IActionResult Message(string su = null,string idchat = null)
        {
            ChatsVM mainchats = new ChatsVM();
            if(su != null)
            {
                mainchats.chats = repo.GetUserChats(Current_idUser,su);
            }
            else
            {
                mainchats.chats = repo.GetUserChats(Current_idUser,null);
            }
            if (idchat != null)
            {
                MessageVM mvm = new MessageVM();
                mvm.myprofile = repo.GetUserById(User.Identity.Name);
                mvm.messages = repo.GetChatMessages(Convert.ToInt32(idchat));
                mvm.chat_attachments = repo.GetChatAttach(Convert.ToInt32(idchat));
                mainchats.idChat = idchat;
                mainchats.currentChat = mainchats.chats.Where(x => x.ChatId == idchat).FirstOrDefault();
                mainchats.messageVm = mvm;
            }         
            return View(mainchats);
        }
        public string GetLocation(string ip)
        {
            IPGeolocationAPI api = new IPGeolocationAPI("7461e5a511964d22ac7310cf8a9e4f02");
            GeolocationParams geoParams = new GeolocationParams();
            geoParams.SetIp(ip);
            Geolocation geolocation = api.GetGeolocation(geoParams);
            if (geolocation.GetStatus() == 200)
            {
                var location = geolocation.GetCity();
                repo.UpdateLocation(Current_idUser, location);
                return location;
            }
            return "Default";
        }
        public IActionResult SendMessageByUser(string mess,DateTime date, string idsender = null)
        {
            ViewBag.Text = mess;
            ViewBag.Time = date;
            if(idsender != null)
            {
                current_user = repo.GetUserById(idsender);
            }
            else
            {
                current_user = repo.GetUserById(User.Identity.Name);
            }          
            return View(current_user);
        }
        public IActionResult Search(string str,string option = "All")
        {
            ViewBag.stringsrch = str;
            SearchAllVM subsearch = new SearchAllVM();
            subsearch.news = repo.GetNewsByStrSearch(str);
            subsearch.users = repo.GetUsers(str);
            switch (option)
            {
                case "News":
                    return View("SearchNews", subsearch);
                case "Users":
                    List<UserTagsVM> ustg = new List<UserTagsVM>();
                    foreach(var u in subsearch.users)
                    {
                        ustg.Add(new UserTagsVM() { user = u, tags = repo.GetUserTags(u.Id)});
                    }
                    return View("SearchUsers", ustg);
                case "Rooms":
                    return View("SearchRooms");
                default:
                    break;
            }
            return View("AllSearch", subsearch);
        }
        public IActionResult ShowPopUpTags()
        {
            AllTags mw = new AllTags();
            mw.user_tags = repo.GetUserTags(Current_idUser);
            mw.pop_tags = repo.GetPopularTags(10);
            return View(mw);
        }
        public void DelUserTag(string id_tag)
        {
            repo.DelUserTag(Current_idUser, Convert.ToInt32(id_tag));
        }
        public void AddTagToUser(string id_tag)
        {
            List<int> user_tags = repo.GetUserTags(Current_idUser).Select(x=>x.id).ToList();
            if (!user_tags.Contains(Convert.ToInt32(id_tag)))
            {
                repo.AddTagToUser(Current_idUser, Convert.ToInt32(id_tag));
            }        
        }
    }
}
