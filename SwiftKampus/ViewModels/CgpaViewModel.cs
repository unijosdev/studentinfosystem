using SwiftKampus.Models;
using SwiftKampus.Services;

namespace SwiftKampus.ViewModels
{
    public class CgpaViewModel
    {
        readonly SchoolDbContext _db;
        readonly GradeRemark _myGradeRemark;
        public CgpaViewModel(SchoolDbContext db)
        {
            _db = db;
            _myGradeRemark = new GradeRemark(db);
        }      

        public double Score { get; set; }
        public int CourseCredit { get; set; }
        public int SchoolProgrammeId { get; set; }
        public int ResultTemplateId { get; set; }


        public string Grading
        {
            get
            {
                return _myGradeRemark.Grading(Score, SchoolProgrammeId, ResultTemplateId);
            }

        }

        public string Remark
        {
            get
            {
                return _myGradeRemark.Remark(Score, SchoolProgrammeId, ResultTemplateId);
            }
        }

        public int GradePoint
        {
            get
            {
                return _myGradeRemark.GradingPoint(Score, SchoolProgrammeId, ResultTemplateId);
            }
        }

        public int QualityPoint
        {
            get
            {
                return CourseCredit * GradePoint;
            }
        }

    }
}