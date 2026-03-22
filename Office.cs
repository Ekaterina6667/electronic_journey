using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("offices")]
    public class Office
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("room_number")]
        public int RoomNumber { get; set; }

        [Column("description")]
        [StringLength(255)]
        public string? Description { get; set; }
    }
}