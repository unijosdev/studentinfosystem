namespace SwiftKampus.ViewModels
{
    public class AdminDashboardVm
    {
        public int TotalUtmeScreening { get; set; }
        public int RegisteredUtme { get; set; }
        public double RegisteredUtmePercentage { get; set; }
        public int UnRegisteredUtme { get; set; }
        public double UnRegisteredUtmePercentage { get; set; }

        public int TotalDeScreening { get; set; }
        public int RegisteredDe { get; set; }
        public double RegisteredDePercentage { get; set; }
        public int UnRegisteredDe { get; set; }
        public double UnRegisteredDePercentage { get; set; }
        public double AllActiveUGStudents { get; set; }
        public double AllActivePGStudents { get; set; }
        public double AllActiveRemdialStds { get; set; }
        public double AllActivePartTime { get; set; }
    }
}