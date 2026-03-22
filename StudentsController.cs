using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            // Получаем полное имя администратора
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string fullName = "Администратор";

            if (int.TryParse(userIdClaim, out int adminId))
            {
                var admin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == adminId && u.RoleId == 1);

                if (admin != null)
                {
                    fullName = $"{admin.LastName} {admin.FirstName} {admin.MiddleName}".Trim();
                }
            }

            ViewBag.AdminFullName = fullName;
            var students = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == 3 && !u.IsArchived) // Только ученики, не в архиве
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View(students);
        }

        // GET: Students/Archived
        public async Task<IActionResult> Archived()
        {
            var students = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == 3 && u.IsArchived) // Только ученики в архиве
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View(students);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User student)
        {
            // Устанавливаем роль ученика
            student.RoleId = 3;
            student.IsArchived = false;

            if (ModelState.IsValid)
            {
                // Проверка уникальности логина
                if (await _context.Users.AnyAsync(u => u.Login == student.Login))
                {
                    ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                    return View(student);
                }

                // Проверка уникальности email
                if (await _context.Users.AnyAsync(u => u.Email == student.Email))
                {
                    ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                    return View(student);
                }

                // Проверка уникальности телефона
                if (await _context.Users.AnyAsync(u => u.PhoneNumber == student.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Пользователь с таким телефоном уже существует");
                    return View(student);
                }

                _context.Add(student);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Ученик {student.LastName} {student.FirstName} успешно добавлен";
                return RedirectToAction(nameof(Index));
            }

            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 3);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            // Проверяем, что это действительно ученик
            var existingStudent = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 3);

            if (existingStudent == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Проверка уникальности логина
                    if (await _context.Users.AnyAsync(u => u.Login == student.Login && u.Id != student.Id))
                    {
                        ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                        return View(student);
                    }

                    // Проверка уникальности email
                    if (await _context.Users.AnyAsync(u => u.Email == student.Email && u.Id != student.Id))
                    {
                        ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                        return View(student);
                    }

                    // Проверка уникальности телефона
                    if (await _context.Users.AnyAsync(u => u.PhoneNumber == student.PhoneNumber && u.Id != student.Id))
                    {
                        ModelState.AddModelError("PhoneNumber", "Пользователь с таким телефоном уже существует");
                        return View(student);
                    }

                    // Обновляем основные поля
                    existingStudent.FirstName = student.FirstName;
                    existingStudent.LastName = student.LastName;
                    existingStudent.MiddleName = student.MiddleName;
                    existingStudent.Login = student.Login;
                    existingStudent.PhoneNumber = student.PhoneNumber;
                    existingStudent.Email = student.Email;

                    // Обновляем пароль только если он был изменен
                    if (!string.IsNullOrEmpty(student.PasswordHash))
                    {
                        existingStudent.PasswordHash = student.PasswordHash;
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Ученик {existingStudent.LastName} {existingStudent.FirstName} успешно обновлен";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(student.Id))
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

            return View(student);
        }

        // POST: Students/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 3);

            if (student == null)
            {
                return NotFound();
            }

            student.IsArchived = true;
            student.ArchivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ученик {student.LastName} {student.FirstName} перемещен в архив";
            return RedirectToAction(nameof(Index));
        }

        // POST: Students/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 3 && u.IsArchived);

            if (student == null)
            {
                return NotFound();
            }

            student.IsArchived = false;
            student.ArchivedAt = null;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ученик {student.LastName} {student.FirstName} восстановлен из архива";
            return RedirectToAction(nameof(Archived));
        }

        // POST: Students/PermanentDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var student = await _context.Users
                .Include(u => u.Grades)
                .Include(u => u.Attendances)
                .Include(u => u.GroupMemberships)
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 3);

            if (student == null)
            {
                return NotFound();
            }

            // Проверяем, есть ли связанные данные
            bool hasRelatedData = (student.Grades != null && student.Grades.Any()) ||
                                  (student.Attendances != null && student.Attendances.Any()) ||
                                  (student.GroupMemberships != null && student.GroupMemberships.Any());

            if (hasRelatedData)
            {
                TempData["Error"] = "Невозможно полностью удалить ученика, так как у него есть связанные данные (оценки, посещаемость, группы)";
                return RedirectToAction(nameof(Archived));
            }

            _context.Users.Remove(student);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ученик {student.LastName} {student.FirstName} полностью удален из системы";
            return RedirectToAction(nameof(Archived));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}