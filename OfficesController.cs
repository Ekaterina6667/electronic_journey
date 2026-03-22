using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Администратор")] // Только администратор
    public class OfficesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OfficesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Offices
        public async Task<IActionResult> Index()
        {
            var offices = await _context.Offices
                .OrderBy(o => o.RoomNumber)
                .ToListAsync();
            return View(offices);
        }

        // GET: Offices/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Offices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomNumber,Description")] Office office)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли уже кабинет с таким номером
                var existingOffice = await _context.Offices
                    .FirstOrDefaultAsync(o => o.RoomNumber == office.RoomNumber);

                if (existingOffice != null)
                {
                    ModelState.AddModelError("RoomNumber", "Кабинет с таким номером уже существует");
                    return View(office);
                }

                _context.Add(office);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Кабинет успешно добавлен";
                return RedirectToAction(nameof(Index));
            }
            return View(office);
        }

        // GET: Offices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var office = await _context.Offices.FindAsync(id);
            if (office == null)
            {
                return NotFound();
            }
            return View(office);
        }

        // POST: Offices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomNumber,Description")] Office office)
        {
            if (id != office.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Проверяем, существует ли другой кабинет с таким же номером
                var existingOffice = await _context.Offices
                    .FirstOrDefaultAsync(o => o.RoomNumber == office.RoomNumber && o.Id != office.Id);

                if (existingOffice != null)
                {
                    ModelState.AddModelError("RoomNumber", "Кабинет с таким номером уже существует");
                    return View(office);
                }

                try
                {
                    _context.Update(office);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Кабинет успешно обновлен";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfficeExists(office.Id))
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
            return View(office);
        }

        // POST: Offices/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var office = await _context.Offices.FindAsync(id);

            if (office == null)
            {
                return NotFound();
            }

            _context.Offices.Remove(office);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Кабинет успешно удален";

            return RedirectToAction(nameof(Index));
        }

        private bool OfficeExists(int id)
        {
            return _context.Offices.Any(e => e.Id == id);
        }
    }
}