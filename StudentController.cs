//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using MusicSchoolJournal.Data;
//using MusicSchoolJournal.Models;
//using System.Security.Claims;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using MusicSchoolJournal.Data;
//using MusicSchoolJournal.Models;
//using System.Security.Claims;
//namespace MusicSchoolJournal.Controllers
//{
//    [Authorize(Roles = "Ученик")]
//    public class StudentController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public StudentController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            // Получаем ID текущего ученика
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!int.TryParse(userIdClaim, out int studentId))
//            {
//                return NotFound();
//            }

//            var student = await _context.Users
//                .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

//            if (student == null)
//            {
//                return NotFound();
//            }

//            // Получаем группы ученика
//            var studentGroups = await _context.GroupMemberships
//                .Where(gm => gm.StudentId == studentId)
//                .Select(gm => gm.GroupId)
//                .ToListAsync();

//            var today = DateTime.Today;
//            var tomorrow = DateTime.Today.AddDays(1);

//            // Получаем расписание на сегодня
//            var todaySchedule = await _context.Schedule
//                .Include(s => s.Weekday)
//                .Include(s => s.LessonTime)
//                .Include(s => s.Subject)
//                .Include(s => s.Teacher)
//                .Include(s => s.Room)
//                .Where(s => studentGroups.Contains(s.GroupId) &&
//                           s.WeekdayId == (int)today.DayOfWeek + 1)
//                .OrderBy(s => s.LessonTime.LessonStart)
//                .ToListAsync();

//            // Получаем расписание на завтра
//            var tomorrowSchedule = await _context.Schedule
//                .Include(s => s.Weekday)
//                .Include(s => s.LessonTime)
//                .Include(s => s.Subject)
//                .Include(s => s.Teacher)
//                .Include(s => s.Room)
//                .Where(s => studentGroups.Contains(s.GroupId) &&
//                           s.WeekdayId == (int)tomorrow.DayOfWeek + 1)
//                .OrderBy(s => s.LessonTime.LessonStart)
//                .ToListAsync();

//            // Получаем оценки
//            var grades = await _context.Grades
//                .Include(g => g.Lesson)
//                    .ThenInclude(l => l.Schedule)
//                        .ThenInclude(s => s.Subject)
//                .Where(g => g.StudentId == studentId)
//                .OrderByDescending(g => g.Lesson.LessonDate)
//                .ToListAsync();
//            // Получаем домашние задания через занятия (lessons)
//            var homeworks = await _context.Homework
//                .Include(h => h.Lesson)
//                    .ThenInclude(l => l.Schedule)
//                        .ThenInclude(s => s.Subject)
//                .Where(h => studentGroups.Contains(h.Lesson.Schedule.GroupId))
//                .ToListAsync();
//            // Получаем объявления
//            var announcements = await _context.Advertisements
//                .OrderByDescending(a => a.PublicationDate)
//                .Take(3)
//                .ToListAsync();

//            // Формируем модель представления
//            var viewModel = new StudentDashboardViewModel
//            {
//                Id = student.Id,
//                FirstName = student.FirstName,
//                LastName = student.LastName,
//                MiddleName = student.MiddleName,
//                TodayLessonsCount = todaySchedule.Count,
//                TomorrowLessonsCount = tomorrowSchedule.Count,
//                PendingHomeworkCount = homeworks.Count,
//                NewAnnouncementsCount = announcements.Count,
//                AverageGrade = grades.Any() ? Math.Round(grades.Average(g => g.GradeValue), 1) : 0
//            };

//            // Ближайшее занятие
//            if (todaySchedule.Any())
//            {
//                var next = todaySchedule.First();
//                viewModel.NextLessonSubject = next.Subject?.SubjectName;
//                viewModel.NextLessonTime = $"{next.LessonTime?.LessonStart:hh\\:mm} - {next.LessonTime?.LessonEnd:hh\\:mm}";
//                viewModel.NextLessonRoom = next.Room?.RoomNumber.ToString();
//                viewModel.NextLessonTeacher = next.Teacher != null
//                    ? $"{next.Teacher.LastName} {next.Teacher.FirstName?[0]}."
//                    : "";
//            }

