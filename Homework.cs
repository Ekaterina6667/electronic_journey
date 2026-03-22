using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("homework")]
    public class Homework
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("lesson_id")]  // Изменено с schedule_id
        public long LessonId { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("attachments")]
        public string? Attachments { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }
    }
}