using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SwiftKampus.Abstractions;
using SwiftKampus.ViewModels;

namespace Unijos.Tests.BusinessLogic
{
    [TestFixture]
    public class StudentQueryManagerTests
    {
        private readonly IStudentQueryManager _studentQmr;

        public StudentQueryManagerTests()
        {
            _studentQmr = Substitute.For<IStudentQueryManager>();
        }

        [Test]
        public async Task GetStudentList_ReturnsListOfStudentIndexVMWhenPassedNull()
        {

            var vms = new List<StudentIndexVM>();

            _studentQmr.GetStudentList(null).Returns(Task.FromResult(vms));

            var result = await _studentQmr.GetStudentList(null);

            Assert.NotNull(result);
        }

        [Test]
        public async Task GetStudentList_ReturnsListOfSTudentIndexVMWhenPassedAnInt()
        {
            var programmeId = 1;

            var vms = new List<StudentIndexVM>{ new StudentIndexVM(), new StudentIndexVM()};

            _studentQmr.GetStudentList(Arg.Any<int>()).Returns(Task.FromResult(vms));

            var result = await _studentQmr.GetStudentList(programmeId);

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetStudentAcademicList_ReturnsListOfStudentIndexVMWhenNullIsPassed()
        {
            var vms = new List<StudentIndexVM> {new StudentIndexVM(), new StudentIndexVM()};

            _studentQmr.GetStudentAcademicList(null, null, false).Returns(Task.FromResult(vms));

            int? id = null;

            var result = await _studentQmr.GetStudentAcademicList(id, null, false);

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsInstanceOf<List<StudentIndexVM>>(result);
        }


    }
}
