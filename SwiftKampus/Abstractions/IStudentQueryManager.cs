using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;

namespace SwiftKampus.Abstractions
{
    public interface IStudentQueryManager
    {
        string GenerateMatricNo(SchoolProgramme model, int year, string facultyCode, int newNumber);
        Task<List<StudentIndexVM>> GetStudentAcademicList(int? schoolProgrammeId, int? facultyId, bool isCleared);
        Task<List<StudentIndexVM>> GetStudentAcademicList(int? schoolProgrammeId, int? facultyId, bool isCleared, int? SessionId);
        Task<List<StudentIndexVM>> GetStudentDeptList(int? schoolProgrammeId, int? deptId, bool isCleared, int? SessionId);
        Task<List<StudentIndexVM>> GetStudentDeptList(int schoolProgrammeId, int? programmeId, int? deptId, int? facultyId, bool isCleared, int sessionId, string ClearanceStage);
        Task<List<StudentIndexVM>> GetStudentfacultyList(int? schoolProgrammeId, int? facultyId, bool isCleared, int? SessionId);
        string GetStudentId(string email);
        Student GetStudent(string email);
        Applicant GetApplicant(string email);
        Student GetStudentByMatNumber(string email);
        Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId, bool activeStudent);
        Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId, int? facultyId, int? departmentId,
                                   int? programmeId, int? SessionId, int? LevelId);
        Task<List<StudentIndexVM>> GetStudentListFor365EmailUpload(int? schoolProgrammeId, int? facultyId, int? departmentId,
                                   int? programmeId, int? SessionId, int? LevelId, string isDownloded);
        Task<List<LibraryPatronUploadVm>> GetStudentListForLibraryUpload(int? schoolProgrammeId, int? facultyId, int? departmentId,
                                   int? programmeId, int? SessionId, int? LevelId);
        Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId, bool activeStudent, int? FacultyId, int? DepartmentId, int? ProgrammeId,
                                                string ClearanceStage, int? SessionId, int? levelId, string gender, string StateOfOrigin);
        Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId, bool activeStudent, int? FacultyId, int? DepartmentId, int? ProgrammeId, 
                                                string ClearanceStage, int? SessionId, int? levelId);
        Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId);
        Task<string> GetUserName(string studentId);
        Task<Student> SaveAndGenerateMatricNo(string studentId);
        //Task UpdateStudentRecord(string schoofeepayment, string studentId);
    }
}