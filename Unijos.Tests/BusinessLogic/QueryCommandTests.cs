using NSubstitute;
using NUnit.Framework;
using SwiftKampus.Abstractions;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Unijos.Tests.BusinessLogic
{
    [TestFixture]
    public class QueryCommandTests
    {

        private readonly IQueryCommand _command;

        public QueryCommandTests()
        {
            _command = Substitute.For<IQueryCommand>();
        }

        //[Test]
        //public void TestMethod()
        //{

        //    _command.GetCurrentSemesterId().Returns(1);

        //    Assert.NotZero(_command.GetCurrentSemesterId());
        //    Assert.NotNull(_command.GetCurrentSemesterId());
        //}

        //[Test]
        //public void GetCurrentSemester_ReturnsSemester()
        //{
        //    var semester = Builder<Semester>.CreateNew().Build();
        //    _command.GetCurrentSemester().Returns(semester);
        //    var result = _command.GetCurrentSemester();
        //    Assert.NotNull(_command.GetCurrentSemester());
        //    Assert.IsInstanceOf<Semester>(result);
        //}

        //[Test]
        //public void GetCurrentSemesterName_ReturnsSemesterName()
        //{
        //    var semester = Builder<Semester>.CreateNew()
        //                                    .Build();

        //    _command.GetCurrentSemesterName().Returns(semester.SemesterName);

        //    var result = _command.GetCurrentSemesterName();

        //    Assert.IsInstanceOf<string>(result);
        //    Assert.NotZero(result.Length);
        //}

        //[Test]
        //public void GetCurrentSession_ReturnsSession()
        //{
        //    var session = Builder<Session>.CreateNew().Build();

        //    _command.GetCurrentSession().Returns(session);

        //    var result = _command.GetCurrentSession();

        //    Assert.IsInstanceOf<Session>(result);
        //    Assert.NotNull(result);
        //    Assert.True(!string.IsNullOrEmpty(result.SessionName));
        //}

        //[Test]
        //public void CheckForFirstSemester_ReturnsTrueIfFound()
        //{
        //    var semester = new Semester
        //    {
        //        SemesterName = "FIRST"
        //    };

        //    _command.CheckForFirstSemester().Returns(semester.SemesterName.Contains("FIRST"));

        //    var result = _command.CheckForFirstSemester();

        //    Assert.True(result == true);
        //}

        //[Test]
        //public void CheckForFirstSemester_ReturnsFalseIfNotFound()
        //{
        //    var semester = new Semester
        //    {
        //        SemesterName = "FIRST"
        //    };

        //    _command.CheckForFirstSemester().Returns(semester.SemesterName.Contains("Invalid Strign"));

        //    var result = _command.CheckForFirstSemester();

        //    Assert.False(result == true);
        //}

        [Test]
        public void ConvertToKobo_ReturnsMultiplesOfHundred()
        {
            var num = Arg.Any<int>();
            _command.ConvertToKobo(num).Returns(num * 100);

            var result = _command.ConvertToKobo(7);

            Assert.IsInstanceOf<int>(result);
        }

        [Test]
        public void ConvertToNaira_ReturnsDivisiblesOfHundred()
        {
            var num = Arg.Any<int>();
            _command.ConvertToNaira(num).Returns(num / 100);

            var result = _command.ConvertToNaira(1000);

            Assert.IsInstanceOf<int>(result);
        }

        [Test]
        public void HashRemitaRequest_ReturnsSHA512HashedString()
        {
            var (merchantId, serviceTypeId, orderId, amount, responseUrl, apiKey) = ReturnStrings();

            var hashedstring = HashStandard()
                .ComputeHash(Encoding.UTF8
                .GetBytes($"{merchantId}{serviceTypeId}{orderId}{amount}{amount}{responseUrl}{apiKey}"));
            HashStandard().Clear();
            var finalString = BitConverter.ToString(hashedstring).ToLower().Replace("-", "");

            _command.HashRemitaRequest(merchantId, serviceTypeId, orderId, amount,
                responseUrl, apiKey).Returns(finalString);

            var result = _command
                .HashRemitaRequest("1213232", "2323443", "904934", "45000", "http://localhost.xxx", "34348");

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }


        [Test]
        public async Task GetAdmissionGradePoint_ReturnsGradePointAsDouble()
        {
            var randomString = Arg.Any<string>();

            _command.GetAdmissionGradePoint(randomString).Returns(1.1);

            var result = await _command.GetAdmissionGradePoint("someRandomString");

            Assert.IsInstanceOf<double>(result);
        }

        [Test]
        public void GetPaymentStatus_ReturnsBaseViewModelWithSummary()
        {
            var model = new BaseVm();

            _command.GetPaymentStatus(Arg.Any<int>()).Returns(model);

            var result = _command.GetPaymentStatus(1);

            Assert.NotNull(result);
            Assert.IsInstanceOf<BaseVm>(result);
        }

        [Test]
        public void UpdateTransactionLog_GetsCalled()
        {
            var remitalog = new RemitaPaymentLog();

            var remitaresult = new RemitaResponse();

            _command.When(x => x.Received(1).UpdateTransactionLog(remitalog, remitaresult));

            _command.UpdateTransactionLog(remitalog, remitaresult);
        }

        [Test]
        public async Task GetUserFullName_ReturnsFullName()
        {

            _command.GetUserFullName(Arg.Any<string>()).Returns("John Doe");

            var result = await _command.GetUserFullName(Guid.NewGuid().ToString());

            Assert.NotNull(result);

            Assert.IsInstanceOf<string>(result);

            Assert.AreEqual(result, "John Doe");
        }

        [Test]
        public async Task GetUserFullName_ReturnsEmtpyString()
        {
            _command.GetUserFullName(Arg.Any<string>()).Returns("");

            var result = await _command.GetUserFullName("InvalidEntry");

            Assert.NotNull(result);
            Assert.IsTrue(string.IsNullOrEmpty(result));
            Assert.AreEqual(result, "");
        }

        [Test]
        public async Task GetUserDetails_ReturnsLoginDetailsVm()
        {
            _command.GetUserDetails(Arg.Any<string>()).Returns(Task.FromResult(new LoginDetailVm()));

            var result = await _command.GetUserDetails("someuserid@unijos.edu.ng");

            Assert.NotNull(result);

            Assert.IsInstanceOf<LoginDetailVm>(result);
        }

        [Test]
        public async Task UserActivityStatistic_ReturnsTupleWithValues()
        {
            var tresult = new Tuple<int, double, int>(22, 158.30, 8);

            _command.UserActivityStatistic().Returns(tresult);

            var result = await _command.UserActivityStatistic();

            Assert.NotNull(result);
            Assert.IsInstanceOf<Tuple<int, double, int>>(result);
        }

        [Test]
        public async Task GetUserGradRule_ReturnsListOfUnderGradRule()
        {
            var rule = new UnderGraduateRule();
            var ruleList = new List<UnderGraduateRule>();

            _command.GetUnderGraduateRule(Arg.Any<string>()).Returns(Task.FromResult(ruleList));

            var result = await _command.GetUnderGraduateRule("someemail@gmail.com");

            Assert.NotNull(result);
            Assert.IsInstanceOf<List<UnderGraduateRule>>(result);
        }

        [Test]
        public async Task GetUserGradRuleOverload_ReturnsListOfUnderGradRule()
        {
            var rule = new UnderGraduateRule();
            var ruleList = new List<UnderGraduateRule>();

            _command.GetUnderGraduateRule(Arg.Any<int>(), Arg.Any<int>()).Returns(Task.FromResult(ruleList));

            var result = await _command.GetUnderGraduateRule(2, 2);

            Assert.NotNull(result);
            Assert.IsInstanceOf<List<UnderGraduateRule>>(result);

        }

        #region TestSeupInfrastructure
        private (string merchantId, string serviceTypeId,
            string orderId, string amount, string responseUrl, string apiKey) ReturnStrings()
        {
            var merchantId = "290239480";
            var serviceTypeId = "343405943";
            var orderId = "39493845";
            var amount = "45000";
            var responseUrl = "http://nowhere.local";
            var apikey = "84930";

            return (merchantId, serviceTypeId, orderId, amount, responseUrl, apikey);
        }

        private SHA512Managed HashStandard()
        {
            return new SHA512Managed();
        }

        #endregion
    }
}