//            // События календаря (ближайшие 7 дней)
//            for (int i = 0; i < 7; i++)
//            {
//                var date = today.AddDays(i);
//                var weekdayId = (int)date.DayOfWeek + 1;

//                // Уроки
//                var lessons = await _context.Schedule
//                    .Include(s => s.LessonTime)
//                    .Include(s => s.Subject)
//                    .Include(s => s.Teacher)
//                    .Include(s => s.Room)
//                    .Where(s => studentGroups.Contains(s.GroupId) && s.WeekdayId == weekdayId)
//                    .OrderBy(s => s.LessonTime.LessonStart)
//                    .ToListAsync();

//                foreach (var lesson in lessons)
//                {
//                    viewModel.CalendarEvents.Add(new CalendarEvent
//                    {
//                        Date = date,
//                        Title = lesson.Subject?.SubjectName ?? "Занятие",
//                        Type = "lesson",
//                        Description = $"Урок с {lesson.LessonTime?.LessonStart:hh\\:mm} до {lesson.LessonTime?.LessonEnd:hh\\:mm}",
//                        Location = $"Каб. {lesson.Room?.RoomNumber}",
//                        Teacher = lesson.Teacher != null
//                            ? $"{lesson.Teacher.LastName} {lesson.Teacher.FirstName?[0]}."
//                            : ""
//                    });
//                }

//                // Домашние задания (проверяем по расписанию)
//                // Здесь можно добавить логику для отображения домашних заданий в календаре
//            }

//            return View(viewModel);
//        }

//        public async Task<IActionResult> Schedule()
//        {
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!int.TryParse(userIdClaim, out int studentId))
//            {
//                return NotFound();
//            }

//            var student = await _context.Users
//                .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

//            if (student == null)
//            {
//                return NotFound();
//            }

//            // Получаем группы ученика
//            var studentGroups = await _context.GroupMemberships
//                .Where(gm => gm.StudentId == studentId)
//                .Select(gm => gm.GroupId)
//                .ToListAsync();

//            var schedule = await _context.Schedule
//                .Include(s => s.Weekday)
//                .Include(s => s.LessonTime)
//                .Include(s => s.Subject)
//                .Include(s => s.Teacher)
//                .Include(s => s.Room)
//                .Where(s => studentGroups.Contains(s.GroupId))
//                .OrderBy(s => s.WeekdayId)
//                .ThenBy(s => s.LessonTime.LessonStart)
//                .ToListAsync();

//            ViewBag.StudentFullName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim();
//            return View(schedule);
//        }
//        // GET: Student/Grades
//        public async Task<IActionResult> Grades(int? quarterId)
//        {
//            // Получаем ID текущего ученика
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!int.TryParse(userIdClaim, out int studentId))
//            {
//                return NotFound();
//            }

//            var student = await _context.Users
//                .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

//            if (student == null)
//            {
//                return NotFound();
//            }

//            // Получаем все четверти
//            var quarters = await _context.AcademicQuarters
//                .OrderByDescending(q => q.StartDate)
//                .ToListAsync();

//            // Создаем базовую модель
//            var viewModel = new StudentGradesViewModel
//            {
//                StudentId = student.Id,
//                StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
//                Quarters = quarters,
//                SubjectGrades = new List<SubjectGrades>()
//            };

//            // Если нет четвертей, возвращаем пустую модель
//            if (!quarters.Any())
//            {
//                return View(viewModel);
//            }

//            // Определяем выбранную четверть
//            int selectedQuarterId;
//            if (quarterId.HasValue && quarters.Any(q => q.Id == quarterId.Value))
//            {
//                selectedQuarterId = quarterId.Value;
//            }
//            else
//            {
//                var today = DateTime.Today;
//                var currentQuarter = quarters.FirstOrDefault(q => today >= q.StartDate && today <= q.EndDate);
//                selectedQuarterId = currentQuarter?.Id ?? quarters.First().Id;
//            }

//            var selectedQuarter = await _context.AcademicQuarters
//                .FirstOrDefaultAsync(q => q.Id == selectedQuarterId);

