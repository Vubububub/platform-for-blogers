namespace platform_for_blogers.Models
{
    // Роли пользователей. Администратора нельзя зарегистрировать с сайта — он создаётся автоматически.
    public enum UserRole
    {
        Blogger,
        Advertiser,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsBlocked { get; set; }
    }
}