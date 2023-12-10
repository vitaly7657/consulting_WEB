using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace m21_e2_WEB.ViewModels
{
    public class LoginViewModel //форма логина
    {
        [Required]
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
