using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Teachers
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out int currentUserId);

            var teachers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.TeacherSpecializations)
                    .ThenInclude(ts => ts.Subject)
                .Where(u => u.RoleId == 2 && !u.IsArchived) // Только учителя, не в архиве
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            // Получаем всех администраторов (завучей), включая текущего
            var vicePrincipals = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.TeacherSpecializations)
                    .ThenInclude(ts => ts.Subject)
                .Where(u => u.RoleId == 1 && !u.IsArchived) // Все администраторы, не в архиве
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            ViewBag.VicePrincipals = vicePrincipals;
            ViewBag.CurrentUserId = currentUserId;

            return View(teachers);
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
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 1); // Только администраторы

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        // GET: Teachers/Archived
        public async Task<IActionResult> Archived()
        {
            var teachers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.TeacherSpecializations)
                    .ThenInclude(ts => ts.Subject)
                .Where(u => u.RoleId == 2 && u.IsArchived) // Только учителя в архиве
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View(teachers);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
            return View();
        }

        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User teacher, int[] selectedSubjects)
        {
            // Устанавливаем роль учителя
            teacher.RoleId = 2;
            teacher.IsArchived = false;

            if (ModelState.IsValid)
            {
                // Проверка уникальности логина
                if (await _context.Users.AnyAsync(u => u.Login == teacher.Login))
                {
                    ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                    ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
                    return View(teacher);
                }

                // Проверка уникальности email
                if (await _context.Users.AnyAsync(u => u.Email == teacher.Email))
                {
                    ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                    ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
                    return View(teacher);
                }

                // Проверка уникальности телефона
                if (await _context.Users.AnyAsync(u => u.PhoneNumber == teacher.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Пользователь с таким телефоном уже существует");
                    ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
                    return View(teacher);
                }

                _context.Add(teacher);
                await _context.SaveChangesAsync();

                // Добавляем специализации (предметы, которые ведет учитель)
                if (selectedSubjects != null && selectedSubjects.Any())
                {
                    foreach (var subjectId in selectedSubjects)
                    {
                        _context.TeacherSpecializations.Add(new TeacherSpecialization
                        {
                            TeacherId = teacher.Id,
                            SubjectId = subjectId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"Учитель {teacher.LastName} {teacher.FirstName} успешно добавлен";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Users
                .Include(u => u.TeacherSpecializations)
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 2);

            if (teacher == null)
            {
                return NotFound();
            }

            // Получаем IDs предметов, которые уже выбраны
            var selectedSubjectIds = teacher.TeacherSpecializations.Select(ts => ts.SubjectId).ToList();
            ViewBag.SelectedSubjects = selectedSubjectIds;
            ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();

            return View(teacher);
        }

        // POST: Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User teacher, int[] selectedSubjects)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            // Проверяем, что это действительно учитель
            var existingTeacher = await _context.Users
                .Include(u => u.TeacherSpecializations)
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 2);

            if (existingTeacher == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Проверка уникальности логина
                    if (await _context.Users.AnyAsync(u => u.Login == teacher.Login && u.Id != teacher.Id))
                    {
                        ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                        ViewBag.SelectedSubjects = selectedSubjects ?? new int[] { };
                        ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
                        return View(teacher);
                    }

                    // Проверка уникальности email
                    if (await _context.Users.AnyAsync(u => u.Email == teacher.Email && u.Id != teacher.Id))
                    {
                        ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                        ViewBag.SelectedSubjects = selectedSubjects ?? new int[] { };
                        ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
                        return View(teacher);
                    }

                    // Проверка уникальности телефона
                    if (await _context.Users.AnyAsync(u => u.PhoneNumber == teacher.PhoneNumber && u.Id != teacher.Id))
                    {
                        ModelState.AddModelError("PhoneNumber", "Пользователь с таким телефоном уже существует");
                        ViewBag.SelectedSubjects = selectedSubjects ?? new int[] { };
                        ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
                        return View(teacher);
                    }

                    // Обновляем основные поля
                    existingTeacher.FirstName = teacher.FirstName;
                    existingTeacher.LastName = teacher.LastName;
                    existingTeacher.MiddleName = teacher.MiddleName;
                    existingTeacher.Login = teacher.Login;
                    existingTeacher.PhoneNumber = teacher.PhoneNumber;
                    existingTeacher.Email = teacher.Email;

                    // Обновляем пароль только если он был изменен
                    if (!string.IsNullOrEmpty(teacher.PasswordHash))
                    {
                        existingTeacher.PasswordHash = teacher.PasswordHash;
                    }

                    // Обновляем специализации
                    // Удаляем старые
                    _context.TeacherSpecializations.RemoveRange(existingTeacher.TeacherSpecializations);

                    // Добавляем новые
                    if (selectedSubjects != null && selectedSubjects.Any())
                    {
                        foreach (var subjectId in selectedSubjects)
                        {
                            _context.TeacherSpecializations.Add(new TeacherSpecialization
                            {
                                TeacherId = existingTeacher.Id,
                                SubjectId = subjectId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Учитель {existingTeacher.LastName} {existingTeacher.FirstName} успешно обновлен";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(teacher.Id))
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

            ViewBag.SelectedSubjects = selectedSubjects ?? new int[] { };
            ViewBag.Subjects = _context.Subjects.OrderBy(s => s.SubjectName).ToList();
            return View(teacher);
        }

        // POST: Teachers/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var teacher = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 2);

            if (teacher == null)
            {
                return NotFound();
            }

            teacher.IsArchived = true;
            teacher.ArchivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Учитель {teacher.LastName} {teacher.FirstName} перемещен в архив";
            return RedirectToAction(nameof(Index));
        }

        // POST: Teachers/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var teacher = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 2 && u.IsArchived);

            if (teacher == null)
            {
                return NotFound();
            }

            teacher.IsArchived = false;
            teacher.ArchivedAt = null;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Учитель {teacher.LastName} {teacher.FirstName} восстановлен из архива";
            return RedirectToAction(nameof(Archived));
        }

        // POST: Teachers/PermanentDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var teacher = await _context.Users
                .Include(u => u.TeacherSpecializations)
                .Include(u => u.Schedules)
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleId == 2);

            if (teacher == null)
            {
                return NotFound();
            }

            // Проверяем, есть ли связанные данные
            bool hasRelatedData = (teacher.Schedules != null && teacher.Schedules.Any());

            if (hasRelatedData)
            {
                TempData["Error"] = "Невозможно полностью удалить учителя, так как у него есть расписание";
                return RedirectToAction(nameof(Archived));
            }

            // Удаляем специализации
            if (teacher.TeacherSpecializations != null && teacher.TeacherSpecializations.Any())
            {
                _context.TeacherSpecializations.RemoveRange(teacher.TeacherSpecializations);
            }

            _context.Users.Remove(teacher);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Учитель {teacher.LastName} {teacher.FirstName} полностью удален из системы";
            return RedirectToAction(nameof(Archived));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}