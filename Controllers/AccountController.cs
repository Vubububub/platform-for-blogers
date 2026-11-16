using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using platform_for_blogers.Data;
using platform_for_blogers.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace platform_for_blogers.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // Показать форму входа
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Обработать вход
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Ищем пользователя по логину и хэшу пароля
            var hash = ComputeSha256Hash(model.Password);
            var user = _db.Users.FirstOrDefault(u =>
                u.Login == model.Login && u.PasswordHash == hash);

            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(model);
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("", "Ваш аккаунт заблокирован администратором");
                return View(model);
            }

            // Создаём ClaimsPrincipal и входим через куки
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Login),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        // Показать форму регистрации
        [HttpGet]
        public IActionResult Register()
        {
            // Передаём в представление только роли Blogger и Advertiser (без Admin)
            ViewBag.Roles = new[]
            {
                new { Value = UserRole.Blogger, Name = "Блогер" },
                new { Value = UserRole.Advertiser, Name = "Рекламодатель" }
            };

            return View();
        }

        // Обработать регистрацию
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new[]
                {
                    new { Value = UserRole.Blogger, Name = "Блогер" },
                    new { Value = UserRole.Advertiser, Name = "Рекламодатель" }
                };
                return View(model);
            }

            // Администратора регистрировать нельзя — только Blogger или Advertiser
            if (model.Role == UserRole.Admin)
            {
                ModelState.AddModelError("", "Регистрация с ролью Администратор недоступна");
                ViewBag.Roles = new[]
                {
                    new { Value = UserRole.Blogger, Name = "Блогер" },
                    new { Value = UserRole.Advertiser, Name = "Рекламодатель" }
                };
                return View(model);
            }

            // Проверяем, что логин свободен
            if (_db.Users.Any(u => u.Login == model.Login))
            {
                ModelState.AddModelError("", "Пользователь с таким логином уже существует");
                ViewBag.Roles = new[]
                {
                    new { Value = UserRole.Blogger, Name = "Блогер" },
                    new { Value = UserRole.Advertiser, Name = "Рекламодатель" }
                };
                return View(model);
            }

            var user = new User
            {
                Login = model.Login,
                PasswordHash = ComputeSha256Hash(model.Password),
                Role = model.Role,
                IsBlocked = false
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            // После регистрации автоматически входим
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Login),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        // Выйти
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Страница доступа запрещён (если не хватает прав)
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Хэш пароля (тот же метод, что и в Program.cs)
        private static string ComputeSha256Hash(string rawData)
        {
            var bytes = Encoding.UTF8.GetBytes(rawData);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}