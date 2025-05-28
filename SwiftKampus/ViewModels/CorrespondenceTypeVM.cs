namespace SwiftKampus.ViewModels
{
    public class CorrespondenceTypeVM
    {
    }


    /// <summary>
    /// Creation of CorrespondenceTypes should be a SuperAdmin or
    /// higher level permitted task not for students.
    /// </summary>
    public class CreateCorrespondenceTypeVM
    {
        public string Name { get; set; }

        public string Description { get; set; }

    }
}