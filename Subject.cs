using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("subjects")]
    public class Subject
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("subject_name")]
        [StringLength(255)]
        public string SubjectName { get; set; }

        [Required]
        [Column("is_group")]
        public bool IsGroup { get; set; }
    }
}