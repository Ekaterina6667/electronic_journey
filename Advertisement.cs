using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("advertisements")]
    public class Advertisement
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("description")]
        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Required]
        [Column("publication_date")]
        public DateTime PublicationDate { get; set; }

        // Навигационное свойство
        [ForeignKey("AdminId")]
        public virtual User? Admin { get; set; }  // ЭТО СВОЙСТВО
    }
}