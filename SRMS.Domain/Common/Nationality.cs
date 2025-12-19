using SRMS.Domain.Abstractions;

namespace SRMS.Domain.Common;

public class Nationality : Entity
{
    public string Name { get; set; } = string.Empty; // e.g. "ليبي"
    public string NameEn { get; set; } = string.Empty; // e.g. "Libyan"
    public string CountryCode { get; set; } = string.Empty; // e.g. "LY"
}
