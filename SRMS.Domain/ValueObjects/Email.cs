namespace SRMS.Domain.ValueObjects;

/// <summary>
/// Email Value Object - بدلاً من string عادي
/// </summary>
public class Email : ValueObject
{
    public string Value { get; set; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory Method لإنشاء Email مع التحقق
    /// </summary>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");

        email = email.Trim().ToLower();

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");

        return new Email(email);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    // Implicit conversion لسهولة الاستخدام
    public static implicit operator string(Email email) => email.Value;
}