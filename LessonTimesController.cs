using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Администратор")] // Только администратор
    public class LessonTimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LessonTimesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LessonTimes
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
            var lessonTimes = await _context.LessonTimes
                .OrderBy(l => l.LessonStart)
                .ToListAsync();
            return View(lessonTimes);
        }

        // GET: LessonTimes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LessonTimes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LessonStart,LessonEnd")] LessonTime lessonTime)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, что время начала раньше времени окончания
                if (lessonTime.LessonStart >= lessonTime.LessonEnd)
                {
                    ModelState.AddModelError("", "Время начала должно быть раньше времени окончания");
                    return View(lessonTime);
                }

                // Проверяем, нет ли уже такого временного слота
                var existing = await _context.LessonTimes
                    .FirstOrDefaultAsync(l => l.LessonStart == lessonTime.LessonStart && l.LessonEnd == lessonTime.LessonEnd);

                if (existing != null)
                {
                    ModelState.AddModelError("", "Такой временной слот уже существует");
                    return View(lessonTime);
                }

                _context.Add(lessonTime);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Учебные часы успешно добавлены";
                return RedirectToAction(nameof(Index));
            }
            return View(lessonTime);
        }

        // GET: LessonTimes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lessonTime = await _context.LessonTimes.FindAsync(id);
            if (lessonTime == null)
            {
                return NotFound();
            }
            return View(lessonTime);
        }

        // POST: LessonTimes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LessonStart,LessonEnd")] LessonTime lessonTime)
        {
            if (id != lessonTime.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Проверяем, что время начала раньше времени окончания
                if (lessonTime.LessonStart >= lessonTime.LessonEnd)
                {
                    ModelState.AddModelError("", "Время начала должно быть раньше времени окончания");
                    return View(lessonTime);
                }

                // Проверяем, нет ли другого такого же временного слота
                var existing = await _context.LessonTimes
                    .FirstOrDefaultAsync(l => l.LessonStart == lessonTime.LessonStart &&
                                             l.LessonEnd == lessonTime.LessonEnd &&
                                             l.Id != lessonTime.Id);

                if (existing != null)
                {
                    ModelState.AddModelError("", "Такой временной слот уже существует");
                    return View(lessonTime);
                }

                try
                {
                    _context.Update(lessonTime);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Учебные часы успешно обновлены";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonTimeExists(lessonTime.Id))
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
            return View(lessonTime);
        }

        // POST: LessonTimes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lessonTime = await _context.LessonTimes.FindAsync(id);

            if (lessonTime == null)
            {
                return NotFound();
            }

            _context.LessonTimes.Remove(lessonTime);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Учебные часы успешно удалены";

            return RedirectToAction(nameof(Index));
        }

        private bool LessonTimeExists(int id)
        {
            return _context.LessonTimes.Any(e => e.Id == id);
        }
    }
}