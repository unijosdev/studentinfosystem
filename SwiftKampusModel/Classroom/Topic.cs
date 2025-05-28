using SwiftKampusModel.Classroom.Assesment;
using SwiftKampusModel.Classroom.Quiz;
using System.Collections.Generic;

namespace SwiftKampusModel.Classroom
{
    public class Topic
    {
        public int TopicId { get; set; }
        public int ModuleId { get; set; }
        public string TopicName { get; set; }
        public int ExpectedTime { get; set; }
        public virtual Module Module { get; set; }
        public virtual ICollection<TopicMaterial> TopicMaterials { get; set; }
        public virtual ICollection<TopicAssignment> TopicAssignments { get; set; }
        public virtual ICollection<StudentTestLog> StudentTestLogs { get; set; }
        public virtual ICollection<TopicQuiz> TopicQuizzes { get; set; }
        public virtual ICollection<StudentAssesmentQuestion> StudentQuestions { get; set; }
        public virtual ICollection<QuizLog> QuizLogs { get; set; }
        public virtual ICollection<StudentTopicQuiz> StudentTopicQuizs { get; set; }
    }
}