namespace MusicSchoolJournal.Models
{
    public class StudentHomeworkViewModel
    {
        public List<Lesson> Lessons { get; set; } = new();
        public Dictionary<long, Homework> Homeworks { get; set; } = new();
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int WeekOffset { get; set; }
        public string StudentName { get; set; } = string.Empty;

        // Для отображения дней недели
        public List<StudentWeekDayInfo> WeekDays { get; set; } = new();
    }

    public class StudentWeekDayInfo
    {
        public int WeekdayId { get; set; }
        public string DayName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsToday { get; set; }
        public string DisplayDate => Date.ToString("dd.MM");
    }
}