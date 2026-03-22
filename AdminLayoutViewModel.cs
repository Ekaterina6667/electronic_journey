namespace MusicSchoolJournal.Models
{
    public class AdminLayoutViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string CurrentController { get; set; } = string.Empty;
        public string CurrentAction { get; set; } = string.Empty;
    }
}