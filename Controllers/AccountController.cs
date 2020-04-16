using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FindMe2.DapRepo;
using FindMe2.MainClasses;
using FindMe2.Models;
using FindMe2.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FindMe2.Controllers
{
    public class AccountController : Controller
    {
        IUserRepository repo;
        public AccountController(IUserRepository r)
        {
            repo = r;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChangePass()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePass(ForgotPassVM model)
        {
            if (ModelState.IsValid)
            {
                var user = repo.GetUserByEmail(model.Email);
                if(user != null)
                {
                    //var token = UserAuth.GetRndToken();
                    var token = "555";
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { email = user.Email, code = token }, protocol: HttpContext.Request.Scheme);
                    EmailService emailService = new EmailService();
                    await emailService.SendEmailAsync(model.Email, "Reset Password",
                        $"Для сброса пароля пройдите по ссылке: <a href='{callbackUrl}'>Click Here!</a>");
                }
            }
            return View("AboutMail");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email,string code = null)
        {
            ResetPassVM model = new ResetPassVM();
            model.Email = email;
            return code == null ? View("Error") : View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPassVM model)
        {
            if (ModelState.IsValid)
            {
                var user = repo.GetUserByEmail(model.Email);
                if (user != null)
                {
                    await repo.ResetPass(user.Login, model.Password);
                    return View("ResetPassConfirm");
                }
            }                
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = repo.GetUserByLogin(model.Login);
                if (user == null)
                {
                    repo.CreateUser(model, UserAuth.HashPass(model.Password)); 
                    await Authenticate(user.Id.ToString());
                    return RedirectToAction("Main", "Home");                                 
                }
                else
                {
                    ModelState.AddModelError("", "Данный логин уже зарегистрирован");
                }
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel logmodel)
        {
            if (ModelState.IsValid)
            {
                User active_user = repo.GetUser(logmodel);
                if (active_user != null)
                {
                    await Authenticate(active_user.Id.ToString());
                    return RedirectToAction("Main", "Home");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(logmodel);
        }
        private async Task Authenticate(string idUser)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, idUser)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}