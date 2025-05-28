using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels
{
    public class CalculateCgpaVm
    {
        public double Score { get; set; }

        [Range(1, 5)]
        public int Credit { get; set; }


    }

    public class CgpaData
    {
        public int unit { get; set; }
        public string score { get; set; }
    }
}