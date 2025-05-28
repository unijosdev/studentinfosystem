using SwiftKampus.Models;
using System;
using System.Linq;

namespace SwiftKampus.Services
{
    public class GradeRemark
    {
        readonly SchoolDbContext _db;
        public GradeRemark(SchoolDbContext db)
        {
            _db = db;
        }

        // This can be private now
        public string Grading(double summaryTotal, int schoolProgrammeId, int resultTemplateId)
        {
            var gradeValue = string.Empty;
            int scoreTotal = (int)summaryTotal;
            var myGrade = _db.Grades.AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(schoolProgrammeId)
                                && x.ResultTemplateId.Equals(resultTemplateId)).ToList();
            foreach (var item in myGrade)
            {
                if (scoreTotal <= item.MaximumValue && scoreTotal >= item.MinimumValue)
                {
                    gradeValue = item.GradeName;
                }

            }
            return !string.IsNullOrEmpty(gradeValue) ? gradeValue : "Grading Not Set";
            // return gradeValue;

        }


        public string Remark(double summaryTotal, int schoolProgrammeId, int resultTemplateId)
        {
            //string myclassName = GetschoolClass(className);
            string remarkValue = "";

            int scoreTotal = (int)summaryTotal;
            var myGrade = _db.Grades.AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(schoolProgrammeId)
                            && x.ResultTemplateId.Equals(resultTemplateId)).ToList();
            foreach (var item in myGrade)
            {
                if (scoreTotal <= item.MaximumValue && scoreTotal >= item.MinimumValue)
                {
                    remarkValue = item.Remark;
                }
            }

            return !string.IsNullOrEmpty(remarkValue) ? remarkValue : "Grading Not Set";
        }

        public int GradingPoint(double summaryTotal, int schoolProgrammeId, int resultTemplateId)
        {
            int remarkValue = 0;
            int scoreTotal = (int)summaryTotal;
            var myGrade = _db.Grades.AsNoTracking().Where(x => x.SchoolProgrammeId.Equals(schoolProgrammeId)
                           && x.ResultTemplateId.Equals(resultTemplateId)).ToList();
            foreach (var item in myGrade)
            {
                if (scoreTotal <= item.MaximumValue && scoreTotal >= item.MinimumValue)
                {
                    remarkValue = Convert.ToInt32(item.GradePoint);
                }
            }
            return remarkValue == 0 ? 0 : remarkValue;
        }

        public string ClassOfDegree(double cgpa, int schoolProgrammeId)
        {
            string remarkValue = string.Empty;
            int scoreTotal = (int)cgpa;
            var classOfDegree = _db.ClassDegrees.AsNoTracking()
                                .Where(x => x.SchoolProgrammeId.Equals(schoolProgrammeId)).ToList();
            foreach (var item in classOfDegree)
            {
                if (scoreTotal <= item.MaximumValue && scoreTotal >= item.MinimumValue)
                {
                    remarkValue = item.Remark;
                }
            }
            return !string.IsNullOrEmpty(remarkValue) ? remarkValue : "Grading Not Set";
        }

        //public string PrincipalRemark(double summaryTotal, string facultyName)
        //{
        //    string myclassName = GetschoolClass(facultyName);
        //    string remarkValue = "";

        //    //int scoreTotal = (int)summaryTotal;
        //    var myGrade = _db.PrincipalComments.AsNoTracking().Where(x => x.ClassName.Equals(myclassName)).ToList();
        //    foreach (var item in myGrade)
        //    {
        //        if (summaryTotal <= item.MaximumGrade && summaryTotal >= item.MinimumGrade)
        //        {
        //            remarkValue = item.Remark;
        //        }
        //    }

        //    return !string.IsNullOrEmpty(remarkValue) ? remarkValue : "Enter Value between 1.0 - 9.0";

        //}

    }
}
