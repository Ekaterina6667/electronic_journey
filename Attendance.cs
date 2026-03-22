using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("attendance")]
    public class Attendance
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("lesson_id")]
        public long LessonId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [Column("is_present")]
        public bool IsPresent { get; set; }

        [Column("absence_reason_id")]
        public int? AbsenceReasonId { get; set; }

        // Навигационные свойства
        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }

        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; } 

        [ForeignKey("AbsenceReasonId")]
        public virtual AbsenceReason? AbsenceReason { get; set; }
    }
}
