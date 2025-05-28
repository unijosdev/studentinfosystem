using SwiftKampus.Abstractions;
using SwiftKampus.Models;
using SwiftKampus.ViewModels;
using SwiftKampusModel;
using SwiftKampusModel.AddmissionApplicant;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;


namespace SwiftKampus.BusinessLogic
{
    public class StudentQueryManager : IStudentQueryManager
    {
        private readonly SchoolDbContext _db;

        public StudentQueryManager(SchoolDbContext db)
        {
            _db = db;
        }

        private List<StudentIndexVM> ValidateClearanceStatus(List<StudentIndexVM> model)
        {
            var studentIndexList = new List<StudentIndexVM>();
            foreach (var student in model)
            {
                var checkBioData = new ClearanceValidationVm(_db, student.Email, student.SchoolProgrammeCode, student.ModeOfEntry);
                if (!string.IsNullOrEmpty(student.ModeOfEntry)
                    && (student.ModeOfEntry.Equals("UTME") || student.ModeOfEntry.Equals("DE") || student.ModeOfEntry.Equals("RS") || student.ModeOfEntry.Equals("IOE")))
                {
                    if (student.SchoolProgrammeCode.Equals(ProgrammeCategory.UnderGraduate.ToString()) || student.SchoolProgrammeCode.Equals(ProgrammeCategory.Institute_Of_Education.ToString()))
                    {
                        if (student.ModeOfEntry.Equals(ModeOfEntry.DE.ToString()))
                        {
                            if (checkBioData.BioData && checkBioData.OLevelResult && checkBioData.NextOfKin
                               && checkBioData.Address && checkBioData.Sponsors && checkBioData.DirectEntryExam)
                            {
                                studentIndexList.Add(student);
                            }
                        }
                        else
                        {
                            if (checkBioData.BioData && checkBioData.OLevelResult && checkBioData.NextOfKin
                               && checkBioData.Address && checkBioData.Sponsors && checkBioData.UploadDocument)
                            {
                                studentIndexList.Add(student);
                            }
                        }
                    }
                    else if (student.ModeOfEntry.Equals(ModeOfEntry.RS.ToString()))
                    {
                        if (checkBioData.BioData && checkBioData.OLevelResult && checkBioData.NextOfKin
                          && checkBioData.Address)
                        {
                            studentIndexList.Add(student);
                        }
                    }
                }
                else
                {
                    if (student.SchoolProgrammeCode.Equals(ProgrammeCategory.Masters.ToString())
                       || student.SchoolProgrammeCode.Equals(ProgrammeCategory.Post_Graduate.ToString())
                       || student.SchoolProgrammeCode.Equals(ProgrammeCategory.MBA.ToString()))
                    {
                        //if (checkBioData.BioData && checkBioData.OLevelResult && checkBioData.NextOfKin
                        //        && checkBioData.Address && checkBioData.EmploymentHistory && checkBioData.AcademicQualification
                        //        && checkBioData.Award && checkBioData.RelevanQualification && checkBioData.Publication
                        //        && checkBioData.Referee && checkBioData.UploadDocument)

                        if (checkBioData.BioData && checkBioData.OLevelResult && checkBioData.AcademicQualification && checkBioData.Referee)
                        {
                            studentIndexList.Add(student);
                        }
                    }
                    if (student.SchoolProgrammeCode.Equals(ProgrammeCategory.Phd.ToString()))
                    {
                        //if (checkBioData.BioData && checkBioData.OLevelResult && checkBioData.NextOfKin
                        //        && checkBioData.Address && checkBioData.EmploymentHistory && checkBioData.AcademicQualification
                        //        && checkBioData.Award && checkBioData.RelevanQualification && checkBioData.Publication
                        //        && checkBioData.Referee && checkBioData.Thesis && checkBioData.UploadDocument)

                        if (checkBioData.BioData && checkBioData.OLevelResult && checkBioData.AcademicQualification && checkBioData.Referee)
                        {
                            studentIndexList.Add(student);
                        }
                    }
                }
            }
            return studentIndexList;
        }

