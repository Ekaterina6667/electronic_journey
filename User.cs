using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("first_name")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [Column("last_name")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Column("middle_name")]
        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [Column("login")]
        [StringLength(20)]
        public string Login { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Required]
        [Column("phone_number")]
        [StringLength(12)]
        public string PhoneNumber { get; set; }

        [Required]
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }

        // Новое поле для мягкого удаления
        [Column("is_archived")]
        public bool IsArchived { get; set; } = false;

        [Column("archived_at")]
        public DateTime? ArchivedAt { get; set; }

        // Навигационное свойство для роли
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        // ===== ДОБАВЛЯЕМ ВСЕ НАВИГАЦИОННЫЕ СВОЙСТВА =====

        // Связь с оценками (как ученик)
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

        // Связь с посещаемостью (как ученик)
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        // Связь с членством в группах (как ученик)
        public virtual ICollection<GroupMembership> GroupMemberships { get; set; } = new List<GroupMembership>();

        // Связь со специализацией учителя (как учитель)
        public virtual ICollection<TeacherSpecialization> TeacherSpecializations { get; set; } = new List<TeacherSpecialization>();

        // Связь с расписанием (как учитель)
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

        // Связь с объявлениями (как автор)
        public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
    }
}