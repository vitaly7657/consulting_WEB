using System.ComponentModel.DataAnnotations;

namespace m21_e2_WEB.ViewModels
{
    public class EditUserViewModel //форма редактирования пользователя
    {
        public string Id { get; set; }
        [Required]
        [Display(Name = "Логин")]
        public string UserName { get; set; }
    }
}
