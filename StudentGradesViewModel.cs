namespace MusicSchoolJournal.Models
{
    public class StudentGradesViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;

        // Выбранная четверть (для текущих оценок)
        public int SelectedQuarterId { get; set; }
        public string QuarterName { get; set; } = string.Empty;
        public List<AcademicQuarter> Quarters { get; set; } = new();

        public int? SelectedAcademicYearId { get; set; }
        public string? AcademicYearName { get; set; }
        public List<AcademicYear> AcademicYears { get; set; } = new();

        // Даты в четверти (все дни от начала до конца) - для текущих оценок
        public List<DateTime> Dates { get; set; } = new();

        // Оценки по предметам - для текущих оценок
        public List<SubjectGrades> SubjectGrades { get; set; } = new();

        // НОВОЕ: Итоговые оценки по четвертям
        public List<SubjectFinalGrades> FinalGrades { get; set; } = new();

        // Средний балл по всем предметам
        public double OverallAverage { get; set; }
    }

    public class SubjectGrades
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;

        // Оценки по датам (ключ - дата, значение - оценка)
        public Dictionary<DateTime, int?> GradesByDate { get; set; } = new();

        // Средний балл по предмету
        public double AverageGrade
        {
            get
            {
                var validGrades = GradesByDate.Values.Where(g => g.HasValue).Select(g => g.Value);
                return validGrades.Any() ? Math.Round(validGrades.Average(), 2) : 0;
            }
        }

        // Количество оценок
        public int GradeCount => GradesByDate.Values.Count(g => g.HasValue);
    }

    public class SubjectFinalGrades
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;

        // Оценки за четверти (ключ - Id четверти)
        public Dictionary<int, decimal?> QuarterGrades { get; set; } = new();

        // Статусы завершения четвертей (ключ - Id четверти)
        public Dictionary<int, bool> QuarterCompleted { get; set; } = new();

        // Годовая оценка
        public decimal? YearGrade { get; set; }
    }
}
