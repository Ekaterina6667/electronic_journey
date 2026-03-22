using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("schedule")]
    public class Schedule
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("lesson_time_id")]
        public int LessonTimeId { get; set; }

        [Required]
        [Column("room_id")]
        public int RoomId { get; set; }

        [Required]
        [Column("teacher_id")]
        public int TeacherId { get; set; }

        [Required]
        [Column("weekday_id")]
        public int WeekdayId { get; set; }

        [Required]
        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Required]
        [Column("group_id")]
        public int GroupId { get; set; }

        // Навигационные свойства
        [ForeignKey("LessonTimeId")]
        public virtual LessonTime? LessonTime { get; set; }

        [ForeignKey("RoomId")]
        public virtual Office? Room { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }

        [ForeignKey("WeekdayId")]
        public virtual Weekday? Weekday { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject? Subject { get; set; }

        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }  
       
    }
}
