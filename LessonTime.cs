using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("lesson_times")]
    public class LessonTime
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("lesson_start")]
        public TimeSpan LessonStart { get; set; }

        [Required]
        [Column("lesson_end")]
        public TimeSpan LessonEnd { get; set; }
    }
}