using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwiftKampus.Abstractions
{
    public interface IQueryCommand
    {
        //bool CheckForFirstSemester();
        int ConvertToKobo(int value);
        int ConvertToNaira(int value);
        void Dispose();
        int GetStudentSchoolProgramme(string userId);
        int GetStudentProgramme(string userId);
        Task<double> GetAdmissionGradePoint(string grade);
        Semester GetCurrentSemester(int schoolProgrammeId);
        List<Semester> GetCurrentSemesterList(int schoolProgrammeId);
        int GetCurrentSemesterId(int schoolProgrammeId);
        string GetCurrentSemesterName(int schoolProgrammeId);
        Session GetCurrentSession(int schoolProgrammeId);
        List<Session> GetCurrentSessionList(int schoolProgrammeId);
        int GetCurrentSessionId(int schoolProgrammeId);
        int GetCurrentProgrammeSessionId(int ProgrammeId);
        int GetLevelByName(string levelName);
        string GetNextLevel(string levelName);
        Level GetLevelById(int levelId);
        string GetCurrentSessionName(int schoolProgrammeId);
        string GetSessionName(int sessionId);
        string GetId();
        BaseVm GetPaymentStatus(int sessionId);
        Task<List<UnderGraduateRule>> GetUnderGraduateRule(int programmeId, int schoolProgrammeId);
        Task<List<UnderGraduateRule>> GetUnderGraduateRule(string userId);
        Task<LoginDetailVm> GetUserDetails(string userId);
        Task<string> GetUserFullName(string userId);
        string HashRemitaRequest(string merchantId, string serviceTypeId, string orderId, string amount, string responseUrl, string apiKey);
        string HashRemitedRePost(string merchantId, string rrr, string apiKey);
        string HashRemitedValidate(string orderID, string apiKey, string merchantId);
        string HashRrrQuery(string rrr, string apiKey, string merchantId);
        void UpdateTransactionLog(RemitaPaymentLog log, RemitaResponse result);
        Task<Tuple<int, double, int>> UserActivityStatistic();
        Task<SchoolProgramme> GetUndergraduateCurrentSession();
        int GetApplicantStudentId(string userId);
        byte[] MapUtmeDePicture(string jambNumber);
    }
}