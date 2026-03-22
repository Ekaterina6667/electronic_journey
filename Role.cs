using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("role_name")]
        [StringLength(100)]
        public string RoleName { get; set; }

        // Навигационное свойство (связь с пользователями)
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}