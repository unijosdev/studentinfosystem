using System.ComponentModel.DataAnnotations;

namespace SwiftKampusModel.AddmissionApplicant
{
    public class ThesisProposal
    {
        public int ThesisProposalId { get; set; }
        public string UserId { get; set; }
        public string SpecialInterest { get; set; }
        [Required]
        public string ProposedTopic { get; set; }
        [DataType(DataType.MultilineText)]
        public string Proposal { get; set; }
        public string Attachment { get; set; }
    }
}