//            if (selectedQuarter == null)
//            {
//                return View(viewModel);
//            }

//            viewModel.SelectedQuarterId = selectedQuarter.Id;
//            viewModel.QuarterName = selectedQuarter.Name; // ДОБАВИТЬ ЭТУ СТРОКУ
//            // Формируем список всех дат в четверти
//            var dates = new List<DateTime>();
//            for (var date = selectedQuarter.StartDate; date <= selectedQuarter.EndDate; date = date.AddDays(1))
//            {
//                dates.Add(date);
//            }
//            viewModel.Dates = dates;

//            // Получаем все предметы ученика (из расписания его групп)
//            var studentGroups = await _context.GroupMemberships
//                .Where(gm => gm.StudentId == studentId)
//                .Select(gm => gm.GroupId)
//                .ToListAsync();

//            var subjects = await _context.Schedule
//                .Where(s => studentGroups.Contains(s.GroupId))
//                .Select(s => s.Subject)
//                .Distinct()
//                .OrderBy(s => s.SubjectName)
//                .ToListAsync();

//            // Получаем все оценки ученика
//            var grades = await _context.Grades
//                .Include(g => g.Lesson)
//                    .ThenInclude(l => l.Schedule)
//                        .ThenInclude(s => s.Subject)
//                .Where(g => g.StudentId == studentId)
//                .ToListAsync();

//            // Для каждого предмета собираем оценки по датам
//            foreach (var subject in subjects)
//            {
//                var subjectGrades = new SubjectGrades
//                {
//                    SubjectId = subject.Id,
//                    SubjectName = subject.SubjectName,
//                    GradesByDate = new Dictionary<DateTime, int?>()
//                };

//                // Инициализируем все даты как null
//                foreach (var date in dates)
//                {
//                    subjectGrades.GradesByDate[date] = null;
//                }

//                // Заполняем оценки
//                var subjectGradesList = grades.Where(g => g.Lesson?.Schedule?.SubjectId == subject.Id).ToList();
//                foreach (var grade in subjectGradesList)
//                {
//                    var lessonDate = grade.Lesson?.LessonDate.Date;
//                    if (lessonDate.HasValue && dates.Contains(lessonDate.Value))
//                    {
//                        subjectGrades.GradesByDate[lessonDate.Value] = grade.GradeValue;
//                    }
//                }

//                viewModel.SubjectGrades.Add(subjectGrades);
//            }

//            // Считаем общий средний балл
//            var allGrades = viewModel.SubjectGrades
//                .SelectMany(sg => sg.GradesByDate.Values)
//                .Where(g => g.HasValue)
//                .Select(g => g.Value)
//                .ToList();

//            viewModel.OverallAverage = allGrades.Any() ? Math.Round(allGrades.Average(), 2) : 0;

//            return View(viewModel);
//        }
//        // GET: Student/Homework
//        public async Task<IActionResult> Homework(int? weekOffset)
//        {
//            // Получаем ID текущего ученика
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!int.TryParse(userIdClaim, out int studentId))
//            {
//                return NotFound();
//            }

//            var student = await _context.Users
//                .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

//            if (student == null)
//            {
//                return NotFound();
//            }

//            // Определяем смещение недели
//            int offset = weekOffset ?? 0;

//            // Определяем начало текущей недели (понедельник)
//            var today = DateTime.Today;
//            var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
//            if (today.DayOfWeek == DayOfWeek.Sunday)
//            {
//                monday = today.AddDays(-6);
//            }

//            var weekStart = monday.AddDays(7 * offset);
//            var weekEnd = weekStart.AddDays(6);

//            // Получаем группы ученика
//            var studentGroups = await _context.GroupMemberships
//                .Where(gm => gm.StudentId == studentId)
//                .Select(gm => gm.GroupId)
//                .ToListAsync();

