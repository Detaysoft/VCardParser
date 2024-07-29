using System.Text;
using System.Text.RegularExpressions;
using VCardParser.Models;

namespace VCardParser.Helpers
{
    public static class Extension
    {
        const string Charset = "CHARSET=UTF-8";

        const string NewLineCrlf = "\r\n";
        const string NewLineCr = "\r";
        const string NewLineLf = "\n";

        const string Header = "BEGIN:VCARD";
        const string Version = "VERSION:2.1";
        const string Name = "N";
        const string Footer = "END:VCARD";
        const string FormattedName = "FN";
        const string OrganizationName = "ORG";
        const string TitlePrefix = "TITLE";
        const string PhotoPrefix = "PHOTO;ENCODING=BASE64;TYPE=JPEG:";
        const string PhonePrefix = "TEL;type=";
        const string PhoneSubPrefix = ",VOICE";
        const string EmailPrefix = "EMAIL;type=";
        const string WebSitePrefix = "X-ABLabel:";
        const string WebSite = "URL:";
        const string AddressPrefix = "ADR;type=";
        const string AddressSubPrefix = ":;;";

        const string Separator = ";";
        const string Dot = ".";
        const string TwoDots = ":";
        const string ItemStr = "item";
        const string Blank = " ";
        const string Transfer = "=";
        const string Comma = ",";

        const string PhotoPrefixForDecoding = "data:image/jpeg;base64,";
        const string EncodingQuotedPrintablePrefixForDecoding = "ENCODING=QUOTED-PRINTABLE";

