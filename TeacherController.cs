using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchoolJournal.Data;
using MusicSchoolJournal.Models;
using System.Security.Claims;
using System.Linq;

namespace MusicSchoolJournal.Controllers
{
    [Authorize(Roles = "Учитель")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Teacher/Index
        public async Task<IActionResult> Index()
        {
            // Получаем ID текущего учителя
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int teacherId))
            {
                return NotFound();
            }

            var teacher = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == teacherId && u.RoleId == 2);

            if (teacher == null)
            {
                return NotFound();
            }

            // Получаем все расписание учителя
            var teacherSchedule = await _context.Schedule
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Group)
                    .ThenInclude(g => g.GroupMemberships)
                .Include(s => s.Room)
                .Where(s => s.TeacherId == teacherId)
                .ToListAsync();

            var today = DateTime.Today;
            var tomorrow = DateTime.Today.AddDays(1);

            // Определяем день недели для сегодня и завтра (1-Пн, 7-Вс)
            var todayWeekday = (int)today.DayOfWeek + 1;
            var tomorrowWeekday = (int)tomorrow.DayOfWeek + 1;

            // Получаем занятия на сегодня (по дню недели)
            var todayLessons = teacherSchedule
                .Where(s => s.WeekdayId == todayWeekday)
                .ToList();

            // Получаем занятия на завтра (по дню недели)
            var tomorrowLessons = teacherSchedule
                .Where(s => s.WeekdayId == tomorrowWeekday)
                .ToList();

            // Получаем занятия на неделю (все дни недели)
            var weekLessons = teacherSchedule; // Все занятия учителя

            // Получаем предметы учителя
            var teacherSubjects = await _context.TeacherSpecializations
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == teacherId)
                .Select(ts => ts.Subject!.SubjectName)
                .ToListAsync();

            // Получаем группы учителя с количеством учеников
            var teacherGroups = teacherSchedule
                .Select(s => s.Group)
                .Where(g => g != null)
                .Distinct()
                .Select(g => new TeacherGroupInfo
                {
                    GroupId = g!.Id,
                    GroupName = g.GroupName,
                    SubjectName = teacherSchedule.FirstOrDefault(s => s.GroupId == g.Id)?.Subject?.SubjectName ?? "",
                    StudentsCount = g.GroupMemberships?.Count ?? 0
                })
                .ToList();

            // Получаем количество учеников (всех, с которыми работает учитель)
            var studentIds = await _context.GroupMemberships
                .Where(gm => teacherSchedule.Select(s => s.GroupId).Contains(gm.GroupId))
                .Select(gm => gm.StudentId)
                .Distinct()
                .CountAsync();

            // Получаем ближайшее занятие
            Schedule? nextLesson = null;
            for (int i = 0; i < 7; i++)
            {
                var checkDate = today.AddDays(i);
                var weekdayId = (int)checkDate.DayOfWeek + 1;
                var dayLessons = teacherSchedule
                    .Where(s => s.WeekdayId == weekdayId)
                    .OrderBy(s => s.LessonTime!.LessonStart)
                    .ToList();

                if (dayLessons.Any())
                {
                    nextLesson = dayLessons.First();
                    break;
                }
            }

            // Получаем объявления
            var announcements = await _context.Advertisements
                .OrderByDescending(a => a.PublicationDate)
                .Take(3)
                .ToListAsync();

            // Формируем модель
            var viewModel = new TeacherDashboardViewModel
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                MiddleName = teacher.MiddleName,
                TodayLessonsCount = todayLessons.Count,
                TomorrowLessonsCount = tomorrowLessons.Count,
                ThisWeekLessonsCount = weekLessons.Count,
                TotalStudentsCount = studentIds,
                PendingHomeworkToCheck = 0,
                NewAnnouncementsCount = announcements.Count,
                Subjects = teacherSubjects,
                Groups = teacherGroups
            };

            // Ближайшее занятие
            if (nextLesson != null)
            {
                viewModel.NextLessonSubject = nextLesson.Subject?.SubjectName;
                viewModel.NextLessonTime = $"{nextLesson.LessonTime?.LessonStart:hh\\:mm} - {nextLesson.LessonTime?.LessonEnd:hh\\:mm}";
                viewModel.NextLessonRoom = nextLesson.Room?.RoomNumber.ToString();
                viewModel.NextLessonGroup = nextLesson.Group?.GroupName;
                viewModel.NextLessonStudentsCount = nextLesson.Group?.GroupMemberships?.Count ?? 0;
            }

            // События календаря (ближайшие 7 дней)
            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                var weekdayId = (int)date.DayOfWeek + 1;

                // Уроки на этот день
                var lessons = teacherSchedule
                    .Where(s => s.WeekdayId == weekdayId)
                    .OrderBy(s => s.LessonTime!.LessonStart)
                    .ToList();

                foreach (var lesson in lessons)
                {
                    viewModel.CalendarEvents.Add(new TeacherCalendarEvent
                    {
                        Date = date,
                        Title = lesson.Subject?.SubjectName ?? "Занятие",
                        Type = "lesson",
                        Description = $"{lesson.LessonTime?.LessonStart:hh\\:mm} - {lesson.LessonTime?.LessonEnd:hh\\:mm}",
                        Location = $"Каб. {lesson.Room?.RoomNumber}",
                        Group = lesson.Group?.GroupName,
                        Subject = lesson.Subject?.SubjectName
                    });
                }
            }

            return View(viewModel);
        }

        // GET: Teacher/Schedule
        public async Task<IActionResult> Schedule()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int teacherId))
            {
                return NotFound();
            }

            var teacher = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == teacherId && u.RoleId == 2);

            var schedule = await _context.Schedule
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Group)
                    .ThenInclude(g => g.GroupMemberships)  // Добавить эту строку
                .Include(s => s.Room)
                .Where(s => s.TeacherId == teacherId)
                .OrderBy(s => s.WeekdayId)
                .ThenBy(s => s.LessonTime!.LessonStart)
                .ToListAsync();

            // Передаем полное ФИО через ViewBag
            if (teacher != null)
            {
                ViewBag.TeacherFullName = $"{teacher.LastName} {teacher.FirstName} {teacher.MiddleName}".Trim();
            }
            else
            {
                ViewBag.TeacherFullName = "Учитель";
            }

            return View(schedule);
        }




        // GET: Teacher/Profile
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int teacherId))
            {
                return NotFound();
            }

            var teacher = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.TeacherSpecializations)
                    .ThenInclude(ts => ts.Subject)
                .FirstOrDefaultAsync(u => u.Id == teacherId);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teacher/Calendar
        public async Task<IActionResult> Calendar()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int teacherId))
            {
                return NotFound();
            }

            var schedule = await _context.Schedule
                .Include(s => s.Weekday)
                .Include(s => s.LessonTime)
                .Include(s => s.Subject)
                .Include(s => s.Group)
                .Include(s => s.Room)
                .Where(s => s.TeacherId == teacherId)
                .OrderBy(s => s.WeekdayId)
                .ThenBy(s => s.LessonTime!.LessonStart)
                .ToListAsync();

            return View(schedule);
        }
        //// GET: Teacher/Grades
        //// GET: Teacher/Grades
        //public async Task<IActionResult> Grades(int? groupId, int? subjectId, int? quarterId)
        //{
        //    // Получаем ID текущего учителя
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (!int.TryParse(userIdClaim, out int teacherId))
        //    {
        //        return NotFound();
        //    }

        //    var teacher = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Id == teacherId && u.RoleId == 2);

        //    if (teacher == null)
        //    {
        //        return NotFound();
        //    }

        //    // Получаем группы учителя (через расписание)
        //    var teacherGroups = await _context.Schedule
        //        .Where(s => s.TeacherId == teacherId)
        //        .Select(s => s.Group)
        //        .Where(g => g != null)
        //        .Distinct()
        //        .OrderBy(g => g!.GroupName)
        //        .ToListAsync();

        //    // Получаем предметы учителя (через специализацию)
        //    var teacherSubjects = await _context.TeacherSpecializations
        //        .Include(ts => ts.Subject)
        //        .Where(ts => ts.TeacherId == teacherId)
        //        .Select(ts => ts.Subject)
        //        .Where(s => s != null)
        //        .OrderBy(s => s!.SubjectName)
        //        .ToListAsync();

        //    // Получаем четверти
        //    var quarters = await _context.AcademicQuarters
        //        .OrderByDescending(q => q.StartDate)
        //        .ToListAsync();

        //    // Создаем базовую модель
        //    var viewModel = new TeacherGradesViewModel
        //    {
        //        TeacherId = teacher.Id,
        //        TeacherName = $"{teacher.LastName} {teacher.FirstName} {teacher.MiddleName}".Trim(),
        //        Groups = teacherGroups!,
        //        Subjects = teacherSubjects!,
        //        Quarters = quarters
        //    };

        //    // Если нет групп или предметов, возвращаем пустую модель
        //    if (!teacherGroups.Any() || !teacherSubjects.Any() || !quarters.Any())
        //    {
        //        return View(viewModel);
        //    }

        //    // Определяем выбранные фильтры
        //    viewModel.SelectedGroupId = groupId ?? teacherGroups.First()!.Id;
        //    viewModel.SelectedSubjectId = subjectId ?? teacherSubjects.First()!.Id;
        //    viewModel.SelectedQuarterId = quarterId ?? quarters.First().Id;

        //    var selectedGroup = await _context.Groups
        //        .FirstOrDefaultAsync(g => g.Id == viewModel.SelectedGroupId);
        //    var selectedSubject = await _context.Subjects
        //        .FirstOrDefaultAsync(s => s.Id == viewModel.SelectedSubjectId);
        //    var selectedQuarter = await _context.AcademicQuarters
        //        .FirstOrDefaultAsync(q => q.Id == viewModel.SelectedQuarterId);

        //    if (selectedGroup == null || selectedSubject == null || selectedQuarter == null)
        //    {
        //        return View(viewModel);
        //    }

        //    viewModel.GroupName = selectedGroup.GroupName;
        //    viewModel.SubjectName = selectedSubject.SubjectName;
        //    viewModel.QuarterName = selectedQuarter.Name;

        //    // Получаем учеников выбранной группы
        //    var students = await _context.GroupMemberships
        //        .Include(gm => gm.Student)
        //        .Where(gm => gm.GroupId == viewModel.SelectedGroupId && gm.Student != null && !gm.Student.IsArchived)
        //        .Select(gm => gm.Student!)
        //        .OrderBy(s => s.LastName)
        //        .ThenBy(s => s.FirstName)
        //        .ToListAsync();

        //    // Получаем ScheduleId для этой группы и предмета
        //    var schedule = await _context.Schedule
        //        .FirstOrDefaultAsync(s => s.TeacherId == teacherId &&
        //                                  s.GroupId == viewModel.SelectedGroupId &&
        //                                  s.SubjectId == viewModel.SelectedSubjectId);

        //    if (schedule == null)
        //    {
        //        return View(viewModel);
        //    }
        //    viewModel.ScheduleId = (int)schedule.Id;

        //    // Формируем список всех дат в четверти (только дни занятий)
        //    var dates = new List<DateTime>();
        //    for (var date = selectedQuarter.StartDate; date <= selectedQuarter.EndDate; date = date.AddDays(1))
        //    {
        //        // Проверяем, есть ли занятие в этот день недели
        //        var weekdayId = (int)date.DayOfWeek + 1;
        //        var hasLesson = schedule.WeekdayId == weekdayId;

        //        if (hasLesson)
        //        {
        //            dates.Add(date);
        //        }
        //    }
        //    viewModel.Dates = dates;

        //    // Получаем все проведенные занятия для этого расписания
        //    var lessons = await _context.Lessons
        //        .Where(l => l.ScheduleId == schedule.Id)
        //        .ToListAsync();

        //    // Получаем все оценки для этих занятий
        //    var lessonIds = lessons.Select(l => l.Id).ToList();
        //    var grades = await _context.Grades
        //        .Where(g => lessonIds.Contains(g.LessonId))
        //        .ToListAsync();

        //    // Группируем оценки по уроку и ученику для быстрого доступа
        //    var gradesByLessonAndStudent = grades
        //        .GroupBy(g => new { g.LessonId, g.StudentId })
        //        .ToDictionary(
        //            g => (g.Key.LessonId, g.Key.StudentId),
        //            g => g.Select(gr => gr.GradeValue).ToList()
        //        );

        //    // Формируем строки для каждого ученика
        //    foreach (var student in students)
        //    {
        //        var studentRow = new StudentGradeRow
        //        {
        //            StudentId = student.Id,
        //            StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
        //            AllGradesByDate = new Dictionary<DateTime, List<int>>()
        //        };

        //        // Инициализируем все даты пустыми списками
        //        foreach (var date in dates)
        //        {
        //            studentRow.AllGradesByDate[date] = new List<int>();
        //        }

        //        // Заполняем оценки
        //        foreach (var lesson in lessons)
        //        {
        //            var lessonDate = lesson.LessonDate.Date;
        //            if (dates.Contains(lessonDate))
        //            {
        //                var key = (lesson.Id, student.Id);
        //                if (gradesByLessonAndStudent.ContainsKey(key))
        //                {
        //                    studentRow.AllGradesByDate[lessonDate] = gradesByLessonAndStudent[key];
        //                }
        //            }
        //        }

        //        viewModel.StudentGrades.Add(studentRow);
        //    }

        //    // ВАЖНО: возвращаем представление с моделью
        //    return View(viewModel);
        //}
        // GET: Teacher/Grades
        public async Task<IActionResult> Grades(int? groupId, int? subjectId, int? quarterId, int? academicYearId, bool showCurrent = true)
        {
            // Получаем ID текущего учителя
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int teacherId))
            {
                return NotFound();
            }

            var teacher = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == teacherId && u.RoleId == 2);

            if (teacher == null)
            {
                return NotFound();
            }

            // Получаем группы учителя (через расписание)
            var teacherGroups = await _context.Schedule
                .Where(s => s.TeacherId == teacherId)
                .Select(s => s.Group)
                .Where(g => g != null)
                .Distinct()
                .OrderBy(g => g!.GroupName)
                .ToListAsync();

            // Получаем предметы учителя (через специализацию)
            var teacherSubjects = await _context.TeacherSpecializations
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == teacherId)
                .Select(ts => ts.Subject)
                .Where(s => s != null)
                .OrderBy(s => s!.SubjectName)
                .ToListAsync();

            // Получаем четверти - ВАЖНО: используем проекцию чтобы избежать навигационных свойств
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

            // Создаем базовую модель
            var viewModel = new TeacherGradesViewModel
            {
                TeacherId = teacher.Id,
                TeacherName = $"{teacher.LastName} {teacher.FirstName} {teacher.MiddleName}".Trim(),
                Groups = teacherGroups!,
                Subjects = teacherSubjects!,
                Quarters = quarters,
                AcademicYears = academicYears,
                ShowCurrentGrades = showCurrent
            };

            // Если нет групп или предметов, возвращаем пустую модель
            if (!teacherGroups.Any() || !teacherSubjects.Any())
            {
                return View(viewModel);
            }

            if (showCurrent)
            {
                // Для текущих оценок нужна четверть
                if (!quarters.Any())
                {
                    return View(viewModel);
                }

                // Определяем выбранные фильтры для текущих оценок
                viewModel.SelectedGroupId = groupId ?? teacherGroups.First()!.Id;
                viewModel.SelectedSubjectId = subjectId ?? teacherSubjects.First()!.Id;
                viewModel.SelectedQuarterId = quarterId ?? quarters.First().Id;

                var selectedGroup = await _context.Groups
                    .FirstOrDefaultAsync(g => g.Id == viewModel.SelectedGroupId);
                var selectedSubject = await _context.Subjects
                    .FirstOrDefaultAsync(s => s.Id == viewModel.SelectedSubjectId);
                var selectedQuarter = await _context.AcademicQuarters
                    .FirstOrDefaultAsync(q => q.Id == viewModel.SelectedQuarterId);

                if (selectedGroup == null || selectedSubject == null || selectedQuarter == null)
                {
                    return View(viewModel);
                }

                viewModel.GroupName = selectedGroup.GroupName;
                viewModel.SubjectName = selectedSubject.SubjectName;
                viewModel.QuarterName = selectedQuarter.Name;

                await LoadCurrentGradesData(viewModel, teacherId);
            }
            else
            {
                // Для итоговых оценок нужен учебный год
                if (!academicYears.Any())
                {
                    return View(viewModel);
                }

                // Определяем выбранные фильтры для итоговых оценок
                viewModel.SelectedGroupId = groupId ?? teacherGroups.First()!.Id;
                viewModel.SelectedSubjectId = subjectId ?? teacherSubjects.First()!.Id;
                viewModel.SelectedAcademicYearId = academicYearId ?? academicYears.First().Id;

                var selectedGroup = await _context.Groups
                    .FirstOrDefaultAsync(g => g.Id == viewModel.SelectedGroupId);
                var selectedSubject = await _context.Subjects
                    .FirstOrDefaultAsync(s => s.Id == viewModel.SelectedSubjectId);
                var selectedYear = await _context.AcademicYears
                    .FirstOrDefaultAsync(y => y.Id == viewModel.SelectedAcademicYearId);

                if (selectedGroup == null || selectedSubject == null || selectedYear == null)
                {
                    return View(viewModel);
                }

                viewModel.GroupName = selectedGroup.GroupName;
                viewModel.SubjectName = selectedSubject.SubjectName;
                viewModel.AcademicYearName = $"{selectedYear.StartYear}-{selectedYear.EndYear}";

                await LoadFinalGradesData(viewModel, teacherId);
            }

            return View(viewModel);
        }

        private async Task LoadCurrentGradesData(TeacherGradesViewModel viewModel, int teacherId)
        {
            // Получаем расписание
            var schedule = await _context.Schedule
                .FirstOrDefaultAsync(s => s.TeacherId == teacherId &&
                                          s.GroupId == viewModel.SelectedGroupId &&
                                          s.SubjectId == viewModel.SelectedSubjectId);

            if (schedule == null)
            {
                return;
            }

            viewModel.ScheduleId = (int)schedule.Id;

            // Получаем выбранную четверть
            var selectedQuarter = await _context.AcademicQuarters
                .FirstOrDefaultAsync(q => q.Id == viewModel.SelectedQuarterId);

            if (selectedQuarter == null) return;

            // Получаем учеников выбранной группы
            var students = await _context.GroupMemberships
                .Include(gm => gm.Student)
                .Where(gm => gm.GroupId == viewModel.SelectedGroupId && gm.Student != null && !gm.Student.IsArchived)
                .Select(gm => gm.Student!)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            // Формируем список всех дат в четверти (только дни занятий)
            var dates = new List<DateTime>();
            for (var date = selectedQuarter.StartDate; date <= selectedQuarter.EndDate; date = date.AddDays(1))
            {
                // Проверяем, есть ли занятие в этот день недели
                var weekdayId = (int)date.DayOfWeek + 1;
                var hasLesson = schedule.WeekdayId == weekdayId;

                if (hasLesson)
                {
                    dates.Add(date);
                }
            }
            viewModel.Dates = dates;

            // Получаем все проведенные занятия для этого расписания
            var lessons = await _context.Lessons
                .Where(l => l.ScheduleId == schedule.Id)
                .ToListAsync();

            // Получаем все оценки для этих занятий
            var lessonIds = lessons.Select(l => l.Id).ToList();
            var grades = await _context.Grades
                .Where(g => lessonIds.Contains(g.LessonId))
                .ToListAsync();

            // Группируем оценки по уроку и ученику для быстрого доступа
            var gradesByLessonAndStudent = grades
                .GroupBy(g => new { g.LessonId, g.StudentId })
                .ToDictionary(
                    g => (g.Key.LessonId, g.Key.StudentId),
                    g => g.Select(gr => gr.GradeValue).ToList()
                );

            // Формируем строки для каждого ученика
            foreach (var student in students)
            {
                var studentRow = new StudentGradeRow
                {
                    StudentId = student.Id,
                    StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
                    AllGradesByDate = new Dictionary<DateTime, List<int>>()
                };

                // Инициализируем все даты пустыми списками
                foreach (var date in dates)
                {
                    studentRow.AllGradesByDate[date] = new List<int>();
                }

                // Заполняем оценки
                foreach (var lesson in lessons)
                {
                    var lessonDate = lesson.LessonDate.Date;
                    if (dates.Contains(lessonDate))
                    {
                        var key = (lesson.Id, student.Id);
                        if (gradesByLessonAndStudent.ContainsKey(key))
                        {
                            studentRow.AllGradesByDate[lessonDate] = gradesByLessonAndStudent[key];
                        }
                    }
                }

                viewModel.StudentGrades.Add(studentRow);
            }
        }

        private async Task LoadFinalGradesData(TeacherGradesViewModel viewModel, int teacherId)
        {
            if (!viewModel.SelectedAcademicYearId.HasValue)
                return;

            // Получаем выбранный учебный год
            var selectedYear = await _context.AcademicYears
                .FirstOrDefaultAsync(y => y.Id == viewModel.SelectedAcademicYearId);

            if (selectedYear == null) return;

            // Формируем даты начала и конца учебного года (сентябрь - август)
            var yearStart = new DateTime(selectedYear.StartYear, 9, 1);
            var yearEnd = new DateTime(selectedYear.EndYear, 8, 31);

            // Получаем четверти, которые попадают в этот учебный год по датам
            var quarters = await _context.AcademicQuarters
                .Where(q => q.StartDate >= yearStart && q.EndDate <= yearEnd)
                .OrderBy(q => q.StartDate)
                .ToListAsync();

            // Если не нашли по датам, пробуем найти по названию (для обратной совместимости)
            if (!quarters.Any())
            {
                quarters = await _context.AcademicQuarters
                    .Where(q => q.Name.Contains(selectedYear.StartYear.ToString()) ||
                               q.Name.Contains(selectedYear.EndYear.ToString()))
                    .OrderBy(q => q.StartDate)
                    .ToListAsync();
            }

            // Получаем учеников группы
            var students = await _context.GroupMemberships
                .Include(gm => gm.Student)
                .Where(gm => gm.GroupId == viewModel.SelectedGroupId && gm.Student != null && !gm.Student.IsArchived)
                .Select(gm => gm.Student!)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            // Получаем расписание
            var schedule = await _context.Schedule
                .FirstOrDefaultAsync(s => s.TeacherId == teacherId &&
                                          s.GroupId == viewModel.SelectedGroupId &&
                                          s.SubjectId == viewModel.SelectedSubjectId);

            if (schedule == null)
                return;

            // Получаем все занятия по этому расписанию
            var lessons = await _context.Lessons
                .Where(l => l.ScheduleId == schedule.Id)
                .ToListAsync();

            var lessonIds = lessons.Select(l => l.Id).ToList();

            // Получаем все оценки
            var grades = await _context.Grades
                .Where(g => lessonIds.Contains(g.LessonId))
                .ToListAsync();

            viewModel.FinalGrades = new List<StudentFinalGradeRow>();

            foreach (var student in students)
            {
                var finalGradeRow = new StudentFinalGradeRow
                {
                    StudentId = student.Id,
                    StudentName = $"{student.LastName} {student.FirstName} {student.MiddleName}".Trim(),
                    QuarterGrades = new Dictionary<int, decimal?>(),
                    QuarterCompleted = new Dictionary<int, bool>()
                };

                // Для каждой четверти вычисляем среднюю оценку
                foreach (var quarter in quarters)
                {
                    // Получаем занятия в этой четверти
                    var lessonIdsInQuarter = lessons
                        .Where(l => l.LessonDate.Date >= quarter.StartDate &&
                                   l.LessonDate.Date <= quarter.EndDate)
                        .Select(l => l.Id)
                        .ToList();

                    // Получаем оценки ученика за эти занятия
                    var studentGrades = grades
                        .Where(g => g.StudentId == student.Id &&
                                   lessonIdsInQuarter.Contains(g.LessonId))
                        .Select(g => g.GradeValue)
                        .ToList();

                    // Вычисляем среднюю оценку за четверть
                    if (studentGrades.Any())
                    {
                        finalGradeRow.QuarterGrades[quarter.Id] = Math.Round((decimal)studentGrades.Average(), 1);
                    }
                    else
                    {
                        finalGradeRow.QuarterGrades[quarter.Id] = null;
                    }

                    // Проверяем, завершена ли четверть (текущая дата больше даты окончания четверти)
                    finalGradeRow.QuarterCompleted[quarter.Id] = DateTime.Now > quarter.EndDate;
                }

                // Вычисляем годовую оценку (среднее арифметическое оценок за четверти)
                var validQuarterGrades = finalGradeRow.QuarterGrades.Values.Where(g => g.HasValue).Select(g => g.Value).ToList();
                if (validQuarterGrades.Any())
                {
                    finalGradeRow.YearGrade = Math.Round(validQuarterGrades.Average(), 1);
                }

                viewModel.FinalGrades.Add(finalGradeRow);
            }
        }
        // GET: Teacher/GetOrCreateLesson
        public async Task<IActionResult> GetOrCreateLesson(long scheduleId, DateTime date) // Измените int на long
        {
            try
            {
                // Проверяем, существует ли уже занятие
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(l => l.ScheduleId == scheduleId && l.LessonDate.Date == date.Date);

                if (lesson == null)
                {
                    // Если занятия еще нет в базе, создаем его
                    lesson = new Lesson
                    {
                        ScheduleId = scheduleId,
                        LessonDate = date
                    };
                    _context.Add(lesson);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Создано новое занятие: LessonId={lesson.Id}, ScheduleId={scheduleId}, Date={date}");
                }
                else
                {
                    Console.WriteLine($"Найдено существующее занятие: LessonId={lesson.Id}, ScheduleId={scheduleId}, Date={date}");
                }

                return Json(new { success = true, lessonId = lesson.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании занятия: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
       
        // POST: Teacher/SaveGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrade(int studentId, long lessonId, int gradeValue)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int teacherId))
                {
                    return Json(new { success = false, message = "Пользователь не найден" });
                }

                var lesson = await _context.Lessons
                    .Include(l => l.Schedule)
                    .FirstOrDefaultAsync(l => l.Id == lessonId);

                if (lesson == null)
                {
                    return Json(new { success = false, message = "Занятие не найдено" });
                }

                if (lesson.Schedule.TeacherId != teacherId)
                {
                    return Json(new { success = false, message = "Нет прав для выставления оценки" });
                }

                if (gradeValue < 1 || gradeValue > 5)
                {
                    return Json(new { success = false, message = "Оценка должна быть от 1 до 5" });
                }

                // Всегда создаем новую оценку (не заменяем существующие)
                var newGrade = new Grade
                {
                    LessonId = lessonId,
                    StudentId = studentId,
                    GradeValue = gradeValue
                };

                _context.Add(newGrade);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Оценка сохранена" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // Модель для приема данных
        public class GradeSaveModel
        {
            public int StudentId { get; set; }
            public long LessonId { get; set; }
            public int GradeValue { get; set; }
        }
        // GET: Teacher/GetStudentGrades (для отображения всех оценок ученика за занятие)
        public async Task<IActionResult> GetStudentGrades(int studentId, long lessonId)
        {
            try
            {
                var grades = await _context.Grades
                    .Where(g => g.StudentId == studentId && g.LessonId == lessonId)
                    .OrderByDescending(g => g.Id)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    grades = grades.Select(g => new { g.Id, g.GradeValue })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Teacher/DeleteGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGrade(int gradeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int teacherId))
                {
                    return Json(new { success = false, message = "Пользователь не найден" });
                }

                var grade = await _context.Grades
                    .Include(g => g.Lesson)
                        .ThenInclude(l => l.Schedule)
                    .FirstOrDefaultAsync(g => g.Id == gradeId);

                if (grade == null)
                {
                    return Json(new { success = false, message = "Оценка не найдена" });
                }

                // Проверяем, что учитель имеет право удалять оценку
                if (grade.Lesson.Schedule.TeacherId != teacherId)
                {
                    return Json(new { success = false, message = "Нет прав для удаления оценки" });
                }

                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Оценка удалена" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // GET: Teacher/Homework
        public async Task<IActionResult> Homework(int? weekOffset)
        {
            // Получаем ID текущего учителя
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int teacherId))
            {
                return NotFound();
            }

            var teacher = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == teacherId && u.RoleId == 2);

            if (teacher == null)
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

            // Получаем все дни недели
            var weekDays = new List<WeekDayInfo>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                weekDays.Add(new WeekDayInfo
                {
                    WeekdayId = i + 1,
                    DayName = GetDayName(i + 1),
                    Date = date,
                    IsToday = date.Date == today.Date
                });
            }

            // ПОЛУЧАЕМ ВСЕ ЗАНЯТИЯ ИЗ РАСПИСАНИЯ УЧИТЕЛЯ (базовое расписание)
            var teacherSchedule = await _context.Schedule
                .Include(s => s.Subject)
                .Include(s => s.Group)
                .Include(s => s.LessonTime)
                .Where(s => s.TeacherId == teacherId)
                .ToListAsync();

            // Получаем уже проведенные занятия (lessons) учителя за выбранную неделю
            var existingLessons = await _context.Lessons
                .Include(l => l.Schedule)
                .Where(l => l.Schedule.TeacherId == teacherId &&
                           l.LessonDate.Date >= weekStart &&
                           l.LessonDate.Date <= weekEnd)
                .ToListAsync();

            // Получаем все домашние задания для проведенных занятий
            var lessonIds = existingLessons.Select(l => l.Id).ToList();
            var homeworks = await _context.Homework
                .Where(h => lessonIds.Contains(h.LessonId))
                .ToDictionaryAsync(h => h.LessonId, h => h);

            // Группируем занятия по дням (НА ОСНОВЕ РАСПИСАНИЯ, а не проведенных занятий)
            var lessonsByDay = new Dictionary<int, List<LessonHomeworkInfo>>();

            foreach (var day in weekDays)
            {
                // Получаем занятия из РАСПИСАНИЯ на этот день недели
                var scheduleForDay = teacherSchedule
                    .Where(s => s.WeekdayId == day.WeekdayId)
                    .OrderBy(s => s.LessonTime.LessonStart)
                    .ToList();

                var lessonInfos = new List<LessonHomeworkInfo>();

                foreach (var schedule in scheduleForDay)
                {
                    // Проверяем, есть ли уже проведенное занятие на эту дату
                    var existingLesson = existingLessons.FirstOrDefault(l =>
                        l.ScheduleId == schedule.Id &&
                        l.LessonDate.Date == day.Date);

                    long? lessonId = existingLesson?.Id;
                    Homework? homework = null;

                    // Если занятие проведено, ищем ДЗ для него
                    if (lessonId.HasValue && homeworks.ContainsKey(lessonId.Value))
                    {
                        homework = homeworks[lessonId.Value];
                    }

                    lessonInfos.Add(new LessonHomeworkInfo
                    {
                        LessonId = lessonId ?? 0,
                        ScheduleId = schedule.Id,
                        SubjectName = schedule.Subject?.SubjectName ?? "",
                        GroupName = schedule.Group?.GroupName ?? "",
                        LessonTime = schedule.LessonTime != null
                            ? $"{schedule.LessonTime.LessonStart:hh\\:mm} - {schedule.LessonTime.LessonEnd:hh\\:mm}"
                            : "",
                        LessonDate = day.Date,
                        HomeworkId = homework?.Id,
                        HomeworkDescription = homework?.Description,
                        HomeworkAttachments = homework?.Attachments,
                        HasHomework = homework != null
                    });
                }

                lessonsByDay[day.WeekdayId] = lessonInfos;
            }

            var viewModel = new TeacherHomeworkViewModel
            {
                TeacherId = teacher.Id,
                TeacherName = $"{teacher.LastName} {teacher.FirstName} {teacher.MiddleName}".Trim(),
                SelectedDate = weekStart,
                SelectedWeekOffset = offset,
                WeekDays = weekDays,
                LessonsByDay = lessonsByDay
            };

            return View(viewModel);
        }
        // Вспомогательный метод для получения названия дня
        private string GetDayName(int weekdayId)
        {
            return weekdayId switch
            {
                1 => "Понедельник",
                2 => "Вторник",
                3 => "Среда",
                4 => "Четверг",
                5 => "Пятница",
                6 => "Суббота",
                7 => "Воскресенье",
                _ => ""
            };
        }
        
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // POST: Teacher/CreateHomework
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHomework(long lessonId, string description, string? attachments)
        {
            try
            {
                // Получаем занятие, чтобы проверить права учителя
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int teacherId))
                {
                    return Json(new { success = false, message = "Пользователь не найден" });
                }

                var lesson = await _context.Lessons
                    .Include(l => l.Schedule)
                    .FirstOrDefaultAsync(l => l.Id == lessonId);

                if (lesson == null)
                {
                    return Json(new { success = false, message = "Занятие не найдено" });
                }

                if (lesson.Schedule.TeacherId != teacherId)
                {
                    return Json(new { success = false, message = "Нет прав для этого занятия" });
                }

                var homework = new Homework
                {
                    LessonId = lessonId,  // Используем LessonId вместо ScheduleId
                    Description = description,
                    Attachments = attachments,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Add(homework);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Домашнее задание добавлено" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // POST: Teacher/DeleteHomework
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHomework(long homeworkId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int teacherId))
                {
                    return Json(new { success = false, message = "Пользователь не найден" });
                }

                var homework = await _context.Homework
                    .Include(h => h.Lesson)  // Include Lesson, не Schedule
                        .ThenInclude(l => l.Schedule)
                    .FirstOrDefaultAsync(h => h.Id == homeworkId);

                if (homework == null)
                {
                    return Json(new { success = false, message = "Домашнее задание не найдено" });
                }

                // Проверяем, что это задание принадлежит учителю
                if (homework.Lesson.Schedule.TeacherId != teacherId)
                {
                    return Json(new { success = false, message = "Нет прав для удаления" });
                }

                _context.Homework.Remove(homework);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Домашнее задание удалено" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Teacher/GetScheduleForFilters
        public async Task<IActionResult> GetScheduleForFilters(int groupId, int subjectId, int weekdayId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int teacherId))
            {
                return Json(new { success = false, message = "Учитель не найден" });
            }

            var schedule = await _context.Schedule
                .Include(s => s.LessonTime)
                .FirstOrDefaultAsync(s => s.TeacherId == teacherId &&
                                          s.GroupId == groupId &&
                                          s.SubjectId == subjectId &&
                                          s.WeekdayId == weekdayId);

            if (schedule == null)
            {
                return Json(new { success = false, message = "Расписание не найдено" });
            }

            // Явное приведение long к int
            return Json(new
            {
                success = true,
                scheduleId = (int)schedule.Id,  // Добавлено (int)
                time = schedule.LessonTime != null
                    ? $"{schedule.LessonTime.LessonStart:hh\\:mm} - {schedule.LessonTime.LessonEnd:hh\\:mm}"
                    : ""
            });
        }
        // POST: Teacher/SaveHomework
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHomework(long lessonId, string description, string attachments)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int teacherId))
                {
                    return Json(new { success = false, message = "Пользователь не найден" });
                }

                // Получаем занятие
                var lesson = await _context.Lessons
                    .Include(l => l.Schedule)
                    .FirstOrDefaultAsync(l => l.Id == lessonId);

                if (lesson == null)
                {
                    return Json(new { success = false, message = "Занятие не найдено" });
                }

                // Проверяем, что это занятие принадлежит учителю
                if (lesson.Schedule.TeacherId != teacherId)
                {
                    return Json(new { success = false, message = "Нет прав для этого занятия" });
                }

                // Ищем существующее домашнее задание для этого занятия
                var existingHomework = await _context.Homework
                    .FirstOrDefaultAsync(h => h.LessonId == lessonId);  // Поиск по LessonId

                if (existingHomework != null)
                {
                    // Обновляем существующее
                    existingHomework.Description = description;
                    existingHomework.Attachments = attachments;
                    _context.Update(existingHomework);
                }
                else
                {
                    // Создаем новое
                    var homework = new Homework
                    {
                        LessonId = lessonId,  // Привязываем к конкретному занятию
                        Description = description,
                        Attachments = attachments,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Add(homework);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Домашнее задание сохранено"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Teacher/GetHomework
        public async Task<IActionResult> GetHomework(long lessonId)  // Измените int на long
        {
            try
            {
                var homework = await _context.Homework
                    .FirstOrDefaultAsync(h => h.LessonId == lessonId);  // Ищем по LessonId

                if (homework == null)
                {
                    return Json(new { success = false, message = "Домашнее задание не найдено" });
                }

                return Json(new
                {
                    success = true,
                    description = homework.Description,
                    attachments = homework.Attachments,
                    homeworkId = homework.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}