using System;

namespace SwiftKampusModel
{
    public class CorrespondenceType
    {
        public CorrespondenceType()
        {
            CorrespondenceTypeId = Guid.NewGuid();
        }
        public string CorrespondenceTypeName { get; set; }

        public string Description { get; set; }

        public Guid CorrespondenceTypeId { get; set; }

        public Correspondence Correspondence { get; set; }
    }
}