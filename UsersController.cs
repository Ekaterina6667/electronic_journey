using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsArchived) // Показываем только неархивированных
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View(users);
        }

        // GET: Users/Archived
        public async Task<IActionResult> Archived()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsArchived)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View(users);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }
        // GET: Teachers/EditVicePrincipal/5
        public async Task<IActionResult> EditVicePrincipal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 1);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Teachers/EditVicePrincipal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVicePrincipal(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.MiddleName = user.MiddleName;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.Email = user.Email;
                    existingUser.Login = user.Login;

                    if (!string.IsNullOrEmpty(user.PasswordHash))
                    {
                        existingUser.PasswordHash = user.PasswordHash;
                    }

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Данные завуча успешно обновлены";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                // Проверка уникальности логина
                if (await _context.Users.AnyAsync(u => u.Login == user.Login))
                {
                    ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                    ViewBag.Roles = _context.Roles.ToList();
                    return View(user);
                }

                // Проверка уникальности email
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                    ViewBag.Roles = _context.Roles.ToList();
                    return View(user);
                }

                // Проверка уникальности телефона
                if (await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Пользователь с таким телефоном уже существует");
                    ViewBag.Roles = _context.Roles.ToList();
                    return View(user);
                }

                user.IsArchived = false;
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Пользователь успешно добавлен";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = _context.Roles.ToList();
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.Roles = _context.Roles.ToList();
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Проверка уникальности логина (исключая текущего пользователя)
                    if (await _context.Users.AnyAsync(u => u.Login == user.Login && u.Id != user.Id))
                    {
                        ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                        ViewBag.Roles = _context.Roles.ToList();
                        return View(user);
                    }

                    // Проверка уникальности email
                    if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != user.Id))
                    {
                        ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                        ViewBag.Roles = _context.Roles.ToList();
                        return View(user);
                    }

                    // Проверка уникальности телефона
                    if (await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber && u.Id != user.Id))
                    {
                        ModelState.AddModelError("PhoneNumber", "Пользователь с таким телефоном уже существует");
                        ViewBag.Roles = _context.Roles.ToList();
                        return View(user);
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Пользователь успешно обновлен";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = _context.Roles.ToList();
            return View(user);
        }

        // POST: Users/Archive/5 (мягкое удаление)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsArchived = true;
            user.ArchivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Пользователь {user.LastName} {user.FirstName} перемещен в архив";

            return RedirectToAction(nameof(Index));
        }

        // POST: Users/Restore/5 (восстановление из архива)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsArchived = false;
            user.ArchivedAt = null;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Пользователь {user.LastName} {user.FirstName} восстановлен из архива";

            return RedirectToAction(nameof(Archived));
        }

        // POST: Users/PermanentDelete/5 (полное удаление - только для администраторов)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var user = await _context.Users
                .Include(u => u.Grades)
                .Include(u => u.Attendances)
                .Include(u => u.GroupMemberships)
                .Include(u => u.TeacherSpecializations)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Проверяем, есть ли связанные данные
            bool hasRelatedData = (user.Grades != null && user.Grades.Any()) ||
                                  (user.Attendances != null && user.Attendances.Any()) ||
                                  (user.GroupMemberships != null && user.GroupMemberships.Any()) ||
                                  (user.TeacherSpecializations != null && user.TeacherSpecializations.Any());

            if (hasRelatedData)
            {
                TempData["Error"] = "Невозможно полностью удалить пользователя, так как у него есть связанные данные (оценки, посещаемость и т.д.)";
                return RedirectToAction(nameof(Archived));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Пользователь {user.LastName} {user.FirstName} полностью удален из системы";

            return RedirectToAction(nameof(Archived));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}