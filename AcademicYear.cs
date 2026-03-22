using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicSchoolJournal.Models
{
    [Table("academic_yards")]  // Имя таблицы как в БД
    public class AcademicYear
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("start_year")]  // Имя колонки как в БД
        [Display(Name = "Год начала")]
        public int StartYear { get; set; }

        [Required]
        [Column("end_year")]    // Имя колонки как в БД
        [Display(Name = "Год окончания")]
        public int EndYear { get; set; }

        [Display(Name = "Учебный год")]
        [NotMapped]  // Это свойство не хранится в БД
        public string Name => $"{StartYear}-{EndYear}";
    }
}