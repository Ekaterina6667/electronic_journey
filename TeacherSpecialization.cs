using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("teacher_specializations")]
    public class TeacherSpecialization
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("teacher_id")]
        public int TeacherId { get; set; }

        [Required]
        [Column("subject_id")]
        public int SubjectId { get; set; }

        // Навигационные свойства
        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }  // ЭТО СВОЙСТВО

        [ForeignKey("SubjectId")]
        public virtual Subject? Subject { get; set; }
    }
}