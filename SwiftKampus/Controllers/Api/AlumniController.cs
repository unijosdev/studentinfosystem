using SwiftKampus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using SwiftKampusModel;
using SwiftKampusModel.Payment;

namespace SwiftKampus.Controllers.Api
{
    public class AlumniController : ApiController
    {
        public readonly SchoolDbContext _db;

        public AlumniController(SchoolDbContext db)
        {
            _db = db;
        }

        // GET api/alumni/GetAlumis
        public IEnumerable<Student> GetAlumnis()
        {
            var students = _db.Students.Where(x => x.IsGraduated == true).ToList();

            return students;
        }

        // GET /api/alumni/GetAlumni/1
        public Student GetAlumni(string id, string lastName, string firstName)
        {
            var student_Name = _db.Students.Where(x => x.FirstName == firstName && x.LastName == lastName && x.IsGraduated == true).FirstOrDefault();
            var student_Matric = _db.Students.Where(x=>x.MatricNo == id && x.IsGraduated == true).FirstOrDefault();

            if(student_Name == null || student_Matric == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (student_Name != null)
                return student_Name;

            return student_Name;
        }

        public SundryAndOtherIncomeCharge GetPaymentSetup(string paymentName)
        {
            var payment = _db.SundryAndOtherIncomeCharges.ToList();

            var paymentSetup = _db.SundryAndOtherIncomeCharges.Where(x => x.ChargeName == paymentName).FirstOrDefault();

            if (paymentSetup == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            else return paymentSetup;
        }
    }
}
