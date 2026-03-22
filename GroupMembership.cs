using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("group_membership")]
    public class GroupMembership
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("group_id")]
        public int GroupId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        // Навигационные свойства
        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }

        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }
    }
}