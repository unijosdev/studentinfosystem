namespace SwiftKampusModel.Library
{
    public class LibraryRegistration
    {
        public int LibraryRegistrationId { get; set; }
        public int MembershipTypeId { get; set; }
        public MembershipType MembershipType { get; set; }
    }
}
