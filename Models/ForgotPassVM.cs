using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.Models
{
    public class ForgotPassVM
    {
        [Required(ErrorMessage = "Заполните поле Email")]
        [EmailAddress(ErrorMessage = "Поле Email имеет некорректный формат")]
        public string Email { get; set; }
    }
}
