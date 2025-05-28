using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NSubstitute;
using NUnit.Framework;
using SwiftKampus.Abstractions;
using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampusModel;
using SwiftKampusModel.Accomodation;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.CourseForum;
using SwiftKampusModel.Library;
using SwiftKampusModel.Payment;

namespace Unijos.Tests.BusinessLogic
{
    [TestFixture]
    public class FeeQueryManagerTests
    {
        private readonly IFeeQueryManager _feeCmd;

        public FeeQueryManagerTests()
        {
            _feeCmd = Substitute.For<IFeeQueryManager>();
        }

        [Test]
        //public async Task GetSchoolFeeList_ReturnsAListOfSchoolFees()
        //{
        //    //Arrange
        //    var paySetting = Builder<PaymentSetting>.CreateNew()
        //        .With(p => p.Session, new Session()).Build();

        //    var student = Builder<Student>.CreateNew()
        //        .With(s => s.AccommodationFeePayments, new List<AccommodationFeePayment>())
        //        .With(s => s.BookIssues, new List<BookIssue>())
        //        .With(s => s.CourseRegistrations, new List<CourseRegistration>())
        //        .With(s => s.DepartmentFeePayments, new List<DepartmentFeePayment>())
        //        .With(s => s.Enrollments, new List<Enrollment>())
        //        .With(s => s.FacultyFeePayments, new List<FacultyFeePayment>())
        //        .With(s => s.ForumQuestions, new List<ForumQuestion>())
        //        .With(s => s.Programme, new Programme())
        //        .With(s => s.SchoolFeePayments, new List<SchoolFeePayment>()).Build();
        //    var feeCategory = "SomeCategory";
        //    var sessionId = 1;
        //    var feeList = new List<FeeList>();

        //    //(paySetting, student, feeCategory, sessionId, feeList)
        //    _feeCmd.GetSchoolFeeList(feeCategory, student, paySetting, sessionId).Returns(Task.FromResult(feeList));

        //    //Act
        //    var result = await _feeCmd.GetSchoolFeeList(feeCategory, student, paySetting, sessionId);

        //    //Assert
        //    Assert.NotNull(result);
        //    Assert.IsInstanceOf<List<FeeList>>(result);
        //}

        //[Test]
        //public async Task GetSchoolFeeListWhenPartPaymentIsFalse_ReturnsListOfFee()
        //{
        //    var paySetting = Builder<PaymentSetting>.CreateNew()
        //        .With(p => p.Session, new Session()).Build();

        //    var student = Builder<Student>.CreateNew()
        //        .With(s => s.AccommodationFeePayments, new List<AccommodationFeePayment>())
        //        .With(s => s.BookIssues, new List<BookIssue>())
        //        .With(s => s.CourseRegistrations, new List<CourseRegistration>())
        //        .With(s => s.DepartmentFeePayments, new List<DepartmentFeePayment>())
        //        .With(s => s.Enrollments, new List<Enrollment>())
        //        .With(s => s.FacultyFeePayments, new List<FacultyFeePayment>())
        //        .With(s => s.ForumQuestions, new List<ForumQuestion>())
        //        .With(s => s.Programme, new Programme())
        //        .With(s => s.SchoolFeePayments, new List<SchoolFeePayment>()).Build();
        //    var feeCategory = "SomeCategory";
        //    var sessionId = 1;
        //    var feeList = new List<FeeList>();

        //    paySetting.AcceptPartPayment = false;
        //    _feeCmd.GetSchoolFeeList(paymentSetting: paySetting, student: student, feeCategory: feeCategory,
        //        sessionId: sessionId, studentStatus: "").Returns(Task.FromResult(feeList));

        //    //Act
        //    var result = await _feeCmd.GetSchoolFeeList(paymentSetting: paySetting, student: student, feeCategory: feeCategory,
        //        sessionId: sessionId, "");

        //    //Assert
        //    Assert.NotNull(result);
        //    Assert.IsInstanceOf<List<FeeList>>(result);
        //}

        //[Test]
        public async Task GetSchoolFeeListByFaculty_ReturnsFilteredList()
        {
            var paySetting = Builder<PaymentSetting>.CreateNew()
                .With(p => p.Session, new Session()).Build();

            var student = Builder<Student>.CreateNew()
                .With(s => s.StudentAccommodationFeePayments, new List<StudentAccommodationFeePayment>())
                .With(s => s.BookIssues, new List<BookIssue>())
                .With(s => s.CourseRegistrations, new List<CourseRegistration>())
                .With(s => s.DepartmentFeePayments, new List<DepartmentFeePayment>())
                .With(s => s.Enrollments, new List<Enrollment>())
                .With(s => s.FacultyFeePayments, new List<FacultyFeePayment>())
                .With(s => s.ForumQuestions, new List<ForumQuestion>())
                .With(s => s.Programme, new Programme())
                .With(s => s.SchoolFeePayments, new List<SchoolFeePayment>()).Build();
            var feeCategory = "SomeCategory";
            var facultyId = 1;
            var sessionId = 1;
            var feeList = new List<FeeList>();

            _feeCmd.GetSchoolFeeListByFaculty(feeCategory, student, paySetting, sessionId, facultyId)
                .Returns(Task.FromResult(feeList));
            var result = await _feeCmd.GetSchoolFeeListByFaculty(feeCategory, student, paySetting, sessionId, facultyId);

            //Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<List<FeeList>>(feeList);
        }

        [Test]
        public async Task GetLatePaymentFeeList_ReturnsFeeListCollection()
        {
            var schoolProgramId = Arg.Any<int>();
            var sessionId = Arg.Any<int>();

            var feeList = new List<FeeList>();

            _feeCmd.GetLatePaymentFeeList(sessionId, schoolProgramId, "", null).Returns(Task.FromResult(feeList));

            var result = await _feeCmd.GetLatePaymentFeeList(2, 2, "", null);

            Assert.NotNull(result);
        }

        [Test]
        public async Task GetSchoolFeePaymentList_ReturnsListOfSchoolFeePayment()
        {
            var feeCategory = Arg.Any<string>();

            var sessionId = Arg.Any<int>();

            var paylist = new List<SchoolFeePayment>();

            _feeCmd.GetSchoolFeePaymentList(feeCategory, sessionId).Returns(Task.FromResult(paylist));

            var result = await _feeCmd.GetSchoolFeePaymentList("someString", 1);

            Assert.NotNull(result);

            Assert.IsInstanceOf<List<SchoolFeePayment>>(result);
        }

        //[Test]
        //public void GetServiceType_ReturnsServiceType()
        //{
        //    var feeCat = Arg.Any<string>();

        //    var serviceT = "2338294";

        //    _feeCmd.GetServiceType(feeCat).Returns(serviceT);

        //    var result = _feeCmd.GetServiceType("someFeeCategory");

        //    Assert.NotNull(result);
        //    Assert.False(string.IsNullOrEmpty(result));
        //}

        [Test]
        public async Task GetPaymentSetting_ReturnsPaymentSetting()
        {
            var sessionId = Arg.Any<int>();

            var schoolprogId = Arg.Any<int>();

            var paysetting = new PaymentSetting();

            _feeCmd.GetPaymentSetting(sessionId, schoolprogId, "").Returns(Task.FromResult(paysetting));

            var result = await _feeCmd.GetPaymentSetting(1, 1, "");

            Assert.NotNull(result);

            Assert.IsInstanceOf<PaymentSetting>(result);
        }
    }
}
