using System.Reflection;
using Fare;
using TruliooExtension.Entities;
using AutoBogus;
using AutoBogus.Conventions;
using AutoBogus.FakeItEasy;
using Bogus;
using Bogus.Extensions.Brazil;
using Bogus.Extensions.Canada;
using Bogus.Extensions.Denmark;
using Bogus.Extensions.Finland;
using Bogus.Extensions.Italy;
using Bogus.Extensions.Norway;
using Bogus.Extensions.Poland;
using Bogus.Extensions.Portugal;
using Bogus.Extensions.Romania;
using Bogus.Extensions.Sweden;
using Bogus.Extensions.UnitedKingdom;
using Bogus.Extensions.UnitedStates;

namespace TruliooExtension.Common;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class FieldFaker
{
    public static List<string> AllFieldName()
    {
        return typeof(FieldFaker).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToList();
    }

    public static FieldFaker GenerateWithCustomFieldGroup(CustomFieldGroup customFieldGroupGlobal, CustomFieldGroup customFieldGroup)
    {
        var faker = new AutoFaker<FieldFaker>()
            .Configure(opt =>
            {
                opt.WithLocale(customFieldGroup.Culture)
                    .WithBinder<FakeItEasyBinder>()
                    .WithConventions();
            });

        ConfigureCredentialFields(faker);
        ConfigureNameFields(faker);
        ConfigureAddressFields(faker);
        ConfigurePhoneFields(faker);
        ConfigureEmailFields(faker);
        ConfigureDateOfBirthFields(faker);
        ConfigureIDFields(faker);
        ConfigureKYBFields(faker);

        var random = new Random();
        ConfigureCustomFields(faker, customFieldGroup, random);
        ConfigureCustomFields(faker, customFieldGroupGlobal, random);

        return faker
            .FinishWith((f, ff) =>
            {
                ff.ConfirmPassword = ff.Password;
                ff.RepeatPassword = ff.Password;
            })
            .FinishWith((f, ff) => ConfigureCultureSpecificFields(f, ff, customFieldGroup.Culture))
            .Generate();
    }
    private static void ConfigureCredentialFields(AutoFaker<FieldFaker> faker)
    {
        faker.RuleFor(o => o.UserName, f => f.Internet.UserName())
            .RuleFor(o => o.Password, f => f.Internet.Password())
            .RuleFor(o => o.ConfirmPassword, f => f.Internet.Password())
            .RuleFor(o => o.RepeatPassword, f => f.Internet.Password());
    }

    private static void ConfigureNameFields(AutoFaker<FieldFaker> faker)
    {
        var nameFields = new Dictionary<string, Func<Faker, string>>
        {
            {
                nameof(FieldFaker.Name), f => f.Name.FullName()
            },
            {
                nameof(FieldFaker.FirstName), f => f.Name.FirstName()
            },
            {
                nameof(FieldFaker.MiddleName), f => f.Name.Random.Words(1)
            },
            {
                nameof(FieldFaker.LastName), f => f.Name.LastName()
            },
            {
                nameof(FieldFaker.FullName), f => f.Name.FullName()
            },
            {
                nameof(FieldFaker.FirstInitial), f => f.Name.FirstName()[0].ToString()
            },
            {
                nameof(FieldFaker.Prefix), f => f.Name.Prefix()
            },
            {
                nameof(FieldFaker.Gender), f => f.PickRandom("M", "F")
            },
            {
                nameof(FieldFaker.Surname), f => f.Name.LastName()
            },
            {
                nameof(FieldFaker.GivenNames), f => f.Name.FirstName()
            },
            {
                nameof(FieldFaker.MiddleInitial), f => f.Name.FirstName()[0].ToString()
            },
            {
                nameof(FieldFaker.FirstSurname), f => f.Name.LastName()
            },
            {
                nameof(FieldFaker.SecondSurname), f => f.Name.LastName()
            },
            {
                nameof(FieldFaker.NameOnCard), f => f.Name.FullName()
            },
            {
                nameof(FieldFaker.PassportFullName), f => f.Name.FullName()
            },
            {
                nameof(FieldFaker.certFamilyName), f => f.Name.LastName()
            },
            {
                nameof(FieldFaker.certGivenNames), f => f.Name.FullName()
            },
            {
                nameof(FieldFaker.mdFullName), f => f.Name.FullName()
            },
            {
                nameof(FieldFaker.imFamilyName), f => f.Name.LastName()
            },
            {
                nameof(FieldFaker.imGivenName), f => f.Name.FirstName()
            },
        };

        foreach (var field in nameFields)
        {
            faker.RuleFor(field.Key, field.Value);
        }
    }

