using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Администратор")]
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Groups
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
            var groups = await _context.Groups
                .Include(g => g.GroupMemberships)
                    .ThenInclude(gm => gm.Student)
                .OrderBy(g => g.GroupName)
                .ToListAsync();

            var viewModel = groups.Select(g => new GroupViewModel
            {
                Id = g.Id,
                GroupName = g.GroupName,
                StudentCount = g.GroupMemberships?.Count ?? 0,
                StudentNames = g.GroupMemberships != null && g.GroupMemberships.Any()
                    ? string.Join(", ", g.GroupMemberships.Select(gm =>
                        $"{gm.Student?.LastName} {gm.Student?.FirstName?.Substring(0, 1)}."))
                    : "Нет учеников"
            }).ToList();

            return View(viewModel);
        }

        // GET: Groups/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Students = await _context.Users
                .Where(u => u.RoleId == 3 && !u.IsArchived)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View();
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Проверяем, существует ли уже группа с таким названием
                if (await _context.Groups.AnyAsync(g => g.GroupName == viewModel.GroupName))
                {
                    ModelState.AddModelError("GroupName", "Группа с таким названием уже существует");
                    ViewBag.Students = await _context.Users
                        .Where(u => u.RoleId == 3 && !u.IsArchived)
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .ToListAsync();
                    return View(viewModel);
                }

                // Создаем группу
                var group = new Group
                {
                    GroupName = viewModel.GroupName
                };

                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                // Добавляем учеников в группу
                if (viewModel.SelectedStudentIds != null && viewModel.SelectedStudentIds.Any())
                {
                    foreach (var studentId in viewModel.SelectedStudentIds)
                    {
                        _context.GroupMemberships.Add(new GroupMembership
                        {
                            GroupId = group.Id,
                            StudentId = studentId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"Группа '{group.GroupName}' успешно создана";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Students = await _context.Users
                .Where(u => u.RoleId == 3 && !u.IsArchived)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
            return View(viewModel);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Groups
                .Include(g => g.GroupMemberships)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            var viewModel = new GroupViewModel
            {
                Id = group.Id,
                GroupName = group.GroupName,
                SelectedStudentIds = group.GroupMemberships?.Select(gm => gm.StudentId).ToList() ?? new List<int>()
            };

            ViewBag.Students = await _context.Users
                .Where(u => u.RoleId == 3 && !u.IsArchived)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return View(viewModel);
        }

        // POST: Groups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GroupViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Проверяем уникальность названия (исключая текущую группу)
                if (await _context.Groups.AnyAsync(g => g.GroupName == viewModel.GroupName && g.Id != id))
                {
                    ModelState.AddModelError("GroupName", "Группа с таким названием уже существует");
                    ViewBag.Students = await _context.Users
                        .Where(u => u.RoleId == 3 && !u.IsArchived)
                        .OrderBy(u => u.LastName)
                        .ThenBy(u => u.FirstName)
                        .ToListAsync();
                    return View(viewModel);
                }

                var group = await _context.Groups
                    .Include(g => g.GroupMemberships)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (group == null)
                {
                    return NotFound();
                }

                // Обновляем название группы
                group.GroupName = viewModel.GroupName;

                // Удаляем старых учеников
                _context.GroupMemberships.RemoveRange(group.GroupMemberships);

                // Добавляем новых учеников
                if (viewModel.SelectedStudentIds != null && viewModel.SelectedStudentIds.Any())
                {
                    foreach (var studentId in viewModel.SelectedStudentIds)
                    {
                        _context.GroupMemberships.Add(new GroupMembership
                        {
                            GroupId = group.Id,
                            StudentId = studentId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Группа '{group.GroupName}' успешно обновлена";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Students = await _context.Users
                .Where(u => u.RoleId == 3 && !u.IsArchived)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
            return View(viewModel);
        }
        // POST: Groups/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMemberships)
                .Include(g => g.Schedule)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            // Проверяем, используется ли группа в расписании
            if (group.Schedule != null && group.Schedule.Any())
            {
                TempData["Error"] = "Нельзя удалить группу, которая используется в расписании";
                return RedirectToAction(nameof(Index));
            }

            // Удаляем связи с учениками
            if (group.GroupMemberships != null && group.GroupMemberships.Any())
            {
                _context.GroupMemberships.RemoveRange(group.GroupMemberships);
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Группа '{group.GroupName}' успешно удалена";
            return RedirectToAction(nameof(Index));
        }
    }
}