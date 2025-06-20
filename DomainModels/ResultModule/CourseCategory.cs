using System;

namespace DomainModels.ResultModule;

public class CourseCategory
    {
        public int CourseCategoryId { get; set; }
        public int DepartmentId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public Department Department { get; set; }
        public ICollection<AssignCourseToCategory> AssignCourseToCategories { get; set; }
    }