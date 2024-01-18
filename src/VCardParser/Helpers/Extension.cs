using System.Text;
using VCardParser.Models;

namespace VCardParser.Helpers
{
    public static class Extension
    {
        const string NewLine = "\r\n";
        const string Separator = ";";
        const string Header = "BEGIN:VCARD\r\nVERSION:4.0";
        const string Name = "N:";
        const string FormattedName = "FN:";
        const string OrganizationName = "ORG:";
        const string TitlePrefix = "TITLE:";
        const string PhotoPrefix = "PHOTO;ENCODING=b;TYPE=JPEG:";
        const string PhonePrefix = "TEL;type=";
        const string PhoneSubPrefix = ",VOICE:";
        const string AddressPrefix = "ADR;type=";
        const string AddressSubPrefix = ":;;";
        const string EmailPrefix = "EMAIL;type=";
        const string WebSitePrefix = "X-ABLabel:";
        const string WebSite = "URL:";
        const string Footer = "END:VCARD";

        const string Dot = ".";
        const string TwoDots = ":";
        const string ItemStr = "item";

        public static Contact DecodeVCard(this string vCard)
        {
            var contact = new Contact
            {
                Emails = new List<EMail>(),
                Links = new List<Link>(),
                Phones = new List<Phone>()
            };

            var splittedVCard = vCard.Split(NewLine).ToList();

            contact.FormattedName = splittedVCard.FirstOrDefault(s => s.StartsWith(FormattedName))?.Split(":").LastOrDefault() ?? string.Empty;

            var names = splittedVCard.FirstOrDefault(s => s.StartsWith(Name))?.Replace(";", ":").Split(":") ?? Array.Empty<string>();
            var firstNames = names.Length > 0 ? names.TakeLast(names.Length - 2) : Array.Empty<string>();
            contact.FirstName = string.Join(" ", firstNames.Where(f => !string.IsNullOrWhiteSpace(f)));
            contact.LastName = names[1];

            var organizasion = splittedVCard.FirstOrDefault(s => s.StartsWith(OrganizationName))?.Replace(";", ":").Split(":");
            contact.Organization = organizasion?.Length > 0 ? organizasion[1] : string.Empty;
            contact.OrganizationPosition = organizasion?.Length > 1 ? organizasion[2] : string.Empty;

            var title = splittedVCard.FirstOrDefault(s => s.StartsWith(TitlePrefix))?.Split(":") ?? Array.Empty<string>();
            contact.Title = string.Join(":", title.Length > 0 ? title.TakeLast(title.Length - 1) : Array.Empty<string>());

            var photoBase64 = splittedVCard.FirstOrDefault(s => s.StartsWith(PhotoPrefix))?.Split(PhotoPrefix).LastOrDefault();
            contact.Photo = !string.IsNullOrWhiteSpace(photoBase64) ? "data:image/jpeg;base64," + photoBase64 : null;


            var emails = splittedVCard.Where(s => s.StartsWith(EmailPrefix));
            foreach (var item in emails)
            {
                var emailArray = item.Replace(";", ":").Replace("=", ":").Split(":");

                EMail mail = new EMail
                {
                    Type = emailArray.Length > 2 ? emailArray[2] : string.Empty,
                    Address = emailArray.LastOrDefault() ?? string.Empty,
                };
                contact.Emails.Add(mail);
            }

            var phones = splittedVCard.Where(s => s.StartsWith(PhonePrefix));
            foreach (var item in phones)
            {
                var phoneArray = item.Replace(";", ":").Replace("=", ":").Replace(",", ":").Split(":");

                Phone phone = new Phone
                {
                    Type = phoneArray.Length > 2 ? phoneArray[2] : string.Empty,
                    Number = phoneArray.LastOrDefault() ?? string.Empty
                };
                contact.Phones.Add(phone);
            }

            var items = splittedVCard.Where(s => s.StartsWith(ItemStr)).Order().ToList();
            for (int i = 0; i < items.Count(); i++)
            {
                if (items[i].Contains(WebSite))
                {
                    var urlArray = items[i].Split(WebSite);
                    var titleArray = items[i + 1].Split(WebSitePrefix);

                    Link link = new Link
                    {
                        Url = string.Join(WebSite, urlArray.TakeLast(urlArray.Length - 1)),
                        Title = string.Join(WebSitePrefix, titleArray.TakeLast(titleArray.Length - 1))
                    };
                    contact.Links.Add(link);
                }
            }

            return contact;
        }

        public static string EncodeVCard(this Contact contact)
        {
            StringBuilder fw = new StringBuilder();
            fw.Append(Header);
            fw.Append(NewLine);

            //Full Name
            if (!string.IsNullOrEmpty(contact.FirstName) || !string.IsNullOrEmpty(contact.LastName))
            {
                fw.Append(Name);
                fw.Append(contact.LastName);
                fw.Append(Separator);
                fw.Append(contact.FirstName);
                fw.Append(Separator);
                fw.Append(NewLine);
            }

            //Formatted Name
            if (!string.IsNullOrEmpty(contact.FormattedName))
            {
                fw.Append(FormattedName);
                fw.Append(contact.FormattedName);
                fw.Append(NewLine);
            }

            //Organization name
            if (!string.IsNullOrEmpty(contact.Organization))
            {
                fw.Append(OrganizationName);
                fw.Append(contact.Organization);
                if (!string.IsNullOrEmpty(contact.OrganizationPosition))
                {
                    fw.Append(Separator);
                    fw.Append(contact.OrganizationPosition);
                }
                fw.Append(NewLine);
            }

            //Title
            if (!string.IsNullOrEmpty(contact.Title))
            {
                fw.Append(TitlePrefix);
                fw.Append(contact.Title);
                fw.Append(NewLine);
            }

            //Photo
            if (!string.IsNullOrEmpty(contact.Photo))
            {
                fw.Append(PhotoPrefix);
                fw.Append(contact.Photo);
                fw.Append(NewLine);
                fw.Append(NewLine);
            }

            //Links
            if (contact?.Links != null)
            {
                for (int i = 0; i < contact.Links.Count; i++)
                {
                    fw.Append(ItemStr);
                    fw.Append(i);
                    fw.Append(Dot);
                    fw.Append(WebSite);
                    fw.Append(contact.Links[i].Url);
                    fw.Append(NewLine);
                    fw.Append(ItemStr);
                    fw.Append(i);
                    fw.Append(Dot);
                    fw.Append(WebSitePrefix);
                    fw.Append(contact.Links[i].Title);
                    fw.Append(NewLine);
                }
            }

            //Phones
            foreach (var item in contact.Phones)
            {
                fw.Append(PhonePrefix);
                fw.Append(item.Type);
                fw.Append(PhoneSubPrefix);
                fw.Append(item.Number);
                fw.Append(NewLine);
            }

            //Addresses
            foreach (var item in contact.Addresses ?? new List<Address>())
            {
                fw.Append(AddressPrefix);
                fw.Append(item.Type);
                fw.Append(AddressSubPrefix);
                fw.Append(item.Description);
                fw.Append(NewLine);
            }

            //Email
            foreach (var item in contact.Emails ?? new List<EMail>())
            {
                fw.Append(EmailPrefix);
                fw.Append(item.Type);
                fw.Append(TwoDots);
                fw.Append(item.Address);
                fw.Append(NewLine);
            }

            fw.Append(Footer);

            return fw.ToString();
        }
    }
}