        public async Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId, bool activeStudent, int? facultyId, int? departmentId,
                                    int? programmeId, string ClearanceStage, int? SessionId, int? LevelId)
        {
            var model = new List<StudentIndexVM>();
            var studentList = new List<Student>();
            if (schoolProgrammeId != null)
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department.Faculty)
                                    .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                                    && x.Active.Equals(activeStudent))
                                   .ToListAsync();
            }
            else
            {
                studentList = await _db.Students.Include(i => i.Programme.Department.Faculty).Include(i => i.Level)
                             .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                            .Where(x => x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                            && x.Active.Equals(activeStudent))
                           .ToListAsync();
            }

            if (programmeId != null)
            {
                studentList = studentList.Where(x => x.Programme.ProgrammeId.Equals((int)programmeId)).ToList();
            }
            else if (departmentId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.DepartmentId.Equals((int)departmentId)).ToList();
            }
            else if (facultyId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)).ToList();
            }

            if (SessionId != null)
            {
                studentList = studentList.Where(x => x.Session.SessionId.Equals((int)SessionId)).ToList();
            }

            if (LevelId != null)
            {
                studentList = studentList.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Department"))
            {
                studentList = studentList.Where(x => x.IsClearedDepartment.Equals(true)
                            && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())).ToList();
            }
            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Academic"))
            {
                studentList = studentList.Where(x => x.IsClearedAcademics.Equals(true)
                            && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())).ToList();
            }

            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Faculty"))
            {
                studentList = studentList.Where(x => x.IsClearedFaculty.Equals(true)
                                && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())).ToList();
            }

            return studentList.Select(s => new StudentIndexVM()
            {
                StudentId = s.StudentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                MiddleName = s.MiddleName,
                Gender = s.Gender,
                ProgrammeName = s.Programme.ProgrammeName,
                MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                PhoneNumber = s.PhoneNumber,
                JambRegNo = s.JambRegNo,
                LevelName = s.Level?.LevelName ?? "",
                ModeOfEntry = s.ModeOfEntry,
                IsScholarship = s.IsSchoolarshipStudent.Equals(true) ? "Activated" : "Not Scholarship",
                StateOfOrigin = s.StateOfOrigin,
                Email = s.Email,
            }).OrderBy(x => x.FullName).ToList();
        }

        public async Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId, int? facultyId, int? departmentId,
                                   int? programmeId, int? SessionId, int? LevelId)
        {
            var model = new List<StudentIndexVM>();
            var studentList = new List<Student>();
            if (schoolProgrammeId != null)
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department.Faculty)
                                    .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                                    && x.Active.Equals(true))
                                   .ToListAsync();
            }
            else
            {
                studentList = await _db.Students.Include(i => i.Programme.Department.Faculty).Include(i => i.Level)
                             .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                            .Where(x => x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                            && x.Active.Equals(true))
                           .ToListAsync();
            }
            studentList = studentList.Where(x => x.IsDownloaded != true).ToList();
            if (programmeId != null)
            {
                studentList = studentList.Where(x => x.Programme.ProgrammeId.Equals((int)programmeId)).ToList();
            }
            else if (departmentId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.DepartmentId.Equals((int)departmentId)).ToList();
            }
            else if (facultyId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)).ToList();
            }

            if (SessionId != null)
            {
                studentList = studentList.Where(x => x.Session.SessionId.Equals((int)SessionId)).ToList();
            }

            if (LevelId != null)
            {
                studentList = studentList.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }


            return studentList.Select(s => new StudentIndexVM()
            {
                StudentId = s.StudentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                MiddleName = s.MiddleName,
                Gender = s.Gender,
                ProgrammeName = s.Programme.ProgrammeName,
                MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                PhoneNumber = !string.IsNullOrEmpty(s.PhoneNumber) ? s.PhoneNumber : "",
                JambRegNo = s.JambRegNo,
                LevelName = s.Level?.LevelName ?? "",
                ModeOfEntry = s.ModeOfEntry,
                //Email = !string.IsNullOrEmpty(s.Email) ? s.Email : !string.IsNullOrEmpty(s.PrimaryEmail) ? s.PrimaryEmail : "",
                Email = !string.IsNullOrEmpty(s.PrimaryEmail) ? s.PrimaryEmail : !string.IsNullOrEmpty(s.Email) ? s.Email : "",
                BloodGroup = s.BloodGroup
            }).OrderBy(x => x.FullName).ToList();
        }

        public async Task<List<StudentIndexVM>> GetStudentListFor365EmailUpload(int? schoolProgrammeId, int? facultyId, int? departmentId,
                                   int? programmeId, int? SessionId, int? LevelId, string isDownloded)
        {
            var model = new List<StudentIndexVM>();
            var studentList = new List<Student>();
            if (schoolProgrammeId != null)
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department.Faculty)
                                    .Include(i => i.Programme.Department)
                                    .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                                    && x.Active.Equals(true) && x.SessionId == (int)SessionId && x.MatricNo != null).Take(10000)
                                   .ToListAsync();
            }
            else
            {
                studentList = await _db.Students.Include(i => i.Programme.Department.Faculty).Include(i => i.Level)
                                    .Include(i => i.Programme.Department)
                             .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                            .Where(x => x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                            && x.Active.Equals(true) && x.MatricNo != null)
                           .ToListAsync();
            }
            bool boolValue;
            bool.TryParse(isDownloded, out boolValue);
            studentList = studentList.Where(x => x.IsDownloaded.Equals(isDownloded) || x.IsDownloaded == null).ToList();
            if (programmeId != null)
            {
                studentList = studentList.Where(x => x.Programme.ProgrammeId.Equals((int)programmeId)).ToList();
            }
            else if (departmentId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.DepartmentId.Equals((int)departmentId)).ToList();
            }
            else if (facultyId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)).ToList();
            }

            if (SessionId != null)
            {
                studentList = studentList.Where(x => x.Session.SessionId.Equals((int)SessionId)).ToList();
            }

            if (LevelId != null)
            {
                studentList = studentList.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }


            return studentList.Select(s => new StudentIndexVM()
            {
                StudentId = s.StudentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                MiddleName = s.MiddleName,
                Gender = s.Gender,
                ProgrammeName = s.Programme.ProgrammeName,
                DeptName = s.Programme.Department.DeptName,
                MatricNo = s.MatricNo,
                PhoneNumber = !string.IsNullOrEmpty(s.PhoneNumber) ? s.PhoneNumber : "",
                JambRegNo = s.JambRegNo,
                LevelName = s.Level?.LevelName ?? "",
                ModeOfEntry = s.ModeOfEntry,
                //Email = !string.IsNullOrEmpty(s.Email) ? s.Email : !string.IsNullOrEmpty(s.PrimaryEmail) ? s.PrimaryEmail : "",
                Email = s.Email,
                BloodGroup = s.BloodGroup
            }).OrderBy(x => x.FullName).ToList();
        }

        public async Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId, bool activeStudent, int? facultyId, int? departmentId,
                                    int? programmeId, string ClearanceStage, int? SessionId, int? LevelId, string gender, string StateOfOrigin)
        {
            var model = new List<StudentIndexVM>();
            var studentList = new List<Student>();
            var studentList2 = new List<Student>();
            if (schoolProgrammeId != null)
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department.Faculty)
                                    .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                                    && x.Active.Equals(activeStudent))
                                   .ToListAsync();
            }
            else
            {
                studentList = await _db.Students.Include(i => i.Programme.Department.Faculty).Include(i => i.Level)
                             .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                            .Where(x => x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                            && x.Active.Equals(activeStudent))
                           .ToListAsync();
            }

            if (programmeId != null)
            {
                studentList = studentList.Where(x => x.Programme.ProgrammeId.Equals((int)programmeId)).ToList();
            }
            else if (departmentId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.DepartmentId.Equals((int)departmentId)).ToList();
            }
            else if (facultyId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)).ToList();
            }

            if (SessionId != null)
            {
                studentList = studentList.Where(x => x.Session.SessionId.Equals((int)SessionId)).ToList();
            }

            if (LevelId != null)
            {
                studentList = studentList.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            if (!string.IsNullOrEmpty(gender) && !gender.Equals("null"))
            {
                studentList = studentList.Where(x => x.Gender.Equals(gender)).ToList();
            }


            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Department"))
            {
                studentList = studentList.Where(x => x.IsClearedDepartment.Equals(true)
                            && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())).ToList();
            }
            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Academic"))
            {
                studentList = studentList.Where(x => x.IsClearedAcademics.Equals(true)
                            && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())).ToList();
            }

            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Faculty"))
            {
                studentList = studentList.Where(x => x.IsClearedFaculty.Equals(true)
                                && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())).ToList();
            }

            if (!string.IsNullOrEmpty(StateOfOrigin) && !StateOfOrigin.Equals("select_state"))
            {
                foreach (var item in studentList)
                {
                    if (item.StateOfOrigin != null && item.StateOfOrigin.Equals(StateOfOrigin))
                    {
                        //studentList2 = studentList.Where(x => x.StateOfOrigin.Equals(StateOfOrigin)).ToList();

                        studentList2.Add(item);
                    }
                }
                return studentList2.Select(s => new StudentIndexVM()
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    MiddleName = s.MiddleName,
                    Gender = s.Gender,
                    ProgrammeName = s.Programme.ProgrammeName,
                    MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                    PhoneNumber = s.PhoneNumber,
                    JambRegNo = s.JambRegNo,
                    LevelName = s.Level?.LevelName ?? "",
                    ModeOfEntry = s.ModeOfEntry,
                    StateOfOrigin = s.StateOfOrigin ?? "",
                    Email = s.Email,
                    IsScholarship = s.IsSchoolarshipStudent.Equals(true) ? "Activated" : "Not Scholarship"
                }).OrderBy(x => x.FullName).ToList();
            }


            return studentList.Select(s => new StudentIndexVM()
            {
                StudentId = s.StudentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                MiddleName = s.MiddleName,
                Gender = s.Gender,
                ProgrammeName = s.Programme.ProgrammeName,
                MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                PhoneNumber = s.PhoneNumber,
                JambRegNo = s.JambRegNo,
                LevelName = s.Level?.LevelName ?? "",
                ModeOfEntry = s.ModeOfEntry,
                StateOfOrigin = s.StateOfOrigin ?? "",
                Email = s.Email,
                IsScholarship = s.IsSchoolarshipStudent.Equals(true) ? "Activated" : "Not Scholarship"
            }).OrderBy(x => x.FullName).ToList();
        }
        public async Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId, bool activeStudent)
        {
            if (schoolProgrammeId != null)
            {
                return await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                                    && x.Active.Equals(activeStudent))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();
            }
            return await _db.Students.Include(i => i.Programme).Include(i => i.Level)
                                .Include(i => i.Level).AsNoTracking()
                               .Where(x => x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                               && x.Active.Equals(activeStudent))
                               .Select(s => new StudentIndexVM()
                               {
                                   StudentId = s.StudentId,
                                   FirstName = s.FirstName,
                                   LastName = s.LastName,
                                   MiddleName = s.MiddleName,
                                   Gender = s.Gender,
                                   ProgrammeName = s.Programme.ProgrammeName,
                                   MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                   PhoneNumber = s.PhoneNumber,
                                   JambRegNo = s.JambRegNo,
                                   LevelName = s.Level.LevelName,
                                   ModeOfEntry = s.ModeOfEntry
                               }).ToListAsync();

        }

        public async Task<List<LibraryPatronUploadVm>> GetStudentListForLibraryUpload(int? schoolProgrammeId, int? facultyId, int? departmentId,
                                  int? programmeId, int? SessionId, int? LevelId)
        {
            var model = new List<LibraryPatronUploadVm>();
            var studentList = new List<Student>();
            if (schoolProgrammeId != null)
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department)
                                    .Include(i => i.Programme.Department.Faculty).Include(i => i.Programme)
                                    .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                                    && x.Active.Equals(true) && !string.IsNullOrEmpty(x.MatricNo))
                                   .ToListAsync();
            }
            else
            {
                studentList = await _db.Students.Include(i => i.Programme.Department.Faculty)
                                .Include(i => i.Programme.Department).Include(i => i.SchoolProgramme)
                                .Include(i => i.Level).Include(i => i.Programme)
                                .Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                .Where(x => x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)
                                && x.Active.Equals(true) && !string.IsNullOrEmpty(x.MatricNo))
                                .ToListAsync();
            }
            studentList = studentList.Where(x => x.IsDownloaded != true).ToList();
            if (programmeId != null)
            {
                studentList = studentList.Where(x => x.Programme.ProgrammeId.Equals((int)programmeId)).ToList();
            }
            else if (departmentId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.DepartmentId.Equals((int)departmentId)).ToList();
            }
            else if (facultyId != null)
            {
                studentList = studentList.Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)).ToList();
            }

            if (SessionId != null)
            {
                studentList = studentList.Where(x => x.Session.SessionId.Equals((int)SessionId)).ToList();
            }

            if (LevelId != null)
            {
                studentList = studentList.Where(x => x.Level.LevelId.Equals((int)LevelId)).ToList();
            }

            foreach (var item in studentList)
            {
                var address = _db.Addresses.Where(a => a.UserId.Trim().ToUpper().Equals(item.Email.Trim().ToUpper()) || a.UserId.Trim().ToUpper().Equals(item.PrimaryEmail.Trim().ToUpper())).FirstOrDefault();
                var contact = _db.NextOfKins.Where(c => c.UserId.Trim().ToUpper().Equals(item.Email.Trim().ToUpper()) || c.UserId.Trim().ToUpper().Equals(item.PrimaryEmail.Trim().ToUpper())).FirstOrDefault();

                model.Add(new LibraryPatronUploadVm
                {
                    cardnumber = item?.MatricNo ?? "",
                    surname = item?.LastName ?? "",
                    firstname = item?.FirstName ?? "",
                    title = item.Gender == null ? "" : item.Gender.Equals(Gender.Male.ToString()) ? Title.Mr.ToString() : Title.Mrs.ToString(),
                    othernames = item?.MiddleName ?? "",
                    initials = item?.FirstName ?? "",
                    streetnumber = address?.HouseNo ?? "",
                    //streettype = "",
                    address = address?.Street ?? "",
                    //address2 = "",
                    city = address?.Street ?? "",
                    state = item?.StateOfOrigin ?? "",
                    //zipcode = "",
                    country = item?.Nationality.ToString() ?? "",
                    email = !string.IsNullOrEmpty(item.PrimaryEmail) ? item.PrimaryEmail : !string.IsNullOrEmpty(item.PrimaryEmail) ? item.Email : "",
                    phone = item?.PhoneNumber ?? "",
                    //mobile = "",
                    //fax = "",
                    //emailpro = "",
                    //phonepro = "",
                    //B_streetnumber = "",
                    //B_streettype = "",
                    B_address = item?.Programme.Department.Faculty.FacultyName ?? "",
                    B_address2 = item?.Programme.Department.DeptName?? "",
                    B_city = "Jos",
                    B_state = "Plateau",
                    B_zipcode = "930001",
                    B_country = "Nigeria",
                    B_email = item?.PrimaryEmail ?? "",
                    B_phone = item?.PhoneNumber ?? "",
                    dateofbirth = item?.DateOfBirth.ToString("yyyy-MM -dd") ?? "",
                    branchcode = getBranchCode(item)?? "",
                    categorycode = item.SchoolProgramme.SchoolProgrammeCode == "UG" ? "STD" : "PG",
                    dateenrolled = DateTime.Now.ToString("yyyy-MM-dd"),
                    //dateexpiry = "",
                    //date_renewed = "",
                    //gonenoaddress = "",
                    //lost = "",
                    //debarred = "",
                    //debarredcomment = "",
                    //contactname = "",
                    //contactfirstname = "",
                    //contacttitle = "",
                    //guarantorid = "",
                    //borrowernotes = "",
                    //relationship = "",
                    sex = item?.Gender ?? "",
                    password = "pass",
                    //flags = "",
                    userid = item?.MatricNo ?? "",
                    //opacnote = "",
                    //contactnote = "",
                    //sort1 = "",
                    //sort2 = "",
                    altcontactfirstname = contact?.FirstName ?? "",
                    altcontactsurname = contact?.LastName ?? "",
                    altcontactaddress1 = contact?.Address ??"",
                    //altcontactaddress2 = "",
                    //altcontactaddress3 = "",
                    //altcontactstate = "",
                    //altcontactzipcode = "",
                    altcontactcountry = item?.Nationality.ToString() ?? "",
                    altcontactphone = contact?.PhoneNumber ?? "",
                    //smsalertnumber = "",
                    //sms_provider_id = "",
                    //privacy = "",
                    //privacy_guarantor_checkouts  = "",
                    //checkprevcheckout = "",
                    //updated_on = "",
                    //lastseen = "",
                    //lang = "",
                    //login_attempts = "",
                    //overdrive_auth_token = ""
                    
                    
                });
            }
            return model;
        }

        private string getBranchCode(Student item)
        {
            var BranchCode = "";
            switch (item.Programme.Department.Faculty.FacultyCode)
            {
                case "AG":
                case "PH":
                case "NS":
                case "VM":
                    return "ujmai";

                case "AR":
                case "SS":
                case "MS":
                case "ED":
                case "EV":
                case "EN":
                    return "ujlass";

                case "BS":
                case "CS":
                case "DS":
                case "HS":
                    return "ujmed";

                default:
                    return "ujlaw";
            }

            return BranchCode;
        }
        public async Task<List<StudentIndexVM>> GetStudentList(int? schoolProgrammeId)
        {
            if (schoolProgrammeId != null)
            {
                return await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();
            }
            return await _db.Students.Include(i => i.Programme).Include(i => i.Level)
                                .Include(i => i.Level).AsNoTracking()
                               .Where(x => x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                               .Select(s => new StudentIndexVM()
                               {
                                   StudentId = s.StudentId,
                                   FirstName = s.FirstName,
                                   LastName = s.LastName,
                                   MiddleName = s.MiddleName,
                                   Gender = s.Gender,
                                   ProgrammeName = s.Programme.ProgrammeName,
                                   MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                   PhoneNumber = s.PhoneNumber,
                                   JambRegNo = s.JambRegNo,
                                   LevelName = s.Level.LevelName,
                                   ModeOfEntry = s.ModeOfEntry
                               }).ToListAsync();

        }


        //Academic Clearance
        public async Task<List<StudentIndexVM>> GetStudentAcademicList(int? schoolProgrammeId, int? facultyId, bool isCleared, int? SessionId)
        {
            var model = new List<StudentIndexVM>();

            var UGSchoolProgramme = await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)).FirstOrDefaultAsync();

            if (UGSchoolProgramme.SchoolProgrammeCode.ToUpper().ToString().Equals("UG"))
            {
                if (schoolProgrammeId != null && facultyId != null && SessionId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                     .Include(i => i.Programme.Department).AsNoTracking()
                                     .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                     && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                     //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                     //&& x.IsClearedAll.Equals(false)
                                     //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                     && x.IsClearedAcademics.Equals(isCleared)
                                     && x.Session.SessionId.Equals((int)SessionId)
                                     && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                     .Select(s => new StudentIndexVM()
                                     {
                                         StudentId = s.StudentId,
                                         FirstName = s.FirstName,
                                         LastName = s.LastName,
                                         MiddleName = s.MiddleName,
                                         Gender = s.Gender,
                                         ProgrammeName = s.Programme.ProgrammeName,
                                         MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                         PhoneNumber = s.PhoneNumber,
                                         JambRegNo = s.JambRegNo,
                                         ModeOfEntry = s.ModeOfEntry,
                                         LevelName = s.Level.LevelName,
                                         SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                     }).Take(2500).ToListAsync();
                }
                else if (schoolProgrammeId != null && facultyId != null && SessionId == null)  // When session is not specified
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                     .Include(i => i.Programme.Department).AsNoTracking()
                                     .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                     && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                     //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                     //&& x.IsClearedAll.Equals(false)
                                     //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                     && x.IsClearedAcademics.Equals(isCleared)
                                     && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                     .Select(s => new StudentIndexVM()
                                     {
                                         StudentId = s.StudentId,
                                         FirstName = s.FirstName,
                                         LastName = s.LastName,
                                         MiddleName = s.MiddleName,
                                         Gender = s.Gender,
                                         ProgrammeName = s.Programme.ProgrammeName,
                                         MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                         PhoneNumber = s.PhoneNumber,
                                         JambRegNo = s.JambRegNo,
                                         ModeOfEntry = s.ModeOfEntry,
                                         LevelName = s.Level.LevelName,
                                         SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                     }).Take(2500).ToListAsync();
                }
                else if (schoolProgrammeId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsClearedAcademics.Equals(isCleared)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order to start with clearance; To revert, uncomment!
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2500).ToListAsync();
                }
                else if (facultyId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsClearedAcademics.Equals(isCleared)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order to start with clearance; To revert, uncomment!
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2500).ToListAsync();
                }
                else
                {
                    model = await _db.Students.Include(i => i.Programme)
                               .Include(i => i.Programme.Department).AsNoTracking()
                               .Where(x => x.IsDelete.Equals(false)
                               //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                               //&& x.IsClearedAll.Equals(false)
                               && x.IsClearedAcademics.Equals(isCleared)
                               && x.Session.SessionId.Equals((int)SessionId)
                               //&& x.IsClearedDepartment.Equals(true) //Commented to change clearance order to start with clearance; To revert, uncomment!
                               && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                               .Select(s => new StudentIndexVM()
                               {
                                   StudentId = s.StudentId,
                                   FirstName = s.FirstName,
                                   LastName = s.LastName,
                                   MiddleName = s.MiddleName,
                                   Gender = s.Gender,
                                   ProgrammeName = s.Programme.ProgrammeName,
                                   MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                   PhoneNumber = s.PhoneNumber,
                                   JambRegNo = s.JambRegNo,
                                   SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                   Email = s.Email,
                                   ModeOfEntry = s.ModeOfEntry
                               }).Take(2500).ToListAsync();
                }
            }
            else
            {
                if (schoolProgrammeId != null && facultyId != null && SessionId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                     .Include(i => i.Programme.Department).AsNoTracking()
                                     .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                     && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                     //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                     //&& x.IsClearedAll.Equals(false)
                                     && x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                     && x.IsClearedAcademics.Equals(isCleared)
                                     && x.Session.SessionId.Equals((int)SessionId)
                                     && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                     .Select(s => new StudentIndexVM()
                                     {
                                         StudentId = s.StudentId,
                                         FirstName = s.FirstName,
                                         LastName = s.LastName,
                                         MiddleName = s.MiddleName,
                                         Gender = s.Gender,
                                         ProgrammeName = s.Programme.ProgrammeName,
                                         MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                         PhoneNumber = s.PhoneNumber,
                                         JambRegNo = s.JambRegNo,
                                         ModeOfEntry = s.ModeOfEntry,
                                         LevelName = s.Level.LevelName,
                                         SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                     }).Take(2500).ToListAsync();
                }
                else if (schoolProgrammeId != null && facultyId != null && SessionId == null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                     .Include(i => i.Programme.Department).AsNoTracking()
                                     .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                     && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                     //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                     //&& x.IsClearedAll.Equals(false)
                                     && x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                     && x.IsClearedAcademics.Equals(isCleared)
                                     && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                     .Select(s => new StudentIndexVM()
                                     {
                                         StudentId = s.StudentId,
                                         FirstName = s.FirstName,
                                         LastName = s.LastName,
                                         MiddleName = s.MiddleName,
                                         Gender = s.Gender,
                                         ProgrammeName = s.Programme.ProgrammeName,
                                         MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                         PhoneNumber = s.PhoneNumber,
                                         JambRegNo = s.JambRegNo,
                                         ModeOfEntry = s.ModeOfEntry,
                                         LevelName = s.Level.LevelName,
                                         SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                     }).Take(2500).ToListAsync();
                }
                else if (schoolProgrammeId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsClearedAcademics.Equals(isCleared)
                                    && x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2500).ToListAsync();
                }
                else if (facultyId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsClearedAcademics.Equals(isCleared)
                                    && x.IsClearedFaculty.Equals(true) //Commented to change clearance order0
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2500).ToListAsync();
                }
                else
                {
                    model = await _db.Students.Include(i => i.Programme)
                               .Include(i => i.Programme.Department).AsNoTracking()
                               .Where(x => x.IsDelete.Equals(false)
                               //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                               //&& x.IsClearedAll.Equals(false)
                               && x.IsClearedAcademics.Equals(isCleared)
                               && x.IsClearedDepartment.Equals(true) //Commented to change clearance order
                               && x.Session.SessionId.Equals((int)SessionId)
                               && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                               .Select(s => new StudentIndexVM()
                               {
                                   StudentId = s.StudentId,
                                   FirstName = s.FirstName,
                                   LastName = s.LastName,
                                   MiddleName = s.MiddleName,
                                   Gender = s.Gender,
                                   ProgrammeName = s.Programme.ProgrammeName,
                                   MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                   PhoneNumber = s.PhoneNumber,
                                   JambRegNo = s.JambRegNo,
                                   SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                   Email = s.Email,
                                   ModeOfEntry = s.ModeOfEntry
                               }).Take(2500).ToListAsync();
                }
            }

            return model;
        }

        public async Task<List<StudentIndexVM>> GetStudentAcademicList(int? schoolProgrammeId, int? facultyId, bool isCleared)
        {
            var model = new List<StudentIndexVM>();

            var UGSchoolProgramme = await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)).FirstOrDefaultAsync();

            if (UGSchoolProgramme.SchoolProgrammeCode.ToUpper().ToString().Equals("UG"))
            {
                if (schoolProgrammeId != null && facultyId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                     .Include(i => i.Programme.Department).AsNoTracking()
                                     .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                     && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                     //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                     //&& x.IsClearedAll.Equals(false)
                                     //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                     && x.IsClearedAcademics.Equals(isCleared)
                                     && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                     .Select(s => new StudentIndexVM()
                                     {
                                         StudentId = s.StudentId,
                                         FirstName = s.FirstName,
                                         LastName = s.LastName,
                                         MiddleName = s.MiddleName,
                                         Gender = s.Gender,
                                         ProgrammeName = s.Programme.ProgrammeName,
                                         MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                         PhoneNumber = s.PhoneNumber,
                                         JambRegNo = s.JambRegNo,
                                         ModeOfEntry = s.ModeOfEntry,
                                         LevelName = s.Level.LevelName,
                                         SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                     }).Take(2500).ToListAsync();
                }
                else if (schoolProgrammeId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsClearedAcademics.Equals(isCleared)
                                    //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order to start with clearance; To revert, uncomment!
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2500).ToListAsync();
                }
                else if (facultyId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsClearedAcademics.Equals(isCleared)
                                    //&& x.IsClearedFaculty.Equals(true) //Commented to change clearance order to start with clearance; To revert, uncomment!
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2500).ToListAsync();
                }
                else
                {
                    model = await _db.Students.Include(i => i.Programme)
                               .Include(i => i.Programme.Department).AsNoTracking()
                               .Where(x => x.IsDelete.Equals(false)
                               //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                               //&& x.IsClearedAll.Equals(false)
                               && x.IsClearedAcademics.Equals(isCleared)
                               //&& x.IsClearedDepartment.Equals(true) //Commented to change clearance order to start with clearance; To revert, uncomment!
                               && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                               .Select(s => new StudentIndexVM()
                               {
                                   StudentId = s.StudentId,
                                   FirstName = s.FirstName,
                                   LastName = s.LastName,
                                   MiddleName = s.MiddleName,
                                   Gender = s.Gender,
                                   ProgrammeName = s.Programme.ProgrammeName,
                                   MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                   PhoneNumber = s.PhoneNumber,
                                   JambRegNo = s.JambRegNo,
                                   SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                   Email = s.Email,
                                   ModeOfEntry = s.ModeOfEntry
                               }).Take(2500).ToListAsync();
                }
            }
            else
            {
                if (schoolProgrammeId != null && facultyId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                     .Include(i => i.Programme.Department).AsNoTracking()
                                     .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                     && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                     //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                     //&& x.IsClearedAll.Equals(false)
                                     && x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                     && x.IsClearedAcademics.Equals(isCleared)
                                     && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                     .Select(s => new StudentIndexVM()
                                     {
                                         StudentId = s.StudentId,
                                         FirstName = s.FirstName,
                                         LastName = s.LastName,
                                         MiddleName = s.MiddleName,
                                         Gender = s.Gender,
                                         ProgrammeName = s.Programme.ProgrammeName,
                                         MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                         PhoneNumber = s.PhoneNumber,
                                         JambRegNo = s.JambRegNo,
                                         ModeOfEntry = s.ModeOfEntry,
                                         LevelName = s.Level.LevelName,
                                         SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                     }).Take(2500).ToListAsync();
                }
                else if (schoolProgrammeId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsClearedAcademics.Equals(isCleared)
                                    && x.IsClearedFaculty.Equals(true) //Commented to change clearance order
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2500).ToListAsync();
                }
                else if (facultyId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.Programme.Department.FacultyId.Equals((int)facultyId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsClearedAcademics.Equals(isCleared)
                                    && x.IsClearedFaculty.Equals(true) //Commented to change clearance order0
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2500).ToListAsync();
                }
                else
                {
                    model = await _db.Students.Include(i => i.Programme)
                               .Include(i => i.Programme.Department).AsNoTracking()
                               .Where(x => x.IsDelete.Equals(false)
                               //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                               //&& x.IsClearedAll.Equals(false)
                               && x.IsClearedAcademics.Equals(isCleared)
                               && x.IsClearedDepartment.Equals(true) //Commented to change clearance order
                               && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                               .Select(s => new StudentIndexVM()
                               {
                                   StudentId = s.StudentId,
                                   FirstName = s.FirstName,
                                   LastName = s.LastName,
                                   MiddleName = s.MiddleName,
                                   Gender = s.Gender,
                                   ProgrammeName = s.Programme.ProgrammeName,
                                   MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                   PhoneNumber = s.PhoneNumber,
                                   JambRegNo = s.JambRegNo,
                                   SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                   Email = s.Email,
                                   ModeOfEntry = s.ModeOfEntry
                               }).Take(2500).ToListAsync();
                }
            }

            return model;
        }

        //Departmental Cleaance
        public async Task<List<StudentIndexVM>> GetStudentDeptList(int? schoolProgrammeId, int? deptId, bool isCleared, int? SessionId)
        {
            //var dept = _db.Departments.FirstOrDefault(x => x.DeptCode.Equals("EV_QSU"));
            //deptId = dept?.DepartmentId;
            var studentList = new List<Student>();
            var model = new List<StudentIndexVM>();

            var UGSchoolProgramme = await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)).FirstOrDefaultAsync();
            var admittedSessionId = await _db.AssignSessionToSchools.Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId) && x.ActiveSession == true)
                                                                  .Select(x => x.SessionId)
                                                                  .FirstOrDefaultAsync();

            if (UGSchoolProgramme.SchoolProgrammeCode.ToUpper().ToString().Equals("UG"))
            {
                if (schoolProgrammeId != null && deptId != null && SessionId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.Programme.Department.DepartmentId.Equals((int)deptId)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsClearedFaculty.Equals(true)   //Added to change order of clearance
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString()) 
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.Active.Equals(true)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName ?? "",
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2000).ToListAsync();
                }
                else if (schoolProgrammeId != null && deptId != null && SessionId == null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.Programme.Department.DepartmentId.Equals((int)deptId)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    && x.IsClearedFaculty.Equals(true)   //Added to change order of clearance
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString()) 
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.Active.Equals(true)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName ?? "",
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2000).ToListAsync();
                 }
                else if (deptId != null)
                {
                    model = await _db.Students.Include(i => i.Programme).Include(i => i.Programme.Department)
                                    .Include(i => i.Level).AsNoTracking().Where(x => x.Active.Equals(true)
                                    && x.Programme.Department.DepartmentId.Equals((int)deptId)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    && x.IsClearedFaculty.Equals(true) //Added to change order of clearance
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2000).ToListAsync();
                }
                else if (schoolProgrammeId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    && x.IsClearedFaculty.Equals(true) //Added to change order of clearance
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    //&& x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.Active.Equals(true)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).Take(2000).ToListAsync();
                }

                return model;
            }
            else
            {
                if (schoolProgrammeId != null && deptId != null && SessionId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.Programme.Department.DepartmentId.Equals((int)deptId)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    //&& x.IsClearedFaculty.Equals(true)   //Added to change order of clearance
                                    && x.StudentStatus.Equals(StudentStatus.New_Student.ToString()) 
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.Active.Equals(true)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName ?? "",
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();
                }
                else if (schoolProgrammeId != null && deptId != null && SessionId == null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.Programme.Department.DepartmentId.Equals((int)deptId)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    //&& x.IsClearedFaculty.Equals(true)   //Added to change order of clearance
                                    && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.Active.Equals(true)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName ?? "",
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();
                }
                else if (deptId != null)
                {
                    model = await _db.Students.Include(i => i.Programme).Include(i => i.Programme.Department)
                                    .Include(i => i.Level).AsNoTracking().Where(x => x.Active.Equals(true)
                                    && x.Programme.Department.DepartmentId.Equals((int)deptId)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    //&& x.IsClearedFaculty.Equals(true) //Added to change order of clearance
                                    && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();
                }
                else if (schoolProgrammeId != null)
                {
                    model = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Programme.Department).Include(i => i.Level).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.IsClearedDepartment.Equals(isCleared)
                                    //&& x.IsClearedFaculty.Equals(true) //Added to change order of clearance
                                    && x.StudentStatus.Equals(StudentStatus.New_Student.ToString())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.Active.Equals(true)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                        Email = s.Email,
                                        ModeOfEntry = s.ModeOfEntry
                                    }).ToListAsync();
                }

                return ValidateClearanceStatus(model);
            }

            //return model;
        }

        public async Task<List<StudentIndexVM>> GetStudentDeptList(int schoolProgrammeId, int? programmeId, int? deptId, int? facultyId, bool isCleared, int sesionId,
            string ClearanceStage)
        {
            var studentList = new List<Student>();
            var model = new List<StudentIndexVM>();
            if (programmeId != null)
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                .Include(i => i.Programme.Department.Faculty).Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                && x.Session.SessionId.Equals(sesionId)
                                && x.Programme.ProgrammeId.Equals((int)programmeId)
                                && x.IsClearedFaculty.Equals(isCleared)
                                && string.IsNullOrEmpty(x.ImeiNo)
                                && x.Active.Equals(true)
                                && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)).ToListAsync();

            }
            else if (deptId != null)
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                .Include(i => i.Programme.Department.Faculty).Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                && x.Session.SessionId.Equals(sesionId)
                                && x.Programme.Department.DepartmentId.Equals((int)deptId)
                                && x.IsClearedFaculty.Equals(isCleared)
                                && x.Active.Equals(true)
                                && string.IsNullOrEmpty(x.ImeiNo)
                                && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)).ToListAsync();

            }
            else if (facultyId != null)
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                .Include(i => i.Programme.Department.Faculty).Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                 && x.Session.SessionId.Equals(sesionId)
                                && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                && x.IsClearedFaculty.Equals(isCleared)
                                && x.Active.Equals(true)
                                && string.IsNullOrEmpty(x.ImeiNo)
                                && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)).ToListAsync();
            }
            else
            {
                studentList = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                .Include(i => i.Programme.Department.Faculty).Include(i => i.Level).Include(i => i.Session).AsNoTracking()
                                .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                 && x.Session.SessionId.Equals(sesionId)
                                && x.IsClearedFaculty.Equals(isCleared)
                                && string.IsNullOrEmpty(x.ImeiNo) 
                                && x.Active.Equals(true)
                                && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false)).ToListAsync();
            }
            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Department"))
            {
                studentList = studentList.Where(x => x.IsClearedDepartment.Equals(isCleared)).ToList();
            }
            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Academic"))
            {
                studentList = studentList.Where(x => x.IsClearedAcademics.Equals(isCleared)).ToList();
            }

            if (!string.IsNullOrEmpty(ClearanceStage) && ClearanceStage.Equals("Faculty"))
            {
                studentList = studentList.Where(x => x.IsClearedFaculty.Equals(isCleared)).ToList();
            }

            //return studentList.Select(s => new StudentIndexVM()
            //{
            //    StudentId = !string.IsNullOrEmpty(s.StudentId) ? s.StudentId : "",
            //    FirstName = !string.IsNullOrEmpty(s.FirstName) ? s.FirstName : "",
            //    LastName = !string.IsNullOrEmpty(s.LastName) ? s.LastName : "",
            //    MiddleName = !string.IsNullOrEmpty(s.MiddleName) ? s.MiddleName : "",
            //    Gender = !string.IsNullOrEmpty(s.Gender) ? s.Gender : "",
            //    ProgrammeName = !string.IsNullOrEmpty(s.Programme.ProgrammeName) ? s.Programme.ProgrammeName : "",
            //    DeptName = !string.IsNullOrEmpty(s.Programme.Department.DeptName) ? s.Programme.Department.DeptName : "",
            //    FacultyName = !string.IsNullOrEmpty(s.StudentId) ? s.Programme.Department.Faculty.FacultyName : "",
            //    MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
            //    PhoneNumber = !string.IsNullOrEmpty(s.PhoneNumber) ? s.PhoneNumber : "",
            //    Email = !string.IsNullOrEmpty(s.Email) ? s.Email : "",
            //    JambRegNo = !string.IsNullOrEmpty(s.JambRegNo) ? s.JambRegNo : "",
            //    ModeOfEntry = !string.IsNullOrEmpty(s.ModeOfEntry) ? s.ModeOfEntry : "",
            //    LevelName = !string.IsNullOrEmpty(s.Level.LevelName) ? s.Level.LevelName : "",
            //    SchoolProgrammeCode = !string.IsNullOrEmpty(s.SchoolProgramme.SchoolProgrammeCode) ? s.SchoolProgramme.SchoolProgrammeCode : "",
            //    Subjects = _db.ApplicantOLevelResults
            //        .Where(o => o.ApplicantId == s.PrimaryEmail) // Match O-Level results to students by email
            //        .GroupBy(o => o.Subject.CourseCode) // Group by subject code
            //        .Select(g => $"{g.Key} - {g.FirstOrDefault().SubjectGrade}") // Format as "SubjectCode - Grade"
            //        .ToList() // Convert to list
            //}).ToList();

            var studentVMs = studentList
                    .Select(s => new
                    {
                        Student = s,
                        Subjects = _db.ApplicantOLevelResults
                            .Where(x => x.ApplicantId == s.Email)
                            .Select(o => new { o.Subject.CourseCode, o.SubjectGrade })
                            .Distinct() // Remove duplicate subject-grade pairs
                            .ToList() // Bring the subject data into memory
                    })
                    .AsEnumerable() // Move the query execution to memory
                    .Select(s => new StudentIndexVM()
                    {
                        StudentId = !string.IsNullOrEmpty(s.Student.StudentId) ? s.Student.StudentId : "",
                        FirstName = !string.IsNullOrEmpty(s.Student.FirstName) ? s.Student.FirstName : "",
                        LastName = !string.IsNullOrEmpty(s.Student.LastName) ? s.Student.LastName : "",
                        MiddleName = !string.IsNullOrEmpty(s.Student.MiddleName) ? s.Student.MiddleName : "",
                        Gender = !string.IsNullOrEmpty(s.Student.Gender) ? s.Student.Gender : "",
                        ProgrammeName = !string.IsNullOrEmpty(s.Student.Programme.ProgrammeName) ? s.Student.Programme.ProgrammeName : "",
                        DeptName = !string.IsNullOrEmpty(s.Student.Programme.Department.DeptName) ? s.Student.Programme.Department.DeptName : "",
                        FacultyName = !string.IsNullOrEmpty(s.Student.StudentId) ? s.Student.Programme.Department.Faculty.FacultyName : "",
                        MatricNo = !string.IsNullOrEmpty(s.Student.MatricNo) ? s.Student.MatricNo : s.Student.JambRegNo,
                        PhoneNumber = !string.IsNullOrEmpty(s.Student.PhoneNumber) ? s.Student.PhoneNumber : "",
                        Email = !string.IsNullOrEmpty(s.Student.Email) ? s.Student.Email : "",
                        JambRegNo = !string.IsNullOrEmpty(s.Student.JambRegNo) ? s.Student.JambRegNo : "",
                        ModeOfEntry = !string.IsNullOrEmpty(s.Student.ModeOfEntry) ? s.Student.ModeOfEntry : "",
                        LevelName = !string.IsNullOrEmpty(s.Student.Level.LevelName) ? s.Student.Level.LevelName : "",
                        SchoolProgrammeCode = !string.IsNullOrEmpty(s.Student.SchoolProgramme.SchoolProgrammeCode) ? s.Student.SchoolProgramme.SchoolProgrammeCode : "",
                        Subjects = s.Subjects
                        .GroupBy(o => o.CourseCode) // Group by CourseCode to ensure uniqueness
                        .Select(g => $"{g.Key} - {g.First().SubjectGrade}") // Format unique entries
                        .ToList()
                                })
                                .ToList();

            return studentVMs;
        }

        //return model;  
        //Faculty Clearance
        public async Task<List<StudentIndexVM>> GetStudentfacultyList(int? schoolProgrammeId, int? facultyId, bool isCleared, int? SessionId)
        {
            var studentList = new List<Student>();
            var UGSchoolProgramme = await _db.SchoolProgrammes.Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)).FirstOrDefaultAsync();
            var admittedSessionId = await _db.AssignSessionToSchools.Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId) && x.ActiveSession == true)
                                                                  .Select(x => x.SessionId)
                                                                  .FirstOrDefaultAsync();

            //Check for Undergraduate Students
            if (UGSchoolProgramme.SchoolProgrammeCode.ToUpper().ToString().Equals("UG"))
            {
                if (schoolProgrammeId != null && facultyId != null)
                {
                    return await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Programme.Department).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                    //&& x.Session.SessionId.Equals(admittedSessionId)  // Commented to allow for previous sessional clearance
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                    //&& x.IsClearedAll.Equals(false)
                                    //&& x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                    && x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                    && x.IsClearedFaculty.Equals(isCleared)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        ModeOfEntry = s.ModeOfEntry,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                    }).Take(1000).ToListAsync();
                }
                if (schoolProgrammeId != null)
                {
                    return await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Programme.Department).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                    //&& x.IsClearedAll.Equals(false)
                                    //&& x.IsClearedDepartment.Equals(true) //Added to change order of clearance
                                    && x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                    //&& x.Session.SessionId.Equals(admittedSessionId)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsClearedFaculty.Equals(isCleared)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        ModeOfEntry = s.ModeOfEntry,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory
                                    }).Take(1000).ToListAsync();
                }
                if (facultyId != null)
                {
                    return await _db.Students.Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.IsDelete.Equals(false)
                                    && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                    && x.IsClearedFaculty.Equals(isCleared)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                                                          //&& x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                                                          //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                                                          //&& x.IsClearedAll.Equals(false)
                                    && x.IsGraduated.Equals(false) && x.IsDelete.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        ModeOfEntry = s.ModeOfEntry,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory
                                    }).Take(1000).ToListAsync();
                }
                return studentList.Select(s => new StudentIndexVM()
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    MiddleName = s.MiddleName,
                    Gender = s.Gender,
                    ProgrammeName = s.Programme.ProgrammeName,
                    MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                    PhoneNumber = s.PhoneNumber,
                    JambRegNo = s.JambRegNo,
                    ModeOfEntry = s.ModeOfEntry,
                    LevelName = s.Level.LevelName,
                    SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                }).Take(1000).ToList();
            }
            else //For all Other Non-Undergraaduate students
            {
                if (schoolProgrammeId != null && facultyId != null)
                {
                    return await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme)
                                    .Include(i => i.Programme.Department).Include(i => i.Session).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                    //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsClearedDepartment.Equals(true) //Added to change order of clearance
                                    && x.IsClearedFaculty.Equals(isCleared)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        ModeOfEntry = s.ModeOfEntry,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                                    }).Take(1000).ToListAsync();
                }
                if (schoolProgrammeId != null)
                {
                    return await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.SchoolProgrammeId.Equals((int)schoolProgrammeId)
                                    //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    && x.IsClearedDepartment.Equals(true) //Added to change order of clearance
                                    && x.IsClearedFaculty.Equals(isCleared)
                                    && x.IsDelete.Equals(false) && x.IsGraduated.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        ModeOfEntry = s.ModeOfEntry,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory
                                    }).Take(1000).ToListAsync();
                }
                if (facultyId != null)
                {
                    return await _db.Students.Include(i => i.Programme).Include(i => i.Session)
                                    .Include(i => i.Programme.Department).AsNoTracking()
                                    .Where(x => x.IsDelete.Equals(false)
                                    && x.Programme.Department.FacultyId.Equals((int)facultyId)
                                    && x.IsClearedFaculty.Equals(isCleared)
                                    && x.IsClearedDepartment.Equals(true)
                                    && x.Session.SessionId.Equals((int)SessionId)
                                    //&& x.IsClearedAcademics.Equals(true) //Added to change order of clearance
                                    //&& x.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                                    //&& x.IsClearedAll.Equals(false)
                                    && x.IsGraduated.Equals(false) && x.IsDelete.Equals(false))
                                    .Select(s => new StudentIndexVM()
                                    {
                                        StudentId = s.StudentId,
                                        FirstName = s.FirstName,
                                        LastName = s.LastName,
                                        MiddleName = s.MiddleName,
                                        Gender = s.Gender,
                                        ProgrammeName = s.Programme.ProgrammeName,
                                        MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                                        PhoneNumber = s.PhoneNumber,
                                        JambRegNo = s.JambRegNo,
                                        ModeOfEntry = s.ModeOfEntry,
                                        LevelName = s.Level.LevelName,
                                        SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory
                                    }).Take(1000).ToListAsync();
                }
                return studentList.Select(s => new StudentIndexVM()
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    MiddleName = s.MiddleName,
                    Gender = s.Gender,
                    ProgrammeName = s.Programme.ProgrammeName,
                    MatricNo = !string.IsNullOrEmpty(s.MatricNo) ? s.MatricNo : s.JambRegNo,
                    PhoneNumber = s.PhoneNumber,
                    JambRegNo = s.JambRegNo,
                    ModeOfEntry = s.ModeOfEntry,
                    LevelName = s.Level.LevelName,
                    SchoolProgrammeCode = s.SchoolProgramme.ProgrammeCategory,
                }).Take(1000).ToList();
            }
            

        }

        public async Task<string> GetUserName(string studentId)
        {
            var username = await _db.Students.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(studentId.Trim().ToUpper()))
                            .Select(s => new { s.LastName, s.FirstName, s.MiddleName }).FirstOrDefaultAsync();
            return $"{username.LastName} {username.FirstName} {username.MiddleName}";

        }

        public string GetStudentId(string email)
        {
            return _db.Students.AsNoTracking().Where(x => x.Email.Trim().ToUpper().Equals(email.Trim().ToUpper()))
                    .Select(s => s.StudentId).FirstOrDefault();

        }

        public Student GetStudent(string email)
        {
            return _db.Students.Include(i => i.Programme).Include(i => i.Level).Include(i => i.Programme.Department)
                        .Include(i => i.SchoolProgramme).AsNoTracking()
                    .Where(x => x.Email.Trim().ToUpper().Equals(email.Trim().ToUpper()))
                   .FirstOrDefault();

        }

        public Applicant GetApplicant(string email)
        {
            return _db.Applicants
                        .Include(i => i.SchoolProgramme).AsNoTracking()
                    .Where(x => x.ApplicantEmail.Trim().ToUpper().Equals(email.Trim().ToUpper()))
                   .FirstOrDefault();

        }
        public Student GetStudentByMatNumber(string mattriNum)
        {
            return _db.Students.Include(i => i.Programme).Include(i => i.Level).Include(i => i.Programme.Department)
                        .Include(i => i.SchoolProgramme).AsNoTracking()
                    .Where(x => x.MatricNo.Trim().ToUpper().Equals(mattriNum.Trim().ToUpper()))
                   .FirstOrDefault();

        }

        //public async Task UpdateStudentRecord(string schoofeepayment, string studentId)
        //{
        //    if (schoofeepayment.Equals(SchoolFeeCategory.School_Charges.ToString()))
        //    {
        //        var student = await _db.Students.Where(x => x.StudentId.Equals(studentId))
        //                            .FirstOrDefaultAsync();
        //        student.StudentStatus = StudentStatus.Returning.ToString();
        //        _db.Entry(student).State = EntityState.Modified;
        //    }
        //}

        public async Task<Student> SaveAndGenerateMatricNo(string studentId)
        {
            var editStudent = await _db.Students.Include(i => i.SchoolProgramme).Include(i => i.Programme.Department.Faculty)
                                    .Include(i => i.Session).AsNoTracking()
                            .Where(x => x.StudentId.Equals(studentId)).FirstOrDefaultAsync();
            if (editStudent.StudentStatus.ToUpper().Equals(StudentStatus.New_Student.ToString().ToUpper())
                && string.IsNullOrEmpty(editStudent.MatricNo))
            {
                var studentCount = await _db.Students.Include(i => i.Programme.Department.Faculty).AsNoTracking()
                                   .CountAsync(x => x.Programme.Department.FacultyId.Equals(editStudent.Programme.Department.Faculty.FacultyId)
                                   && x.Session.SessionId.Equals(editStudent.Session.SessionId)
                                   && x.SchoolProgramme.SchoolProgrammeId.Equals(editStudent.SchoolProgrammeId)
                                   && !string.IsNullOrEmpty(x.MatricNo));

                string matricNo = null;
                string newEmail = null;

                do
                {
                    studentCount += 1;
                    matricNo = GenerateMatricNo(editStudent.SchoolProgramme, editStudent.Session.StartDate.Year,
                                    editStudent.Programme.Department.Faculty.FacultyCode, studentCount);
                    newEmail = matricNo.Replace("UJ", "");
                    newEmail = newEmail.Replace("/", "");

                } while (await _db.Students.AsNoTracking().AnyAsync(x => x.MatricNo.Trim().ToUpper().Equals(matricNo.Trim().ToUpper())));
                newEmail = $"{newEmail}@unijos.edu.ng";
                if (!string.IsNullOrEmpty(matricNo) && !string.IsNullOrEmpty(newEmail))
                {
                    var signEmail = editStudent.Email;
                    editStudent.StudentStatus = StudentStatus.Returning.ToString();
                    editStudent.MatricNo = matricNo;
                    editStudent.Email = newEmail;
                    editStudent.PrimaryEmail = signEmail;

                    _db.Set<Student>().AddOrUpdate(editStudent);

                    var user = await _db.Users.Where(x => x.Id.Trim().ToUpper().Equals(editStudent.StudentId.Trim().ToUpper())
                                   || x.Email.Trim().ToUpper().Equals(editStudent.PrimaryEmail.Trim().ToUpper())).FirstOrDefaultAsync();

                    if (user != null)
                    {
                        user.Email = newEmail;
                        user.UserName = newEmail;
                        _db.Entry(user).State = EntityState.Modified;
                    }
                    await _db.SaveChangesAsync();

                }
            }
            return editStudent;

        }

        public string GenerateMatricNo(SchoolProgramme model, int year, string facultyCode, int newNumber)
        {
            string myNo = ConvertToProperMatric(newNumber);
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())
                    && model.ProgrammeType.Equals(ProgrammeType.Full_Time.ToString()))
            {
                return $"UJ/{year}/{facultyCode}/{myNo}";
            }
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.UnderGraduate.ToString())
                    && model.ProgrammeType.Equals(ProgrammeType.Part_Time.ToString()))
            {
                return $"UJ/{year}/PT{facultyCode}/{myNo}";
            }
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.Phd.ToString())
                   || model.ProgrammeCategory.Equals(ProgrammeCategory.Masters.ToString())
                   || model.ProgrammeCategory.Equals(ProgrammeCategory.MBA.ToString())
                   || model.ProgrammeCategory.Equals(ProgrammeCategory.Post_Graduate.ToString()))
            {
                return $"UJ/{year}/PG{facultyCode}/{myNo}";
            }
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.Remedial_Science.ToString())
                  || model.ProgrammeCategory.Equals(ProgrammeCategory.IJMB.ToString())
                  || model.ProgrammeCategory.Equals(ProgrammeCategory.Preliminary_French.ToString()))
            {
                return $"UJ/{year}/{model.SchoolProgrammeCode}/{myNo}";
            }
            if (model.ProgrammeCategory.Equals(ProgrammeCategory.Institute_Of_Education.ToString()))
            {
                return $"UJ/{year}/{facultyCode}/PT/{myNo}";
            }
            return "";
        }



        private string ConvertToProperMatric(int number)
        {
            string no = number.ToString();
            if (no.Count() == 1)
            {
                return $"000{number}";
            }
            if (no.Count() == 2)
            {
                return $"00{number}";
            }
            if (no.Count() == 3)
            {
                return $"0{number}";
            }
            return number.ToString();
        }


    }
}