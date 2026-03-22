using System.ComponentModel.DataAnnotations;

namespace MusicSchoolJournal.Models
{
    public class GroupViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название группы")]
        [Display(Name = "Название группы")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string GroupName { get; set; } = string.Empty;

        [Display(Name = "Ученики в группе")]
        public List<int> SelectedStudentIds { get; set; } = new List<int>();

        // Для отображения в списке
        public int StudentCount { get; set; }
        public string StudentNames { get; set; } = string.Empty;
    }
}