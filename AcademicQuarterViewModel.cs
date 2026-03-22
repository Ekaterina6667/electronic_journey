using System.ComponentModel.DataAnnotations;

namespace MusicSchoolJournal.Models
{
    public class AcademicQuarterViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название четверти")]
        [Display(Name = "Название четверти")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите дату начала")]
        [Display(Name = "Дата начала")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Выберите дату окончания")]
        [Display(Name = "Дата окончания")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Для отображения в списке
        public bool IsCurrent
        {
            get
            {
                var today = DateTime.Today;
                return today >= StartDate && today <= EndDate;
            }
        }

        public int DurationDays => (EndDate - StartDate).Days;
    }
}