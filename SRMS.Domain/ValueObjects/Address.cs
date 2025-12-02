namespace SRMS.Domain.ValueObjects;

/// <summary>
/// Address Value Object - العنوان المركب
/// </summary>
public class Address : ValueObject
{
    public string City { get; private set; }
    public string Street { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    private Address(string city, string street, string state, string postalCode, string country)
    {
        City = city;
        Street = street;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string city, string street, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        return new Address(
            city.Trim(),
            street?.Trim() ?? "",
            state?.Trim() ?? "",
            postalCode?.Trim() ?? "",
            country?.Trim() ?? "Libya"
        );
    }

    public string GetFullAddress()
    {
        var parts = new[] { Street, City, State, PostalCode, Country }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => GetFullAddress();
}