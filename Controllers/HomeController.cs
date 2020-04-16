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

namespace FindMe2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        IUserRepository repo;
        User current_user;
        IWebHostEnvironment _appEnvironment;
        public HomeController(IUserRepository rep, IWebHostEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
            repo = rep;
        }
        [HttpGet]
        public IActionResult Main(string content = null)
        {
            MainNewsVM main_news = new MainNewsVM();
            if (content == "popular")
            {
                List<Tag> pop_tags = repo.GetPopularTags(5);

                main_news.user_tags = repo.GetUserTags(Convert.ToInt32(User.Identity.Name)).Select(x => x.Title).ToList();
                main_news.popular_tags = pop_tags.Select(x => x.Title).ToList();
                main_news.author_news = repo.GetPopularNews(4);
                main_news.content = content;

                return View(main_news);
            }
            else
            {               
                List<Tag> user_tags = repo.GetUserTags(Convert.ToInt32(User.Identity.Name));

                main_news.user_tags = user_tags.Select(x => x.Title).ToList();
                main_news.popular_tags = repo.GetPopularTags(5).Select(x => x.Title).ToList();
                main_news.author_news = repo.GetNewsByUserTags(Convert.ToInt32(User.Identity.Name));
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
    }
}