//            // Получаем все занятия для групп ученика на выбранную неделю
//            var lessons = await _context.Lessons
//                .Include(l => l.Schedule)
//                    .ThenInclude(s => s.Subject)
//                .Include(l => l.Schedule)
//                    .ThenInclude(s => s.Teacher)
//                .Include(l => l.Schedule)
//                    .ThenInclude(s => s.LessonTime)
//                .Include(l => l.Schedule)
//                    .ThenInclude(s => s.Group)
//                .Where(l => studentGroups.Contains(l.Schedule.GroupId) &&
//                           l.LessonDate.Date >= weekStart &&
//                           l.LessonDate.Date <= weekEnd)
//                .OrderBy(l => l.LessonDate)
//                .ThenBy(l => l.Schedule.LessonTime.LessonStart)
//                .ToListAsync();

//            // Получаем домашние задания для этих занятий (по LessonId)
//            var lessonIds = lessons.Select(l => l.Id).ToList();
//            var homeworks = await _context.Homework
//                .Include(h => h.Lesson) // Подгружаем связанное занятие
//                    .ThenInclude(l => l.Schedule)
//                        .ThenInclude(s => s.Subject)
//                .Include(h => h.Lesson)
//                    .ThenInclude(l => l.Schedule)
//                        .ThenInclude(s => s.Teacher)
//                .Include(h => h.Lesson)
//                    .ThenInclude(l => l.Schedule)
//                        .ThenInclude(s => s.Weekday)
//                .Where(h => lessonIds.Contains(h.LessonId))
//                .ToDictionaryAsync(h => h.LessonId, h => h);

//            // Создаем список дней недели
//            var weekdays = new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
//            var weekDays = new List<StudentWeekDayInfo>();
//            for (int i = 0; i < 7; i++)
//            {
//                var date = weekStart.AddDays(i);
//                weekDays.Add(new StudentWeekDayInfo
//                {
//                    WeekdayId = i + 1,
//                    DayName = weekdays[i],
//                    Date = date,
//                    IsToday = date.Date == today.Date
//                });
//            }

//            var viewModel = new StudentHomeworkViewModel
//            {
//                Lessons = lessons,
//                Homeworks = homeworks,
//                WeekStart = weekStart,
//                WeekEnd = weekEnd,
//                WeekOffset = offset,
//                StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
//                WeekDays = weekDays
//            };

//            return View(viewModel);
//        }
//        // GET: Student/Profile
//        public async Task<IActionResult> Profile()
//        {
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!int.TryParse(userIdClaim, out int studentId))
//            {
//                return NotFound();
//            }

//            var student = await _context.Users
//                .Include(u => u.Role)
//                .FirstOrDefaultAsync(u => u.Id == studentId);

//            if (student == null)
//            {
//                return NotFound();
//            }

//            return View(student);
//        }

//        // GET: Student/Calendar
//        public async Task<IActionResult> Calendar()
//        {
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (!int.TryParse(userIdClaim, out int studentId))
//            {
//                return NotFound();
//            }

//            var studentGroups = await _context.GroupMemberships
//                .Where(gm => gm.StudentId == studentId)
//                .Select(gm => gm.GroupId)
//                .ToListAsync();

//            var schedule = await _context.Schedule
//                .Include(s => s.Weekday)
//                .Include(s => s.LessonTime)
//                .Include(s => s.Subject)
//                .Include(s => s.Room)
//                .Include(s => s.Teacher)
//                .Where(s => studentGroups.Contains(s.GroupId))
//                .OrderBy(s => s.WeekdayId)
//                .ThenBy(s => s.LessonTime.LessonStart)
//                .ToListAsync();