    private static void ConfigureAddressFields(AutoFaker<FieldFaker> faker)
    {
        var addressFields = new Dictionary<string, Func<Faker, string>>
        {
            {
                nameof(FieldFaker.Address1), f => f.Address.FullAddress()
            },
            {
                nameof(FieldFaker.UnitNumber), f => f.Address.BuildingNumber()
            },
            {
                nameof(FieldFaker.StreetNumber), f => f.Address.BuildingNumber()
            },
            {
                nameof(FieldFaker.StreetName), f => f.Address.StreetName()
            },
            {
                nameof(FieldFaker.StreetType), f => f.Address.StreetSuffix()
            },
            {
                nameof(FieldFaker.Suburb), f => f.Address.City()
            },
            {
                nameof(FieldFaker.State), f => f.Address.State()
            },
            {
                nameof(FieldFaker.PostalCode), f => f.Address.ZipCode()
            },
            {
                nameof(FieldFaker.Address2), f => f.Address.SecondaryAddress()
            },
            {
                nameof(FieldFaker.ProvinceCode), f => f.Address.StateAbbr()
            },
            {
                nameof(FieldFaker.City), f => f.Address.City()
            },
            {
                nameof(FieldFaker.HouseNumber), f => f.Address.BuildingNumber()
            },
            {
                nameof(FieldFaker.BuildingNumber), f => f.Address.BuildingNumber()
            },
            {
                nameof(FieldFaker.BuildingName), f => f.Address.Random.Word()
            },
            {
                nameof(FieldFaker.District), f => f.Address.City()
            },
            {
                nameof(FieldFaker.CivicNumber), f => f.Address.BuildingNumber()
            },
            {
                nameof(FieldFaker.Province), f => f.Address.State()
            },
            {
                nameof(FieldFaker.County), f => f.Address.County()
            },
            {
                nameof(FieldFaker.FloorNumber), f => f.Random.Number().ToString()
            },
            {
                nameof(FieldFaker.Prefecture), f => f.Address.State()
            },
            {
                nameof(FieldFaker.Aza), f => f.Address.StreetName()
            },
            {
                nameof(FieldFaker.AreaNumbers), f => f.Address.Random.Number(32000).ToString()
            },
            {
                nameof(FieldFaker.Municipality), f => f.Address.City()
            },
            {
                nameof(FieldFaker.HouseExtension), f => f.PickRandomParam("A", "B", "C", "D", "E", "F", "G")
            },
            {
                nameof(FieldFaker.StateProvince), f => f.Address.State()
            },
            {
                nameof(FieldFaker.DependentStreetName), f => f.Address.StreetName()
            },
            {
                nameof(FieldFaker.DependentStreetType), f => f.Address.StreetSuffix()
            },
            {
                nameof(FieldFaker.Street1), f => f.Address.StreetName()
            }
        };

        foreach (var field in addressFields)
        {
            faker.RuleFor(field.Key, field.Value);
        }
    }

    private static void ConfigurePhoneFields(AutoFaker<FieldFaker> faker)
    {
        var phoneFields = new Dictionary<string, Func<Faker, string>>
        {
            {
                nameof(FieldFaker.Telephone), f => f.Phone.PhoneNumber()
            },
            {
                nameof(FieldFaker.Telephone2), f => f.Phone.PhoneNumber()
            },
            {
                nameof(FieldFaker.HomeTelephoneNumber), f => f.Phone.PhoneNumber()
            },
            {
                nameof(FieldFaker.WorkTelephoneNumber), f => f.Phone.PhoneNumber()
            },
            {
                nameof(FieldFaker.CellNumber), f => f.Phone.PhoneNumber()
            },
            {
                nameof(FieldFaker.WorkTelephone), f => f.Phone.PhoneNumber()
            },
            {
                nameof(FieldFaker.Phone), f => f.Phone.PhoneNumber()
            }
        };

        foreach (var field in phoneFields)
        {
            faker.RuleFor(field.Key, field.Value);
        }
    }

    private static void ConfigureEmailFields(AutoFaker<FieldFaker> faker)
    {
        var emailFields = new Dictionary<string, Func<Faker, string>>
        {
            {
                nameof(FieldFaker.EmailAddress), f => f.Internet.Email()
            },
            {
                nameof(FieldFaker.Email), f => f.Internet.Email()
            }
        };

        foreach (var field in emailFields)
        {
            faker.RuleFor(field.Key, field.Value);
        }
    }

    private static void ConfigureDateOfBirthFields(AutoFaker<FieldFaker> faker)
    {
        faker.RuleFor(o => o.DayOfBirth, f => f.Date.Past(20).Day.ToString())
            .RuleFor(o => o.MonthOfBirth, f => f.Date.Past(20).Month.ToString())
            .RuleFor(o => o.YearOfBirth, f => f.Date.Past(20).Year.ToString());
    }

    private static void ConfigureIDFields(AutoFaker<FieldFaker> faker)
    {
        var idFields = new Dictionary<string, Func<Faker, string>>
        {
            {
                nameof(FieldFaker.NationalIDNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.TaxIDNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.SocialInsuranceNumber), f => f.Person.Sin()
            },
            {
                nameof(FieldFaker.PersonalIdentityCode), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.InseeNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.HongKongIDNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.PersonalPublicServiceNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.CodiceFiscale), f => string.Join("", f.Random.Digits(16))
            },
            {
                nameof(FieldFaker.CURPIDNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.NRICNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.PinNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.SgNRICNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.SocialSecurityNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.PassportNumber), f => string.Join("", f.Random.Digits(11))
            },
            {
                nameof(FieldFaker.Cpf), f => f.Person.Cpf()
            },
            {
                nameof(FieldFaker.Cnpj), f => f.Company.Cnpj()
            },
            {
                nameof(FieldFaker.Sin), f => f.Person.Sin()
            }
        };

        foreach (var field in idFields)
        {
            faker.RuleFor(field.Key, field.Value);
        }
    }

    private static void ConfigureKYBFields(AutoFaker<FieldFaker> faker)
    {
        var kybFields = new Dictionary<string, Func<Faker, string>>
        {
            {
                nameof(FieldFaker.BusinessName), f => f.Company.CompanyName()
            },
            {
                nameof(FieldFaker.BusinessRegistrationNumber), f => f.Random.AlphaNumeric(10)
            },
            {
                nameof(FieldFaker.DUNSNumber), f => f.Random.AlphaNumeric(9)
            },
            {
                nameof(FieldFaker.TradestyleName), f => f.Company.CompanyName()
            },
            {
                nameof(FieldFaker.JurisdictionOfIncorporation), f => f.Address.State()
            }
        };

        foreach (var field in kybFields)
        {
            faker.RuleFor(field.Key, field.Value);
        }
    }

    private static void ConfigureCustomFields(AutoFaker<FieldFaker> faker, CustomFieldGroup customFieldGroup, Random random)
    {
        foreach (var customField in customFieldGroup.CustomFields)
        {
            if (!string.IsNullOrEmpty(customField.StaticValue))
            {
                faker.RuleFor(customField.DataField, _ => customField.StaticValue);
            }

            if (!string.IsNullOrEmpty(customField.Template))
            {
                faker.RuleFor(customField.DataField, _ => new Xeger(customField.Template, random).Generate());
            }

            if (customField.IsIgnore)
            {
                faker.Ignore(customField.DataField);
            }
        }
    }

