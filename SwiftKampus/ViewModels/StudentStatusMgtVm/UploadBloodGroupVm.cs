using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SwiftKampus.ViewModels
{
    public class UploadBloodGroupVm
    {
        public string StudentId { get; set; }

        [Required]
        public string BloodGroup { get; set; }
    }
}