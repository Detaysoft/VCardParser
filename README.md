# VCardParser

You can perform VCard decode and encode operations by using the contact class in the project.

The nuget packages is available [here](https://www.nuget.org/packages/VCardParser)
## How to use

* NuGet Command: **Install-Package VCardParser**

* Example
```C#
using VCardEncodeDecode.Helpers;
using VCardEncodeDecode.Models;

Contact contact = new Contact
{
    FirstName = "FirstName",
    LastName = "LastName",
    FormattedName = "FirstName LastName",
    Organization = "Company",
    OrganizationPosition = "Team",
    Title = "Title",
    Photo = "bae64foto",
    Emails = new List<EMail>
    {
        new EMail
        {
            Address = "email@mail.com",
            Type = "Home"
        },
        new EMail
        {
            Address = "email2@mail.com",
            Type = "Company"
        },
    },
    Phones = new List<Phone>
    {
        new Phone
        {
            Number = "1111111111",
            Type = "Cell"
        },
        new Phone
        {
            Number = "+902222222222",
            Type = "Company"
        },
    }
};

// encode contact model for creating vcard file
var encodedVCard = contact.EncodeVCard();

// console output
Console.WriteLine(encodedVCard);

// exporting vcf file
File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filename.vcf"), encodedVCard);

// decode vcard file from vcf file to contact model
encodedVCard.DecodeVCard();
```