// ReSharper disable once CognitiveComplexity
    private static void ConfigureCultureSpecificFields(Faker f, FieldFaker ff, string culture)
    {
        switch (culture)
        {
            case "pt_BR":
                ff.TaxIDNumber = f.Person.Cpf();
                ff.BusinessRegistrationNumber = f.Company.Cnpj();
                break;

            case "fr_CA":
                ff.SocialInsuranceNumber = f.Person.Sin();
                break;

            case "da":
                ff.CPRNumber = f.Person.Cpr();
                break;

            case "fi":
                ff.PersonalIdentityCode = f.Person.Henkilotunnus();
                ff.NationalIDNumber = f.Person.Henkilotunnus();
                break;

            case "it":
                ff.CodiceFiscale = f.Person.CodiceFiscale();
                ff.TaxIDNumber = f.Finance.CodiceFiscale(ff.LastName, ff.FirstName,
                    DateTime.Parse(ff.YearOfBirth + "-" + ff.MonthOfBirth + "-" + ff.DayOfBirth, new CultureInfo("en")),
                    ff.Gender == "M");
                break;

            case "nb_NO":
                ff.PersonalIdentityCode = f.Person.Fodselsnummer();
                ff.NationalIDNumber = f.Person.Fodselsnummer();
                break;

            case "pl":
                ff.NationalIDNumber = f.Person.Pesel();
                ff.TaxIDNumber = f.Company.Nip();
                ff.BusinessRegistrationNumber = f.Company.Regon();
                break;

            case "pt_PT":
                ff.TaxIDNumber = f.Person.Nif();
                ff.NationalIDNumber = f.Person.Nif();
                ff.BusinessRegistrationNumber = f.Company.Nipc();
                break;

            case "ro":
                ff.TaxIDNumber = f.Person.Cnp();
                ff.NationalIDNumber = f.Person.Cnp();
                ff.PersonalIdentityCode = f.Person.Cnp();
                break;

            case "sv":
                ff.PersonalIdentityCode = f.Person.Samordningsnummer();
                ff.NationalIDNumber = f.Person.Personnummer();
                ff.TaxIDNumber = f.Person.Personnummer();
                break;

            case "en_GB":
            case "en_IE":
                ff.DriverLicenceVersionNumber = f.Vehicle.GbRegistrationPlate(DateTime.Now.Subtract(TimeSpan.FromDays(365 * 18)), DateTime.Now);
                ff.SortCode = f.Finance.SortCode();
                ff.SocialInsuranceNumber = f.Finance.Nino();
                ff.NINumber = f.Finance.Nino();
                ff.TaxIDNumber = f.Finance.VatNumber(VatRegistrationNumberType.Standard);
                ff.Country = f.Address.CountryOfUnitedKingdom();
                break;

            case "en_US":
            case "en_CA":
                ff.SocialSecurityNumber = f.Person.Ssn();
                ff.TaxIDNumber = f.Company.Ein();
                ff.NationalIDNumber = f.Person.Ssn();
                ff.USCreditAgency = f.Finance.CreditCardNumber();
                ff.US1987SIC1 = f.Finance.Account(4);
                ff.US1987SIC2 = f.Finance.Account(4);
                ff.US1987SIC3 = f.Finance.Account(4);
                ff.US1987SIC4 = f.Finance.Account(4);
                ff.US1987SIC5 = f.Finance.Account(4);
                ff.US1987SIC6 = f.Finance.Account(4);
                break;
        }
    }
    public string Name { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string MaidenName { get; set; }
    public string Address1 { get; set; }
    public string UnitNumber { get; set; }
    public string StreetNumber { get; set; }
    public string StreetName { get; set; }
    public string StreetType { get; set; }
    public string Suburb { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Telephone { get; set; }
    public string Gender { get; set; }
    public string DayOfBirth { get; set; }
    public string MonthOfBirth { get; set; }
    public string YearOfBirth { get; set; }
    public string FullName { get; set; }
    public string AccessYear { get; set; }
    public string MedicareNumber { get; set; }
    public string MedicareReference { get; set; }
    public string TaxFileNumber { get; set; }
    public string DriverLicenceNumber { get; set; }
    public string FirstInitial { get; set; }
    public string RTACardNumber { get; set; }
    public string DayOfExpiry { get; set; }
    public string MonthOfExpiry { get; set; }
    public string YearOfExpiry { get; set; }
    public string PassportNumber { get; set; }
    public string PlaceOfBirth { get; set; }
    public string CountryOfBirth { get; set; }
    public string FamilyNameAtBirth { get; set; }
    public string FamilyNameAtCitizenship { get; set; }
    public string Prefix { get; set; }
    public string SocialSecurityNumber { get; set; }
    public string DriverLicenceState { get; set; }
    public string HouseNumber { get; set; }
    public string City { get; set; }
    public string SocialInsuranceNumber { get; set; }
    public string CPRNumber { get; set; }
    public string PassportMRZLine1 { get; set; }
    public string PassportMRZLine2 { get; set; }
    public string Nationality { get; set; }
    public string NationalIDNumber { get; set; }
    public string BusinessSuburb { get; set; }
    public string Province { get; set; }
    public string District { get; set; }
    public string County { get; set; }
    public string BuildingName { get; set; }
    public string RoomNumber { get; set; }
    public string BuildingNumber { get; set; }
    public string CityOfIssue { get; set; }
    public string ProvinceOfIssue { get; set; }
    public string DistrictOfIssue { get; set; }
    public string CountyOfIssue { get; set; }
    public string FirstSurname { get; set; }
    public string SecondSurname { get; set; }
    public string CURPIDNumber { get; set; }
    public string FloorNumber { get; set; }
    public string DriverLicenceVersionNumber { get; set; }
    public string InseeNumber { get; set; }
    public string PinNumber { get; set; }
    public string JpegImage { get; set; }
    public string GivenInitials { get; set; }
    public string RegistrationNumber { get; set; }
    public string SgNRICNumber { get; set; }
    public string RegistrationYear { get; set; }
    public string CertificateNumber { get; set; }
    public string YearOfPrint { get; set; }
    public string MonthOfPrint { get; set; }
    public string DayOfPrint { get; set; }
    public string GroomFamilyName { get; set; }
    public string GroomGivenName { get; set; }
    public string BrideFamilyName { get; set; }
    public string BrideGivenName { get; set; }
    public string YearOfMarriage { get; set; }
    public string MonthOfMarriage { get; set; }
    public string DayOfMarriage { get; set; }
    public string RegistrationState { get; set; }
    public string Surname { get; set; }
    public string GivenNames { get; set; }
    public string BlockNumber { get; set; }
    public string PassportFullName { get; set; }
    public string PassportDayOfExpiry { get; set; }
    public string PassportMonthOfExpiry { get; set; }
    public string PassportYearOfExpiry { get; set; }
    public string Address2 { get; set; }
    public string ProvinceCode { get; set; }
    public string HomeTelephoneAreaCode { get; set; }
    public string HomeTelephoneNumber { get; set; }
    public string WorkTelephoneAreaCode { get; set; }
    public string WorkTelephoneNumber { get; set; }
    public string CellNumber { get; set; }
    public string Street1 { get; set; }
    public string Region { get; set; }
    public string HongKongIDNumber { get; set; }
    public string CodiceFiscale { get; set; }
    public string TelephonePrefix { get; set; }
    public string CityOfBirth { get; set; }
    public string ProvinceOfBirth { get; set; }
    public string ItIdDocumentNumber { get; set; }
    public string ItIdDocumentYearOfIssue { get; set; }
    public string ItIdDocumentMonthOfIssue { get; set; }
    public string ItIdDocumentDayOfIssue { get; set; }
    public string ItIdDocumentCityOfIssue { get; set; }
    public string ItIdDocumentProvinceOfIssue { get; set; }
    public string ItIdDocumentCountryOfIssue { get; set; }
    public string HouseExtension { get; set; }
    public string Prefecture { get; set; }
    public string Municipality { get; set; }
    public string Aza { get; set; }
    public string AreaNumbers { get; set; }
    public string CivicNumber { get; set; }
    public string BirthRegistrationNumber { get; set; }
    public string BirthRegistrationYear { get; set; }
    public string BirthCertificateNumber { get; set; }
    public string BirthRegistrationState { get; set; }
    public string YearOfBirthCertificatePrinting { get; set; }
    public string MonthOfBirthCertificatePrinting { get; set; }
    public string DayOfBirthCertificatePrinting { get; set; }
    public string FamilyName { get; set; }
    public string PassportCountry { get; set; }
    public string WatchlistState { get; set; }
    public string WatchlistData { get; set; }
    public string WatchlistFullName { get; set; }
    public string CanadaSixMonthResidency { get; set; }
    public string SecondUnitNumber { get; set; }
    public string SecondStreetNumber { get; set; }
    public string SecondStreetName { get; set; }
    public string SecondStreetType { get; set; }
    public string SecondSuburb { get; set; }
    public string SecondState { get; set; }
    public string SecondPostalCode { get; set; }
    public string SecondPropertyName { get; set; }
    public string SecondPropertyName2 { get; set; }
    public string PropertyName { get; set; }
    public string PropertyName2 { get; set; }
    public string DependentStreetName { get; set; }
    public string DependentStreetType { get; set; }
    public string WorkTelephone { get; set; }
    public string MiddleInitial { get; set; }
    public string PersonalPublicServiceNumber { get; set; }
    public string IsDeceased { get; set; }
    public string DeceasedMatchText { get; set; }
    public string NHSNumber { get; set; }
    public string EmailAddress { get; set; }
    public string NINumber { get; set; }
    public string NRICNumber { get; set; }
    public string ItIdDocumentType { get; set; }
    public string FraudFlag { get; set; }
    public string StateOfBirth { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string RepeatPassword { get; set; }
    public string PersonalIdentityCode { get; set; }
    public string MinimumAge { get; set; }
    public string MedicareColor { get; set; }
    public string MedicareDayOfExpiry { get; set; }
    public string MedicareMonthOfExpiry { get; set; }
    public string MedicareYearOfExpiry { get; set; }
    public string AuImmiCardNumber { get; set; }
    public string StockNumber { get; set; }
    public string CitizenshipAcquisitionYear { get; set; }
    public string CitizenshipAcquisitionMonth { get; set; }
    public string CitizenshipAcquisitionDay { get; set; }
    public string MxRFCNumber { get; set; }

    //The following added specifically for business verification
    public string BusinessName { get; set; }
    public string TradestyleName { get; set; }
    public string RegisteredAddressIndicator { get; set; }
    public string Country { get; set; }
    public string MailingAddress1 { get; set; }
    public string MailingCity { get; set; }
    public string MailingCounty { get; set; }
    public string MailingState { get; set; }
    public string MailingCountry { get; set; }
    public string MailingPostalCode { get; set; }
    public string BusinessIDType { get; set; }
    public string CountryAccessCode { get; set; }
    public string CableTelexNumber { get; set; }
    public string FacsimileNumber { get; set; }
    public string ChiefExecutiveOfficerName { get; set; }
    public string ChiefExecutiveOfficerTitle { get; set; }
    public string LineOfBusiness { get; set; }
    public string US1987SIC1 { get; set; }
    public string US1987SIC2 { get; set; }
    public string US1987SIC3 { get; set; }
    public string US1987SIC4 { get; set; }
    public string US1987SIC5 { get; set; }
    public string US1987SIC6 { get; set; }
    public string PrimaryLocalActivityCode { get; set; }
    public string LocalActivityType { get; set; }
    public string YearStarted { get; set; }
    public string SalesVolumeLocalCurrency { get; set; }
    public string SalesVolumeReliability { get; set; }
    public string SalesVolumeUSDollars { get; set; }
    public string LocalCurrency { get; set; }
    public string EmployeesHere { get; set; }
    public string EmployeesHereReliability { get; set; }
    public string EmployeesTotal { get; set; }
    public string EmployeesTotalReliability { get; set; }
    public string PrincipalsIncludedIndicator { get; set; }
    public string ImportExportAgent { get; set; }
    public string LegalStatus { get; set; }
    public string OrganizationalStatus { get; set; }
    public string SubsidiaryIndicator { get; set; }
    public string FullReportYear { get; set; }
    public string FullReportMonth { get; set; }
    public string FullReportDay { get; set; }
    public string HeadquarterParentBusinessName { get; set; }
    public string HeadquarterParentAddress1 { get; set; }
    public string HeadquarterParentCity { get; set; }
    public string HeadquarterParentState { get; set; }
    public string HeadquarterParentCountry { get; set; }
    public string HeadquarterParentPostalCode { get; set; }
    public string DomesticUltimateBusinessName { get; set; }
    public string DomesticUltimateAddress1 { get; set; }
    public string DomesticUltimateCity { get; set; }
    public string DomesticUltimateState { get; set; }
    public string DomesticUltimatePostalCode { get; set; }
    public string GlobalUltimateIndicator { get; set; }
    public string GlobalUltimateBusinessName { get; set; }
    public string GlobalUltimateAddress1 { get; set; }
    public string GlobalUltimateCity { get; set; }
    public string GlobalUltimateState { get; set; }
    public string GlobalUltimateCountry { get; set; }
    public string GlobalUltimatePostalCode { get; set; }
    public string FamilyMembersCount { get; set; }
    public string HierarchyCode { get; set; }
    public string LastUpdateYear { get; set; }
    public string LastUpdateMonth { get; set; }
    public string LastUpdateDay { get; set; }
    public string PublicRecordIndicator { get; set; }
    public string OpenSuitIndicator { get; set; }
    public string OpenLienIndicator { get; set; }
    public string OpenJudgementIndicator { get; set; }
    public string BankruptcyIndicator { get; set; }

    public string BusinessIDNumber { get; set; }

    //End of fields added specifically for business verification
    public string UserID { get; set; }
    public string IPAddress { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string AuthenticityScore { get; set; }
    public string AuthenticityDetails { get; set; }
    public string AuthenticityReasons { get; set; }
    public string Cpf { get; set; }
    public string Cnpj { get; set; }
    public string Sin { get; set; }
    public string DUNSNumber { get; set; } //Business verification
    public string certFamilyName { get; set; }
    public string certGivenNames { get; set; }
    public string dlFamilyName { get; set; }
    public string dlGivenName { get; set; }
    public string dlMiddleName { get; set; }
    public string imFamilyName { get; set; }
    public string imGivenName { get; set; }
    public string mdFullName { get; set; }
    public string ppFamilyName { get; set; }
    public string ppGivenNames { get; set; }
    public string viFamilyName { get; set; }
    public string viGivenNames { get; set; }
    public string SubRegion { get; set; }
    public string LatinFullName { get; set; }
    public string MonthStarted { get; set; }               //Business verification
    public string DayStarted { get; set; }                 //Business verification
    public string SecondaryLocalActivityCode { get; set; } //Business verification
    public string VehicleRegistrationPlate { get; set; }
    public string MothersFirstNames { get; set; }
    public string MothersLastName { get; set; }
    public string FathersFirstNames { get; set; }
    public string FathersLastName { get; set; }
    public string Location { get; set; }
    public string BankAccountNumber { get; set; }
    public string AddressAssociation { get; set; }
    public string NameOnCard { get; set; }
    public string FatherFullName { get; set; }
    public string POBox { get; set; }
    public string DriverLicenseFrontImage { get; set; }
    public string DriverLicenseBackImage { get; set; }
    public string PassportImage { get; set; }
    public string PassportSerie { get; set; }
    public string WatchlistHitDetails { get; set; }
    public string ThreeYearCredit { get; set; }
    public string FraudMessage { get; set; }
    public string SerialNumber { get; set; }
    public string TestField { get; set; }
    public string DocumentFrontImage { get; set; }
    public string DocumentBackImage { get; set; }
    public string LivePhoto { get; set; }
    public string DocumentType { get; set; }
    public string DayOfIssue { get; set; }
    public string MonthOfIssue { get; set; }
    public string YearOfIssue { get; set; }
    public string Issuer { get; set; }
    public string DocumentNumber { get; set; }
    public string MRZ1 { get; set; }
    public string MRZ2 { get; set; }
    public string MRZ3 { get; set; }
    public string Class { get; set; }
    public string VoterID { get; set; }
    public string DocumentVerified { get; set; }
    public string DatasourceTransactionID { get; set; }
    public string HouseRegistrationNumber { get; set; }
    public string Telephone2 { get; set; }
    public string BusinessStatus { get; set; }
    public string YearOfIncorporation { get; set; }
    public string MonthOfIncorporation { get; set; }
    public string DayOfIncorporation { get; set; }
    public string BusinessDetails { get; set; }
    public string PhoneContractType { get; set; }
    public string PhoneContractStatus { get; set; }
    public string InternalPassportNumber { get; set; }
    public string StateProvince { get; set; }
    public string DocumentSeries { get; set; }
    public string TaxIDNumber { get; set; }
    public string Source { get; set; }
    public string BusinessRegistrationNumber { get; set; }
    public string JurisdictionOfIncorporation { get; set; }
    public string AcceptIncompleteDocument { get; set; }
    public string ShareholderListDocument { get; set; }
    public string DocumentCost { get; set; }
    public string DocumentUrl { get; set; }
    public string DocumentMetaData { get; set; }
    public string DocumentAvailable { get; set; }
    public string PhoneCityOfIssue { get; set; }
    public string Operator { get; set; }
    public string ThirdPartyReference { get; set; }
    public string PhoneProvinceOfIssue { get; set; }
    public string SortCode { get; set; }
    public string CreditDetails { get; set; }
    public string BusinessType { get; set; }
    public string BusinessActivities { get; set; }
    public string UBO { get; set; }
    public string EnhancedProfile { get; set; }
    public string PersonsWithSignificantControl { get; set; }
    public string UltimateBeneficialOwners { get; set; }
    public string PhoneType { get; set; }
    public string Barcode { get; set; }
    public string KybSearchResult { get; set; }
    public string NumberOfResultsReceived { get; set; }
    public string IsDatasourceCalled { get; set; }
    public string IsResponseReceived { get; set; }
    public string IsUniqueHit { get; set; }
    public string FinancialInformationDocument { get; set; }
    public string Score { get; set; }
    public string PhoneAccountType { get; set; }
    public string ValidateDocumentImageQuality { get; set; }
    public string USCreditAgency { get; set; }
    public string USResidentFiles { get; set; }
    public string USTelephoneFile { get; set; }
    public string PhoneAccountUser { get; set; }
    public string MitekManual { get; set; }
    public string BusinessNameSearchMessage { get; set; }
    public string BusinessNameSearchCount { get; set; }
    public string BusinessNameSearchResults { get; set; }
    public string BusinessRegistrationNumberSearchMessage { get; set; }
    public string BusinessRegistrationNumberSearchCount { get; set; }
    public string BusinessRegistrationNumberSearchResults { get; set; }
    public string Entities { get; set; }
    public string DualProcess { get; set; }
    public string BusinessNameBestMatchCount { get; set; }
    public string DocumentExtracted { get; set; }
    public string VendorDocumentClassification { get; set; }
    public string PhoneAccountTenure { get; set; }
    public string IsBeta { get; set; }
    public string DocumentCountryCode { get; set; }
    public string BasicProductCalled { get; set; }
    public string BasicProductReturned { get; set; }
    public string EnhancedProductCalled { get; set; }
    public string EnhancedProductReturned { get; set; }
    public string EntitiesProductCalled { get; set; }
    public string EntitiesProductReturned { get; set; }
    public string Communication { get; set; }
    public string SearchProductCalled { get; set; }
    public string SearchProductReturned { get; set; }
    public string RequestReasonCode { get; set; }
    public string MatchingDetails { get; set; }
    public string BusinessLegalForm { get; set; }
    public string IndustryLegalForm { get; set; }
    public string IndustryCodeDescription { get; set; }
    public string ResultInformation { get; set; }
    public string DocumentProductRequested { get; set; }
    public string DocumentProductReturned { get; set; }
    public string ShareholderOrderingMessage { get; set; }
    public string RiskLevel { get; set; }
    public string Recommendation { get; set; }
    public string TamperingCheck { get; set; }
    public string ClassifiedDocument { get; set; }
    public string DataCrosscheck { get; set; }
    public string DataValidation { get; set; }
    public string DocumentCountryCheck { get; set; }
    public string LivePhotoCrosscheck { get; set; }
    public string DocumentData { get; set; }
    public string FieldUsed { get; set; }
    public string SecurityFeatureCheck { get; set; }
    public string ValidDocument { get; set; }
    public string MachineReadableDocument { get; set; }
    public string DocumentTypeCheck { get; set; }
    public string PhoneDeviceStatus { get; set; }
    public string PhoneRoamingStatus { get; set; }
    public string EmailDiagnostics { get; set; }
    public string EmailWarnings { get; set; }
    public string ValidEmail { get; set; }
    public string EmailDomainCreationDays { get; set; }
    public string EmailFirstSeenDays { get; set; }
    public string ValidIPAddress { get; set; }
    public string IPAddressWarnings { get; set; }
    public string IPDistanceFromAddress { get; set; }
    public string IPDistanceFromPhone { get; set; }
    public string IPAddressCity { get; set; }
    public string IPAddressCountry { get; set; }
    public string IPAddressPostalCode { get; set; }
    public string OriginalBusinessStatus { get; set; }
    public string Shareholders { get; set; }
    public string ManagingDirectors { get; set; }
    public string Secretaries { get; set; }
    public string Directors { get; set; }
    public string Officers { get; set; }
    public string CountryLocation { get; set; }
    public string RegisteredCountry { get; set; }
    public string OtherEntities { get; set; }
    public string ClassifiedType { get; set; }
    public string DateOfDissolution { get; set; }
    public string PreviousName { get; set; }
    public string FullAddress { get; set; }
    public string AddressCountryCode { get; set; }
    public string AddressCountryName { get; set; }
    public string SiretNumber { get; set; }
    public string Shares { get; set; }
    public string LegalIdentityNumber { get; set; }
    public string BusinessNameChangeDate { get; set; }
    public string OtherData { get; set; }
    public string BusinessNameISOLatin { get; set; }
    public string NameOfOfficialRegistrar { get; set; }
    public string Branches { get; set; }
    public string BusinessLinks { get; set; }
    public string Monitoring { get; set; }
    public string DocumentUOM { get; set; }
    public string ProductRetrievalDetails { get; set; }
    public string DocumentExtractions { get; set; }
    public string Website { get; set; }
    public string EnhancedRaw { get; set; }
    public string VendorTransactionalReferences { get; set; }
    public string AdditionalProductsCalled { get; set; }
    public string AdditionalProductsReturned { get; set; }
    public string FieldValidationWarning { get; set; }
    public string FieldPreprocessingWarning { get; set; }
    public string OfficialDocument { get; set; }
    public string FieldValueModified { get; set; }
    public string FieldValueFormat { get; set; }
    public string OtherNames { get; set; }
    public string WebDomain { get; set; }
    public string WebDomains { get; set; }
    public string CountryName { get; set; }
    public string TaxIDNumbers { get; set; }
    public string IndustryCodeDescriptions { get; set; }
    public string CommandAnalytics { get; set; }
    public string BusinessStatusDetails { get; set; }
    public string AlternateFirstName { get; set; }
    public string AlternateMiddleName { get; set; }
    public string AlternateLastName { get; set; }
    public string AlternateFullName { get; set; }
    public string AlternateAddressLine1 { get; set; }
    public string AlternateAddressLine2 { get; set; }
    public string AlternateFullAddress { get; set; }
    public string AlternateDocumentNumber { get; set; }
    public string AlternateDocumentNumber2 { get; set; }
    public string AlternateDateOfBirth { get; set; }
    public string AlternateExpiryDate { get; set; }
    public string AlternateIssueDate { get; set; }
    public string AlternatePostalCode { get; set; }
    public string AlternateCity { get; set; }
    public string DocumentNumber2 { get; set; }
    public string AuthenticationServiceID { get; set; }
    public string HasAuthenticated { get; set; }
    public string AuthenticationServiceDetails { get; set; }
    public string BusinessReferenceID { get; set; }
    public string PartialMatchDetails { get; set; }
    public string BestCompaniesByDistanceCount { get; set; }
    public string IsExactBusinessNameMatch { get; set; }
    public string BestBusinessNameDistanceScore { get; set; }
    public string IsBusinessAddressReturned { get; set; }
    public string UniqueCompanyResolutionMethod { get; set; }
    public string AddressFound { get; set; }
    public string JurisdictionOfIncorporationCode { get; set; }
    public string Address1Standard { get; set; }
    public string CityStandard { get; set; }
    public string PostalCodeStandard { get; set; }
    public string StateProvinceStandard { get; set; }
    public string PhonePortedStatus { get; set; }
    public string MobileCountryCode { get; set; }
    public string MobileNetworkCode { get; set; }
    public string Address2Standard { get; set; }
    public string OCRStatus { get; set; }
    public string ValidBusinessRegNum { get; set; }
    public string ImageEncoding { get; set; }
    public string Liveness { get; set; }
    public string FrontImageCaptureMode { get; set; }
    public string FrontImageRetryCount { get; set; }
    public string FrontImageTimeStamp { get; set; }
    public string BackImageCaptureMode { get; set; }
    public string BackImageRetryCount { get; set; }
    public string BackImageTimeStamp { get; set; }
    public string LivePhotoCaptureMode { get; set; }
    public string LivePhotoRetryCount { get; set; }
    public string LivePhotoTimeStamp { get; set; }
    public string BusinessIDs { get; set; }
    public string RiskDescription { get; set; }
    public string MobileSimSwapDate { get; set; }
    public string ImageResolutionCheck { get; set; }
    public string PhotoEditingSoftwareCheck { get; set; }
    public string FraudCheck { get; set; }
    public string GeoLocationCity { get; set; }
    public string GeoLocationStateProvince { get; set; }
    public string GeoLocationCountry { get; set; }
    public string GeoLocationPostalCode { get; set; }
    public string MotherFullName { get; set; }
    public string SpouseFullName { get; set; }
    public string ScreenDetectionCheck { get; set; }
    public string BRNCountry { get; set; }
    public string BRNJurisdiction { get; set; }
    public string BRNName { get; set; }
    public string BRNType { get; set; }
    public string BRNMask { get; set; }
    public string BRNLength { get; set; }
    public string BRNDescription { get; set; }
    public string IPStateProvince { get; set; }
    public string GPSDistanceFromAddress { get; set; }
    public string OneTimePasscode { get; set; }
    public string NumberOfAttempts { get; set; }
    public string PartnerName { get; set; }
    public string AccountProviderName { get; set; }
    public string TimeoutReached { get; set; }
    public string TranslatedDatasourcesBRN { get; set; }
    public string YearOfDeath { get; set; }
    public string NationalIDStatus { get; set; }
    public string NationalIDType { get; set; }
    public string TamperResult { get; set; }
    public string TamperSensitivity { get; set; }
    public string DriverLicenceCardNumber { get; set; }
    public string BRNCountryCode { get; set; }
    public string BRNJurisdictionCode { get; set; }
    public string BRNConfigurationID { get; set; }
    public string BRNTranslatedBRNs { get; set; }
    public string IRSNameControlMatched { get; set; }
    public string InputBusinessNameTranslated { get; set; }
    public string InputBusinessNameTransliterated { get; set; }
    public string ReturnedBusinessNameTranslated { get; set; }
    public string ReturnedBusinessNameTransliterated { get; set; }
    public string UserInputBRNMask { get; set; }
    public string VendorReturnedBRNMask { get; set; }
    public string TaxIDNumberUserInputMask { get; set; }
    public string TaxIDNumberVendorReturnedMask { get; set; }
    public string TaxIDNumberCountry { get; set; }
    public string TaxIDNumberJurisdiction { get; set; }
    public string TaxIDNumberName { get; set; }
    public string TaxIDNumberType { get; set; }
    public string TaxIDNumberMask { get; set; }
    public string TaxIDNumberLength { get; set; }
    public string TaxIDNumberDescription { get; set; }
    public string TaxIDNumberCountryCode { get; set; }
    public string TaxIDNumberJurisdictionCode { get; set; }
    public string TaxIDNumberConfigurationID { get; set; }
    public string TaxIDNumberTranslatedTaxIDNumbers { get; set; }
    public string TaxIDNumberTranslatedDatasources { get; set; }
    public string Warning { get; set; }
    public string DetectedLanguage { get; set; }
    public string IsTranslationSupported { get; set; }
    public string IsTransliterationSupported { get; set; }
    public string TranslatedText { get; set; }
    public string TransliteratedText { get; set; }
    public string IsAddressCorrected { get; set; }
    public string StandardizedCompanyOwnershipHierarchy { get; set; }
    public string StandardizedRegistrationNumbers { get; set; }
    public string StandardizedLocations { get; set; }
    public string StandardizedIndustries { get; set; }
    public string StandardizedStockExchanges { get; set; }
    public string StandardizedDirectorsOfficers { get; set; }
    public string StandardizedCommunication { get; set; }
    public string TranslationFeatureCalled { get; set; }
    public string DetectionFeatureCalled { get; set; }
    public string TransliterationFeatureCalled { get; set; }
    public string TranslationCharacters { get; set; }
    public string DetectionCharacters { get; set; }
    public string TransliterationCharacters { get; set; }
    public string StandardizedShareCapitals { get; set; }
    public string SixMonthSingleSource { get; set; }
    public string SecondAddress1 { get; set; }
    public string SecondBuildingName { get; set; }
    public string SecondBuildingNumber { get; set; }
    public string SecondCity { get; set; }
    public string SecondCounty { get; set; }
    public string UtilityBill { get; set; }
    public string SecondFullAddress { get; set; }
    public string OriginalBusinessLegalForm { get; set; }
    public string StandardizedMetadata { get; set; }
    public string StatementDate { get; set; }
    public string LanguageIntelligence { get; set; }
    public string BusinessNameMetaphoneMatched { get; set; }
    public string TranslatedInputBusinessNameMetaphoneMatched { get; set; }
    public string TranslatedOutputBusinessNameMetaphoneMatched { get; set; }
    public string TranslatedInputOutputBusinessNameMetaphoneMatched { get; set; }
    public string TransliteratedInputBusinessNameMetaphoneMatched { get; set; }
    public string TransliteratedOutputBusinessNameMetaphoneMatched { get; set; }
    public string TransliteratedInputOutputBusinessNameMetaphoneMatched { get; set; }
    public string RespectEnhanced { get; set; }
    public string ComprehensiveViewMetadata { get; set; }
    public string SupersedeTieringOverride { get; set; }
    public string AccountNumber { get; set; }
    public string Subject { get; set; }
    public string HashSalt { get; set; }
    public string StandardizedFilings { get; set; }
    public string Filings { get; set; }
    public string GeoCoordinate { get; set; }
    public string DeviceType { get; set; }
    public string OperatingSystem { get; set; }
    public string Browser { get; set; }
    public string CallbackUrl { get; set; }
    public string ScheduleFrequency { get; set; }
    public string EnrollmentId { get; set; }
    public string RiskScoreValue { get; set; }
    public string PeopleOfSignificantControl { get; set; }
    public string UserAgent { get; set; }
    public string Action { get; set; }
    public string DeviceID { get; set; }
    public string IP { get; set; }
    public string FilingProductCalled { get; set; }
    public string FilingProductReturned { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Signal { get; set; }
    public string VendorFraudMonitorId { get; set; }
    public string Notification { get; set; }
    public string KYBOutOfSyncUpdated { get; set; }
    public string CallbackResponse { get; set; }
    public string ComprehensiveScoring { get; set; }
    public string Details { get; set; }
    public string RiskScore { get; set; }
    public string RiskModels { get; set; }
    public string RiskSignals { get; set; }
    public string IgnoreSuperDataset { get; set; }
    public string ArticleOfAssociation { get; set; }
    public string RegistrationDetails { get; set; }
    public string AnnualReport { get; set; }
    public string StandardizedBusinessNames { get; set; }
    public string NetworkScore { get; set; }
    public string NetworkSignals { get; set; }
    public string NetworkDetails { get; set; }
    public string MobileScore { get; set; }
    public string MobileSignals { get; set; }
    public string EkataRiskSignals { get; set; }
    public string AnomalyScore { get; set; }
    public string RiskDetails { get; set; }
    public string AnomalySignals { get; set; }
    public string OwnershipCompanyReturned { get; set; }
    public string RegisterReport { get; set; }
    public string AnnualAccounts { get; set; }
    public string OfficialFilings { get; set; }
    public string TradeRegisterReport { get; set; }
    public string RegisterCheck { get; set; }
    public string BeneficialOwnersCheck { get; set; }
    public string FiledDocuments { get; set; }
    public string FiledChanges { get; set; }
    public string ArticleOfAuthority { get; set; }
    public string CreditCheck { get; set; }
    public string CreditReport { get; set; }
    public string AgentAddressChange { get; set; }
    public string CapitalDocument { get; set; }
    public string LiquidationDocument { get; set; }
    public string MortgageDocument { get; set; }
    public string BiometricsUrl { get; set; }
    public string CompletePlus { get; set; }
    public string CompletePlusProductCalled { get; set; }
    public string CompletePlusProductReturned { get; set; }
    public string GISAExtract { get; set; }
    public string VRExtract { get; set; }
    public string InputCityNameTranslated { get; set; }
    public string InputCityNameTransliterated { get; set; }
    public string BiometricSimilarityScore { get; set; }
    public string HomeJurisdiction { get; set; }
    public string BiometricMatchIndicator { get; set; }
    public string IsTransactionLimitTrial { get; set; }
    public string AMLAssistAndMobileDualProcess { get; set; }
    public string StandardizedIncorporationDetails { get; set; }
    public string VendorData { get; set; }
    public string WfsClientID { get; set; }
    public string OidcAcr { get; set; }
    public string RiskSummaryScore { get; set; }
    public string RiskWeight { get; set; }
    public string RiskOutputLevel { get; set; }
    public string ResultReturned { get; set; }
    public string WinningRecord { get; set; }
    public string StandardizedSubmitterCheckData { get; set; }
    public string SocialSecurityNumberType { get; set; }
    public string SubSource { get; set; }
    public string IsDsUsedInSuccessfulRuleMatch { get; set; }
    public string PlaceOfRegistration { get; set; }
    public string EntityType { get; set; }
    public string AMLAssistCreditFileAndMobileDualProcess { get; set; }
    public string ForceEnhanced { get; set; }
    public string NumberOfTradelines { get; set; }
    public string TradelineSummary { get; set; }
    public string DetailedTradelines { get; set; }
    public string InternalTradelineLogs { get; set; }
    public string InternalLookupLogs { get; set; }
    public string InternalCreditFileandMobileDualProcess { get; set; }
    public string InternalAMLMobileDualProcess { get; set; }
}
