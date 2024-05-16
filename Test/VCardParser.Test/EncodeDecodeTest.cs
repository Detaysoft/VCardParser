using System.Text.Json;
using VCardParser.Helpers;
using VCardParser.Models;
using Xunit;

namespace VCardParser.Test
{
    public class EncodeDecodeTest
    {
        private readonly string encodedVCard = "BEGIN:VCARD\nVERSION:2.1\nN;CHARSET=UTF-8:LastName;FirstName;\nFN;CHARSET=UTF-8:FirstName LastName\nORG;CHARSET=UTF-8:Test;Tester\nTITLE;CHARSET=UTF-8:Software Developer\nitem0.URL:https://www.writeurlhere.com\nitem0.X-ABLabel:Other\nTEL;type=Home,VOICE:1111111111\nTEL;type=Work,VOICE:2222222222\nTEL;type=Cell,VOICE:+903333333333\nEMAIL;type=Other:email@email.com\nEMAIL;type=Work:email2@email.com\nEND:VCARD";

        private readonly Contact decodedVCard = new Contact
        {
            FirstName = "FirstName",
            LastName = "LastName",
            FormattedName = "FirstName LastName",
            Phones = new List<Phone>
                {
                    { new Phone() { Number = "1111111111", Type = "Home" } },
                    { new Phone() { Number = "2222222222", Type = "Work" } },
                    { new Phone() { Number = "+903333333333", Type = "Cell" } }
                },
            Emails = new List<EMail>
                {
                    { new EMail() { Address = "email@email.com", Type = "Other" } },
                    { new EMail() { Address = "email2@email.com", Type = "Work" } }
                },
            Links = new List<Link>
                {
                    { new Link() { Url = "https://www.writeurlhere.com", Title = "Other"} }
                },
            Organization = "Test",
            Title = "Software Developer",
            OrganizationPosition = "Tester",
        };

        [Fact]
        public void DecodeTest()
        {
            Assert.Equal(JsonSerializer.Serialize(encodedVCard.DecodeVCard()), JsonSerializer.Serialize(decodedVCard));
        }

        [Fact]
        public void EncodeTest()
        {
            Assert.Equal(encodedVCard, decodedVCard.EncodeVCard());
        }

        [Fact]
        public void FailureTest()
        {
            Assert.NotEqual(encodedVCard.Replace("\n", ""), decodedVCard.EncodeVCard());
        }
    }
}