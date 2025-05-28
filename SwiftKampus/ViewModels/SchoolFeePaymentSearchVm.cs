
using System;
using System.ComponentModel.DataAnnotations;

namespace SwiftKampus.ViewModels
{
    public class SchoolFeePaymentSearchVm
    {
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}