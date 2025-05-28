using SwiftKampus.Abstractions;
using SwiftKampus.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftKampus.BusinessLogic
{
    public class StaffQueryManager : IStaffQueryManager
    {
        private readonly SchoolDbContext _db;

        public StaffQueryManager(SchoolDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetStaffDepartmentId(string userId)
        {
            return await _db.Staffs.AsNoTracking().Include(i => i.Department)
                .Where(x => x.Email.Equals(userId))
                .Select(s => s.Department.DepartmentId).FirstOrDefaultAsync();
        }
    }
}