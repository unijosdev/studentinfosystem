using Microsoft.Ajax.Utilities;
using SwiftKampus.Models;
using SwiftKampus.Services;
using SwiftKampus.ViewModels.Result;
using SwiftKampusModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftKampus.BusinessLogic
{
    public class ResultCommand
    {
        readonly SchoolDbContext _db;
        readonly StudentQueryManager _studentQuery;
        readonly QueryCommand _query;
        public ResultCommand(SchoolDbContext db)
        {
            _db = db;
            _studentQuery = new StudentQueryManager(_db);
            _query = new QueryCommand(_db);
        }

        public int GetResultTemplateId(int deptId, int sessionId, string levelName)
        {
            var deptResultTemplate = _db.DeptResultTypes.Where(x => x.DepartmentId.Equals(deptId) &&
                                        x.SessionId.Equals(sessionId)).FirstOrDefault();

            if (deptResultTemplate != null)
            {
                if (levelName.ToUpper().Equals(levelName) && deptResultTemplate.HaveHundredLevelResult.Equals(true))
                {
                    return (int)deptResultTemplate.ResultTemplateForHundred;
                }
                return deptResultTemplate.ResultTemplateId;
            }
            //return _db.ResultTemplates.Where(x => x.ResultType.Equals(ResultNameType.Regular_Result.ToString()))
            //                .Select(s => s.ResultTemplateId).FirstOrDefault();
            return 0;
        }

        public int CheckForResultTemplate(int CourseId, int SessionId, string levelName)
        {
            var deptId = _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(CourseId))
                                .Select(s => s.Programme.Department.DepartmentId).FirstOrDefault();

            var checkResultTemplate = GetResultTemplateId(deptId, SessionId, levelName);
            return checkResultTemplate;
        }

        public async Task<bool> CheckDeptApproval(int CourseId, int SessionId, int levelId, int programmeId)
        {
            return await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Programme).AsNoTracking()
                            .Where(x => x.CourseId.Equals(CourseId) && x.Programme.ProgrammeId.Equals(programmeId) &&
                            x.SessionId.Equals(SessionId) && x.Level.LevelId.Equals(levelId))
                            .Select(s => s.IsDeptApproved).FirstOrDefaultAsync();
        }

        public async Task<bool> CheckForResult(int CourseId, int SessionId, int levelId, int programmeId)
        {
            return await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Programme).AsNoTracking()
                            .AnyAsync(x => x.CourseId.Equals(CourseId) && x.Programme.ProgrammeId.Equals(programmeId) &&
                            x.SessionId.Equals(SessionId) && x.Level.LevelId.Equals(levelId));
        }

        public async Task<bool> CheckForResult(string studentId, int CourseId, int SessionId, int levelId, int programmeId)
        {
            return await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Programme).AsNoTracking()
                            .AnyAsync(x => x.StudentId.Equals(studentId) && x.CourseId.Equals(CourseId) && x.Programme.ProgrammeId.Equals(programmeId) &&
                            x.SessionId.Equals(SessionId) && x.Level.LevelId.Equals(levelId));
        }

        public async Task<List<ContinuousAssessment>> GetCurrentCa(string studentId, int levelId, int sessionId)
        {
            return await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Programme).Include(i => i.SessionId)
                            .AsNoTracking().Where(x => x.StudentId.Equals(studentId) && x.Level.LevelId.Equals(levelId)
                            && x.SessionId.Equals(sessionId)).ToListAsync();
        }

        public async Task<Tuple<bool, double>> CheckForProbation(string studentId)
        {
            var student = _studentQuery.GetStudent(studentId);
            var deptId = _db.Programmes.Include(i => i.Department).AsNoTracking()
                            .Where(x => x.ProgrammeId.Equals((int)student.ProgrammeId))
                            .Select(s => s.Department.DepartmentId).FirstOrDefault();
            DeptResultType deptResultTemplate = GetDeptResultTemplate((int)student.SessionId, deptId);
            var cgpa = Convert.ToDouble(await CalculateCgpa(student.StudentId));

            if (cgpa <= deptResultTemplate.ProbationMark)
            {
                var result = new Tuple<bool, double>(true, cgpa);
                return result;
            }
            else
            {
                var result = new Tuple<bool, double>(false, cgpa);
                return result;
            }

        }

        public async Task<List<NyscListVm>> GetNyscList(int programmeId, int levelId, int sessionId)
        {
            var builder = new StringBuilder();
            var rptbuilder = new StringBuilder();
            var model = new List<NyscListVm>();
            string remarks = string.Empty;
            var level = _query.GetLevelById(levelId);
            var courseRegs = _db.CourseRegistrations.Include(i => i.Students)
                                .Include(i => i.Course).Include(i => i.Programme).AsNoTracking()
                                .Where(x => x.ProgrammeId.Equals(programmeId) &&
                                x.LevelId.Equals(levelId) && x.SessionId.Equals(sessionId) &&
                                x.IsApproved.Equals(true)).ToList().DistinctBy(d => d.StudentId).ToList();
            var deptId = _db.Programmes.Include(i => i.Department).AsNoTracking()
                            .Where(x => x.ProgrammeId.Equals(programmeId))
                            .Select(s => s.Department.DepartmentId).FirstOrDefault();
            var deptResultTemplate = _db.DeptResultTypes.AsNoTracking()
                                        .Where(x => x.DepartmentId.Equals(deptId)).FirstOrDefault();
            if (deptResultTemplate != null)
            {
                int failMark = deptResultTemplate.FailMark;

                int count = 1;
                foreach (var courseReg in courseRegs)
                {
                    var ca = await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                        .Where(x => x.StudentId.Equals(courseReg.StudentId) &&
                                        x.Level.LevelId.Equals(levelId) && x.SessionId.Equals(sessionId))
                                        .ToListAsync();
                    var student = _db.Students.Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.SchoolProgramme).AsNoTracking()
                                    .Where(x => x.StudentId.Equals(courseReg.StudentId))
                                    .FirstOrDefault();


                    var sessionParameters = await CheckLevelAndGenerateResult(courseReg.StudentId, levelId, sessionId, failMark, courseReg.Students.ModeOfEntry.Trim().ToUpper());
                    var allCa = new List<ContinuousAssessment>();
                    //allCa.AddRange(ca);

                    foreach (var assesment in sessionParameters)
                    {
                        allCa.AddRange(assesment.ContinuousAssessments);
                    }

                    var currentGpa = sessionParameters.Where(x => x.LevelName.Equals(level.LevelName)).Select(s => s.DGPA).FirstOrDefault();
                    if (currentGpa < deptResultTemplate.ProbationMark)
                    {
                        builder.Append("PROBATION, ");
                    }

                    if (allCa.Any(x => x.Total <= failMark))
                    {
                        var failedCourses = allCa.Where(c => c.Total <= failMark).DistinctBy(d => d.CourseId).ToList();

                        foreach (var course in failedCourses)
                        {
                            var checkForPassedCourse = allCa.Where(x => x.CourseId.Equals(course.CourseId)).ToList();
                            if (checkForPassedCourse.Any(x => x.Total > failMark))
                            {
                                if (!builder.ToString().Contains("PASS"))
                                {
                                    builder.Append("PASS");
                                }
                            }
                            else
                            {
                                var regCourse = ca.Where(x => x.CourseId.Equals(course.CourseId)).FirstOrDefault();
                                if (!builder.ToString().Contains("RPT"))
                                {
                                    builder.Append("RPT  ");
                                }
                                builder.Append(course.Course.CourseCode);
                                builder.Append(", ");

                            }

                        }
                        remarks = builder.ToString() + " " + rptbuilder.ToString();
                    }
                    else
                    {
                        remarks = "PASS";
                    }
                    var grade = new GradeRemark(_db);
                    if (remarks.Equals("PASS"))
                    {
                        var nyscList = new NyscListVm()
                        {
                            Sn = count,
                            MatricNo = student.MatricNo,
                            Course = student.Programme.ProgrammeName,
                            DateOfBirth = student.DateOfBirth,
                            Gender = student.Gender,
                            JambRegNo = student.JambRegNo,
                            MaritalStatus = student.MaritalStatus,
                            Surname = student.LastName,
                            OtherName = $"{student.FirstName} {student.MiddleName}",
                            PhoneNumber = student.PhoneNumber,
                            ProgrammeMode = student.SchoolProgramme.ProgrammeType.Replace("_", " "),
                            Qualification = student.Programme.AwardingDegreeName,
                            ServiceYear = DateTime.Now.Year.ToString(),
                            StateOfOrigin = student.StateOfOrigin,
                            YearOfResult = CalculateYearOfResult(student.Session.SessionName, student.Programme, student.ModeOfEntry.ToUpper()),
                            ClassOfDegree = grade.ClassOfDegree(currentGpa, student.SchoolProgrammeId),
                            SchoolProgrammeId = student.SchoolProgrammeId
                        };
                        model.Add(nyscList);
                        count = count + 1;
                    }
                    builder.Clear();
                    remarks = string.Empty;

                }
                return model;
            }
            return null;
        }

        private string CalculateYearOfResult(string sessionName, Programme programme, string modeOfEntry)
        {
            var level = _query.GetLevelById((int)programme.LevelId);
            var numberOfYears = Convert.ToInt16(level.LevelOrder.Substring(0, 1));

            if (modeOfEntry.ToUpper().Trim().Equals("DE"))
            {
                for (int i = 0; i < numberOfYears - 2; i++)
                {
                    sessionName = _query.GetNextSession(sessionName);
                }
            }
            else
            {
                for (int i = 0; i < numberOfYears - 1; i++)
                {
                    sessionName = _query.GetNextSession(sessionName);
                }
            }

            return sessionName;
        }

        public async Task<string> CalculateCgpaByLevel(string studentId, int sessionId, int levelId)
        {
            var student = _studentQuery.GetStudent(studentId);
            var deptId = _db.Programmes.Include(i => i.Department).AsNoTracking()
                            .Where(x => x.ProgrammeId.Equals((int)student.ProgrammeId))
                            .Select(s => s.Department.DepartmentId).FirstOrDefault();
            DeptResultType deptResultTemplate = GetDeptResultTemplate((int)student.SessionId, deptId);
            var cgpa = await GetStudentCa(student.StudentId, levelId, sessionId, deptResultTemplate);
            var result = CalculateCaSum(cgpa.SessionParameters);
            return result[0].CGPA;
        }

        public DeptResultType GetDeptResultTemplate(int sessionId, int deptId)
        {
            return _db.DeptResultTypes.AsNoTracking().Where(x => x.DepartmentId.Equals(deptId) &&
                        x.SessionId.Equals(sessionId)).FirstOrDefault();
        }

        public async Task<DeptSanateFormatSummaryVm> GetStudentCa(string studentId, int levelId, int sessionId, DeptResultType deptResultTemplate)
        {
            var builder = new StringBuilder();
            var model = new DeptSanateFormatSummaryVm();
            var level = _query.GetLevelById(levelId);
            var ca = await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course)
                                        .Include(i => i.Student).Include(i => i.Programme).AsNoTracking()
                                         .Where(x => x.StudentId.Equals(studentId) && x.Level.LevelId.Equals(levelId) &&
                                         x.SessionId.Equals(sessionId)).ToListAsync();


            if (deptResultTemplate != null)
            {
                int failMark = deptResultTemplate.FailMark;
                int count = 1;
                var courseReg = ca.FirstOrDefault();

                var summary = new DeptSanateFormatSummaryVm()
                {
                    Sn = count,
                    FullName = courseReg.Student.FullName,
                    MatricNo = courseReg.Student.MatricNo,
                    ModeOfEntry = courseReg.Student.ModeOfEntry,
                    MNSA = courseReg.Programme.NoOfSemesters.ToString(),
                    NSS = ca.OrderBy(o => o.SessionId).DistinctBy(d => new { d.SessionId, d.SemesterId }).Count().ToString(),
                    ContinuousAssessments = ca.OrderBy(o => o.Course.CourseName).ToList(),
                };

                summary.SessionParameters = await CheckLevelAndGenerateResult(courseReg.StudentId, levelId, sessionId, failMark, courseReg.Student.ModeOfEntry.Trim().ToUpper());
                var allCa = new List<ContinuousAssessment>();
                //allCa.AddRange(ca);
                foreach (var assesment in summary.SessionParameters)
                {
                    allCa.AddRange(assesment.ContinuousAssessments);
                }

                var currentGpa = summary.SessionParameters.Where(x => x.LevelName.Equals(level.LevelName)).Select(s => s.DGPA).FirstOrDefault();
                if (currentGpa < deptResultTemplate.ProbationMark)
                {
                    builder.Append("PROBATION, ");
                }

                if (allCa.Any(x => x.Total <= failMark))
                {
                    var failedCourses = allCa.Where(c => c.Total <= failMark).DistinctBy(d => d.CourseId).ToList();

                    foreach (var course in failedCourses)
                    {
                        var checkForPassedCourse = allCa.Where(x => x.CourseId.Equals(course.CourseId)).ToList();
                        if (checkForPassedCourse.Any(x => x.Total > failMark))
                        {
                            if (!builder.ToString().Contains("RPT  "))
                            {
                                if (!builder.ToString().Contains("PASS"))
                                {
                                    builder.Append("PASS");
                                }
                            }
                        }
                        else
                        {
                            if (!builder.ToString().Contains("RPT  "))
                            {
                                builder.Append("RPT  ");
                            }
                            builder.Append(course.Course.CourseCode);
                            builder.Append(", ");
                        }

                    }
                    summary.Remarks = builder.ToString();
                }
                else
                {
                    summary.Remarks = "PASS";
                }

                model = summary;
                builder.Clear();

                count = count + 1;

                return model;
            }
            else
            {
                return null;
            }
        }

        public List<Course> CheckForCarryOver(List<ContinuousAssessment> ca, Student student)
        {
            var level = _query.GetLevelById((int)student.LevelId);

            var allCoreCourses = new List<Course>();
            if (level.LevelOrder.Equals("200"))
            {
                if (student.ModeOfEntry.Trim().ToUpper() != "DE")
                {
                    var levelId = GetLevelByName("100");
                    allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                           .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                           x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                        x.DeActivatedCourse.Equals(false)).ToList());
                }

            }
            if (level.LevelOrder.Equals("300"))
            {

                var levelId = GetLevelByName("200");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());
                if (student.ModeOfEntry.Trim().ToUpper() != "DE")
                {
                    levelId = GetLevelByName("100");
                    allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                           .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                           x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                        x.DeActivatedCourse.Equals(false)).ToList());
                }

            }
            if (level.LevelOrder.Equals("400"))
            {

                var levelId = GetLevelByName("300");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                levelId = GetLevelByName("200");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                if (student.ModeOfEntry.Trim().ToUpper() != "DE")
                {
                    levelId = GetLevelByName("100");
                    allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                           .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                           x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                        x.DeActivatedCourse.Equals(false)).ToList());
                }
            }
            if (level.LevelOrder.Equals("500"))
            {
                var levelId = GetLevelByName("400");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                levelId = GetLevelByName("300");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                levelId = GetLevelByName("200");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                if (student.ModeOfEntry.Trim().ToUpper() != "DE")
                {
                    levelId = GetLevelByName("100");
                    allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                           .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                           x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                        x.DeActivatedCourse.Equals(false)).ToList());
                }
            }
            if (level.LevelOrder.Equals("600"))
            {

                var levelId = GetLevelByName("500");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                levelId = GetLevelByName("400");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                levelId = GetLevelByName("300");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                levelId = GetLevelByName("200");
                allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                    x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                    x.DeActivatedCourse.Equals(false)).ToList());

                if (student.ModeOfEntry.Trim().ToUpper() != "DE")
                {
                    levelId = GetLevelByName("100");
                    allCoreCourses.AddRange(_db.Courses.Include(i => i.Programme).Include(i => i.Level).AsNoTracking()
                                           .Where(x => x.Programme.ProgrammeId.Equals((int)student.ProgrammeId) &&
                                           x.Level.LevelId.Equals(levelId) && x.CourseType.Trim().ToUpper().Equals("CORE") &&
                                        x.DeActivatedCourse.Equals(false)).ToList());
                }
            }

            var courseList = new List<Course>();
            var allCa = new List<ContinuousAssessment>();
            allCa.AddRange(ca);
            var deptId = _db.Programmes.Include(i => i.Department).AsNoTracking()
                         .Where(x => x.ProgrammeId.Equals((int)student.ProgrammeId))
                         .Select(s => s.Department.DepartmentId).FirstOrDefault();
            DeptResultType deptResultTemplate = GetDeptResultTemplate((int)student.SessionId, deptId);

            if (deptResultTemplate != null)
            {
                if (allCa.Any(x => x.Total <= deptResultTemplate.FailMark))
                {
                    var failedCourses = allCa.Where(c => c.Total <= deptResultTemplate.FailMark).ToList();

                    foreach (var course in failedCourses)
                    {
                        var checkForPassedCourse = allCa.Where(x => x.CourseId.Equals(course.CourseId)).ToList();
                        if (checkForPassedCourse.Any(x => x.Total > deptResultTemplate.FailMark))
                        {
                        }
                        else
                        {
                            courseList.Add(course.Course);
                        }

                    }
                }
            }

            foreach (var coreCourses in allCoreCourses)
            {
                var checkForNotRegisteredCourses = ca.FirstOrDefault(x => x.CourseId.Equals(coreCourses.CourseId));
                if (checkForNotRegisteredCourses == null)
                {
                    courseList.Add(coreCourses);
                }
            }
            return courseList;
        }

        public async Task<List<Course>> CheckForPreviousLevelCourse(Student student)
        {
            var level = _query.GetLevelById((int)student.LevelId);

            var allCoreCourses = new List<Course>();
            if (level.LevelOrder.Equals("200"))
            {
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "100"));
            }
            if (level.LevelOrder.Equals("300"))
            {
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "100"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "200"));
            }
            if (level.LevelOrder.Equals("400"))
            {
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "100"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "200"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "300"));

            }
            if (level.LevelOrder.Equals("500"))
            {
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "100"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "200"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "300"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "400"));
            }
            if (level.LevelOrder.Equals("600"))
            {
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "100"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "200"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "300"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "400"));
                allCoreCourses.AddRange(await GetCoursesByProgrammeLevel((int)student.ProgrammeId, "500"));
            }

           
            return allCoreCourses;
        }

        public async Task<List<Course>> GetCoursesByProgrammeLevel(int programmeId, string LevelName)
        {
            return await _db.Courses.Include(i => i.Programme).Include(i => i.Semester).Include(i => i.Level)
                                    .AsNoTracking()
                                    .Where(x => x.Programme.ProgrammeId.Equals(programmeId)
                                     && x.Level.LevelOrder.Equals(LevelName)
                                     && x.DeActivatedCourse.Equals(false))
                                    .ToListAsync();
        }

        public async Task<List<DeptSanateFormatSummaryVm>> GetSummaryCompleteFormatData(int? programmeId, int? levelId, int? sessionId)
        {

            var builder = new StringBuilder();
            var rptbuilder = new StringBuilder();
            var model = new List<DeptSanateFormatSummaryVm>();
            var level = _query.GetLevelById((int)levelId);
            var schoolProgramme = _query.GetSchoolProgById((int)levelId);
            var courseRegs = new List<Student>();
            int programmeid = (int)programmeId;
            var sessionCourseRegs = _db.CourseRegistrations.Include(i => i.Students)
                        .Include(i => i.Course).Include(i => i.Programme).AsNoTracking()
                        .Where(x => x.ProgrammeId.Equals((int)programmeId) &&
                        x.LevelId.Equals((int)levelId) && x.SessionId.Equals((int)sessionId) &&
                        x.IsApproved.Equals(true)).ToList().DistinctBy(d => d.StudentId).ToList();
            var admittedSessionId = GetAdmittedSession((int)levelId, (int)sessionId, schoolProgramme.SchoolProgrammeId);
            if (admittedSessionId.Item2 != 0)
            {
                courseRegs.AddRange(_db.Students.Include(i => i.Session).Include(i => i.Programme).AsNoTracking()
                                   .Where(x => x.Session.SessionId.Equals(admittedSessionId.Item2)
                                   && x.Programme.ProgrammeId.Equals(programmeid) && x.IsGraduated.Equals(false)
                                   && x.Active.Equals(true) && x.ModeOfEntry.Equals("DE")).ToList());
            }
            courseRegs.AddRange(_db.Students.Include(i => i.Session).Include(i => i.Programme).AsNoTracking()
                                    .Where(x => x.Session.SessionId.Equals(admittedSessionId.Item1)
                                    && x.Programme.ProgrammeId.Equals(programmeid) && x.IsGraduated.Equals(false)
                                    && x.Active.Equals(true)).ToList());

            var deptId = _db.Programmes.Include(i => i.Department).AsNoTracking()
                            .Where(x => x.ProgrammeId.Equals((int)programmeId))
                            .Select(s => s.Department.DepartmentId).FirstOrDefault();
            var deptResultTemplate = _db.DeptResultTypes.AsNoTracking()
                                        .Where(x => x.DepartmentId.Equals(deptId)).FirstOrDefault();
            if (deptResultTemplate != null)
            {
                int failMark = deptResultTemplate.FailMark;

                int count = 1;
                foreach (var courseReg in courseRegs.OrderBy(o => o.MatricNo).ToList())
                {
                    var ca = await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                        .Where(x => x.StudentId.Equals(courseReg.StudentId) &&
                                        x.Level.LevelId.Equals((int)levelId)
                                        && x.Programme.ProgrammeId.Equals(programmeid) && x.SessionId.Equals((int)sessionId))
                                        .ToListAsync();

                    var summary = new DeptSanateFormatSummaryVm()
                    {
                        Sn = count,
                        FullName = courseReg.FullName,
                        MatricNo = courseReg.MatricNo,
                        ModeOfEntry = courseReg.ModeOfEntry,
                        MNSA = courseReg.Programme.NoOfSemesters.ToString(),
                        ContinuousAssessments = ca,
                    };

                    summary.NSS = CalculateNumberOfSemesterSpent(courseReg.StudentId).ToString();
                    summary.SessionParameters = await CheckLevelAndGenerateResult(courseReg.StudentId, (int)levelId, (int)sessionId, failMark, courseReg.ModeOfEntry.Trim().ToUpper());
                    var allCa = new List<ContinuousAssessment>();
                    //allCa.AddRange(ca);

                    foreach (var assesment in summary.SessionParameters)
                    {
                        allCa.AddRange(assesment.ContinuousAssessments);
                    }

                    var checkIfRegistered = sessionCourseRegs.FirstOrDefault(s => s.StudentId.Equals(courseReg.StudentId));
                    var sessionName = _query.GetSessionNameById((int)sessionId);
                    var partSession = _query.GetSessionPartName(sessionName);
                    if (checkIfRegistered == null && partSession >= 2018)
                    {
                        summary.Remarks = "VOLUNTARY WITHDRAWAL";
                    }
                    else
                    {
                        var currentGpa = summary.SessionParameters.Where(x => x.LevelName.Equals(level.LevelName)).Select(s => s.DGPA).FirstOrDefault();
                        if (currentGpa < deptResultTemplate.ProbationMark && programmeid != 139)
                        {
                            builder.Append("PROBATION, ");
                        }

                        if (allCa.Any(x => x.Total <= failMark))
                        {
                            var failedCourses = allCa.Where(c => c.Total <= failMark).DistinctBy(d => d.CourseId).ToList();

                            foreach (var course in failedCourses)
                            {
                                var checkForPassedCourse = allCa.Where(x => x.CourseId.Equals(course.CourseId)).ToList();
                                if (checkForPassedCourse.Any(x => x.Total > failMark))
                                {
                                    if (!builder.ToString().Contains("RPT  "))
                                    {
                                        if (!builder.ToString().Contains("PASS"))
                                        {
                                            builder.Append("PASS");
                                        }
                                    }
                                }
                                else
                                {
                                    var regCourse = ca.Where(x => x.CourseId.Equals(course.CourseId)).FirstOrDefault();
                                    if(programmeid == 139) //Added to temporarily exclude Pharmacy from repeat courseList
                                    {
                                        if (!builder.ToString().Contains("RPT"))
                                        {
                                            builder.Append("RPT " + level.LevelName);
                                        }
                                    }
                                    else
                                    {
                                        if (!builder.ToString().Contains("RPT"))
                                        {
                                            builder.Append("RPT  ");
                                        }
                                        builder.Append(course.Course.CourseCode);
                                        builder.Append(", ");
                                        //}
                                        
                                    }
                                }

                            }
                            summary.Remarks = builder.ToString() + " " + rptbuilder.ToString();
                        }
                        else if (allCa.Count == 0)
                        {
                            summary.Remarks = "NO GRADE";
                        }
                        else
                        {
                            summary.Remarks = "PASS";
                        }
                    }
                    model.Add(summary);
                    builder.Clear();

                    count = count + 1;
                }
                return model.OrderBy(o => o.MatricNo).ToList();
            }
            return null;
        }

        //public async Task<List<DeptSanateFormatSummaryVm>> GetSummaryCompleteFormatData(int? programmeId, int? levelId, int? sessionId)
        //{

        //    var builder = new StringBuilder();
        //    var rptbuilder = new StringBuilder();
        //    var model = new List<DeptSanateFormatSummaryVm>();
        //    var level = _query.GetLevelById((int)levelId);
        //    var courseRegs = _db.CourseRegistrations.Include(i => i.Students)
        //                .Include(i => i.Course).Include(i => i.Programme).AsNoTracking()
        //                .Where(x => x.ProgrammeId.Equals((int)programmeId) &&
        //                x.LevelId.Equals((int)levelId) && x.SessionId.Equals((int)sessionId) &&
        //                x.IsApproved.Equals(true)).ToList().DistinctBy(d => d.StudentId).ToList();
        //    var deptId = _db.Programmes.Include(i => i.Department).AsNoTracking()
        //                    .Where(x => x.ProgrammeId.Equals((int)programmeId))
        //                    .Select(s => s.Department.DepartmentId).FirstOrDefault();
        //    var deptResultTemplate = _db.DeptResultTypes.AsNoTracking()
        //                                .Where(x => x.DepartmentId.Equals(deptId)).FirstOrDefault();
        //    if (deptResultTemplate != null)
        //    {
        //        int failMark = deptResultTemplate.FailMark;

        //        int count = 1;
        //        foreach (var courseReg in courseRegs)
        //        {
        //            var ca = await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
        //                                .Where(x => x.StudentId.Equals(courseReg.StudentId) &&
        //                                x.Level.LevelId.Equals((int)levelId) && x.SessionId.Equals((int)sessionId))
        //                                .ToListAsync();
        //            var tgp = ca.Sum(x => x.QualityPoint);
        //            var tcr = ca.Sum(s => s.Course.Credits);
        //            string gpa = string.Format("{0:F2}", Convert.ToDouble(tgp) / Convert.ToDouble(tcr));
        //            var summary = new DeptSanateFormatSummaryVm()
        //            {
        //                Sn = count,
        //                FullName = courseReg.Students.FullName,
        //                MatricNo = courseReg.Students.MatricNo,
        //                ModeOfEntry = courseReg.Students.ModeOfEntry,
        //                MNSA = courseReg.Programme.NoOfSemesters.ToString(),
        //                ContinuousAssessments = ca,
        //            };

        //            summary.NSS = CalculateNumberOfSemesterSpent(courseReg.StudentId).ToString();
        //            summary.SessionParameters = await CheckLevelAndGenerateResult(courseReg.StudentId, (int)levelId, (int)sessionId, failMark, courseReg.Students.ModeOfEntry.Trim().ToUpper());
        //            var allCa = new List<ContinuousAssessment>();
        //            //allCa.AddRange(ca);

        //            foreach (var assesment in summary.SessionParameters)
        //            {
        //                allCa.AddRange(assesment.ContinuousAssessments);
        //            }

        //            var currentGpa = summary.SessionParameters.Where(x => x.LevelName.Equals(level.LevelName)).Select(s => s.DGPA).FirstOrDefault();
        //            if (currentGpa < deptResultTemplate.ProbationMark)
        //            {
        //                builder.Append("PROBATION, ");
        //            }

        //            if (allCa.Any(x => x.Total <= failMark))
        //            {
        //                var failedCourses = allCa.Where(c => c.Total <= failMark).DistinctBy(d => d.CourseId).ToList();

        //                foreach (var course in failedCourses)
        //                {
        //                    var checkForPassedCourse = allCa.Where(x => x.CourseId.Equals(course.CourseId)).ToList();
        //                    if (checkForPassedCourse.Any(x => x.Total > failMark))
        //                    {
        //                        if (!builder.ToString().Contains("RPT  "))
        //                        {
        //                            if (!builder.ToString().Contains("PASS"))
        //                            {
        //                                builder.Append("PASS");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var regCourse = ca.Where(x => x.CourseId.Equals(course.CourseId)).FirstOrDefault();
        //                        //if (regCourse != null)
        //                        //{
        //                        //    if (!rptbuilder.ToString().Contains("REG"))
        //                        //    {
        //                        //        rptbuilder.Append("REG  ");
        //                        //    }
        //                        //    rptbuilder.Append(course.Course.CourseCode);
        //                        //    rptbuilder.Append(", ");
        //                        //}
        //                        //else
        //                        //{
        //                        if (!builder.ToString().Contains("RPT"))
        //                        {
        //                            builder.Append("RPT  ");
        //                        }
        //                        builder.Append(course.Course.CourseCode);
        //                        builder.Append(", ");
        //                        //}

        //                    }

        //                }
        //                summary.Remarks = builder.ToString() + " " + rptbuilder.ToString();
        //            }
        //            else
        //            {
        //                summary.Remarks = "PASS";
        //            }

        //            model.Add(summary);
        //            builder.Clear();

        //            count = count + 1;
        //        }
        //        return model;
        //    }
        //    return null;
        //}

        public int CalculateNumberOfSemesterSpent(string studentId)
        {
            var caList = _db.ContinuousAssessments.Include(i => i.Course).AsNoTracking()
                                        .Where(x => x.StudentId.Equals(studentId))
                                        .DistinctBy(d => new { d.SessionId, d.Course.SemesterId }).ToList();
            return caList.Count();
        }

        public async Task<SessionParameters> CalculatePastResult(string studentId, int levelId, int SessionId, int failMark)
        {
            var ca = await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                       .Where(x => x.StudentId.Equals(studentId) &&
                                       x.Level.LevelId.Equals(levelId) && x.SessionId.Equals(SessionId))
                                       .ToListAsync();
            var level = _query.GetLevelById(levelId);
            var tgp = ca.Sum(x => x.QualityPoint);
            var tcr = ca.Sum(s => s.Course.Credits);
            string gpa = string.Format("{0:F2}", Convert.ToDouble(tgp) / Convert.ToDouble(tcr));
            var model = new SessionParameters()
            {
                LevelName = level.LevelName,
                TGP = tgp,
                TCR = tcr,
                TCE = ca.Where(c => c.Total > failMark).Sum(s => s.Course.Credits),
                GPA = gpa,
                DGPA = Convert.ToDouble(tgp) / Convert.ToDouble(tcr),
                ContinuousAssessments = ca
            };
            return model;
        }

        public async Task<string> CalculateCgpa(string studentId)
        {
            var ca = await _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                       .Where(x => x.StudentId.Equals(studentId))
                                       .ToListAsync();
            var tgp = ca.Sum(x => x.QualityPoint);
            var tcr = ca.Sum(s => s.Course.Credits);
            string cgpa = string.Format("{0:F2}", Convert.ToDouble(tgp) / Convert.ToDouble(tcr));
            return cgpa;
        }

        private async Task<List<SessionParameters>> CheckLevelAndGenerateResult(string studentId, int levelId, int SessionId, int failMark, string modeOfEntry)
        {
            var model = new List<SessionParameters>();
            var level = _query.GetLevelById(levelId);
            var stdSchProg = _query.GetSchoolProgById(levelId);
            string levelName = level.LevelOrder.Trim();
            string sessionName = _query.GetSessionNameById(SessionId);
            if (levelName.Equals("600"))
            {
                model.Add(await CalculatePastResult(studentId, levelId, SessionId, failMark));


                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out int newLevelId, out int newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                if (modeOfEntry == "DE")
                {
                    model.Add(new SessionParameters()
                    {
                        LevelName = "100",
                        TGP = 0,
                        TCR = 0,
                        TCE = 0,
                        GPA = "0.00",
                        DGPA = 0,
                        ContinuousAssessments = _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                      .Where(x => x.StudentId.Equals(studentId) &&
                                      x.Level.LevelId.Equals(newLevelId) && x.SessionId.Equals(newSessionId))
                                      .ToList()
                    });
                }
                else
                {
                    GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                    model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));
                }

            }
            if (levelName.Equals("500"))
            {
                model.Add(await CalculatePastResult(studentId, levelId, SessionId, failMark));


                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out int newLevelId, out int newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                if (modeOfEntry == "DE")
                {
                    model.Add(new SessionParameters()
                    {
                        LevelName = "100",
                        TGP = 0,
                        TCR = 0,
                        TCE = 0,
                        GPA = "0.00",
                        DGPA = 0,
                        ContinuousAssessments = _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                      .Where(x => x.StudentId.Equals(studentId) &&
                                      x.Level.LevelId.Equals(newLevelId) && x.SessionId.Equals(newSessionId))
                                      .ToList()
                    });
                }
                else
                {
                    GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                    model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));
                }
            }
            if (levelName.Equals("400"))
            {
                model.Add(await CalculatePastResult(studentId, levelId, SessionId, failMark));


                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out int newLevelId, out int newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                if (modeOfEntry == "DE")
                {
                    model.Add(new SessionParameters()
                    {
                        LevelName = "100",
                        TGP = 0,
                        TCR = 0,
                        TCE = 0,
                        GPA = "0.00",
                        DGPA = 0,
                        ContinuousAssessments = _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                      .Where(x => x.StudentId.Equals(studentId) &&
                                      x.Level.LevelId.Equals(newLevelId) && x.SessionId.Equals(newSessionId))
                                      .ToList()
                    });
                }
                else
                {
                    GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                    model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));
                }
            }
            else if (levelName.Equals("300"))
            {
                model.Add(await CalculatePastResult(studentId, levelId, SessionId, failMark));

                int newLevelId;
                int newSessionId;

                GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));

                if (modeOfEntry == "DE")
                {
                    model.Add(new SessionParameters()
                    {
                        LevelName = "100",
                        TGP = 0,
                        TCR = 0,
                        TCE = 0,
                        GPA = "0.00",
                        DGPA = 0,
                        ContinuousAssessments = _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                      .Where(x => x.StudentId.Equals(studentId) &&
                                      x.Level.LevelId.Equals(newLevelId) && x.SessionId.Equals(newSessionId))
                                      .ToList()
                    });
                }
                else
                {
                    GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                    model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));
                }

            }
            else if (levelName.Equals("200"))
            {
                model.Add(await CalculatePastResult(studentId, levelId, SessionId, failMark));

                int newLevelId = 0;
                int newSessionId = 0;

                if (modeOfEntry == "DE")
                {
                    model.Add(new SessionParameters()
                    {
                        LevelName = "100",
                        TGP = 0,
                        TCR = 0,
                        TCE = 0,
                        GPA = "0.00",
                        DGPA = 0,
                        ContinuousAssessments = _db.ContinuousAssessments.Include(i => i.Level).Include(i => i.Course).AsNoTracking()
                                      .Where(x => x.StudentId.Equals(studentId) &&
                                      x.Level.LevelId.Equals(newLevelId) && x.SessionId.Equals(newSessionId))
                                      .ToList()
                    });
                }
                else
                {
                    GeneratePreviousLevelAndSession(ref levelName, ref sessionName, out newLevelId, out newSessionId, stdSchProg.SchoolProgrammeId);
                    model.Add(await CalculatePastResult(studentId, newLevelId, newSessionId, failMark));
                }

            }
            else if (levelName.Equals("100"))
            {
                model.Add(await CalculatePastResult(studentId, levelId, SessionId, failMark));
            }

            model = CalculateCaSum(model);
            return model;
        }

        private static List<SessionParameters> CalculateCaSum(List<SessionParameters> model)
        {
            if (model.Count == 6)
            {
                model[0].CTCE = model.Sum(s => s.TCE);
                model[0].CTCR = model.Sum(s => s.TCR);
                model[0].CTGP = model.Sum(s => s.TGP);
                model[0].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[0].CTGP) / Convert.ToDouble(model[0].CTCR));

                model[1].CTCE = model[1].TCE + model[2].TCE + model[3].TCE;
                model[1].CTCR = model[1].TCR + model[2].TCR + model[3].TCR;
                model[1].CTGP = model[1].TGP + model[2].TGP + model[3].TGP;
                model[1].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[1].CTGP) / Convert.ToDouble(model[1].CTCR));

                model[2].CTCE = model[2].TCE + model[3].TCE;
                model[2].CTCR = model[2].TCR + model[3].TCR;
                model[2].CTGP = model[2].TGP + model[3].TGP;
                model[2].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[2].CTGP) / Convert.ToDouble(model[2].CTCR));

                model[3].CTCE = model[3].TCE + model[4].TCE;
                model[3].CTCR = model[3].TCR + model[4].TCR;
                model[3].CTGP = model[3].TGP + model[4].TGP;
                model[3].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[3].CTGP) / Convert.ToDouble(model[3].CTCR));

                model[4].CTCE = model[4].TCE + model[3].TCE;
                model[4].CTCR = model[4].TCR + model[3].TCR;
                model[4].CTGP = model[4].TGP + model[3].TGP;
                model[4].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[4].CTGP) / Convert.ToDouble(model[4].CTCR));
            }
            if (model.Count == 5)
            {
                model[0].CTCE = model.Sum(s => s.TCE);
                model[0].CTCR = model.Sum(s => s.TCR);
                model[0].CTGP = model.Sum(s => s.TGP);
                model[0].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[0].CTGP) / Convert.ToDouble(model[0].CTCR));

                model[1].CTCE = model[1].TCE + model[2].TCE + model[3].TCE;
                model[1].CTCR = model[1].TCR + model[2].TCR + model[3].TCR;
                model[1].CTGP = model[1].TGP + model[2].TGP + model[3].TGP;
                model[1].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[1].CTGP) / Convert.ToDouble(model[1].CTCR));

                model[2].CTCE = model[2].TCE + model[3].TCE;
                model[2].CTCR = model[2].TCR + model[3].TCR;
                model[2].CTGP = model[2].TGP + model[3].TGP;
                model[2].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[2].CTGP) / Convert.ToDouble(model[2].CTCR));

                model[3].CTCE = model[3].TCE + model[4].TCE;
                model[3].CTCR = model[3].TCR + model[4].TCR;
                model[3].CTGP = model[3].TGP + model[4].TGP;
                model[3].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[3].CTGP) / Convert.ToDouble(model[3].CTCR));
            }
            if (model.Count == 4)
            {
                model[0].CTCE = model.Sum(s => s.TCE);
                model[0].CTCR = model.Sum(s => s.TCR);
                model[0].CTGP = model.Sum(s => s.TGP);
                model[0].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[0].CTGP) / Convert.ToDouble(model[0].CTCR));

                model[1].CTCE = model[1].TCE + model[2].TCE + model[3].TCE;
                model[1].CTCR = model[1].TCR + model[2].TCR + model[3].TCR;
                model[1].CTGP = model[1].TGP + model[2].TGP + model[3].TGP;
                model[1].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[1].CTGP) / Convert.ToDouble(model[1].CTCR));

                model[2].CTCE = model[2].TCE + model[3].TCE;
                model[2].CTCR = model[2].TCR + model[3].TCR;
                model[2].CTGP = model[2].TGP + model[3].TGP;
                model[2].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[2].CTGP) / Convert.ToDouble(model[2].CTCR));
            }
            if (model.Count == 3)
            {
                model[0].CTCE = model.Sum(s => s.TCE);
                model[0].CTCR = model.Sum(s => s.TCR);
                model[0].CTGP = model.Sum(s => s.TGP);
                model[0].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[0].CTGP) / Convert.ToDouble(model[0].CTCR));

                model[1].CTCE = model[1].TCE + model[2].TCE;
                model[1].CTCR = model[1].TCR + model[2].TCR;
                model[1].CTGP = model[1].TGP + model[2].TGP;
                model[1].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[1].CTGP) / Convert.ToDouble(model[1].CTCR));

            }
            if (model.Count == 2)
            {
                model[0].CTCE = model.Sum(s => s.TCE);
                model[0].CTCR = model.Sum(s => s.TCR);
                model[0].CTGP = model.Sum(s => s.TGP);
                model[0].CGPA = string.Format("{0:F2}", Convert.ToDouble(model[0].CTGP) / Convert.ToDouble(model[0].CTCR));

            }
            if (model.Count == 1)
            {
                model[0].CGPA = string.Format("{0:F2}", model.Sum(s => s.DGPA));
            }
            return model;
        }

        private void GeneratePreviousLevelAndSession(ref string levelName, ref string sessionName, out int newLevelId, out int newSessionId, int schoolProgId)
        {
            levelName = _query.GetPreviousLevel(levelName);
            newLevelId = _query.GetLevelByName(levelName);
            sessionName = _query.GetPreviousSession(sessionName, schoolProgId);
            newSessionId = _query.GetSessionIdByName(sessionName);
        }

        private int GeneratePreviousLevel(ref string levelName)
        {
            levelName = _query.GetPreviousLevel(levelName);
            var newLevelId = _query.GetLevelByName(levelName);
            return newLevelId;
        }

        public int GetLevelByName(string levelName)
        {
            return _query.GetLevelByName(levelName);
        }


        public Tuple<int, int> GetAdmittedSession(int levelId, int sessionId, int schoolProgrammeId)
        {
            var levelOrder = _db.Levels.AsNoTracking().Where(x => x.LevelId.Equals(levelId)).Select(s => s.LevelOrder)
                                .FirstOrDefault();
            var sessionName = _db.Sessions.AsNoTracking().Where(x => x.SessionId.Equals(sessionId))
                                    .Select(s => s.SessionName).FirstOrDefault();
            if (levelOrder.Equals("100"))
            {
                return new Tuple<int, int>(sessionId, 0);
            }
            if (levelOrder.Equals("200"))
            {
                var newSessionName = _query.GetPreviousSession(sessionName, schoolProgrammeId);
                var newSessioId = _query.GetSessionIdByName(newSessionName);
                var deSessionId = _query.GetSessionIdByName(sessionName);
                return new Tuple<int, int>(newSessioId, deSessionId);
            }
            if (levelOrder.Equals("300"))
            {
                var newSessionName = _query.GetPreviousSession(sessionName, schoolProgrammeId);
                var deSessionId = _query.GetSessionIdByName(newSessionName);
                newSessionName = _query.GetPreviousSession(newSessionName, schoolProgrammeId);
                var newSessioId = _query.GetSessionIdByName(newSessionName);
                return new Tuple<int, int>(newSessioId, deSessionId);
            }
            if (levelOrder.Equals("400"))
            {
                var newSessionName = _query.GetPreviousSession(sessionName, schoolProgrammeId);
                newSessionName = _query.GetPreviousSession(newSessionName, schoolProgrammeId);
                var deSessionId = _query.GetSessionIdByName(newSessionName);
                newSessionName = _query.GetPreviousSession(newSessionName, schoolProgrammeId);

                var newSessioId = _query.GetSessionIdByName(newSessionName);
                return new Tuple<int, int>(newSessioId, deSessionId);
            }
            if (levelOrder.Equals("500"))
            {
                var newSessionName = _query.GetPreviousSession(sessionName, schoolProgrammeId);
                newSessionName = _query.GetPreviousSession(newSessionName, schoolProgrammeId);
                newSessionName = _query.GetPreviousSession(newSessionName, schoolProgrammeId);
                var deSessionId = _query.GetSessionIdByName(newSessionName);
                newSessionName = _query.GetPreviousSession(newSessionName, schoolProgrammeId);

                var newSessioId = _query.GetSessionIdByName(newSessionName);
                return new Tuple<int, int>(newSessioId, deSessionId);
            }
            return new Tuple<int, int>(0, 0);

        }

        public bool GetStudentSiwesStatus(int programmeId, string studentId)
        {
            bool isQualified = false;
            var preRequisiteCourses = _db.CoursePrerequisites.AsNoTracking().Include(i => i.Course.Programme)
                                    .Where(x => x.Course.Programme.ProgrammeId.Equals(programmeId)
                                    && x.IsSiwes.Equals(true)).ToList();

            var studentCaList = _db.ContinuousAssessments.AsNoTracking()
                                .Where(x => x.StudentId.Equals(studentId) && x.IsSenateApproved.Equals(true))
                                .ToList();
            foreach (var course in preRequisiteCourses)
            {
                isQualified = studentCaList.Any(x => x.CourseId.Equals(course) && x.Total > 39);
            }
            return isQualified;
        }

        public List<Student> GetSiwesList(int programmeId, int levelId)
        {
            var isDisQualified = false;
            var students = new List<Student>();
            var studentInProgramme = _db.Students.Include(i => i.Programme).AsNoTracking()
                                        .Where(x => x.Programme.ProgrammeId.Equals(programmeId) && x.Level.LevelId.Equals(levelId))
                                        .ToList();
            var preRequisiteCourses = _db.CoursePrerequisites.AsNoTracking().Include(i => i.Course.Programme)
                                    .Where(x => x.Course.Programme.ProgrammeId.Equals(programmeId)
                                    && x.IsSiwes.Equals(true)).ToList();
            foreach (var studentId in studentInProgramme)
            {
                var studentCaList = _db.ContinuousAssessments.AsNoTracking()
                                .Where(x => x.StudentId.Equals(studentId) && x.IsSenateApproved.Equals(true))
                                .ToList();
                foreach (var course in preRequisiteCourses)
                {
                    isDisQualified = studentCaList.Any(x => x.CourseId.Equals(course) && x.Total < 39);
                    
                }
                if (isDisQualified.Equals(false))
                {
                    students.Add(studentId);
                }
            }
            return students;            
        }

        public async Task<List<Student>> StudentRegForCourse(int programmeId, int sessionId)
        {
            var preRequisiteCourseId = GetSiwesCourseForProgramme(programmeId);
            if (preRequisiteCourseId > 0)
            {
                return await _db.CourseRegistrations.AsNoTracking().Include(i => i.Programme).Include(i => i.Level)
                           .Where(x => x.CourseId.Equals(preRequisiteCourseId) && x.SessionId.Equals(sessionId))
                           .Select(s => s.Students).ToListAsync();
            }
            return null;           
        }

        public int GetSiwesCourseForProgramme(int programmeId)
        {
            return _db.CoursePrerequisites.AsNoTracking().Include(i => i.Course.Programme)
                                   .Where(x => x.Course.Programme.ProgrammeId.Equals(programmeId)
                                   && x.IsSiwes.Equals(true)).Select(x => x.CourseId).FirstOrDefault();
        }

        public async Task<string> getCourseCode(int courseId)
        {
            return await _db.Courses.AsNoTracking().Where(x => x.CourseId.Equals(courseId)).Select(x => x.CourseCode).FirstOrDefaultAsync();
        }
        
        public async Task<int> getCourseIdForStudentProgramme(int programmeId, string courseCode)
        {
            return await _db.Courses.Include(i => i.Programme).AsNoTracking()
                                     .Where(x => x.Programme.ProgrammeId.Equals(programmeId) && x.CourseCode.Trim().ToUpper()
                                     .Equals(courseCode.Trim().ToUpper()))
                                     .Select(x => x.CourseId).FirstOrDefaultAsync();
        }
    }
}