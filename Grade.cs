using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("grades")]
    public class Grade
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
        [Column("grade")]
        public int GradeValue { get; set; }

        // Навигационные свойства
        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }

        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }  // ЭТО СВОЙСТВО НУЖНО
    }
}