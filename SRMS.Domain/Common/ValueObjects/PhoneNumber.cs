namespace SRMS.Domain.Common.ValueObjects;

/// <summary>
/// PhoneNumber Value Object
/// </summary>
public class PhoneNumber : ValueObject
{
    public string Value { get; private set; }
    public string CountryCode { get; private set; }

    private PhoneNumber(string value, string countryCode)
    {
        Value = value;
        CountryCode = countryCode;
    }

    public static PhoneNumber Create(string phoneNumber, string countryCode = "+966")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty");

        // إزالة المسافات والرموز
        phoneNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (phoneNumber.Length < 9 || phoneNumber.Length > 15)
            throw new ArgumentException("Invalid phone number length");

        return new PhoneNumber(phoneNumber, countryCode);
    }

    public string GetFormatted() => $"{CountryCode} {Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }

    public override string ToString() => GetFormatted();
}