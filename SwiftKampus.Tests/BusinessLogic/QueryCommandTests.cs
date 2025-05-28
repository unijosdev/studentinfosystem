using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using SwiftKampusModel.Payment;

namespace SwiftKampus.Tests.BusinessLogic
{
    [TestClass]
    public class QueryCommandTests
    {
        private SchoolDbContext _db;

        private QueryCommand _qCommand;

        public QueryCommandTests()
        {
            _db = new SchoolDbContext();
            
            _qCommand = new QueryCommand(_db);
        }

        [TestMethod]
        public void GetCurrentActiveSessionId_ReturnsIdOfSession()
        {
            //Act
            var result = _qCommand.GetCurrentSemesterId();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(int));
        }

        [TestMethod]
        public void GetCurrentSessionName_ReturnsNameOfActiveSession()
        {
            var result = _qCommand.GetCurrentSessionName();

            //Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(string));
        }

        [TestMethod]
        public void GetCurrentSemesterName_ReturnsNameOfSemester()
        {
            var name = _qCommand.GetCurrentSemesterName();

            Assert.IsInstanceOfType(name, typeof(string));
        }
    }
}
