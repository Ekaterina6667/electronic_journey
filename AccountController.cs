using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: /Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Ученик")
            {
                return RedirectToAction("Profile", "Student");
            }
            else if (role == "Учитель")
            {
                return RedirectToAction("Profile", "Teacher");
            }
            else if (role == "Администратор")
            {
                return View("AdminProfile", user);
            }

            return View(user);
        }
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Если пользователь уже авторизован, перенаправляем на главную
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Ищем пользователя в БД
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Login == model.Login);

                // Проверяем пароль (В РЕАЛЬНОСТИ ХЕШИРУЙТЕ!)
                if (user != null && user.PasswordHash == model.Password)
                {
                    // Создаем claims (утверждения о пользователе)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Login),
                        new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                        new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Пользователь")
                    };

                    // Создаем identity
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Настройки аутентификации
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                    };

                    // Выполняем вход
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Неверный логин или пароль");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdminProfile(User model, string? NewPassword)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.Login = model.Login;

            if (!string.IsNullOrEmpty(NewPassword))
            {
                // В реальном проекте здесь должно быть хеширование пароля
                user.PasswordHash = NewPassword;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Профиль успешно обновлен";

            return RedirectToAction("Profile");
        }
    }
}