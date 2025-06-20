using System;

namespace DomainModels.ResultModule;

public class CourseUpload
{
    public Guid CourseUploadId { get; set; }

    public Guid CourseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string FileLocation { get; set; } = string.Empty;

    public Course Course { get; set; } = default!;

}
