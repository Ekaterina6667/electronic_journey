using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class AcademicQuartersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcademicQuartersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AcademicQuarters
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
            var quarters = await _context.AcademicQuarters
                .Include(q => q.Creator)  
                .OrderByDescending(q => q.StartDate)
                .ToListAsync();

            var viewModel = quarters.Select(q => new AcademicQuarterViewModel
            {
                Id = q.Id,
                Name = q.Name,
                StartDate = q.StartDate,
                EndDate = q.EndDate
            }).ToList();

            return View(viewModel);
        }

        // GET: AcademicQuarters/Create
        public IActionResult Create()
        {
            var viewModel = new AcademicQuarterViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(2)
            };
            return View(viewModel);
        }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(AcademicQuarterViewModel viewModel)
{
    if (ModelState.IsValid)
    {
        // Проверка, что дата начала раньше даты окончания
        if (viewModel.StartDate >= viewModel.EndDate)
        {
            ModelState.AddModelError("", "Дата начала должна быть раньше даты окончания");
            return View(viewModel);
        }

        var quarter = new AcademicQuarter
        {
            Name = viewModel.Name,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate
        };
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        quarter.CreatedBy = int.Parse(adminId);

        _context.Add(quarter);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Четверть '{quarter.Name}' успешно создана";
        return RedirectToAction(nameof(Index));
    }

    return View(viewModel);
}

        // GET: AcademicQuarters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quarter = await _context.AcademicQuarters.FindAsync(id);
            if (quarter == null)
            {
                return NotFound();
            }

            var viewModel = new AcademicQuarterViewModel
            {
                Id = quarter.Id,
                Name = quarter.Name,
                StartDate = quarter.StartDate,
                EndDate = quarter.EndDate
            };

            return View(viewModel);
        }

        // POST: AcademicQuarters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcademicQuarterViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Проверка, что дата начала раньше даты окончания
                if (viewModel.StartDate >= viewModel.EndDate)
                {
                    ModelState.AddModelError("", "Дата начала должна быть раньше даты окончания");
                    return View(viewModel);
                }

                // Проверка уникальности названия (исключая текущую)
                if (await _context.AcademicQuarters.AnyAsync(q => q.Name == viewModel.Name && q.Id != id))
                {
                    ModelState.AddModelError("Name", "Четверть с таким названием уже существует");
                    return View(viewModel);
                }

                // Проверка пересечения с существующими четвертями (исключая текущую)
                var overlapping = await _context.AcademicQuarters
                    .Where(q => q.Id != id)
                    .AnyAsync(q => (viewModel.StartDate <= q.EndDate && viewModel.EndDate >= q.StartDate));

                if (overlapping)
                {
                    ModelState.AddModelError("", "Даты четверти пересекаются с существующей четвертью");
                    return View(viewModel);
                }

                var quarter = await _context.AcademicQuarters.FindAsync(id);
                if (quarter == null)
                {
                    return NotFound();
                }

                quarter.Name = viewModel.Name;
                quarter.StartDate = viewModel.StartDate;
                quarter.EndDate = viewModel.EndDate;

                try
                {
                    _context.Update(quarter);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Четверть '{quarter.Name}' успешно обновлена";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuarterExists(quarter.Id))
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

            return View(viewModel);
        }

        // POST: AcademicQuarters/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var quarter = await _context.AcademicQuarters.FindAsync(id);
            if (quarter == null)
            {
                return NotFound();
            }

            // Проверяем, используется ли четверть (например, в расписании или оценках)
            // Пока просто удаляем, но в будущем можно добавить проверки

            _context.AcademicQuarters.Remove(quarter);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Четверть '{quarter.Name}' успешно удалена";

            return RedirectToAction(nameof(Index));
        }

        private bool QuarterExists(int id)
        {
            return _context.AcademicQuarters.Any(e => e.Id == id);
        }
    }
}
