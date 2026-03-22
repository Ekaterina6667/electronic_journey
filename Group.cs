using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("groups")]
    public class Group
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("group_name")]
        [StringLength(100)]
        public string GroupName { get; set; }

        // Навигационное свойство для связи с GroupMembership
        public virtual ICollection<GroupMembership> GroupMemberships { get; set; } = new List<GroupMembership>();

        // Навигационное свойство для связи с расписанием
        public virtual ICollection<Schedule> Schedule { get; set; } = new List<Schedule>();
    }
}