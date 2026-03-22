namespace MusicSchoolJournal.Models
{
    public class TeacherDashboardViewModel
    {
        // Информация о пользователе
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        // Статистика
        public int TodayLessonsCount { get; set; }
        public int TomorrowLessonsCount { get; set; }
        public int ThisWeekLessonsCount { get; set; }
        public int TotalStudentsCount { get; set; }
        public int PendingHomeworkToCheck { get; set; }
        public int NewAnnouncementsCount { get; set; }

        // Ближайшее занятие
        public string? NextLessonSubject { get; set; }
        public string? NextLessonTime { get; set; }
        public string? NextLessonRoom { get; set; }
        public string? NextLessonGroup { get; set; }
        public int NextLessonStudentsCount { get; set; }

        // Предметы, которые ведет учитель
        public List<string> Subjects { get; set; } = new();

        // Группы, с которыми работает
        public List<TeacherGroupInfo> Groups { get; set; } = new();

        // События календаря
        public List<TeacherCalendarEvent> CalendarEvents { get; set; } = new();
    }

    public class TeacherGroupInfo
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int StudentsCount { get; set; }
    }

    public class TeacherCalendarEvent
    {
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // lesson, homework, event
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Group { get; set; }
        public string? Subject { get; set; }
    }
}