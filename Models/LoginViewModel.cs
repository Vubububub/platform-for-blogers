using System.ComponentModel.DataAnnotations;

namespace platform_for_blogers.Models
{
    // Отдельная модель для формы входа — не используем класс User напрямую
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}