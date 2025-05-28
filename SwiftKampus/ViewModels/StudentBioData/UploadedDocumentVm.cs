using SwiftKampusModel.AddmissionApplicant;
using System.Collections.Generic;

namespace SwiftKampus.ViewModels.StudentBioData
{
    public class UploadedDocumentVm
    {
        public RelevantDocument RelevantDocument { get; set; }
        public List<RelevantDocument> RelevantDocuments { get; set; }
    }
}