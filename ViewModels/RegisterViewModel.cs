using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateBonuses.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Длина свойства {0} должна быть от {2} до {1} символов")]
        [RegularExpression(@"^[A-ZА-Я]+[а-яА-Яa-zA-Z]*$")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 2, ErrorMessage = "Длина свойства {0} должна быть от {2} до {1} символов")]
        [RegularExpression(@"^[A-ZА-Я]+[а-яА-Яa-zA-Z]*$")]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Длина свойства {0} должна быть от {2} до {1} символов")]
        [RegularExpression(@"^[a-zA-Z0-9_\.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9]+$",ErrorMessage ="Вы ввели недопустимый email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвердить пароль")]
        public string PasswordConfirm { get; set; }
    }
}
