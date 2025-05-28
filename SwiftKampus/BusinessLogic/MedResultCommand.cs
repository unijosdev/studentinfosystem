using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;
using SwiftKampus.Models;
using SwiftKampus.ViewModels.MedResultVm;
using System.Linq;

namespace SwiftKampus.BusinessLogic
{
    public class MedResultCommand
    {
        readonly SchoolDbContext _db;
        public MedResultCommand(SchoolDbContext db)
        {
            _db = db;
        }

       
    }
}