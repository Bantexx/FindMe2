using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Логин не указан")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Пароль не указан")]
        public string Password { get; set; }
    }
}
