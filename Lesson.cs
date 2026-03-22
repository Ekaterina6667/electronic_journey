using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("lessons")]
    public class Lesson
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("schedule_id")]
        public long ScheduleId { get; set; }

        [Required]
        [Column("lesson_date", TypeName = "date")] // Указываем тип date
        public DateTime LessonDate { get; set; }

        // Навигационное свойство
        [ForeignKey("ScheduleId")]
        public virtual Schedule? Schedule { get; set; }
    }
}