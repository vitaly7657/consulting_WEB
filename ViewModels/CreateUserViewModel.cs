using System.ComponentModel.DataAnnotations;

namespace m21_e2_WEB.ViewModels
{
    public class CreateUserViewModel //форма создания пользователя
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
