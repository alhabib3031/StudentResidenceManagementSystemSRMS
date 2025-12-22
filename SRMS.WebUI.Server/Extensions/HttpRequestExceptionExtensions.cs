using System.Net.Http;
using System.Text.Json;

namespace SRMS.WebUI.Server.Extensions;

public static class HttpRequestExceptionExtensions
{
    public static async Task<T?> GetContentAs<T>(this HttpRequestException exception)
    {
        if (exception.Data.Contains("Content"))
        {
            var content = exception.Data["Content"] as string;
            if (!string.IsNullOrWhiteSpace(content))
            {
                return JsonSerializer.Deserialize<T>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
        }

        return default;
    }
}
