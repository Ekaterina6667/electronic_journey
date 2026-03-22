using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Schedule
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
            var schedule = await _context.Schedule
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Group)
                .Include(s => s.Room)
                .OrderBy(s => s.WeekdayId)
                .ThenBy(s => s.LessonTime.LessonStart)
                .ToListAsync();

            var viewModel = schedule.Select(s => new ScheduleViewModel
            {
                Id = s.Id,
                WeekdayId = s.WeekdayId,
                WeekdayName = s.Weekday?.DayName,
                LessonTimeId = s.LessonTimeId,
                LessonTimeDisplay = s.LessonTime != null
                    ? $"{s.LessonTime.LessonStart:hh\\:mm} - {s.LessonTime.LessonEnd:hh\\:mm}"
                    : "",
                SubjectId = s.SubjectId,
                SubjectName = s.Subject?.SubjectName,
                TeacherId = s.TeacherId,
                TeacherName = s.Teacher != null
                    ? $"{s.Teacher.LastName} {s.Teacher.FirstName?[0]}."
                    : "",
                GroupId = s.GroupId,
                GroupName = s.Group?.GroupName,
                RoomId = s.RoomId,
                RoomNumber = s.Room?.RoomNumber ?? 0
            }).ToList();

            return View(viewModel);
        }
        // GET: Schedule/Edit/5
        
        // GET: Schedule/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new ScheduleViewModel();
         
            await PopulateDropDownLists(viewModel);
            return View(viewModel);
        }


        // POST: Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Проверка на конфликт расписания
                var conflict = await CheckScheduleConflict(
                    viewModel.WeekdayId,
                    viewModel.LessonTimeId,
                    viewModel.RoomId,
                    viewModel.TeacherId,
                    null); // null для создания (нет ID)

                if (conflict != null)
                {
                    ModelState.AddModelError("", conflict);
                    await PopulateDropDownLists(viewModel);
                    return View(viewModel);
                }

                var schedule = new Schedule
                {
                    WeekdayId = viewModel.WeekdayId,
                    LessonTimeId = viewModel.LessonTimeId,
                    SubjectId = viewModel.SubjectId,
                    TeacherId = viewModel.TeacherId,
                    GroupId = viewModel.GroupId,
                    RoomId = viewModel.RoomId
                };

                _context.Add(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Занятие успешно добавлено в расписание";
                return RedirectToAction(nameof(Index));
            }
          
            return View(viewModel);
        }

        // GET: Schedule/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedule.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            var viewModel = new ScheduleViewModel
            {
                Id = schedule.Id,
                WeekdayId = schedule.WeekdayId,
                LessonTimeId = schedule.LessonTimeId,
                SubjectId = schedule.SubjectId,
                TeacherId = schedule.TeacherId,
                GroupId = schedule.GroupId,
                RoomId = schedule.RoomId
            };
            
            return View(viewModel);
        }

        // POST: Schedule/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, ScheduleViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Проверка на конфликт расписания
                var conflict = await CheckScheduleConflict(
                    viewModel.WeekdayId,
                    viewModel.LessonTimeId,
                    viewModel.RoomId,
                    viewModel.TeacherId,
                    id);

                if (conflict != null)
                {
                    ModelState.AddModelError("", conflict);
                    await PopulateDropDownLists(viewModel);
                    return View(viewModel);
                }

                var schedule = await _context.Schedule.FindAsync(id);
                if (schedule == null)
                {
                    return NotFound();
                }

                schedule.WeekdayId = viewModel.WeekdayId;
                schedule.LessonTimeId = viewModel.LessonTimeId;
                schedule.SubjectId = viewModel.SubjectId;
                schedule.TeacherId = viewModel.TeacherId;
                schedule.GroupId = viewModel.GroupId;
                schedule.RoomId = viewModel.RoomId;

                try
                {
                    _context.Update(schedule);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Занятие успешно обновлено";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
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

            await PopulateDropDownLists(viewModel);
            return View(viewModel);
        }

        // POST: Schedule/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var schedule = await _context.Schedule.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            // Проверяем, есть ли проведенные занятия по этому расписанию
            var hasLessons = await _context.Lessons.AnyAsync(l => l.ScheduleId == id);
            if (hasLessons)
            {
                TempData["Error"] = "Нельзя удалить расписание, по которому уже были проведены занятия";
                return RedirectToAction(nameof(Index));
            }

            _context.Schedule.Remove(schedule);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Занятие удалено из расписания";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для проверки конфликтов
        private async Task<string?> CheckScheduleConflict(int weekdayId, int lessonTimeId, int roomId, int teacherId, long? excludeId)
        {
            var query = _context.Schedule
                .Where(s => s.WeekdayId == weekdayId && s.LessonTimeId == lessonTimeId);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            var conflicts = await query.ToListAsync();

            // Проверка конфликта по кабинету
            if (conflicts.Any(s => s.RoomId == roomId))
            {
                return "Этот кабинет уже занят в выбранное время";
            }

            // Проверка конфликта по учителю
            if (conflicts.Any(s => s.TeacherId == teacherId))
            {
                return "Этот учитель уже ведет занятие в выбранное время";
            }

            return null;
        }

        // Вспомогательный метод для заполнения выпадающих списков
        private async Task PopulateDropDownLists(ScheduleViewModel viewModel)
        {
            viewModel.Weekdays = await _context.Weekdays
                .OrderBy(w => w.Id)
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.DayName
                })
                .ToListAsync();

            viewModel.LessonTimes = await _context.LessonTimes
                .OrderBy(l => l.LessonStart)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = $"{l.LessonStart:hh\\:mm} - {l.LessonEnd:hh\\:mm}"
                })
                .ToListAsync();

            viewModel.Subjects = await _context.Subjects
                .OrderBy(s => s.SubjectName)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.SubjectName
                })
                .ToListAsync();

            viewModel.Teachers = await _context.Users
                .Where(u => u.RoleId == 2 && !u.IsArchived)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.LastName} {u.FirstName} {u.MiddleName}"
                })
                .ToListAsync();

            viewModel.Groups = await _context.Groups
                .OrderBy(g => g.GroupName)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.GroupName
                })
                .ToListAsync();

            viewModel.Rooms = await _context.Offices
                .OrderBy(o => o.RoomNumber)
                .Select(o => new SelectListItem
                {
                    Value = o.Id.ToString(),
                    Text = o.RoomNumber.ToString()
                })
                .ToListAsync();
        }

        private bool ScheduleExists(long id)
        {
            return _context.Schedule.Any(e => e.Id == id);
        }
    }
}