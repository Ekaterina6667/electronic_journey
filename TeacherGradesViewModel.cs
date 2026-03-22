//namespace MusicSchoolJournal.Models
//{
//    public class TeacherGradesViewModel
//    {
//        public int TeacherId { get; set; }
//        public string TeacherName { get; set; } = string.Empty;

//        // Фильтры
//        public int SelectedGroupId { get; set; }
//        public int SelectedSubjectId { get; set; }
//        public int SelectedQuarterId { get; set; }

//        // ScheduleId для создания занятий
//        public long ScheduleId { get; set; }

//        // Справочники для фильтров
//        public List<Group> Groups { get; set; } = new();
//        public List<Subject> Subjects { get; set; } = new();
//        public List<AcademicQuarter> Quarters { get; set; } = new();

//        // Данные для таблицы оценок
//        public List<StudentGradeRow> StudentGrades { get; set; } = new();
//        public List<DateTime> Dates { get; set; } = new();

//        // Выбранная группа и предмет
//        public string? GroupName { get; set; }
//        public string? SubjectName { get; set; }
//        public string? QuarterName { get; set; }
//    }
//    public class StudentGradeRow
//    {
//        public int StudentId { get; set; }
//        public string StudentName { get; set; } = string.Empty;

//        // Словарь для хранения списка оценок по датам
//        public Dictionary<DateTime, List<int>> AllGradesByDate { get; set; } = new();

//        // Для обратной совместимости (средний балл)
//        public double AverageGrade
//        {
//            get
//            {
//                var allGrades = AllGradesByDate.Values.SelectMany(g => g);
//                return allGrades.Any() ? Math.Round(allGrades.Average(), 2) : 0;
//            }
//        }

//        public int GradeCount => AllGradesByDate.Values.Sum(g => g.Count);

//        // Для отображения в виде строки
//        public string GetGradesDisplay(DateTime date)
//        {
//            return AllGradesByDate.ContainsKey(date) && AllGradesByDate[date].Any()
//                ? string.Join(", ", AllGradesByDate[date])
//                : "";
//        }
//    }

//}
namespace MusicSchoolJournal.Models
{
    public class TeacherGradesViewModel
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;

        // Фильтры
        public int SelectedGroupId { get; set; }
        public int SelectedSubjectId { get; set; }
        public int SelectedQuarterId { get; set; }
        public int? SelectedAcademicYearId { get; set; } // Новое

        // ScheduleId для создания занятий
        public long ScheduleId { get; set; }

        // Справочники для фильтров
        public List<Group> Groups { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
        public List<AcademicQuarter> Quarters { get; set; } = new();
        public List<AcademicYear> AcademicYears { get; set; } = new(); // Новое

        // Выбранная группа, предмет, четверть, год
        public string? GroupName { get; set; }
        public string? SubjectName { get; set; }
        public string? QuarterName { get; set; }
        public string? AcademicYearName { get; set; } // Новое

        // Для переключения между вкладками
        public bool ShowCurrentGrades { get; set; } = true;

        // Данные для таблицы текущих оценок
        public List<StudentGradeRow> StudentGrades { get; set; } = new();
        public List<DateTime> Dates { get; set; } = new();

        // Данные для таблицы итоговых оценок
        public List<StudentFinalGradeRow> FinalGrades { get; set; } = new();
    }

    public class StudentGradeRow
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;

        // Словарь для хранения списка оценок по датам
        public Dictionary<DateTime, List<int>> AllGradesByDate { get; set; } = new();

        // Для обратной совместимости (средний балл)
        public double AverageGrade
        {
            get
            {
                var allGrades = AllGradesByDate.Values.SelectMany(g => g);
                return allGrades.Any() ? Math.Round(allGrades.Average(), 2) : 0;
            }
        }

        public int GradeCount => AllGradesByDate.Values.Sum(g => g.Count);

        // Для отображения в виде строки
        public string GetGradesDisplay(DateTime date)
        {
            return AllGradesByDate.ContainsKey(date) && AllGradesByDate[date].Any()
                ? string.Join(", ", AllGradesByDate[date])
                : "";
        }
    }

    public class StudentFinalGradeRow
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;

        // Оценки за четверти (ключ - Id четверти)
        public Dictionary<int, decimal?> QuarterGrades { get; set; } = new();

        // Статусы завершения четвертей (ключ - Id четверти)
        public Dictionary<int, bool> QuarterCompleted { get; set; } = new();

        // Годовая оценка
        public decimal? YearGrade { get; set; }

        // Средний балл за все четверти (для отображения)
        public decimal? AverageGrade
        {
            get
            {
                var grades = QuarterGrades.Values.Where(g => g.HasValue).Select(g => g.Value);
                return grades.Any() ? Math.Round(grades.Average(), 2) : null;
            }
        }
    }
}