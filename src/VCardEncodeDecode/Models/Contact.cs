namespace VCardEncodeDecode.Models
{
    public class Contact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FormattedName { get; set; }
        public string Organization { get; set; }
        public string OrganizationPosition { get; set; }
        public string Title { get; set; }
        public string Photo { get; set; }
        public List<Link> Links { get; set; }
        public List<EMail> Emails { get; set; }
        public List<Phone> Phones { get; set; }
        public List<Address> Addresses { get; set; }
    }
}
