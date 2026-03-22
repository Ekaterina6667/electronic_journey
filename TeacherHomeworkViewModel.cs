namespace MusicSchoolJournal.Models
{
    public class TeacherHomeworkViewModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public DateTime SelectedDate { get; set; }
        public int SelectedWeekOffset { get; set; }
        public List<WeekDayInfo> WeekDays { get; set; } = new();
        public Dictionary<int, List<LessonHomeworkInfo>> LessonsByDay { get; set; } = new();
    }

    public class WeekDayInfo
    {
        public int WeekdayId { get; set; }
        public string DayName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsToday { get; set; }  // Добавлен set;
        public string DisplayDate => Date.ToString("dd.MM");
    }

    public class LessonHomeworkInfo
    {
        public long LessonId { get; set; }
        public long ScheduleId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string LessonTime { get; set; } = string.Empty;
        public DateTime LessonDate { get; set; }
        public long? HomeworkId { get; set; }
        public string? HomeworkDescription { get; set; }
        public string? HomeworkAttachments { get; set; }
        public bool HasHomework { get; set; }  // Добавлен set;
    }
}