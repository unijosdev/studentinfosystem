using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using OfficeOpenXml;

using Rotativa;

using SwiftKampus.Abstractions;
using SwiftKampus.BusinessLogic;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels;
using SwiftKampus.ViewModels.Fee_Management;
using SwiftKampus.ViewModels.StudentBioData;
using SwiftKampus.ViewModels.StudentStatusMgtVm;
using SwiftKampusModel;

namespace SwiftKampus.Controllers.Api
{
    public class HealthInformationSystemController : ApiController
    {
        public readonly SchoolDbContext _db;

        public HealthInformationSystemController(SchoolDbContext db)
        {
            _db = db;
        }

        // GET /api/HealthInformationSystem/GetStudent/1
        [Route("api/HealthInformationSystem/GetStudent")]
        [HttpGet]
        public IHttpActionResult GetStudent(string id)
        {
           
            var student = _db.Students.Include(x => x.SchoolProgramme)
                                            .Include(x => x.Programme)
                                            .Include(x => x.Programme.Department)
                                            .Include(x => x.Programme.Department.Faculty)
                                            .Include(x => x.Session)
                                            .Include(x => x.Level)
                                            .Where(x => x.MatricNo.ToUpper() == id.Trim().ToUpper() || x.Email.ToUpper() == id.Trim().ToUpper())
                                            .Select(x => new
                                            {
                                                StudentId = x.StudentId,
                                                MatricNo = x.MatricNo,
                                                JambRegNo = x.JambRegNo,
                                                Email = x.Email,
                                                PrimaryEmail = x.PrimaryEmail,
                                                LastName = x.LastName,
                                                FirstName = x.FirstName,
                                                MiddleName = x.MiddleName,
                                                DateOfBirth = x.DateOfBirth.ToString(),
                                                SchoolProgramme = x.SchoolProgramme.FancyName,
                                                Faculty = x.Programme.Department.Faculty.FacultyName,
                                                Department = x.Programme.Department.DeptName,
                                                Programme = x.Programme.ProgrammeName,
                                                Level = x.Level.LevelName,
                                                Session = x.Session.SessionName,
                                                ModeOfEntry = x.ModeOfEntry,
                                                StudentStatus = x.StudentStatus,
                                                Active = x.Active,
                                                IsGraduated = x.IsGraduated,
                                                IsClearedDepartment = x.IsClearedDepartment,
                                                IsClearedFaculty = x.IsClearedFaculty,
                                                IsClearedAcademics = x.IsClearedAcademics,
                                                Gender = x.Gender,
                                                BloodGroup = x.BloodGroup,
                                                Passport = x.Passport,
                                            })
                                            .FirstOrDefault();

            //var nextOfKin = _db.NextOfKins.Where(x => x.UserId == student.PrimaryEmail || x.UserId == student.Email).FirstOrDefault();
            //if (nextOfKin != null)
            //{

            //}

            if (student == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            if (student != null)
                return Ok(new { student });

            return Ok();
        }
    }
}