        public static Contact DecodeVCard(this string vCard)
        {
            var contact = new Contact
            {
                Emails = new List<EMail>(),
                Links = new List<Link>(),
                Phones = new List<Phone>()
            };

            var splittedVCard = vCard.Replace(Charset + TwoDots, string.Empty).Replace(Charset + Separator, string.Empty).Replace(NewLineCrlf, NewLineLf).Replace(NewLineCr, NewLineLf).Split(NewLineLf).ToList();

            for (int i = 0; i < splittedVCard.Count; i++)
            {
                if (splittedVCard[i].Contains(EncodingQuotedPrintablePrefixForDecoding))
                {
                    var encodedStringValueList = splittedVCard[i].Replace(TwoDots, Separator).Split(EncodingQuotedPrintablePrefixForDecoding);
                    var encodedStringValue = encodedStringValueList.Last().Trim(';');
                    var decodedStringValue = DecodeQuotedPrintables(encodedStringValue);
                    splittedVCard[i] = splittedVCard[i].Replace(string.Join(string.Empty, EncodingQuotedPrintablePrefixForDecoding, TwoDots, encodedStringValue), decodedStringValue);
                }
            }

            contact.FormattedName = splittedVCard.FirstOrDefault(s => s.StartsWith(FormattedName))?.Replace(Separator, TwoDots).Split(TwoDots).LastOrDefault() ?? string.Empty;

            var names = splittedVCard.FirstOrDefault(s => s.StartsWith(Name))?.Replace(Separator, TwoDots).Split(TwoDots) ?? Array.Empty<string>();
            var firstNames = names.Length > 0 ? names.TakeLast(names.Length - 2) : Array.Empty<string>();
            contact.FirstName = string.Join(Blank, firstNames.Where(f => !string.IsNullOrWhiteSpace(f)));
            contact.LastName = names[1];

            var organizasion = splittedVCard.FirstOrDefault(s => s.StartsWith(OrganizationName))?.Replace(Separator, TwoDots).Split(TwoDots);
            contact.Organization = organizasion?.Length > 1 ? organizasion[1] : string.Empty;
            contact.OrganizationPosition = organizasion?.Length > 2 ? organizasion[2] : string.Empty;

            var title = splittedVCard.FirstOrDefault(s => s.StartsWith(TitlePrefix))?.Replace(Separator, TwoDots).Split(TwoDots) ?? Array.Empty<string>();
            contact.Title = string.Join(TwoDots, title.Length > 0 ? title.TakeLast(title.Length - 1) : Array.Empty<string>());

            var photoBase64 = splittedVCard.FirstOrDefault(s => s.StartsWith(PhotoPrefix.Split(Separator).FirstOrDefault()))?.Split(TwoDots).LastOrDefault();
            if (!string.IsNullOrWhiteSpace(photoBase64))
                contact.Photo = PhotoPrefixForDecoding + photoBase64;

            var emails = splittedVCard.Where(s => s.StartsWith(EmailPrefix.Split(Separator).FirstOrDefault()));
            foreach (var item in emails)
            {
                var emailArray = item.Replace(Separator, TwoDots).Replace(Transfer, TwoDots).Split(TwoDots);

                EMail mail = new EMail
                {
                    Type = emailArray.Length > 2 ? emailArray[2] : string.Empty,
                    Address = emailArray.LastOrDefault() ?? string.Empty,
                };
                contact.Emails.Add(mail);
            }

            var phones = splittedVCard.Where(s => s.StartsWith(PhonePrefix.Split(Separator).FirstOrDefault()));
            foreach (var item in phones)
            {
                var phoneArray = item.Replace(Separator, TwoDots).Replace(Transfer, TwoDots).Replace(Comma, TwoDots).Split(TwoDots);

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

            var urls = splittedVCard.Where(s => s.StartsWith(WebSite));
            foreach (var item in urls)
            {
                var urlArray = item.Replace(Separator, TwoDots).Replace(Transfer, TwoDots).Split(TwoDots);

                Link link = new Link
                {
                    Url = string.Join(TwoDots, urlArray.TakeLast(urlArray.Length - 1)),
                    Title = urlArray.Length > 2 ? urlArray[2] : string.Empty
                };

                if (!contact.Links.Any(l => l.Url.Equals(link.Url)))
                    contact.Links.Add(link);
            }

            return contact;
        }

        public static string EncodeVCard(this Contact contact)
        {
            StringBuilder fw = new StringBuilder();
            fw.Append(Header);
            fw.Append(NewLineLf);
            fw.Append(Version);
            fw.Append(NewLineLf);

            //Full Name
            if (!string.IsNullOrEmpty(contact.FirstName) || !string.IsNullOrEmpty(contact.LastName))
            {
                fw.Append(Name);
                fw.Append(Separator);
                fw.Append(Charset);
                fw.Append(TwoDots);
                fw.Append(contact.LastName);
                fw.Append(Separator);
                fw.Append(contact.FirstName);
                fw.Append(Separator);
                fw.Append(NewLineLf);
            }

            //Formatted Name
            if (!string.IsNullOrEmpty(contact.FormattedName))
            {
                fw.Append(FormattedName);
                fw.Append(Separator);
                fw.Append(Charset);
                fw.Append(TwoDots);
                fw.Append(contact.FormattedName);
                fw.Append(NewLineLf);
            }

            //Organization name
            if (!string.IsNullOrEmpty(contact.Organization))
            {
                fw.Append(OrganizationName);
                fw.Append(Separator);
                fw.Append(Charset);
                fw.Append(TwoDots);
                fw.Append(contact.Organization);
                if (!string.IsNullOrEmpty(contact.OrganizationPosition))
                {
                    fw.Append(Separator);
                    fw.Append(contact.OrganizationPosition);
                }
                fw.Append(NewLineLf);
            }

            //Title
            if (!string.IsNullOrEmpty(contact.Title))
            {
                fw.Append(TitlePrefix);
                fw.Append(Separator);
                fw.Append(Charset);
                fw.Append(TwoDots);
                fw.Append(contact.Title);
                fw.Append(NewLineLf);
            }

            //Photo
            if (!string.IsNullOrEmpty(contact.Photo))
            {
                fw.Append(PhotoPrefix);
                fw.Append(contact.Photo);
                fw.Append(NewLineLf);
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
                    fw.Append(NewLineLf);
                    fw.Append(ItemStr);
                    fw.Append(i);
                    fw.Append(Dot);
                    fw.Append(WebSitePrefix);
                    fw.Append(contact.Links[i].Title);
                    fw.Append(NewLineLf);
                }
            }

            //Phones
            foreach (var item in contact?.Phones ?? new List<Phone>())
            {
                fw.Append(PhonePrefix);
                fw.Append(item.Type);
                fw.Append(PhoneSubPrefix);
                fw.Append(TwoDots);
                fw.Append(item.Number);
                fw.Append(NewLineLf);
            }

            //Addresses
            foreach (var item in contact?.Addresses ?? new List<Address>())
            {
                fw.Append(AddressPrefix);
                fw.Append(item.Type);
                fw.Append(AddressSubPrefix);
                fw.Append(item.Description);
                fw.Append(NewLineLf);
            }

            //Email
            foreach (var item in contact?.Emails ?? new List<EMail>())
            {
                fw.Append(EmailPrefix);
                fw.Append(item.Type);
                fw.Append(TwoDots);
                fw.Append(item.Address);
                fw.Append(NewLineLf);
            }

            fw.Append(Footer);

            return fw.ToString();
        }

        private static string DecodeQuotedPrintables(string input)
        {
            var occurences = new Regex("(\\=[0-9A-F][0-9A-F])+");
            var matches = occurences.Matches(input);

            foreach (Match match in matches)
            {
                byte[] bytes = new byte[match.Value.Length / 3];
                for (int i = 0; i < match.Value.Length / 3; i++)
                {
                    bytes[i] = byte.Parse(match.Value.Substring(i * 3 + 1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                }
                char[] hexChar = Encoding.UTF8.GetChars(bytes);
                input = input.Replace(match.Value, string.Join(string.Empty, hexChar));
            }
            return input;
        }

    }
}
