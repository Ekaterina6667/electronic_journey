using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MusicSchoolJournal.Models
{
    public class ScheduleViewModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Выберите день недели")]
        [Display(Name = "День недели")]
        public int WeekdayId { get; set; }

        [Required(ErrorMessage = "Выберите время занятия")]
        [Display(Name = "Время")]
        public int LessonTimeId { get; set; }

        [Required(ErrorMessage = "Выберите предмет")]
        [Display(Name = "Предмет")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Выберите учителя")]
        [Display(Name = "Учитель")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Выберите группу")]
        [Display(Name = "Группа")]
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Выберите кабинет")]
        [Display(Name = "Кабинет")]
        public int RoomId { get; set; }

        // Для отображения в списке
        public string? WeekdayName { get; set; }
        public string? LessonTimeDisplay { get; set; }
        public string? SubjectName { get; set; }
        public string? TeacherName { get; set; }
        public string? GroupName { get; set; }
        public int RoomNumber { get; set; }

        // Для выпадающих списков
        public IEnumerable<SelectListItem>? Weekdays { get; set; }
        public IEnumerable<SelectListItem>? LessonTimes { get; set; }
        public IEnumerable<SelectListItem>? Subjects { get; set; }
        public IEnumerable<SelectListItem>? Teachers { get; set; }
        public IEnumerable<SelectListItem>? Groups { get; set; }
        public IEnumerable<SelectListItem>? Rooms { get; set; }
    }
}