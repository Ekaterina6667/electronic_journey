using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("absence_reasons")]
    public class AbsenceReason
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("reason_description")]
        [StringLength(255)]
        public string ReasonDescription { get; set; }

        [Required]
        [Column("is_excused")]
        public bool IsExcused { get; set; }
    }
}