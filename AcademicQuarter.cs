using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("academic_quarters")]
    public class AcademicQuarter
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("start_date", TypeName = "date")] // Указываем тип date
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date", TypeName = "date")] // Указываем тип date
        public DateTime EndDate { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }
    }
}