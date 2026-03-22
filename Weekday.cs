using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("weekdays")]
    public class Weekday
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("day_name")]
        [StringLength(20)]
        public string DayName { get; set; }
    }
}