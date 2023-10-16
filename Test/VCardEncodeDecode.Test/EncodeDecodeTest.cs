using System.Text.Json;
using VCardEncodeDecode.Helpers;
using VCardEncodeDecode.Models;
using Xunit;

namespace VCardEncodeDecode.Test
{
    public class EncodeDecodeTest
    {
        private readonly string encodedVCard = "BEGIN:VCARD\r\nVERSION:3.0\r\nN:LastName;FirstName;\r\nFN:FirstName LastName\r\nORG:Test;Tester\r\nTITLE:Software Developer\r\nitem0.URL:https://www.writeurlhere.com\r\nitem0.X-ABLabel:Other\r\nTEL;type=Home,VOICE:1111111111\r\nTEL;type=Work,VOICE:2222222222\r\nTEL;type=Cell,VOICE:+903333333333\r\nEMAIL;type=Other:email@email.com\r\nEMAIL;type=Work:email2@email.com\r\nEND:VCARD";

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
            Assert.NotEqual(encodedVCard.Replace("\r\n", ""), decodedVCard.EncodeVCard());
        }

        [Fact]
        public void PhoneCantNull()
        {
            decodedVCard.Phones = null;

            Assert.Throws<NullReferenceException>(() => decodedVCard.EncodeVCard());
        }
    }
}