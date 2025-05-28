using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampusModel;
using SwiftKampusModel.Payment;

namespace SwiftKampus.Abstractions
{
    public interface IFeeQueryManager
    {
        decimal GetFeeAmount(SchoolFeePaymentVm model, List<FeeList> feeList, PaymentSetting paymentSetting);
        Task<List<FeeList>> GetLatePaymentFeeList(int sessionId, int schoolProgrammeId, string feeType, DateTime? checkDate);
        Task<PaymentSetting> GetPaymentSetting(int sessionId, int schoolProgrammeId, string studentStatus);
        Task<PaymentSetting> GetPaymentSetting(int sessionId, int schoolProgrammeId);
        Task<List<FeeList>> GetSchoolFeeList(string feeCategory, Student student, PaymentSetting paymentSetting, int sessionId);
        Task<List<FeeList>> GetSchoolFeeListReport(string feeCategory, int SchoolProgrammeId, int sessionId, string studentType);
        Task<List<FeeList>> GetSchoolFeeListByFaculty(string feeCategory, Student student, PaymentSetting paymentSetting, int sessionId, int facultyId);
        Task<List<SchoolFeePayment>> GetSchoolFeePaymentList(string feeCategory, int sessionId);
        //Task<List<FeeList>> GetDepartmentPaymentList(int deptId, int sessionId);
        Task<List<FeeList>> GetDepartmentPaymentList(int deptId, int sessionId, int levelId);
        string GetServiceType(string feeCategory, string schoolProgrammeCode);
    }
}