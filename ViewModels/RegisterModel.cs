using FindMe2.MainClasses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Логин не указан")]
        [StringLength(35, MinimumLength =3,ErrorMessage = "Длина логина должна быть от 3 до 35 символов")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Пароль не указан")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email не указан")]
        [EmailAddress(ErrorMessage = "Email адрес имеет неверный формат")]
        public string Email { get; set; }
    }
}
