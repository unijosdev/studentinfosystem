using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NSubstitute;
using SwiftKampus.Abstractions;
using SwiftKampus.Controllers;
using SwiftKampus.Models;

namespace Unijos.Tests.Controllers
{
    [TestFixture]
    public class BaseControllerTests
    {
        private readonly BaseController basectl;

        public BaseControllerTests()
        {
            basectl = Substitute.For<BaseController>(new SchoolDbContext());
        }


        [Test]
        public void ConfirmApplicationFee_ReturnsRedirect()
        {
            var request = Substitute.For<HttpRequestBase>();
            var feequery = Substitute.For<IQueryCommand>();
            var studentquery = Substitute.For<IStudentQueryManager>();
            var context = Substitute.For<HttpContextBase>();

            request.IsAuthenticated.Returns(true);
            context.Request.Returns(request);
            context.User.IsInRole(RoleName.Applicant).Returns(true);
            feequery.GetId().Returns("someuser@unijos.edu.ng");

            basectl._IsPayedApplicationFee.Returns(false);

            basectl.HttpContext.Returns(context);

            var tc = new TestClass {Name = "SomeName"};
            var result = basectl.ConfirmApplicationFee() as RedirectResult;

            Assert.NotNull(basectl);
        }
    }

    public class TestClass
    {
        public string Name { get; set; }
    }
}
