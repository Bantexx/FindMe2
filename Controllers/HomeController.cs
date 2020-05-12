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
        IHubContext<ChatHub> _hubContext;
        IUserRepository repo;
        User current_user;
        IWebHostEnvironment _appEnvironment;
        private int Current_idUser;
        public HomeController(IUserRepository rep, IWebHostEnvironment appEnvironment, IHttpContextAccessor contextAccessor, IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
            _appEnvironment = appEnvironment;
            repo = rep;
            Current_idUser = Convert.ToInt32(contextAccessor.HttpContext.User.Identity.Name);
        }
        [HttpGet]
        public IActionResult Main(string content = null)
        {
            MainNewsVM main_news = new MainNewsVM();
            main_news.user_tags = repo.GetUserTags(Current_idUser).Select(x => x.Title).ToList();
            main_news.popular_tags = repo.GetPopularTags(7).Select(x => x.Title).ToList();
            main_news.content = content;
            main_news.current_user = repo.GetUserById(User.Identity.Name);
            main_news.fav_news = repo.GetFavNews(Current_idUser);
            main_news.All_tags = repo.GetAllTags();
            if (content == "popular")
            {
                main_news.News = repo.GetPopularNews(7);
            }
            else if(content == "usertags")
            {               
                main_news.News = repo.GetNewsByUserTags(Current_idUser);
            }
            else
            {
                main_news.News = repo.GetAllNews();
            }
            return View(main_news);
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
                string path =  "/Users/" + User.Identity.Name ;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path + "/" + uploadedFile.FileName, FileMode.OpenOrCreate))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                repo.UpdateAvatar(path + "/" + uploadedFile.FileName, Current_idUser);
            }
            return RedirectToAction("Profile");
        }
        [HttpGet]
        public IActionResult Message(string su = null,string idchat = null,string srch_mess = null)
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
                if(srch_mess != null)
                {
                    mvm.messages = repo.GetChatMessages(Convert.ToInt32(idchat)).Where(x=>x.Text!=null && x.Text.IndexOf(srch_mess)!=-1).ToList();
                    mainchats.search_str = srch_mess;
                }
                else
                {
                    mvm.messages = repo.GetChatMessages(Convert.ToInt32(idchat));
                }
                mvm.chat_attachments = repo.GetChatAttach(Convert.ToInt32(idchat));
                mvm.FavMessages = repo.GetMessFavo(Convert.ToInt32(idchat),Current_idUser).Select(x=>x.Id_Mess).ToList();
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
        public async Task<IActionResult> SendMessageByUser(string idc,string idr,string mess, List<IFormFile> files)
        {

            try
            {            
                int id_chat = Convert.ToInt32(idc);
                int id_receiver = Convert.ToInt32(idr);
                int check_chat = repo.GetChat(id_receiver,Current_idUser);
                if (id_chat == check_chat)
                {
                    repo.AddMessage(Current_idUser, mess, DateTime.Now, id_chat);
                    Message current_mess = repo.GetLastMessage(id_chat);
                    if (files.Count > 0)
                    {
                        string path = "/Users/" + User.Identity.Name + "/chats/" + idc;
                        if (!Directory.Exists(_appEnvironment.WebRootPath + path))
                        {
                            Directory.CreateDirectory(_appEnvironment.WebRootPath + path);
                        }
                        foreach (var file in files)
                        {
                            string ext = file.ContentType.Split('/')[1];
                            string file_name = Path.GetRandomFileName() + "." + ext;
                            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path + "/" + file_name, FileMode.OpenOrCreate))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                            await repo.AddAttachToMess(current_mess.Id_chat, current_mess.Id_mess, path + "/" + file_name);                                                        
                        }
                        
                    }
                    await _hubContext.Clients.User(idr).SendAsync("showMess", idc);
                }                            
            }
            catch (FormatException)
            {
                return new NoContentResult();
            }                
            return Redirect("/Home/Message?idchat="+ idc);
        }
        public IActionResult ShowMessage(string idc)
        {
            MessageVM mvm = new MessageVM();
            try
            {
                int id_chat = Convert.ToInt32(idc);
                Message mess = repo.GetLastMessage(id_chat);
                mvm.messages = new List<Message>(){ mess};
                mvm.chat_attachments = repo.GetMessAttach(id_chat,mess.Id_mess);
                mvm.myprofile = repo.GetUserById(mess.Id_sender.ToString());
            }
            catch(FormatException)
            {
                return new NoContentResult();
            }
            return View("SendMessageByUser",mvm);
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
        [HttpGet]
        public IActionResult AddDialog(string idu)
        {
            User check_user = repo.GetUserById(idu);
            if(check_user != null)
            {
                repo.CreateChat(Current_idUser, check_user.Id);
                return RedirectToAction("Message");
            }
            return new NoContentResult();
        }
        public bool AddToFavoriteNews(string idn)
        {
            try
            {
                int id_news = Convert.ToInt32(idn);
                repo.AddToFavoNews(id_news,Current_idUser);
                return true;
            }
            catch(FormatException)
            {
                return false;
            }
            
        }
        public bool DeleteFromFavoritesNews(string idn)
        {
            try
            {
                int id_news = Convert.ToInt32(idn);
                repo.RemoveFromFavoNews(id_news, Current_idUser);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }

        }
        [HttpPost]
        public IActionResult AddNews(string Title,string Text)
        {
            repo.AddNews(Title,Text,Current_idUser,DateTime.Now);
            return RedirectToAction("Main");          
        }
        public IActionResult ShowProfilePopup(string id_user)
        {
            ProfileModel pm = new ProfileModel(repo.GetUserById(id_user),repo.GetUserTags(Convert.ToInt32(id_user)));
            return View("Profile_PopUp", pm);
        }
        public int GetInterestedNews(string id_news)
        {
            try
            {
                return repo.GetInterestedNews(Convert.ToInt32(id_news)); 
            }
            catch(FormatException)
            {
                return 0;
            }
        }
        public bool DeleteCurrentChat(string id_c)
        {
            try
            {
                repo.DeleteChat(Convert.ToInt32(id_c),Current_idUser);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public bool AddToFavMess(string id_c,string id_m)
        {
            try
            {
                repo.AddToFavMess(Convert.ToInt32(id_c), Convert.ToInt32(id_m),Current_idUser);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public int[] GetFavoMess(string id_c)
        {
            try
            {
                var fav_messages = repo.GetMessFavo(Convert.ToInt32(id_c), Current_idUser);
                return fav_messages.Select(x => x.Id_Mess).ToArray();
            }
            catch (FormatException)
            {
                return null;
            }
        }
        public bool DelFromFavMess(string id_c,string id_m)
        {
            try
            {
                repo.DelFromFavMess(Convert.ToInt32(id_c), Convert.ToInt32(id_m), Current_idUser);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public bool DelMessFromChat(string id_c,string id_m)
        {
            try
            {
                repo.DelMessFromChat(Convert.ToInt32(id_c), Convert.ToInt32(id_m), Current_idUser);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
