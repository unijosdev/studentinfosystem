using System.Threading.Tasks;

namespace SwiftKampus.Abstractions
{
    public interface IStaffQueryManager
    {
        Task<int> GetStaffDepartmentId(string userId);
    }
}