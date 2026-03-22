using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    public class AdvertisementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdvertisementsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Ученик")]
        public async Task<IActionResult> Index()
        {
            var advertisements = await _context.Advertisements
                .OrderByDescending(a => a.PublicationDate)
                .ToListAsync();

            // Получаем полное имя ученика
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int studentId))
            {
                var student = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

                if (student != null)
                {
                    ViewBag.StudentFullName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim();
                }
            }

            return View(advertisements);
        }
        // GET: Advertisements/TeacherIndex - для учителя (только просмотр)
        [Authorize(Roles = "Учитель")]
        public async Task<IActionResult> TeacherIndex()
        {
            var advertisements = await _context.Advertisements
                .Include(a => a.Admin)
                .OrderByDescending(a => a.PublicationDate)
                .ToListAsync();

            // Получаем полное имя учителя
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int teacherId))
            {
                var teacher = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == teacherId && u.RoleId == 2);

                if (teacher != null)
                {
                    ViewBag.TeacherFullName = $"{teacher.LastName} {teacher.FirstName} {teacher.MiddleName}".Trim();
                }
                else
                {
                    ViewBag.TeacherFullName = "Учитель";
                }
            }
            else
            {
                ViewBag.TeacherFullName = "Учитель";
            }

            return View("TeacherIndex", advertisements);
        }
        // GET: Advertisements/Create - только для администратора
        [Authorize(Roles = "Администратор")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Advertisements/Create
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description")] Advertisement advertisement)
        {
            if (ModelState.IsValid)
            {
                // Получаем ID текущего администратора
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int adminId))
                {
                    advertisement.AdminId = adminId;
                    advertisement.PublicationDate = DateTime.UtcNow;

                    _context.Add(advertisement);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Объявление успешно добавлено";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(advertisement);
        }

        // GET: Advertisements/Edit/5 - только для администратора
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return NotFound();
            }
            return View(advertisement);
        }

        // POST: Advertisements/Edit/5
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description")] Advertisement advertisement)
        {
            if (id != advertisement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAd = await _context.Advertisements.FindAsync(id);
                    if (existingAd == null)
                    {
                        return NotFound();
                    }

                    // Обновляем только описание, дата и автор остаются
                    existingAd.Description = advertisement.Description;

                    _context.Update(existingAd);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Объявление успешно обновлено";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdvertisementExists(advertisement.Id))
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
            return View(advertisement);
        }
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> AdminIndex()
        {
            var advertisements = await _context.Advertisements
                .Include(a => a.Admin)
                .OrderByDescending(a => a.PublicationDate)
                .ToListAsync();

            // Получаем полное имя администратора
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int adminId))
            {
                var admin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == adminId && u.RoleId == 1);

                if (admin != null)
                {
                    ViewBag.AdminFullName = $"{admin.LastName} {admin.FirstName} {admin.MiddleName}".Trim();
                }
            }

            return View("AdminIndex", advertisements);
        }
        // POST: Advertisements/Delete/5 - только для администратора
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement != null)
            {
                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Объявление успешно удалено";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AdvertisementExists(int id)
        {
            return _context.Advertisements.Any(e => e.Id == id);
        }
    }
}