//            return View(schedule);
//        }
//    }
//}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Ученик")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Получаем ID текущего ученика
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return NotFound();
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

            if (student == null)
            {
                return NotFound();
            }

            // Получаем группы ученика
            var studentGroups = await _context.GroupMemberships
                .Where(gm => gm.StudentId == studentId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var today = DateTime.Today;
            var tomorrow = DateTime.Today.AddDays(1);

            // Получаем расписание на сегодня
            var todaySchedule = await _context.Schedule
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Room)
                .Where(s => studentGroups.Contains(s.GroupId) &&
                           s.WeekdayId == (int)today.DayOfWeek + 1)
                .OrderBy(s => s.LessonTime.LessonStart)
                .ToListAsync();

            // Получаем расписание на завтра
            var tomorrowSchedule = await _context.Schedule
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Room)
                .Where(s => studentGroups.Contains(s.GroupId) &&
                           s.WeekdayId == (int)tomorrow.DayOfWeek + 1)
                .OrderBy(s => s.LessonTime.LessonStart)
                .ToListAsync();

            // Получаем оценки
            var grades = await _context.Grades
                .Include(g => g.Lesson)
                    .ThenInclude(l => l.Schedule)
                        .ThenInclude(s => s.Subject)
                .Where(g => g.StudentId == studentId)
                .OrderByDescending(g => g.Lesson.LessonDate)
                .ToListAsync();

            // Получаем домашние задания через занятия (lessons)
            var homeworks = await _context.Homework
                .Include(h => h.Lesson)
                    .ThenInclude(l => l.Schedule)
                        .ThenInclude(s => s.Subject)
                .Where(h => studentGroups.Contains(h.Lesson.Schedule.GroupId))
                .ToListAsync();

            // Получаем объявления
            var announcements = await _context.Advertisements
                .OrderByDescending(a => a.PublicationDate)
                .Take(3)
                .ToListAsync();

            // Формируем модель представления
            var viewModel = new StudentDashboardViewModel
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                MiddleName = student.MiddleName,
                TodayLessonsCount = todaySchedule.Count,
                TomorrowLessonsCount = tomorrowSchedule.Count,
                PendingHomeworkCount = homeworks.Count,
                NewAnnouncementsCount = announcements.Count,
                AverageGrade = grades.Any() ? Math.Round(grades.Average(g => g.GradeValue), 1) : 0
            };

            // Ближайшее занятие
            if (todaySchedule.Any())
            {
                var next = todaySchedule.First();
                viewModel.NextLessonSubject = next.Subject?.SubjectName;
                viewModel.NextLessonTime = $"{next.LessonTime?.LessonStart:hh\\:mm} - {next.LessonTime?.LessonEnd:hh\\:mm}";
                viewModel.NextLessonRoom = next.Room?.RoomNumber.ToString();
                viewModel.NextLessonTeacher = next.Teacher != null
                    ? $"{next.Teacher.LastName} {next.Teacher.FirstName?[0]}."
                    : "";
            }

            // События календаря (ближайшие 7 дней)
            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                var weekdayId = (int)date.DayOfWeek + 1;

                // Уроки
                var lessons = await _context.Schedule
                    .Include(s => s.LessonTime)
                    .Include(s => s.Subject)
                    .Include(s => s.Teacher)
                    .Include(s => s.Room)
                    .Where(s => studentGroups.Contains(s.GroupId) && s.WeekdayId == weekdayId)
                    .OrderBy(s => s.LessonTime.LessonStart)
                    .ToListAsync();

                foreach (var lesson in lessons)
                {
                    viewModel.CalendarEvents.Add(new CalendarEvent
                    {
                        Date = date,
                        Title = lesson.Subject?.SubjectName ?? "Занятие",
                        Type = "lesson",
                        Description = $"Урок с {lesson.LessonTime?.LessonStart:hh\\:mm} до {lesson.LessonTime?.LessonEnd:hh\\:mm}",
                        Location = $"Каб. {lesson.Room?.RoomNumber}",
                        Teacher = lesson.Teacher != null
                            ? $"{lesson.Teacher.LastName} {lesson.Teacher.FirstName?[0]}."
                            : ""
                    });
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Schedule()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return NotFound();
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

            if (student == null)
            {
                return NotFound();
            }

            // Получаем группы ученика
            var studentGroups = await _context.GroupMemberships
                .Where(gm => gm.StudentId == studentId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var schedule = await _context.Schedule
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Room)
                .Where(s => studentGroups.Contains(s.GroupId))
                .OrderBy(s => s.WeekdayId)
                .ThenBy(s => s.LessonTime.LessonStart)
                .ToListAsync();

            ViewBag.StudentFullName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim();
            return View(schedule);
        }

        // GET: Student/Grades
        public async Task<IActionResult> Grades(int? quarterId, int? academicYearId, bool showCurrent = true)
        {
            // Получаем ID текущего ученика
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return NotFound();
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

            if (student == null)
            {
                return NotFound();
            }

            // Получаем группу ученика
            var groupMembership = await _context.GroupMemberships
                .FirstOrDefaultAsync(gm => gm.StudentId == studentId);

            if (groupMembership == null)
            {
                return View(new StudentGradesViewModel
                {
                    StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
                    Quarters = new List<AcademicQuarter>(),
                    AcademicYears = new List<AcademicYear>()
                });
            }

            // Получаем четверти
            var quarters = await _context.AcademicQuarters
                .Select(q => new AcademicQuarter
                {
                    Id = q.Id,
                    Name = q.Name,
                    StartDate = q.StartDate,
                    EndDate = q.EndDate,
                    CreatedBy = q.CreatedBy
                })
                .OrderByDescending(q => q.StartDate)
                .ToListAsync();

            // Получаем учебные годы
            var academicYears = await _context.AcademicYears
                .OrderByDescending(y => y.StartYear)
                .ToListAsync();

            // Создаем модель
            var viewModel = new StudentGradesViewModel
            {
                StudentId = student.Id,
                StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
                Quarters = quarters,
                AcademicYears = academicYears,
                SelectedQuarterId = quarterId ?? quarters.FirstOrDefault()?.Id ?? 0,
                SelectedAcademicYearId = academicYearId ?? academicYears.FirstOrDefault()?.Id,
                SubjectGrades = new List<SubjectGrades>(),
                FinalGrades = new List<SubjectFinalGrades>()
            };

            if (showCurrent)
            {
                // Загружаем текущие оценки
                await LoadCurrentGradesData(viewModel, groupMembership.GroupId);
            }
            else
            {
                // Загружаем итоговые оценки
                await LoadFinalGradesData(viewModel, groupMembership.GroupId);
            }

            return View(viewModel);
        }

        private async Task LoadCurrentGradesData(StudentGradesViewModel viewModel, int groupId)
        {
            // Получаем выбранную четверть
            var selectedQuarter = await _context.AcademicQuarters
                .FirstOrDefaultAsync(q => q.Id == viewModel.SelectedQuarterId);

            if (selectedQuarter == null) return;

            viewModel.QuarterName = selectedQuarter.Name;

            // Получаем расписание для группы ученика
            var schedules = await _context.Schedule
                .Include(s => s.Subject)
                .Where(s => s.GroupId == groupId)
                .ToListAsync();

            // Формируем список дат в четверти
            var dates = new List<DateTime>();
            for (var date = selectedQuarter.StartDate; date <= selectedQuarter.EndDate; date = date.AddDays(1))
            {
                var weekdayId = (int)date.DayOfWeek + 1;
                if (schedules.Any(s => s.WeekdayId == weekdayId))
                {
                    dates.Add(date);
                }
            }
            viewModel.Dates = dates.OrderBy(d => d).ToList();

            // Получаем все занятия за этот период
            var lessons = await _context.Lessons
                .Where(l => l.LessonDate >= selectedQuarter.StartDate &&
                           l.LessonDate <= selectedQuarter.EndDate &&
                           schedules.Select(s => s.Id).Contains(l.ScheduleId))
                .ToListAsync();

            // Получаем оценки ученика
            var grades = await _context.Grades
                .Where(g => g.StudentId == viewModel.StudentId &&
                           lessons.Select(l => l.Id).Contains(g.LessonId))
                .ToListAsync();

            // Группируем оценки по урокам
            var gradesByLesson = grades
                .GroupBy(g => g.LessonId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Формируем оценки по предметам
            foreach (var schedule in schedules)
            {
                var subjectGrades = new SubjectGrades
                {
                    SubjectId = schedule.Subject?.Id ?? 0,
                    SubjectName = schedule.Subject?.SubjectName ?? "Предмет",
                    GradesByDate = new Dictionary<DateTime, int?>()
                };

                // Инициализируем все даты
                foreach (var date in viewModel.Dates)
                {
                    subjectGrades.GradesByDate[date] = null;
                }

                // Заполняем оценки для занятий этого предмета
                var subjectLessons = lessons.Where(l => l.ScheduleId == schedule.Id).ToList();
                foreach (var lesson in subjectLessons)
                {
                    if (gradesByLesson.ContainsKey(lesson.Id))
                    {
                        // Берем последнюю оценку за урок (если их несколько)
                        var lastGrade = gradesByLesson[lesson.Id].OrderByDescending(g => g.Id).FirstOrDefault();
                        if (lastGrade != null && subjectGrades.GradesByDate.ContainsKey(lesson.LessonDate))
                        {
                            subjectGrades.GradesByDate[lesson.LessonDate] = lastGrade.GradeValue;
                        }
                    }
                }

                viewModel.SubjectGrades.Add(subjectGrades);
            }

            // Вычисляем общий средний балл
            var allGrades = viewModel.SubjectGrades
                .SelectMany(s => s.GradesByDate.Values)
                .Where(g => g.HasValue)
                .Select(g => g.Value);

            viewModel.OverallAverage = allGrades.Any() ? Math.Round(allGrades.Average(), 2) : 0;
        }

        private async Task LoadFinalGradesData(StudentGradesViewModel viewModel, int groupId)
        {
            if (!viewModel.SelectedAcademicYearId.HasValue)
                return;

            // Получаем выбранный учебный год
            var selectedYear = await _context.AcademicYears
                .FirstOrDefaultAsync(y => y.Id == viewModel.SelectedAcademicYearId);

            if (selectedYear == null) return;

            viewModel.AcademicYearName = $"{selectedYear.StartYear}-{selectedYear.EndYear}";

            // Формируем даты начала и конца учебного года (сентябрь - август)
            var yearStart = new DateTime(selectedYear.StartYear, 9, 1);
            var yearEnd = new DateTime(selectedYear.EndYear, 8, 31);

            // Получаем четверти за этот учебный год
            var quarters = await _context.AcademicQuarters
                .Where(q => q.StartDate >= yearStart && q.EndDate <= yearEnd)
                .OrderBy(q => q.StartDate)
                .ToListAsync();

            // Если не нашли по датам, пробуем по названию
            if (!quarters.Any())
            {
                quarters = await _context.AcademicQuarters
                    .Where(q => q.Name.Contains(selectedYear.StartYear.ToString()) ||
                               q.Name.Contains(selectedYear.EndYear.ToString()))
                    .OrderBy(q => q.StartDate)
                    .ToListAsync();
            }

            // Получаем расписание для группы
            var schedules = await _context.Schedule
                .Include(s => s.Subject)
                .Where(s => s.GroupId == groupId)
                .ToListAsync();

            // Получаем все занятия за год
            var lessons = await _context.Lessons
                .Where(l => l.LessonDate >= yearStart && l.LessonDate <= yearEnd &&
                           schedules.Select(s => s.Id).Contains(l.ScheduleId))
                .ToListAsync();

            // Получаем оценки ученика
            var grades = await _context.Grades
                .Where(g => g.StudentId == viewModel.StudentId &&
                           lessons.Select(l => l.Id).Contains(g.LessonId))
                .ToListAsync();

            viewModel.FinalGrades = new List<SubjectFinalGrades>();

            foreach (var schedule in schedules)
            {
                var finalGrade = new SubjectFinalGrades
                {
                    SubjectId = schedule.Subject?.Id ?? 0,
                    SubjectName = schedule.Subject?.SubjectName ?? "Предмет",
                    QuarterGrades = new Dictionary<int, decimal?>(),
                    QuarterCompleted = new Dictionary<int, bool>()
                };

                // Для каждой четверти вычисляем среднюю оценку
                foreach (var quarter in quarters)
                {
                    // Получаем занятия в этой четверти
                    var lessonIdsInQuarter = lessons
                        .Where(l => l.ScheduleId == schedule.Id &&
                                   l.LessonDate >= quarter.StartDate &&
                                   l.LessonDate <= quarter.EndDate)
                        .Select(l => l.Id)
                        .ToList();

                    // Получаем оценки за эти занятия
                    var quarterGrades = grades
                        .Where(g => lessonIdsInQuarter.Contains(g.LessonId))
                        .Select(g => g.GradeValue)
                        .ToList();

                    // Вычисляем среднюю оценку
                    if (quarterGrades.Any())
                    {
                        finalGrade.QuarterGrades[quarter.Id] = Math.Round((decimal)quarterGrades.Average(), 1);
                    }
                    else
                    {
                        finalGrade.QuarterGrades[quarter.Id] = null;
                    }

                    // Проверяем, завершена ли четверть
                    finalGrade.QuarterCompleted[quarter.Id] = DateTime.Now > quarter.EndDate;
                }

                // Вычисляем годовую оценку
                var validGrades = finalGrade.QuarterGrades.Values.Where(g => g.HasValue).Select(g => g.Value).ToList();
                if (validGrades.Any())
                {
                    finalGrade.YearGrade = Math.Round(validGrades.Average(), 1);
                }

                viewModel.FinalGrades.Add(finalGrade);
            }
        }

        // GET: Student/Homework
        public async Task<IActionResult> Homework(int? weekOffset)
        {
            // Получаем ID текущего ученика
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return NotFound();
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId && u.RoleId == 3);

            if (student == null)
            {
                return NotFound();
            }

            // Определяем смещение недели
            int offset = weekOffset ?? 0;

            // Определяем начало текущей недели (понедельник)
            var today = DateTime.Today;
            var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                monday = today.AddDays(-6);
            }

            var weekStart = monday.AddDays(7 * offset);
            var weekEnd = weekStart.AddDays(6);

            // Получаем группы ученика
            var studentGroups = await _context.GroupMemberships
                .Where(gm => gm.StudentId == studentId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            // Получаем все занятия для групп ученика на выбранную неделю
            var lessons = await _context.Lessons
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Subject)
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Teacher)
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.LessonTime)
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Group)
                .Where(l => studentGroups.Contains(l.Schedule.GroupId) &&
                           l.LessonDate.Date >= weekStart &&
                           l.LessonDate.Date <= weekEnd)
                .OrderBy(l => l.LessonDate)
                .ThenBy(l => l.Schedule.LessonTime.LessonStart)
                .ToListAsync();

            // Получаем домашние задания для этих занятий (по LessonId)
            var lessonIds = lessons.Select(l => l.Id).ToList();
            var homeworks = await _context.Homework
                .Include(h => h.Lesson)
                    .ThenInclude(l => l.Schedule)
                        .ThenInclude(s => s.Subject)
                .Include(h => h.Lesson)
                    .ThenInclude(l => l.Schedule)
                        .ThenInclude(s => s.Teacher)
                .Include(h => h.Lesson)
                    .ThenInclude(l => l.Schedule)
                        .ThenInclude(s => s.Weekday)
                .Where(h => lessonIds.Contains(h.LessonId))
                .ToDictionaryAsync(h => h.LessonId, h => h);

            // Создаем список дней недели
            var weekdays = new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
            var weekDays = new List<StudentWeekDayInfo>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                weekDays.Add(new StudentWeekDayInfo
                {
                    WeekdayId = i + 1,
                    DayName = weekdays[i],
                    Date = date,
                    IsToday = date.Date == today.Date
                });
            }

            var viewModel = new StudentHomeworkViewModel
            {
                Lessons = lessons,
                Homeworks = homeworks,
                WeekStart = weekStart,
                WeekEnd = weekEnd,
                WeekOffset = offset,
                StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
                WeekDays = weekDays
            };

            return View(viewModel);
        }

        // GET: Student/Profile
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return NotFound();
            }

            var student = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == studentId);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Student/Calendar
        public async Task<IActionResult> Calendar()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return NotFound();
            }

            var studentGroups = await _context.GroupMemberships
                .Where(gm => gm.StudentId == studentId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var schedule = await _context.Schedule
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Room)
                .Include(s => s.Teacher)
                .Where(s => studentGroups.Contains(s.GroupId))
                .OrderBy(s => s.WeekdayId)
                .ThenBy(s => s.LessonTime.LessonStart)
                .ToListAsync();

            return View(schedule);
        }
    }
}