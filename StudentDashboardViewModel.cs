namespace MusicSchoolJournal.Models
{
    public class StudentDashboardViewModel
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
        public int PendingHomeworkCount { get; set; }
        public int NewAnnouncementsCount { get; set; }
        public double AverageGrade { get; set; }

        // Ближайшее занятие
        public string? NextLessonSubject { get; set; }
        public string? NextLessonTime { get; set; }
        public string? NextLessonRoom { get; set; }
        public string? NextLessonTeacher { get; set; }

        // События календаря
        public List<CalendarEvent> CalendarEvents { get; set; } = new();
    }

    public class CalendarEvent
    {
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // lesson, homework, event
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Teacher { get; set; }
    }
}