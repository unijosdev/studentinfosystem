using System.Collections.Generic;

namespace SwiftKampusModel.Library
{
    public class MembershipType
    {
        public int MembershipTypeId { get; set; }

        public string TypeOfMembership { get; set; }


        public int NumberOfBookToBorrow { get; set; }

        public virtual ICollection<LibraryRegistration> LibraryRegistrations { get; set; }
    }
}
